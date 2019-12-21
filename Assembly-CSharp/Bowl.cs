using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class Bowl : LiquidContainer
{
	protected override void Awake()
	{
		base.Awake();
		this.m_UnsafeWaterVis = base.transform.Find("UnsafeWaterVis").gameObject;
		this.m_UnsafeWaterVis.SetActive(false);
		this.m_WaterVis = base.transform.Find("WaterVis").gameObject;
		this.m_WaterVis.SetActive(false);
		this.m_HerbVis = base.transform.Find("HerbVis").gameObject;
		for (int i = 0; i < this.m_HerbVis.transform.childCount; i++)
		{
			GameObject gameObject = this.m_HerbVis.transform.GetChild(i).gameObject;
			this.m_HerbVisualisations.Add(gameObject.name, gameObject);
			gameObject.SetActive(false);
		}
		this.m_Particle = base.transform.Find("ParticleVis").gameObject;
		this.m_Particle.SetActive(false);
		this.m_ItemSlot = base.transform.Find("ItemSlot").gameObject.GetComponent<ItemSlot>();
		this.m_ItemSlot.m_ShowOnlyIfItemIsCorrect = true;
		this.m_ItemSlot.gameObject.SetActive(true);
		Transform transform = base.transform.Find("DefaultVis");
		if (transform)
		{
			this.m_DefaultVis = transform.gameObject;
		}
		GameObject gameObject2 = base.transform.Find("ItemVis").gameObject;
		for (int j = 0; j < gameObject2.transform.childCount; j++)
		{
			GameObject gameObject3 = gameObject2.transform.GetChild(j).gameObject;
			this.m_ItemVisualisations.Add(gameObject3.name, gameObject3);
			gameObject3.SetActive(false);
		}
	}

	protected override void Start()
	{
		base.Start();
		this.m_ItemSlot.m_ItemIDList = new List<ItemID>();
		for (int i = 0; i < LiquidManager.Get().m_LiquidDatas.Count; i++)
		{
			ItemID itemComponent = LiquidManager.Get().m_LiquidDatas[i].m_ItemComponent;
			if (itemComponent != ItemID.None)
			{
				this.m_ItemSlot.m_ItemIDList.Add(itemComponent);
			}
		}
		this.m_ItemSlot.m_ItemID = new string[this.m_ItemSlot.m_ItemIDList.Count];
		for (int j = 0; j < this.m_ItemSlot.m_ItemIDList.Count; j++)
		{
			this.m_ItemSlot.m_ItemID[j] = this.m_ItemSlot.m_ItemIDList[j].ToString();
		}
		this.m_ItemSlot.gameObject.SetActive(false);
	}

	public override bool CanTrigger()
	{
		if (this.m_CantTriggerDuringDialog && DialogsManager.Get().IsAnyDialogPlaying())
		{
			return false;
		}
		if (BowlController.Get().m_Bowl == this)
		{
			return base.CanTrigger();
		}
		return this.m_State != Bowl.State.UnsafeWaterCooking && this.m_State != Bowl.State.HerbCooking;
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Pour)
		{
			base.Fill(LiquidInHandsController.Get().m_Container);
			return;
		}
		if (action == TriggerAction.TYPE.Drink)
		{
			this.Drink();
			if (((LiquidContainerInfo)this.m_Info).m_Amount == 0f)
			{
				if (this.m_Item)
				{
					UnityEngine.Object.Destroy(this.m_Item.gameObject);
					this.m_Item = null;
				}
				this.m_ItemSlot.gameObject.SetActive(false);
				if (this.m_ItemVis)
				{
					this.m_ItemVis.gameObject.SetActive(false);
					this.m_ItemVis = null;
				}
				this.SetState(Bowl.State.None);
			}
		}
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		if (GreenHellGame.IsPadControllerActive() && Inventory3DManager.Get().IsActive() && Inventory3DManager.Get().CanSetCarriedItem(true))
		{
			actions.Add(TriggerAction.TYPE.Pick);
		}
		switch (this.m_State)
		{
		case Bowl.State.None:
			if (LiquidInHandsController.Get().IsActive())
			{
				actions.Add(TriggerAction.TYPE.Pour);
			}
			else if (!this.m_RackChild)
			{
				actions.Add(TriggerAction.TYPE.Take);
			}
			break;
		case Bowl.State.UnsafeWater:
		case Bowl.State.WaterCooked:
		case Bowl.State.Herb:
		case Bowl.State.HerbCooked:
			if (this.m_LCInfo.CanDrink())
			{
				actions.Add(TriggerAction.TYPE.Drink);
			}
			else
			{
				actions.Add(TriggerAction.TYPE.Take);
			}
			break;
		}
		actions.Add(TriggerAction.TYPE.Expand);
	}

	public override void OnGet()
	{
		base.OnGet();
		LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)this.m_Info;
		if (liquidContainerInfo.m_LiquidType == LiquidType.UnsafeWater || liquidContainerInfo.m_LiquidType == LiquidType.DirtyWater)
		{
			this.SetState(Bowl.State.UnsafeWater);
			return;
		}
		if (liquidContainerInfo.m_LiquidType == LiquidType.Water)
		{
			this.SetState(Bowl.State.WaterCooked);
			return;
		}
		this.SetState(Bowl.State.HerbCooked);
	}

	public override void OnInsertItem(ItemSlot slot)
	{
		base.OnInsertItem(slot);
		if (slot == this.m_ItemSlot)
		{
			this.m_Item = slot.m_Item;
			this.m_Item.gameObject.SetActive(false);
			this.m_Item.ItemsManagerUnregister();
			if (this.m_ItemVisualisations.ContainsKey(this.m_Item.m_Info.m_ID.ToString()))
			{
				this.m_ItemVis = this.m_ItemVisualisations[this.m_Item.m_Info.m_ID.ToString()];
			}
			else
			{
				this.m_ItemVis = this.m_DefaultVis;
			}
			if (this.m_ItemVis)
			{
				this.m_ItemVis.SetActive(true);
			}
			slot.Deactivate();
			slot.gameObject.SetActive(false);
		}
	}

	public void OnFirecampAdd(Firecamp firecamp)
	{
		this.m_Firecamp = firecamp;
		HUDBowlCooking.Get().RegisterBowl(this);
		this.m_ItemSlot.gameObject.SetActive(false);
		this.SetState(Bowl.State.None);
	}

	public void OnFirecampRemove(Firecamp firecamp)
	{
		this.m_Firecamp = null;
		HUDBowlCooking.Get().UnregisterBowl(this);
		this.m_ItemSlot.gameObject.SetActive(false);
		this.SetState(Bowl.State.None);
	}

	public float GetCookingLevel()
	{
		Bowl.State state = this.m_State;
		if (state != Bowl.State.UnsafeWaterCooking && state != Bowl.State.HerbCooking)
		{
			return 0f;
		}
		if (this.m_CookingLength <= 0f)
		{
			return 1f;
		}
		return this.m_CookingDuration / this.m_CookingLength;
	}

	private void SetState(Bowl.State state)
	{
		if (this.m_State == state)
		{
			return;
		}
		this.m_State = state;
		this.OnEnterState();
	}

	private void OnEnterState()
	{
		if (this.m_DefaultVis)
		{
			this.m_DefaultVis.SetActive(false);
		}
		this.m_UnsafeWaterVis.SetActive(false);
		this.m_WaterVis.SetActive(false);
		foreach (string key in this.m_HerbVisualisations.Keys)
		{
			this.m_HerbVisualisations[key].SetActive(false);
		}
		this.m_HerbVis.SetActive(false);
		if (this.m_ItemVis)
		{
			this.m_ItemVis.SetActive(false);
		}
		if (this.m_State != Bowl.State.None)
		{
			this.InitializeAudio();
		}
		LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)this.m_Info;
		LiquidManager.Get().GetLiquidData(liquidContainerInfo.m_LiquidType);
		switch (this.m_State)
		{
		case Bowl.State.None:
			this.m_ItemSlot.gameObject.SetActive(false);
			if (this.m_ItemVis)
			{
				this.m_ItemVis.gameObject.SetActive(false);
				this.m_ItemVis = null;
				return;
			}
			break;
		case Bowl.State.UnsafeWater:
		case Bowl.State.UnsafeWaterCooking:
			this.m_CookingDuration = 0f;
			this.m_UnsafeWaterVis.SetActive(true);
			return;
		case Bowl.State.WaterCooked:
			liquidContainerInfo.OnCooked();
			this.m_WaterVis.SetActive(true);
			return;
		case Bowl.State.Herb:
			if (this.m_ItemVis)
			{
				this.m_ItemVis.SetActive(true);
				return;
			}
			break;
		case Bowl.State.HerbCooking:
			this.m_CookingDuration = 0f;
			if (this.m_ItemVis)
			{
				this.m_ItemVis.SetActive(true);
				return;
			}
			break;
		case Bowl.State.HerbCooked:
		{
			this.m_HerbVis.SetActive(true);
			LiquidData liquidData = LiquidManager.Get().GetLiquidData(this.m_LCInfo.m_LiquidType);
			bool flag = false;
			foreach (string text in this.m_HerbVisualisations.Keys)
			{
				bool flag2 = liquidData != null && text == liquidData.m_ItemComponent.ToString();
				this.m_HerbVisualisations[text].SetActive(flag2);
				if (!flag && flag2)
				{
					flag = true;
				}
			}
			if (!flag && this.m_DefaultVis)
			{
				this.m_DefaultVis.SetActive(true);
			}
			break;
		}
		default:
			return;
		}
	}

	private void InitializeAudio()
	{
		if (!this.m_AudioInitialized)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
		}
		if (Bowl.s_BoilingSound.Count == 0)
		{
			AudioClip item = Resources.Load("Sounds/Items/GH_Gotowanie_Wody_02") as AudioClip;
			Bowl.s_BoilingSound.Add(item);
			item = (Resources.Load("Sounds/Items/GH_Gotowanie_Wody_04") as AudioClip);
			Bowl.s_BoilingSound.Add(item);
			item = (Resources.Load("Sounds/Items/GH_Gotowanie_Wody_05_mocne") as AudioClip);
			Bowl.s_BoilingSound.Add(item);
			item = (Resources.Load("Sounds/Items/GH_Water_Boiling_02") as AudioClip);
			Bowl.s_BoilingSound.Add(item);
			item = (Resources.Load("Sounds/Items/GH_Water_Boliing_01") as AudioClip);
			Bowl.s_BoilingSound.Add(item);
		}
	}

	private void CreateHerb()
	{
		if (!this.m_Item)
		{
			return;
		}
		LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)this.m_Info;
		LiquidData liquidDataByComponents = LiquidManager.Get().GetLiquidDataByComponents(liquidContainerInfo.m_LiquidType, this.m_Item.m_Info.m_ID);
		if (liquidDataByComponents != null)
		{
			liquidContainerInfo.m_LiquidType = liquidDataByComponents.m_LiquidType;
		}
		else
		{
			liquidContainerInfo.m_LiquidType = LiquidType.Default_Infusion;
		}
		ItemsManager.Get().OnLiquidBoiled(liquidContainerInfo.m_LiquidType);
		if (liquidContainerInfo.m_LiquidType == LiquidType.None)
		{
			DebugUtils.Assert("Can't set liquid type - " + this.m_Item.m_Info.m_ID.ToString(), true, DebugUtils.AssertType.Info);
		}
		UnityEngine.Object.Destroy(this.m_Item.gameObject);
		this.m_Item = null;
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateParticle();
		this.UpdateState();
		this.UpdateSounds();
		if (!this.ReplIsOwner())
		{
			return;
		}
		if (base.IsInWater() && !base.m_InInventory)
		{
			WaterCollider waterCollider = this.m_CurrentWaters[this.m_CurrentWaters.Count - 1];
			this.m_LCInfo.m_LiquidType = (waterCollider.m_LiquidSource ? waterCollider.m_LiquidSource.m_LiquidType : LiquidType.UnsafeWater);
			this.m_LCInfo.m_Amount = this.m_LCInfo.m_Capacity;
			return;
		}
		if (this.m_LCInfo.m_Amount < this.m_LCInfo.m_Capacity && !base.transform.parent && base.transform.up.y > 0.45f && RainManager.Get().IsRain() && !RainManager.Get().IsInRainCutter(base.transform.position))
		{
			bool flag = true;
			if (Inventory3DManager.Get().m_CarriedItem == this && RainManager.Get().IsInRainCutter(Player.Get().transform.position))
			{
				flag = false;
			}
			if (flag)
			{
				if (this.m_LCInfo.m_LiquidType != LiquidType.Water && this.m_LCInfo.m_Amount == 0f)
				{
					this.m_LCInfo.m_LiquidType = LiquidType.Water;
				}
				if (this.m_LCInfo.m_LiquidType == LiquidType.Water)
				{
					float num = Time.deltaTime;
					if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
					{
						num = Player.GetSleepTimeFactor();
					}
					this.m_LCInfo.m_Amount += num * 0.2f;
					this.m_LCInfo.m_Amount = Mathf.Clamp(this.m_LCInfo.m_Amount, 0f, this.m_LCInfo.m_Capacity);
				}
			}
		}
	}

	private void UpdateSpoiling()
	{
		if (this.m_SpoilTime <= 0f)
		{
			this.m_SpoilingDuration = 0f;
			return;
		}
		if (base.transform.parent)
		{
			this.m_SpoilingDuration = 0f;
			return;
		}
		if (this.m_LCInfo.m_Amount == 0f || this.m_LCInfo.m_LiquidType != LiquidType.Water)
		{
			this.m_SpoilingDuration = 0f;
			return;
		}
		float num = Time.deltaTime;
		if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
		{
			num = Player.GetSleepTimeFactor();
		}
		this.m_SpoilingDuration += num;
		if (this.m_SpoilingDuration >= this.m_SpoilTime)
		{
			this.m_LCInfo.m_LiquidType = LiquidType.UnsafeWater;
			this.m_SpoilingDuration = 0f;
		}
	}

	private void UpdateParticle()
	{
		bool flag = this.m_State != Bowl.State.None && this.m_Firecamp && this.m_Firecamp.m_Burning;
		if (flag != this.m_ParticleActive)
		{
			this.m_ParticleActive = flag;
			this.m_Particle.SetActive(this.m_ParticleActive);
		}
	}

	private void UpdateState()
	{
		LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)this.m_Info;
		if (this.m_State != Bowl.State.None && liquidContainerInfo.m_Amount == 0f)
		{
			this.SetState(Bowl.State.None);
			return;
		}
		switch (this.m_State)
		{
		case Bowl.State.None:
			if (liquidContainerInfo.m_Amount >= 1f)
			{
				LiquidType liquidType = liquidContainerInfo.m_LiquidType;
				if (liquidType == LiquidType.UnsafeWater || liquidType == LiquidType.DirtyWater)
				{
					this.SetState(Bowl.State.UnsafeWater);
					return;
				}
				if (liquidType == LiquidType.Water)
				{
					this.SetState(Bowl.State.WaterCooked);
					return;
				}
				this.SetState(Bowl.State.HerbCooked);
				return;
			}
			break;
		case Bowl.State.UnsafeWater:
			if (this.m_Firecamp && this.m_Firecamp.m_Burning)
			{
				this.SetState(Bowl.State.UnsafeWaterCooking);
				return;
			}
			break;
		case Bowl.State.UnsafeWaterCooking:
			this.m_CookingDuration += Time.deltaTime;
			if (this.m_CookingDuration >= this.m_CookingLength)
			{
				this.SetState(Bowl.State.WaterCooked);
				return;
			}
			if (this.m_Firecamp && !this.m_Firecamp.m_Burning)
			{
				this.SetState(Bowl.State.UnsafeWater);
				return;
			}
			break;
		case Bowl.State.WaterCooked:
			if (this.m_Item)
			{
				this.SetState(Bowl.State.Herb);
				return;
			}
			this.m_ItemSlot.gameObject.SetActive(this.m_Firecamp != null);
			return;
		case Bowl.State.Herb:
			if (this.m_Firecamp && this.m_Firecamp.m_Burning)
			{
				this.SetState(Bowl.State.HerbCooking);
				return;
			}
			break;
		case Bowl.State.HerbCooking:
			this.m_CookingDuration += Time.deltaTime;
			if (this.m_CookingDuration >= this.m_CookingLength)
			{
				this.CreateHerb();
				this.SetState(Bowl.State.HerbCooked);
				return;
			}
			if (this.m_Firecamp && !this.m_Firecamp.m_Burning)
			{
				this.SetState(Bowl.State.Herb);
			}
			break;
		case Bowl.State.HerbCooked:
			break;
		default:
			return;
		}
	}

	private void UpdateSounds()
	{
		Bowl.State state = this.m_State;
		if (state == Bowl.State.UnsafeWaterCooking || state == Bowl.State.HerbCooking)
		{
			if (this.m_CookingDuration >= this.m_CookingLength * 0.5f && this.m_AudioSource != null && !this.m_AudioSource.isPlaying)
			{
				this.m_AudioSource.clip = Bowl.s_BoilingSound[UnityEngine.Random.Range(0, Bowl.s_BoilingSound.Count)];
				this.m_AudioSource.spatialBlend = 1f;
				this.m_AudioSource.Play();
				return;
			}
		}
		else if (this.m_AudioSource != null && this.m_AudioSource.isPlaying)
		{
			this.m_AudioSource.Stop();
		}
	}

	public Texture GetHUDIcon()
	{
		switch (this.m_State)
		{
		case Bowl.State.None:
		case Bowl.State.UnsafeWater:
		case Bowl.State.UnsafeWaterCooking:
			return this.m_EmptyIcon;
		case Bowl.State.WaterCooked:
			return this.m_CookedIcon;
		case Bowl.State.HerbCooking:
		case Bowl.State.HerbCooked:
			return this.m_HerbIcon;
		}
		return null;
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("BowlState" + index, (int)this.m_State);
		SaveGame.SaveVal("BowlSpoilingDur" + index, this.m_SpoilingDuration);
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.SetState((Bowl.State)SaveGame.LoadIVal("BowlState" + index));
		if (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionReleaseCandidate)
		{
			this.m_SpoilingDuration = SaveGame.LoadFVal("BowlSpoilingDur" + index);
		}
	}

	protected override void UpdateSlotsActivity()
	{
		if (base.m_InInventory)
		{
			this.m_PourSlot.gameObject.SetActive(false);
			this.m_GetSlot.gameObject.SetActive(false);
			return;
		}
		base.UpdateSlotsActivity();
	}

	public override void OnAddToInventory()
	{
		base.OnAddToInventory();
		this.Spill(-1f);
		this.SetState(Bowl.State.None);
	}

	public override void OnAddToStorage(Storage storage)
	{
		base.OnAddToStorage(storage);
		this.Spill(-1f);
		this.SetState(Bowl.State.None);
	}

	public override void OnTake()
	{
		base.OnTake();
		this.Spill(-1f);
		this.SetState(Bowl.State.None);
	}

	public override void Spill(float amount = -1f)
	{
		base.Spill(-1f);
		if (this.m_ItemVis)
		{
			this.m_ItemVis.gameObject.SetActive(false);
			this.m_ItemVis = null;
		}
	}

	public override void OnReplicationPrepare_CJGenerated()
	{
		base.OnReplicationPrepare_CJGenerated();
		if (Math.Abs(this.m_CookingDuration_Repl - this.m_CookingDuration) >= 1f)
		{
			this.m_CookingDuration_Repl = this.m_CookingDuration;
			this.ReplSetDirty();
		}
	}

	public override void OnReplicationSerialize_CJGenerated(P2PNetworkWriter writer, bool initial_state)
	{
		base.OnReplicationSerialize_CJGenerated(writer, initial_state);
		writer.Write(this.m_CookingDuration_Repl);
	}

	public override void OnReplicationDeserialize_CJGenerated(P2PNetworkReader reader, bool initial_state)
	{
		base.OnReplicationDeserialize_CJGenerated(reader, initial_state);
		this.m_CookingDuration_Repl = reader.ReadFloat();
	}

	public override void OnReplicationResolve_CJGenerated()
	{
		base.OnReplicationResolve_CJGenerated();
		this.m_CookingDuration = this.m_CookingDuration_Repl;
	}

	public Bowl.State m_State;

	private Firecamp m_Firecamp;

	private GameObject m_UnsafeWaterVis;

	private GameObject m_WaterVis;

	private GameObject m_HerbVis;

	private Dictionary<string, GameObject> m_HerbVisualisations = new Dictionary<string, GameObject>();

	private ItemSlot m_ItemSlot;

	private Item m_Item;

	private GameObject m_ItemVis;

	private GameObject m_Particle;

	private bool m_ParticleActive;

	private Dictionary<string, GameObject> m_ItemVisualisations = new Dictionary<string, GameObject>();

	private GameObject m_DefaultVis;

	[Replicate(new string[]
	{
		"precision:1"
	})]
	private float m_CookingDuration;

	public float m_CookingLength = 10f;

	public Texture m_EmptyIcon;

	public Texture m_CookedIcon;

	public Texture m_HerbIcon;

	private AudioSource m_AudioSource;

	[HideInInspector]
	public bool m_RackChild;

	private float m_SpoilingDuration;

	public float m_SpoilTime;

	private bool m_AudioInitialized;

	private static List<AudioClip> s_BoilingSound = new List<AudioClip>();

	private float m_CookingDuration_Repl;

	public enum State
	{
		None,
		UnsafeWater,
		UnsafeWaterCooking,
		WaterCooked,
		Herb,
		HerbCooking,
		HerbCooked
	}
}
