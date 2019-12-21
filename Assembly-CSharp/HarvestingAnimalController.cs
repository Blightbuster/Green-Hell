using System;
using System.Collections.Generic;
using AIs;
using CJTools;
using Enums;
using UnityEngine;

public class HarvestingAnimalController : PlayerController
{
	public static HarvestingAnimalController Get()
	{
		return HarvestingAnimalController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HarvestingAnimalController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.HarvestingAnimal;
		GameObject prefab = GreenHellGame.Instance.GetPrefab("Stone_Blade");
		this.m_StoneBlade = UnityEngine.Object.Instantiate<GameObject>(prefab);
		Weapon component = this.m_StoneBlade.GetComponent<Weapon>();
		if (component != null)
		{
			if (component.GetComponent<ReplicationComponent>())
			{
				component.GetComponent<ReplicationComponent>().enabled = false;
			}
			UnityEngine.Object.Destroy(component);
		}
		TrailRenderer component2 = this.m_StoneBlade.GetComponent<TrailRenderer>();
		if (component2 != null)
		{
			UnityEngine.Object.Destroy(component2);
		}
		this.m_StoneBlade.SetActive(false);
		this.m_StoneBladeHolder = this.m_StoneBlade.transform.FindDeepChild("Holder");
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_AnimLoops = 0;
		Collider collider = Player.Get().m_Collider;
		Physics.IgnoreCollision(collider, this.m_Body.m_BoxCollider);
		if (this.m_Body.m_RagdollBones != null)
		{
			foreach (Rigidbody rigidbody in this.m_Body.m_RagdollBones)
			{
				Physics.IgnoreCollision(collider, rigidbody.gameObject.GetComponent<Collider>());
			}
		}
		this.m_Animator.SetBool(this.m_HarvestingHash, true);
		this.m_StoneBlade.SetActive(true);
		this.m_StoneBlade.transform.parent = this.m_Player.GetRHand();
		this.m_Player.m_AudioModule.PlayHarvestAnimalSound();
		this.m_CorrectPosition = true;
		this.m_End = false;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		LookController.Get().m_WantedLookDev.y = 0f;
		this.m_Player.m_AudioModule.StopHarvestAnimalSound();
		this.m_StoneBlade.SetActive(false);
		if (this.m_Animator.isInitialized)
		{
			this.m_Animator.ResetTrigger(this.m_HarvestingFinishHash);
			this.m_Animator.ResetTrigger(this.m_HarvestingAnimalEndHash);
			this.m_Animator.SetBool(this.m_HarvestingHash, false);
		}
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		if (this.m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == this.m_CutInHash)
		{
			this.m_Animator.SetBool(this.m_HarvestingHash, false);
		}
		Vector3 a = this.m_Body.transform.TransformPoint(this.m_Body.m_BoxCollider.center) - base.transform.position;
		if (a.magnitude > 0.5f && this.m_CorrectPosition)
		{
			base.transform.position += a * Time.deltaTime;
		}
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		Transform rhand = this.m_Player.GetRHand();
		Quaternion rhs = Quaternion.Inverse(this.m_StoneBladeHolder.localRotation);
		this.m_StoneBlade.transform.rotation = rhand.rotation;
		this.m_StoneBlade.transform.rotation *= rhs;
		Vector3 b = this.m_StoneBladeHolder.parent.position - this.m_StoneBladeHolder.position;
		this.m_StoneBlade.transform.position = rhand.position;
		this.m_StoneBlade.transform.position += b;
		if (this.m_End && this.m_Animator.GetCurrentAnimatorStateInfo(1).shortNameHash != this.m_AnimalharvestmaskHash)
		{
			this.m_Body.Harvest();
			this.Stop();
			this.m_CorrectPosition = false;
		}
	}

	public void SetBody(DeadBody body)
	{
		this.m_Body = body;
	}

	public DeadBody GetBody()
	{
		return this.m_Body;
	}

	public override Dictionary<Transform, float> GetBodyRotationBonesParams()
	{
		return null;
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		if (id == AnimEventID.HarvestingEnd)
		{
			this.m_AnimLoops++;
			if (this.m_AnimLoops == Skill.Get<HarvestingAnimalsSkill>().GetAnimationsCount())
			{
				this.m_Animator.SetTrigger(this.m_HarvestingFinishHash);
				return;
			}
		}
		else
		{
			if (id == AnimEventID.HarvestingFinishEnd)
			{
				this.m_Animator.SetTrigger(this.m_HarvestingAnimalEndHash);
				this.m_End = true;
				return;
			}
			if (id == AnimEventID.HarvestingSpawnFX)
			{
				ParticlesManager.Get().Spawn("Animal Harvest", Camera.main.transform.position + Camera.main.transform.forward * 0.1f + Vector3.down * 0.5f, Player.Get().transform.rotation, Vector3.zero, null, -1f, false);
			}
		}
	}

	public bool BlockInventoryInputs()
	{
		return base.enabled;
	}

	private int m_CutInHash = Animator.StringToHash("CutIn");

	private int m_HarvestingHash = Animator.StringToHash("HarvestingAnimal");

	private int m_HarvestingFinishHash = Animator.StringToHash("HarvestingAnimalFinish");

	private int m_HarvestingAnimalEndHash = Animator.StringToHash("HarvestingAnimalEnd");

	private int m_AnimalharvestmaskHash = Animator.StringToHash("Animalharvestmask");

	private DeadBody m_Body;

	private int m_AnimLoops;

	private static HarvestingAnimalController s_Instance;

	private GameObject m_StoneBlade;

	private Transform m_StoneBladeHolder;

	private bool m_CorrectPosition;

	private bool m_End;
}
