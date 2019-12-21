using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCredits : MenuScreen
{
	protected override void Awake()
	{
		base.Awake();
		this.m_FadedColor = this.m_TextColor;
		this.m_FadedColor.a = 0f;
		this.m_CreditsText = base.gameObject.GetComponentInChildren<Text>(true);
		this.m_CreditsText.color = this.m_TextColor;
		this.m_StartY = this.m_CreditsText.rectTransform.anchoredPosition.y;
	}

	public override void OnShow()
	{
		base.OnShow();
		string text = GreenHellGame.Instance.GetLocalization().Get("MainMenu_CreditText", true);
		text.Replace("<br>", Environment.NewLine);
		this.m_CreditsText.text = text;
		Vector2 anchoredPosition = this.m_CreditsText.rectTransform.anchoredPosition;
		anchoredPosition.y = this.m_StartY;
		this.m_CreditsText.rectTransform.anchoredPosition = anchoredPosition;
	}

	protected override void Update()
	{
		base.Update();
		Vector3 vector = this.m_CreditsText.rectTransform.anchoredPosition;
		vector.y += Time.unscaledDeltaTime * this.m_Speed;
		if (vector.y > 1460f)
		{
			vector.y = this.m_StartY;
		}
		this.m_CreditsText.rectTransform.anchoredPosition = vector;
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
		{
			base.StartCoroutine(this.FadeOut());
			if (MainMenuManager.Get())
			{
				MainMenuManager.Get().SetActiveScreen(typeof(MainMenu), true);
				return;
			}
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				GreenHellGame.Instance.ReturnToMainMenu();
			}
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
		yield break;
	}

	public Color m_TextColor;

	private Text m_CreditsText;

	private float m_StartY;

	public float m_Speed = 50f;

	private Color m_FadedColor = Color.white;
}
