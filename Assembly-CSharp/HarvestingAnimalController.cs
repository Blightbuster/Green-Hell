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
		this.m_ControllerType = PlayerControllerType.HarvestingAnimal;
		GameObject prefab = GreenHellGame.Instance.GetPrefab("Stone_Blade");
		this.m_StoneBlade = UnityEngine.Object.Instantiate<GameObject>(prefab);
		Weapon component = this.m_StoneBlade.GetComponent<Weapon>();
		if (component != null)
		{
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
		Collider component = Player.Get().gameObject.GetComponent<Collider>();
		Physics.IgnoreCollision(component, this.m_Body.m_BoxCollider);
		if (this.m_Body.m_RagdollBones != null)
		{
			foreach (Transform transform in this.m_Body.m_RagdollBones)
			{
				Physics.IgnoreCollision(component, transform.GetComponent<Collider>());
			}
		}
		this.m_Animator.SetTrigger(this.m_HarvestingHash);
		this.m_StoneBlade.SetActive(true);
		this.m_StoneBlade.transform.parent = this.m_Player.GetRHand();
		this.m_Player.m_AudioModule.PlayHarvestAnimalSound();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		LookController.Get().m_WantedLookDev.y = 0f;
		this.m_Player.m_AudioModule.StopHarvestAnimalSound();
		this.m_StoneBlade.SetActive(false);
		if (this.m_Animator.isInitialized)
		{
			this.m_Animator.ResetTrigger(this.m_HarvestingHash);
			this.m_Animator.ResetTrigger(this.m_HarvestingFinishHash);
		}
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		Vector3 a = this.m_Body.transform.TransformPoint(this.m_Body.m_BoxCollider.center) - base.transform.position;
		if (a.magnitude > 0.5f)
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
	}

	public void SetBody(DeadBody body)
	{
		this.m_Body = body;
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
			}
		}
		else if (id == AnimEventID.HarvestingFinishEnd)
		{
			this.m_Body.Harvest();
			this.Stop();
		}
		else if (id == AnimEventID.HarvestingSpawnFX)
		{
			ParticlesManager.Get().Spawn("Animal Harvest", Player.Get().GetRHand().position, Player.Get().GetRHand().rotation, null);
		}
	}

	private int m_HarvestingHash = Animator.StringToHash("HarvestingAnimal");

	private int m_HarvestingFinishHash = Animator.StringToHash("HarvestingAnimalFinish");

	private DeadBody m_Body;

	private int m_AnimLoops;

	private static HarvestingAnimalController s_Instance;

	private GameObject m_StoneBlade;

	private Transform m_StoneBladeHolder;
}
