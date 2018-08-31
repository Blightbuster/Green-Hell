using System;
using UnityEngine;

public class WatchController : PlayerController
{
	public static WatchController Get()
	{
		return WatchController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		WatchController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.Watch;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Animator.SetBool(this.m_BWatch, true);
		Watch.Get().gameObject.SetActive(true);
		PlayerAudioModule.Get().PlayWatchShowSound();
		this.m_InInventory = Inventory3DManager.Get().gameObject.activeSelf;
		if (this.m_InInventory)
		{
			Inventory3DManager.Get().Deactivate();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetBool(this.m_BWatch, false);
		Watch.Get().gameObject.SetActive(false);
		PlayerAudioModule.Get().PlayWatchHideSound();
		if (this.m_InInventory)
		{
			Player.Get().m_OpenBackpackSheduled = true;
			this.m_InInventory = false;
		}
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateInputs();
	}

	private void UpdateInputs()
	{
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (axis > 0.01f)
		{
			this.m_Mode++;
			if (this.m_Mode >= WatchMode.Count)
			{
				this.m_Mode = WatchMode.PlayerParameters;
			}
			if (this.m_Mode == WatchMode.Sanity)
			{
				this.m_Mode = WatchMode.PlayerParameters;
			}
			PlayerAudioModule.Get().PlayWatchSwitchTabSound();
		}
		else if (axis < -0.01f)
		{
			this.m_Mode--;
			if (this.m_Mode < WatchMode.PlayerParameters)
			{
				this.m_Mode = WatchMode.Sanity;
			}
			if (this.m_Mode == WatchMode.Sanity)
			{
				this.m_Mode = WatchMode.Hour;
			}
			PlayerAudioModule.Get().PlayWatchSwitchTabSound();
		}
		if (this.m_Mode == WatchMode.Sanity)
		{
			Watch.Get().SetState(Watch.State.Sanity);
		}
		else if (this.m_Mode == WatchMode.Hour)
		{
			Watch.Get().SetState(Watch.State.Time);
		}
		else if (this.m_Mode == WatchMode.PlayerParameters)
		{
			Watch.Get().SetState(Watch.State.Macronutrients);
		}
		else if (this.m_Mode == WatchMode.Compass)
		{
			Watch.Get().SetState(Watch.State.Compass);
		}
	}

	private int m_BWatch = Animator.StringToHash("Watch");

	[HideInInspector]
	public WatchMode m_Mode;

	private bool m_InInventory;

	private static WatchController s_Instance;
}
