using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class HUDProcess : HUDBase
{
	public static HUDProcess Get()
	{
		return HUDProcess.s_Instance;
	}

	protected override void Awake()
	{
		HUDProcess.s_Instance = this;
		base.Awake();
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override bool ShouldShow()
	{
		return this.m_Datas.Count > 0 && !MapController.Get().IsActive() && !NotepadController.Get().IsActive();
	}

	public void RegisterProcess(Item item, string icon_name, IProcessor processor, bool allow_enabled = false)
	{
		ProcessIconData processIconData = new ProcessIconData();
		processIconData.obj = UnityEngine.Object.Instantiate<GameObject>(this.m_IconPrefab, base.transform);
		processIconData.item = item;
		processIconData.icon_name = icon_name;
		processIconData.canvas_group = processIconData.obj.GetComponent<CanvasGroup>();
		processIconData.icon = processIconData.obj.transform.Find("Icon").gameObject.GetComponent<Image>();
		Sprite sprite = null;
		ItemsManager.Get().m_ItemIconsSprites.TryGetValue(icon_name.Empty() ? "Default_Pickup" : icon_name, out sprite);
		processIconData.icon.sprite = sprite;
		processIconData.allow_enabled = allow_enabled;
		processIconData.processor = processor;
		processIconData.progress = processIconData.obj.transform.Find("Progress").gameObject.GetComponent<Image>();
		this.m_Datas.Add(processIconData);
	}

	public void UnregisterProcess(Item item)
	{
		foreach (ProcessIconData processIconData in this.m_Datas)
		{
			if (processIconData.item == item)
			{
				UnityEngine.Object.Destroy(processIconData.obj);
				this.m_Datas.Remove(processIconData);
				break;
			}
		}
	}

	public override void UpdateAfterCamera()
	{
		base.UpdateAfterCamera();
		if (!Camera.main || !base.enabled)
		{
			return;
		}
		for (int i = 0; i < this.m_Datas.Count; i++)
		{
			ProcessIconData processIconData = this.m_Datas[i];
			processIconData.obj.transform.position = Camera.main.WorldToScreenPoint(processIconData.item.GetIconPos());
			if (processIconData.obj.transform.position.z <= 0f || (!processIconData.allow_enabled && processIconData.item.enabled))
			{
				processIconData.obj.SetActive(false);
			}
			else
			{
				processIconData.obj.SetActive(true);
				float b = Vector3.Distance(processIconData.item.transform.position, Player.Get().transform.position);
				float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, b, HUDProcess.s_DistToActivate, HUDProcess.s_DistToActivate * 0.5f);
				processIconData.canvas_group.alpha = proportionalClamp;
				processIconData.progress.fillAmount = processIconData.processor.GetProcessProgress(processIconData.item);
			}
		}
	}

	private List<ProcessIconData> m_Datas = new List<ProcessIconData>();

	public GameObject m_IconPrefab;

	public static float s_DistToActivate = 3f;

	private static HUDProcess s_Instance;
}
