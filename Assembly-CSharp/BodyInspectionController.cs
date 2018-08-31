using System;
using System.Collections;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class BodyInspectionController : PlayerController
{
	public static BodyInspectionController Get()
	{
		return BodyInspectionController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		BodyInspectionController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.BodyInspection;
		this.SetWoundSlots();
		this.InitializeMaggotsVertices();
		this.InitializeAntsVertices();
		this.m_StartFOV = Camera.main.fieldOfView;
		Transform transform = Camera.main.transform.FindDeepChild("OutlineCamera");
		this.m_OutlineCamera = transform.GetComponent<Camera>();
		this.SetupAudio();
	}

	private void SetupAudio()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		AudioClip item = (AudioClip)Resources.Load("Sounds/Player/arm_bandaging_01");
		this.m_ArmBandagingSound.Add(item);
	}

	protected new virtual void Start()
	{
		base.Start();
		this.m_LeftLegRotationBones.Clear();
		this.m_LeftLegRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:UpLeg.L"));
		this.m_LeftLegRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:Leg.L"));
		this.m_LeftLegRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:Foot.L"));
		this.m_RightLegRotationBones.Clear();
		this.m_RightLegRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:UpLeg.R"));
		this.m_RightLegRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:Leg.R"));
		this.m_RightLegRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:Foot.R"));
		this.m_LeftArmRotationBones.Clear();
		this.m_LeftArmRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:Arm.L"));
		this.m_LeftArmRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:ForeArm.L"));
		this.m_LeftArmRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:Hand.L"));
		this.m_RightArmRotationBones.Clear();
		this.m_RightArmRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:Arm.R"));
		this.m_RightArmRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:ForeArm.R"));
		this.m_RightArmRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:Hand.R"));
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Player.BlockMoves();
		this.m_Player.BlockRotation();
		this.SetState(BIState.Starting);
		this.m_Inputs.Reset();
		this.m_Animator.SetBool(this.m_BBodyInspection, true);
		this.m_SelectedWound = null;
		this.m_AnimatorX = -1f;
		this.m_AnimatorY = 1f;
		EventsManager.OnEvent(Enums.Event.EnterInspection, 1);
		CursorManager.Get().ShowCursor(true);
		this.m_DelayAnimatorReset = false;
		this.m_LeaveAfterBandage = false;
		this.m_SpineAddLayerBlendWeightToRestore = this.m_Animator.GetLayerWeight(4);
		this.m_Animator.SetLayerWeight(4, 1f);
		Watch.Get().gameObject.SetActive(true);
		HintsManager.Get().ShowHint("Inspection", 10f);
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			HintsManager.Get().ShowHint("Inspection_Backpack", 10f);
		}
		this.m_ControllerEnabled = true;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Player.UnblockMoves();
		this.m_Player.UnblockRotation();
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		if (this.m_Animator.isInitialized)
		{
			if (this.m_DelayAnimatorReset)
			{
				base.StartCoroutine(BodyInspectionController.DelayedResetAnimator(this.m_Animator, 0.2f, Time.time, this.m_BBodyInspection));
			}
			else
			{
				this.m_Animator.SetBool(this.m_BBodyInspection, false);
			}
		}
		CursorManager.Get().ShowCursor(false);
		this.m_ActiveSlot = null;
		if (Camera.main && Camera.main.gameObject.activeSelf)
		{
			Camera.main.fieldOfView = this.m_StartFOV;
			this.m_OutlineCamera.fieldOfView = this.m_StartFOV;
		}
		this.m_LeaveAfterBandage = false;
		this.m_Animator.SetLayerWeight(4, this.m_SpineAddLayerBlendWeightToRestore);
		if (Watch.Get() != null)
		{
			Watch.Get().gameObject.SetActive(false);
		}
		this.m_Animator.SetBool(this.m_BBI_BandageLeftHand, false);
		this.m_Animator.SetBool(this.m_BBI_BandageRightHand, false);
		this.m_Animator.SetBool(this.m_BBI_BandageLeftLeg, false);
		this.m_Animator.SetBool(this.m_BBI_BandageRightLeg, false);
		this.m_HideAndShowLimb = false;
		this.m_HideAndShowProgress = 0f;
		this.m_Animator.SetBool("Inventory", false);
		this.m_Animator.SetFloat(this.m_FBI_Show_Hand, 0f);
		this.m_Animator.SetFloat(this.m_FBI_Show_Leg, 0f);
		this.m_ControllerEnabled = false;
	}

	public static IEnumerator DelayedResetAnimator(Animator animator, float delay, float start_time, int hash)
	{
		while (Time.time - start_time < delay)
		{
			yield return null;
		}
		animator.SetBool(hash, false);
		yield break;
	}

	private void SetState(BIState state)
	{
		if (this.m_State == state)
		{
			return;
		}
		this.m_State = state;
		this.OnSetState();
	}

	private void OnSetState()
	{
		bool flag = false;
		int injuriesCount = PlayerInjuryModule.Get().GetInjuriesCount();
		for (int i = 0; i < injuriesCount; i++)
		{
			Injury injury = PlayerInjuryModule.Get().GetInjury(i);
			if (this.m_State == BIState.RotateLeftArm)
			{
				if (injury.GetInjuryPlace() == InjuryPlace.LHand)
				{
					flag = true;
					break;
				}
			}
			else if (this.m_State == BIState.RotateRightArm)
			{
				if (injury.GetInjuryPlace() == InjuryPlace.RHand)
				{
					flag = true;
					break;
				}
			}
			else if (this.m_State == BIState.RotateLeftLeg)
			{
				if (injury.GetInjuryPlace() == InjuryPlace.LLeg)
				{
					flag = true;
					break;
				}
			}
			else if (this.m_State == BIState.RotateRightLeg && injury.GetInjuryPlace() == InjuryPlace.RLeg)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			PlayerSanityModule.Get().OnWhispersEvent(PlayerSanityModule.WhisperType.Inspection);
		}
	}

	public override void ControllerUpdate()
	{
		if (this.m_State > BIState.Starting)
		{
			this.UpdateInput();
		}
		if (this.m_State == BIState.Starting)
		{
			this.UpdateStarting();
		}
		else if (this.m_State == BIState.ChooseLimb)
		{
			this.UpdateChooseLimb();
		}
		else if (this.m_State == BIState.RotateLeftArm)
		{
			this.UpdateRotateLeftHand();
		}
		else if (this.m_State == BIState.RotateRightArm)
		{
			this.UpdateRotateRightLeg();
		}
		else if (this.m_State == BIState.RotateLeftLeg)
		{
			this.UpdateRotateLeftHand();
		}
		else if (this.m_State == BIState.RotateRightLeg)
		{
			this.UpdateRotateRightLeg();
		}
		else if (this.m_State == BIState.BandageLeftHandBegin || this.m_State == BIState.BandageRightHandBegin || this.m_State == BIState.BandageLeftLegBegin || this.m_State == BIState.BandageRightLegBegin)
		{
			this.UpdateLimbBandageBegin();
		}
		else if (this.m_State == BIState.Leaving)
		{
			this.UpdateLeaving();
		}
		this.UpdateBlend();
		this.UpdateBodyRotation();
		this.UpdateSelectedWound();
		this.UpdateDeleech();
		this.UpdateFOV();
		this.UpdateCursor();
	}

	private void UpdateStarting()
	{
		Vector2 lookDev = this.m_Player.m_FPPController.GetLookDev();
		float num = lookDev.y * 6f * Time.deltaTime;
		lookDev.y -= num;
		this.m_Player.m_FPPController.SetLookDev(lookDev);
		if (Mathf.Abs(lookDev.y) < 1f)
		{
			this.SetState(BIState.ChooseLimb);
		}
	}

	private void UpdateChooseLimb()
	{
		if (this.m_Inputs.m_RotateLimb)
		{
			if (this.m_AnimatorX < -0.98f && this.m_AnimatorY > 0.98f)
			{
				this.SetState(BIState.RotateLeftArm);
			}
			else if (this.m_AnimatorX > 0.98f && this.m_AnimatorY > 0.98f)
			{
				this.SetState(BIState.RotateRightArm);
			}
			else if (this.m_AnimatorX < -0.98f && this.m_AnimatorY < -0.98f)
			{
				this.SetState(BIState.RotateLeftLeg);
			}
			else if (this.m_AnimatorX > 0.98f && this.m_AnimatorY < -0.98f)
			{
				this.SetState(BIState.RotateRightLeg);
			}
		}
	}

	public bool ScenarioLookingAt(string limb)
	{
		if (limb != null)
		{
			if (limb == "LLeg")
			{
				return this.m_State == BIState.RotateLeftLeg;
			}
			if (limb == "RLeg")
			{
				return this.m_State == BIState.RotateRightLeg;
			}
			if (limb == "LHand")
			{
				return this.m_State == BIState.RotateLeftArm;
			}
			if (limb == "RHand")
			{
				return this.m_State == BIState.RotateRightArm;
			}
		}
		return false;
	}

	public bool ScenarioIsLookingAtInjury(string injury_type)
	{
		InjuryType type = (InjuryType)Enum.Parse(typeof(InjuryType), injury_type);
		List<Injury> allInjuries = PlayerInjuryModule.Get().GetAllInjuries(type);
		if (allInjuries.Count == 0)
		{
			return false;
		}
		foreach (Injury injury in allInjuries)
		{
			if (injury.GetInjuryPlace() == InjuryPlace.LHand && this.m_State == BIState.RotateLeftArm)
			{
				return true;
			}
			if (injury.GetInjuryPlace() == InjuryPlace.RHand && this.m_State == BIState.RotateRightArm)
			{
				return true;
			}
			if (injury.GetInjuryPlace() == InjuryPlace.LLeg && this.m_State == BIState.RotateLeftLeg)
			{
				return true;
			}
			if (injury.GetInjuryPlace() == InjuryPlace.RLeg && this.m_State == BIState.RotateRightLeg)
			{
				return true;
			}
		}
		return false;
	}

	public bool ScenarioIsLookingAtInjuryState(string injury_state)
	{
		InjuryState state = (InjuryState)Enum.Parse(typeof(InjuryState), injury_state);
		List<Injury> allInjuriesOfState = PlayerInjuryModule.Get().GetAllInjuriesOfState(state);
		if (allInjuriesOfState.Count == 0)
		{
			return false;
		}
		foreach (Injury injury in allInjuriesOfState)
		{
			if (injury.GetInjuryPlace() == InjuryPlace.LHand && this.m_State == BIState.RotateLeftArm)
			{
				return true;
			}
			if (injury.GetInjuryPlace() == InjuryPlace.RHand && this.m_State == BIState.RotateRightArm)
			{
				return true;
			}
			if (injury.GetInjuryPlace() == InjuryPlace.LLeg && this.m_State == BIState.RotateLeftLeg)
			{
				return true;
			}
			if (injury.GetInjuryPlace() == InjuryPlace.RLeg && this.m_State == BIState.RotateRightLeg)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateRotateLeftHand()
	{
		if (!this.m_Inputs.m_RotateLimb)
		{
			this.SetState(BIState.ChooseLimb);
			return;
		}
	}

	private void UpdateRotateRightHand()
	{
		if (!this.m_Inputs.m_RotateLimb)
		{
			this.SetState(BIState.ChooseLimb);
			return;
		}
	}

	private void UpdateRotateLeftLeg()
	{
		if (!this.m_Inputs.m_RotateLimb)
		{
			this.SetState(BIState.ChooseLimb);
			return;
		}
	}

	private void UpdateRotateRightLeg()
	{
		if (!this.m_Inputs.m_RotateLimb)
		{
			this.SetState(BIState.ChooseLimb);
			return;
		}
	}

	private void UpdateLeaving()
	{
		if (Mathf.Abs(this.m_AnimatorX) < 0.05f && Mathf.Abs(1f - this.m_AnimatorY) < 0.05f && Mathf.Abs(this.m_Inputs.m_LimbRotation) > 0.45f && Mathf.Abs(this.m_Inputs.m_LimbRotation) < 0.55f && Mathf.Abs(Camera.main.fieldOfView - this.m_StartFOV) < 0.05f)
		{
			this.Stop();
		}
	}

	private void UpdateBlend()
	{
		float num = 0f;
		float num2 = 0f;
		if (this.m_State < BIState.Leaving)
		{
			num = ((this.m_Inputs.m_ChooseLimbX < 0f) ? -1f : 1f);
			num2 = ((this.m_Inputs.m_ChooseLimbY < 0f) ? -1f : 1f);
		}
		else if (this.m_State == BIState.Leaving)
		{
			num2 = 1f;
		}
		if (this.m_HideAndShowLimb)
		{
			float num3 = 1f - Mathf.Sin(this.m_HideAndShowProgress * 3.14159274f);
			num *= num3;
			num2 *= num3;
			this.m_HideAndShowProgress += Time.deltaTime * 1f;
			if (this.m_HideAndShowProgress > 1f)
			{
				this.m_HideAndShowLimb = false;
				this.m_HideAndShowProgress = 0f;
			}
		}
		float num4 = this.m_AnimatorX + (num - this.m_AnimatorX) * Time.deltaTime * 6f;
		float num5 = this.m_AnimatorY + (num2 - this.m_AnimatorY) * Time.deltaTime * 6f;
		this.m_Animator.SetFloat(this.m_FBI_Show_Hand, num4);
		this.m_Animator.SetFloat(this.m_FBI_Show_Leg, num5);
		this.m_AnimatorX = num4;
		this.m_AnimatorY = num5;
	}

	private void UpdateBodyRotation()
	{
		this.UpdateStateTime();
	}

	private void UpdateStateTime()
	{
		AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(0);
		if (this.m_ControllerEnabled && currentAnimatorStateInfo.shortNameHash == this.m_BodyInspectionStateHash && this.m_State != BIState.BandageLeftHand && this.m_State != BIState.BandageRightHand && this.m_State != BIState.BandageLeftLeg && this.m_State != BIState.BandageRightLeg)
		{
			this.m_Animator.Play(currentAnimatorStateInfo.fullPathHash, 0, this.m_Inputs.m_LimbRotation);
		}
	}

	private void UpdateInput()
	{
		this.m_Inputs.m_SelectItem = InputsManager.Get().IsActionActive(InputsManager.InputAction.BISelectItem);
		this.m_Inputs.m_RotateLimb = InputsManager.Get().IsActionActive(InputsManager.InputAction.BIRotateLimb);
		this.m_Inputs.m_SelectLimb = InputsManager.Get().IsActionActive(InputsManager.InputAction.BISelectLimb);
		if (this.m_State != BIState.ChooseLimb)
		{
			if (this.m_State == BIState.RotateLeftArm || this.m_State == BIState.RotateRightArm || this.m_State == BIState.RotateLeftLeg || this.m_State == BIState.RotateRightLeg)
			{
				float num = 1f;
				if (this.m_State == BIState.RotateLeftArm || this.m_State == BIState.RotateLeftLeg)
				{
					num = -1f;
				}
				this.m_Inputs.m_LimbRotation += CrossPlatformInputManager.GetAxis("Mouse X") * this.m_MouseSensitivityX * 0.2f * num;
				this.m_Inputs.m_LimbRotation = Mathf.Clamp(this.m_Inputs.m_LimbRotation, 0f, 0.999f);
				this.m_Inputs.m_LeftArmMouseY += CrossPlatformInputManager.GetAxis("Mouse Y") * this.m_MouseSensitivityY;
				this.m_Inputs.m_LeftArmMouseY = Mathf.Clamp(this.m_Inputs.m_LeftArmMouseY, 0f, 1f);
			}
			else if (this.m_State == BIState.Leaving)
			{
				this.ZeroLimbRotationWithBlend();
				this.ZeroChooseLimbWithBlend();
			}
		}
	}

	public override void OnInputAction(InputsManager.InputAction action)
	{
		if (this.CanLeave() && action == InputsManager.InputAction.QuitBodyInspection && Time.time - Inventory3DManager.Get().m_DeactivationTime > 0.3f)
		{
			this.m_Animator.SetBool("Inventory", true);
			this.SetState(BIState.Leaving);
		}
	}

	public void Hide()
	{
		if (this.CanLeave())
		{
			this.SetState(BIState.Leaving);
		}
	}

	public bool CanLeave()
	{
		return this.m_State == BIState.ChooseLimb || this.m_State == BIState.RotateLeftArm || this.m_State == BIState.RotateRightArm || this.m_State == BIState.RotateLeftLeg || this.m_State == BIState.RotateRightLeg;
	}

	private void ZeroChooseLimbWithBlend()
	{
		float num = 6f;
		float num2 = this.m_Inputs.m_ChooseLimbX * num * Time.deltaTime;
		this.m_Inputs.m_ChooseLimbX -= num2;
		if (this.m_State == BIState.Leaving)
		{
			num2 = (1f - this.m_Inputs.m_ChooseLimbY) * num * Time.deltaTime;
			this.m_Inputs.m_ChooseLimbY += num2;
		}
		else
		{
			num2 = this.m_Inputs.m_ChooseLimbY * num * Time.deltaTime;
			this.m_Inputs.m_ChooseLimbY -= num2;
		}
	}

	private void ZeroLimbRotationWithBlend()
	{
		float num = 6f;
		float num2 = (0.5f - this.m_Inputs.m_LimbRotation) * num * Time.deltaTime;
		this.m_Inputs.m_LimbRotation += num2;
	}

	public void DebugAttachLeeches()
	{
		for (int i = 0; i < this.m_WoundSlots.Count; i++)
		{
			if (this.m_WoundSlots[i].IsInjuryOfType(InjuryType.Leech))
			{
				PlayerInjuryModule.Get().AddInjury(InjuryType.Leech, this.m_WoundSlots[i].m_InjuryPlace, this.m_WoundSlots[i], InjuryState.Open, 0, null);
			}
		}
	}

	public void DebugAttachWorms()
	{
		for (int i = 0; i < this.m_WoundSlots.Count; i++)
		{
			if (this.m_WoundSlots[i].IsInjuryOfType(InjuryType.Worm))
			{
				PlayerInjuryModule.Get().AddInjury(InjuryType.Worm, this.m_WoundSlots[i].m_InjuryPlace, this.m_WoundSlots[i], InjuryState.Open, 0, null);
			}
		}
	}

	private void SetWoundSlots()
	{
		this.m_WoundSlots.Clear();
		for (int i = 0; i < 11; i++)
		{
			if (i != 5)
			{
				if (i == 7)
				{
					string name = string.Empty;
					for (int j = 0; j < 42; j++)
					{
						name = "Wound" + ((j >= 10) ? j.ToString() : ("0" + j.ToString()));
						Transform transform = this.m_Player.gameObject.transform.FindDeepChild(name);
						if (transform != null)
						{
							BIWoundSlot biwoundSlot = transform.gameObject.GetComponent<BIWoundSlot>();
							if (biwoundSlot == null)
							{
								biwoundSlot = transform.gameObject.AddComponent<BIWoundSlot>();
							}
							if (!transform.gameObject.activeSelf)
							{
								transform.gameObject.SetActive(true);
							}
							biwoundSlot.m_Transform = transform;
							if (j <= 7)
							{
								biwoundSlot.m_InjuryPlace = InjuryPlace.LHand;
							}
							else if (j <= 14)
							{
								biwoundSlot.m_InjuryPlace = InjuryPlace.RHand;
							}
							else if (j <= 27)
							{
								biwoundSlot.m_InjuryPlace = InjuryPlace.LLeg;
							}
							else if (j <= 41)
							{
								biwoundSlot.m_InjuryPlace = InjuryPlace.RLeg;
							}
							biwoundSlot.m_InjuryType.Add(InjuryType.Leech);
							if (j == 0 || j == 5 || j == 6 || j == 9 || j == 11 || j == 12 || j == 16 || j == 21 || j == 24 || j == 29 || j == 31 || j == 37)
							{
								biwoundSlot.m_InjuryType.Add(InjuryType.Worm);
							}
							if (!this.m_WoundSlots.Contains(biwoundSlot))
							{
								this.m_WoundSlots.Add(biwoundSlot);
							}
						}
					}
				}
				else
				{
					for (int k = 0; k < 4; k++)
					{
						if (i != 4 || k == 0)
						{
							string name2 = "Hand_L_Wound0" + k;
							Transform transform2 = this.m_Player.gameObject.transform.FindDeepChild(name2);
							if (transform2 != null)
							{
								BIWoundSlot biwoundSlot2 = transform2.gameObject.GetComponent<BIWoundSlot>();
								if (biwoundSlot2 == null)
								{
									biwoundSlot2 = transform2.gameObject.AddComponent<BIWoundSlot>();
									biwoundSlot2.m_Transform = transform2;
								}
								biwoundSlot2.m_InjuryPlace = InjuryPlace.LHand;
								biwoundSlot2.m_InjuryType.Add((InjuryType)i);
								if (!this.m_WoundSlots.Contains(biwoundSlot2))
								{
									this.m_WoundSlots.Add(biwoundSlot2);
								}
								if (i == 4 && k == 0 && biwoundSlot2.m_AdditionalMeshes == null)
								{
									biwoundSlot2.m_AdditionalMeshes = new List<GameObject>();
									transform2 = this.m_Player.gameObject.transform.FindDeepChild("Hand_L_Wound01");
									biwoundSlot2.m_AdditionalMeshes.Add(transform2.gameObject);
									transform2 = this.m_Player.gameObject.transform.FindDeepChild("Hand_L_Wound02");
									biwoundSlot2.m_AdditionalMeshes.Add(transform2.gameObject);
								}
							}
							name2 = "Hand_R_Wound0" + k;
							transform2 = this.m_Player.gameObject.transform.FindDeepChild(name2);
							if (transform2 != null)
							{
								BIWoundSlot biwoundSlot3 = transform2.gameObject.GetComponent<BIWoundSlot>();
								if (biwoundSlot3 == null)
								{
									biwoundSlot3 = transform2.gameObject.AddComponent<BIWoundSlot>();
									biwoundSlot3.m_Transform = transform2;
								}
								biwoundSlot3.m_InjuryPlace = InjuryPlace.RHand;
								biwoundSlot3.m_InjuryType.Add((InjuryType)i);
								if (!this.m_WoundSlots.Contains(biwoundSlot3))
								{
									this.m_WoundSlots.Add(biwoundSlot3);
								}
								if (i == 4 && k == 0 && biwoundSlot3.m_AdditionalMeshes == null)
								{
									biwoundSlot3.m_AdditionalMeshes = new List<GameObject>();
									transform2 = this.m_Player.gameObject.transform.FindDeepChild("Hand_R_Wound01");
									biwoundSlot3.m_AdditionalMeshes.Add(transform2.gameObject);
									transform2 = this.m_Player.gameObject.transform.FindDeepChild("Hand_R_Wound02");
									biwoundSlot3.m_AdditionalMeshes.Add(transform2.gameObject);
								}
							}
							name2 = "Leg_L_Wound0" + k;
							transform2 = this.m_Player.gameObject.transform.FindDeepChild(name2);
							if (transform2 != null)
							{
								BIWoundSlot biwoundSlot4 = transform2.gameObject.GetComponent<BIWoundSlot>();
								if (biwoundSlot4 == null)
								{
									biwoundSlot4 = transform2.gameObject.AddComponent<BIWoundSlot>();
									biwoundSlot4.m_Transform = transform2;
								}
								biwoundSlot4.m_InjuryPlace = InjuryPlace.LLeg;
								biwoundSlot4.m_InjuryType.Add((InjuryType)i);
								if (!this.m_WoundSlots.Contains(biwoundSlot4))
								{
									this.m_WoundSlots.Add(biwoundSlot4);
								}
								if (i == 4 && k == 0 && biwoundSlot4.m_AdditionalMeshes == null)
								{
									biwoundSlot4.m_AdditionalMeshes = new List<GameObject>();
									transform2 = this.m_Player.gameObject.transform.FindDeepChild("Leg_L_Wound01");
									biwoundSlot4.m_AdditionalMeshes.Add(transform2.gameObject);
									transform2 = this.m_Player.gameObject.transform.FindDeepChild("Leg_L_Wound02");
									biwoundSlot4.m_AdditionalMeshes.Add(transform2.gameObject);
								}
							}
							name2 = "Leg_R_Wound0" + k;
							transform2 = this.m_Player.gameObject.transform.FindDeepChild(name2);
							if (transform2 != null)
							{
								BIWoundSlot biwoundSlot5 = transform2.gameObject.GetComponent<BIWoundSlot>();
								if (biwoundSlot5 == null)
								{
									biwoundSlot5 = transform2.gameObject.AddComponent<BIWoundSlot>();
									biwoundSlot5.m_Transform = transform2;
								}
								biwoundSlot5.m_InjuryPlace = InjuryPlace.RLeg;
								biwoundSlot5.m_InjuryType.Add((InjuryType)i);
								if (!this.m_WoundSlots.Contains(biwoundSlot5))
								{
									this.m_WoundSlots.Add(biwoundSlot5);
								}
								if (i == 4 && k == 0 && biwoundSlot5.m_AdditionalMeshes == null)
								{
									biwoundSlot5.m_AdditionalMeshes = new List<GameObject>();
									transform2 = this.m_Player.gameObject.transform.FindDeepChild("Leg_R_Wound01");
									biwoundSlot5.m_AdditionalMeshes.Add(transform2.gameObject);
									transform2 = this.m_Player.gameObject.transform.FindDeepChild("Leg_R_Wound02");
									biwoundSlot5.m_AdditionalMeshes.Add(transform2.gameObject);
								}
							}
						}
					}
				}
			}
		}
	}

	private void InitializeMaggotsVertices()
	{
		BIWoundSlot woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound00");
		int[] array = new int[]
		{
			11,
			24,
			51,
			56,
			5,
			58,
			13,
			60
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound01");
		array = new int[]
		{
			23,
			48,
			24,
			35,
			42,
			36,
			54
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound02");
		array = new int[]
		{
			14,
			17,
			44,
			25,
			12,
			0
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound00");
		array = new int[]
		{
			52,
			51,
			59,
			48,
			60,
			25
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound01");
		array = new int[]
		{
			22,
			17,
			23,
			49,
			55,
			32,
			36
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound02");
		array = new int[]
		{
			42,
			13,
			46,
			50,
			10,
			54
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound00");
		array = new int[]
		{
			36,
			5,
			25,
			44,
			41,
			4,
			38
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound01");
		array = new int[]
		{
			27,
			43,
			25,
			45,
			11,
			41,
			24
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound02");
		array = new int[]
		{
			35,
			38,
			40,
			17,
			5,
			12,
			15,
			45
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound00");
		array = new int[]
		{
			39,
			18,
			13,
			40,
			6,
			37,
			46
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound01");
		array = new int[]
		{
			22,
			14,
			38,
			5,
			1,
			42,
			15
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound02");
		array = new int[]
		{
			42,
			14,
			15,
			26,
			36,
			47
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound00");
		array = new int[]
		{
			66,
			14,
			62,
			22,
			50,
			10,
			52,
			26,
			53,
			6,
			55,
			9,
			48
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound01");
		array = new int[]
		{
			44,
			17,
			9,
			49,
			24,
			43,
			5,
			53,
			54,
			37,
			47,
			32
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound02");
		array = new int[]
		{
			20,
			49,
			24,
			7,
			44,
			15,
			31,
			52,
			11,
			46,
			13,
			51
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound00");
		array = new int[]
		{
			14,
			61,
			10,
			53,
			26,
			52,
			50,
			22,
			9,
			56,
			51,
			6
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound01");
		array = new int[]
		{
			44,
			18,
			48,
			9,
			43,
			24,
			23,
			47,
			5,
			53,
			37,
			52,
			32
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound02");
		array = new int[]
		{
			24,
			44,
			20,
			15,
			31,
			50,
			13,
			48,
			1,
			53
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound00");
		array = new int[]
		{
			19,
			37,
			39,
			40,
			3,
			9
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound01");
		array = new int[]
		{
			38,
			31,
			24,
			48,
			5,
			23,
			22,
			36,
			11,
			45
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound02");
		array = new int[]
		{
			26,
			15,
			43,
			36,
			22,
			9,
			52,
			5,
			38
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound00");
		array = new int[]
		{
			35,
			36,
			5,
			39,
			46,
			25,
			37,
			25,
			45,
			9,
			41,
			7,
			49
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound01");
		array = new int[]
		{
			23,
			11,
			39,
			36,
			7,
			52,
			24,
			51,
			31,
			22,
			41,
			5
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound02");
		array = new int[]
		{
			46,
			36,
			15,
			26,
			39,
			37,
			5
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound00");
		array = new int[]
		{
			25,
			9,
			26,
			45,
			63
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundAbrassion);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound01");
		array = new int[]
		{
			22,
			5,
			50,
			36,
			23,
			52
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundAbrassion);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound02");
		array = new int[]
		{
			15,
			50,
			53,
			1,
			47,
			14
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundAbrassion);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound00");
		array = new int[]
		{
			10,
			8,
			12,
			31,
			25
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundAbrassion);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound01");
		array = new int[]
		{
			22,
			1,
			45,
			48,
			51
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundAbrassion);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound02");
		array = new int[]
		{
			15,
			1,
			52,
			46,
			42
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundAbrassion);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound00");
		array = new int[]
		{
			4,
			39,
			36,
			14
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundAbrassion);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound01");
		array = new int[]
		{
			23,
			37,
			11,
			44
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundAbrassion);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound02");
		array = new int[]
		{
			37,
			16,
			38,
			7,
			39
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundAbrassion);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound00");
		array = new int[]
		{
			44,
			40,
			37,
			18,
			14
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundAbrassion);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound01");
		array = new int[]
		{
			23,
			49,
			7,
			50,
			38,
			40
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundAbrassion);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound02");
		array = new int[]
		{
			16,
			38,
			5,
			12,
			7
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundAbrassion);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound00");
		array = new int[]
		{
			60,
			49,
			56,
			32
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundScratch);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound01");
		array = new int[]
		{
			16,
			23,
			35,
			22
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundScratch);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound02");
		array = new int[]
		{
			16,
			17,
			51,
			12,
			13
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundScratch);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound00");
		array = new int[]
		{
			10,
			57,
			24,
			48,
			49
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundScratch);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound01");
		array = new int[]
		{
			44,
			23,
			17,
			16,
			47
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundScratch);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound02");
		array = new int[]
		{
			14,
			12,
			16,
			31,
			53
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundScratch);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound00");
		array = new int[]
		{
			43,
			25,
			18,
			30
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundScratch);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound01");
		array = new int[]
		{
			27,
			11,
			8,
			12
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundScratch);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound02");
		array = new int[]
		{
			14,
			1,
			2,
			35
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundScratch);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound00");
		array = new int[]
		{
			4,
			30,
			38,
			25
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundScratch);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound01");
		array = new int[]
		{
			27,
			11,
			12,
			8
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundScratch);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound02");
		array = new int[]
		{
			14,
			35,
			36,
			1
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.SmallWoundScratch);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound00");
		array = new int[]
		{
			11,
			8
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.WormHole);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound01");
		array = new int[]
		{
			23,
			17
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.WormHole);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound02");
		array = new int[]
		{
			14,
			31
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.WormHole);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound00");
		array = new int[]
		{
			52
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.WormHole);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound01");
		array = new int[]
		{
			22
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.WormHole);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound02");
		array = new int[]
		{
			14
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.WormHole);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound00");
		array = new int[]
		{
			36
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.WormHole);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound01");
		array = new int[]
		{
			42
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.WormHole);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound02");
		array = new int[]
		{
			14
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.WormHole);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound00");
		array = new int[]
		{
			39
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.WormHole);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound01");
		array = new int[]
		{
			37
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.WormHole);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound02");
		array = new int[]
		{
			14
		};
		this.InitializeMaggotsVerticesSlot(woundSlot, ref array, InjuryType.WormHole);
	}

	private void InitializeAntsVertices()
	{
		BIWoundSlot woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound00");
		int[] array = new int[]
		{
			10,
			60,
			52,
			51,
			31
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound01");
		array = new int[]
		{
			17,
			50,
			45,
			35,
			54
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound02");
		array = new int[]
		{
			53,
			13,
			17,
			13,
			53,
			17
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound00");
		array = new int[]
		{
			61,
			49,
			9,
			31,
			52
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound01");
		array = new int[]
		{
			15,
			17,
			47,
			52
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound02");
		array = new int[]
		{
			44,
			16,
			17,
			13,
			52
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound00");
		array = new int[]
		{
			3,
			43,
			42,
			36,
			25,
			54
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound01");
		array = new int[]
		{
			24,
			27,
			42,
			11,
			44
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound02");
		array = new int[]
		{
			2,
			39,
			14,
			35,
			15,
			26
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound00");
		array = new int[]
		{
			41,
			39,
			18
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound01");
		array = new int[]
		{
			51,
			27,
			37,
			11,
			40
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound02");
		array = new int[]
		{
			26,
			35,
			42
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.Laceration);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound00");
		array = new int[]
		{
			66,
			14,
			62,
			22,
			50,
			10,
			52,
			26,
			53,
			6,
			55,
			9,
			48
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound01");
		array = new int[]
		{
			44,
			17,
			9,
			49,
			24,
			43,
			5,
			53,
			54,
			37,
			47,
			32
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.RHand, "Hand_R_Wound02");
		array = new int[]
		{
			20,
			49,
			24,
			7,
			44,
			15,
			31,
			52,
			11,
			46,
			13,
			51
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound00");
		array = new int[]
		{
			14,
			61,
			10,
			53,
			26,
			52,
			50,
			22,
			9,
			56,
			51,
			6
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound01");
		array = new int[]
		{
			44,
			18,
			48,
			9,
			43,
			24,
			23,
			47,
			5,
			53,
			37,
			52,
			32
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.LHand, "Hand_L_Wound02");
		array = new int[]
		{
			24,
			44,
			20,
			15,
			31,
			50,
			13,
			48,
			1,
			53
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound00");
		array = new int[]
		{
			19,
			37,
			39,
			40,
			3,
			9
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound01");
		array = new int[]
		{
			38,
			31,
			24,
			48,
			5,
			23,
			22,
			36,
			11,
			45
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.RLeg, "Leg_R_Wound02");
		array = new int[]
		{
			26,
			15,
			43,
			36,
			22,
			9,
			52,
			5,
			38
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound00");
		array = new int[]
		{
			35,
			36,
			5,
			39,
			46,
			25,
			37,
			25,
			45,
			9,
			41,
			7,
			49
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound01");
		array = new int[]
		{
			23,
			11,
			39,
			36,
			7,
			52,
			24,
			51,
			31,
			22,
			41,
			5
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
		woundSlot = this.GetWoundSlot(InjuryPlace.LLeg, "Leg_L_Wound02");
		array = new int[]
		{
			46,
			36,
			15,
			26,
			39,
			37,
			5
		};
		this.InitializeAntsVerticesSlot(woundSlot, ref array, InjuryType.LacerationCat);
	}

	private void InitializeMaggotsVerticesSlot(BIWoundSlot slot, ref int[] indices, InjuryType injury_type)
	{
		SkinnedMeshRenderer component = slot.m_Transform.GetComponent<SkinnedMeshRenderer>();
		Vector3[] vertices = component.sharedMesh.vertices;
		Vector3[] normals = component.sharedMesh.normals;
		Vector4[] tangents = component.sharedMesh.tangents;
		BoneWeight[] boneWeights = component.sharedMesh.boneWeights;
		Vector3[] array = null;
		WeightList[] array2 = null;
		switch (injury_type)
		{
		case InjuryType.SmallWoundAbrassion:
			slot.m_AbrassionMaggotsVertices = new Vector3[indices.Length];
			slot.m_AbrassionMaggotsWeightList = new WeightList[indices.Length];
			array = slot.m_AbrassionMaggotsVertices;
			array2 = slot.m_AbrassionMaggotsWeightList;
			break;
		case InjuryType.SmallWoundScratch:
			slot.m_ScratchMaggotsVertices = new Vector3[indices.Length];
			slot.m_ScratchMaggotsWeightList = new WeightList[indices.Length];
			array = slot.m_ScratchMaggotsVertices;
			array2 = slot.m_ScratchMaggotsWeightList;
			break;
		case InjuryType.Laceration:
			slot.m_LacerationMaggotsVertices = new Vector3[indices.Length];
			slot.m_LacerationMaggotsWeightList = new WeightList[indices.Length];
			array = slot.m_LacerationMaggotsVertices;
			array2 = slot.m_LacerationMaggotsWeightList;
			break;
		case InjuryType.LacerationCat:
			slot.m_LacerationCatMaggotsVertices = new Vector3[indices.Length];
			slot.m_LacerationCatMaggotsWeightList = new WeightList[indices.Length];
			array = slot.m_LacerationCatMaggotsVertices;
			array2 = slot.m_LacerationCatMaggotsWeightList;
			break;
		case InjuryType.WormHole:
			slot.m_WormHoleMaggotsVertices = new Vector3[indices.Length];
			slot.m_WormHoleMaggotsWeightList = new WeightList[indices.Length];
			array = slot.m_WormHoleMaggotsVertices;
			array2 = slot.m_WormHoleMaggotsWeightList;
			break;
		}
		for (int i = 0; i < indices.Length; i++)
		{
			array[i] = vertices[indices[i]];
			BoneWeight boneWeight = component.sharedMesh.boneWeights[i];
			array2[i] = new WeightList();
			if (boneWeight.weight0 != 0f)
			{
				Vector3 p = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyPoint3x4(vertices[indices[i]]);
				Vector3 n = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(normals[indices[i]]);
				Vector4 tangent = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(tangents[indices[i]]);
				array2[i].transforms[0] = component.bones[boneWeight.boneIndex0];
				array2[i].weights[0] = new VertexWeight(i, p, n, tangent, boneWeight.weight0);
			}
			if (boneWeight.weight1 != 0f)
			{
				Vector3 p = component.sharedMesh.bindposes[boneWeight.boneIndex1].MultiplyPoint3x4(vertices[indices[i]]);
				Vector3 n = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(normals[indices[i]]);
				Vector4 tangent = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(tangents[indices[i]]);
				array2[i].transforms[1] = component.bones[boneWeight.boneIndex1];
				array2[i].weights[1] = new VertexWeight(i, p, n, tangent, boneWeight.weight1);
			}
			if (boneWeight.weight2 != 0f)
			{
				Vector3 p = component.sharedMesh.bindposes[boneWeight.boneIndex2].MultiplyPoint3x4(vertices[indices[i]]);
				Vector3 n = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(normals[indices[i]]);
				Vector4 tangent = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(tangents[indices[i]]);
				array2[i].transforms[2] = component.bones[boneWeight.boneIndex2];
				array2[i].weights[2] = new VertexWeight(i, p, n, tangent, boneWeight.weight2);
			}
			if (boneWeight.weight3 != 0f)
			{
				Vector3 p = component.sharedMesh.bindposes[boneWeight.boneIndex3].MultiplyPoint3x4(vertices[indices[i]]);
				Vector3 n = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(normals[indices[i]]);
				Vector4 tangent = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(tangents[indices[i]]);
				array2[i].transforms[3] = component.bones[boneWeight.boneIndex3];
				array2[i].weights[3] = new VertexWeight(i, p, n, tangent, boneWeight.weight3);
			}
			array2[i].rotation = UnityEngine.Random.Range(0f, 360f);
		}
	}

	private void InitializeAntsVerticesSlot(BIWoundSlot slot, ref int[] indices, InjuryType injury_type)
	{
		SkinnedMeshRenderer component = slot.m_Transform.GetComponent<SkinnedMeshRenderer>();
		Vector3[] vertices = component.sharedMesh.vertices;
		Vector3[] normals = component.sharedMesh.normals;
		Vector4[] tangents = component.sharedMesh.tangents;
		BoneWeight[] boneWeights = component.sharedMesh.boneWeights;
		Vector3[] array = null;
		WeightList[] array2 = null;
		Transform transform = this.m_Player.transform.FindDeepChild("PL");
		if (injury_type != InjuryType.Laceration)
		{
			if (injury_type == InjuryType.LacerationCat)
			{
				slot.m_LacerationCatAntsVertices = new Vector3[indices.Length];
				slot.m_LacerationCatAntsWeightList = new WeightList[indices.Length];
				array = slot.m_LacerationCatAntsVertices;
				array2 = slot.m_LacerationCatAntsWeightList;
			}
		}
		else
		{
			slot.m_LacerationAntsVertices = new Vector3[indices.Length];
			slot.m_LacerationAntsWeightList = new WeightList[indices.Length];
			array = slot.m_LacerationAntsVertices;
			array2 = slot.m_LacerationAntsWeightList;
		}
		for (int i = 0; i < indices.Length; i++)
		{
			array[i] = vertices[indices[i]];
			BoneWeight boneWeight = component.sharedMesh.boneWeights[i];
			array2[i] = new WeightList();
			if (boneWeight.weight0 != 0f)
			{
				Matrix4x4 rhs = default(Matrix4x4);
				Matrix4x4 lhs = component.transform.localToWorldMatrix.inverse;
				lhs *= transform.localToWorldMatrix;
				rhs = component.sharedMesh.bindposes[boneWeight.boneIndex0].inverse;
				Vector3 p = (lhs * rhs).inverse.MultiplyPoint3x4(vertices[indices[i]]);
				Vector3 n = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(normals[indices[i]]);
				Vector4 tangent = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(tangents[indices[i]]);
				array2[i].transforms[0] = component.bones[boneWeight.boneIndex0];
				array2[i].weights[0] = new VertexWeight(i, p, n, tangent, boneWeight.weight0);
			}
			if (boneWeight.weight1 != 0f)
			{
				Matrix4x4 rhs2 = default(Matrix4x4);
				Matrix4x4 lhs2 = component.transform.localToWorldMatrix.inverse;
				lhs2 *= transform.localToWorldMatrix;
				rhs2 = component.sharedMesh.bindposes[boneWeight.boneIndex1].inverse;
				Vector3 p = (lhs2 * rhs2).inverse.MultiplyPoint3x4(vertices[indices[i]]);
				Vector3 n = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(normals[indices[i]]);
				Vector4 tangent = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(tangents[indices[i]]);
				array2[i].transforms[1] = component.bones[boneWeight.boneIndex1];
				array2[i].weights[1] = new VertexWeight(i, p, n, tangent, boneWeight.weight1);
			}
			if (boneWeight.weight2 != 0f)
			{
				Matrix4x4 rhs3 = default(Matrix4x4);
				Matrix4x4 lhs3 = component.transform.localToWorldMatrix.inverse;
				lhs3 *= transform.localToWorldMatrix;
				rhs3 = component.sharedMesh.bindposes[boneWeight.boneIndex2].inverse;
				Vector3 p = (lhs3 * rhs3).inverse.MultiplyPoint3x4(vertices[indices[i]]);
				Vector3 n = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(normals[indices[i]]);
				Vector4 tangent = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(tangents[indices[i]]);
				array2[i].transforms[2] = component.bones[boneWeight.boneIndex2];
				array2[i].weights[2] = new VertexWeight(i, p, n, tangent, boneWeight.weight2);
			}
			if (boneWeight.weight3 != 0f)
			{
				Matrix4x4 rhs4 = default(Matrix4x4);
				Matrix4x4 lhs4 = component.transform.localToWorldMatrix.inverse;
				lhs4 *= transform.localToWorldMatrix;
				rhs4 = component.sharedMesh.bindposes[boneWeight.boneIndex3].inverse;
				Vector3 p = (lhs4 * rhs4).inverse.MultiplyPoint3x4(vertices[indices[i]]);
				Vector3 n = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(normals[indices[i]]);
				Vector4 tangent = component.sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyVector(tangents[indices[i]]);
				array2[i].transforms[3] = component.bones[boneWeight.boneIndex3];
				array2[i].weights[3] = new VertexWeight(i, p, n, tangent, boneWeight.weight3);
			}
			array2[i].rotation = UnityEngine.Random.Range(0f, 360f);
		}
	}

	private void UpdateSelectedWound()
	{
		this.m_SelectedWound = null;
		if (this.m_State != BIState.RotateLeftArm && this.m_State != BIState.RotateRightArm && this.m_State != BIState.RotateLeftLeg && this.m_State != BIState.RotateRightLeg)
		{
			return;
		}
		this.m_VisibleSlots.Clear();
		for (int i = 0; i < this.m_WoundSlots.Count; i++)
		{
			BIWoundSlot biwoundSlot = this.m_WoundSlots[i];
			if (biwoundSlot.m_Wound != null && biwoundSlot.IsVisible())
			{
				this.m_VisibleSlots.Add(biwoundSlot);
			}
		}
		if (this.m_VisibleSlots.Count == 0)
		{
			return;
		}
		BIWoundSlotComparer comparer = new BIWoundSlotComparer();
		this.m_VisibleSlots.Sort(comparer);
		float num = 0f;
		switch (this.m_State)
		{
		case BIState.RotateLeftArm:
			num = this.m_Inputs.m_LeftArmMouseY;
			break;
		case BIState.RotateRightArm:
			num = this.m_Inputs.m_RightArmMouseY;
			break;
		case BIState.RotateLeftLeg:
			num = this.m_Inputs.m_LeftLegMouseY;
			break;
		case BIState.RotateRightLeg:
			num = this.m_Inputs.m_RightLegMouseY;
			break;
		}
		int num2 = Mathf.FloorToInt(num / (1f / (float)this.m_VisibleSlots.Count));
		if (num2 >= this.m_VisibleSlots.Count)
		{
			num2 = this.m_VisibleSlots.Count - 1;
		}
		this.m_SelectedWound = this.m_VisibleSlots[num2];
		if (this.m_Inputs.m_SelectLimb && this.m_SelectedWound.GetInjury() != null && !this.m_SelectedWound.GetInjury().GetDiagnosed())
		{
			this.m_SelectedWound.GetInjury().SetDiagnosed(true);
			EventsManager.OnEvent(Enums.Event.DiagnoseInjury, 1);
		}
	}

	public BIWoundSlot GetWoundSlot(InjuryPlace place, string name)
	{
		for (int i = 0; i < this.m_WoundSlots.Count; i++)
		{
			if (this.m_WoundSlots[i].m_InjuryPlace == place && this.m_WoundSlots[i].name == name)
			{
				return this.m_WoundSlots[i];
			}
		}
		return null;
	}

	public BIWoundSlot GetWoundSlot(InjuryPlace place, InjuryType injury_type, List<string> available_names = null)
	{
		List<BIWoundSlot> list = new List<BIWoundSlot>();
		for (int i = 0; i < this.m_WoundSlots.Count; i++)
		{
			if (this.m_WoundSlots[i].m_InjuryPlace == place && this.m_WoundSlots[i].IsInjuryOfType(injury_type) && (available_names == null || available_names.Contains(this.m_WoundSlots[i].gameObject.name)))
			{
				list.Add(this.m_WoundSlots[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public BIWoundSlot GetFreeWoundSlot(InjuryPlace place, InjuryType injury_type)
	{
		bool flag = false;
		bool flag2 = true;
		List<BIWoundSlot> list = new List<BIWoundSlot>();
		for (int i = 0; i < this.m_WoundSlots.Count; i++)
		{
			if (this.m_WoundSlots[i].m_InjuryPlace == place && this.m_WoundSlots[i].m_Wound != null)
			{
				flag = true;
				if (this.m_WoundSlots[i].m_Injury.m_Type != InjuryType.Leech)
				{
					flag2 = false;
				}
			}
		}
		if (injury_type != InjuryType.Leech && flag)
		{
			return null;
		}
		if (injury_type == InjuryType.Leech && flag && !flag2)
		{
			return null;
		}
		for (int j = 0; j < this.m_WoundSlots.Count; j++)
		{
			if (this.m_WoundSlots[j].m_InjuryPlace == place && this.m_WoundSlots[j].m_Wound == null && this.m_WoundSlots[j].IsInjuryOfType(injury_type))
			{
				list.Add(this.m_WoundSlots[j]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public void StartBandage(InjuryPlace limb)
	{
		switch (limb)
		{
		case InjuryPlace.LHand:
			this.SetState(BIState.BandageLeftHandBegin);
			break;
		case InjuryPlace.RHand:
			this.SetState(BIState.BandageRightHandBegin);
			break;
		case InjuryPlace.RLeg:
			this.SetState(BIState.BandageRightLegBegin);
			break;
		case InjuryPlace.LLeg:
			this.SetState(BIState.BandageLeftLegBegin);
			break;
		}
		Inventory3DManager.Get().Deactivate();
		this.PlayBandagingSound();
	}

	private void UpdateLimbBandageBegin()
	{
		this.ZeroChooseLimbWithBlend();
		this.ZeroLimbRotationWithBlend();
		if (Mathf.Abs(this.m_Inputs.m_ChooseLimbX) < 0.05f && Mathf.Abs(this.m_Inputs.m_ChooseLimbY) < 0.05f && Mathf.Abs(this.m_Inputs.m_LimbRotation) > 0.45f && Mathf.Abs(this.m_Inputs.m_LimbRotation) < 0.55f)
		{
			switch (this.m_State)
			{
			case BIState.BandageLeftHandBegin:
				this.SetState(BIState.BandageLeftHand);
				this.m_Animator.SetBool(this.m_BBI_BandageLeftHand, true);
				break;
			case BIState.BandageRightHandBegin:
				this.SetState(BIState.BandageRightHand);
				this.m_Animator.SetBool(this.m_BBI_BandageRightHand, true);
				break;
			case BIState.BandageLeftLegBegin:
				this.SetState(BIState.BandageLeftLeg);
				this.m_Animator.SetBool(this.m_BBI_BandageLeftLeg, true);
				break;
			case BIState.BandageRightLegBegin:
				this.SetState(BIState.BandageRightLeg);
				this.m_Animator.SetBool(this.m_BBI_BandageRightLeg, true);
				break;
			}
		}
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.BIShowBandage)
		{
			this.PlaceBandage();
		}
		else if (id == AnimEventID.BIBandageEnd)
		{
			this.EndBandage();
		}
		else if (id == AnimEventID.BIHack)
		{
			this.ApplyHack();
		}
	}

	private void ApplyHack()
	{
		this.m_Animator.SetBool("Inventory", true);
	}

	private void PlaceBandage()
	{
		if (this.m_ActiveSlot != null && this.m_ActiveSlot.m_Injury != null)
		{
			this.m_ActiveSlot.m_Injury.StartHealing();
			this.ShowBandage();
		}
	}

	private void EndBandage()
	{
		this.Stop();
	}

	private void ShowBandage()
	{
		Transform transform = null;
		if (this.m_ActiveSlot.m_InjuryPlace == InjuryPlace.LHand)
		{
			transform = this.m_Player.gameObject.transform.FindDeepChild("dressing00");
		}
		else if (this.m_ActiveSlot.m_InjuryPlace == InjuryPlace.RHand)
		{
			transform = this.m_Player.gameObject.transform.FindDeepChild("dressing01");
		}
		else if (this.m_ActiveSlot.m_InjuryPlace == InjuryPlace.LLeg)
		{
			transform = this.m_Player.gameObject.transform.FindDeepChild("dressing02");
		}
		else if (this.m_ActiveSlot.m_InjuryPlace == InjuryPlace.RLeg)
		{
			transform = this.m_Player.gameObject.transform.FindDeepChild("dressing03");
		}
		transform.gameObject.SetActive(true);
		this.m_ActiveSlot.m_Injury.m_Bandage = transform.gameObject;
		this.m_ActiveSlot.m_Injury.StartHealing();
	}

	private void UpdateBandageToChooseLimbBlend()
	{
		AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(0);
		this.m_Inputs.m_LimbRotation = currentAnimatorStateInfo.normalizedTime;
		if (currentAnimatorStateInfo.normalizedTime > 0f && currentAnimatorStateInfo.normalizedTime < 0.8f)
		{
			this.SetState(BIState.ChooseLimb);
		}
	}

	public void AttachMaggots()
	{
		if (this.m_ActiveSlot.m_Maggots == null)
		{
			this.m_ActiveSlot.m_Maggots = new List<GameObject>();
		}
		switch (this.m_ActiveSlot.m_Injury.m_Type)
		{
		case InjuryType.SmallWoundAbrassion:
			for (int i = 0; i < this.m_ActiveSlot.m_AbrassionMaggotsVertices.Length; i++)
			{
				Item item = ItemsManager.Get().CreateItem(ItemID.Maggot, false, Vector3.zero, Quaternion.identity);
				GameObject gameObject = item.gameObject;
				gameObject.name = "Maggot in wound";
				this.m_ActiveSlot.m_Maggots.Add(gameObject);
			}
			this.m_ActiveSlot.m_Injury.StartHealing();
			break;
		case InjuryType.SmallWoundScratch:
			for (int j = 0; j < this.m_ActiveSlot.m_ScratchMaggotsVertices.Length; j++)
			{
				Item item2 = ItemsManager.Get().CreateItem(ItemID.Maggot, false, Vector3.zero, Quaternion.identity);
				GameObject gameObject2 = item2.gameObject;
				gameObject2.name = "Maggot in wound";
				this.m_ActiveSlot.m_Maggots.Add(gameObject2);
			}
			this.m_ActiveSlot.m_Injury.StartHealing();
			break;
		case InjuryType.Laceration:
			for (int k = 0; k < this.m_ActiveSlot.m_LacerationMaggotsVertices.Length; k++)
			{
				Item item3 = ItemsManager.Get().CreateItem(ItemID.Maggot, false, Vector3.zero, Quaternion.identity);
				GameObject gameObject3 = item3.gameObject;
				gameObject3.name = "Maggot in wound";
				this.m_ActiveSlot.m_Maggots.Add(gameObject3);
			}
			this.m_ActiveSlot.m_Injury.StartHealing();
			break;
		case InjuryType.LacerationCat:
			for (int l = 0; l < this.m_ActiveSlot.m_LacerationCatMaggotsVertices.Length; l++)
			{
				Item item4 = ItemsManager.Get().CreateItem(ItemID.Maggot, false, Vector3.zero, Quaternion.identity);
				GameObject gameObject4 = item4.gameObject;
				gameObject4.name = "Maggot in wound";
				this.m_ActiveSlot.m_Maggots.Add(gameObject4);
			}
			this.m_ActiveSlot.m_Injury.StartHealing();
			break;
		case InjuryType.WormHole:
			for (int m = 0; m < this.m_ActiveSlot.m_WormHoleMaggotsVertices.Length; m++)
			{
				Item item5 = ItemsManager.Get().CreateItem(ItemID.Maggot, false, Vector3.zero, Quaternion.identity);
				GameObject gameObject5 = item5.gameObject;
				gameObject5.name = "Maggot in wound";
				this.m_ActiveSlot.m_Maggots.Add(gameObject5);
			}
			this.m_ActiveSlot.m_Injury.StartHealing();
			break;
		}
	}

	public void AttachAnts()
	{
		if (this.m_ActiveSlot.m_Ants == null)
		{
			this.m_ActiveSlot.m_Ants = new List<GameObject>();
		}
		InjuryType type = this.m_ActiveSlot.m_Injury.m_Type;
		if (type != InjuryType.Laceration)
		{
			if (type == InjuryType.LacerationCat)
			{
				for (int i = 0; i < this.m_ActiveSlot.m_LacerationCatAntsVertices.Length; i++)
				{
					Item item = ItemsManager.Get().CreateItem(ItemID.Ant_Head, false, Vector3.zero, Quaternion.identity);
					GameObject gameObject = item.gameObject;
					gameObject.name = "Ant in wound";
					this.m_ActiveSlot.m_Ants.Add(gameObject);
				}
				this.m_ActiveSlot.m_Injury.StartHealing();
			}
		}
		else
		{
			for (int j = 0; j < this.m_ActiveSlot.m_LacerationAntsVertices.Length; j++)
			{
				Item item2 = ItemsManager.Get().CreateItem(ItemID.Ant_Head, false, Vector3.zero, Quaternion.identity);
				GameObject gameObject2 = item2.gameObject;
				gameObject2.name = "Ant in wound";
				this.m_ActiveSlot.m_Ants.Add(gameObject2);
			}
			this.m_ActiveSlot.m_Injury.StartHealing();
		}
	}

	private void UpdateFOV()
	{
		if (this.m_State == BIState.Leaving)
		{
			float fieldOfView = Camera.main.fieldOfView + (this.m_StartFOV - Camera.main.fieldOfView) * Time.deltaTime * 6f;
			Camera.main.fieldOfView = fieldOfView;
			this.m_OutlineCamera.fieldOfView = fieldOfView;
		}
		else
		{
			float fieldOfView2 = Camera.main.fieldOfView + (this.m_FOV - Camera.main.fieldOfView) * Time.deltaTime * 6f;
			Camera.main.fieldOfView = fieldOfView2;
			this.m_OutlineCamera.fieldOfView = fieldOfView2;
		}
	}

	private void UpdateCursor()
	{
		Vector2 zero = Vector2.zero;
		zero.x = Input.mousePosition.x / (float)Screen.width;
		zero.y = Input.mousePosition.y / (float)Screen.height;
		Ray ray = Camera.main.ViewportPointToRay(zero);
		RaycastHit[] array = Physics.RaycastAll(ray, 5f);
		for (int i = 0; i < array.Length; i++)
		{
			for (int j = 0; j < this.m_WoundSlots.Count; j++)
			{
				if (array[i].collider.gameObject == this.m_WoundSlots[j].m_Wound && this.m_WoundSlots[j].m_Wound != null && this.m_WoundSlots[j].m_Injury.m_Type == InjuryType.Leech)
				{
					CursorManager.Get().SetCursor(CursorManager.TYPE.Hand_0);
					return;
				}
			}
		}
		if (this.m_CarryingItemDeleech)
		{
			CursorManager.Get().SetCursor(CursorManager.TYPE.Hand_1);
			return;
		}
		CursorManager.Get().SetCursor((!this.m_Inputs.m_RotateLimb) ? CursorManager.TYPE.Normal : CursorManager.TYPE.InspectionArrow);
	}

	private void UpdateDeleech()
	{
		if (this.m_Inputs.m_SelectItem)
		{
			if (this.m_CarryingItemDeleech == null)
			{
				Vector3 zero = Vector3.zero;
				zero.x = Input.mousePosition.x / (float)Screen.width;
				zero.y = Input.mousePosition.y / (float)Screen.height;
				Ray ray = Camera.main.ViewportPointToRay(zero);
				RaycastHit[] array = Physics.RaycastAll(ray, 5f);
				for (int i = 0; i < array.Length; i++)
				{
					for (int j = 0; j < this.m_WoundSlots.Count; j++)
					{
						if (array[i].collider.gameObject == this.m_WoundSlots[j].m_Wound && this.m_WoundSlots[j].m_Wound != null && this.m_WoundSlots[j].m_Injury.m_Type == InjuryType.Leech)
						{
							Item componentDeepChild = General.GetComponentDeepChild<Item>(this.m_WoundSlots[j].m_Wound);
							componentDeepChild.gameObject.transform.parent = null;
							this.m_WoundSlots[j].m_Injury.StartHealing();
							this.m_CarryingItemDeleech = componentDeepChild;
							this.m_DistFromScreen = (componentDeepChild.transform.position - Camera.main.transform.position).magnitude;
							this.m_WoundSlots[j].m_Wound = null;
							PlayerAudioModule.Get().PlayLeechOutSound();
							return;
						}
					}
				}
			}
			else
			{
				Vector3 vector = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
				vector = Camera.main.transform.position + (vector - Camera.main.transform.position).normalized * this.m_DistFromScreen;
				this.m_CarryingItemDeleech.transform.position = vector;
				Animator component = this.m_CarryingItemDeleech.GetComponent<Animator>();
				component.SetBool("Drink", false);
			}
		}
		else if (this.m_CarryingItemDeleech != null)
		{
			this.m_CarryingItemDeleech = null;
		}
	}

	public void Deworm(Item item, BIWoundSlot slot)
	{
		slot.m_Injury.StartHealing();
		UnityEngine.Object.Destroy(slot.m_Wound);
		UnityEngine.Object.Destroy(item.gameObject);
	}

	public void UpdateMaggots()
	{
		for (int i = 0; i < this.m_WoundSlots.Count; i++)
		{
			if (this.m_WoundSlots[i].m_LacerationMaggotsVertices != null && this.m_WoundSlots[i].m_Maggots != null)
			{
				BIWoundSlot biwoundSlot = this.m_WoundSlots[i];
				Vector3[] array = null;
				WeightList[] array2 = null;
				switch (biwoundSlot.m_Injury.m_Type)
				{
				case InjuryType.SmallWoundAbrassion:
					array = biwoundSlot.m_AbrassionMaggotsVertices;
					array2 = biwoundSlot.m_AbrassionMaggotsWeightList;
					break;
				case InjuryType.SmallWoundScratch:
					array = biwoundSlot.m_ScratchMaggotsVertices;
					array2 = biwoundSlot.m_ScratchMaggotsWeightList;
					break;
				case InjuryType.Laceration:
					array = biwoundSlot.m_LacerationMaggotsVertices;
					array2 = biwoundSlot.m_LacerationMaggotsWeightList;
					break;
				case InjuryType.LacerationCat:
					array = biwoundSlot.m_LacerationCatMaggotsVertices;
					array2 = biwoundSlot.m_LacerationCatMaggotsWeightList;
					break;
				case InjuryType.WormHole:
					array = biwoundSlot.m_WormHoleMaggotsVertices;
					array2 = biwoundSlot.m_WormHoleMaggotsWeightList;
					break;
				}
				for (int j = 0; j < array.Length; j++)
				{
					Vector3 vector = Vector3.zero;
					Vector3 vector2 = Vector3.zero;
					Vector3 vector3 = Vector3.zero;
					for (int k = 0; k < 4; k++)
					{
						if (array2[j].transforms[k] != null)
						{
							VertexWeight vertexWeight = array2[j].weights[k];
							vector += array2[j].transforms[k].localToWorldMatrix.MultiplyPoint3x4(vertexWeight.localPosition) * vertexWeight.weight;
							vector2 += array2[j].transforms[k].localToWorldMatrix.MultiplyVector(vertexWeight.localNormal) * vertexWeight.weight;
							Vector3 vector4 = vertexWeight.localTangent;
							vector3 += array2[j].transforms[k].localToWorldMatrix.MultiplyVector(vector4) * vertexWeight.weight;
						}
					}
					biwoundSlot.m_Maggots[j].transform.position = vector;
					biwoundSlot.m_Maggots[j].transform.rotation = Quaternion.LookRotation(vector3, vector2);
					biwoundSlot.m_Maggots[j].transform.Rotate(Vector3.up, array2[j].rotation, Space.Self);
				}
			}
		}
	}

	public void UpdateAnts()
	{
		for (int i = 0; i < this.m_WoundSlots.Count; i++)
		{
			if (this.m_WoundSlots[i].m_LacerationAntsVertices != null && this.m_WoundSlots[i].m_Ants != null)
			{
				BIWoundSlot biwoundSlot = this.m_WoundSlots[i];
				Vector3[] array = null;
				WeightList[] array2 = null;
				InjuryType type = biwoundSlot.m_Injury.m_Type;
				if (type != InjuryType.Laceration)
				{
					if (type == InjuryType.LacerationCat)
					{
						array = biwoundSlot.m_LacerationCatAntsVertices;
						array2 = biwoundSlot.m_LacerationCatAntsWeightList;
					}
				}
				else
				{
					array = biwoundSlot.m_LacerationAntsVertices;
					array2 = biwoundSlot.m_LacerationAntsWeightList;
				}
				for (int j = 0; j < array.Length; j++)
				{
					Vector3 vector = Vector3.zero;
					Vector3 vector2 = Vector3.zero;
					Vector3 vector3 = Vector3.zero;
					for (int k = 0; k < 4; k++)
					{
						if (array2[j].transforms[k] != null)
						{
							VertexWeight vertexWeight = array2[j].weights[k];
							vector += array2[j].transforms[k].localToWorldMatrix.MultiplyPoint3x4(vertexWeight.localPosition) * vertexWeight.weight;
							vector2 += array2[j].transforms[k].localToWorldMatrix.MultiplyVector(vertexWeight.localNormal) * vertexWeight.weight;
							Vector3 vector4 = vertexWeight.localTangent;
							vector3 += array2[j].transforms[k].localToWorldMatrix.MultiplyVector(vector4) * vertexWeight.weight;
						}
					}
					if (biwoundSlot.m_Ants[j])
					{
						biwoundSlot.m_Ants[j].transform.rotation = Quaternion.LookRotation(vector3, vector2);
						biwoundSlot.m_Ants[j].transform.Rotate(Vector3.up, array2[j].rotation, Space.Self);
						biwoundSlot.m_Ants[j].transform.position = vector;
					}
					else
					{
						Item item = ItemsManager.Get().CreateItem(ItemID.Ant_Head, false, Vector3.zero, Quaternion.identity);
						GameObject gameObject = item.gameObject;
						gameObject.name = "Ant in wound";
						biwoundSlot.m_Ants[j] = gameObject;
						biwoundSlot.m_Ants[j].transform.rotation = Quaternion.LookRotation(vector3, vector2);
						biwoundSlot.m_Ants[j].transform.Rotate(Vector3.up, array2[j].rotation, Space.Self);
						biwoundSlot.m_Ants[j].transform.position = vector;
					}
				}
			}
		}
	}

	public void DestroyMaggots(BIWoundSlot slot)
	{
		if (slot.m_Maggots == null)
		{
			return;
		}
		for (int i = 0; i < slot.m_Maggots.Count; i++)
		{
			UnityEngine.Object.Destroy(slot.m_Maggots[i]);
		}
		slot.m_Maggots = null;
	}

	public void DestroyAnts(BIWoundSlot slot)
	{
		if (slot.m_Ants == null)
		{
			return;
		}
		for (int i = 0; i < slot.m_Ants.Count; i++)
		{
			if (slot.m_Ants[i])
			{
				UnityEngine.Object.Destroy(slot.m_Ants[i]);
			}
		}
		slot.m_Ants = null;
	}

	public void HideAndShowLimb()
	{
		this.m_HideAndShowLimb = true;
		this.m_HideAndShowProgress = 0f;
	}

	private void PlayBandagingSound()
	{
		this.m_AudioSource.PlayOneShot(this.m_ArmBandagingSound[UnityEngine.Random.Range(0, this.m_ArmBandagingSound.Count)]);
	}

	private int m_BBodyInspection = Animator.StringToHash("BodyInspection");

	private int m_FBI_Show_Hand = Animator.StringToHash("BI_Show_Hand");

	private int m_FBI_Show_Leg = Animator.StringToHash("BI_Show_Leg");

	private int m_BBI_BandageLeftHand = Animator.StringToHash("BIBandageLeftHand");

	private int m_BBI_BandageRightHand = Animator.StringToHash("BIBandageRightHand");

	private int m_BBI_BandageLeftLeg = Animator.StringToHash("BIBandageLeftLeg");

	private int m_BBI_BandageRightLeg = Animator.StringToHash("BIBandageRightLeg");

	private int m_BodyInspectionStateHash = Animator.StringToHash("Body Inspection BlendTree");

	public BodyInspectionControllerInputs m_Inputs = new BodyInspectionControllerInputs();

	private float m_MouseSensitivityX = 0.1f;

	private float m_MouseSensitivityY = 0.1f;

	private List<Transform> m_LeftArmRotationBones = new List<Transform>();

	private List<Transform> m_RightArmRotationBones = new List<Transform>();

	private List<Transform> m_LeftLegRotationBones = new List<Transform>();

	private List<Transform> m_RightLegRotationBones = new List<Transform>();

	public BIState m_State;

	[HideInInspector]
	public List<BIWoundSlot> m_WoundSlots = new List<BIWoundSlot>();

	public BIWoundSlot m_SelectedWound;

	public List<BIWoundSlot> m_VisibleSlots = new List<BIWoundSlot>();

	[HideInInspector]
	public float m_AnimatorX;

	[HideInInspector]
	public float m_AnimatorY = 1f;

	public BIWoundSlot m_ActiveSlot;

	public bool m_DelayAnimatorReset;

	private static BodyInspectionController s_Instance;

	private float m_FOV = 60f;

	private float m_StartFOV = 60f;

	private bool m_LeaveAfterBandage;

	private float m_SpineAddLayerBlendWeightToRestore;

	private Camera m_OutlineCamera;

	private AudioSource m_AudioSource;

	private List<AudioClip> m_ArmBandagingSound = new List<AudioClip>();

	private bool m_ControllerEnabled;

	public Item m_CarryingItemDeleech;

	private float m_DistFromScreen = 0.3f;

	private bool m_HideAndShowLimb;

	private float m_HideAndShowProgress;
}
