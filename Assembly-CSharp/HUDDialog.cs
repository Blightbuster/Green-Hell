using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDDialog : HUDBase
{
	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override void Awake()
	{
		base.Awake();
		this.m_TextGen = new TextGenerator();
		this.m_WTIcon = base.transform.Find("WalkieTalkieIcon").GetComponent<RawImage>();
	}

	protected override bool ShouldShow()
	{
		return DialogsManager.Get().m_CurrentDialog != null && ((DialogsManager.Get().m_CurrentDialog.m_CurrentNode != null && DialogsManager.Get().m_CurrentDialog.m_CurrentNode.m_Wait == 0f) || (DialogsManager.Get().m_CurrentDialog.m_CurrentAdditionalNode != null && DialogsManager.Get().m_CurrentDialog.m_CurrentAdditionalNode.m_Wait == 0f));
	}

	protected override void OnShow()
	{
		base.OnShow();
		DialogNode node = this.GetNode();
		if (node != null)
		{
			this.Setup(node);
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		this.m_WTIcon.gameObject.SetActive(false);
		this.m_Text.gameObject.SetActive(false);
		this.m_WTIconActive = false;
	}

	private DialogNode GetNode()
	{
		DialogNode dialogNode = DialogsManager.Get().m_CurrentDialog.m_CurrentNode;
		if (dialogNode == null)
		{
			dialogNode = DialogsManager.Get().m_CurrentDialog.m_CurrentAdditionalNode;
		}
		return dialogNode;
	}

	protected override void Update()
	{
		base.Update();
		if (DialogsManager.Get().m_CurrentDialog == null)
		{
			return;
		}
		DialogNode node = this.GetNode();
		if (node != null && node != this.m_CurrentNode)
		{
			this.Setup(node);
		}
		this.UpdateRadius();
	}

	private void UpdateRadius()
	{
		if (this.m_CurrentNode != null && this.m_CurrentNode.m_SubtitlesRadius > 0f && this.m_CurrentNode.m_Object && this.m_CurrentNode.m_Object != Player.Get().gameObject)
		{
			if (this.m_CurrentNode.m_Object.transform.position.Distance(Player.Get().transform.position) > this.m_CurrentNode.m_SubtitlesRadius)
			{
				this.m_WTIcon.gameObject.SetActive(false);
				this.m_Text.gameObject.SetActive(false);
				return;
			}
			this.m_WTIcon.gameObject.SetActive(this.m_WTIconActive);
			this.m_Text.gameObject.SetActive(true);
		}
	}

	private void Setup(DialogNode node)
	{
		this.m_WTIcon.gameObject.SetActive(false);
		this.m_Text.gameObject.SetActive(false);
		this.m_WTIconActive = false;
		if (node.m_Wait > 0f)
		{
			return;
		}
		if (GreenHellGame.Instance.m_Settings.m_Subtitles == SubtitlesSetting.Off)
		{
			return;
		}
		foreach (Coroutine routine in this.m_Coruotines)
		{
			base.StopCoroutine(routine);
		}
		this.m_Coruotines.Clear();
		foreach (DialogTextData dialogTextData in node.m_TextDatas)
		{
			this.m_Coruotines.Add(base.StartCoroutine(this.SetupText(dialogTextData.m_Text, dialogTextData.m_Time, node.m_WalkieTalkie)));
		}
		this.m_CurrentNode = node;
	}

	private IEnumerator SetupText(string text, float delay, bool wt_active)
	{
		yield return new WaitForSeconds(delay);
		if (base.enabled)
		{
			if (!this.m_Text.gameObject.activeSelf)
			{
				this.m_Text.gameObject.SetActive(true);
			}
			this.m_Text.text = GreenHellGame.Instance.GetLocalization().Get(text, true);
			this.m_Text.fontSize = ((GreenHellGame.Instance.m_Settings.m_Subtitles == SubtitlesSetting.Large) ? HUDDialog.FONT_SIZE_LARGE : HUDDialog.FONT_SIZE);
			TextGenerationSettings generationSettings = this.m_Text.GetGenerationSettings(this.m_Text.rectTransform.rect.size);
			generationSettings.scaleFactor = 1f;
			float num = Mathf.Min(this.m_TextGen.GetPreferredWidth(this.m_Text.text, generationSettings), generationSettings.generationExtents.x);
			this.m_WTIconActive = wt_active;
			if (this.m_WTIcon.gameObject.activeSelf != this.m_WTIconActive)
			{
				this.m_WTIcon.gameObject.SetActive(this.m_WTIconActive);
			}
			if (this.m_WTIcon.gameObject.activeSelf)
			{
				Vector3 localPosition = this.m_Text.rectTransform.localPosition;
				localPosition.x -= num * 0.5f + 25f;
				this.m_WTIcon.transform.localPosition = localPosition;
			}
			this.UpdateRadius();
		}
		yield break;
		yield break;
	}

	public Text m_Text;

	private TextGenerator m_TextGen;

	private DialogNode m_CurrentNode;

	private RawImage m_WTIcon;

	private bool m_WTIconActive;

	private List<Coroutine> m_Coruotines = new List<Coroutine>();

	public static int FONT_SIZE = 18;

	public static int FONT_SIZE_LARGE = 25;
}
