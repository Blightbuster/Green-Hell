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
				if (Time.time - this.m_EnterStateTime >= this.m_PassOutDuration)
				{
					this.WakeUp();
				}
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

	private void DebugUpdatePassingOutCamera()
	{
		if (!this.IsState(ConsciousnessController.ConsciousnessState.PassingOut))
		{
			return;
		}
	}

	private void StartPassingOut()
	{
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
	}

	public void WakeUp()
	{
		FadeSystem.Get().FadeOut(FadeType.All, new VDelegate(this.WakeUpOnFade), 1.5f, null);
	}

	public void WakeUpOnFade()
	{
		this.m_Player.UnblockMoves();
		this.m_Player.UnblockRotation();
		this.m_MovesBlocked = false;
		this.m_PassingOutProgress = 0f;
		this.ApplyParamsOnWakeUp();
		this.SetState(ConsciousnessController.ConsciousnessState.None);
		this.Stop();
		this.m_Animator.SetInteger(this.m_PassOutHash, 2);
		FadeSystem.Get().FadeIn(FadeType.All, null, 1.5f);
	}

	private void ApplyParamsOnWakeUp()
	{
		foreach (string text in this.m_FParams.Keys)
		{
			PropertyInfo property = this.m_ConditionModule.GetType().GetProperty(text);
			float num = (float)property.GetValue(this.m_ConditionModule, null);
			num += this.m_FParams[text];
			property.SetValue(this.m_ConditionModule, num, null);
		}
		this.m_ConditionModule.ClampParams();
		this.m_Player.GetComponent<SleepController>().UpdateLastWakeUpTime();
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

	private PlayerConditionModule m_ConditionModule;

	private float m_EnterStateTime;

	private float m_PassingOutDuration = 1f;

	private float m_PassOutDuration = 1f;

	private float m_PassingOutProgress;

	private ConsciousnessController.ConsciousnessState m_State;

	private Dictionary<string, float> m_FParams = new Dictionary<string, float>();

	private float m_EnergyToPassOut;

	private int m_PassOutHash = Animator.StringToHash("PassOut");

	private static ConsciousnessController s_Instance;

	private bool m_MovesBlocked = true;

	public enum ConsciousnessState
	{
		None,
		PassingOut,
		PassedOut
	}
}
