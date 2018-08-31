using System;
using System.Collections.Generic;
using System.Diagnostics;
using UltimateWater.Internal;
using UnityEngine;

namespace UltimateWater
{
	public sealed class WaveParticlesQuadtree : Quadtree<WaveParticle>
	{
		public WaveParticlesQuadtree(Rect rect, int maxElementsPerNode, int maxTotalElements) : base(rect, maxElementsPerNode, maxTotalElements)
		{
			this._Qroot = this;
			this._ParticleGroups = new WaveParticlesGroup[maxElementsPerNode >> 3];
			this.CreateMesh();
		}

		private WaveParticlesQuadtree(WaveParticlesQuadtree root, Rect rect, int maxElementsPerNode) : this(rect, maxElementsPerNode, 0)
		{
			this._Qroot = root;
		}

		public bool DebugMode
		{
			get
			{
				return this._DebugMode;
			}
			set
			{
				this._DebugMode = value;
			}
		}

		public void UpdateSimulation(float time)
		{
			if (this._Qa != null)
			{
				this._Qa.UpdateSimulation(time);
				this._Qb.UpdateSimulation(time);
				this._Qc.UpdateSimulation(time);
				this._Qd.UpdateSimulation(time);
			}
			else if (this._NumElements != 0)
			{
				this.UpdateParticles(time);
			}
		}

		public void UpdateSimulation(float time, float maxExecutionTimeExp)
		{
			if (this._Stopwatch == null)
			{
				this._Stopwatch = new Stopwatch();
			}
			this._Stopwatch.Reset();
			this._Stopwatch.Start();
			this.UpdateSimulation(time);
			float num = (float)this._Stopwatch.ElapsedTicks / 10000f;
			if (num > 50f)
			{
				num = 50f;
			}
			this._Stress = this._Stress * 0.98f + (Mathf.Exp(num) - maxExecutionTimeExp) * 0.04f;
			if (this._Stress < 1f)
			{
				this._Stress = 1f;
			}
			if (this._Stress >= 20f)
			{
				this._Stress = 20f;
			}
		}

		public void Render(Rect renderRect)
		{
			if (!base.Rect.Overlaps(renderRect))
			{
				return;
			}
			if (this._Qa != null)
			{
				this._Qa.Render(renderRect);
				this._Qb.Render(renderRect);
				this._Qc.Render(renderRect);
				this._Qd.Render(renderRect);
			}
			else if (this._NumElements != 0)
			{
				this._Mesh.vertices = this._Vertices;
				if (this._TangentsPackChanged)
				{
					this._Mesh.tangents = this._TangentsPack;
					this._TangentsPackChanged = false;
				}
				if (this._Qroot._DebugMode)
				{
					this._Mesh.normals = this._DebugData;
				}
				Graphics.DrawMeshNow(this._Mesh, Matrix4x4.identity, 0);
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			if (this._Mesh != null)
			{
				this._Mesh.Destroy();
				this._Mesh = null;
			}
			this._Vertices = null;
			this._TangentsPack = null;
		}

		private void UpdateParticles(float time)
		{
			List<WaterCamera> enabledWaterCameras = WaterCamera.EnabledWaterCameras;
			int count = enabledWaterCameras.Count;
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				if (base.Rect.Overlaps(enabledWaterCameras[i].LocalMapsRect))
				{
					flag = true;
					break;
				}
			}
			int num;
			int num2;
			int num3;
			if (!flag)
			{
				num = this._LastUpdateIndex;
				num2 = this._LastUpdateIndex + 8;
				num3 = num << 2;
				if (num2 >= this._Elements.Length)
				{
					num2 = this._Elements.Length;
					this._LastUpdateIndex = 0;
				}
				else
				{
					this._LastUpdateIndex = num2;
				}
			}
			else
			{
				num = 0;
				num2 = this._Elements.Length;
				num3 = 0;
			}
			WaveParticlesQuadtree quadtree = (!flag) ? null : this._Qroot;
			float num4 = (!flag) ? 1.5f : 0.01f;
			float num5 = (!flag) ? 8f : 0.4f;
			bool flag2 = false;
			num4 *= this._Qroot._Stress;
			num5 *= this._Qroot._Stress;
			int num6 = 0;
			while (this._ParticleGroups != null && num6 < this._ParticleGroups.Length)
			{
				WaveParticlesGroup waveParticlesGroup = this._ParticleGroups[num6];
				if (waveParticlesGroup != null)
				{
					if (waveParticlesGroup.LeftParticle == null || !waveParticlesGroup.LeftParticle.IsAlive)
					{
						this._NumParticleGroups--;
						this._ParticleGroups[num6] = null;
					}
					else if (time >= waveParticlesGroup.LastUpdateTime + num4)
					{
						if (time >= waveParticlesGroup.LastCostlyUpdateTime + num5 && !flag2)
						{
							if (!this.RectContainsParticleGroup(waveParticlesGroup))
							{
								this._NumParticleGroups--;
								this._ParticleGroups[num6] = null;
								goto IL_201;
							}
							waveParticlesGroup.CostlyUpdate(quadtree, time);
							flag2 = true;
							if (waveParticlesGroup.LeftParticle == null || !waveParticlesGroup.LeftParticle.IsAlive)
							{
								this._NumParticleGroups--;
								this._ParticleGroups[num6] = null;
								goto IL_201;
							}
						}
						waveParticlesGroup.Update(time);
					}
				}
				IL_201:
				num6++;
			}
			if (this._Elements != null)
			{
				for (int j = num; j < num2; j++)
				{
					WaveParticle waveParticle = this._Elements[j];
					if (waveParticle != null)
					{
						if (waveParticle.IsAlive)
						{
							if (this._MarginRect.Contains(waveParticle.Position))
							{
								Vector3 vertexData = waveParticle.VertexData;
								Vector4 packedParticleData = waveParticle.PackedParticleData;
								this._Vertices[num3] = vertexData;
								this._TangentsPack[num3++] = packedParticleData;
								this._Vertices[num3] = vertexData;
								this._TangentsPack[num3++] = packedParticleData;
								this._Vertices[num3] = vertexData;
								this._TangentsPack[num3++] = packedParticleData;
								this._Vertices[num3] = vertexData;
								this._TangentsPack[num3++] = packedParticleData;
								this._TangentsPackChanged = true;
							}
							else
							{
								base.RemoveElementAt(j);
								this._Vertices[num3++].x = float.NaN;
								this._Vertices[num3++].x = float.NaN;
								this._Vertices[num3++].x = float.NaN;
								this._Vertices[num3++].x = float.NaN;
								this._Qroot.AddElement(waveParticle);
							}
						}
						else
						{
							base.RemoveElementAt(j);
							this._Vertices[num3++].x = float.NaN;
							this._Vertices[num3++].x = float.NaN;
							this._Vertices[num3++].x = float.NaN;
							this._Vertices[num3++].x = float.NaN;
							waveParticle.AddToCache();
						}
					}
					else
					{
						num3 += 4;
					}
				}
			}
		}

		private bool HasParticleGroup(WaveParticlesGroup group)
		{
			for (int i = 0; i < this._ParticleGroups.Length; i++)
			{
				if (this._ParticleGroups[i] == group)
				{
					return true;
				}
			}
			return false;
		}

		private void AddParticleGroup(WaveParticlesGroup group)
		{
			if (this._ParticleGroups.Length == this._NumParticleGroups)
			{
				Array.Resize<WaveParticlesGroup>(ref this._ParticleGroups, this._NumParticleGroups << 1);
			}
			this._LastGroupIndex++;
			while (this._LastGroupIndex < this._ParticleGroups.Length)
			{
				if (this._ParticleGroups[this._LastGroupIndex] == null)
				{
					this._NumParticleGroups++;
					this._ParticleGroups[this._LastGroupIndex] = group;
					return;
				}
				this._LastGroupIndex++;
			}
			this._LastGroupIndex = 0;
			while (this._LastGroupIndex < this._ParticleGroups.Length)
			{
				if (this._ParticleGroups[this._LastGroupIndex] == null)
				{
					this._NumParticleGroups++;
					this._ParticleGroups[this._LastGroupIndex] = group;
					return;
				}
				this._LastGroupIndex++;
			}
		}

		private bool RectContainsParticleGroup(WaveParticlesGroup group)
		{
			WaveParticle waveParticle = group.LeftParticle;
			if (!waveParticle.IsAlive)
			{
				return false;
			}
			while (!this._MarginRect.Contains(waveParticle.Position))
			{
				waveParticle = waveParticle.RightNeighbour;
				if (waveParticle == null)
				{
					return false;
				}
			}
			return true;
		}

		protected override void AddElementAt(WaveParticle particle, int index)
		{
			base.AddElementAt(particle, index);
			if (!this.HasParticleGroup(particle.Group))
			{
				this.AddParticleGroup(particle.Group);
			}
		}

		protected override void RemoveElementAt(int index)
		{
			base.RemoveElementAt(index);
			int num = index << 2;
			this._Vertices[num++].x = float.NaN;
			this._Vertices[num++].x = float.NaN;
			this._Vertices[num++].x = float.NaN;
			this._Vertices[num].x = float.NaN;
		}

		protected override void SpawnChildNodes()
		{
			this._Mesh.Destroy();
			this._Mesh = null;
			float width = base.Rect.width * 0.5f;
			float height = base.Rect.height * 0.5f;
			this._A = (this._Qa = new WaveParticlesQuadtree(this._Qroot, new Rect(base.Rect.xMin, this._Center.y, width, height), this._Elements.Length));
			this._B = (this._Qb = new WaveParticlesQuadtree(this._Qroot, new Rect(this._Center.x, this._Center.y, width, height), this._Elements.Length));
			this._C = (this._Qc = new WaveParticlesQuadtree(this._Qroot, new Rect(base.Rect.xMin, base.Rect.yMin, width, height), this._Elements.Length));
			this._D = (this._Qd = new WaveParticlesQuadtree(this._Qroot, new Rect(this._Center.x, base.Rect.yMin, width, height), this._Elements.Length));
			this._Vertices = null;
			this._TangentsPack = null;
			this._ParticleGroups = null;
			this._NumParticleGroups = 0;
		}

		private void CreateMesh()
		{
			int num = this._Elements.Length << 2;
			this._Vertices = new Vector3[num];
			for (int i = 0; i < this._Vertices.Length; i++)
			{
				this._Vertices[i].x = float.NaN;
			}
			this._TangentsPack = new Vector4[num];
			Vector2[] array = new Vector2[num];
			int j = 0;
			while (j < array.Length)
			{
				array[j++] = new Vector2(0f, 0f);
				array[j++] = new Vector2(0f, 1f);
				array[j++] = new Vector2(1f, 1f);
				array[j++] = new Vector2(1f, 0f);
			}
			int[] array2 = new int[num];
			for (int k = 0; k < array2.Length; k++)
			{
				array2[k] = k;
			}
			this._Mesh = new Mesh
			{
				hideFlags = HideFlags.DontSave,
				name = "Wave Particles",
				vertices = this._Vertices,
				uv = array,
				tangents = this._TangentsPack
			};
			this._Mesh.SetIndices(array2, MeshTopology.Quads, 0);
		}

		private Mesh _Mesh;

		private Vector3[] _Vertices;

		private Vector4[] _TangentsPack;

		private Vector3[] _DebugData;

		private WaveParticlesGroup[] _ParticleGroups;

		private int _NumParticleGroups;

		private int _LastGroupIndex = -1;

		private float _Stress = 1f;

		private bool _TangentsPackChanged;

		private Stopwatch _Stopwatch;

		private int _LastUpdateIndex;

		private bool _DebugMode;

		private WaveParticlesQuadtree _Qa;

		private WaveParticlesQuadtree _Qb;

		private WaveParticlesQuadtree _Qc;

		private WaveParticlesQuadtree _Qd;

		private readonly WaveParticlesQuadtree _Qroot;
	}
}
