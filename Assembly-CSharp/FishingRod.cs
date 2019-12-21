using System;
using System.Collections.Generic;
using AIs;
using CJTools;
using Enums;
using UnityEngine;

public class FishingRod : MonoBehaviour, IItemSlotParent
{
	private void Awake()
	{
		this.m_FishingRodItem = base.gameObject.GetComponent<Item>();
		this.m_Vein.transform.parent = this.m_Top.transform;
		this.m_Vein.transform.localPosition = Vector3.zero;
		this.m_Vein.transform.localRotation = Quaternion.identity;
		if (this.m_HookSlot)
		{
			this.m_HookSlot.gameObject.SetActive(false);
		}
		this.m_DefaultFloatLocalPos = this.m_Float.transform.localPosition;
		this.m_DefaultFloatLocalRot = this.m_Float.transform.localRotation;
		this.m_Animator = base.gameObject.GetComponent<Animator>();
	}

	private void Start()
	{
		int num = 1;
		for (;;)
		{
			string name = "Bone" + ((num < 10) ? ("0" + num) : string.Concat(num));
			Transform transform = base.gameObject.transform.FindDeepChild(name);
			if (!transform)
			{
				break;
			}
			this.m_Bones.Add(transform.gameObject);
			num++;
		}
		base.enabled = false;
	}

	private void OnDisable()
	{
		this.SetState(FishingRod.State.None);
	}

	public bool CanInsertItem(Item item)
	{
		return true;
	}

	public void OnRemoveItem(ItemSlot slot)
	{
		if (this.m_HookSlot == slot && this.m_Hook)
		{
			if (slot.m_Item)
			{
				slot.m_Item.m_CantSave = false;
			}
			this.m_Hook.SetFishingRod(null);
			this.m_Hook = null;
		}
	}

	public void OnInsertItem(ItemSlot slot)
	{
		if (this.m_HookSlot == slot)
		{
			this.m_Hook = slot.m_Item.GetComponent<FishingHook>();
			this.m_Hook.SetFishingRod(this);
			slot.m_Item.m_CantSave = true;
			HintsManager.Get().ShowHint("FishRod_Bait", 10f);
		}
	}

	public void StartCast()
	{
		this.m_Animator.SetTrigger(this.m_FishingRodCast);
		this.SetState(FishingRod.State.Cast);
	}

	public void StartFishing(Vector3 pos)
	{
		this.m_FloatPos = pos;
		this.SetupFishTank();
		this.SetupHookInWaterPos();
		if (this.m_FishTank)
		{
			this.m_FishTank.StartFishing(this);
		}
		this.m_Fish = null;
		this.m_Biting = false;
		this.SetState(FishingRod.State.Fishing);
	}

	public void StopFishing()
	{
		if (this.m_Fish)
		{
			UnityEngine.Object.Destroy(this.m_Fish);
			this.m_Fish = null;
		}
		if (this.m_FishTank)
		{
			this.m_FishTank.StopFishing();
			this.m_FishTank = null;
		}
		this.m_Rotation = 0f;
		this.RotateBones();
		this.SetState(FishingRod.State.None);
	}

	public void Break()
	{
		this.SetState(FishingRod.State.None);
		this.m_Rotation = 0f;
		this.UpdateAll();
	}

	public void SetBiting(bool set)
	{
		this.m_Biting = set;
		ParticlesManager.Get().Spawn("SmallSplash_Size_C", this.m_FloatPos, Quaternion.identity, Vector3.zero, null, -1f, false);
	}

	public void Strike()
	{
		this.m_Fish = null;
		Fish fish = this.m_Hook ? this.m_Hook.GetFish() : null;
		if (fish)
		{
			this.m_Fish = fish;
			this.m_Fish.Catch();
			this.m_Hook.DeleteBait();
			Skill.Get<FishingSkill>().OnSkillAction();
		}
		DamageInfo damageInfo = new DamageInfo();
		damageInfo.m_Damage = this.m_FishingRodItem.m_Info.m_DamageSelf;
		damageInfo.m_DamageItem = this.m_FishingRodItem;
		this.m_FishingRodItem.TakeDamage(damageInfo);
		this.SetState(FishingRod.State.Strike);
	}

	public void Cancel()
	{
		DamageInfo damageInfo = new DamageInfo();
		damageInfo.m_Damage = this.m_FishingRodItem.m_Info.m_DamageSelf * 0.5f;
		damageInfo.m_DamageItem = this.m_FishingRodItem;
		this.m_FishingRodItem.TakeDamage(damageInfo);
		this.SetState(FishingRod.State.Cancel);
	}

	public void DestroyFish()
	{
		if (this.m_Fish)
		{
			UnityEngine.Object.Destroy(this.m_Fish.gameObject);
			this.m_Fish = null;
		}
	}

	private void SetState(FishingRod.State state)
	{
		this.m_State = state;
		this.m_HideVein = false;
	}

	private void SetupFishTank()
	{
		this.m_FishTank = null;
		int num = Physics.RaycastNonAlloc(new Ray(this.m_FloatPos + Vector3.up * 0.1f, Vector3.down), FishingRod.s_RaycastHitsTmp);
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < num; i++)
		{
			FishTank component = FishingRod.s_RaycastHitsTmp[i].collider.gameObject.GetComponent<FishTank>();
			if (component)
			{
				this.m_FishTank = component;
				return;
			}
		}
	}

	private void SetupHookInWaterPos()
	{
		if (!this.m_Hook)
		{
			return;
		}
		float terrainY = MainLevel.GetTerrainY(this.m_FloatPos);
		if (terrainY >= this.m_FloatPos.y)
		{
			DebugUtils.Assert("[FishingRod:OnEnterState] Float is under terrain!", true, DebugUtils.AssertType.Info);
		}
		Vector3 floatPos = this.m_FloatPos;
		if (this.m_FishTank)
		{
			floatPos.y = this.m_FishTank.transform.position.y;
		}
		if (floatPos.y < terrainY + 0.2f)
		{
			floatPos.y = terrainY + 0.2f;
		}
		floatPos.y = Mathf.Min(floatPos.y, this.m_FloatPos.y);
		this.m_Hook.transform.position = floatPos;
		this.m_HookInWaterPos = floatPos;
	}

	private void LateUpdate()
	{
		this.UpdateAll();
	}

	private void UpdateAll()
	{
		this.UpdateBones();
		this.UpdateHookSlot();
		this.UpdateVein();
		this.UpdateFloat();
		this.UpdateHook();
	}

	private void UpdateBones()
	{
		if (this.m_Biting)
		{
			this.m_Rotation += this.m_BitingRotationSpeed * Time.deltaTime;
			this.m_Rotation = Mathf.Clamp(this.m_Rotation, this.m_MaxBitingRotation, 0f);
		}
		else if (this.m_Rotation != 0f)
		{
			float num = Mathf.Max(0f, 1f - this.m_SpringDamping * Time.fixedDeltaTime);
			float num2 = -this.m_Rotation * this.m_SpringStiffness * Time.fixedDeltaTime;
			this.m_SpringCurrVel = this.m_SpringCurrVel * num + num2;
			this.m_Rotation += this.m_SpringCurrVel * Time.fixedDeltaTime;
			if (Mathf.Abs(this.m_Rotation) < this.m_SpringThreshold && Mathf.Abs(this.m_SpringCurrVel) < this.m_SpringThreshold)
			{
				this.m_Rotation = 0f;
				this.m_SpringCurrVel = this.m_SpringDefaultvel;
			}
		}
		this.RotateBones();
	}

	private void RotateBones()
	{
		Quaternion localRotation = Quaternion.identity;
		for (int i = 1; i < this.m_Bones.Count; i++)
		{
			localRotation = this.m_Bones[i].transform.localRotation;
			localRotation.z = this.m_Rotation * ((float)i / (float)this.m_Bones.Count);
			this.m_Bones[i].transform.localRotation = localRotation;
		}
	}

	private void UpdateHookSlot()
	{
		this.m_HookSlot.gameObject.SetActive(this.m_Hook || (Inventory3DManager.Get() && Inventory3DManager.Get().IsActive() && Player.Get().GetCurrentItem() == this.m_FishingRodItem));
		if (this.m_HookSlot.gameObject.activeSelf && !this.m_HookSlot.m_Active)
		{
			this.m_HookSlot.Activate();
		}
	}

	private void UpdateVein()
	{
		switch (FishingController.Get().m_State)
		{
		case FishingController.State.Waiting:
		case FishingController.State.Fail:
		case FishingController.State.Presentation:
			this.m_Vein.transform.position = this.m_Float.transform.position;
			this.m_VeinElement.SetActive(false);
			return;
		case FishingController.State.Reel:
		case FishingController.State.Strike:
			this.m_Vein.transform.position = (this.m_HideVein ? this.m_Top.transform.position : this.m_Float.transform.position);
			this.m_VeinElement.SetActive(false);
			return;
		}
		this.m_Vein.transform.position = this.m_Top.transform.position;
		this.m_VeinElement.SetActive(true);
	}

	private void UpdateFloat()
	{
		this.m_Float.gameObject.SetActive(!this.m_Biting);
		if (this.m_State == FishingRod.State.Cast)
		{
			return;
		}
		if (this.m_Biting)
		{
			this.m_FloatDive += Time.deltaTime;
			this.m_FloatDive = Mathf.Clamp(this.m_FloatDive, 0f, 0.2f);
		}
		else if (this.m_FloatDive > 0f)
		{
			this.m_FloatDive -= Time.deltaTime;
			this.m_FloatDive = Mathf.Clamp(this.m_FloatDive, 0f, 0.2f);
		}
		if (this.m_State == FishingRod.State.Fishing || this.m_State == FishingRod.State.Strike || this.m_State == FishingRod.State.Cancel)
		{
			this.m_Float.transform.rotation = Player.Get().transform.rotation;
			this.m_Float.transform.position = this.m_FloatPos + Vector3.down * this.m_FloatDive;
			return;
		}
		this.m_Float.transform.localRotation = this.m_DefaultFloatLocalRot;
		this.m_Float.transform.localPosition = this.m_DefaultFloatLocalPos;
	}

	private void UpdateHook()
	{
		if (!this.m_Hook)
		{
			return;
		}
		bool flag = FishingController.Get().m_State != FishingController.State.Cast;
		if (this.m_Hook.gameObject.activeSelf != flag)
		{
			this.m_Hook.gameObject.SetActive(flag);
		}
		this.m_Hook.m_BaitSlot.gameObject.SetActive((Inventory3DManager.Get() && Inventory3DManager.Get().IsActive()) || this.m_Hook.m_Bait);
		if (this.m_Hook.m_BaitSlot.gameObject.activeSelf && !this.m_Hook.m_BaitSlot.m_Active)
		{
			this.m_Hook.m_BaitSlot.Activate();
		}
		if (!flag)
		{
			return;
		}
		if (this.m_State == FishingRod.State.Fishing || this.m_State == FishingRod.State.Strike || this.m_State == FishingRod.State.Cancel)
		{
			this.m_Hook.transform.rotation = this.m_Float.transform.rotation;
			this.m_Hook.transform.position = this.m_HookInWaterPos;
			return;
		}
		this.m_Hook.transform.localRotation = Quaternion.identity;
		this.m_Hook.transform.localPosition = Vector3.zero;
	}

	public void Save(int index)
	{
		SaveGame.SaveVal("FishingRodHook" + index, (int)(this.m_Hook ? this.m_Hook.m_Item.GetInfoID() : ItemID.None));
		SaveGame.SaveVal("FishingRodBait" + index, (int)((this.m_Hook && this.m_Hook.m_Bait) ? this.m_Hook.m_Bait.GetInfoID() : ItemID.None));
	}

	public void SetupAfterLoad(int index)
	{
		this.m_HookToAddAfterLoad = null;
		this.m_BaitToAddAfterLoad = null;
		int num = SaveGame.LoadIVal("FishingRodHook" + index);
		if (num != -1)
		{
			this.m_HookToAddAfterLoad = ItemsManager.Get().CreateItem((ItemID)num, true, Vector3.zero, Quaternion.identity);
			num = SaveGame.LoadIVal("FishingRodBait" + index);
			if (num != -1)
			{
				this.m_BaitToAddAfterLoad = ItemsManager.Get().CreateItem((ItemID)num, true, Vector3.zero, Quaternion.identity);
			}
		}
	}

	private void Update()
	{
		if (this.m_HookToAddAfterLoad)
		{
			if (!this.m_HookSlot.m_Initialized)
			{
				return;
			}
			if (this.m_BaitToAddAfterLoad && !this.m_HookToAddAfterLoad.GetComponent<FishingHook>().m_BaitSlot.m_Initialized)
			{
				return;
			}
			this.m_HookSlot.InsertItem(this.m_HookToAddAfterLoad);
			this.m_HookToAddAfterLoad.gameObject.SetActive(true);
			this.m_HookToAddAfterLoad = null;
			if (this.m_BaitToAddAfterLoad)
			{
				this.m_HookToAddAfterLoad.GetComponent<FishingHook>().m_BaitSlot.InsertItem(this.m_BaitToAddAfterLoad);
				this.m_BaitToAddAfterLoad.gameObject.SetActive(true);
				this.m_BaitToAddAfterLoad = null;
			}
		}
	}

	private void OnDestroy()
	{
		if (!Player.Get())
		{
			return;
		}
		if (FishingController.Get() && FishingController.Get().IsActive() && FishingController.Get().m_FishingRod == this)
		{
			Player.Get().SetCurrentItem(Hand.Right, null);
			Player.Get().SetWantedItem(Hand.Right, null, true);
			Player.Get().StopController(PlayerControllerType.Fishing);
		}
	}

	public void ResetVis()
	{
		this.m_Animator.CrossFade(this.m_EmptyHash, 0f);
	}

	private FishingRod.State m_State;

	private List<GameObject> m_Bones = new List<GameObject>();

	public GameObject m_Top;

	public float m_Rotation;

	private float m_BitingRotationSpeed = -0.2f;

	private float m_MaxBitingRotation = -0.25f;

	private Vector3 m_DefaultFloatLocalPos = Vector3.zero;

	private Quaternion m_DefaultFloatLocalRot = Quaternion.identity;

	private Vector3 m_FloatPos = Vector3.zero;

	public ItemSlot m_HookSlot;

	public GameObject m_Float;

	[HideInInspector]
	public FishingHook m_Hook;

	private Vector3 m_HookInWaterPos = Vector3.zero;

	private FishTank m_FishTank;

	[HideInInspector]
	public Fish m_Fish;

	private bool m_Biting;

	public GameObject m_Vein;

	public GameObject m_VeinElement;

	[HideInInspector]
	public bool m_HideVein;

	private float m_SpringDefaultvel = 1f;

	private float m_SpringCurrVel = 1f;

	private float m_SpringStiffness = 500f;

	private float m_SpringDamping = 5f;

	private float m_SpringThreshold = 0.01f;

	[HideInInspector]
	public Item m_FishingRodItem;

	private Animator m_Animator;

	private int m_FishingRodCast = Animator.StringToHash("FishingRodCast");

	private int m_EmptyHash = Animator.StringToHash("Empty");

	private float m_FloatDive;

	private Item m_HookToAddAfterLoad;

	private Item m_BaitToAddAfterLoad;

	private static RaycastHit[] s_RaycastHitsTmp = new RaycastHit[20];

	private enum State
	{
		None,
		Cast,
		Fishing,
		Strike,
		Cancel
	}
}
