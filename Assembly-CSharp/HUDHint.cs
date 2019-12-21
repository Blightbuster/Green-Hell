using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDHint : HUDBase
{
	public static HUDHint Get()
	{
		return HUDHint.s_Instance;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override void Awake()
	{
		base.Awake();
		HUDHint.s_Instance = this;
		this.m_BG.enabled = false;
		this.m_Text.enabled = false;
		this.m_Icon.enabled = false;
		this.m_ActiveHint = null;
		this.m_IconWidth = this.m_PadIcons[0].rectTransform.rect.width;
	}

	protected override void Start()
	{
		base.Start();
		this.m_TextGen = new TextGenerator();
	}

	protected override bool ShouldShow()
	{
		return this.m_HintsQueue.Count > 0 && GreenHellGame.Instance.m_Settings.m_Hints;
	}

	protected override void OnShow()
	{
		base.OnShow();
		foreach (Image image in this.m_PadIcons)
		{
			image.gameObject.SetActive(false);
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		this.m_BG.enabled = false;
		this.m_Text.enabled = false;
		this.m_Icon.enabled = false;
		this.m_ActiveHint = null;
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_HintsQueue.Count == 0)
		{
			this.m_BG.enabled = false;
			this.m_Text.enabled = false;
			this.m_Icon.enabled = false;
			this.m_ActiveHint = null;
			return;
		}
		this.m_BG.enabled = true;
		this.m_Text.enabled = true;
		this.m_Icon.enabled = true;
		if (this.m_ActiveHint != this.m_HintsQueue[0])
		{
			this.m_ActiveHint = this.m_HintsQueue[0];
			Debug.Log(this.m_ActiveHint.m_Name);
			this.m_HintShowTime = Time.time;
			if (GreenHellGame.IsPCControllerActive())
			{
				this.m_Text.text = this.m_ActiveHint.m_LocalizedText;
			}
			else
			{
				this.SetupPadText();
			}
			this.UpdateBackground();
		}
		else if (this.m_ActiveHint != null && ((this.m_ActiveHint.m_Duration > 0f && Time.time > this.m_ActiveHint.m_Duration + this.m_HintShowTime) || Input.GetKeyDown(KeyCode.Space)))
		{
			this.m_ActiveHint.m_LastShowTime = Time.time;
			this.m_ActiveHint = null;
			this.m_HintsQueue.RemoveAt(0);
		}
		this.UpdateIcon();
	}

	private void UpdateBackground()
	{
		TextGenerationSettings generationSettings = this.m_Text.GetGenerationSettings(this.m_Text.rectTransform.rect.size);
		generationSettings.scaleFactor = 1f;
		float x = this.m_TextGen.GetPreferredWidth(this.m_Text.text, generationSettings) + 20f;
		float y = this.m_TextGen.GetPreferredHeight(this.m_Text.text, generationSettings) * 1.05f;
		this.m_BG.rectTransform.sizeDelta = new Vector2(x, y);
		this.m_BG.rectTransform.position = this.m_Text.rectTransform.position;
	}

	private void UpdateIcon()
	{
		Vector3 v = this.m_BG.rectTransform.anchoredPosition;
		v.x -= this.m_BG.rectTransform.sizeDelta.x * 0.5f + this.m_Icon.rectTransform.sizeDelta.x * 0.5f;
		this.m_Icon.rectTransform.anchoredPosition = v;
		Color white = Color.white;
		white.a = Mathf.Abs(Mathf.Sin((Time.time - this.m_HintShowTime) * 5f));
		if (Time.time - this.m_HintShowTime > 5f)
		{
			white.a = 0f;
		}
		this.m_Icon.color = white;
	}

	public void ShowHint(Hint hint)
	{
		if (GreenHellGame.Instance.m_Settings.m_Hints)
		{
			this.m_HintsQueue.Add(hint);
		}
	}

	public void HideHint(Hint hint)
	{
		foreach (Hint hint2 in this.m_HintsQueue)
		{
			if (hint2.m_Name == hint.m_Name)
			{
				this.m_HintsQueue.Remove(hint2);
				break;
			}
		}
	}

	public bool IsHint(Hint hint)
	{
		return this.m_ActiveHint == hint || this.m_HintsQueue.Contains(hint);
	}

	public bool IsHintActive(Hint hint)
	{
		return this.m_ActiveHint == hint;
	}

	public bool IsAnyHintActive()
	{
		return this.m_ActiveHint != null && this.m_HintsQueue.Count == 0;
	}

	public override void SetupController()
	{
		base.SetupController();
		if (this.m_ActiveHint != null)
		{
			if (GreenHellGame.IsPCControllerActive())
			{
				this.m_Text.text = this.m_ActiveHint.m_LocalizedText;
			}
			else
			{
				this.SetupPadText();
			}
			this.UpdateBackground();
		}
	}

	private void SetupPadText()
	{
		string text = this.m_ActiveHint.m_LocalizedPadText;
		foreach (Image image in this.m_PadIcons)
		{
			image.gameObject.SetActive(false);
		}
		TextGenerationSettings generationSettings = this.m_Text.GetGenerationSettings(this.m_Text.rectTransform.rect.size);
		generationSettings.scaleFactor = 1f;
		float num = 20f;
		float preferredWidth = this.m_TextGen.GetPreferredWidth(" ", generationSettings);
		int num2 = (int)(num / preferredWidth);
		string text2 = "";
		for (int i = 0; i < num2; i++)
		{
			text2 += " ";
		}
		float preferredWidth2 = this.m_TextGen.GetPreferredWidth(text2, generationSettings);
		int j = 0;
		int num3 = 0;
		int num4 = 0;
		j = text.IndexOf("[", j);
		if (j < 0)
		{
			this.m_Text.text = text;
			return;
		}
		string text3 = text;
		int num5 = 0;
		while (j >= 0)
		{
			num3 = text3.IndexOf("]", j);
			if (num4 == 0)
			{
				text = text3.Substring(num4, j - num4);
			}
			else
			{
				text += text3.Substring(num4 + 1, j - num4 - 1);
			}
			text += text2;
			string value = text3.Substring(j + 1, num3 - j - 1);
			InputsManager.InputAction input_action = InputsManager.InputAction.None;
			Enum.TryParse<InputsManager.InputAction>(value, true, out input_action);
			InputActionData actionDataByInputAction = InputsManager.Get().GetActionDataByInputAction(input_action, ControllerType.Pad);
			if (actionDataByInputAction != null)
			{
				this.m_PadIcons[num5].sprite = actionDataByInputAction.m_PadIcon;
				this.m_PadIcons[num5].gameObject.SetActive(true);
			}
			num4 = num3;
			j = text3.IndexOf("[", num4);
			num5++;
		}
		text += text3.Substring(num3 + 1, text3.Length - num3 - 1);
		this.m_Text.text = text;
		float preferredWidth3 = this.m_TextGen.GetPreferredWidth(text, generationSettings);
		Vector3 localPosition = this.m_Text.rectTransform.localPosition;
		localPosition.x -= preferredWidth3 * 0.5f;
		j = 0;
		text3 = text;
		num5 = 0;
		j = text.IndexOf(text2, j);
		while (j >= 0)
		{
			text = text3.Substring(0, j);
			float preferredWidth4 = this.m_TextGen.GetPreferredWidth(text, generationSettings);
			Vector3 localPosition2 = localPosition;
			localPosition2.x += preferredWidth4 + preferredWidth + preferredWidth2 * 0.5f;
			this.m_PadIcons[num5].rectTransform.localPosition = localPosition2;
			num4 = j + num2;
			j = text3.IndexOf(text2, num4);
			num5++;
		}
	}

	public RawImage m_BG;

	public Text m_Text;

	public RawImage m_Icon;

	public List<Image> m_PadIcons = new List<Image>();

	private float m_IconWidth;

	private List<Hint> m_HintsQueue = new List<Hint>();

	private Hint m_ActiveHint;

	private float m_HintShowTime;

	private TextGenerator m_TextGen;

	private static HUDHint s_Instance;
}
