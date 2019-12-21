using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class HUDAI3D : HUDBase
{
	public static HUDAI3D Get()
	{
		return HUDAI3D.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDAI3D.s_Instance = this;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override bool ShouldShow()
	{
		return this.m_DebugActive && this.m_AIs.Count > 0 && !Player.Get().m_DreamActive && !MapController.Get().IsActive() && !NotepadController.Get().IsActive();
	}

	public override void UpdateAfterCamera()
	{
		base.UpdateAfterCamera();
		if (Camera.main == null)
		{
			foreach (GameObject key in this.m_AIs.Keys)
			{
				HUDAI3D.AIData aidata = this.m_AIs[key];
				aidata.dist.gameObject.SetActive(false);
				aidata.icon.gameObject.SetActive(false);
				aidata.text.gameObject.SetActive(false);
			}
			return;
		}
		foreach (GameObject gameObject in this.m_AIs.Keys)
		{
			HUDAI3D.AIData aidata2 = this.m_AIs[gameObject];
			Vector3 vector = gameObject.transform.position;
			vector = CameraManager.Get().m_MainCamera.WorldToViewportPoint(vector);
			vector = CameraManager.Get().m_MainCamera.ViewportToScreenPoint(vector);
			if (vector.z <= 0f)
			{
				aidata2.icon.gameObject.SetActive(false);
			}
			else
			{
				aidata2.icon.gameObject.SetActive(true);
				aidata2.icon.transform.position = vector;
				Color color = aidata2.icon.color;
				float num = Vector3.Distance(gameObject.transform.position, Player.Get().transform.position);
				color.a = CJTools.Math.GetProportionalClamp(0.6f, 1f, num, 10f, 0f);
				aidata2.icon.color = color;
				color = aidata2.text.color;
				color.a = aidata2.icon.color.a;
				aidata2.text.color = color;
				aidata2.text.gameObject.SetActive(true);
				aidata2.dist.text = Mathf.CeilToInt(num).ToString() + "m";
				aidata2.dist.color = color;
				aidata2.dist.gameObject.SetActive(true);
			}
		}
	}

	public void AddAI(GameObject obj, string text = "")
	{
		if (!obj || this.m_AIs.ContainsKey(obj))
		{
			return;
		}
		HUDAI3D.AIData aidata = default(HUDAI3D.AIData);
		aidata.icon = UnityEngine.Object.Instantiate<GameObject>(this.m_IconPrefab, base.transform).GetComponent<RawImage>();
		aidata.text = aidata.icon.transform.Find("Name").GetComponent<Text>();
		aidata.text.text = text;
		aidata.dist = aidata.icon.transform.Find("Dist").GetComponent<Text>();
		aidata.dist.gameObject.SetActive(base.enabled);
		aidata.icon.gameObject.SetActive(base.enabled);
		aidata.text.gameObject.SetActive(base.enabled);
		this.m_AIs.Add(obj, aidata);
	}

	public void RemoveAI(GameObject obj)
	{
		if (!obj)
		{
			if (!GreenHellGame.ROADSHOW_DEMO)
			{
				DebugUtils.Assert("[HUDAI3D:RemoveAI] Can't remove non-existent AI", true, DebugUtils.AssertType.Info);
			}
			return;
		}
		if (!this.m_AIs.ContainsKey(obj))
		{
			return;
		}
		if (this.m_AIs[obj].icon != null)
		{
			UnityEngine.Object.Destroy(this.m_AIs[obj].icon.gameObject);
		}
		this.m_AIs.Remove(obj);
	}

	private Dictionary<GameObject, HUDAI3D.AIData> m_AIs = new Dictionary<GameObject, HUDAI3D.AIData>();

	public GameObject m_IconPrefab;

	private static HUDAI3D s_Instance;

	public bool m_DebugActive;

	private struct AIData
	{
		public RawImage icon;

		public Text text;

		public Text dist;
	}
}
