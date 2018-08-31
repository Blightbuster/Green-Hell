using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCredits : MainMenuScreen
{
	private void Awake()
	{
		this.m_FadedColor = this.m_TextColor;
		this.m_FadedColor.a = 0f;
		this.m_CreditsText = base.gameObject.GetComponentInChildren<Text>(true);
		this.m_CreditsText.color = this.m_TextColor;
		this.m_StartY = this.m_CreditsText.rectTransform.anchoredPosition.y;
	}

	public override void OnShow()
	{
		base.OnShow();
		string text = GreenHellGame.Instance.GetLocalization().Get("MainMenu_CreditText");
		text.Replace("<br>", Environment.NewLine);
		this.m_CreditsText.text = text;
	}

	private void Update()
	{
		Vector3 v = this.m_CreditsText.rectTransform.anchoredPosition;
		v.y += Time.unscaledDeltaTime * this.m_Speed;
		if (v.y > 1700f)
		{
			v.y = this.m_StartY;
		}
		this.m_CreditsText.rectTransform.anchoredPosition = v;
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
		{
			base.StartCoroutine(this.FadeOut());
			MainMenuManager.Get().SetActiveScreen(typeof(MainMenu), true);
		}
	}

	public void OnEnable()
	{
		if (this.m_CreditsText != null)
		{
			this.m_CreditsText.color = this.m_TextColor;
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
	}

	public Color m_TextColor;

	private Text m_CreditsText;

	private float m_StartY;

	public float m_Speed = 50f;

	private Color m_FadedColor = Color.white;
}
