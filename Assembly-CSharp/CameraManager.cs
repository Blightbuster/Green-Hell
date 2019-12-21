using System;
using CJTools;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
	public Camera m_MainCamera
	{
		get
		{
			if (this.m_MainCameraInternal == null)
			{
				this.m_MainCameraInternal = Camera.main;
			}
			return this.m_MainCameraInternal;
		}
	}

	public static CameraManager Get()
	{
		return CameraManager.s_Instance;
	}

	private void Awake()
	{
		CameraManager.s_Instance = this;
		this.m_DefaultFOV = Camera.main.fieldOfView;
		this.m_OverideFOVValue = Camera.main.fieldOfView;
		this.m_UnderShowerEffect = Camera.main.gameObject.GetComponent<WaterWarpImageEffect>();
		this.m_MainCameraInternal = Camera.main;
	}

	public void SetTarget(Being target)
	{
		this.m_Target = target;
		if (this.m_Target.IsPlayer())
		{
			Player.Get().UnblockMoves();
			Player.Get().UnblockRotation();
			return;
		}
		Player.Get().BlockMoves();
		Player.Get().BlockRotation();
	}

	public void SetMode(CameraManager.Mode mode)
	{
		this.m_Mode = mode;
	}

	private void Update()
	{
		Camera main = Camera.main;
		if (this.m_MainCameraInternal != main)
		{
			this.m_MainCameraInternal = main;
		}
		this.UpdateZoom();
		this.UpdateUnderShowerEffect();
		if (!GreenHellGame.DEBUG)
		{
			return;
		}
		if ((Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha0)) || (Input.GetKeyDown(InputHelpers.PadButton.R3.KeyFromPad()) && Input.GetKeyDown(InputHelpers.PadButton.L3.KeyFromPad())))
		{
			if (this.m_Mode == CameraManager.Mode.Normal)
			{
				this.m_Mode = CameraManager.Mode.Free;
				Player.Get().BlockMoves();
				Player.Get().BlockRotation();
				return;
			}
			if (this.m_Mode == CameraManager.Mode.Free)
			{
				this.m_Mode = CameraManager.Mode.Tpp;
				Player.Get().UnblockMoves();
				Player.Get().UnblockRotation();
				return;
			}
			this.m_Mode = CameraManager.Mode.Normal;
			Player.Get().UnblockMoves();
			Player.Get().UnblockRotation();
		}
	}

	private void UpdateUnderShowerEffect()
	{
		if (!this.m_UnderShowerEffect)
		{
			return;
		}
		if (Player.Get().IsTakingShower())
		{
			if (this.m_UnderShowerEffect.bump < 0.7f)
			{
				this.m_UnderShowerEffect.bump += Time.deltaTime;
				this.m_UnderShowerEffect.bump = Mathf.Clamp(this.m_UnderShowerEffect.bump, 0f, this.m_MaxUnderShowerEffect);
			}
		}
		else if (this.m_UnderShowerEffect.bump > 0f)
		{
			this.m_UnderShowerEffect.bump -= Time.deltaTime;
			this.m_UnderShowerEffect.bump = Mathf.Clamp(this.m_UnderShowerEffect.bump, 0f, this.m_MaxUnderShowerEffect);
		}
		this.m_UnderShowerEffect.enabled = (this.m_UnderShowerEffect.bump > 0f);
	}

	private void LateUpdate()
	{
		if (MenuInGameManager.Get().IsAnyScreenVisible() || !this.m_MainCamera || !this.m_MainCamera.enabled)
		{
			return;
		}
		switch (this.m_Mode)
		{
		case CameraManager.Mode.Normal:
			this.UpdateNormalMode();
			break;
		case CameraManager.Mode.Free:
			this.UpdateFreeMode();
			break;
		case CameraManager.Mode.CutscenePlayer:
			this.UpdateCutscenePlayerMode();
			break;
		case CameraManager.Mode.Tpp:
			this.UpdateTppMode();
			break;
		}
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().CustomUpdate();
		}
		HUDManager.Get().UpdateAfterCamera();
		if (this.m_OverrideFOV)
		{
			this.OverrideFOV();
		}
		if (this.m_AddRoll)
		{
			this.AddRoll();
		}
	}

	private void OverrideFOV()
	{
		this.m_MainCamera.fieldOfView = this.m_OverideFOVValue;
	}

	private void AddRoll()
	{
		this.m_MainCamera.transform.Rotate(this.m_MainCamera.transform.forward, this.m_AddRollValue, Space.World);
	}

	private void UpdateNormalMode()
	{
		if (this.m_Target == null)
		{
			Debug.Log("[CameraManager:UpdateNormalMode] Target is not set!");
			return;
		}
		if (SwimController.Get().IsActive() && SwimController.Get().m_State == SwimState.Swim)
		{
			this.m_YOffset += (this.m_YSwimOffset - this.m_YOffset) * Time.deltaTime;
		}
		else
		{
			this.m_YOffset += -this.m_YOffset * Time.deltaTime;
		}
		Vector3 vector = (this.m_Target.GetREyeTransform().position + this.m_Target.GetLEyeTransform().position) * 0.5f;
		vector += this.m_Target.GetREyeTransform().up * this.m_YOffset;
		this.m_MainCamera.transform.position = vector;
		this.m_MainCamera.transform.rotation = this.m_Target.GetREyeTransform().rotation;
		if (this.m_TODRaysUpdateEnabled)
		{
			this.UpdateTodRays();
		}
		if (this.m_InterpolateFov && !BodyInspectionController.Get().IsActive())
		{
			this.InterpolateFov();
		}
	}

	private void InterpolateFov()
	{
		if (!this.m_MainCamera)
		{
			return;
		}
		float fov = this.m_FOVInterpolatorObjects[0].m_FOV;
		float b = Player.Get().transform.position.Distance(this.m_FOVInterpolatorObjects[0].transform.position);
		float fov2 = this.m_FOVInterpolatorObjects[1].m_FOV;
		float b2 = Player.Get().transform.position.Distance(this.m_FOVInterpolatorObjects[1].transform.position);
		float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, b, this.m_FOVInterpolatorObjects[0].m_MaxRadius, this.m_FOVInterpolatorObjects[0].m_Radius);
		float proportionalClamp2 = CJTools.Math.GetProportionalClamp(0f, 1f, b2, this.m_FOVInterpolatorObjects[1].m_MaxRadius, this.m_FOVInterpolatorObjects[1].m_Radius);
		float num = proportionalClamp + proportionalClamp2;
		float fieldOfView;
		if (num == 0f)
		{
			fieldOfView = this.m_DefaultFOV + GreenHellGame.Instance.m_Settings.m_FOVChange * GreenHellGame.Instance.m_Settings.m_FOVMaxChange;
		}
		else
		{
			float num2 = this.m_DefaultFOV + GreenHellGame.Instance.m_Settings.m_FOVChange * GreenHellGame.Instance.m_Settings.m_FOVMaxChange;
			fieldOfView = fov * (proportionalClamp / 1f) + fov2 * (proportionalClamp2 / 1f) + num2 * (1f - num);
		}
		this.m_MainCamera.fieldOfView = fieldOfView;
	}

	public void StartFovInterpolator(string i1, string i2)
	{
		FOVObject[] array = Resources.FindObjectsOfTypeAll<FOVObject>();
		FOVObject fovobject = null;
		FOVObject fovobject2 = null;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].name == i1)
			{
				fovobject = array[j];
				break;
			}
		}
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k].name == i2)
			{
				fovobject2 = array[k];
				break;
			}
		}
		DebugUtils.Assert(fovobject && fovobject2, true);
		this.m_FOVInterpolatorObjects[0] = fovobject;
		this.m_FOVInterpolatorObjects[1] = fovobject2;
		fovobject.m_MaxRadius = fovobject.transform.position.Distance(fovobject2.transform.position);
		fovobject2.m_MaxRadius = fovobject.transform.position.Distance(fovobject2.transform.position);
		this.m_InterpolateFov = true;
	}

	public void StopFovInterpolator()
	{
		this.m_FOVInterpolatorObjects[0] = null;
		this.m_FOVInterpolatorObjects[1] = null;
		this.m_InterpolateFov = false;
	}

	private void UpdateTppMode()
	{
		if (this.m_Target == null)
		{
			Debug.Log("[CameraManager:UpdateTppMode] Target is not set!");
			return;
		}
		if (SwimController.Get().IsActive() && SwimController.Get().m_State == SwimState.Swim)
		{
			this.m_YOffset += (this.m_YSwimOffset - this.m_YOffset) * Time.deltaTime;
		}
		else
		{
			this.m_YOffset += -this.m_YOffset * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.PageUp))
		{
			this.m_TppCamDistance += 0.1f;
		}
		if (Input.GetKey(KeyCode.PageDown) && this.m_TppCamDistance > 0f)
		{
			this.m_TppCamDistance -= 0.1f;
		}
		Vector3 vector = this.m_Target.GetREyeTransform().position - this.m_Target.GetLEyeTransform().position;
		vector *= 0.5f;
		vector += this.m_Target.GetLEyeTransform().position;
		vector += this.m_Target.GetREyeTransform().up * this.m_YOffset;
		vector -= this.m_Target.GetREyeTransform().forward * this.m_TppCamDistance;
		this.m_MainCamera.transform.position = vector;
		this.m_MainCamera.transform.rotation = this.m_Target.GetREyeTransform().rotation;
		if (this.m_TODRaysUpdateEnabled)
		{
			this.UpdateTodRays();
		}
	}

	public void EnableTODRaysUpdate(bool set)
	{
		this.m_TODRaysUpdateEnabled = set;
	}

	private void UpdateTodRays()
	{
		if (this.m_TodRays == null)
		{
			this.m_TodRays = this.m_MainCamera.GetComponent<TOD_Rays>();
			DebugUtils.Assert(this.m_TodRays, true);
		}
		if (Player.Get().IsCameraUnderwater())
		{
			this.m_TodRays.Intensity = this.m_TodRaysIntensityUnderwater;
			return;
		}
		this.m_TodRays.Intensity = this.m_TodRaysIntensityDefault;
	}

	private void UpdateCutscenePlayerMode()
	{
		Vector3 position = this.m_Target.GetCamTransform().position;
		this.m_MainCamera.transform.position = position;
		this.m_MainCamera.transform.rotation = this.m_Target.GetCamTransform().rotation;
	}

	private void UpdateFreeMode()
	{
		this.m_MainCamera.transform.position += this.m_MainCamera.transform.forward * this.m_FreeCamSpeed * Time.deltaTime * InputsManager.Get().GetActionValue(InputsManager.InputAction.Forward);
		this.m_MainCamera.transform.position -= this.m_MainCamera.transform.forward * this.m_FreeCamSpeed * Time.deltaTime * InputsManager.Get().GetActionValue(InputsManager.InputAction.Backward);
		this.m_MainCamera.transform.position -= this.m_MainCamera.transform.right * this.m_FreeCamSpeed * Time.deltaTime * InputsManager.Get().GetActionValue(InputsManager.InputAction.Left);
		this.m_MainCamera.transform.position += this.m_MainCamera.transform.right * this.m_FreeCamSpeed * Time.deltaTime * InputsManager.Get().GetActionValue(InputsManager.InputAction.Right);
		this.m_MainCamera.transform.position += this.m_MainCamera.transform.forward * Input.GetAxis("Mouse ScrollWheel") * this.m_FreeCamSpeed * this.m_FreeCamWheelSpeedMul * Time.deltaTime;
		Vector2 vector = new Vector2(InputsManager.Get().GetActionValue(InputsManager.InputAction.LookUp) - InputsManager.Get().GetActionValue(InputsManager.InputAction.LookDown), InputsManager.Get().GetActionValue(InputsManager.InputAction.LookRight) - InputsManager.Get().GetActionValue(InputsManager.InputAction.LookLeft));
		if (GreenHellGame.Instance.m_Settings.m_ControllerType == ControllerType.Pad)
		{
			float magnitude = vector.magnitude;
			if (magnitude > 1f)
			{
				vector /= magnitude;
			}
			vector *= Mathf.Pow(vector.magnitude, 2f);
		}
		this.m_FreeCamRot.x = this.m_FreeCamRot.x + vector.x * this.m_FreeCamRotSensitivityY * Time.deltaTime * 50f;
		this.m_FreeCamRot.y = this.m_FreeCamRot.y + vector.y * this.m_FreeCamRotSensitivityX * Time.deltaTime * 50f;
		this.m_MainCamera.transform.localEulerAngles = new Vector3(-this.m_FreeCamRot.x, this.m_FreeCamRot.y, 0f);
		if (Input.GetKey(KeyCode.PageUp))
		{
			this.m_FreeCamSpeed += 1f;
		}
		if (Input.GetKey(KeyCode.PageDown))
		{
			this.m_FreeCamSpeed -= 1f;
			if (this.m_FreeCamSpeed < 0f)
			{
				this.m_FreeCamSpeed = 0f;
			}
		}
		RaycastHit raycastHit;
		if (Input.GetKey(KeyCode.P) && Physics.Raycast(this.m_MainCamera.transform.position, -Vector3.up, out raycastHit))
		{
			Player.Get().Reposition(raycastHit.point, null);
		}
	}

	public void SetZoom(float zoom)
	{
		this.m_WantedZoom = Mathf.Clamp01(zoom);
	}

	private void UpdateZoom()
	{
		if (this.m_Zoom == this.m_WantedZoom)
		{
			return;
		}
		this.m_Zoom += (this.m_WantedZoom - this.m_Zoom) * Time.deltaTime;
		if (this.m_MainCamera != null)
		{
			float a = this.m_DefaultFOV + GreenHellGame.Instance.m_Settings.m_FOVChange * GreenHellGame.Instance.m_Settings.m_FOVMaxChange;
			this.m_MainCamera.fieldOfView = CJTools.Math.GetProportionalClamp(a, this.m_MaxZoomFOV, this.m_Zoom, 0f, 1f);
		}
	}

	public float GetDefaultFOV()
	{
		return this.m_DefaultFOV;
	}

	public void ScenarioSetFOV(float fov)
	{
		this.m_MainCamera.fieldOfView = fov;
		Camera[] componentsInChildren = this.m_MainCamera.GetComponentsInChildren<Camera>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].fieldOfView = fov;
		}
	}

	public void ScenarioResetFOV()
	{
		this.m_MainCamera.fieldOfView = this.m_DefaultFOV;
		Camera[] componentsInChildren = this.m_MainCamera.GetComponentsInChildren<Camera>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].fieldOfView = this.m_DefaultFOV;
		}
	}

	public CameraManager.Mode m_Mode;

	private Being m_Target;

	public float m_FreeCamSpeed;

	[HideInInspector]
	public float m_TppCamDistance = 1f;

	public float m_FreeCamWheelSpeedMul = 1f;

	public float m_FreeCamRotSensitivityX;

	public float m_FreeCamRotSensitivityY;

	private Vector2 m_FreeCamRot = Vector2.zero;

	private static CameraManager s_Instance;

	private Camera m_MainCameraInternal;

	private float m_Zoom;

	private float m_WantedZoom;

	private float m_MaxZoomFOV = 35f;

	private float m_DefaultFOV;

	private WaterWarpImageEffect m_UnderShowerEffect;

	public float m_MaxUnderShowerEffect = 0.7f;

	[HideInInspector]
	public bool m_OverrideFOV;

	public float m_OverideFOVValue = 75f;

	[HideInInspector]
	public bool m_AddRoll;

	public float m_AddRollValue;

	private float m_YOffset;

	private float m_YSwimOffset = 0.1f;

	private bool m_InterpolateFov;

	private FOVObject[] m_FOVInterpolatorObjects = new FOVObject[2];

	private TOD_Rays m_TodRays;

	private float m_TodRaysIntensityDefault = 0.2f;

	private float m_TodRaysIntensityUnderwater = 0.05f;

	private bool m_TODRaysUpdateEnabled = true;

	public enum Mode
	{
		Normal,
		Free,
		CutscenePlayer,
		Tpp
	}
}
