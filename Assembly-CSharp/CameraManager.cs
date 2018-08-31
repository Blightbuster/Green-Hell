using System;
using CJTools;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
	public static CameraManager Get()
	{
		return CameraManager.s_Instance;
	}

	private void Awake()
	{
		CameraManager.s_Instance = this;
		this.m_DefaultFOV = Camera.main.fieldOfView;
	}

	public void SetTarget(Being target)
	{
		this.m_Target = target;
		if (this.m_Target.IsPlayer())
		{
			Player.Get().UnblockMoves();
			Player.Get().UnblockRotation();
		}
		else
		{
			Player.Get().BlockMoves();
			Player.Get().BlockRotation();
		}
	}

	public void SetMode(CameraManager.Mode mode)
	{
		this.m_Mode = mode;
	}

	private void Update()
	{
		this.UpdateZoom();
		if (!Debug.isDebugBuild)
		{
			return;
		}
		if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha0))
		{
			if (this.m_Mode == CameraManager.Mode.Normal)
			{
				this.m_Mode = CameraManager.Mode.Free;
				Player.Get().BlockMoves();
				Player.Get().BlockRotation();
			}
			else
			{
				this.m_Mode = CameraManager.Mode.Normal;
				Player.Get().UnblockMoves();
				Player.Get().UnblockRotation();
			}
		}
	}

	private void LateUpdate()
	{
		if (MenuInGameManager.Get().IsAnyScreenVisible() || !this.m_MainCamera.enabled)
		{
			return;
		}
		CameraManager.Mode mode = this.m_Mode;
		if (mode != CameraManager.Mode.Normal)
		{
			if (mode != CameraManager.Mode.Free)
			{
				if (mode == CameraManager.Mode.CutscenePlayer)
				{
					this.UpdateCutscenePlayerMode();
				}
			}
			else
			{
				this.UpdateFreeMode();
			}
		}
		else
		{
			this.UpdateNormalMode();
		}
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().CustomUpdate();
		}
		HUDManager.Get().UpdateAfterCamera();
	}

	private void UpdateNormalMode()
	{
		if (this.m_Target == null)
		{
			Debug.Log("[CameraManager:UpdateNormalMode] Target is not set!");
			return;
		}
		Vector3 vector = this.m_Target.GetREyeTransform().position - this.m_Target.GetLEyeTransform().position;
		vector *= 0.5f;
		vector += this.m_Target.GetLEyeTransform().position;
		this.m_MainCamera.transform.position = vector;
		this.m_MainCamera.transform.rotation = this.m_Target.GetREyeTransform().rotation;
	}

	private void UpdateCutscenePlayerMode()
	{
		Vector3 position = this.m_Target.GetCamTransform().position;
		this.m_MainCamera.transform.position = position;
		this.m_MainCamera.transform.rotation = this.m_Target.GetCamTransform().rotation;
	}

	private void UpdateFreeMode()
	{
		if (Input.GetKey(KeyCode.W))
		{
			this.m_MainCamera.transform.position += this.m_MainCamera.transform.forward * this.m_FreeCamSpeed * Time.deltaTime;
		}
		else if (Input.GetKey(KeyCode.S))
		{
			this.m_MainCamera.transform.position -= this.m_MainCamera.transform.forward * this.m_FreeCamSpeed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.A))
		{
			this.m_MainCamera.transform.position -= this.m_MainCamera.transform.right * this.m_FreeCamSpeed * Time.deltaTime;
		}
		else if (Input.GetKey(KeyCode.D))
		{
			this.m_MainCamera.transform.position += this.m_MainCamera.transform.right * this.m_FreeCamSpeed * Time.deltaTime;
		}
		this.m_MainCamera.transform.position += this.m_MainCamera.transform.forward * Input.GetAxis("Mouse ScrollWheel") * this.m_FreeCamSpeed * this.m_FreeCamWheelSpeedMul * Time.deltaTime;
		this.m_FreeCamRot.x = this.m_FreeCamRot.x + Input.GetAxis("Mouse Y") * this.m_FreeCamRotSensitivityX;
		this.m_FreeCamRot.y = this.m_FreeCamRot.y + Input.GetAxis("Mouse X") * this.m_FreeCamRotSensitivityY;
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
			Player.Get().transform.position = raycastHit.point;
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
		if (Camera.main != null)
		{
			Camera.main.fieldOfView = CJTools.Math.GetProportionalClamp(this.m_DefaultFOV, this.m_MaxZoomFOV, this.m_Zoom, 0f, 1f);
		}
	}

	public CameraManager.Mode m_Mode;

	private Being m_Target;

	public float m_FreeCamSpeed;

	public float m_FreeCamWheelSpeedMul = 1f;

	public float m_FreeCamRotSensitivityX;

	public float m_FreeCamRotSensitivityY;

	private Vector2 m_FreeCamRot = Vector2.zero;

	private static CameraManager s_Instance;

	public Camera m_MainCamera;

	private float m_Zoom;

	private float m_WantedZoom;

	private float m_MaxZoomFOV = 35f;

	private float m_DefaultFOV;

	public enum Mode
	{
		Normal,
		Free,
		CutscenePlayer
	}
}
