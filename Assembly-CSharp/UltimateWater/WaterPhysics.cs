using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UltimateWater
{
	[AddComponentMenu("Ultimate Water/Water Physics")]
	public sealed class WaterPhysics : MonoBehaviour
	{
		public Water AffectingWater
		{
			get
			{
				return (this._WaterProbe == null) ? this._WaterOverride : this._WaterProbe.CurrentWater;
			}
			set
			{
				bool flag = this._WaterOverride == null;
				this._WaterOverride = value;
				if (this._WaterOverride == null)
				{
					if (!flag)
					{
						this.OnWaterLeave();
					}
					this.CreateWaterProbe();
				}
				else
				{
					this.DestroyWaterProbe();
					this.OnWaterLeave();
					this.OnWaterEnter();
				}
			}
		}

		public float BuoyancyIntensity
		{
			get
			{
				return this._BuoyancyIntensity;
			}
			set
			{
				this._BuoyancyIntensity = value;
				if (this.AffectingWater != null)
				{
					this.PrecomputeBuoyancy();
				}
			}
		}

		public float DragCoefficient
		{
			get
			{
				return this._DragCoefficient;
			}
			set
			{
				this._DragCoefficient = value;
				if (this.AffectingWater != null)
				{
					this.PrecomputeDrag();
				}
			}
		}

		public float FlowIntensity
		{
			get
			{
				return this._FlowIntensity;
			}
			set
			{
				this._FlowIntensity = value;
				if (this.AffectingWater != null)
				{
					this.PrecomputeFlow();
				}
			}
		}

		public float AverageWaterElevation
		{
			get
			{
				return this._AverageWaterElevation;
			}
		}

		public float Volume
		{
			get
			{
				return this._Volume;
			}
		}

		public float GetEquilibriumMass(float fluidDensity = 999.8f)
		{
			return this._Volume * this._BuoyancyIntensity * fluidDensity;
		}

		public float GetTotalBuoyancy(float fluidDensity = 999.8f)
		{
			return Physics.gravity.magnitude * this._Volume * this._BuoyancyIntensity * fluidDensity / this._RigidBody.mass;
		}

		private void Awake()
		{
			this._LocalCollider = base.GetComponent<Collider>();
			this._RigidBody = base.GetComponent<Rigidbody>();
			WaterPhysics._RayUp = new Ray(Vector3.zero, Vector3.up);
			WaterPhysics._RayDown = new Ray(Vector3.zero, Vector3.down);
			if (this._LocalCollider.IsNullReference(this) || this._RigidBody.IsNullReference(this))
			{
				return;
			}
			Vector3 position = base.transform.position;
			this._LastPositionX = position.x;
			this._LastPositionZ = position.z;
			this.OnValidate();
			this.PrecomputeSamples();
			if (this._UseImprovedDragAndFlowForces)
			{
				this.PrecomputeImprovedDrag();
			}
		}

		private void OnEnable()
		{
			if (this._WaterOverride == null)
			{
				this.CreateWaterProbe();
			}
		}

		private void OnDisable()
		{
			this.DestroyWaterProbe();
			this.OnWaterLeave();
		}

		private void OnValidate()
		{
			this._NumSamplesInv = 1f / (float)this._SampleCount;
			if (this._LocalCollider != null)
			{
				this._Volume = this._LocalCollider.ComputeVolume();
				this._Area = this._LocalCollider.ComputeArea();
				if (this._TotalArea == 0f)
				{
					this.UpdateTotalArea();
				}
				if (this._UseImprovedDragAndFlowForces && !(this._LocalCollider is MeshCollider))
				{
					this._UseImprovedDragAndFlowForces = false;
					Debug.LogErrorFormat("Improved drag force won't work colliders other than mesh colliders. '{0}' collider has a wrong type.", new object[]
					{
						base.name
					});
				}
				if (this._UseImprovedDragAndFlowForces && ((MeshCollider)this._LocalCollider).sharedMesh.vertexCount > 3000)
				{
					this._UseImprovedDragAndFlowForces = false;
					Mesh sharedMesh = ((MeshCollider)this._LocalCollider).sharedMesh;
					Debug.LogErrorFormat("Improved drag force won't work with meshes that have more than 3000 vertices. '{0}' has {1} vertices.", new object[]
					{
						sharedMesh.name,
						sharedMesh.vertexCount
					});
				}
			}
			this._FlowIntensity = Mathf.Max(this._FlowIntensity, 0f);
			this._BuoyancyIntensity = Mathf.Max(this._BuoyancyIntensity, 0f);
			if (this.AffectingWater != null)
			{
				this.PrecomputeBuoyancy();
				this.PrecomputeDrag();
				this.PrecomputeFlow();
			}
		}

		private void FixedUpdate()
		{
			if (this._UseImprovedDragAndFlowForces)
			{
				this.ImprovedFixedUpdate();
			}
			else
			{
				this.SimpleFixedUpdate();
			}
		}

		private void Reset()
		{
			Rigidbody rigidbody = base.GetComponent<Rigidbody>();
			Collider component = base.GetComponent<Collider>();
			if (rigidbody == null && component != null)
			{
				rigidbody = base.gameObject.AddComponent<Rigidbody>();
				rigidbody.mass = this.GetEquilibriumMass(999.8f);
			}
		}

		private void SimpleFixedUpdate()
		{
			Water affectingWater = this.AffectingWater;
			if (affectingWater == null || this._RigidBody.isKinematic)
			{
				return;
			}
			Bounds bounds = this._LocalCollider.bounds;
			float y = bounds.min.y;
			float y2 = bounds.max.y;
			Vector3 position = base.transform.position;
			float maxDistance = y2 - y + 80f;
			float fixedDeltaTime = Time.fixedDeltaTime;
			float num = fixedDeltaTime * (1f - this._RigidBody.drag * fixedDeltaTime) / this._RigidBody.mass;
			float time = affectingWater.Time;
			this._AverageWaterElevation = 0f;
			int i = 0;
			while (i < this._SampleCount)
			{
				Vector3 vector = base.transform.TransformPoint(this._CachedSamplePositions[this._CachedSampleIndex]);
				Vector3 vector2;
				Vector3 vector3;
				this._Samples[i].GetAndResetFast(vector.x, vector.z, time, out vector2, out vector3);
				vector2.x += position.x - this._LastPositionX;
				vector2.z += position.z - this._LastPositionZ;
				float y3 = vector2.y;
				vector2.y = y - 20f;
				WaterPhysics._RayUp.origin = vector2;
				this._AverageWaterElevation += y3;
				RaycastHit raycastHit;
				if (!this._LocalCollider.Raycast(WaterPhysics._RayUp, out raycastHit, maxDistance))
				{
					goto IL_3F1;
				}
				float y4 = raycastHit.point.y;
				Vector3 normal = raycastHit.normal;
				vector2.y = y2 + 20f;
				WaterPhysics._RayDown.origin = vector2;
				this._LocalCollider.Raycast(WaterPhysics._RayDown, out raycastHit, maxDistance);
				float y5 = raycastHit.point.y;
				float num2 = (y3 - y4) / (y5 - y4);
				if (num2 > 0f)
				{
					if (num2 > 1f)
					{
						num2 = 1f;
					}
					Vector3 vector4 = this._BuoyancyPart * num2;
					float num3 = num2 * 0.5f;
					vector2.y = y4 * (1f - num3) + y5 * num3;
					if (this._UseCheapDrag)
					{
						Vector3 pointVelocity = this._RigidBody.GetPointVelocity(vector2);
						Vector3 vector5 = pointVelocity + vector4 * num;
						Vector3 a;
						a.x = ((vector5.x <= 0f) ? (vector5.x * vector5.x) : (-vector5.x * vector5.x));
						a.y = ((vector5.y <= 0f) ? (vector5.y * vector5.y) : (-vector5.y * vector5.y));
						a.z = ((vector5.z <= 0f) ? (vector5.z * vector5.z) : (-vector5.z * vector5.z));
						Vector3 a2 = a * this._DragPart;
						float num4 = a2.magnitude * num;
						float num5 = num4 * num4;
						float num6 = Vector3.Dot(pointVelocity, pointVelocity);
						if (num5 > num6)
						{
							num2 *= Mathf.Sqrt(num6) / num4;
						}
						vector4 += a2 * num2;
					}
					this._RigidBody.AddForceAtPosition(vector4, vector2, ForceMode.Force);
					if (!this._UseCheapFlow)
					{
						goto IL_3F1;
					}
					float num7 = Vector3.Dot(vector3, vector3);
					if (num7 == 0f)
					{
						goto IL_3F1;
					}
					num3 = -1f / num7;
					float num8 = Vector3.Dot(normal, vector3) * num3 + 0.5f;
					if (num8 > 0f)
					{
						vector4 = vector3 * (num8 * this._FlowPart);
						vector2.y = y4;
						this._RigidBody.AddForceAtPosition(vector4, vector2, ForceMode.Force);
						goto IL_3F1;
					}
					goto IL_3F1;
				}
				IL_416:
				i++;
				continue;
				IL_3F1:
				if (++this._CachedSampleIndex >= this._CachedSampleCount)
				{
					this._CachedSampleIndex = 0;
					goto IL_416;
				}
				goto IL_416;
			}
			this._AverageWaterElevation *= this._NumSamplesInv;
			this._LastPositionX = position.x;
			this._LastPositionZ = position.z;
		}

		private void ImprovedFixedUpdate()
		{
			Water affectingWater = this.AffectingWater;
			if (affectingWater == null || this._RigidBody.isKinematic)
			{
				return;
			}
			float time = affectingWater.Time;
			float improvedDragPart = this._ImprovedDragPart;
			Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
			Vector4 row = localToWorldMatrix.GetRow(1);
			Vector3 center = this._LocalCollider.bounds.center;
			this._AverageWaterElevation = 0f;
			int num = 0;
			for (int i = 0; i < this._DragNormals.Length; i++)
			{
				Vector3 vector = localToWorldMatrix.MultiplyPoint3x4(this._DragCenters[i]);
				Vector3 pointVelocity = this._RigidBody.GetPointVelocity(vector);
				Vector3 vector2 = localToWorldMatrix.MultiplyVector(this._DragNormals[i]);
				float num2;
				Vector3 lhs;
				this._ImprovedDragSamples[i].GetAndResetFast(vector.x, vector.z, time, out num2, out lhs);
				this._AverageWaterElevation += num2;
				float num3 = Vector3.Dot(vector2, pointVelocity);
				float num4 = Vector3.Dot(lhs, vector2) * this._ImprovedFlowPart;
				float num11;
				if (num3 > 0f || num4 > 0f)
				{
					float num5 = WaterPhysics.SingleComponentTransform(ref this._DragVertices[num++], ref row);
					float num6 = WaterPhysics.SingleComponentTransform(ref this._DragVertices[num++], ref row);
					float num7 = WaterPhysics.SingleComponentTransform(ref this._DragVertices[num++], ref row);
					float num8 = num2 - num5;
					float num9 = num2 - num6;
					float num10 = num2 - num7;
					if (num8 > 0f)
					{
						if (num9 > 0f)
						{
							num11 = ((num10 < 0f) ? ((num8 + num9) / (num8 + num9 - num10)) : 1f);
						}
						else
						{
							num11 = ((num10 < 0f) ? (num8 / (num8 - num9 - num10)) : ((num8 + num10) / (num8 - num9 + num10)));
						}
					}
					else if (num9 > 0f)
					{
						num11 = ((num10 < 0f) ? (num9 / (num9 - num10 - num8)) : ((num9 + num10) / (num9 + num10 - num8)));
					}
					else
					{
						num11 = ((num10 < 0f) ? 0f : (num10 / (num10 - num8 - num9)));
					}
					if (num11 <= 0f || num11 > 1.02f)
					{
						num11 = 0f;
					}
				}
				else
				{
					num11 = 0f;
					num += 3;
				}
				float num12 = this._DragAreas[i] * num11;
				float num13 = (num3 <= 0f) ? 0f : (improvedDragPart * num3 * num3 * num12);
				float magnitude = pointVelocity.magnitude;
				num13 = ((magnitude == 0f) ? 0f : (num13 / magnitude));
				Vector3 force = pointVelocity * num13;
				if (center.y > vector.y)
				{
					if (num2 > center.y)
					{
						num11 = this._PolygonVolumes[i];
						force.x += this._ImprovedBuoyancyPart.x * num11;
						force.y += this._ImprovedBuoyancyPart.y * num11;
						force.z += this._ImprovedBuoyancyPart.z * num11;
					}
					else if (num2 > vector.y)
					{
						num11 = this._PolygonVolumes[i] * (num2 - vector.y) / (center.y - vector.y);
						force.x += this._ImprovedBuoyancyPart.x * num11;
						force.y += this._ImprovedBuoyancyPart.y * num11;
						force.z += this._ImprovedBuoyancyPart.z * num11;
					}
				}
				else if (num2 > vector.y)
				{
					num11 = this._PolygonVolumes[i];
					force.x += this._ImprovedBuoyancyPart.x * num11;
					force.y += this._ImprovedBuoyancyPart.y * num11;
					force.z += this._ImprovedBuoyancyPart.z * num11;
				}
				else if (num2 > center.y)
				{
					num11 = this._PolygonVolumes[i] * (num2 - center.y) / (vector.y - center.y);
					force.x += this._ImprovedBuoyancyPart.x * num11;
					force.y += this._ImprovedBuoyancyPart.y * num11;
					force.z += this._ImprovedBuoyancyPart.z * num11;
				}
				if (num4 > 0f)
				{
					magnitude = lhs.magnitude;
					float num14 = (magnitude == 0f) ? 0f : (num4 * num12 / magnitude);
					force.x += lhs.x * num14;
					force.y += lhs.y * num14;
					force.z += lhs.z * num14;
				}
				this._RigidBody.AddForceAtPosition(force, vector, ForceMode.Force);
			}
			this._AverageWaterElevation /= (float)this._DragNormals.Length;
		}

		private static float SingleComponentTransform(ref Vector3 point, ref Vector4 row)
		{
			return point.x * row.x + point.y * row.y + point.z * row.z + row.w;
		}

		private void CreateWaterProbe()
		{
			if (this._WaterProbe == null)
			{
				this._WaterProbe = WaterVolumeProbe.CreateProbe(this._RigidBody.transform, this._LocalCollider.bounds.extents.magnitude);
				this._WaterProbe.Enter.AddListener(new UnityAction(this.OnWaterEnter));
				this._WaterProbe.Leave.AddListener(new UnityAction(this.OnWaterLeave));
			}
		}

		private void DestroyWaterProbe()
		{
			if (this._WaterProbe != null)
			{
				this._WaterProbe.gameObject.Destroy();
				this._WaterProbe = null;
			}
		}

		private void OnWaterEnter()
		{
			this.CreateWaterSamplers();
			this.AffectingWater.ProfilesManager.ValidateProfiles();
			this.PrecomputeBuoyancy();
			this.PrecomputeDrag();
			this.PrecomputeFlow();
		}

		private void OnWaterLeave()
		{
			if (this._Samples != null)
			{
				for (int i = 0; i < this._SampleCount; i++)
				{
					this._Samples[i].Stop();
				}
				this._Samples = null;
			}
		}

		private bool ValidateForEditor()
		{
			if (this._LocalCollider == null)
			{
				this._LocalCollider = base.GetComponent<Collider>();
				this._RigidBody = base.GetComponentInParent<Rigidbody>();
				this.OnValidate();
			}
			return this._LocalCollider != null && this._RigidBody != null;
		}

		private void PrecomputeSamples()
		{
			List<Vector3> list = new List<Vector3>();
			float num = 0.5f;
			float num2 = 1f;
			int num3 = this._SampleCount * 18;
			Transform transform = base.transform;
			Vector3 vector;
			Vector3 vector2;
			ColliderExtensions.GetLocalMinMax(this._LocalCollider, out vector, out vector2);
			int num4 = 0;
			while (num4 < 4 && list.Count < num3)
			{
				for (float num5 = num; num5 <= 1f; num5 += num2)
				{
					for (float num6 = num; num6 <= 1f; num6 += num2)
					{
						for (float num7 = num; num7 <= 1f; num7 += num2)
						{
							Vector3 vector3 = new Vector3(Mathf.Lerp(vector.x, vector2.x, num5), Mathf.Lerp(vector.y, vector2.y, num6), Mathf.Lerp(vector.z, vector2.z, num7));
							if (this._LocalCollider.IsPointInside(transform.TransformPoint(vector3)))
							{
								list.Add(vector3);
							}
						}
					}
				}
				num2 = num;
				num *= 0.5f;
				num4++;
			}
			this._CachedSamplePositions = list.ToArray();
			this._CachedSampleCount = this._CachedSamplePositions.Length;
			WaterPhysics.Shuffle<Vector3>(this._CachedSamplePositions);
		}

		private void PrecomputeImprovedDrag()
		{
			MeshCollider meshCollider = (MeshCollider)this._LocalCollider;
			Mesh sharedMesh = meshCollider.sharedMesh;
			Vector3[] vertices = sharedMesh.vertices;
			Vector3[] normals = sharedMesh.normals;
			int[] indices = sharedMesh.GetIndices(0);
			int num = indices.Length / 3;
			this._DragNormals = new Vector3[num];
			this._DragVertices = new Vector3[num * 3];
			this._DragCenters = new Vector3[num];
			this._DragAreas = new float[num];
			this._PolygonVolumes = new float[num];
			Vector3 b = this._LocalCollider.transform.InverseTransformPoint(this._LocalCollider.bounds.center);
			int num2 = 0;
			int i = 0;
			while (i < indices.Length)
			{
				Vector3 vector = vertices[indices[i]];
				Vector3 vector2 = vertices[indices[i + 1]];
				Vector3 vector3 = vertices[indices[i + 2]];
				this._DragVertices[i] = vector;
				this._DragVertices[i + 1] = vector2;
				this._DragVertices[i + 2] = vector3;
				this._DragAreas[num2] = Vector3.Cross(vector2 - vector, vector3 - vector).magnitude * 0.5f;
				this._DragCenters[num2] = (vector + vector2 + vector3) * 0.333333343f;
				Vector3 a = normals[indices[i++]];
				Vector3 b2 = normals[indices[i++]];
				Vector3 b3 = normals[indices[i++]];
				this._DragNormals[num2] = (a + b2 + b3) * 0.333333343f;
				Vector3 p = vector - b;
				Vector3 p2 = vector2 - b;
				Vector3 p3 = vector3 - b;
				this._PolygonVolumes[num2++] = Mathf.Abs(ColliderExtensions.SignedVolumeOfTriangle(p, p2, p3));
			}
			this._ImprovedDragSamples = new WaterSample[num];
		}

		private void UpdateTotalArea()
		{
			Rigidbody componentInParent = base.GetComponentInParent<Rigidbody>();
			WaterPhysics[] componentsInChildren = componentInParent.GetComponentsInChildren<WaterPhysics>();
			this._TotalArea = 0f;
			foreach (WaterPhysics waterPhysics in componentsInChildren)
			{
				if (!(waterPhysics.GetComponentInParent<Rigidbody>() != componentInParent))
				{
					if (waterPhysics._Area == -1f && waterPhysics._LocalCollider != null)
					{
						waterPhysics._Area = waterPhysics._LocalCollider.ComputeArea();
					}
					this._TotalArea += waterPhysics._Area;
				}
			}
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j]._TotalArea = this._TotalArea;
			}
		}

		private void CreateWaterSamplers()
		{
			Water affectingWater = this.AffectingWater;
			if (this._UseImprovedDragAndFlowForces)
			{
				for (int i = 0; i < this._ImprovedDragSamples.Length; i++)
				{
					this._ImprovedDragSamples[i] = new WaterSample(affectingWater, WaterSample.DisplacementMode.HeightAndForces, this._Precision);
					this._ImprovedDragSamples[i].Start(base.transform.TransformPoint(this._DragCenters[i]));
				}
			}
			else
			{
				if (this._Samples == null || this._Samples.Length != this._SampleCount)
				{
					this._Samples = new WaterSample[this._SampleCount];
				}
				for (int j = 0; j < this._SampleCount; j++)
				{
					this._Samples[j] = new WaterSample(affectingWater, WaterSample.DisplacementMode.HeightAndForces, this._Precision);
					this._Samples[j].Start(base.transform.TransformPoint(this._CachedSamplePositions[this._CachedSampleIndex]));
					if (++this._CachedSampleIndex >= this._CachedSampleCount)
					{
						this._CachedSampleIndex = 0;
					}
				}
			}
		}

		private void PrecomputeBuoyancy()
		{
			this._BuoyancyPart = -Physics.gravity * (this._NumSamplesInv * this._Volume * this._BuoyancyIntensity * this.AffectingWater.Density);
			this._ImprovedBuoyancyPart = -Physics.gravity * (this._BuoyancyIntensity * this.AffectingWater.Density);
		}

		private void PrecomputeDrag()
		{
			this._UseCheapDrag = (this._DragCoefficient > 0f && !this._UseImprovedDragAndFlowForces);
			this._DragPart = 0.5f * this._DragCoefficient * this._Area * this._NumSamplesInv * this.AffectingWater.Density;
			this._ImprovedDragPart = -0.5f * this._DragCoefficient * this.AffectingWater.Density;
		}

		private void PrecomputeFlow()
		{
			this._UseCheapFlow = (this._FlowIntensity > 0f && !this._UseImprovedDragAndFlowForces);
			this._FlowPart = this._FlowIntensity * this._DragCoefficient * this._Area * this._NumSamplesInv * 100f;
			this._ImprovedFlowPart = this._FlowIntensity * this._DragCoefficient * -100f;
		}

		private static void Shuffle<T>(IList<T> array)
		{
			int i = array.Count;
			while (i > 1)
			{
				int index = UnityEngine.Random.Range(0, i--);
				T value = array[i];
				array[i] = array[index];
				array[index] = value;
			}
		}

		[Tooltip("Controls precision of the simulation. Keep it low (1 - 2) for small and not important objects. Prefer high values (15 - 30) for ships etc.")]
		[FormerlySerializedAs("sampleCount")]
		[Range(1f, 30f)]
		[SerializeField]
		private int _SampleCount = 20;

		[Tooltip("Controls drag force. Determined experimentally in wind tunnels. Example values:\n https://en.wikipedia.org/wiki/Drag_coefficient#General")]
		[FormerlySerializedAs("dragCoefficient")]
		[Range(0f, 6f)]
		[SerializeField]
		private float _DragCoefficient = 0.9f;

		[Range(0.125f, 1f)]
		[Tooltip("Determines how many waves will be used in computations. Set it low for big objects, larger than most of the waves. Set it high for smaller objects of size comparable to many waves.")]
		[FormerlySerializedAs("precision")]
		[SerializeField]
		private float _Precision = 0.5f;

		[FormerlySerializedAs("buoyancyIntensity")]
		[Range(0.1f, 10f)]
		[Tooltip("Adjust buoyancy proportionally, if your collider is bigger or smaller than the actual object. Lowering this may fix some weird behaviour of objects with extremely low density like beach balls or baloons.")]
		[SerializeField]
		private float _BuoyancyIntensity = 1f;

		[SerializeField]
		[Tooltip("Horizontal flow force intensity.")]
		[FormerlySerializedAs("flowIntensity")]
		private float _FlowIntensity = 1f;

		[FormerlySerializedAs("useImprovedDragAndFlowForces")]
		[SerializeField]
		[Tooltip("Temporarily supports only mesh colliders.")]
		private bool _UseImprovedDragAndFlowForces;

		private Vector3[] _CachedSamplePositions;

		private int _CachedSampleIndex;

		private int _CachedSampleCount;

		private Collider _LocalCollider;

		private Rigidbody _RigidBody;

		private float _Volume;

		private float _Area = -1f;

		private float _TotalArea;

		private WaterSample[] _Samples;

		private float _NumSamplesInv;

		private Vector3 _BuoyancyPart;

		private Vector3 _ImprovedBuoyancyPart;

		private float _DragPart;

		private float _ImprovedDragPart;

		private float _FlowPart;

		private float _ImprovedFlowPart;

		private float _AverageWaterElevation;

		private bool _UseCheapDrag;

		private bool _UseCheapFlow;

		private Water _WaterOverride;

		private WaterVolumeProbe _WaterProbe;

		private float _LastPositionX;

		private float _LastPositionZ;

		private Vector3[] _DragNormals;

		private Vector3[] _DragCenters;

		private Vector3[] _DragVertices;

		private float[] _PolygonVolumes;

		private float[] _DragAreas;

		private WaterSample[] _ImprovedDragSamples;

		private static Ray _RayUp;

		private static Ray _RayDown;
	}
}
