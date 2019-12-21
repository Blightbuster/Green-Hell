using System;
using CJTools;
using Enums;
using UnityEngine;

public class PlayerPostProcessModule : PlayerModule
{
	public static PlayerPostProcessModule Get()
	{
		return PlayerPostProcessModule.s_Instance;
	}

	private void Awake()
	{
		PlayerPostProcessModule.s_Instance = this;
	}

	public override void OnTakeDamage(DamageInfo info)
	{
		base.OnTakeDamage(info);
		if (info.m_Blocked)
		{
			return;
		}
		if (info.m_DamageType != DamageType.Insects && info.m_DamageType != DamageType.Cut && info.m_DamageType != DamageType.Thrust && info.m_DamageType != DamageType.None)
		{
			this.m_LastDamageTime = Time.time;
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Blood, 1f);
		}
	}

	public void StartPPInterpolator(string i1, string i2, string def)
	{
		PostProcessObject[] array = Resources.FindObjectsOfTypeAll<PostProcessObject>();
		PostProcessObject postProcessObject = null;
		PostProcessObject postProcessObject2 = null;
		this.m_DefaultEffect = (PostProcessManager.Effect)Enum.Parse(typeof(PostProcessManager.Effect), def);
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].name == i1)
			{
				postProcessObject = array[j];
				break;
			}
		}
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k].name == i2)
			{
				postProcessObject2 = array[k];
				break;
			}
		}
		DebugUtils.Assert(postProcessObject && postProcessObject2, true);
		this.m_PPInterpolatorObjects[0] = postProcessObject;
		this.m_PPInterpolatorObjects[1] = postProcessObject2;
		postProcessObject.m_MaxRadius = postProcessObject.transform.position.Distance(postProcessObject2.transform.position);
		postProcessObject2.m_MaxRadius = postProcessObject.transform.position.Distance(postProcessObject2.transform.position);
		this.m_InterpolatePP = true;
	}

	public void StopPPInterpolator()
	{
		this.m_PPInterpolatorObjects[0] = null;
		this.m_PPInterpolatorObjects[1] = null;
		this.m_InterpolatePP = false;
	}

	private void InterpolatePP()
	{
		PostProcessManager.Effect effect = this.m_PPInterpolatorObjects[0].m_Effect;
		float b = Player.Get().transform.position.Distance(this.m_PPInterpolatorObjects[0].transform.position);
		PostProcessManager.Effect effect2 = this.m_PPInterpolatorObjects[1].m_Effect;
		float b2 = Player.Get().transform.position.Distance(this.m_PPInterpolatorObjects[1].transform.position);
		float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, b, this.m_PPInterpolatorObjects[0].m_MaxRadius, this.m_PPInterpolatorObjects[0].m_Radius);
		float proportionalClamp2 = CJTools.Math.GetProportionalClamp(0f, 1f, b2, this.m_PPInterpolatorObjects[1].m_MaxRadius, this.m_PPInterpolatorObjects[1].m_Radius);
		PostProcessManager.Get().SetWeight(effect, proportionalClamp);
		PostProcessManager.Get().SetWeight(effect2, proportionalClamp2);
		PostProcessManager.Get().SetWeight(this.m_DefaultEffect, 1f - (proportionalClamp + proportionalClamp2));
		for (int i = 0; i < 12; i++)
		{
			PostProcessManager.Effect effect3 = (PostProcessManager.Effect)i;
			if (effect3 != effect && effect3 != effect2 && effect3 != this.m_DefaultEffect)
			{
				PostProcessManager.Get().SetWeight(effect3, 0f);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (this.m_InterpolatePP)
		{
			this.InterpolatePP();
			return;
		}
		this.UpdateGamePP();
		this.UpdateDamagePP();
		this.UpdatePoisonPP();
		this.UpdateSanityPP();
		this.UpdateNotepadPP();
		this.UpdateLowHPPP();
		this.UpdateLowEnergyPP();
		this.UpdateUnderwaterPP();
		this.UpdateDreamPP();
		this.UpdateDebugDofPP();
	}

	private void UpdateGamePP()
	{
		float num = Player.Get().GetComponent<NotepadController>().IsActive() ? 0f : 1f;
		num = (((float)PlayerSanityModule.Get().m_Sanity < 10f) ? 0f : num);
		num = (Player.Get().IsCameraUnderwater() ? 0f : num);
		num = (Player.Get().m_DreamPPActive ? 0f : num);
		float weight = PostProcessManager.Get().GetWeight(PostProcessManager.Effect.Game);
		if (weight != num)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Game, weight + (num - weight) * Time.deltaTime * 2f);
		}
	}

	private void UpdateDamagePP()
	{
		float num = 0f;
		float weight = PostProcessManager.Get().GetWeight(PostProcessManager.Effect.Blood);
		if (PlayerConditionModule.Get().IsHPCriticalLevel())
		{
			num = CJTools.Math.GetProportionalClamp(0f, 1f, PlayerConditionModule.Get().GetHP(), PlayerConditionModule.Get().m_CriticalLevel, 0f);
		}
		if (this.m_LastDamageTime > 0f && Time.time - this.m_LastDamageTime < this.m_DamageEffectDuration)
		{
			num = 1f;
		}
		if (PlayerInjuryModule.Get().GetAllInjuriesOfState(InjuryState.Bleeding).Count > 0)
		{
			num = 1f;
		}
		if (weight > 0f && Time.time - this.m_LastDamageTime >= this.m_DamageEffectDuration)
		{
			num = Mathf.Max(num, weight - Time.deltaTime * 0.5f);
		}
		num = (Player.Get().m_DreamPPActive ? 0f : num);
		if (num != weight)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Blood, num);
		}
	}

	private void UpdatePoisonPP()
	{
		float num = 0f;
		PlayerDiseasesModule playerDiseasesModule = PlayerDiseasesModule.Get();
		int num2 = playerDiseasesModule.GetDisease(ConsumeEffect.FoodPoisoning).IsActive() ? playerDiseasesModule.GetDisease(ConsumeEffect.FoodPoisoning).m_Level : 0;
		if ((float)num2 > 0f)
		{
			num = CJTools.Math.GetProportionalClamp(0f, 1f, (float)num2, 0f, this.m_PoisonLevelToMaxEffect);
		}
		num = (Player.Get().m_DreamPPActive ? 0f : num);
		float weight = PostProcessManager.Get().GetWeight(PostProcessManager.Effect.Poison);
		if (weight != num)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Poison, weight + (num - weight) * Time.deltaTime * 2f);
		}
	}

	private void UpdateSanityPP()
	{
		float num = CJTools.Math.GetProportionalClamp(0f, 1f, (float)PlayerSanityModule.Get().m_Sanity / 100f, 1f, 0f);
		num = (Player.Get().m_DreamPPActive ? 0f : num);
		float weight = PostProcessManager.Get().GetWeight(PostProcessManager.Effect.Sanity);
		if (weight != num)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Sanity, weight + (num - weight) * Time.deltaTime * 2f);
		}
	}

	private void UpdateNotepadPP()
	{
		float num = Player.Get().GetComponent<NotepadController>().IsActive() ? 1f : 0f;
		float weight = PostProcessManager.Get().GetWeight(PostProcessManager.Effect.Notepad);
		if (weight != num)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Notepad, weight + (num - weight) * Time.deltaTime * 2f);
		}
	}

	private void UpdateLowHPPP()
	{
		float num = (PlayerConditionModule.Get().m_HP < 10f) ? 1f : 0f;
		num = (Player.Get().m_DreamPPActive ? 0f : num);
		float weight = PostProcessManager.Get().GetWeight(PostProcessManager.Effect.LowHP);
		if (weight != num)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.LowHP, weight + (num - weight) * Time.deltaTime * 2f);
		}
	}

	private void UpdateLowEnergyPP()
	{
		float num = (PlayerConditionModule.Get().m_Energy < 10f) ? 1f : 0f;
		num = (Player.Get().m_DreamPPActive ? 0f : num);
		float weight = PostProcessManager.Get().GetWeight(PostProcessManager.Effect.LowEnergy);
		if (weight != num)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.LowEnergy, weight + (num - weight) * Time.deltaTime * 2f);
		}
	}

	private void UpdateUnderwaterPP()
	{
		float num = Player.Get().IsCameraUnderwater() ? 1f : 0f;
		num = (Player.Get().m_DreamPPActive ? 0f : num);
		if (PostProcessManager.Get().GetWeight(PostProcessManager.Effect.Underwater) != num)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Underwater, num);
		}
	}

	private void UpdateDreamPP()
	{
		float num = Player.Get().m_DreamPPActive ? 1f : 0f;
		if (PostProcessManager.Get().GetWeight(PostProcessManager.Effect.Dream) != num)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Dream, num);
		}
	}

	private void UpdateDebugDofPP()
	{
		float num = this.m_DebugDofPP ? 1f : 0f;
		if (PostProcessManager.Get().GetWeight(PostProcessManager.Effect.DebugDof) != num)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.DebugDof, num);
		}
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.DemoHitReaction)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Blood, 1f);
		}
	}

	private float m_LastDamageTime;

	public float m_DamageEffectDuration;

	public float m_PoisonLevelToMaxEffect;

	private static PlayerPostProcessModule s_Instance;

	private PostProcessObject[] m_PPInterpolatorObjects = new PostProcessObject[2];

	private PostProcessManager.Effect m_DefaultEffect;

	private bool m_InterpolatePP;

	[HideInInspector]
	public bool m_DebugDofPP;
}
