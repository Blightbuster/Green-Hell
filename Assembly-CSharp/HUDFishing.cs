using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDFishing : HUDBase
{
	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override void Start()
	{
		base.Start();
		this.m_FishingController = Player.Get().GetComponent<FishingController>();
		DebugUtils.Assert(this.m_FishingController, true);
		this.m_MarkerStartY = this.m_Marker.rectTransform.position.y;
		this.m_Marker.enabled = false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Marker.enabled = true;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Marker.enabled = true;
	}

	protected override bool ShouldShow()
	{
		return false;
	}

	protected override void Update()
	{
		base.Update();
		Vector3 position = this.m_Marker.rectTransform.position;
		position.y = this.m_MarkerStartY + this.m_MarkerMaxShift * 1f;
		this.m_Marker.rectTransform.position = position;
	}

	private FishingController m_FishingController;

	public Image m_Marker;

	private float m_MarkerStartY;

	public float m_MarkerMaxShift;
}
