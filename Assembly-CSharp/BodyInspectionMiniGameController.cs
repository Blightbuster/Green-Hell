using System;
using CJTools;
using Enums;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class BodyInspectionMiniGameController : PlayerController
{
	public static BodyInspectionMiniGameController Get()
	{
		return BodyInspectionMiniGameController.s_Instance;
	}

	protected override void Awake()
	{
		BodyInspectionMiniGameController.s_Instance = this;
	}

	private void CreateWorm()
	{
		GameObject prefab = GreenHellGame.Instance.GetPrefab("botfly");
		if (!prefab)
		{
			DebugUtils.Assert("[ConstructionGhostManager:Load] ERROR - Can't load prefab - " + base.name, true, DebugUtils.AssertType.Info);
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
		gameObject.name = prefab.name;
		this.m_Worm = gameObject;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeOut(FadeType.All, new VDelegate(this.StartMinigame), 1.5f, null);
		this.m_LHAnimator.SetInteger(this.m_IBIMinigameState, 1);
		this.m_LHAnimator.SetFloat(this.m_FBIMinigameBlend, 0.5f);
		this.m_LHAnimator.speed = 0f;
		this.m_LHAnimator.Play(this.m_LHBlendTreeState, 0, 0f);
		this.m_RHAnimator.SetInteger(this.m_IBIMinigameState, 1);
		this.m_RHAnimator.speed = 0f;
		this.m_RHAnimator.Play(this.m_RHState, 0, 0f);
		this.m_State = EBIMState.Rotation;
		this.m_CorrectAngle = UnityEngine.Random.Range(-30f, 30f);
		this.m_Worm.SetActive(true);
		this.m_CurrentWormState = 0;
		Inventory3DManager.Get().Deactivate();
	}

	protected override void OnDisable()
	{
		base.OnEnable();
		this.ResetCamera();
		this.m_State = EBIMState.None;
		this.m_Worm.SetActive(false);
	}

	private void SetCamera()
	{
		this.m_MainCamera.enabled = false;
		this.m_Camera.gameObject.SetActive(true);
		this.m_Camera.enabled = true;
	}

	private void ResetCamera()
	{
		if (this.m_MainCamera != null)
		{
			this.m_MainCamera.enabled = true;
		}
		if (this.m_Camera != null)
		{
			this.m_Camera.gameObject.SetActive(false);
			this.m_Camera.enabled = false;
		}
	}

	private void StartMinigame()
	{
		this.SetCamera();
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeIn(FadeType.All, null, 1.5f);
		this.m_SkyDome.gameObject.SetActive(false);
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdatInputs();
		this.UpdateCorrectAngle();
		this.UpdateResult();
		this.UpdateWorm();
		this.UpdateAnimationAndState();
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		this.SetRHandTransform();
	}

	public override void OnInputAction(InputsManager.InputAction action)
	{
		if ((action == InputsManager.InputAction.Quit || action == InputsManager.InputAction.AdditionalQuit) && GreenHellGame.GetFadeSystem().CanStartFade())
		{
			FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
			fadeSystem.FadeOut(FadeType.All, new VDelegate(this.StopMinigame), 1.5f, null);
		}
	}

	private void UpdateAnimationAndState()
	{
		if (this.m_LMB && !this.m_LastLMB && this.m_State == EBIMState.Rotation)
		{
			this.m_State = EBIMState.SqueezeForward;
		}
		if (this.m_State == EBIMState.Rotation)
		{
			this.m_LHAnimator.speed = 0f;
			this.m_LHAnimator.Play(this.m_LHBlendTreeState, 0, 0f);
			this.m_RHAnimator.speed = 0f;
			this.m_RHAnimator.Play(this.m_RHState, 0, 0f);
			this.m_AnimationFrame = 0f;
			this.m_RHAnimator.Play(this.m_RHState, 0, this.m_AnimationFrame);
			float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, this.m_MouseAxisX, -30f, 30f);
			this.m_LHAnimator.SetFloat(this.m_FBIMinigameBlend, proportionalClamp);
		}
		else if (this.m_State == EBIMState.SqueezeForward)
		{
			this.m_LHAnimator.speed = 1f;
			this.m_AnimationFrame = this.m_LHAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
			if (this.m_AnimationFrame > 1f)
			{
				this.m_AnimationFrame = 1f;
				this.m_State = EBIMState.Rotation;
				if (this.m_Result > 0.1f)
				{
					this.m_CurrentWormState++;
					if (this.m_CurrentWormState == 3)
					{
						this.m_State = EBIMState.PrePresentation;
					}
				}
				else
				{
					PlayerSanityModule.Get().OnEvent(PlayerSanityModule.SanityEventType.WormMinigameFail, 1);
				}
			}
			this.m_RHAnimator.Play(this.m_RHState, 0, this.m_AnimationFrame);
		}
		else if (this.m_State == EBIMState.PrePresentation)
		{
			float num = this.m_LHAnimator.GetFloat(this.m_FBIMinigameBlend);
			num += (0.5f - num) * Time.deltaTime * 4f;
			if (Mathf.Abs(num - 0.5f) < 0.05f)
			{
				num = 0.5f;
				this.m_RHAnimator.speed = 1f;
				this.m_RHAnimator.SetInteger(this.m_IBIMinigameState, 2);
				this.m_LHAnimator.speed = 1f;
				this.m_LHAnimator.SetInteger(this.m_IBIMinigameState, 2);
				this.m_State = EBIMState.Presentation;
			}
			this.m_MouseAxisX = CJTools.Math.GetProportionalClamp(-30f, 30f, num, 0f, 1f);
			this.m_LHAnimator.SetFloat(this.m_FBIMinigameBlend, num);
		}
		else if (this.m_State == EBIMState.Presentation)
		{
			AnimatorStateInfo currentAnimatorStateInfo = this.m_RHAnimator.GetCurrentAnimatorStateInfo(0);
			if (currentAnimatorStateInfo.shortNameHash == this.m_GrabWormState && currentAnimatorStateInfo.normalizedTime > 0.2f && this.m_Worm.transform.parent == null)
			{
				Transform parent = this.m_RH.transform.FindDeepChild("HandIndex4.R");
				this.m_Worm.transform.parent = parent;
			}
			if (currentAnimatorStateInfo.shortNameHash == this.m_GrabWormState && currentAnimatorStateInfo.normalizedTime > 0.99f)
			{
				this.m_Player.GetComponent<PlayerInjuryModule>().HealInjury(this.m_Injury);
				this.m_Injury = null;
				FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
				fadeSystem.FadeOut(FadeType.All, new VDelegate(this.StopMinigame), 1.5f, null);
				this.m_State = EBIMState.Leaving;
				PlayerSanityModule.Get().OnEvent(PlayerSanityModule.SanityEventType.WormMinigameSuccess, 1);
			}
		}
		this.m_LastLMB = this.m_LMB;
	}

	private void StopMinigame()
	{
		this.m_SkyDome.gameObject.SetActive(true);
		this.ResetCamera();
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeIn(FadeType.All, null, 1.5f);
		this.m_Player.SetWantedItem(Hand.Left, null, true);
		this.m_Player.SetWantedItem(Hand.Right, null, true);
		this.Stop();
	}

	private void SetRHandTransform()
	{
		this.m_RhandTrans.rotation = this.m_RHStartRotation;
		Quaternion localRotation = this.m_RhandTrans.localRotation;
		Quaternion rhs = Quaternion.Inverse(localRotation);
		this.m_RH.transform.rotation = this.m_LhandTrans.rotation * rhs;
		Vector3 b = this.m_RH.transform.position - this.m_RhandTrans.position;
		this.m_RH.transform.position = this.m_LhandTrans.position;
		this.m_RH.transform.position += b;
		this.m_RhandTrans.Rotate(this.m_RhandTrans.right, this.m_MouseAxisX, Space.World);
	}

	private void UpdatInputs()
	{
		if (this.m_State == EBIMState.Rotation)
		{
			this.m_MouseAxisX -= CrossPlatformInputManager.GetAxis("Mouse X") * this.m_MouseSensitivityX * Time.deltaTime;
			if (this.m_MouseAxisX < -30f)
			{
				this.m_MouseAxisX = -30f;
			}
			if (this.m_MouseAxisX > 30f)
			{
				this.m_MouseAxisX = 30f;
			}
			this.m_LMB = InputsManager.Get().IsActionActive(InputsManager.InputAction.LMB);
		}
	}

	private void UpdateCorrectAngle()
	{
		if (this.m_State != EBIMState.Rotation)
		{
			return;
		}
		this.m_CorrectAngle += this.m_AngleChangeSpeed * this.m_AngleChangeDirection * Time.deltaTime;
		if (UnityEngine.Random.Range(0f, 1f) < 0.05f)
		{
			this.m_AngleChangeDirection *= -1f;
		}
		if (this.m_CorrectAngle < -30f)
		{
			this.m_CorrectAngle = -30f;
			this.m_AngleChangeDirection = 1f;
		}
		if (this.m_CorrectAngle > 30f)
		{
			this.m_CorrectAngle = 30f;
			this.m_AngleChangeDirection = -1f;
		}
	}

	private void UpdateResult()
	{
		this.m_Result = 1f - Mathf.Abs(this.m_MouseAxisX - this.m_CorrectAngle) / this.m_CorrectAngleGap;
		this.m_Result = Mathf.Clamp01(this.m_Result);
	}

	private void UpdateWorm()
	{
		if (this.m_State < EBIMState.Presentation)
		{
			this.m_Worm.transform.rotation = this.m_WormHookTrans.rotation;
			this.m_Worm.transform.Rotate(this.m_Worm.transform.up, 180f, Space.World);
			Vector3 vector = Vector3.zero;
			vector = this.m_WormHookTrans.right * this.m_WormStartOffset + (this.m_WormHookTrans.right * this.m_WormEndOffset - this.m_WormHookTrans.right * this.m_WormStartOffset) * ((float)this.m_CurrentWormState / 3f);
			float num = 0.7f;
			AnimatorStateInfo currentAnimatorStateInfo = this.m_LHAnimator.GetCurrentAnimatorStateInfo(0);
			if (this.m_State == EBIMState.SqueezeForward && this.m_Result > 0.1f && currentAnimatorStateInfo.normalizedTime > num)
			{
				float num2 = currentAnimatorStateInfo.normalizedTime - num;
				float num3 = 1f / (1f - num);
				vector += this.m_WormHookTrans.right * (num2 * num3) * ((this.m_WormEndOffset - this.m_WormStartOffset) / 3f);
			}
			this.m_Worm.transform.position = this.m_WormHookTrans.position + vector;
			if (this.m_Result > 0.1f)
			{
				this.m_WormAnimator.speed = 0f;
			}
			else
			{
				this.m_WormAnimator.speed = 1f;
			}
		}
	}

	private static BodyInspectionMiniGameController s_Instance;

	public Camera m_Camera;

	private Camera m_MainCamera;

	public GameObject m_SkyDome;

	public GameObject m_LH;

	public GameObject m_RH;

	public int m_IBIMinigameState = Animator.StringToHash("BodyInspectionMinigame");

	public int m_FBIMinigameBlend = Animator.StringToHash("Blend");

	public int m_LHBlendTreeState = Animator.StringToHash("BIMGBlendTree");

	public int m_RHState = Animator.StringToHash("Squeeze");

	private int m_GrabWormState = Animator.StringToHash("GrabWorm");

	private Animator m_LHAnimator;

	private Animator m_RHAnimator;

	private float m_MouseAxisX;

	private bool m_LMB;

	private bool m_LastLMB;

	private float m_RHRotation;

	private float m_MouseSensitivityX = 30f;

	private Quaternion m_RHStartRotation;

	private EBIMState m_State;

	private float m_AnimationFrame;

	private float m_CorrectAngle;

	private float m_CorrectAngleGap = 10f;

	private float m_AngleChangeSpeed = 20f;

	[HideInInspector]
	public float m_AngleChangeDirection = 1f;

	[HideInInspector]
	public float m_Result;

	private GameObject m_Worm;

	public float m_WormStartOffset = -0.012f;

	public float m_WormEndOffset = -0.004f;

	private const int m_NumWormStages = 3;

	private int m_CurrentWormState;

	private Transform m_RhandTrans;

	private Transform m_LhandTrans;

	private Transform m_WormHookTrans;

	private Animator m_WormAnimator;

	public Injury m_Injury;
}
