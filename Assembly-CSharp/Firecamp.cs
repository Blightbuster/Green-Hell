using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class Firecamp : Construction, IItemSlotParent
{
	protected override void Awake()
	{
		base.Awake();
		Firecamp.s_Firecamps.Add(this);
		this.m_Sound = base.gameObject.GetComponent<AudioSource>();
		for (int i = 0; i < 100; i++)
		{
			Transform transform = base.transform.Find("Mesh_0" + i);
			if (!transform)
			{
				break;
			}
			this.m_MeshLevels.Add(transform.gameObject);
		}
		if (this.m_WoodSlot)
		{
			this.m_WoodSlot.m_ActivityUpdate = false;
		}
		for (int j = 0; j < this.m_CookingSlots.Length; j++)
		{
			this.m_CookingSlots[j].m_ActivityUpdate = false;
		}
		this.m_DamageBounds = this.m_BoxCollider.bounds;
		this.m_DamageBounds.Expand(0.5f);
	}

	protected override void Start()
	{
		base.Start();
		this.SetupEffects();
		this.m_FireLevel = 1f;
		if (this.m_IgniteOnStart)
		{
			this.StartBurning();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (HUDFirecamp.Get())
		{
			HUDFirecamp.Get().UnregisterFirecamp(this);
		}
		Firecamp.s_Firecamps.Remove(this);
	}

	public static bool IsAnyBurning()
	{
		for (int i = 0; i < Firecamp.s_Firecamps.Count; i++)
		{
			if (Firecamp.s_Firecamps[i].m_Burning)
			{
				return true;
			}
		}
		return false;
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_Burning)
		{
			this.m_BurningDuration += MainLevel.Instance.m_TODSky.Cycle.GameTimeDelta;
		}
		this.CheckRain();
		this.UpdateHUD();
		this.UpdateFireLevel();
		this.UpdateLightIntensity();
		this.UpdateSlots();
		this.UpdateLightNoise();
		this.UpdateMesh();
		this.UpdatePlayerSanity();
		this.UpdateDamage();
		if (this.m_DebugIgnite)
		{
			this.StartBurning();
			this.m_DebugIgnite = false;
		}
		if (this.m_DebugExtinguish)
		{
			this.Extinguish();
			this.m_DebugExtinguish = false;
		}
	}

	private void UpdateDamage()
	{
		if (!this.m_Burning)
		{
			return;
		}
		if (Time.time - this.m_LastDamageTime < 1.5f)
		{
			return;
		}
		if (this.m_DamageBounds.Contains(Player.Get().transform.position))
		{
			float proportionalClamp = CJTools.Math.GetProportionalClamp(5f, 10f, this.m_FireLevel, 0f, 1f);
			Player.Get().GiveDamage(base.gameObject, null, proportionalClamp, Vector3.up, DamageType.None, 0, false);
			this.m_LastDamageTime = Time.time;
		}
	}

	private void CheckRain()
	{
		if (!this.m_Burning)
		{
			this.m_RainDuration = 0f;
			return;
		}
		if (RainManager.Get().IsRain() && !RainManager.Get().IsInRainCutter(base.transform.position))
		{
			this.m_RainDuration += Time.deltaTime;
			if (this.m_RainDuration >= this.m_MinRainDurationToExtinguish)
			{
				this.Extinguish();
			}
		}
		else
		{
			this.m_RainDuration = 0f;
		}
	}

	private void UpdateSlots()
	{
		bool flag = this.m_Burning && this.m_FireLevel < 1f;
		if (this.m_WoodSlot.m_Active != flag)
		{
			this.m_WoodSlot.Activate(flag);
		}
		for (int i = 0; i < this.m_CookingSlots.Length; i++)
		{
			if (!this.m_CookingSlots[i].m_Active)
			{
				this.m_CookingSlots[i].Activate(true);
			}
		}
	}

	private void UpdateLightNoise()
	{
		if (!this.m_Burning)
		{
			return;
		}
		float num = (Mathf.Sin(Time.time * this.m_LightNoiseSpeed) + Mathf.Sin(Time.time * this.m_LightNoiseSpeed * 1.4f)) * this.m_LightNoiseRange;
		this.m_Light.range = this.m_LightRange + num;
	}

	private void UpdateHUD()
	{
		if (this.m_NextUpdateHUDTime > Time.time)
		{
			return;
		}
		float num = Vector3.Distance(Player.Get().transform.position, base.transform.position);
		if (num <= HUDFirecamp.s_DistToActivate)
		{
			if (!this.m_HUDActive)
			{
				HUDFirecamp.Get().RegisterFirecamp(this);
				this.m_HUDActive = true;
			}
		}
		else if (this.m_HUDActive)
		{
			HUDFirecamp.Get().UnregisterFirecamp(this);
			this.m_HUDActive = false;
		}
		this.m_NextUpdateHUDTime = Time.time + CJTools.Math.GetProportionalClamp(0.1f, 5f, num, 5f, 50f);
	}

	private void UpdateFireLevel()
	{
		if (!this.m_Burning)
		{
			return;
		}
		if (this.m_EndlessFire)
		{
			this.m_FireLevel = 1f;
			return;
		}
		this.m_FireLevel = CJTools.Math.GetProportionalClamp(1f, 0f, this.m_BurningDuration, 0f, this.m_BurningLength);
		if (this.m_FireLevel == 0f)
		{
			this.BurnOut();
		}
	}

	private void UpdateLightIntensity()
	{
		if (this.m_Burning && this.m_Light.intensity < this.m_MaxLightIntensity)
		{
			float proportionalClamp = CJTools.Math.GetProportionalClamp(0.3f, 1f, this.m_FireLevel, 0f, 1f);
			float num = this.m_MaxLightIntensity * proportionalClamp;
			if (this.m_LightIntensityChangeDuration > 0f)
			{
				this.m_Light.intensity += Time.deltaTime / this.m_LightIntensityChangeDuration;
				this.m_Light.intensity = Mathf.Clamp(this.m_Light.intensity, 0f, num);
			}
			else
			{
				this.m_Light.intensity = num;
			}
			for (int i = 0; i < this.m_Particles.Count; i++)
			{
				this.m_Particles[i].emission.rateOverTime = proportionalClamp * 10f;
			}
		}
		else if (!this.m_Burning)
		{
			this.m_Light.intensity = 0f;
		}
	}

	private void UpdateMesh()
	{
		int count = this.m_MeshLevels.Count;
		if (count == 0)
		{
			return;
		}
		int num = (int)CJTools.Math.GetProportionalClamp(0f, (float)count, this.m_FireLevel, 1f, 0f);
		for (int i = 0; i < count; i++)
		{
			this.m_MeshLevels[i].SetActive(i == num);
		}
	}

	private void UpdatePlayerSanity()
	{
		if (this.m_Burning && Player.Get().transform.position.Distance(base.transform.position) < 5f)
		{
			this.m_PlayerCloseDuration += Time.deltaTime;
		}
		else
		{
			this.m_PlayerCloseDuration = 0f;
		}
		if (this.m_PlayerCloseDuration >= PlayerSanityModule.Get().GetEventInterval(PlayerSanityModule.SanityEventType.Firecamp))
		{
			PlayerSanityModule.Get().OnEvent(PlayerSanityModule.SanityEventType.Firecamp, 1);
			this.m_PlayerCloseDuration = 0f;
		}
	}

	private void SetupEffects()
	{
		this.m_Light.gameObject.SetActive(this.m_Burning);
		for (int i = 0; i < this.m_Particles.Count; i++)
		{
			this.m_Particles[i].gameObject.SetActive(this.m_Burning);
		}
		if (this.m_FireAnim)
		{
			this.m_FireAnim.SetActive(this.m_Burning);
		}
		if (this.m_Burning)
		{
			this.m_Sound.Play();
		}
		else
		{
			this.m_Sound.Stop();
		}
		this.SetupMaterial(base.transform);
	}

	private void SetupMaterial(Transform trans)
	{
		for (int i = 0; i < trans.childCount; i++)
		{
			Transform child = trans.GetChild(i);
			if (child.name.Contains("small_stick"))
			{
				child.GetComponent<MeshRenderer>().material = ((!this.m_Burning) ? ((this.m_FireLevel != 1f && this.m_BurnoutMat) ? this.m_BurnoutMat : this.m_NormalSmallStickMat) : this.m_BurningStickMat);
			}
			else if (child.name.Contains("stick"))
			{
				child.GetComponent<MeshRenderer>().material = ((!this.m_Burning) ? ((this.m_FireLevel != 1f && this.m_BurnoutMat) ? this.m_BurnoutMat : this.m_NormalStickMat) : this.m_BurningStickMat);
			}
			this.SetupMaterial(child);
		}
	}

	private void SetupSlots()
	{
		foreach (ItemSlot itemSlot in this.m_CookingSlots)
		{
			if (itemSlot.m_Item == null)
			{
				itemSlot.gameObject.SetActive(true);
			}
		}
		this.m_WoodSlot.gameObject.SetActive(true);
	}

	public override bool CanTrigger()
	{
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		if (currentItem)
		{
			ItemID id = currentItem.m_Info.m_ID;
			if (id == ItemID.Fire && !this.m_Burning)
			{
				return true;
			}
			if (id == ItemID.Torch || id == ItemID.Weak_Torch || id == ItemID.Tobacco_Torch)
			{
				Torch torch = (Torch)currentItem;
				if (torch.m_Burning != this.m_Burning)
				{
					return true;
				}
			}
		}
		return this.ShouldShowEmberRequired();
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		if (!currentItem)
		{
			return;
		}
		ItemID id = currentItem.m_Info.m_ID;
		if (id == ItemID.Fire && !this.m_Burning)
		{
			actions.Add(TriggerAction.TYPE.Ignite);
		}
		if (id == ItemID.Torch || id == ItemID.Weak_Torch || id == ItemID.Tobacco_Torch)
		{
			Torch torch = (Torch)currentItem;
			if (torch.m_Burning != this.m_Burning)
			{
				actions.Add(TriggerAction.TYPE.Ignite);
			}
		}
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Ignite)
		{
			Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
			if (currentItem && currentItem.GetInfoID() == ItemID.Fire)
			{
				ItemController.Get().IgniteFirecamp(this);
			}
			else if (action == TriggerAction.TYPE.Ignite)
			{
				Player.Get().m_TorchController.OnFirecampIgnite(this);
			}
		}
	}

	public void StartBurning()
	{
		if (this.m_Burning)
		{
			return;
		}
		this.m_Burning = true;
		this.OnStartBurning();
		EventsManager.OnEvent(Enums.Event.IgniteFire, 1);
	}

	private void OnStartBurning()
	{
		this.m_RainDuration = 0f;
		this.SetupEffects();
		this.SetupSlots();
	}

	public void Extinguish()
	{
		this.m_Burning = false;
		this.m_WoodSlot.gameObject.SetActive(false);
		this.SetupEffects();
	}

	private void BurnOut()
	{
		for (int i = 0; i < this.m_CookingSlots.Length; i++)
		{
			this.m_CookingSlots[i].RemoveItem();
		}
		if (this.m_AshPrefab != null)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_AshPrefab, base.transform.position, base.transform.rotation);
		}
		GameObject prefab = GreenHellGame.Instance.GetPrefab("Charcoal");
		UnityEngine.Object.Instantiate<GameObject>(prefab, base.transform.position + Vector3.up * 0.1f + Vector3.forward * 0.08f, base.transform.rotation);
		UnityEngine.Object.Instantiate<GameObject>(prefab, base.transform.position + Vector3.up * 0.1f + Vector3.left * 0.08f, base.transform.rotation);
		UnityEngine.Object.Instantiate<GameObject>(prefab, base.transform.position + Vector3.up * 0.1f + Vector3.right * 0.08f, base.transform.rotation);
		HUDFirecamp.Get().UnregisterFirecamp(this);
		Firecamp.s_Firecamps.Remove(this);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public override bool HasRestInfluence()
	{
		return this.m_Burning;
	}

	public bool CanInsertItem(Item item)
	{
		return true;
	}

	public void OnInsertItem(ItemSlot slot)
	{
		if (slot == this.m_WoodSlot)
		{
			this.m_BurningDuration -= slot.m_Item.m_Info.m_AddFirecamBurningTime;
			if (this.m_BurningDuration < 0f)
			{
				this.m_BurningDuration = 0f;
			}
			UnityEngine.Object.Destroy(slot.m_Item.gameObject);
			return;
		}
		bool flag = false;
		for (int i = 0; i < this.m_CookingSlots.Length; i++)
		{
			if (slot == this.m_CookingSlots[i])
			{
				flag = true;
				break;
			}
		}
		if (flag && slot.m_Item.m_Info.m_Type == ItemType.Bowl)
		{
			Collider component = slot.m_Item.gameObject.GetComponent<Collider>();
			component.isTrigger = true;
			slot.Deactivate();
			Bowl bowl = (Bowl)slot.m_Item;
			bowl.OnFirecampAdd(this);
		}
	}

	public void OnRemoveItem(ItemSlot slot)
	{
		if (slot.m_Item && slot.m_Item.m_Info.m_Type == ItemType.Bowl)
		{
			if (!slot.m_IsBeingDestroyed)
			{
				slot.gameObject.SetActive(true);
			}
			Bowl bowl = (Bowl)slot.m_Item;
			bowl.OnFirecampRemove(this);
		}
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("Fire" + index, this.m_Burning);
		SaveGame.SaveVal("BurningDuration" + index, this.m_BurningDuration);
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.m_Burning = SaveGame.LoadBVal("Fire" + index);
		this.m_BurningDuration = SaveGame.LoadFVal("BurningDuration" + index);
		if (this.m_Burning)
		{
			this.OnStartBurning();
		}
	}

	public override string GetIconName()
	{
		if (this.ShouldShowEmberRequired())
		{
			return "ember_required";
		}
		return base.GetIconName();
	}

	public override string GetTriggerInfoLocalized()
	{
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		if (!currentItem)
		{
			return GreenHellGame.Instance.GetLocalization().Get("HUD_Trigger_Ember_Required");
		}
		return base.GetTriggerInfoLocalized();
	}

	private bool ShouldShowEmberRequired()
	{
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		bool flag = false;
		if (currentItem)
		{
			ItemID id = currentItem.m_Info.m_ID;
			flag = (id == ItemID.Fire || id == ItemID.Torch || id == ItemID.Weak_Torch || id == ItemID.Tobacco_Torch);
		}
		return !flag && !this.m_Burning;
	}

	public override bool ShowAdditionalInfo()
	{
		return this.ShouldShowEmberRequired();
	}

	public override string GetAdditionalInfoLocalized()
	{
		return GreenHellGame.Instance.GetLocalization().Get("HUD_Trigger_Ember_Req_Additional_Info");
	}

	public bool m_Burning;

	public float m_BurningLength = 1f;

	private float m_BurningDuration;

	public float m_FireLevel;

	public List<ParticleSystem> m_Particles = new List<ParticleSystem>();

	public GameObject m_FireAnim;

	public Light m_Light;

	private float m_MaxLightIntensity = 2f;

	private float m_LightIntensityChangeDuration = 2f;

	public ItemSlot[] m_CookingSlots;

	public ItemSlot m_WoodSlot;

	public bool m_IgniteOnStart;

	public bool m_EndlessFire;

	private float m_NextUpdateHUDTime;

	private bool m_HUDActive;

	public bool m_DebugIgnite;

	public bool m_DebugExtinguish;

	public static List<Firecamp> s_Firecamps = new List<Firecamp>();

	public float m_LightRange = 1f;

	public float m_LightNoiseSpeed = 20f;

	public float m_LightNoiseRange = 0.1f;

	private float m_LastDamageTime;

	private Bounds m_DamageBounds;

	private List<GameObject> m_MeshLevels = new List<GameObject>();

	private float m_PlayerCloseDuration;

	public GameObject m_AshPrefab;

	private AudioSource m_Sound;

	public Material m_NormalStickMat;

	public Material m_NormalSmallStickMat;

	public Material m_BurningStickMat;

	public Material m_BurnoutMat;

	private float m_RainDuration;

	private float m_MinRainDurationToExtinguish = 15f;
}
