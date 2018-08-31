using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class HUDFirecamp : HUDBase
{
	public static HUDFirecamp Get()
	{
		return HUDFirecamp.s_Instance;
	}

	protected override void Awake()
	{
		HUDFirecamp.s_Instance = this;
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

	public void RegisterFirecamp(Firecamp firecamp)
	{
		FirecampIconData firecampIconData = new FirecampIconData();
		firecampIconData.obj = UnityEngine.Object.Instantiate<GameObject>(this.m_IconPrefab, base.transform);
		firecampIconData.firecamp = firecamp;
		firecampIconData.canvas_group = firecampIconData.obj.GetComponent<CanvasGroup>();
		firecampIconData.icon = firecampIconData.obj.transform.Find("Icon").gameObject.GetComponent<Image>();
		firecampIconData.mask = firecampIconData.obj.transform.Find("Mask").gameObject.GetComponent<Image>();
		this.m_Datas.Add(firecampIconData);
		this.UpdateIcons();
	}

	public void UnregisterFirecamp(Firecamp firecamp)
	{
		foreach (FirecampIconData firecampIconData in this.m_Datas)
		{
			if (firecampIconData.firecamp == firecamp)
			{
				UnityEngine.Object.Destroy(firecampIconData.obj);
				this.m_Datas.Remove(firecampIconData);
				break;
			}
		}
	}

	public override void UpdateAfterCamera()
	{
		base.UpdateAfterCamera();
		this.UpdateIcons();
	}

	private void UpdateIcons()
	{
		if (!Camera.main)
		{
			return;
		}
		foreach (FirecampIconData firecampIconData in this.m_Datas)
		{
			if (!firecampIconData.firecamp.m_Burning || !base.enabled)
			{
				firecampIconData.obj.SetActive(false);
			}
			else
			{
				firecampIconData.obj.transform.position = Camera.main.WorldToScreenPoint(firecampIconData.firecamp.transform.position + Vector3.up * 0.5f);
				if (firecampIconData.obj.transform.position.z <= 0f)
				{
					firecampIconData.obj.SetActive(false);
				}
				else
				{
					firecampIconData.obj.SetActive(true);
					float b = Vector3.Distance(firecampIconData.firecamp.transform.position, Player.Get().transform.position);
					float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, b, HUDFirecamp.s_DistToActivate, HUDFirecamp.s_DistToActivate * 0.5f);
					firecampIconData.canvas_group.alpha = proportionalClamp;
					firecampIconData.mask.fillAmount = 1f - firecampIconData.firecamp.m_FireLevel;
				}
			}
		}
	}

	private List<FirecampIconData> m_Datas = new List<FirecampIconData>();

	public GameObject m_IconPrefab;

	private static HUDFirecamp s_Instance;

	public static float s_DistToActivate = 3f;
}
