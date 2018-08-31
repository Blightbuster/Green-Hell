using System;
using CJTools;
using Enums;
using UnityEngine;

public class PlayerPostProcessModule : PlayerModule
{
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

	public override void Update()
	{
		base.Update();
		this.UpdateGamePP();
		this.UpdateDamagePP();
		this.UpdatePoisonPP();
		this.UpdateSanityPP();
		this.UpdateNotepadPP();
		this.UpdateLowHPPP();
		this.UpdateLowEnergyPP();
	}

	private void UpdateGamePP()
	{
		float num = (!Player.Get().GetComponent<NotepadController>().IsActive()) ? 1f : 0f;
		num = (((float)PlayerSanityModule.Get().m_Sanity >= 10f) ? num : 0f);
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
		if (num != weight)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Blood, num);
		}
	}

	private void UpdatePoisonPP()
	{
		float num = 0f;
		PlayerDiseasesModule playerDiseasesModule = PlayerDiseasesModule.Get();
		int num2 = (!playerDiseasesModule.GetDisease(ConsumeEffect.FoodPoisoning).IsActive()) ? 0 : playerDiseasesModule.GetDisease(ConsumeEffect.FoodPoisoning).m_Level;
		if ((float)num2 > 0f)
		{
			num = CJTools.Math.GetProportionalClamp(0f, 1f, (float)num2, 0f, this.m_PoisonLevelToMaxEffect);
		}
		float weight = PostProcessManager.Get().GetWeight(PostProcessManager.Effect.Poison);
		if (weight != num)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Poison, weight + (num - weight) * Time.deltaTime * 2f);
		}
	}

	private void UpdateSanityPP()
	{
		float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, (float)PlayerSanityModule.Get().m_Sanity / 100f, 1f, 0f);
		float weight = PostProcessManager.Get().GetWeight(PostProcessManager.Effect.Sanity);
		if (weight != proportionalClamp)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Sanity, weight + (proportionalClamp - weight) * Time.deltaTime * 2f);
		}
	}

	private void UpdateNotepadPP()
	{
		float num = (!Player.Get().GetComponent<NotepadController>().IsActive()) ? 0f : 1f;
		float weight = PostProcessManager.Get().GetWeight(PostProcessManager.Effect.Notepad);
		if (weight != num)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Notepad, weight + (num - weight) * Time.deltaTime * 2f);
		}
	}

	private void UpdateLowHPPP()
	{
		float num = (PlayerConditionModule.Get().m_HP >= 10f) ? 0f : 1f;
		float weight = PostProcessManager.Get().GetWeight(PostProcessManager.Effect.LowHP);
		if (weight != num)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.LowHP, weight + (num - weight) * Time.deltaTime * 2f);
		}
	}

	private void UpdateLowEnergyPP()
	{
		float num = (PlayerConditionModule.Get().m_Energy >= 10f) ? 0f : 1f;
		float weight = PostProcessManager.Get().GetWeight(PostProcessManager.Effect.LowEnergy);
		if (weight != num)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.LowEnergy, weight + (num - weight) * Time.deltaTime * 2f);
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
}
