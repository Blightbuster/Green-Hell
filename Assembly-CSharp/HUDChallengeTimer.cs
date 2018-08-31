using System;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class HUDChallengeTimer : HUDBase
{
	public static HUDChallengeTimer Get()
	{
		return HUDChallengeTimer.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDChallengeTimer.s_Instance = this;
		this.m_Color = this.m_TimerText.color;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override bool ShouldShow()
	{
		return this.m_Active;
	}

	public void Activate(Challenge challenge)
	{
		this.m_Active = true;
		this.m_Challenge = challenge;
		this.SetupTimeLimit(challenge);
	}

	public void Deactivate()
	{
		this.m_Active = false;
	}

	private void SetupTimeLimit(Challenge challenge)
	{
		this.m_TimerText.text = GreenHellGame.Instance.GetLocalization().Get("HUDChallenge_TimeLimit");
		Text timerText = this.m_TimerText;
		timerText.text += " ";
		Text timerText2 = this.m_TimerText;
		timerText2.text += ChallengesManager.Get().DateTimeToLocalizedString(challenge.m_EndDate, false);
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_Challenge == null)
		{
			return;
		}
		float gameTime = MainLevel.Instance.m_TODSky.Cycle.GameTime;
		float num = this.m_Challenge.m_Duration - gameTime;
		if (num < this.m_TimeToEndToStartBlink)
		{
			if (!this.m_Blink)
			{
				float proportionalClamp = CJTools.Math.GetProportionalClamp(this.m_MaxBlinkInterval, this.m_MinBlinkInterval, num, this.m_TimeToEndToStartBlink, 0f);
				if (this.m_LastBlinkTime == 0f || Time.time - this.m_LastBlinkTime >= proportionalClamp)
				{
					this.m_Blink = true;
					this.m_StartBlinkTime = Time.time;
				}
			}
			else
			{
				float t = Mathf.Abs(Mathf.Sin((Time.time - this.m_StartBlinkTime) * 5f));
				this.m_TimerText.color = Color.Lerp(this.m_Color, this.m_RedColor, t);
				float num2 = Time.time - this.m_StartBlinkTime;
				if (num2 >= 1.25f)
				{
					this.m_Blink = false;
					this.m_TimerText.color = this.m_Color;
					this.m_LastBlinkTime = Time.time;
				}
			}
		}
	}

	private bool m_Active;

	public Color m_RedColor = Color.red;

	private Color m_Color = Color.white;

	public Text m_TimerText;

	private Challenge m_Challenge;

	public float m_TimeToEndToStartBlink = 12f;

	private float m_StartBlinkTime;

	private float m_LastBlinkTime;

	public float m_MaxBlinkInterval = 100f;

	public float m_MinBlinkInterval = 1f;

	private bool m_Blink;

	private static HUDChallengeTimer s_Instance;
}
