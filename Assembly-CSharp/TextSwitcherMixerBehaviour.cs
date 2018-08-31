using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class TextSwitcherMixerBehaviour : PlayableBehaviour
{
	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		this.m_TrackBinding = (playerData as Text);
		if (this.m_TrackBinding == null)
		{
			return;
		}
		if (!this.m_FirstFrameHappened)
		{
			this.m_DefaultColor = this.m_TrackBinding.color;
			this.m_DefaultFontSize = this.m_TrackBinding.fontSize;
			this.m_DefaultText = this.m_TrackBinding.text;
			this.m_FirstFrameHappened = true;
		}
		int inputCount = playable.GetInputCount<Playable>();
		Color a = Color.clear;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		int num4 = 0;
		for (int i = 0; i < inputCount; i++)
		{
			float inputWeight = playable.GetInputWeight(i);
			TextSwitcherBehaviour behaviour = ((ScriptPlayable<T>)playable.GetInput(i)).GetBehaviour();
			a += behaviour.color * inputWeight;
			num += (float)behaviour.fontSize * inputWeight;
			num2 += inputWeight;
			if (inputWeight > num3)
			{
				this.m_TrackBinding.text = behaviour.text;
				num3 = inputWeight;
			}
			if (!Mathf.Approximately(inputWeight, 0f))
			{
				num4++;
			}
		}
		this.m_TrackBinding.color = a + this.m_DefaultColor * (1f - num2);
		this.m_TrackBinding.fontSize = Mathf.RoundToInt(num + (float)this.m_DefaultFontSize * (1f - num2));
		if (num4 != 1 && 1f - num2 > num3)
		{
			this.m_TrackBinding.text = this.m_DefaultText;
		}
	}

	public override void OnPlayableDestroy(Playable playable)
	{
		this.m_FirstFrameHappened = false;
		if (this.m_TrackBinding == null)
		{
			return;
		}
		this.m_TrackBinding.color = this.m_DefaultColor;
		this.m_TrackBinding.fontSize = this.m_DefaultFontSize;
		this.m_TrackBinding.text = this.m_DefaultText;
	}

	private Color m_DefaultColor;

	private int m_DefaultFontSize;

	private string m_DefaultText;

	private Text m_TrackBinding;

	private bool m_FirstFrameHappened;
}
