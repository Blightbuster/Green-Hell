using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDHint : HUDBase
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
		this.m_BG.enabled = false;
		this.m_Text.enabled = false;
		this.m_ActiveHint = null;
	}

	protected override void Start()
	{
		base.Start();
		this.m_TextGen = new TextGenerator();
	}

	protected override bool ShouldShow()
	{
		return this.m_HintsQueue.Count > 0;
	}

	protected override void OnHide()
	{
		base.OnHide();
		this.m_BG.enabled = false;
		this.m_Text.enabled = false;
		this.m_ActiveHint = null;
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_HintsQueue.Count == 0)
		{
			this.m_BG.enabled = false;
			this.m_Text.enabled = false;
			this.m_ActiveHint = null;
		}
		else
		{
			this.m_BG.enabled = true;
			this.m_Text.enabled = true;
			if (this.m_ActiveHint != this.m_HintsQueue[0])
			{
				this.m_ActiveHint = this.m_HintsQueue[0];
				this.m_HintShowTime = Time.time;
				this.m_Text.text = this.m_ActiveHint.m_LocalizedText;
				this.UpdateBackground();
			}
			else if (this.m_ActiveHint != null && this.m_ActiveHint.m_Duration > 0f && Time.time > this.m_ActiveHint.m_Duration + this.m_HintShowTime)
			{
				this.m_ActiveHint = null;
				this.m_HintsQueue.RemoveAt(0);
			}
		}
	}

	private void UpdateBackground()
	{
		TextGenerationSettings generationSettings = this.m_Text.GetGenerationSettings(this.m_Text.rectTransform.rect.size);
		generationSettings.scaleFactor = 1f;
		float x = this.m_TextGen.GetPreferredWidth(this.m_Text.text, generationSettings) * 1.05f;
		float y = this.m_TextGen.GetPreferredHeight(this.m_Text.text, generationSettings) * 1.05f;
		this.m_BG.rectTransform.sizeDelta = new Vector2(x, y);
		this.m_BG.rectTransform.position = this.m_Text.rectTransform.position;
	}

	public void ShowHint(Hint hint)
	{
		this.m_HintsQueue.Add(hint);
	}

	public void HideHint(Hint hint)
	{
		this.m_HintsQueue.Remove(hint);
	}

	public bool IsHint(Hint hint)
	{
		return this.m_ActiveHint == hint || this.m_HintsQueue.Contains(hint);
	}

	public RawImage m_BG;

	public Text m_Text;

	private List<Hint> m_HintsQueue = new List<Hint>();

	private Hint m_ActiveHint;

	private float m_HintShowTime;

	private TextGenerator m_TextGen;
}
