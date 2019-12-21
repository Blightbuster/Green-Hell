using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDSaving : HUDBase
{
	public static HUDSaving Get()
	{
		return HUDSaving.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDSaving.s_Instance = this;
	}

	protected override void Start()
	{
		base.Start();
		this.m_Text.text = GreenHellGame.Instance.GetLocalization().Get("HUD_Saving", true);
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.All);
	}

	protected override bool ShouldShow()
	{
		return this.m_Active;
	}

	protected override void OnShow()
	{
		base.OnShow();
	}

	public void Activate()
	{
		if (!this.m_Active)
		{
			this.m_Active = true;
			this.m_ActivationTime = Time.time;
		}
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateIcon();
		this.UpdateActivity();
	}

	private void UpdateIcon()
	{
		this.m_Icon.rectTransform.Rotate(0f, 0f, -this.m_RotationSpeed);
	}

	private void UpdateActivity()
	{
		if (this.m_Active && SaveGame.m_State != SaveGame.State.Save && SaveGame.m_State != SaveGame.State.SaveCoop && Time.time - this.m_ActivationTime >= this.m_MinActiveTime)
		{
			this.m_Active = false;
		}
	}

	public Text m_Text;

	public Image m_Icon;

	public float m_RotationSpeed;

	private bool m_Active;

	private float m_ActivationTime;

	private float m_MinActiveTime = 3f;

	private static HUDSaving s_Instance;
}
