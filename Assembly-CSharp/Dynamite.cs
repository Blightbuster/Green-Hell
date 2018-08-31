using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class Dynamite : Construction
{
	protected override void Start()
	{
		base.Start();
		this.ExplodeLight.intensity = 0f;
	}

	public override bool CanTrigger()
	{
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		if (!currentItem)
		{
			return false;
		}
		ItemID id = currentItem.m_Info.m_ID;
		if (id == ItemID.Torch || id == ItemID.Weak_Torch || id == ItemID.Tobacco_Torch)
		{
			Torch torch = (Torch)currentItem;
			return torch.m_Burning;
		}
		return false;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		actions.Add(TriggerAction.TYPE.Ignite);
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Ignite)
		{
			TorchController.Get().OnDynamiteIgnite(this);
		}
	}

	public void StartBurning()
	{
		this.offset = 0f;
		this.BurningFuseAudio.Play();
		this.FuseCentre.SetActive(true);
		this.FuseLight.intensity = UnityEngine.Random.Range(0.2f, 0.4f);
		this.m_State = Dynamite.State.Burning;
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_State == Dynamite.State.Burning)
		{
			this.UpdateBurning();
		}
		else if (this.m_State == Dynamite.State.Explosion)
		{
			this.UpdateExplosion();
		}
	}

	private void UpdateBurning()
	{
		if (this.offset < 0.43f)
		{
			this.offset += Time.deltaTime * 0.11f;
			this.FuseObject.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(this.offset, 0f));
			return;
		}
		this.Explosion();
	}

	private void Explosion()
	{
		this.FuseCentre.SetActive(false);
		this.Bomb.SetActive(false);
		this.BombExplode.SetActive(true);
		this.ExplodeVideoParticles.Play();
		this.SparkParticles.Play();
		this.SparkTrailsParticles.Play();
		this.ExplodeAudio.Play();
		this.m_State = Dynamite.State.Explosion;
		this.explosion_duration = 0f;
		float b = Player.Get().transform.position.Distance(base.transform.position);
		float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, this.m_Damage, b, this.m_DamageRange, 0f);
		if (proportionalClamp > 0f)
		{
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.m_Damage = proportionalClamp;
			damageInfo.m_HitDir = (Player.Get().transform.position - base.transform.position).normalized;
			Player.Get().TakeDamage(damageInfo);
		}
	}

	private void UpdateExplosion()
	{
		if (this.explosion_duration < this.fadeTime)
		{
			this.explosion_duration += Time.deltaTime;
			this.ExplodeLight.intensity = Mathf.Lerp(this.fadeStart, this.fadeEnd, this.explosion_duration / this.fadeTime);
		}
		if (this.explosion_duration > 5f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public Dynamite.State m_State;

	public GameObject FuseCentre;

	public Light FuseLight;

	public ParticleSystem ExplodeVideoParticles;

	public ParticleSystem SparkTrailsParticles;

	public ParticleSystem SparkParticles;

	public AudioSource ExplodeAudio;

	public GameObject Bomb;

	public GameObject FuseObject;

	public GameObject BombExplode;

	public AudioSource BurningFuseAudio;

	public Light ExplodeLight;

	private float offset;

	private float fadeStart = 1.5f;

	private float fadeEnd;

	private float fadeTime = 1f;

	private float explosion_duration;

	public float m_Damage;

	public float m_DamageRange;

	public enum State
	{
		None,
		Burning,
		Explosion
	}
}
