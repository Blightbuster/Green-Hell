using System;
using System.Collections.Generic;
using System.Reflection;
using CJTools;
using Enums;
using UnityEngine;

public class SleepController : PlayerController
{
	public static SleepController Get()
	{
		return SleepController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		SleepController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.Sleep;
		this.m_ConditionModule = this.m_Player.GetComponent<PlayerConditionModule>();
		this.m_TODTime = this.m_Sky.gameObject.GetComponent<TOD_Time>();
		this.UpdateLastWakeUpTime();
		this.LoadScript();
	}

	private void LoadScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Player/Player_Sleep.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "FParam")
			{
				this.m_FParams[key.GetVariable(0).SValue] = key.GetVariable(1).FValue;
			}
			else if (key.GetName() == "SleepDuration")
			{
				this.m_SleepDuration = key.GetVariable(0).IValue;
			}
			else if (key.GetName() == "WormChance")
			{
				this.m_WormChance = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "WoundWormChanceFactor")
			{
				this.m_WoundWormChanceFactor = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "RainWormChanceFactor")
			{
				this.m_RainWormChanceFactor = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "BedWormChanceFactor")
			{
				this.m_BedWormChanceFactor.Add((int)Enum.Parse(typeof(ItemID), key.GetVariable(0).SValue), key.GetVariable(1).FValue);
			}
			else if (key.GetName() == "FireWormChanceFactor")
			{
				this.m_FireWormChanceFactor.Add((int)Enum.Parse(typeof(ItemID), key.GetVariable(0).SValue), key.GetVariable(1).FValue);
			}
		}
	}

	public bool GetParamOnWakeUp(string param_name, out float param)
	{
		return this.m_FParams.TryGetValue(param_name, out param);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.m_Player.IsDead())
		{
			this.m_Animator.SetBool(this.m_StandUpHash, false);
		}
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateSleeping();
	}

	private void UpdateSleeping()
	{
		float num = Time.time - this.m_StartSleepingTime;
		this.m_Progress = num / (float)this.m_SleepDuration;
		this.m_Progress = Mathf.Clamp01(this.m_Progress);
		float num2 = this.m_Progress - this.m_PrevProgress;
		this.m_TODTime.AddHours((float)this.m_SleepDuration * num2, true);
		this.m_HoursDelta = (float)this.m_SleepDuration * num2;
		int num3 = (int)((float)this.m_SleepDuration * this.m_Progress);
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
			float num6 = (this.m_FParams[text] <= 0f) ? this.m_FParams[text] : (this.m_FParams[text] * this.m_ParamsMul);
			if (PlayerInjuryModule.Get().GetNumWounds() <= 0 || !(text == "m_HP"))
			{
				num5 += num6 * num4;
			}
			property.SetValue(this.m_ConditionModule, num5, null);
		}
		this.m_ConditionModule.ClampParams();
		if (this.m_Progress >= 1f)
		{
			this.WakeUp(false, true);
		}
		this.SetupSurroundingConstructions();
		this.m_PrevProgress = this.m_Progress;
	}

	public void StartSleeping(RestingPlace place = null, bool block_moves = true)
	{
		if (!Player.Get().CanSleep())
		{
			return;
		}
		this.m_RestingPlace = place;
		this.SetupSurroundingConstructions();
		if (block_moves)
		{
			this.m_Player.BlockMoves();
			this.m_Player.BlockRotation();
		}
		this.m_StartSleepingTime = Time.time;
		this.m_StartSleepHour = this.m_Sky.Cycle.Hour;
		this.m_Progress = 0f;
		this.m_PrevProgress = 0f;
		this.m_HourProgress = 0;
		HUDSleeping.Get().gameObject.SetActive(true);
		MenuInGameManager.Get().HideMenu();
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		if (currentItem != null && currentItem.m_Info.IsHeavyObject())
		{
			this.m_Player.DropItem(currentItem);
		}
		Player.Get().StartController(PlayerControllerType.Sleep);
		PlayerAudioModule.Get().PlaySleepSound();
		GreenHellGame.Instance.SetSnapshot(AudioMixerSnapshotGame.Sleep, 0.5f);
		if (Inventory3DManager.Get().IsActive())
		{
			Inventory3DManager.Get().Deactivate();
		}
	}

	private void SetupSurroundingConstructions()
	{
		Bounds coumpoundObjectBounds = General.GetCoumpoundObjectBounds(base.gameObject);
		DebugRender.DrawBox(coumpoundObjectBounds, Color.blue);
		List<Construction> list = new List<Construction>();
		for (int i = 0; i < Construction.s_Constructions.Count; i++)
		{
			Construction construction = Construction.s_Constructions[i];
			if (construction.gameObject.activeSelf)
			{
				float magnitude = (construction.gameObject.transform.position - base.gameObject.transform.position).magnitude;
				if (magnitude <= 10f)
				{
					float paramsMulRadius = ((ConstructionInfo)construction.m_Info).m_ParamsMulRadius;
					if (paramsMulRadius >= 0f)
					{
						if (magnitude < paramsMulRadius)
						{
							list.Add(construction);
						}
					}
					else
					{
						Bounds coumpoundObjectBounds2 = General.GetCoumpoundObjectBounds(construction.gameObject);
						DebugRender.DrawBox(coumpoundObjectBounds2, Color.blue);
						if (coumpoundObjectBounds2.Intersects(coumpoundObjectBounds))
						{
							list.Add(construction);
						}
					}
				}
			}
		}
		if (this.m_RestingPlace)
		{
			ConstructionInfo constructionInfo = (ConstructionInfo)this.m_RestingPlace.m_Info;
			float num = constructionInfo.m_RestingParamsMul;
			for (int j = 0; j < list.Count; j++)
			{
				ConstructionInfo constructionInfo2 = (ConstructionInfo)list[j].m_Info;
				if (list[j] != null && list[j].HasRestInfluence())
				{
					num *= constructionInfo2.m_RestingParamsMul;
				}
			}
			this.m_ParamsMul = num;
		}
		this.m_Shelter = null;
		this.m_Firecamps.Clear();
		foreach (Construction construction2 in list)
		{
			if (construction2.GetType() == typeof(Firecamp))
			{
				this.m_Firecamps.Add((Firecamp)construction2);
			}
			else
			{
				string text = construction2.m_Info.m_ID.ToString();
				text = text.ToLower();
				if (text.Contains("shelter"))
				{
					this.m_Shelter = construction2;
				}
			}
		}
	}

	public void WakeUp(bool force_update = false, bool unblock_moves = true)
	{
		if (force_update)
		{
			this.m_TODTime.AddHours((float)this.m_SleepDuration, true);
			this.CheckWorm();
			this.SetupSurroundingConstructions();
		}
		if (unblock_moves)
		{
			this.m_Player.UnblockMoves();
			this.m_Player.UnblockRotation();
		}
		this.m_StartSleepingTime = 0f;
		this.m_ParamsMul = 1f;
		this.UpdateLastWakeUpTime();
		if (this.m_RestingPlace != null)
		{
			EventsManager.OnEvent(Enums.Event.Sleep, 1, this.m_SleepDuration, 1);
			PlayerSanityModule.Get().OnEvent(PlayerSanityModule.SanityEventType.BedSleep, 1);
		}
		else
		{
			EventsManager.OnEvent(Enums.Event.Sleep, 1, this.m_SleepDuration, 0);
			PlayerSanityModule.Get().OnEvent(PlayerSanityModule.SanityEventType.GroundSleep, 1);
			PlayerInjuryModule.Get().SleptOnGround();
		}
		GreenHellGame.Instance.SetSnapshot(AudioMixerSnapshotGame.Default, 0.5f);
		this.m_Animator.SetBool(this.m_StandUpHash, true);
		this.Stop();
	}

	public bool IsSleeping()
	{
		return this.m_StartSleepingTime > 0f;
	}

	public void UpdateLastWakeUpTime()
	{
		this.m_LastWakeUpTime = (float)this.m_Sky.Cycle.Month * 30f * 24f + (float)this.m_Sky.Cycle.Day * 24f + this.m_Sky.Cycle.Hour;
	}

	private float CalcChanceToGetWorm()
	{
		float num = this.m_WormChance;
		if (this.m_RestingPlace)
		{
			float num2 = 1f;
			if (this.m_BedWormChanceFactor.TryGetValue((int)this.m_RestingPlace.m_Info.m_ID, out num2))
			{
				num *= num2;
			}
		}
		bool activeSelf = RainManager.Get().m_RainCone.activeSelf;
		if (activeSelf && !this.m_Shelter)
		{
			num *= this.m_RainWormChanceFactor;
		}
		foreach (Firecamp firecamp in this.m_Firecamps)
		{
			if (firecamp.m_Burning)
			{
				float num3 = 1f;
				if (this.m_FireWormChanceFactor.TryGetValue((int)firecamp.m_Info.m_ID, out num3))
				{
					num *= num3;
				}
			}
		}
		return num;
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
			BIWoundSlot freeWoundSlot = BodyInspectionController.Get().GetFreeWoundSlot((InjuryPlace)i, InjuryType.Worm);
			if (freeWoundSlot != null)
			{
				PlayerInjuryModule.Get().AddInjury(InjuryType.Worm, (InjuryPlace)i, freeWoundSlot, InjuryState.Open, 0, null);
				return;
			}
		}
	}

	public override bool ForceReceiveAnimEvent()
	{
		return this.m_Animator.GetBool(this.m_StandUpHash);
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.RecoverEnd)
		{
			this.m_Animator.SetBool(this.m_StandUpHash, false);
		}
	}

	public int m_SleepDuration = 8;

	private Dictionary<string, float> m_FParams = new Dictionary<string, float>();

	private PlayerConditionModule m_ConditionModule;

	private float m_StartSleepingTime;

	private float m_ParamsMul = 1f;

	public TOD_Sky m_Sky;

	public TOD_Time m_TODTime;

	private float m_StartSleepHour;

	public float m_LastWakeUpTime;

	private int m_HourProgress;

	[HideInInspector]
	public float m_Progress;

	private float m_PrevProgress;

	private RestingPlace m_RestingPlace;

	private float m_WormChance;

	private float m_WoundWormChanceFactor = 1f;

	private float m_RainWormChanceFactor = 1f;

	private Dictionary<int, float> m_BedWormChanceFactor = new Dictionary<int, float>();

	private Dictionary<int, float> m_FireWormChanceFactor = new Dictionary<int, float>();

	private Construction m_Shelter;

	private List<Firecamp> m_Firecamps = new List<Firecamp>();

	private int m_StandUpHash = Animator.StringToHash("StandUpAfterSleep");

	private static SleepController s_Instance;

	[HideInInspector]
	public float m_HoursDelta;
}
