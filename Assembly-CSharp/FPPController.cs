using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

[RequireComponent(typeof(CharacterControllerProxy))]
public class FPPController : PlayerController
{
	public Vector3 m_CharacterVelocity
	{
		get
		{
			return this.m_WantedSpeed.current;
		}
	}

	public Vector2 GetLookDev()
	{
		return this.m_LookController.m_LookDev;
	}

	public void SetLookDev(Vector2 dev)
	{
		this.m_LookController.m_LookDev = dev;
		this.m_LookController.m_WantedLookDev = dev;
	}

	public static FPPController Get()
	{
		return FPPController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		FPPController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.FPP;
		this.m_Inputs = new FPPControllerInputs();
		this.m_CharacterController = base.GetComponent<CharacterControllerProxy>();
		this.m_Height = this.m_CharacterController.height;
		this.m_AudioModule = base.gameObject.GetComponent<PlayerAudioModule>();
		DebugUtils.Assert(this.m_AudioModule, true);
		this.m_LookController = base.gameObject.GetComponent<LookController>();
		this.m_BodyRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:Spine"));
		this.m_BodyRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:Spine1"));
		this.m_BodyRotationBones.Add(base.gameObject.transform.FindDeepChild("mixamorig:Spine2"));
		Quaternion rotation = this.m_CharacterController.transform.rotation;
		this.m_LookController.m_LookDev.x = rotation.eulerAngles.y;
		this.m_CurentMoveSpeed = 0f;
		this.m_WantedSpeed.Init(Vector3.zero, this.m_Acceleration);
		this.m_WantedPos.Init(Vector3.zero, this.m_PositionAcceleration);
		this.m_SlidingSpeedSoft.Init(Vector3.zero, 1f);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_CollisionFlags = CollisionFlags.Below;
		this.m_LastCollisionFlags = CollisionFlags.Below;
		this.m_SkipCompensation = true;
		this.m_WantedPos.Force(base.transform.position);
		this.m_TimeInAir = 0f;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Duck = false;
		this.m_Animator.SetBool(this.m_BDuck, this.m_Duck);
		if (this.m_CharacterController.gameObject.activeSelf)
		{
			this.m_CharacterController.Move(Vector3.zero, true);
		}
		this.DestroyWashParticles();
		this.m_WantedSpeed.Reset();
	}

	public override void ControllerUpdate()
	{
		this.UpdateInputs();
		this.m_Player.UpdateCharacterControllerSizeAndCenter();
		this.UpdateWantedSpeed();
		this.UpdateDuck();
		this.UpdateSprint();
		this.UpdateWashParticles();
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
	}

	private void UpdateWantedSpeed()
	{
		if (!this.IsActive() || (FreeHandsLadderController.Get().IsActive() && !this.m_JumpInProgress))
		{
			this.m_WantedSpeed.Reset();
			this.m_SlidingSpeedSoft.Reset();
			this.m_SlideDir = Vector3.zero;
			this.m_CurentMoveSpeed = 0f;
			this.m_CurrentMoveSpeed = MoveSpeed.Idle;
			return;
		}
		Vector2 vector = new Vector2(this.m_Inputs.m_Horizontal, this.m_Inputs.m_Vertical);
		if (vector.sqrMagnitude > 1f)
		{
			vector.Normalize();
		}
		bool flag = (this.m_CollisionFlags & CollisionFlags.Below) == CollisionFlags.None;
		bool flag2 = !flag && (this.m_CollisionFlagsAddSpeed & CollisionFlags.Below) == CollisionFlags.None;
		bool flag3 = this.m_Inputs.m_Sprint;
		float num = 0f;
		this.m_CurrentMoveSpeed = MoveSpeed.Idle;
		bool flag4 = this.m_Player && this.m_Player.GetMovesBlocked();
		if (this.m_RunDepletedStamina)
		{
			if (flag3)
			{
				flag3 = false;
			}
			else
			{
				this.m_RunDepletedStamina = false;
			}
		}
		if (BowController.Get().m_MaxAim)
		{
			flag3 = false;
		}
		if (flag3 && this.m_RunBlocked)
		{
			flag3 = false;
		}
		if (!flag4 && vector.magnitude > 0.5f)
		{
			if (flag3)
			{
				if (vector.y < -0.5f)
				{
					num = this.m_BackwardWalkSpeed;
					this.m_CurrentMoveSpeed = MoveSpeed.Walk;
				}
				else
				{
					num = this.m_RunSpeed;
					this.m_CurrentMoveSpeed = MoveSpeed.Run;
				}
			}
			else if (vector.magnitude > 0.5f)
			{
				num = ((vector.y >= 0f) ? this.m_WalkSpeed : this.m_BackwardWalkSpeed);
				this.m_CurrentMoveSpeed = MoveSpeed.Walk;
			}
			if (this.m_Player && this.m_Player.IsStaminaDepleted())
			{
				num = this.m_WalkSpeed;
				this.m_CurrentMoveSpeed = MoveSpeed.Walk;
				if (flag3)
				{
					this.m_RunDepletedStamina = true;
				}
			}
		}
		if (this.m_Duck)
		{
			num *= this.m_DuckSpeedMul;
		}
		Vector3 vector2 = this.m_CharacterController.transform.InverseTransformVector(this.m_WantedSpeed.target);
		vector2.x = vector.x * num;
		vector2.z = vector.y * num;
		if (this.m_Dodge)
		{
			num = 10f;
			if (this.m_DodgeDirection != Direction.Backward)
			{
				vector2.x = ((this.m_DodgeDirection == Direction.Right) ? 1f : -1f);
				vector2.z = 0f;
			}
			else
			{
				vector2.x = 0f;
				vector2.z = -1f;
			}
			if (Time.time - this.m_DodgeStartTime > 0.15f)
			{
				this.m_Dodge = false;
			}
		}
		float y = vector2.y;
		vector2.y = 0f;
		vector2.Normalize();
		vector2 *= num;
		vector2.y = y;
		if (InventoryBackpack.Get().IsMaxOverload())
		{
			vector2 *= this.m_MaxOverloadSpeedMul;
		}
		bool flag5 = this.m_TimeInAir < 0.5f;
		if (flag5)
		{
			this.m_WantedSpeed.target = this.m_CharacterController.transform.TransformVector(vector2);
		}
		if (flag2)
		{
			this.m_SlideDir = this.m_AdditionalSpeed.normalized;
		}
		else if (!flag)
		{
			this.m_SlideDir = Vector3.zero;
		}
		if (flag || (!this.m_SlideDir.IsZero() && flag2))
		{
			Vector3 vector3 = this.m_WantedSpeed.target;
			vector3.y = 0f;
			Vector3 slideDir = this.m_SlideDir;
			slideDir.y = 0f;
			if (Vector3.Dot(vector3, slideDir) < 0f)
			{
				Vector3 normalized = Vector3.Cross(slideDir, Vector3.up).normalized;
				vector3 = Vector3.Dot(vector3, normalized) * normalized;
				this.m_WantedSpeed.target.x = vector3.x;
				this.m_WantedSpeed.target.z = vector3.z;
			}
		}
		if (Time.timeScale > 0f)
		{
			if (this.m_SlideDir.IsZero())
			{
				this.m_SlidingSpeedSoft.target = Vector3.zero;
				this.m_SlidingSpeedSoft.omega = this.m_SlideDeceleration;
			}
			else
			{
				float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, this.m_SlideAngle, this.m_CharacterController.slopeLimit, this.m_CharacterController.slopeLimit + 20f);
				this.m_SlidingSpeedSoft.target = this.m_SlideDir * this.m_SlideMaxSpeed * Mathf.Lerp(0.2f, 1f, proportionalClamp);
				this.m_SlidingSpeedSoft.omega = Mathf.Lerp(this.m_SlideAcceleration * 0.1f, this.m_SlideAcceleration, proportionalClamp);
			}
			if (!flag5)
			{
				this.m_WantedSpeed.target = this.m_WantedSpeed.target - this.m_SlidingSpeedSoft;
			}
			this.m_SlidingSpeedSoft.Update(Time.deltaTime / Time.timeScale);
			this.m_WantedSpeed.target = this.m_WantedSpeed.target + this.m_SlidingSpeedSoft;
		}
		if (this.m_Player.m_UseGravity)
		{
			this.m_WantedSpeed.target = this.m_WantedSpeed.target + Physics.gravity * Time.deltaTime;
		}
		else
		{
			this.m_WantedSpeed.current.x = (this.m_WantedSpeed.target.x = 0f);
			this.m_WantedSpeed.current.y = (this.m_WantedSpeed.target.y = 0f);
			this.m_WantedSpeed.current.z = (this.m_WantedSpeed.target.z = 0f);
		}
		if (this.m_WantedSpeed.target.y < -10f)
		{
			this.m_WantedSpeed.target.y = -10f;
		}
		Vector3 b = Vector3.zero;
		if (!this.m_Player.GetMovesBlocked() && this.m_CharacterController.detectCollisions && !this.m_SkipCompensation && !FreeHandsLadderController.Get().IsActive())
		{
			b = -this.m_CharacterController.transform.TransformVector(this.m_Player.m_CharacterControllerDelta);
		}
		b.y = 0f;
		if ((this.m_CollisionFlags & CollisionFlags.Below) == CollisionFlags.None)
		{
			this.m_WantedSpeed.omega = this.m_AccelerationInAir;
			this.m_WantedPos.omega = this.m_AccelerationInAir;
		}
		else if (this.m_Dodge)
		{
			this.m_WantedSpeed.omega = this.m_DodgeAcceleration;
			this.m_WantedPos.omega = this.m_DodgeAcceleration;
		}
		else
		{
			this.m_WantedSpeed.omega = this.m_Acceleration;
			this.m_WantedPos.omega = this.m_PositionAcceleration;
		}
		if (Time.timeScale > 0f)
		{
			this.m_WantedSpeed.Update(Time.deltaTime / Time.timeScale);
		}
		this.m_CurentMoveSpeed = this.m_WantedSpeed.current.To2D().magnitude;
		this.m_CollisionFlagsAddSpeed = CollisionFlags.None;
		this.m_SlideAngle = 0f;
		this.m_AdditionalSpeed = Vector3.zero;
		this.m_CollisionFlags = this.m_CharacterController.Move(this.m_Player.m_SpeedMul * this.m_WantedSpeed.current * Time.deltaTime, false);
		Vector2.zero.Set(this.m_WantedSpeed.current.x, this.m_WantedSpeed.current.z);
		if ((this.m_CollisionFlags & CollisionFlags.Sides) != CollisionFlags.None)
		{
			this.m_LastSideCollisionTime = Time.time;
		}
		if ((this.m_CollisionFlags & CollisionFlags.Below) != CollisionFlags.None && (this.m_LastCollisionFlags & CollisionFlags.Below) == CollisionFlags.None)
		{
			this.m_Player.OnLanding(this.m_CharacterController.velocity);
			this.m_JumpInProgress = false;
		}
		if ((this.m_CollisionFlags & CollisionFlags.Below) != CollisionFlags.None)
		{
			this.m_TimeInAir = 0f;
		}
		else
		{
			this.m_TimeInAir += Time.deltaTime;
		}
		this.m_LastCollisionFlags = this.m_CollisionFlags;
		this.m_SkipCompensation = false;
		this.m_WantedPos.current = base.transform.position;
		this.m_WantedPos.target = this.m_CharacterController.transform.position + b;
		if (Time.timeScale > 0f)
		{
			this.m_WantedPos.Update(Time.deltaTime / Time.timeScale);
		}
		base.transform.position = this.m_WantedPos.current;
	}

	private bool CanCrouch()
	{
		return !Inventory3DManager.Get().IsActive() && !MapController.Get().IsActive() && !NotepadController.Get().IsActive() && !HeavyObjectController.Get().IsActive() && !WalkieTalkieController.Get().IsActive() && !ConstructionController.Get().IsActive() && !HUDItem.Get().m_Active && !HUDWheel.Get().m_Active;
	}

	public override void OnInputAction(InputActionData action_data)
	{
		base.OnInputAction(action_data);
		if (LoadingScreen.Get().m_Active)
		{
			return;
		}
		if (InventoryBackpack.Get().IsMaxOverload())
		{
			return;
		}
		if (action_data.m_Action == InputsManager.InputAction.Jump && HUDMovie.Get().GetMovieType() == MovieType.None)
		{
			bool flag = FreeHandsLadderController.Get().IsActive();
			if ((this.m_CollisionFlags & CollisionFlags.Below) > CollisionFlags.None || flag)
			{
				bool flag2 = InputsManager.Get().IsActionActive(InputsManager.InputAction.DodgeLeft);
				bool flag3 = InputsManager.Get().IsActionActive(InputsManager.InputAction.DodgeRight);
				bool flag4 = InputsManager.Get().IsActionActive(InputsManager.InputAction.DodgeForward);
				bool flag5 = InputsManager.Get().IsActionActive(InputsManager.InputAction.DodgeBackward);
				if (!flag && flag2 && !flag4)
				{
					this.Dodge(Direction.Left);
					return;
				}
				if (!flag && flag3 && !flag4)
				{
					this.Dodge(Direction.Right);
					return;
				}
				if (!flag && flag5)
				{
					this.Dodge(Direction.Backward);
					return;
				}
				this.Jump();
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.Duck && GreenHellGame.Instance.m_Settings.m_ToggleCrouch)
		{
			if (this.CanCrouch())
			{
				this.m_Inputs.m_Duck = !this.m_Inputs.m_Duck;
				this.m_Inputs.m_Sprint = false;
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.Duck && !GreenHellGame.Instance.m_Settings.m_ToggleCrouch)
		{
			if (this.CanCrouch())
			{
				this.m_Inputs.m_Duck = true;
				this.m_Inputs.m_Sprint = false;
				return;
			}
		}
		else
		{
			if (action_data.m_Action == InputsManager.InputAction.DuckStop && !GreenHellGame.Instance.m_Settings.m_ToggleCrouch)
			{
				this.m_Inputs.m_Duck = false;
				return;
			}
			if (action_data.m_Action == InputsManager.InputAction.Sprint && (GreenHellGame.Instance.m_Settings.m_ToggleRunOption == GameSettings.ToggleRunOption.Yes || GreenHellGame.Instance.m_Settings.m_ToggleRunOption == GameSettings.ToggleRunOption.Always))
			{
				this.m_Inputs.m_Sprint = !this.m_Inputs.m_Sprint;
				this.m_Inputs.m_Duck = false;
			}
		}
	}

	private bool CanJump()
	{
		return PlayerConditionModule.Get().m_Stamina >= PlayerConditionModule.Get().GetStaminaDecrease(StaminaDecreaseReason.Jump) && !Player.Get().GetMovesBlocked() && !InsectsController.Get().IsActive() && !ScenarioManager.Get().IsDream() && !Player.Get().m_Animator.GetBool(TriggerController.Get().m_BDrinkWater) && !Player.Get().m_CurrentLift && !ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding");
	}

	private void Jump()
	{
		if (!this.CanJump())
		{
			return;
		}
		this.m_WantedSpeed.target.y = (this.m_WantedSpeed.current.y = this.m_JumpSpeed);
		this.m_Player.DecreaseStamina(StaminaDecreaseReason.Jump);
		EventsManager.OnEvent(Enums.Event.Jump, 1);
		this.m_AudioModule.PlayFeetJumpSound(1f, false);
		this.m_AudioModule.PlayJumpSound();
		if (FishingController.Get().IsActive())
		{
			FishingController.Get().OnJump();
		}
		if (WeaponSpearController.Get().IsActive())
		{
			WeaponSpearController.Get().Jump();
		}
		else
		{
			this.m_Animator.SetTrigger(this.m_TJump);
		}
		this.m_JumpInProgress = true;
	}

	public void ScenarioBlockDodge()
	{
		this.m_ScenarioDodgeBlocked = true;
	}

	public void ScenarioUnblockDodge()
	{
		this.m_ScenarioDodgeBlocked = false;
	}

	private void Dodge(Direction direction)
	{
		if (this.m_ScenarioDodgeBlocked)
		{
			return;
		}
		if (Player.Get().GetMovesBlocked())
		{
			return;
		}
		if (PlayerConditionModule.Get().IsLowStamina())
		{
			return;
		}
		this.m_DodgeDirection = direction;
		this.m_Player.DecreaseStamina(StaminaDecreaseReason.Dodge);
		EventsManager.OnEvent(Enums.Event.Dodge, 1);
		this.m_AudioModule.PlayFeetJumpSound(1f, false);
		this.m_AudioModule.PlayJumpSound();
		this.m_Animator.SetTrigger((direction == Direction.Left) ? this.m_TDodgeRight : this.m_TDodgeRight);
		this.m_DodgeStartTime = Time.time;
		this.m_Dodge = true;
	}

	private void UpdateDuck()
	{
		if (ScenarioManager.Get().IsDream())
		{
			this.m_Animator.SetBool(this.m_BDuck, false);
			return;
		}
		if (base.GetComponent<WeaponController>().IsActive() && base.GetComponent<WeaponController>().DuckDuringAttack())
		{
			this.m_Animator.SetBool(this.m_BDuck, true);
			return;
		}
		this.m_Duck = this.m_Inputs.m_Duck;
		this.m_Animator.SetBool(this.m_BDuck, false);
	}

	private void UpdateSprint()
	{
		if (this.m_Inputs.m_Sprint && !this.IsRunning() && GreenHellGame.Instance.m_Settings.m_ToggleRunOption != GameSettings.ToggleRunOption.Always)
		{
			this.m_Inputs.m_Sprint = false;
		}
	}

	public void UpdateBodyRotation()
	{
		int count = this.m_BodyRotationBones.Count;
		if (count == 0)
		{
			return;
		}
		if (!this.m_Player || !this.m_Player.GetRotationBlocked())
		{
			float xsensitivity = GreenHellGame.Instance.m_Settings.m_XSensitivity;
			float ysensitivity = GreenHellGame.Instance.m_Settings.m_YSensitivity;
			this.m_LookController.UpdateLookDev(this.m_Inputs.m_MouseX * xsensitivity, this.m_Inputs.m_MouseY * ysensitivity);
		}
		Quaternion rotation = default(Quaternion);
		rotation = Quaternion.Euler(0f, this.m_LookController.m_LookDev.x, 0f);
		this.m_CharacterController.transform.rotation = rotation;
		base.transform.rotation = rotation;
		float num = 1f / (float)count;
		for (int i = 0; i < count; i++)
		{
			this.m_BodyRotationBonesParams[this.m_BodyRotationBones[i]] = -this.m_LookController.m_LookDev.y * num;
		}
	}

	public override Dictionary<Transform, float> GetBodyRotationBonesParams()
	{
		return this.m_BodyRotationBonesParams;
	}

	public override void OnBlockRotation()
	{
		base.OnBlockRotation();
		this.m_Inputs.Reset(false);
	}

	private void UpdateInputs()
	{
		if (!Player.Get().GetMovesBlocked())
		{
			this.m_Inputs.m_Vertical = InputsManager.Get().GetActionValue(InputsManager.InputAction.Forward) - InputsManager.Get().GetActionValue(InputsManager.InputAction.Backward);
			this.m_Inputs.m_Vertical = Mathf.Sign(this.m_Inputs.m_Vertical) * (this.m_Inputs.m_Vertical * this.m_Inputs.m_Vertical);
			this.m_Inputs.m_Horizontal = InputsManager.Get().GetActionValue(InputsManager.InputAction.Right) - InputsManager.Get().GetActionValue(InputsManager.InputAction.Left);
			this.m_Inputs.m_Horizontal = Mathf.Sign(this.m_Inputs.m_Horizontal) * (this.m_Inputs.m_Horizontal * this.m_Inputs.m_Horizontal);
			this.m_Inputs.m_Jump = InputsManager.Get().IsActionActive(InputsManager.InputAction.Jump);
			if (GreenHellGame.IsPadControllerActive() && HUDWheel.Get().m_Active)
			{
				this.m_Inputs.m_Duck = false;
			}
			if (GreenHellGame.Instance.m_Settings.m_ToggleRunOption == GameSettings.ToggleRunOption.No)
			{
				this.m_Inputs.m_Sprint = InputsManager.Get().IsActionActive(InputsManager.InputAction.Sprint);
			}
		}
		else if (!MainLevel.Instance.IsPause())
		{
			this.m_Inputs.m_Jump = false;
			this.m_Inputs.m_Sprint = false;
		}
		if (!this.m_Player.GetRotationBlocked())
		{
			Vector2 lookInput = InputHelpers.GetLookInput(this.m_LookSensitivityX, this.m_LookSensitivityY, this.m_PadSensitivity);
			this.m_Inputs.m_MouseX = lookInput.x;
			this.m_Inputs.m_MouseY = lookInput.y;
		}
	}

	public bool IsWalking()
	{
		return this.m_CurrentMoveSpeed == MoveSpeed.Walk;
	}

	public bool IsRunning()
	{
		return this.m_CurrentMoveSpeed == MoveSpeed.Run;
	}

	public bool IsDepleted()
	{
		return this.m_CurrentMoveSpeed == MoveSpeed.Depleted;
	}

	public bool IsIdle()
	{
		return this.m_CurrentMoveSpeed == MoveSpeed.Idle;
	}

	public bool IsDuck()
	{
		return this.m_Duck;
	}

	public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (!base.enabled)
		{
			return;
		}
		Construction componentUpRecursive = General.GetComponentUpRecursive<Construction>(hit.gameObject);
		if (hit.normal.y > 0.9f && componentUpRecursive != null && General.IsRoofConstruction(componentUpRecursive.m_Info.m_ID))
		{
			componentUpRecursive.TakeDamage(new DamageInfo
			{
				m_Damager = this.m_Player.gameObject,
				m_Damage = 1000000f
			});
		}
		Vector3 normal = hit.normal;
		Ramp component = hit.gameObject.GetComponent<Ramp>();
		float num = 57.29578f * Mathf.Abs(Mathf.Acos(normal.y));
		float num2 = (component != null) ? component.m_Angle : this.m_CharacterController.slopeLimit;
		if (num > num2)
		{
			float num3 = 0.3f;
			this.m_AdditionalSpeed.x = this.m_AdditionalSpeed.x + (1f - normal.y) * normal.x * (1f - num3);
			this.m_AdditionalSpeed.z = this.m_AdditionalSpeed.z + (1f - normal.y) * normal.z * (1f - num3);
			this.m_CollisionFlagsAddSpeed |= CollisionFlags.Sides;
		}
		else
		{
			this.m_CollisionFlagsAddSpeed |= CollisionFlags.Below;
		}
		this.m_SlideAngle = Mathf.Max(this.m_SlideAngle, num);
	}

	public MoveSpeed GetCurrentMoveSpeed()
	{
		return this.m_CurrentMoveSpeed;
	}

	public void ScenarioBlockRun()
	{
		this.m_RunBlocked = true;
	}

	public void ScenarioUnblockRun()
	{
		this.m_RunBlocked = false;
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		if (id == AnimEventID.SpawnWashingLHandFX)
		{
			Transform lhand = Player.Get().GetLHand();
			this.m_LHandParticle = ParticlesManager.Get().Spawn("fx_washing_player", lhand.position, Quaternion.identity, Vector3.zero, null, -1f, false);
			return;
		}
		if (id == AnimEventID.SpawnWashingRHandFX)
		{
			Transform rhand = Player.Get().GetRHand();
			this.m_RHandParticle = ParticlesManager.Get().Spawn("fx_washing_player", rhand.position, Quaternion.identity, Vector3.zero, null, -1f, false);
			return;
		}
		if (id == AnimEventID.DestroyWashingLHandFX)
		{
			ParticlesManager.Get().Remove(this.m_LHandParticle);
			this.m_LHandParticle = null;
			return;
		}
		if (id == AnimEventID.DestroyWashingRHandFX)
		{
			ParticlesManager.Get().Remove(this.m_RHandParticle);
			this.m_RHandParticle = null;
		}
	}

	private void UpdateWashParticles()
	{
		if (this.m_LHandParticle)
		{
			Transform lhand = Player.Get().GetLHand();
			this.m_LHandParticle.transform.position = lhand.position;
		}
		if (this.m_RHandParticle)
		{
			Transform rhand = Player.Get().GetRHand();
			this.m_RHandParticle.transform.position = rhand.position;
		}
	}

	public void OnDeactivateHUDItem()
	{
		this.DestroyWashParticles();
	}

	private void DestroyWashParticles()
	{
		if (this.m_LHandParticle)
		{
			ParticlesManager.Get().Remove(this.m_LHandParticle);
			this.m_LHandParticle = null;
		}
		if (this.m_RHandParticle)
		{
			ParticlesManager.Get().Remove(this.m_RHandParticle);
			this.m_RHandParticle = null;
		}
	}

	[SerializeField]
	public float m_WalkSpeed = 4f;

	[SerializeField]
	public float m_BackwardWalkSpeed = 2f;

	[SerializeField]
	public float m_RunSpeed = 8f;

	[SerializeField]
	public float m_DepletedSpeed = 20f;

	[SerializeField]
	public float m_DuckSpeedMul = 0.5f;

	[SerializeField]
	private float m_JumpSpeed = 4.5f;

	private float m_DodgeStartTime;

	private Direction m_DodgeDirection = Direction.Backward;

	private const float m_DodgeSpeed = 10f;

	private const float m_DodgeDuration = 0.15f;

	[HideInInspector]
	public bool m_Dodge;

	[SerializeField]
	private float m_LookSensitivityX = 4f;

	[SerializeField]
	private float m_LookSensitivityY = 2f;

	[SerializeField]
	private float m_PadSensitivity = 150f;

	private CharacterControllerProxy m_CharacterController;

	[HideInInspector]
	public SpringVec3Ex m_WantedSpeed;

	[HideInInspector]
	public SpringVec3Ex m_WantedPos;

	private readonly Vector3 m_Acceleration = new Vector3(40f, 20f, 40f);

	private readonly Vector3 m_AccelerationInAir = new Vector3(20f, 100f, 20f);

	private readonly Vector3 m_PositionAcceleration = new Vector3(20f, 20f, 20f);

	private readonly Vector3 m_DodgeAcceleration = new Vector3(150f, 20f, 150f);

	private bool m_SkipCompensation;

	private CollisionFlags m_CollisionFlags;

	private CollisionFlags m_CollisionFlagsAddSpeed;

	public CollisionFlags m_LastCollisionFlags;

	private float m_LastSideCollisionTime = float.MinValue;

	private FPPControllerInputs m_Inputs;

	private bool m_Duck;

	private float m_Height = 1.8f;

	private List<Transform> m_BodyRotationBones = new List<Transform>();

	private MoveSpeed m_CurrentMoveSpeed;

	[HideInInspector]
	public float m_CurentMoveSpeed;

	private float m_MaxOverloadSpeedMul = 0.3f;

	private PlayerAudioModule m_AudioModule;

	private LookController m_LookController;

	private Dictionary<Transform, float> m_BodyRotationBonesParams = new Dictionary<Transform, float>();

	private Vector3 m_AdditionalSpeed;

	private SpringVec3 m_SlidingSpeedSoft;

	private float m_SlideAcceleration = 1f;

	private float m_SlideDeceleration = 100f;

	private float m_SlideMaxSpeed = 5f;

	private float m_SlideAngle;

	private Vector3 m_SlideDir;

	private static FPPController s_Instance;

	private int m_BDuck = Animator.StringToHash("Duck");

	private int m_TJump = Animator.StringToHash("Jump");

	private int m_TDodgeLeft = Animator.StringToHash("DodgeLeft");

	private int m_TDodgeRight = Animator.StringToHash("DodgeRight");

	private bool m_RunDepletedStamina;

	[HideInInspector]
	public bool m_RunBlocked;

	private bool m_ScenarioDodgeBlocked;

	public float m_TimeInAir;

	private bool m_JumpInProgress;

	private GameObject m_LHandParticle;

	private GameObject m_RHandParticle;
}
