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
		return !ScenarioManager.Get().IsDreamOrPreDream() && (this.m_Datas.Count > 0 && !MapController.Get().IsActive()) && !NotepadController.Get().IsActive();
	}

	public bool IsProcessRegistered(IProcessor processor)
	{
		for (int i = 0; i < this.m_Datas.Count; i++)
		{
			if (this.m_Datas[i].processor == processor)
			{
				return true;
			}
		}
		return false;
	}

	public void RegisterProcess(Trigger trigger, string icon_name, IProcessor processor, bool allow_enabled = false)
	{
		ProcessIconData processIconData = new ProcessIconData();
		processIconData.obj = UnityEngine.Object.Instantiate<GameObject>(this.m_IconPrefab, base.transform);
		processIconData.trigger = trigger;
		processIconData.icon_name = icon_name;
		processIconData.canvas_group = processIconData.obj.GetComponent<CanvasGroup>();
		processIconData.icon = processIconData.obj.transform.Find("Icon").gameObject.GetComponent<Image>();
		Sprite sprite = null;
		ItemsManager.Get().m_ItemIconsSprites.TryGetValue((!icon_name.Empty()) ? icon_name : "Default_Pickup", out sprite);
		processIconData.icon.sprite = sprite;
		processIconData.allow_enabled = allow_enabled;
		processIconData.processor = processor;
		processIconData.progress = processIconData.obj.transform.Find("Progress").gameObject.GetComponent<Image>();
		this.m_Datas.Add(processIconData);
	}

	public void SetIcon(IProcessor processor, string icon_name)
	{
		for (int i = 0; i < this.m_Datas.Count; i++)
		{
			if (this.m_Datas[i].processor == processor)
			{
				ProcessIconData processIconData = this.m_Datas[i];
				Sprite sprite = null;
				ItemsManager.Get().m_ItemIconsSprites.TryGetValue((!icon_name.Empty()) ? icon_name : "Default_Pickup", out sprite);
				processIconData.icon.sprite = sprite;
			}
		}
	}

	public void UnregisterProcess(Trigger trigger)
	{
		foreach (ProcessIconData processIconData in this.m_Datas)
		{
			if (processIconData.trigger == trigger)
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
		if (!CameraManager.Get().m_MainCamera || !base.enabled)
		{
			return;
		}
		for (int i = 0; i < this.m_Datas.Count; i++)
		{
			ProcessIconData processIconData = this.m_Datas[i];
			if (!processIconData.trigger)
			{
				processIconData.obj.SetActive(false);
			}
			else
			{
				processIconData.obj.transform.position = CameraManager.Get().m_MainCamera.WorldToScreenPoint(processIconData.trigger.GetIconPos());
				if (processIconData.obj.transform.position.z <= 0f || (!processIconData.allow_enabled && processIconData.trigger.enabled))
				{
					processIconData.obj.SetActive(false);
				}
				else
				{
					processIconData.obj.SetActive(true);
					float b = Vector3.Distance(processIconData.trigger.transform.position, Player.Get().transform.position);
					float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, b, HUDProcess.s_DistToActivate, HUDProcess.s_DistToActivate * 0.5f);
					processIconData.canvas_group.alpha = proportionalClamp;
					processIconData.progress.fillAmount = processIconData.processor.GetProcessProgress(processIconData.trigger);
				}
			}
		}
	}

	private List<ProcessIconData> m_Datas = new List<ProcessIconData>();

	public GameObject m_IconPrefab;

	public static float s_DistToActivate = 3f;

	private static HUDProcess s_Instance = null;
}
