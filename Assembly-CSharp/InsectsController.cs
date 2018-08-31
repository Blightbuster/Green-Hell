using System;
using Enums;
using UnityEngine;

public class InsectsController : PlayerController
{
	public static InsectsController Get()
	{
		return InsectsController.s_Instance;
	}

	protected override void Awake()
	{
		InsectsController.s_Instance = this;
		base.Awake();
		this.m_ControllerType = PlayerControllerType.Insects;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (this.m_Player.GetCurrentItem(Hand.Right) != null)
		{
			this.m_Player.GetCurrentItem(Hand.Right).gameObject.SetActive(false);
		}
		if (this.m_Player.GetCurrentItem(Hand.Left) != null)
		{
			this.m_Player.GetCurrentItem(Hand.Left).gameObject.SetActive(false);
		}
		if (this.m_Sensor && this.m_Sensor.m_Type == InsectsSensor.Type.Wasps)
		{
			InsectsManager.Get().Activate(InsectsManager.InsectType.Wasp);
			this.m_Animator.SetBool(this.m_BWaspsParam, true);
			this.m_EndTime = Time.time + 3.663f;
		}
		else
		{
			InsectsManager.Get().Activate(InsectsManager.InsectType.Ant);
			this.m_Animator.SetBool(this.m_BInsectsParam, true);
			this.m_EndTime = Time.time + 2.01663f;
		}
		this.m_StartTime = Time.time;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.m_Player.GetCurrentItem(Hand.Right) != null)
		{
			this.m_Player.GetCurrentItem(Hand.Right).gameObject.SetActive(true);
		}
		if (this.m_Player.GetCurrentItem(Hand.Left) != null)
		{
			this.m_Player.GetCurrentItem(Hand.Left).gameObject.SetActive(true);
		}
		this.m_Animator.SetBool(this.m_BInsectsParam, false);
		this.m_Animator.SetBool(this.m_BWaspsParam, false);
		InsectsManager.Get().FlyAway(Hand.Left);
		InsectsManager.Get().FlyAway(Hand.Right);
		this.m_Sensor = null;
		this.m_StartTime = float.MaxValue;
		this.m_EndTime = float.MaxValue;
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		if (this.IsActive() && Time.time > this.m_EndTime)
		{
			this.Stop();
		}
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.ShakingOffDamage)
		{
			Vector3 hit_dir = new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
			hit_dir.Normalize();
			this.m_Player.GiveDamage(null, null, 10f, hit_dir, DamageType.Insects, 0, false);
		}
		else if (id == AnimEventID.ShakingOffEnd)
		{
			this.Stop();
		}
		else if (id == AnimEventID.WaspsFlyAwayLHand)
		{
			InsectsManager.Get().FlyAway(Hand.Left);
		}
		else if (id == AnimEventID.WaspsFlyAwayRHand)
		{
			InsectsManager.Get().FlyAway(Hand.Right);
		}
	}

	private int m_BInsectsParam = Animator.StringToHash("Insects");

	private int m_BWaspsParam = Animator.StringToHash("Wasps");

	private static InsectsController s_Instance;

	[HideInInspector]
	public InsectsSensor m_Sensor;

	private float m_StartTime = float.MaxValue;

	private float m_EndTime = float.MaxValue;
}
