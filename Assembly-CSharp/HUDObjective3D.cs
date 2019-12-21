using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class HUDObjective3D : HUDBase
{
	public static HUDObjective3D Get()
	{
		return HUDObjective3D.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDObjective3D.s_Instance = this;
		if (GreenHellGame.ROADSHOW_DEMO)
		{
			this.m_Active = true;
		}
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override bool ShouldShow()
	{
		return this.m_Active && this.m_ShowObjectives && !Player.Get().m_DreamActive && !MapController.Get().IsActive() && !NotepadController.Get().IsActive();
	}

	public override void UpdateAfterCamera()
	{
		base.UpdateAfterCamera();
		if (CameraManager.Get().m_MainCamera == null)
		{
			foreach (GameObject key in this.m_Objectives.Keys)
			{
				HUDObjective3D.Objective objective = this.m_Objectives[key];
				objective.dist.gameObject.SetActive(false);
				objective.icon.gameObject.SetActive(false);
				objective.text.gameObject.SetActive(false);
			}
			return;
		}
		foreach (GameObject gameObject in this.m_Objectives.Keys)
		{
			HUDObjective3D.Objective objective2 = this.m_Objectives[gameObject];
			Vector3 vector = CameraManager.Get().m_MainCamera.WorldToViewportPoint(gameObject.transform.position);
			vector = CameraManager.Get().m_MainCamera.ViewportToScreenPoint(vector);
			if (vector.z <= 0f)
			{
				objective2.icon.gameObject.SetActive(false);
			}
			else
			{
				objective2.icon.gameObject.SetActive(true);
				objective2.icon.transform.position = vector;
				Color color = objective2.icon.color;
				float num = Vector3.Distance(gameObject.transform.position, Player.Get().transform.position);
				color.a = CJTools.Math.GetProportionalClamp(0.6f, 1f, num, 10f, 0f);
				objective2.icon.color = color;
				color = objective2.text.color;
				color.a = objective2.icon.color.a;
				objective2.text.color = color;
				objective2.text.gameObject.SetActive(true);
				objective2.dist.text = Mathf.CeilToInt(num).ToString() + "m";
				objective2.dist.color = color;
				objective2.dist.gameObject.SetActive(true);
			}
		}
	}

	public void AddObjective(GameObject obj, string text)
	{
		if (!obj || this.m_Objectives.ContainsKey(obj))
		{
			return;
		}
		HUDObjective3D.Objective objective = default(HUDObjective3D.Objective);
		objective.icon = UnityEngine.Object.Instantiate<GameObject>(this.m_IconPrefab, base.transform).GetComponent<RawImage>();
		objective.text = objective.icon.transform.Find("Name").GetComponent<Text>();
		objective.text.text = GreenHellGame.Instance.GetLocalization().Get(text, true);
		objective.dist = objective.icon.transform.Find("Dist").GetComponent<Text>();
		this.m_Objectives.Add(obj, objective);
		this.m_ShowObjectives = true;
	}

	public void RemoveObjective(GameObject obj)
	{
		if (!obj)
		{
			if (!GreenHellGame.ROADSHOW_DEMO)
			{
				DebugUtils.Assert("[HUDObjective3D:RemoveObjective] Can't remove non-existent objective", true, DebugUtils.AssertType.Info);
			}
			return;
		}
		if (!this.m_Objectives.ContainsKey(obj))
		{
			return;
		}
		UnityEngine.Object.Destroy(this.m_Objectives[obj].icon.gameObject);
		this.m_Objectives.Remove(obj);
		this.m_ShowObjectives = (this.m_Objectives.Count > 0);
	}

	private Dictionary<GameObject, HUDObjective3D.Objective> m_Objectives = new Dictionary<GameObject, HUDObjective3D.Objective>();

	public GameObject m_IconPrefab;

	public bool m_Active;

	private bool m_ShowObjectives;

	private static HUDObjective3D s_Instance;

	private struct Objective
	{
		public RawImage icon;

		public Text text;

		public Text dist;
	}
}
