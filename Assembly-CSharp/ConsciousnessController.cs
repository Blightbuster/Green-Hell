using System;
using System.Collections.Generic;
using System.Reflection;
using CJTools;
using Enums;
using UnityEngine;

public class ConsciousnessController : PlayerController
{
	public static ConsciousnessController Get()
	{
		return ConsciousnessController.s_Instance;
	}

	protected override void Awake()
	{
		ConsciousnessController.s_Instance = this;
		base.Awake();
		this.m_ControllerType = PlayerControllerType.Consciousness;
		this.m_ConditionModule = this.m_Player.GetComponent<PlayerConditionModule>();
		this.LoadScript();
		this.m_TODTime = MainLevel.Instance.m_TODTime;
	}

	private void LoadScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Player/Player_Consciousness.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "PassingOutDuration")
			{
				this.m_PassingOutDuration = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "PassOutDuration")
			{
				this.m_PassOutDuration = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "FParam")
			{
				this.m_FParams[key.GetVariable(0).SValue] = key.GetVariable(1).FValue;
			}
			else if (key.GetName() == "EnergyToPassOut")
			{
				this.m_EnergyToPassOut = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "WormChance")
			{
				this.m_WormChance = key.GetVariable(0).FValue;
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.StartPassingOut();
		PlayerAudioModule.Get().PlayPassOutSound();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.m_MovesBlocked)
		{
			this.m_Player.UnblockMoves();
			this.m_Player.UnblockRotation();
		}
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateState();
		this.DebugUpdatePassingOutCamera();
	}

	private void UpdateState()
	{
		ConsciousnessController.ConsciousnessState state = this.m_State;
		if (state != ConsciousnessController.ConsciousnessState.PassingOut)
		{
			if (state == ConsciousnessController.ConsciousnessState.PassedOut)
			{
				this.UpdatePassedOut();
			}
		}
		else
		{
			float b = Time.time - this.m_EnterStateTime;
			this.m_PassingOutProgress = CJTools.Math.GetProportionalClamp(0f, 1f, b, 0f, this.m_PassingOutDuration);
			if (this.m_PassingOutProgress == 1f)
			{
				this.PassOut();
			}
		}
	}

	private void UpdatePassedOut()
	{
		float num = Time.time - this.m_StartPassOutTime;
		this.m_Progress = num / this.m_PassOutDurationRealTime;
		this.m_Progress = Mathf.Clamp01(this.m_Progress);
		float num2 = this.m_Progress - this.m_PrevProgress;
		this.m_TODTime.AddHours(this.m_PassOutDuration * num2, true);
		this.m_HoursDelta = this.m_PassOutDuration * num2;
		int num3 = (int)(this.m_PassOutDuration * this.m_Progress);
		if (num3 > this.m_HourProgress)
		{
			this.CheckWorm();
		}
		this.m_HourProgress = num3;
		float num4 = this.m_Progress - this.m_PrevProgress;
		foreach (string text in this.m_FParams.Keys)
		{
			PropertyInfo property = this.m_ConditionModule.GetType().GetProperty(text);
			float num5 = (float)property.GetValue(this.m_ConditionModule, null);
			float num6 = (this.m_FParams[text] <= 0f) ? this.m_FParams[text] : this.m_FParams[text];
			if (PlayerInjuryModule.Get().GetNumWounds() <= 0 || !(text == "m_HP"))
			{
				num5 += num6 * num4;
			}
			property.SetValue(this.m_ConditionModule, num5, null);
		}
		this.m_ConditionModule.ClampParams();
		if (this.m_Progress >= 1f)
		{
			this.WakeUp();
		}
		this.m_PrevProgress = this.m_Progress;
	}

	private void DebugUpdatePassingOutCamera()
	{
		if (!this.IsState(ConsciousnessController.ConsciousnessState.PassingOut))
		{
			return;
		}
	}

	private void StartPassingOut()
	{
		this.m_StartPassOutTime = 0f;
		this.m_Progress = 0f;
		this.m_PrevProgress = 0f;
		this.m_HoursDelta = 0f;
		this.m_HourProgress = 0;
		this.m_Player.BlockMoves();
		this.m_Player.BlockRotation();
		this.m_MovesBlocked = true;
		this.SetState(ConsciousnessController.ConsciousnessState.PassingOut);
		this.m_Animator.SetInteger(this.m_PassOutHash, 1);
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().Deactivate();
		}
	}

	private void PassOut()
	{
		this.SetState(ConsciousnessController.ConsciousnessState.PassedOut);
		this.m_StartPassOutTime = Time.time;
	}

	public void WakeUp()
	{
		this.m_StartPassOutTime = 0f;
		FadeSystem.Get().FadeOut(FadeType.All, new VDelegate(this.WakeUpOnFade), 1.5f, null);
	}

	public void WakeUpOnFade()
	{
		this.m_Player.UnblockMoves();
		this.m_Player.UnblockRotation();
		this.m_MovesBlocked = false;
		this.m_PassingOutProgress = 0f;
		this.SetState(ConsciousnessController.ConsciousnessState.None);
		this.Stop();
		this.m_Animator.SetInteger(this.m_PassOutHash, 2);
		FadeSystem.Get().FadeIn(FadeType.All, null, 1.5f);
	}

	public bool IsState(ConsciousnessController.ConsciousnessState state)
	{
		return this.m_State == state;
	}

	public void SetState(ConsciousnessController.ConsciousnessState state)
	{
		if (this.m_State == state)
		{
			return;
		}
		this.m_State = state;
		this.m_EnterStateTime = Time.time;
	}

	public float GetPassingOutProgress()
	{
		return this.m_PassingOutProgress;
	}

	public float GetEnergyToPassOut()
	{
		return this.m_EnergyToPassOut;
	}

	public override bool ForceReceiveAnimEvent()
	{
		return this.m_Animator.GetInteger(this.m_PassOutHash) == 2;
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.RecoverEnd)
		{
			this.m_Animator.SetInteger(this.m_PassOutHash, 0);
		}
	}

	private void CheckWorm()
	{
		float num = this.CalcChanceToGetWorm();
		if (UnityEngine.Random.Range(0f, 1f) < num)
		{
			this.TryAddWorm();
		}
	}

	private void TryAddWorm()
	{
		for (int i = 0; i < 4; i++)
		{
			BIWoundSlot freeWoundSlot = BodyInspectionController.Get().GetFreeWoundSlot((InjuryPlace)i, InjuryType.Worm, true);
			if (freeWoundSlot != null)
			{
				PlayerInjuryModule.Get().AddInjury(InjuryType.Worm, (InjuryPlace)i, freeWoundSlot, InjuryState.Open, 0, null);
				return;
			}
		}
	}

	private float CalcChanceToGetWorm()
	{
		float num = this.m_WormChance;
		bool activeSelf = RainManager.Get().m_RainCone.activeSelf;
		if (activeSelf)
		{
			num *= this.m_RainWormChanceFactor;
		}
		return num;
	}

	public bool IsUnconscious()
	{
		return this.m_StartPassOutTime > 0f;
	}

	private PlayerConditionModule m_ConditionModule;

	private float m_EnterStateTime;

	private float m_PassingOutDuration = 1f;

	private float m_PassOutDuration = 2f;

	private float m_PassingOutProgress;

	private ConsciousnessController.ConsciousnessState m_State;

	private Dictionary<string, float> m_FParams = new Dictionary<string, float>();

	private float m_EnergyToPassOut;

	private int m_PassOutHash = Animator.StringToHash("PassOut");

	private static ConsciousnessController s_Instance;

	private bool m_MovesBlocked = true;

	private float m_WormChance;

	private float m_RainWormChanceFactor = 1f;

	private float m_StartPassOutTime;

	private float m_Progress;

	private float m_PrevProgress;

	public TOD_Sky m_Sky;

	public TOD_Time m_TODTime;

	[HideInInspector]
	public float m_HoursDelta;

	private int m_HourProgress;

	private float m_PassOutDurationRealTime = 3f;

	public enum ConsciousnessState
	{
		None,
		PassingOut,
		PassedOut
	}
}
