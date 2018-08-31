using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDInfoLog : HUDBase
{
	public static HUDInfoLog Get()
	{
		return HUDInfoLog.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDInfoLog.s_Instance = null;
		this.m_TargetBGPos = this.m_BG.rectTransform.anchoredPosition;
		this.m_TargetTitlePos = this.m_Title.rectTransform.anchoredPosition;
		this.m_TargetTextPos = this.m_Text.rectTransform.anchoredPosition;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override bool ShouldShow()
	{
		return this.m_Active;
	}

	protected override void Update()
	{
		base.Update();
		if (!this.m_Active)
		{
			if (this.m_Infos.Count > 0)
			{
				this.m_Title.gameObject.SetActive(true);
				this.m_BG.gameObject.SetActive(true);
				this.m_Title.text = this.m_Infos[0].Key;
				this.m_Text.text = this.m_Infos[0].Value;
				this.m_StartTime = Time.time;
				this.m_Active = true;
				this.m_Infos.RemoveAt(0);
				this.PlaceOutsideOfScreen();
			}
		}
		else
		{
			float num = 0.5f;
			if (Time.time - this.m_StartTime > 10f)
			{
				if (this.m_Infos.Count > 0)
				{
					this.m_Title.gameObject.SetActive(true);
					this.m_BG.gameObject.SetActive(true);
					this.m_Title.text = this.m_Infos[0].Key;
					this.m_Text.text = this.m_Infos[0].Value;
					this.m_StartTime = Time.time;
					this.m_Active = true;
					this.m_Infos.RemoveAt(0);
					this.PlaceOutsideOfScreen();
				}
				else
				{
					this.m_Title.gameObject.SetActive(false);
					this.m_BG.gameObject.SetActive(false);
					this.m_StartTime = -1f;
					this.m_Active = false;
				}
			}
			else
			{
				float a = Mathf.Clamp01((10f - (Time.time - this.m_StartTime)) / num);
				Color color = this.m_BG.color;
				color.a = a;
				this.m_BG.color = color;
				this.m_Title.color = color;
				this.m_Text.color = color;
			}
		}
		if (this.m_Active)
		{
			Vector2 anchoredPosition = this.m_BG.rectTransform.anchoredPosition;
			anchoredPosition.x += (this.m_TargetBGPos.x - this.m_BG.rectTransform.anchoredPosition.x) * Time.deltaTime * 6f;
			this.m_BG.rectTransform.anchoredPosition = anchoredPosition;
			anchoredPosition = this.m_Title.rectTransform.anchoredPosition;
			anchoredPosition.x += (this.m_TargetTitlePos.x - this.m_Title.rectTransform.anchoredPosition.x) * Time.deltaTime * 6f;
			this.m_Title.rectTransform.anchoredPosition = anchoredPosition;
			anchoredPosition = this.m_Text.rectTransform.anchoredPosition;
			anchoredPosition.x += (this.m_TargetTextPos.x - this.m_Text.rectTransform.anchoredPosition.x) * Time.deltaTime * 6f;
			this.m_Text.rectTransform.anchoredPosition = anchoredPosition;
		}
	}

	private void PlaceOutsideOfScreen()
	{
		Vector2 anchoredPosition = this.m_BG.rectTransform.anchoredPosition;
		anchoredPosition.x = this.m_TargetBGPos.x + this.m_BG.rectTransform.sizeDelta.x;
		this.m_BG.rectTransform.anchoredPosition = anchoredPosition;
		anchoredPosition = this.m_Title.rectTransform.anchoredPosition;
		anchoredPosition.x = this.m_TargetTitlePos.x + this.m_BG.rectTransform.sizeDelta.x;
		this.m_Title.rectTransform.anchoredPosition = anchoredPosition;
		anchoredPosition = this.m_Text.rectTransform.anchoredPosition;
		anchoredPosition.x = this.m_TargetTextPos.x + this.m_BG.rectTransform.sizeDelta.x;
		this.m_Text.rectTransform.anchoredPosition = anchoredPosition;
	}

	public void AddInfo(string title, string text)
	{
		this.m_Infos.Add(new KeyValuePair<string, string>(title, text));
		this.Update();
	}

	private List<KeyValuePair<string, string>> m_Infos = new List<KeyValuePair<string, string>>();

	public Text m_Title;

	public Text m_Text;

	public RawImage m_BG;

	private bool m_Active;

	private float m_StartTime = -1f;

	private const float m_Duration = 10f;

	public static HUDInfoLog s_Instance;

	private Vector2 m_TargetBGPos = Vector2.zero;

	private Vector2 m_TargetTitlePos = Vector2.zero;

	private Vector2 m_TargetTextPos = Vector2.zero;
}
