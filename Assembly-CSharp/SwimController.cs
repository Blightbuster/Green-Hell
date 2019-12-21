using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.Audio;

public class SwimController : PlayerController
{
	public static SwimController Get()
	{
		return SwimController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		SwimController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.Swim;
		this.m_LookController = base.gameObject.GetComponent<LookController>();
		this.m_CharacterController = base.GetComponent<CharacterControllerProxy>();
		this.m_SmoothPos.Init(Vector3.zero, this.m_PositionAcceleration);
		if (this.m_ScubaDivingAudioSource == null)
		{
			this.m_ScubaDivingAudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_ScubaDivingAudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
		}
		if (this.m_ScubaDivingBubblesAudioSource == null)
		{
			this.m_ScubaDivingBubblesAudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_ScubaDivingBubblesAudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
		}
		this.m_ScubaDivingExhaleSounds.Clear();
		AudioClip item = (AudioClip)Resources.Load("Sounds/Player/scubadiving_breath_exhale");
		this.m_ScubaDivingExhaleSounds.Add(item);
		this.m_ScubaDivingInhaleSounds.Clear();
		item = (AudioClip)Resources.Load("Sounds/Player/scubadiving_breath_inhale");
		this.m_ScubaDivingInhaleSounds.Add(item);
		this.m_ScubaDivingBubblesSounds.Clear();
		item = (AudioClip)Resources.Load("Sounds/Player/scubadiving_exhale_bubbles_01");
		this.m_ScubaDivingBubblesSounds.Add(item);
		item = (AudioClip)Resources.Load("Sounds/Player/scubadiving_exhale_bubbles_02");
		this.m_ScubaDivingBubblesSounds.Add(item);
		item = (AudioClip)Resources.Load("Sounds/Player/scubadiving_exhale_bubbles_03");
		this.m_ScubaDivingBubblesSounds.Add(item);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Animator.SetBool(this.m_BSwim, true);
		this.m_Player.m_UseGravity = false;
		this.m_DiveBone = this.m_Player.gameObject.transform.FindDeepChild("mixamorig:Spine");
		this.m_SwimBones.Clear();
		this.m_SwimBones.Add(this.m_Player.gameObject.transform.FindDeepChild("mixamorig:Eye.R"));
		this.m_WantedSpeed.Force(this.m_CharacterController.velocity);
		this.m_SwimBonesRotation = this.m_LookController.m_LookDev.y;
		this.m_DiveBonesRotation = 0f;
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().Deactivate();
		}
		this.m_State = SwimState.Swim;
		if (this.m_Player.m_FPPController.m_TimeInAir > 1f)
		{
			this.m_CheckHighSpeed = true;
			PlayerAudioModule.Get().PlayFallIntoWaterSound();
		}
		else
		{
			PlayerAudioModule.Get().PlayFeetLandingSound(1f, false);
		}
		Item currentItem = Player.Get().GetCurrentItem();
		if (currentItem && currentItem.GetInfoID() == ItemID.Fire)
		{
			Player.Get().DropItem(currentItem);
		}
		Player.Get().StopAim();
		this.m_SmoothPos.Force(base.transform.position);
		DialogsManager.Get().StopDialog();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetBool(this.m_BSwim, false);
		this.m_Animator.SetFloat(this.m_FSwimSpeedLong, 0f);
		this.m_Animator.SetFloat(this.m_FSwimSpeedLeft, 0f);
		this.m_Animator.SetFloat(this.m_FSwimSpeedRight, 0f);
		this.m_Player.m_UseGravity = true;
		GreenHellGame instance = GreenHellGame.Instance;
		if (instance != null)
		{
			instance.SetSnapshot(AudioMixerSnapshotGame.Default, 0.5f);
		}
		this.m_State = SwimState.None;
		this.m_HighFallingSpeedLastTime = false;
		this.m_CheckHighSpeed = false;
		this.m_WantedSpeed.Reset();
		this.m_LastDisableTime = Time.time;
	}

	private void UpdateInputs()
	{
		if (this.m_Player.GetRotationBlocked())
		{
			return;
		}
		this.m_Inputs.m_Vertical = 0f;
		this.m_Inputs.m_Horizontal = 0f;
		if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Forward))
		{
			this.m_Inputs.m_Vertical = 1f;
		}
		else if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Backward))
		{
			this.m_Inputs.m_Vertical = -1f;
		}
		if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Left))
		{
			this.m_Inputs.m_Horizontal = -1f;
		}
		else if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Right))
		{
			this.m_Inputs.m_Horizontal = 1f;
		}
		Vector2 lookInput = InputHelpers.GetLookInput(this.m_LookSensitivityX, this.m_LookSensitivityY, 150f);
		this.m_Inputs.m_MouseX = lookInput.x;
		this.m_Inputs.m_MouseY = lookInput.y;
	}

	private void Update()
	{
		this.UpdateInputs();
		this.m_Player.UpdateCharacterControllerSizeAndCenter();
		this.UpdateWantedSpeed();
	}

	private void UpdateWantedSpeed()
	{
		if (!this.IsActive())
		{
			this.m_WantedSpeed.Reset();
			return;
		}
		if (this.m_CheckHighSpeed && this.m_Player.m_Velocity.y < -0.5f)
		{
			Vector3 a = this.m_Player.m_Velocity - this.m_Player.m_Velocity * 0.1f;
			this.m_CharacterController.Move(a * Time.deltaTime, true);
			this.m_HighFallingSpeedLastTime = true;
			return;
		}
		this.m_CheckHighSpeed = false;
		if (this.m_HighFallingSpeedLastTime && !this.ShouldSwim())
		{
			this.SetState(SwimState.Dive);
		}
		this.m_HighFallingSpeedLastTime = false;
		Vector2 zero = Vector2.zero;
		zero.Set(this.m_Inputs.m_Horizontal, this.m_Inputs.m_Vertical);
		Vector3 vector = this.m_CharacterController.transform.InverseTransformVector(this.m_WantedSpeed);
		float waterLevel = this.m_Player.GetWaterLevel();
		this.CalculateSpeedAdd();
		if (this.m_State == SwimState.Swim)
		{
			vector.x = zero.x * 0.8f;
			vector.z = zero.y * 1.4f + this.m_SpeedAddMax * this.m_SpeedAdd;
			Vector3 position = this.m_Player.transform.position;
			Vector3 vector2 = position;
			vector2.y = waterLevel - this.m_PlayerPosOffset;
			this.m_DestPosY = vector2.y;
			this.m_PosY = position.y;
			Vector3 vector3 = vector2 - position;
			vector.y = vector3.y;
		}
		else if (this.m_State == SwimState.Dive)
		{
			Vector3 vector4 = new Vector3(0f, 0f, 1f);
			vector4 = Quaternion.Euler(-this.m_LookController.m_LookDev.y, 0f, 0f) * vector4;
			vector4 *= zero.y * 1.4f;
			vector4.x = zero.x * 0.8f;
			vector = vector4;
			Vector3 vector5 = vector4;
			vector5.x = 0f;
			vector += vector5.normalized * this.m_SpeedAddMax * this.m_SpeedAdd;
		}
		Vector3 a2 = this.m_CharacterController.transform.TransformVector(vector);
		this.m_WantedSpeed.target = this.m_WantedSpeed.target + (a2 - this.m_WantedSpeed) * (Time.deltaTime * 3f);
		if (this.m_State == SwimState.Swim)
		{
			this.m_WantedSpeed.target.y = this.m_DestPosY - this.m_PosY;
		}
		if (this.m_WantedSpeed.target.sqrMagnitude > this.m_WantedSpeed.current.sqrMagnitude)
		{
			this.m_WantedSpeed.omega = this.m_Acceleration;
		}
		else
		{
			this.m_WantedSpeed.omega = this.m_Deceleration;
		}
		this.m_WantedSpeed.Update(Time.deltaTime);
		this.m_CharacterController.Move(this.m_WantedSpeed.current * Time.deltaTime, false);
		this.m_SmoothPos.current = base.transform.position;
		this.m_SmoothPos.target = this.m_CharacterController.transform.position;
		if (Time.timeScale > 0f)
		{
			this.m_SmoothPos.Update(Time.deltaTime / Time.timeScale);
		}
		base.transform.position = this.m_SmoothPos.current;
		base.transform.rotation = this.m_CharacterController.transform.rotation;
	}

	private void CalculateSpeedAdd()
	{
		if (Time.time < this.m_SpeedAddStartTime || Time.time > this.m_SpeedAddStartTime + this.m_SpeedAddPeriod)
		{
			this.m_SpeedAdd = 0f;
			return;
		}
		this.m_SpeedAdd = Mathf.Sin((Time.time - this.m_SpeedAddStartTime) * 3.14159274f / this.m_SpeedAddPeriod);
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateState();
		this.UpdateAnimator();
		this.UpdateScubaDivingSounds();
	}

	private void UpdateState()
	{
		if (this.m_State == SwimState.Swim)
		{
			if (this.ShouldDive() && Time.time - this.m_StateChangedTime > this.m_StateChangeCooldown)
			{
				this.SetState(SwimState.Dive);
				return;
			}
		}
		else if (this.m_State == SwimState.Dive && this.ShouldSwim() && Time.time - this.m_StateChangedTime > this.m_StateChangeCooldown)
		{
			this.SetState(SwimState.Swim);
		}
	}

	private void SetState(SwimState state)
	{
		this.m_State = state;
		this.m_StateChangedTime = Time.time;
		this.OnSetState(state);
	}

	private void OnSetState(SwimState state)
	{
		if (state == SwimState.Dive)
		{
			PlayerAudioModule.Get().PlayBeforeDivingSound();
			this.UpdateAudioMixer(true);
			GreenHellGame.Instance.SetSnapshot(AudioMixerSnapshotGame.Underwater, 0.5f);
			return;
		}
		if (state == SwimState.Swim)
		{
			this.UpdateAudioMixer(false);
			GreenHellGame.Instance.SetSnapshot(AudioMixerSnapshotGame.Default, 0.5f);
			PlayerAudioModule.Get().PlayAfterDivingSound();
		}
	}

	private bool ShouldSwim()
	{
		return this.m_Player.transform.position.y > this.m_Player.GetWaterLevel() - this.m_PlayerPosOffset;
	}

	private bool ShouldDive()
	{
		return this.m_LookController.m_LookDev.y < -this.m_MinAngleToDive && this.m_Inputs.m_Vertical > 0.5f;
	}

	private void UpdateAudioMixer(bool m_IsUnderWater)
	{
		if (m_IsUnderWater && this.m_AudioMixer != null)
		{
			this.m_AudioMixer.SetFloat("MyExposedParam", 2000f);
			return;
		}
		if (!m_IsUnderWater && this.m_AudioMixer != null)
		{
			this.m_AudioMixer.SetFloat("MyExposedParam", 22000f);
		}
	}

	private void UpdateAnimator()
	{
		if (this.m_Inputs.m_Vertical > 0.5f)
		{
			this.m_Animator.SetFloat(this.m_FSwimSpeedLong, 1.4f);
			this.m_Animator.SetFloat(this.m_FSwimSpeedLeft, 0f);
			this.m_Animator.SetFloat(this.m_FSwimSpeedRight, 0f);
			return;
		}
		if (this.m_Inputs.m_Vertical < -0.5f)
		{
			this.m_Animator.SetFloat(this.m_FSwimSpeedLong, -1.4f);
			this.m_Animator.SetFloat(this.m_FSwimSpeedLeft, 0f);
			this.m_Animator.SetFloat(this.m_FSwimSpeedRight, 0f);
			return;
		}
		this.m_Animator.SetFloat(this.m_FSwimSpeedLong, 0f);
		if (this.m_Inputs.m_Horizontal > 0.5f)
		{
			this.m_Animator.SetFloat(this.m_FSwimSpeedRight, 0.8f);
			this.m_Animator.SetFloat(this.m_FSwimSpeedLeft, 0f);
			return;
		}
		if (this.m_Inputs.m_Horizontal < -0.5f)
		{
			this.m_Animator.SetFloat(this.m_FSwimSpeedRight, 0f);
			this.m_Animator.SetFloat(this.m_FSwimSpeedLeft, 0.8f);
			return;
		}
		this.m_Animator.SetFloat(this.m_FSwimSpeedRight, 0f);
		this.m_Animator.SetFloat(this.m_FSwimSpeedLeft, 0f);
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		this.UpdateBodyRotation();
		this.UpdateEyesPosition();
	}

	private void UpdateEyesPosition()
	{
		Transform leyeTransform = this.m_Player.GetLEyeTransform();
		Transform reyeTransform = this.m_Player.GetREyeTransform();
		this.m_WantedLEyePos = leyeTransform.position;
		this.m_WantedREyePos = reyeTransform.position;
		if (this.m_State == SwimState.Dive)
		{
			float num = this.m_Player.GetWaterLevel() - 0.09f;
			if (this.m_WantedLEyePos.y > num)
			{
				this.m_WantedLEyePos.y = num;
			}
			if (this.m_WantedREyePos.y > num)
			{
				this.m_WantedREyePos.y = num;
			}
		}
		leyeTransform.position = this.m_WantedLEyePos;
		reyeTransform.position = this.m_WantedREyePos;
	}

	private void UpdateBodyRotation()
	{
		if (!this.m_Player || !this.m_Player.GetRotationBlocked())
		{
			float num = this.m_LookController.m_WantedLookDev.y + this.m_Inputs.m_MouseY;
			if (this.m_State == SwimState.Swim && num < -(this.m_MinAngleToDive + 2f))
			{
				float num2 = -(this.m_MinAngleToDive + 2f) - num;
				this.m_Inputs.m_MouseY += num2;
			}
			this.m_LookController.UpdateLookDev(this.m_Inputs.m_MouseX, this.m_Inputs.m_MouseY);
		}
		Quaternion rotation = default(Quaternion);
		rotation = Quaternion.Euler(0f, this.m_LookController.m_LookDev.x, 0f);
		this.m_CharacterController.transform.rotation = rotation;
		float num3 = 1f / (float)this.m_SwimBones.Count;
		if (this.m_State == SwimState.Swim)
		{
			this.m_SwimBonesRotation += (this.m_LookController.m_LookDev.y - this.m_SwimBonesRotation) * 0.1f;
			this.m_DiveBonesRotation *= 0.9f;
		}
		else if (this.m_State == SwimState.Dive)
		{
			this.m_SwimBonesRotation *= 0.9f;
			this.m_DiveBonesRotation += (this.m_LookController.m_LookDev.y - this.m_DiveBonesRotation) * 0.1f;
		}
		for (int i = 0; i < this.m_SwimBones.Count; i++)
		{
			this.m_BodyRotationBonesParams[this.m_SwimBones[i]] = -this.m_SwimBonesRotation * num3;
		}
		this.m_BodyRotationBonesParams[this.m_DiveBone] = -this.m_DiveBonesRotation;
	}

	public override void OnBlockRotation()
	{
		base.OnBlockRotation();
		this.m_Inputs.Reset();
	}

	public SwimState GetState()
	{
		return this.m_State;
	}

	public override Dictionary<Transform, float> GetBodyRotationBonesParams()
	{
		return this.m_BodyRotationBonesParams;
	}

	public bool IsSwimming()
	{
		return base.enabled && this.m_WantedSpeed.current.magnitude > 0.1f;
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (this.m_State == SwimState.Dive && (id == AnimEventID.DiveSound0 || id == AnimEventID.DiveSound1 || id == AnimEventID.DiveSound2))
		{
			PlayerAudioModule.Get().PlayDiveSound();
			if (this.m_Inputs.m_Vertical > 0.5f)
			{
				this.m_SpeedAddStartTime = Time.time;
				return;
			}
		}
		else if (this.m_State == SwimState.Swim && (id == AnimEventID.SwimSound0 || id == AnimEventID.SwimSound1 || id == AnimEventID.SwimSound2))
		{
			PlayerAudioModule.Get().PlaySwimSound();
			this.SpawnSwimHandFXs();
			if (this.m_Inputs.m_Vertical > 0.5f)
			{
				this.m_SpeedAddStartTime = Time.time;
			}
		}
	}

	private void SpawnSwimHandFXs()
	{
		Vector3 position = this.m_Player.GetLHand().position;
		float y = this.m_Player.GetWaterLevel() + 0.01f;
		float y2 = this.m_Player.GetWaterLevel() + 0.02f;
		position.y = y;
		ParticlesManager.Get().Spawn("water_splashes_foam_player_hand", position, Quaternion.identity, this.m_WantedSpeed, this.m_Player.transform, 1f, true);
		position.y = y2;
		ParticlesManager.Get().Spawn("water_splashes_player_hand_a", position, Quaternion.identity, this.m_WantedSpeed, this.m_Player.transform, 1f, true);
		position = this.m_Player.GetRHand().position;
		position.y = y;
		ParticlesManager.Get().Spawn("water_splashes_foam_player_hand", position, Quaternion.identity, this.m_WantedSpeed, this.m_Player.transform, 1f, true);
		position.y = y2;
		ParticlesManager.Get().Spawn("water_splashes_player_hand_a", position, Quaternion.identity, this.m_WantedSpeed, this.m_Player.transform, 1f, true);
	}

	private void UpdateBodyFXs()
	{
		if (Time.time - this.m_BodyFXSpawnLastTime > this.m_BodyFXSpawnCooldown)
		{
			Vector3 position = this.m_Player.GetSpine1().position;
			float y = this.m_Player.GetWaterLevel() + 0.01f;
			float y2 = this.m_Player.GetWaterLevel() + 0.02f;
			position.y = y;
			ParticlesManager.Get().Spawn("water_splashes_foam_player", position, Quaternion.identity, this.m_WantedSpeed, this.m_Player.transform, 1f, true);
			position.y = y2;
			ParticlesManager.Get().Spawn("water_waves_lux_projector", position, Quaternion.identity, this.m_WantedSpeed, this.m_Player.transform, 1f, true);
			this.m_BodyFXSpawnLastTime = Time.time;
		}
	}

	private void UpdateScubaDivingSounds()
	{
		if (Player.Get().m_InfinityDiving && this.m_State == SwimState.Dive)
		{
			if (this.m_NextScubaDivingSoundType == SwimController.ScubaDivingSoundType.Inhale)
			{
				if (Time.time - this.m_ScubaDivingSoundLastTime > 3f)
				{
					this.m_ScubaDivingAudioSource.clip = this.m_ScubaDivingInhaleSounds[UnityEngine.Random.Range(0, this.m_ScubaDivingInhaleSounds.Count)];
					this.m_ScubaDivingAudioSource.Play();
					this.m_ScubaDivingSoundLastTime = Time.time;
					this.m_NextScubaDivingSoundType = SwimController.ScubaDivingSoundType.Exhale;
					return;
				}
			}
			else if (this.m_NextScubaDivingSoundType == SwimController.ScubaDivingSoundType.Exhale && Time.time - this.m_ScubaDivingSoundLastTime > 3f)
			{
				this.m_ScubaDivingAudioSource.clip = this.m_ScubaDivingExhaleSounds[UnityEngine.Random.Range(0, this.m_ScubaDivingExhaleSounds.Count)];
				this.m_ScubaDivingAudioSource.Play();
				this.m_ScubaDivingSoundLastTime = Time.time;
				this.m_NextScubaDivingSoundType = SwimController.ScubaDivingSoundType.Inhale;
				this.m_ScubaDivingBubblesAudioSource.clip = this.m_ScubaDivingBubblesSounds[UnityEngine.Random.Range(0, this.m_ScubaDivingBubblesSounds.Count)];
				this.m_ScubaDivingBubblesAudioSource.Play();
				return;
			}
		}
		else
		{
			if (this.m_ScubaDivingAudioSource.isPlaying)
			{
				this.m_ScubaDivingAudioSource.Stop();
			}
			if (this.m_ScubaDivingBubblesAudioSource.isPlaying)
			{
				this.m_ScubaDivingBubblesAudioSource.Stop();
			}
			this.m_NextScubaDivingSoundType = SwimController.ScubaDivingSoundType.Inhale;
			this.m_ScubaDivingSoundLastTime = 0f;
		}
	}

	private const float SWIM_SPEED_FORWARD = 1.4f;

	private const float SWIM_SPEED_BACKWARD = 1.4f;

	private const float SWIM_SPEED_LEFT = 0.8f;

	private const float SWIM_SPEED_RIGHT = 0.8f;

	public SwimState m_State;

	private LookController m_LookController;

	private CharacterControllerProxy m_CharacterController;

	private SwimControllerInputs m_Inputs = new SwimControllerInputs();

	[SerializeField]
	private float m_LookSensitivityX = 2f;

	[SerializeField]
	private float m_LookSensitivityY = 2f;

	private SpringVec3Ex m_WantedSpeed;

	private readonly Vector3 m_Acceleration = new Vector3(20f, 20f, 20f);

	private readonly Vector3 m_Deceleration = new Vector3(10f, 20f, 10f);

	private SpringVec3Ex m_SmoothPos;

	private readonly Vector3 m_PositionAcceleration = new Vector3(20f, 20f, 20f);

	private Transform m_DiveBone;

	private List<Transform> m_SwimBones = new List<Transform>();

	private float m_SwimBonesRotation;

	private float m_DiveBonesRotation;

	private float m_MinAngleToDive = 35f;

	private Dictionary<Transform, float> m_BodyRotationBonesParams = new Dictionary<Transform, float>();

	private int m_BSwim = Animator.StringToHash("Swim");

	private int m_FSwimSpeedLong = Animator.StringToHash("SwimSpeedLong");

	private int m_FSwimSpeedLeft = Animator.StringToHash("SwimSpeedLeft");

	private int m_FSwimSpeedRight = Animator.StringToHash("SwimSpeedRight");

	private static SwimController s_Instance;

	public AudioMixer m_AudioMixer;

	[HideInInspector]
	public bool m_Debug;

	private float m_SpeedAdd;

	private float m_SpeedAddMax = 1f;

	private float m_SpeedAddPeriod = 1f;

	private float m_SpeedAddStartTime = float.MinValue;

	private List<AudioClip> m_ScubaDivingInhaleSounds = new List<AudioClip>();

	private List<AudioClip> m_ScubaDivingExhaleSounds = new List<AudioClip>();

	private List<AudioClip> m_ScubaDivingBubblesSounds = new List<AudioClip>();

	[HideInInspector]
	public float m_LastDisableTime;

	private float m_DestPosY;

	private float m_PosY;

	private float m_PlayerPosOffset = 1.55f;

	private bool m_CheckHighSpeed;

	private bool m_HighFallingSpeedLastTime;

	private float m_StateChangedTime;

	private float m_StateChangeCooldown = 0.5f;

	private Vector3 m_WantedLEyePos = Vector3.zero;

	private Vector3 m_WantedREyePos = Vector3.zero;

	private float m_BodyFXSpawnLastTime = float.MinValue;

	private float m_BodyFXSpawnCooldown = 3f;

	private AudioSource m_ScubaDivingAudioSource;

	private AudioSource m_ScubaDivingBubblesAudioSource;

	private SwimController.ScubaDivingSoundType m_NextScubaDivingSoundType;

	private float m_ScubaDivingSoundLastTime;

	private enum ScubaDivingSoundType
	{
		Inhale,
		Exhale
	}
}
