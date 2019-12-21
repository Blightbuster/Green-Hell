using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDChallengeResult : HUDBase, IInputsReceiver
{
	public static HUDChallengeResult Get()
	{
		return HUDChallengeResult.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDChallengeResult.s_Instance = this;
		Color color = this.m_QuitButton.image.color;
		color.a = 0f;
		this.m_QuitButton.image.color = color;
		this.m_QuitText = this.m_QuitButton.GetComponentInChildren<Text>();
		color = this.m_QuitText.color;
		color.a = 0f;
		this.m_QuitText.color = color;
		this.m_QuitButton.interactable = false;
		this.m_QuitButton.gameObject.SetActive(false);
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

	protected override void OnShow()
	{
		base.OnShow();
		Color color = this.m_BG.color;
		color.a = 0f;
		this.m_BG.color = color;
		this.m_CanvasGroup.alpha = 0f;
		color = this.m_QuitButton.image.color;
		color.a = 0f;
		this.m_QuitButton.image.color = color;
		color = this.m_QuitText.color;
		color.a = 0f;
		this.m_QuitText.color = color;
		this.m_QuitButton.interactable = true;
		this.m_QuitButton.gameObject.SetActive(true);
		this.m_CursorVisible = false;
	}

	protected override void OnHide()
	{
		base.OnHide();
		this.m_QuitButton.gameObject.SetActive(false);
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_BG.color.a < 1f)
		{
			Color color = this.m_BG.color;
			color.a += Time.deltaTime;
			color.a = Mathf.Clamp01(color.a);
			this.m_BG.color = color;
		}
		else if (this.m_CanvasGroup.alpha < 1f)
		{
			this.m_CanvasGroup.alpha += Time.deltaTime;
			this.m_CanvasGroup.alpha = Mathf.Clamp01(this.m_CanvasGroup.alpha);
			Color color2 = this.m_QuitButton.image.color;
			color2.a = this.m_CanvasGroup.alpha;
			this.m_QuitButton.image.color = color2;
			color2 = this.m_QuitText.color;
			color2.a = this.m_CanvasGroup.alpha;
			this.m_QuitText.color = color2;
		}
		if (this.m_CanvasGroup.alpha < 1f)
		{
			if (this.m_CursorVisible)
			{
				this.ShowCursor(false);
				return;
			}
		}
		else
		{
			if (GreenHellGame.IsPCControllerActive() && !this.m_CursorVisible)
			{
				this.ShowCursor(true);
				return;
			}
			if (GreenHellGame.IsPadControllerActive() && this.m_CursorVisible)
			{
				this.ShowCursor(false);
			}
		}
	}

	private void ShowCursor(bool show)
	{
		if (show == this.m_CursorVisible)
		{
			return;
		}
		CursorManager.Get().ShowCursor(show, false);
		this.m_CursorVisible = show;
	}

	public void Activate(bool success, Challenge challenge)
	{
		this.m_SuccessText.text = GreenHellGame.Instance.GetLocalization().Get(success ? "HUDChallengeResult_Success" : "HUDChallengeResult_Fail", true);
		this.SetupIcon(challenge);
		this.SetupName(challenge);
		this.SetupStartEndTimes(challenge);
		this.SetupBestResult(challenge);
		this.SetupCurrentResult(challenge);
		this.m_Active = true;
	}

	private void SetupIcon(Challenge challenge)
	{
		this.m_Icon.sprite = challenge.m_Icon;
	}

	private void SetupName(Challenge challenge)
	{
		this.m_NameText.text = GreenHellGame.Instance.GetLocalization().Get(challenge.m_NameID, true);
	}

	private void SetupStartEndTimes(Challenge challenge)
	{
		this.m_StartTimeText.text = ChallengesManager.Get().DateTimeToLocalizedString(challenge.m_StartDate, true);
		this.m_EndTimeText.text = ChallengesManager.Get().DateTimeToLocalizedString(challenge.m_EndDate, true);
	}

	private void SetupBestResult(Challenge challenge)
	{
		if (challenge.m_BestScore > 0f)
		{
			this.m_BestTimeText.text = GreenHellGame.Instance.GetLocalization().Get("ChallengeResult_BestTime", true);
			Text bestTimeText = this.m_BestTimeText;
			bestTimeText.text += " - ";
			DateTime date = challenge.m_StartDate.AddHours((double)challenge.m_BestScore);
			Text bestTimeText2 = this.m_BestTimeText;
			bestTimeText2.text += ChallengesManager.Get().DateTimeToLocalizedString(date, false);
			float fillAmount = challenge.m_BestScore / challenge.m_Duration;
			this.m_BestResultBelt.fillAmount = fillAmount;
			return;
		}
		this.m_BestTimeText.text = string.Empty;
		this.m_BestResultBelt.fillAmount = 0f;
	}

	private void SetupCurrentResult(Challenge challenge)
	{
		this.m_CurrentTimeText.text = GreenHellGame.Instance.GetLocalization().Get("ChallengeResult_CurrTime", true);
		Text currentTimeText = this.m_CurrentTimeText;
		currentTimeText.text += " - ";
		DateTime date = challenge.m_StartDate.AddHours((double)challenge.m_CurrentScore);
		Text currentTimeText2 = this.m_CurrentTimeText;
		currentTimeText2.text += ChallengesManager.Get().DateTimeToLocalizedString(date, false);
		float fillAmount = challenge.m_CurrentScore / challenge.m_Duration;
		this.m_CurrentResultBelt.fillAmount = fillAmount;
	}

	public void OnButtonQuit()
	{
		LoadingScreen.Get().Show(LoadingScreenState.ReturnToMainMenu);
		GreenHellGame.Instance.ReturnToMainMenu();
	}

	public void OnInputAction(InputActionData action_data)
	{
		if (action_data.m_Action == InputsManager.InputAction.Button_B)
		{
			this.OnButtonQuit();
		}
	}

	public bool CanReceiveAction()
	{
		return base.enabled;
	}

	public bool CanReceiveActionPaused()
	{
		return true;
	}

	private bool m_Active;

	public RawImage m_BG;

	public Image m_Icon;

	public Text m_NameText;

	public Text m_SuccessText;

	public Text m_StartTimeText;

	public Text m_EndTimeText;

	public Text m_BestTimeText;

	public Text m_CurrentTimeText;

	public Button m_QuitButton;

	private Text m_QuitText;

	public Image m_CurrentResultBelt;

	public Image m_BestResultBelt;

	public CanvasGroup m_CanvasGroup;

	private bool m_CursorVisible;

	private static HUDChallengeResult s_Instance;
}
