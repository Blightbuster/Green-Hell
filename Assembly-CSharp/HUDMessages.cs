using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class HUDMessages : HUDBase
{
	public static HUDMessages Get()
	{
		return HUDMessages.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDMessages.s_Instance = this;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override bool ShouldShow()
	{
		if (Player.Get().m_DreamActive)
		{
			this.ShiftStartTime();
			return false;
		}
		return this.m_Messages.Count > 0;
	}

	private void ShiftStartTime()
	{
		for (int i = 0; i < this.m_Messages.Count; i++)
		{
			this.m_Messages[i].m_StartTime += Time.deltaTime;
		}
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateElements();
	}

	public void AddMessage(string text, Color? color = null, HUDMessageIcon icon = HUDMessageIcon.None, string icon_name = "")
	{
		string[] array = text.Split(new char[]
		{
			'\n'
		});
		for (int i = 0; i < array.Length; i++)
		{
			HUDMessage hudmessage = new HUDMessage();
			hudmessage.m_StartTime = Time.time;
			hudmessage.m_HudElem = base.AddElement("HUDMessageElement");
			hudmessage.m_HudElem.transform.SetParent(base.transform, false);
			hudmessage.m_TextComponent = hudmessage.m_HudElem.GetComponentInChildren<Text>();
			hudmessage.m_TextComponent.text = array[i];
			hudmessage.m_TextComponent.color = ((color != null) ? color.Value : Color.white);
			hudmessage.m_BGComponent = hudmessage.m_HudElem.transform.FindDeepChild("BG").GetComponentInChildren<RawImage>();
			hudmessage.m_IconComponent = hudmessage.m_HudElem.transform.FindDeepChild("Icon").GetComponentInChildren<Image>();
			if (icon == HUDMessageIcon.None)
			{
				hudmessage.m_IconComponent.enabled = false;
			}
			else if (icon == HUDMessageIcon.Carbo)
			{
				hudmessage.m_IconComponent.enabled = true;
				hudmessage.m_IconComponent.sprite = this.m_CarboIcon;
				hudmessage.m_IconComponent.color = IconColors.GetColor(IconColors.Icon.Carbo);
			}
			else if (icon == HUDMessageIcon.Fat)
			{
				hudmessage.m_IconComponent.enabled = true;
				hudmessage.m_IconComponent.sprite = this.m_FatIcon;
				hudmessage.m_IconComponent.color = IconColors.GetColor(IconColors.Icon.Fat);
			}
			else if (icon == HUDMessageIcon.Proteins)
			{
				hudmessage.m_IconComponent.enabled = true;
				hudmessage.m_IconComponent.sprite = this.m_ProteinsIcon;
				hudmessage.m_IconComponent.color = IconColors.GetColor(IconColors.Icon.Proteins);
			}
			else if (icon == HUDMessageIcon.Hydration)
			{
				hudmessage.m_IconComponent.enabled = true;
				hudmessage.m_IconComponent.sprite = this.m_HydrationIcon;
				hudmessage.m_IconComponent.color = IconColors.GetColor(IconColors.Icon.Hydration);
			}
			else if (icon == HUDMessageIcon.Energy)
			{
				hudmessage.m_IconComponent.enabled = true;
				hudmessage.m_IconComponent.sprite = this.m_EnergyIcon;
				hudmessage.m_IconComponent.color = IconColors.GetColor(IconColors.Icon.Energy);
			}
			else if (icon == HUDMessageIcon.FoodPoisoning)
			{
				hudmessage.m_IconComponent.enabled = true;
				hudmessage.m_IconComponent.sprite = this.m_FoodPoisoningIcon;
				hudmessage.m_IconComponent.color = Color.white;
			}
			else if (icon == HUDMessageIcon.Item)
			{
				if (icon_name.Empty())
				{
					hudmessage.m_IconComponent.enabled = false;
				}
				Sprite sprite = null;
				ItemsManager.Get().m_ItemIconsSprites.TryGetValue(icon_name, out sprite);
				if (sprite != null)
				{
					hudmessage.m_IconComponent.sprite = sprite;
					hudmessage.m_IconComponent.gameObject.SetActive(true);
				}
				else
				{
					hudmessage.m_IconComponent.gameObject.SetActive(false);
				}
			}
			hudmessage.m_HudElem.transform.SetParent(base.transform);
			hudmessage.m_TargetBGPos = hudmessage.m_BGComponent.transform.position;
			hudmessage.m_TargetTextPos = hudmessage.m_TextComponent.transform.position;
			hudmessage.m_TargetIconPos = hudmessage.m_IconComponent.transform.position;
			Vector3 vector = Vector3.zero;
			Vector3 position = hudmessage.m_BGComponent.transform.position;
			vector = position;
			position.x = (float)Screen.width;
			vector -= position;
			hudmessage.m_TextComponent.transform.position -= vector;
			hudmessage.m_BGComponent.transform.position -= vector;
			hudmessage.m_IconComponent.transform.position -= vector;
			if (Player.Get().m_DreamActive)
			{
				hudmessage.m_HudElem.gameObject.SetActive(false);
			}
			this.m_Messages.Insert(0, hudmessage);
		}
		while (this.m_Messages.Count >= this.MAX_COUNT)
		{
			HUDMessage hudmessage2 = this.m_Messages[this.m_Messages.Count - 1];
			UnityEngine.Object.Destroy(hudmessage2.m_HudElem);
			this.m_Messages.Remove(hudmessage2);
		}
		this.UpdateElements();
	}

	private void UpdateElements()
	{
		int i = 0;
		while (i < this.m_Messages.Count)
		{
			HUDMessage hudmessage = this.m_Messages[i];
			if (Time.time - hudmessage.m_StartTime > this.DURATION_TIME)
			{
				UnityEngine.Object.Destroy(hudmessage.m_HudElem);
				this.m_Messages.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
		float num = 0f;
		for (int j = 0; j < this.m_Messages.Count; j++)
		{
			HUDMessage hudmessage2 = this.m_Messages[j];
			hudmessage2.m_HudElem.transform.localPosition = new Vector3(0f, num, 0f);
			Vector3 position = hudmessage2.m_BGComponent.transform.position;
			position.x += (hudmessage2.m_TargetBGPos.x - hudmessage2.m_BGComponent.transform.position.x) * Time.deltaTime * 6f;
			hudmessage2.m_BGComponent.transform.position = position;
			position = hudmessage2.m_TextComponent.transform.position;
			position.x += (hudmessage2.m_TargetTextPos.x - hudmessage2.m_TextComponent.transform.position.x) * Time.deltaTime * 6f;
			hudmessage2.m_TextComponent.transform.position = position;
			position = hudmessage2.m_IconComponent.transform.position;
			position.x += (hudmessage2.m_TargetIconPos.x - hudmessage2.m_IconComponent.transform.position.x) * Time.deltaTime * 6f;
			hudmessage2.m_IconComponent.transform.position = position;
			num -= hudmessage2.m_BGComponent.rectTransform.sizeDelta.y * 0.8f;
			float a = Mathf.Clamp01((this.DURATION_TIME - (Time.time - hudmessage2.m_StartTime)) / 0.5f);
			Color color = hudmessage2.m_TextComponent.color;
			color.a = a;
			hudmessage2.m_TextComponent.color = color;
			color = hudmessage2.m_BGComponent.color;
			color.a = a;
			hudmessage2.m_BGComponent.color = color;
			color = hudmessage2.m_IconComponent.color;
			color.a = a;
			hudmessage2.m_IconComponent.color = color;
		}
	}

	private float DURATION_TIME = 5f;

	private int MAX_COUNT = 10;

	private List<HUDMessage> m_Messages = new List<HUDMessage>();

	private static HUDMessages s_Instance;

	public Sprite m_ProteinsIcon;

	public Sprite m_CarboIcon;

	public Sprite m_FatIcon;

	public Sprite m_HydrationIcon;

	public Sprite m_EnergyIcon;

	public Sprite m_FoodPoisoningIcon;
}
