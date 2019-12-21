using System;
using UnityEngine;

namespace AIs
{
	public class HearingModule : AIModule
	{
		public void OnNoise(Noise noise)
		{
			float num;
			switch (noise.m_Type)
			{
			case Noise.Type.Sneak:
				num = this.m_AI.m_Params.m_HearingSneakRange;
				break;
			case Noise.Type.Walk:
				num = this.m_AI.m_Params.m_HearingWalkRange;
				break;
			case Noise.Type.Run:
				num = this.m_AI.m_Params.m_HearingRunRange;
				break;
			case Noise.Type.Swim:
				num = this.m_AI.m_Params.m_HearingSwimRange;
				break;
			case Noise.Type.Action:
				num = this.m_AI.m_Params.m_HearingActionRange;
				break;
			default:
				return;
			}
			float num2 = num * num;
			float sqrMagnitude = (this.m_AI.transform.position - noise.m_Position).sqrMagnitude;
			if (sqrMagnitude <= num2)
			{
				this.m_Noise = noise;
				return;
			}
			if (sqrMagnitude <= num2 * 1.5f)
			{
				this.m_LowNoise = noise;
			}
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (this.m_Noise != null && Time.time - this.m_Noise.m_Time >= this.m_NoiseDuration)
			{
				this.m_Noise = null;
			}
			if (this.m_LowNoise != null && Time.time - this.m_LowNoise.m_Time >= this.m_NoiseDuration)
			{
				this.m_LowNoise = null;
			}
			if (this.m_Noise == null && this.m_LowNoise == null)
			{
				this.m_NoNoiseDuration += Time.deltaTime;
				return;
			}
			this.m_NoNoiseDuration = 0f;
		}

		public Noise m_Noise;

		public Noise m_LowNoise;

		private float m_NoiseDuration = 1f;

		[NonSerialized]
		public float m_NoNoiseDuration;
	}
}
