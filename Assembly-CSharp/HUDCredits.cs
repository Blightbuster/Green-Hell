using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUDCredits : HUDBase
{
	protected override void Awake()
	{
		base.Awake();
		this.m_FadedColor = this.m_TextColor;
		this.m_FadedColor.a = 0f;
		this.m_CreditsText = base.gameObject.GetComponentInChildren<Text>(true);
		this.m_CreditsText.color = this.m_TextColor;
		string text = GreenHellGame.Instance.GetLocalization().Get("MainMenu_CreditText", true);
		text.Replace("<br>", Environment.NewLine);
		this.m_CreditsText.text = text;
		this.m_StartY = this.m_CreditsText.rectTransform.anchoredPosition.y;
		Vector2 anchoredPosition = this.m_CreditsText.rectTransform.anchoredPosition;
		anchoredPosition.y = this.m_StartY;
		this.m_CreditsText.rectTransform.anchoredPosition = anchoredPosition;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Credits);
	}

	protected override bool ShouldShow()
	{
		return true;
	}

	protected override void Update()
	{
		base.Update();
		if (MainLevel.Instance.IsPause())
		{
			return;
		}
		Vector3 vector = this.m_CreditsText.rectTransform.anchoredPosition;
		vector.y += Time.unscaledDeltaTime * this.m_TextSpeed;
		this.m_CreditsText.rectTransform.anchoredPosition = vector;
		if ((!this.m_Returning && !Music.Get().IsMusicPlaying(0) && vector.y > 2010f) || Input.GetKeyDown(KeyCode.Space))
		{
			GreenHellGame.Instance.ReturnToMainMenu();
			this.m_Returning = true;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (this.m_CreditsText != null)
		{
			this.m_CreditsText.color = this.m_TextColor;
			Vector2 anchoredPosition = this.m_CreditsText.rectTransform.anchoredPosition;
			anchoredPosition.y = this.m_StartY;
			this.m_CreditsText.rectTransform.anchoredPosition = anchoredPosition;
		}
	}

	private IEnumerator FadeOut()
	{
		float time = 0f;
		while (time <= 0.5f)
		{
			this.m_CreditsText.color = Color.Lerp(this.m_TextColor, this.m_FadedColor, time);
			time += Time.unscaledDeltaTime;
			yield return null;
		}
		yield break;
		yield break;
	}

	public Color m_TextColor;

	private Text m_CreditsText;

	private float m_StartY;

	public float m_TextSpeed = 28f;

	private Color m_FadedColor = Color.white;

	private bool m_Returning;
}
