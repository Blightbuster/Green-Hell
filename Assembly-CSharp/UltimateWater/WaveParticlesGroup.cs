using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public class WaveParticlesGroup
	{
		public WaveParticlesGroup(float startTime)
		{
			this._Id = ++WaveParticlesGroup._NextId;
			this.LastCostlyUpdateTime = startTime;
			this.LastUpdateTime = startTime;
		}

		public int ParticleCount
		{
			get
			{
				int num = 0;
				for (WaveParticle waveParticle = this.LeftParticle; waveParticle != null; waveParticle = waveParticle.RightNeighbour)
				{
					num++;
				}
				return num;
			}
		}

		public int Id
		{
			get
			{
				return this._Id;
			}
		}

		public WaveParticle LastParticle
		{
			get
			{
				if (this.LeftParticle == null)
				{
					return null;
				}
				WaveParticle waveParticle = this.LeftParticle;
				while (waveParticle.RightNeighbour != null)
				{
					waveParticle = waveParticle.RightNeighbour;
				}
				return waveParticle;
			}
		}

		public void CostlyUpdate(WaveParticlesQuadtree quadtree, float time)
		{
			WaveParticle waveParticle = this.LeftParticle;
			float deltaTime = time - this.LastCostlyUpdateTime;
			this.LastCostlyUpdateTime = time;
			int num = 0;
			do
			{
				WaveParticle waveParticle2 = waveParticle;
				waveParticle = waveParticle.RightNeighbour;
				num += waveParticle2.CostlyUpdate((num >= 30) ? null : quadtree, deltaTime);
			}
			while (waveParticle != null);
			waveParticle = this.LeftParticle;
			if (waveParticle == null)
			{
				return;
			}
			WaveParticle waveParticle3 = waveParticle;
			int num2 = 0;
			do
			{
				WaveParticle waveParticle4 = waveParticle;
				waveParticle = waveParticle.RightNeighbour;
				num2++;
				if (waveParticle4 != waveParticle3 && (waveParticle4.DisallowSubdivision || waveParticle == null))
				{
					if (num2 > 3)
					{
						WaveParticlesGroup.FilterRefractedDirections(waveParticle3, num2);
					}
					waveParticle3 = waveParticle;
					num2 = 0;
				}
			}
			while (waveParticle != null);
		}

		public void Update(float time)
		{
			WaveParticle waveParticle = this.LeftParticle;
			float num = time - this.LastUpdateTime;
			this.LastUpdateTime = time;
			float num2 = (num >= 1f) ? 1f : num;
			float invStep = 1f - num2;
			do
			{
				WaveParticle waveParticle2 = waveParticle;
				waveParticle = waveParticle.RightNeighbour;
				waveParticle2.Update(num, num2, invStep);
			}
			while (waveParticle != null);
		}

		private static void FilterRefractedDirections(WaveParticle left, int waveLength)
		{
			WaveParticle waveParticle = left;
			int num = waveLength / 2;
			Vector2 a = default(Vector2);
			for (int i = 0; i < num; i++)
			{
				a += waveParticle.Direction;
				waveParticle = waveParticle.RightNeighbour;
			}
			Vector2 vector = default(Vector2);
			for (int j = num; j < waveLength; j++)
			{
				vector += waveParticle.Direction;
				waveParticle = waveParticle.RightNeighbour;
			}
			a.Normalize();
			vector.Normalize();
			waveParticle = left;
			for (int k = 0; k < waveLength; k++)
			{
				waveParticle.Direction = Vector2.Lerp(a, vector, (float)k / (float)(waveLength - 1));
				waveParticle = waveParticle.RightNeighbour;
			}
		}

		[FormerlySerializedAs("lastUpdateTime")]
		public float LastUpdateTime;

		[FormerlySerializedAs("lastCostlyUpdateTime")]
		public float LastCostlyUpdateTime;

		[FormerlySerializedAs("leftParticle")]
		public WaveParticle LeftParticle;

		private readonly int _Id;

		private static int _NextId;
	}
}
