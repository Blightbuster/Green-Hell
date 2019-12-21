using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class MudMixer : Construction, IProcessor, IItemSlotParent, IGhostPartParent, ITriggerThrough
{
	protected override void Awake()
	{
		base.Awake();
		this.m_Sound = base.gameObject.GetComponent<AudioSource>();
		this.m_MixingSound = this.m_Sound.clip;
		this.m_InsertMudSound = (AudioClip)Resources.Load("Sounds/Items/mud_from_water_drop_01");
		this.m_AshSlot.gameObject.SetActive(false);
		this.m_WaterSlot.gameObject.SetActive(false);
		base.RegisterConstantUpdateItem();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.UnregisterConstantUpdateItem();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (this.m_State == MudMixer.State.None)
		{
			this.SetState(MudMixer.State.WaitingForClay0);
		}
	}

	public override bool IsMudMixer()
	{
		return true;
	}

	public override void SetLayer(Transform trans, int layer)
	{
		trans.gameObject.layer = layer;
	}

	public float GetProcessProgress(Trigger trigger)
	{
		return this.m_WaterAmount / this.m_RequiredWaterAmount;
	}

	private void SetState(MudMixer.State state)
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
		switch (this.m_State)
		{
		case MudMixer.State.WaitingForClay0:
			this.m_AshSlot.gameObject.SetActive(false);
			this.m_WaterSlot.gameObject.SetActive(false);
			this.m_Mud0.Reset();
			this.m_Mud0.gameObject.SetActive(true);
			this.m_Mud0.enabled = true;
			this.m_Mud1.gameObject.SetActive(false);
			this.m_Mud1.enabled = false;
			this.m_Ash.gameObject.SetActive(false);
			this.m_Ash.enabled = false;
			return;
		case MudMixer.State.WaitingForClay1:
			this.m_AshSlot.gameObject.SetActive(false);
			this.m_WaterSlot.gameObject.SetActive(false);
			this.m_Mud0.gameObject.SetActive(true);
			this.m_Mud0.enabled = false;
			this.m_Mud1.Reset();
			this.m_Mud1.gameObject.SetActive(true);
			this.m_Mud1.enabled = true;
			this.m_Ash.gameObject.SetActive(false);
			this.m_Ash.enabled = false;
			return;
		case MudMixer.State.WaitingForAsh:
			this.m_AshSlot.gameObject.SetActive(false);
			this.m_WaterSlot.gameObject.SetActive(false);
			this.m_Mud0.gameObject.SetActive(true);
			this.m_Mud0.enabled = false;
			this.m_Mud1.gameObject.SetActive(true);
			this.m_Mud1.enabled = false;
			this.m_Ash.Reset();
			this.m_Ash.gameObject.SetActive(true);
			this.m_Ash.enabled = true;
			return;
		case MudMixer.State.WaitingForWater:
			this.m_WaterAmount = 0f;
			HUDProcess.Get().RegisterProcess(this, "HUD_pourOut_water", this, true);
			this.m_AshSlot.gameObject.SetActive(false);
			this.m_WaterSlot.gameObject.SetActive(true);
			this.m_Mud0.gameObject.SetActive(true);
			this.m_Mud0.enabled = false;
			this.m_Mud1.gameObject.SetActive(true);
			this.m_Mud1.enabled = false;
			this.m_Ash.gameObject.SetActive(true);
			this.m_Ash.enabled = false;
			return;
		case MudMixer.State.Full:
			this.m_AshSlot.gameObject.SetActive(false);
			this.m_WaterSlot.gameObject.SetActive(false);
			this.m_Mud0.gameObject.SetActive(true);
			this.m_Mud0.enabled = false;
			this.m_Mud1.gameObject.SetActive(true);
			this.m_Mud1.enabled = false;
			this.m_Ash.gameObject.SetActive(true);
			this.m_Ash.enabled = false;
			HUDProcess.Get().UnregisterProcess(this);
			this.m_WaterAmount = 0f;
			return;
		case MudMixer.State.Mix:
			MudMixerController.Get().SetMixer(this);
			Player.Get().HideWeapon();
			Player.Get().StartController(PlayerControllerType.MudMixer);
			this.m_Sound.clip = this.m_MixingSound;
			this.m_Sound.Play();
			return;
		case MudMixer.State.Ready:
			this.m_Sound.Stop();
			this.m_AshSlot.gameObject.SetActive(false);
			this.m_WaterSlot.gameObject.SetActive(false);
			this.m_Mud0.gameObject.SetActive(false);
			this.m_Mud0.enabled = false;
			this.m_Mud1.gameObject.SetActive(false);
			this.m_Mud1.enabled = false;
			this.m_Ash.gameObject.SetActive(false);
			this.m_Ash.enabled = false;
			return;
		default:
			return;
		}
	}

	public override void ConstantUpdate()
	{
		if (ItemsManager.Get().m_ItemsToSetupAfterLoad.Count > 0)
		{
			return;
		}
		base.ConstantUpdate();
		if (this.m_State == MudMixer.State.WaitingForWater && this.m_WaterAmount < this.m_RequiredWaterAmount && RainManager.Get().IsRain() && !RainManager.Get().IsInRainCutter(base.transform.position))
		{
			float num = Time.deltaTime;
			if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
			{
				num = Player.GetSleepTimeFactor();
			}
			this.m_WaterAmount += num * 8f;
			this.m_WaterAmount = Mathf.Clamp(this.m_WaterAmount, 0f, this.m_RequiredWaterAmount);
			if (this.m_WaterAmount >= this.m_RequiredWaterAmount)
			{
				this.SetState(MudMixer.State.Full);
				return;
			}
		}
		else if (this.m_State == MudMixer.State.Ready)
		{
			int i = 0;
			while (i < this.m_ResultItems.Count)
			{
				if (this.m_ResultItems[i] == null || this.m_ResultItems[i].m_WasTriggered)
				{
					this.m_ResultItems.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
			if (this.m_ResultItems.Count == 0)
			{
				this.SetState(MudMixer.State.WaitingForClay0);
			}
		}
	}

	public bool CanInsertItem(Item item)
	{
		return this.m_State == MudMixer.State.WaitingForAsh || this.m_State == MudMixer.State.WaitingForWater;
	}

	public void OnInsertItem(ItemSlot slot)
	{
		if (slot == this.m_WaterSlot)
		{
			Item item = slot.m_Item;
			LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)item.m_Info;
			float waterAmount = this.m_WaterAmount;
			this.m_WaterAmount += liquidContainerInfo.m_Amount;
			this.m_WaterAmount = Mathf.Min(this.m_WaterAmount, this.m_RequiredWaterAmount);
			float num = this.m_WaterAmount - waterAmount;
			liquidContainerInfo.m_Amount -= num;
			slot.RemoveItem();
			InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true);
			if (this.m_WaterAmount >= this.m_RequiredWaterAmount)
			{
				this.SetState(MudMixer.State.Full);
			}
			if (num > 0f)
			{
				PlayerAudioModule.Get().PlayWaterSpillSound(1f, false);
				return;
			}
		}
		else if (slot == this.m_AshSlot)
		{
			UnityEngine.Object.Destroy(slot.m_Item.gameObject);
			this.SetState(MudMixer.State.WaitingForWater);
		}
	}

	public void OnRemoveItem(ItemSlot slot)
	{
	}

	public Material GetActiveMaterial()
	{
		return this.m_ActiveMaterial;
	}

	public Material GetHighlightedMaterial()
	{
		return this.m_HighlightedMaterial;
	}

	public void OnGhostFulfill(bool from_save)
	{
		if (this.m_State == MudMixer.State.WaitingForClay0)
		{
			this.SetState(MudMixer.State.WaitingForClay1);
		}
		else if (this.m_State == MudMixer.State.WaitingForClay1)
		{
			this.SetState(MudMixer.State.WaitingForAsh);
		}
		else if (this.m_State == MudMixer.State.WaitingForAsh)
		{
			this.SetState(MudMixer.State.WaitingForWater);
		}
		this.m_Sound.PlayOneShot(this.m_InsertMudSound);
	}

	public override bool ShowAdditionalInfo()
	{
		return this.m_State == MudMixer.State.WaitingForAsh || this.m_State == MudMixer.State.WaitingForWater;
	}

	public override string GetAdditionalInfoLocalized()
	{
		return GreenHellGame.Instance.GetLocalization().Get("HUD_Trigger_Ember_Req_Additional_Info", true);
	}

	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && !HeavyObjectController.Get().IsActive() && this.m_State == MudMixer.State.Full;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		actions.Add(TriggerAction.TYPE.Use);
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Use)
		{
			this.SetState(MudMixer.State.Mix);
		}
	}

	public void OnFinishMixing()
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_ResultPrefab, base.transform.position, base.transform.rotation);
		this.m_ResultItems = new List<Item>(gameObject.GetComponentsInChildren<Item>());
		foreach (Item item in this.m_ResultItems)
		{
			item.transform.parent = null;
			item.m_StaticPhx = true;
			item.UpdatePhx();
		}
		UnityEngine.Object.Destroy(gameObject);
		this.SetState(MudMixer.State.Ready);
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("MudMixerState" + index, (int)this.m_State);
		SaveGame.SaveVal("MudMixerWAmount" + index, this.m_WaterAmount);
		SaveGame.SaveVal("MudMixerResultCount" + index, this.m_ResultItems.Count);
		for (int i = 0; i < this.m_ResultItems.Count; i++)
		{
			SaveGame.SaveVal("MudMixerResult" + index * 666 + i.ToString(), this.m_ResultItems[i].m_UniqueID);
		}
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.m_State = MudMixer.State.None;
		this.SetState((MudMixer.State)SaveGame.LoadIVal("MudMixerState" + index));
		this.m_WaterAmount = SaveGame.LoadFVal("MudMixerWAmount" + index);
	}

	public override void SetupAfterLoad(int index)
	{
		base.SetupAfterLoad(index);
		this.m_ResultItems.Clear();
		int num = SaveGame.LoadIVal("MudMixerResultCount" + index);
		for (int i = 0; i < num; i++)
		{
			int num2 = SaveGame.LoadIVal("MudMixerResult" + index * 666 + i.ToString());
			foreach (Item item in Item.s_AllItems)
			{
				if (item.m_UniqueID == num2)
				{
					this.m_ResultItems.Add(item);
					break;
				}
			}
		}
	}

	public override void DestroyMe(bool check_connected = true)
	{
		base.DestroyMe(check_connected);
		if (this.m_Mud0.m_Fulfilled && UnityEngine.Random.Range(0f, 1f) < 0.5f)
		{
			ItemsManager.Get().CreateItem(this.m_Mud0.m_ItemID, true, base.transform.position + UnityEngine.Random.insideUnitSphere * 0.2f, base.transform.rotation);
		}
		if (this.m_Mud1.m_Fulfilled && UnityEngine.Random.Range(0f, 1f) < 0.5f)
		{
			ItemsManager.Get().CreateItem(this.m_Mud1.m_ItemID, true, base.transform.position + UnityEngine.Random.insideUnitSphere * 0.2f, base.transform.rotation);
		}
		if (this.m_State == MudMixer.State.WaitingForWater && UnityEngine.Random.Range(0f, 1f) < 0.5f)
		{
			ItemsManager.Get().CreateItem(ItemID.Campfire_ash, true, base.transform.position + UnityEngine.Random.insideUnitSphere * 0.2f, base.transform.rotation);
		}
	}

	private MudMixer.State m_State;

	public float m_RequiredWaterAmount = 200f;

	private float m_WaterAmount;

	public GhostPart m_Mud0;

	public GhostPart m_Mud1;

	public GhostPart m_Ash;

	public Material m_ActiveMaterial;

	public Material m_HighlightedMaterial;

	public ItemSlot m_AshSlot;

	public ItemSlot m_WaterSlot;

	private AudioSource m_Sound;

	private AudioClip m_MixingSound;

	private AudioClip m_InsertMudSound;

	public GameObject m_ResultPrefab;

	private List<Item> m_ResultItems = new List<Item>();

	private enum State
	{
		None,
		WaitingForClay0,
		WaitingForClay1,
		WaitingForAsh,
		WaitingForWater,
		Full,
		Mix,
		Ready
	}
}
