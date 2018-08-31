using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class HUDBowlCooking : HUDBase
{
	public static HUDBowlCooking Get()
	{
		return HUDBowlCooking.s_Instance;
	}

	protected override void Awake()
	{
		HUDBowlCooking.s_Instance = this;
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
		return this.m_Datas.Count > 0;
	}

	public void RegisterBowl(Bowl bowl)
	{
		BowlIconData bowlIconData = new BowlIconData();
		bowlIconData.obj = UnityEngine.Object.Instantiate<GameObject>(this.m_IconPrefab, base.transform);
		bowlIconData.canvas_group = bowlIconData.obj.GetComponent<CanvasGroup>();
		bowlIconData.icon = bowlIconData.obj.transform.Find("Icon").gameObject.GetComponent<RawImage>();
		bowlIconData.mask = bowlIconData.obj.transform.Find("Mask").gameObject.GetComponent<Image>();
		this.m_Datas[bowl] = bowlIconData;
	}

	public void UnregisterBowl(Bowl bowl)
	{
		BowlIconData bowlIconData = this.m_Datas[bowl];
		UnityEngine.Object.Destroy(bowlIconData.obj);
		this.m_Datas.Remove(bowl);
	}

	public override void UpdateAfterCamera()
	{
		base.UpdateAfterCamera();
		foreach (Bowl bowl in this.m_Datas.Keys)
		{
			BowlIconData bowlIconData = this.m_Datas[bowl];
			bowlIconData.obj.transform.position = Camera.main.WorldToScreenPoint(bowl.transform.position + Vector3.up * 0.3f);
			if (bowlIconData.obj.transform.position.z <= 0f)
			{
				bowlIconData.obj.SetActive(false);
			}
			else
			{
				bowlIconData.obj.SetActive(true);
				Texture hudicon = bowl.GetHUDIcon();
				if (hudicon && hudicon != bowlIconData.icon.texture)
				{
					bowlIconData.icon.texture = hudicon;
				}
				bowlIconData.mask.fillAmount = bowl.GetCookingLevel();
				float b = Vector3.Distance(bowl.transform.position, Player.Get().transform.position);
				float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, b, HUDBowlCooking.s_DistToActivate, HUDBowlCooking.s_DistToActivate * 0.5f);
				bowlIconData.canvas_group.alpha = proportionalClamp;
			}
		}
	}

	private Dictionary<Bowl, BowlIconData> m_Datas = new Dictionary<Bowl, BowlIconData>();

	public GameObject m_IconPrefab;

	private static HUDBowlCooking s_Instance;

	public static float s_DistToActivate = 3f;
}
