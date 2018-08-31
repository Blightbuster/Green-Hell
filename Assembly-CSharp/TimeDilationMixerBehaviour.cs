using System;
using UnityEngine;
using UnityEngine.Playables;

public class TimeDilationMixerBehaviour : PlayableBehaviour
{
	public override void OnPlayableCreate(Playable playable)
	{
		this.m_OldTimeScale = Time.timeScale;
	}

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		int inputCount = playable.GetInputCount<Playable>();
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < inputCount; i++)
		{
			float inputWeight = playable.GetInputWeight(i);
			num2 += inputWeight;
			TimeDilationBehaviour behaviour = ((ScriptPlayable<T>)playable.GetInput(i)).GetBehaviour();
			num += inputWeight * behaviour.timeScale;
		}
		Time.timeScale = num + this.m_OldTimeScale * (1f - num2);
	}

	public override void OnPlayableDestroy(Playable playable)
	{
		Time.timeScale = this.m_OldTimeScale;
	}

	private float m_OldTimeScale = 1f;
}
