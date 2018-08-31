using System;
using UltimateWater.Internal;
using UnityEngine;

namespace UltimateWater
{
	public class FastWaterPhysics : MonoBehaviour
	{
		private void Awake()
		{
			FastWaterPhysics._RayUp = new Ray(Vector3.zero, Vector3.up);
			FastWaterPhysics._RayDown = new Ray(Vector3.zero, Vector3.down);
		}

		private void Start()
		{
			this._RigidBody = base.GetComponentInParent<Rigidbody>();
			this._LocalCollider = base.GetComponentInParent<Collider>();
			if (this._Water == null)
			{
				this._Water = ApplicationSingleton<WaterSystem>.Instance.BoundlessWaters[0];
			}
			this.OnValidate();
			Vector3 position = base.transform.position;
			this._LastPositionX = position.x;
			this._LastPositionZ = position.z;
			this._Sample = new WaterSample(this._Water, WaterSample.DisplacementMode.HeightAndForces, 1f);
			this._Sample.Start(base.transform.position);
		}

		private void OnValidate()
		{
			if (this._LocalCollider != null)
			{
				this._Volume = this._LocalCollider.ComputeVolume();
				this._Area = this._LocalCollider.ComputeArea();
			}
			if (this._FlowIntensity < 0f)
			{
				this._FlowIntensity = 0f;
			}
			if (this._BuoyancyIntensity < 0f)
			{
				this._BuoyancyIntensity = 0f;
			}
			if (this._Water != null)
			{
				this.PrecomputeBuoyancy();
				this.PrecomputeDrag();
				this.PrecomputeFlow();
			}
		}

		private void FixedUpdate()
		{
			if (this._RigidBody.isKinematic)
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
			float time = this._Water.Time;
			Vector3 position2 = base.transform.position;
			Vector3 vector;
			Vector3 vector2;
			this._Sample.GetAndResetFast(position2.x, position2.z, time, out vector, out vector2);
			vector.x += position.x - this._LastPositionX;
			vector.z += position.z - this._LastPositionZ;
			float y3 = vector.y;
			vector.y = y - 20f;
			FastWaterPhysics._RayUp.origin = vector;
			RaycastHit raycastHit;
			if (this._LocalCollider.Raycast(FastWaterPhysics._RayUp, out raycastHit, maxDistance))
			{
				float y4 = raycastHit.point.y;
				Vector3 normal = raycastHit.normal;
				vector.y = y2 + 20f;
				FastWaterPhysics._RayDown.origin = vector;
				this._LocalCollider.Raycast(FastWaterPhysics._RayDown, out raycastHit, maxDistance);
				float y5 = raycastHit.point.y;
				float num2 = (y3 - y4) / (y5 - y4);
				if (num2 <= 0f)
				{
					return;
				}
				if (num2 > 1f)
				{
					num2 = 1f;
				}
				Vector3 vector3 = this._BuoyancyPart * num2;
				float num3 = num2 * 0.5f;
				vector.y = y4 * (1f - num3) + y5 * num3;
				if (this._UseCheapDrag)
				{
					Vector3 pointVelocity = this._RigidBody.GetPointVelocity(vector);
					Vector3 vector4 = pointVelocity + vector3 * num;
					Vector3 a;
					a.x = ((vector4.x <= 0f) ? (vector4.x * vector4.x) : (-vector4.x * vector4.x));
					a.y = ((vector4.y <= 0f) ? (vector4.y * vector4.y) : (-vector4.y * vector4.y));
					a.z = ((vector4.z <= 0f) ? (vector4.z * vector4.z) : (-vector4.z * vector4.z));
					Vector3 a2 = a * this._DragPart;
					float num4 = a2.magnitude * num;
					float num5 = num4 * num4;
					float num6 = Vector3.Dot(pointVelocity, pointVelocity);
					if (num5 > num6)
					{
						num2 *= Mathf.Sqrt(num6) / num4;
					}
					vector3 += a2 * num2;
				}
				this._RigidBody.AddForceAtPosition(vector3, vector, ForceMode.Force);
				if (this._UseCheapFlow)
				{
					float num7 = Vector3.Dot(vector2, vector2);
					if (num7 != 0f)
					{
						num3 = -1f / num7;
						float num8 = Vector3.Dot(normal, vector2) * num3 + 0.5f;
						if (num8 > 0f)
						{
							vector3 = vector2 * (num8 * this._FlowPart);
							vector.y = y4;
							this._RigidBody.AddForceAtPosition(vector3, vector, ForceMode.Force);
						}
					}
				}
			}
			this._LastPositionX = position.x;
			this._LastPositionZ = position.z;
		}

		private void PrecomputeBuoyancy()
		{
			this._BuoyancyPart = -Physics.gravity * (this._Volume * this._BuoyancyIntensity * this._Water.Density);
		}

		private void PrecomputeDrag()
		{
			this._UseCheapDrag = (this._DragCoefficient > 0f);
			this._DragPart = 0.25f * this._DragCoefficient * this._Area * this._Water.Density;
		}

		private void PrecomputeFlow()
		{
			this._UseCheapFlow = (this._FlowIntensity > 0f);
			this._FlowPart = this._FlowIntensity * this._DragCoefficient * this._Area * 100f;
		}

		[SerializeField]
		private Water _Water;

		[SerializeField]
		[Tooltip("Adjust buoyancy proportionally, if your collider is bigger or smaller than the actual object. Lowering this may fix some weird behavior of objects with extremely low density like beach balls or baloons.")]
		private float _BuoyancyIntensity = 1f;

		[SerializeField]
		[Range(0f, 3f)]
		[Tooltip("Controls drag force. Determined experimentally in wind tunnels. Example values:\n https://en.wikipedia.org/wiki/Drag_coefficient#General")]
		private float _DragCoefficient = 0.9f;

		[SerializeField]
		[Tooltip("Horizontal flow force intensity.")]
		private float _FlowIntensity = 1f;

		private Rigidbody _RigidBody;

		private Collider _LocalCollider;

		private WaterSample _Sample;

		private float _LastPositionX;

		private float _LastPositionZ;

		private Vector3 _BuoyancyPart;

		private float _DragPart;

		private float _FlowPart;

		private float _Volume;

		private float _Area;

		private bool _UseCheapDrag;

		private bool _UseCheapFlow;

		private static Ray _RayUp;

		private static Ray _RayDown;
	}
}
