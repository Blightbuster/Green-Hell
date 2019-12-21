using System;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class HUDCrosshair : HUDBase
{
	public static HUDCrosshair Get()
	{
		return HUDCrosshair.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDCrosshair.s_Instance = this;
	}

	protected override void Start()
	{
		base.Start();
		this.m_CrosshairLeft.rectTransform.sizeDelta = this.m_Size;
		this.m_CrosshairRight.rectTransform.sizeDelta = this.m_Size;
		this.m_Crosshair_Down.rectTransform.sizeDelta = this.m_Size;
		this.m_CrosshairLeft.color = this.m_CrosshairColor;
		this.m_CrosshairRight.color = this.m_CrosshairColor;
		this.m_Crosshair_Down.color = this.m_CrosshairColor;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	public override void ConstantUpdate()
	{
		base.ConstantUpdate();
		if (Player.Get().IsDead())
		{
			this.m_ShowCrosshair = false;
		}
	}

	protected override bool ShouldShow()
	{
		return GreenHellGame.Instance.m_Settings.m_Crosshair && !NotepadController.Get().IsActive() && !MapController.Get().IsActive();
	}

	public void ShowCrosshair()
	{
		this.m_ShowCrosshair = true;
	}

	public void HideCrosshair()
	{
		this.m_ShowCrosshair = false;
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.UpdateAimVisibility();
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateAimVisibility();
	}

	private void UpdateAimVisibility()
	{
		bool active = this.m_ShowCrosshair && !Inventory3DManager.Get().gameObject.activeSelf && !BodyInspectionController.Get().IsActive() && !HUDWheel.Get().enabled && !HUDNewWheel.Get().enabled && !BodyInspectionMiniGameController.Get().IsActive();
		this.m_CrosshairLeft.gameObject.SetActive(active);
		this.m_CrosshairRight.gameObject.SetActive(active);
		this.m_Crosshair_Down.gameObject.SetActive(active);
	}

	public void SetAimPower(float power)
	{
		this.m_WantedDistance = CJTools.Math.GetProportionalClamp(this.m_InitialDistance, this.m_FinalDistance, power, 0f, 1f);
		if (this.m_WantedDistance >= this.m_FinalDistance)
		{
			this.m_TempPosition = Vector3.zero;
			this.m_TempPosition.x = this.m_WantedDistance;
			this.m_CrosshairRight.rectTransform.localPosition = this.m_TempPosition;
			this.m_TempPosition.x = this.m_WantedDistance * -1f;
			this.m_CrosshairLeft.rectTransform.localPosition = this.m_TempPosition;
			this.m_TempPosition = Vector3.zero;
			this.m_TempPosition.y = this.m_WantedDistance * -1f;
			this.m_Crosshair_Down.rectTransform.localPosition = this.m_TempPosition;
		}
	}

	private bool m_ShowCrosshair;

	public RawImage m_CrosshairLeft;

	public RawImage m_CrosshairRight;

	public RawImage m_Crosshair_Down;

	public Vector2 m_Size = new Vector2(30f, 3f);

	public Color m_CrosshairColor = Color.white;

	private float m_InitialDistance = 50f;

	public const float FINAL_DISTANCE = 18f;

	[HideInInspector]
	public float m_FinalDistance;

	private float m_WantedDistance;

	private Vector3 m_TempPosition = Vector3.zero;

	private static HUDCrosshair s_Instance;
}
