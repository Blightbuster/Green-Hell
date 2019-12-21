using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class MenuDebugCamera : MenuDebugScreen
{
	public override void OnShow()
	{
		base.OnShow();
		this.m_FOVValue.value = Camera.main.fieldOfView;
		if (this.m_ToggleAddRoll.isOn)
		{
			this.m_RollValue.value = CameraManager.Get().m_AddRollValue;
		}
		else
		{
			this.m_RollValue.value = 0f;
		}
		global::PostProcessManager.Get().GetVolume(global::PostProcessManager.Effect.DebugDof).profile.TryGetSettings<DepthOfField>(out this.m_DofLayer);
	}

	protected override void Update()
	{
		base.Update();
		CameraManager.Get().m_OverrideFOV = this.m_ToggleOverrideFov.isOn;
		CameraManager.Get().m_OverideFOVValue = this.m_FOVValue.value;
		CameraManager.Get().m_AddRoll = this.m_ToggleAddRoll.isOn;
		CameraManager.Get().m_AddRollValue = this.m_RollValue.value;
		PlayerPostProcessModule.Get().m_DebugDofPP = this.m_ToggleEnableDof.isOn;
		if (this.m_ToggleEnableDof.isOn)
		{
			this.m_DofLayer.focusDistance.value = this.m_DofFocusDistance.value;
			this.m_DofLayer.aperture.value = this.m_DofAperture.value;
			this.m_DofLayer.focalLength.value = this.m_DofFocalLength.value;
		}
		this.m_FocusDistanceText.text = "Focus distance: " + this.m_DofFocusDistance.value.ToString();
		this.m_ApertureText.text = "Aperture: " + this.m_DofAperture.value.ToString();
		this.m_FocalLengthText.text = "Focal lemgth: " + this.m_DofFocalLength.value.ToString();
	}

	public Toggle m_ToggleOverrideFov;

	public Slider m_FOVValue;

	public Toggle m_ToggleAddRoll;

	public Slider m_RollValue;

	public Toggle m_ToggleEnableDof;

	public Slider m_DofFocusDistance;

	public Text m_FocusDistanceText;

	public Slider m_DofAperture;

	public Text m_ApertureText;

	public Slider m_DofFocalLength;

	public Text m_FocalLengthText;

	private DepthOfField m_DofLayer;
}
