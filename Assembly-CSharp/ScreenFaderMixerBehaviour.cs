using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ScreenFaderMixerBehaviour : PlayableBehaviour
{
	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		this.m_TrackBinding = (playerData as Image);
		if (this.m_TrackBinding == null)
		{
			return;
		}
		if (!this.m_FirstFrameHappened)
		{
			this.m_DefaultColor = this.m_TrackBinding.color;
			this.m_FirstFrameHappened = true;
		}
		int inputCount = playable.GetInputCount<Playable>();
		Color a = Color.clear;
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		for (int i = 0; i < inputCount; i++)
		{
			float inputWeight = playable.GetInputWeight(i);
			ScreenFaderBehaviour behaviour = ((ScriptPlayable<T>)playable.GetInput(i)).GetBehaviour();
			a += behaviour.color * inputWeight;
			num += inputWeight;
			if (inputWeight > num2)
			{
				num2 = inputWeight;
			}
			if (!Mathf.Approximately(inputWeight, 0f))
			{
				num3++;
			}
		}
		this.m_TrackBinding.color = a + this.m_DefaultColor * (1f - num);
	}

	public override void OnPlayableDestroy(Playable playable)
	{
		this.m_FirstFrameHappened = false;
		if (this.m_TrackBinding == null)
		{
			return;
		}
		this.m_TrackBinding.color = this.m_DefaultColor;
	}

	private Color m_DefaultColor;

	private Image m_TrackBinding;

	private bool m_FirstFrameHappened;
}
