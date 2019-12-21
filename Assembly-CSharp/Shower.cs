using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class Shower : Construction, IItemSlotParent, IProcessor
{
	protected override void Awake()
	{
		base.Awake();
		this.m_AudioSourceStartStop = base.gameObject.GetComponent<AudioSource>();
		this.m_ItemSlot = base.transform.Find("ItemSlot").gameObject.GetComponent<ItemSlot>();
		this.m_ItemSlot.m_ShowOnlyIfItemIsCorrect = true;
		this.m_ItemSlot.gameObject.SetActive(false);
		base.RegisterConstantUpdateItem();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!Shower.s_Showers.Contains(this))
		{
			Shower.s_Showers.Add(this);
		}
		if (Shower.s_StartSound == null)
		{
			Shower.s_StartSound = (AudioClip)Resources.Load("Sounds/Constructions/shower_start");
		}
		if (Shower.s_StopSound == null)
		{
			Shower.s_StopSound = (AudioClip)Resources.Load("Sounds/Constructions/shower_end");
		}
		if (Shower.s_LoopSound == null)
		{
			Shower.s_LoopSound = (AudioClip)Resources.Load("Sounds/Constructions/shower_stream_water_mono_loop");
		}
		if (Shower.s_Loop2DSound == null)
		{
			Shower.s_Loop2DSound = (AudioClip)Resources.Load("Sounds/Constructions/shower_bath_stereo_loop");
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Shower.s_Showers.Contains(this))
		{
			Shower.s_Showers.Remove(this);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Shower.s_Showers.Contains(this))
		{
			Shower.s_Showers.Remove(this);
		}
		base.UnregisterConstantUpdateItem();
	}

	protected override void Start()
	{
		base.Start();
		HUDProcess.Get().RegisterProcess(this, "HUD_drinking_water", this, true);
		this.m_ItemSlot.m_ItemTypeList.Add(ItemType.LiquidContainer);
	}

	public float GetProcessProgress(Trigger trigger)
	{
		return this.m_Amount / this.m_Capacity;
	}

	public override Vector3 GetIconPos()
	{
		return this.m_ProcessIconDummy.transform.position;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (this.m_Active)
		{
			actions.Add(TriggerAction.TYPE.TurnOff);
			return;
		}
		actions.Add(TriggerAction.TYPE.TurnOn);
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (this.m_Active)
		{
			this.TurnOff();
			return;
		}
		this.TurnOn();
	}

	private void FillWithLiquid(Item item)
	{
		LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)((LiquidContainer)item).m_Info;
		this.m_Amount += liquidContainerInfo.m_Amount;
		liquidContainerInfo.m_Amount = 0f;
		Mathf.Clamp(this.m_Amount, 0f, this.m_Capacity);
	}

	public override void UpdatePhx()
	{
	}

	private void UpdateWaterFromRain(float delta_time)
	{
		if (this.m_Amount < this.m_Capacity && RainManager.Get().IsRain() && !RainManager.Get().IsInRainCutter(base.transform.position))
		{
			this.m_Amount += delta_time * 1.6f;
			this.m_Amount = Mathf.Clamp(this.m_Amount, 0f, this.m_Capacity);
		}
	}

	public override void ConstantUpdate()
	{
		base.ConstantUpdate();
		float num = Time.deltaTime;
		if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
		{
			num = Player.GetSleepTimeFactor();
		}
		this.UpdateWaterFromRain(num);
		if (!this.m_Active)
		{
			return;
		}
		this.m_Amount -= 10f * num;
		if (this.m_Amount <= 0f)
		{
			this.TurnOff();
		}
		Mathf.Clamp(this.m_Amount, 0f, this.m_Capacity);
	}

	private void TurnOff()
	{
		this.m_Active = false;
		this.UpdateEffects();
		this.m_AudioSourceStartStop.clip = Shower.s_StopSound;
		this.m_AudioSourceStartStop.loop = false;
		this.m_AudioSourceStartStop.Play();
		base.StartCoroutine(AudioFadeOut.FadeOut(this.m_AudioSourceLoop, 1.5f, 0f, null));
		if (this.m_StereoLoopCoroutine != null)
		{
			base.StopCoroutine(this.m_StereoLoopCoroutine);
		}
		this.m_StereoLoopCoroutine = base.StartCoroutine(AudioFadeOut.FadeOut(this.m_AudioSourceLoop2D, 1.5f, 0f, null));
	}

	private void TurnOn()
	{
		this.m_Active = true;
		this.UpdateEffects();
		this.m_AudioSourceStartStop.clip = Shower.s_StartSound;
		this.m_AudioSourceStartStop.loop = false;
		this.m_AudioSourceStartStop.Play();
		this.m_AudioSourceLoop.clip = Shower.s_LoopSound;
		this.m_AudioSourceLoop.loop = true;
		this.m_AudioSourceLoop.Play();
		base.StartCoroutine(AudioFadeOut.FadeIn(this.m_AudioSourceLoop, 1.5f, 1f, null));
		this.m_AudioSourceLoop2D.clip = Shower.s_Loop2DSound;
		this.m_AudioSourceLoop2D.loop = true;
		this.m_AudioSourceLoop2D.volume = 0f;
		if (this.m_IsPlayerInside)
		{
			if (this.m_StereoLoopCoroutine != null)
			{
				base.StopCoroutine(this.m_StereoLoopCoroutine);
			}
			this.m_StereoLoopCoroutine = base.StartCoroutine(AudioFadeOut.FadeIn(this.m_AudioSourceLoop2D, 1.5f, 1f, null));
		}
	}

	private void UpdateEffects()
	{
		if (this.m_Active)
		{
			this.m_FXObject.SetActive(true);
			this.m_OffVisObject.SetActive(false);
			this.m_AudioSourceStartStop.Play();
			return;
		}
		this.m_FXObject.SetActive(false);
		this.m_OffVisObject.SetActive(true);
		this.m_AudioSourceStartStop.Stop();
	}

	protected override void OnTriggerEnter(Collider other)
	{
		base.OnTriggerEnter(other);
		if (other.gameObject == Camera.main.gameObject)
		{
			this.m_IsPlayerInside = true;
			if (this.m_Active)
			{
				if (this.m_StereoLoopCoroutine != null)
				{
					base.StopCoroutine(this.m_StereoLoopCoroutine);
				}
				this.m_StereoLoopCoroutine = base.StartCoroutine(AudioFadeOut.FadeIn(this.m_AudioSourceLoop2D, 1.5f, 1f, null));
			}
		}
	}

	protected override void OnTriggerExit(Collider other)
	{
		base.OnTriggerExit(other);
		if (other.gameObject == Camera.main.gameObject)
		{
			this.m_IsPlayerInside = false;
			if (this.m_Active)
			{
				if (this.m_StereoLoopCoroutine != null)
				{
					base.StopCoroutine(this.m_StereoLoopCoroutine);
				}
				this.m_StereoLoopCoroutine = base.StartCoroutine(AudioFadeOut.FadeOut(this.m_AudioSourceLoop2D, 1.5f, 0f, null));
			}
		}
	}

	public virtual void OnInsertItem(ItemSlot slot)
	{
		if (slot == this.m_ItemSlot)
		{
			Item item = slot.m_Item;
			this.FillWithLiquid(item);
			slot.RemoveItem();
			InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true);
			PlayerAudioModule.Get().PlayWaterSpillSound(1f, false);
		}
	}

	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && (this.m_Active || this.m_Amount > 0f);
	}

	public virtual bool CanInsertItem(Item item)
	{
		return true;
	}

	public virtual void OnRemoveItem(ItemSlot slot)
	{
	}

	public override bool IsShower()
	{
		return true;
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("ShowerAmount" + index, this.m_Amount);
		SaveGame.SaveVal("ShowerActive" + index, this.m_Active);
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.m_Amount = SaveGame.LoadFVal("ShowerAmount" + index);
		this.m_Active = SaveGame.LoadBVal("ShowerActive" + index);
		this.UpdateEffects();
	}

	private ItemSlot m_ItemSlot;

	public float m_Capacity = 100f;

	public float m_Amount;

	public bool m_Active;

	public GameObject m_FXObject;

	public GameObject m_OffVisObject;

	public static List<Shower> s_Showers = new List<Shower>();

	public bool m_IsPlayerInside;

	public AudioSource m_AudioSourceStartStop;

	public AudioSource m_AudioSourceLoop;

	public AudioSource m_AudioSourceLoop2D;

	public GameObject m_ProcessIconDummy;

	private Coroutine m_StereoLoopCoroutine;

	private static AudioClip s_StartSound = null;

	private static AudioClip s_StopSound = null;

	private static AudioClip s_LoopSound = null;

	private static AudioClip s_Loop2DSound = null;
}
