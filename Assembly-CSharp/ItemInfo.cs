using System;
using System.Collections.Generic;
using AIs;
using Enums;
using UnityEngine;

public class ItemInfo
{
	public BackpackPocket m_BackpackPocket { get; set; }

	public InventoryCellsGroup m_InventoryCellsGroup { get; set; }

	public InventoryCellsGroup m_PrevInventoryCellsGroup { get; set; }

	public bool m_InventoryRotated { get; set; }

	public float m_Mass { get; set; }

	public ItemID m_ID { get; set; }

	public ItemType m_Type { get; set; }

	public bool m_CanBeAddedToInventory { get; set; }

	public bool m_CanBeRemovedFromInventory { get; set; }

	public bool m_CanBeFocusedInInventory { get; set; }

	public bool m_CantBeDraggedInInventory { get; set; }

	public bool m_Harvestable { get; set; }

	public bool m_Eatable { get; set; }

	public bool m_Drinkable { get; set; }

	public bool m_CanEquip { get; set; }

	public bool m_Craftable { get; set; }

	public float m_Health { get; set; }

	public float m_MaxHealth { get; set; }

	public float m_HealthLossPerSec { get; set; }

	public float m_DamageSelf { get; set; }

	public Dictionary<int, int> m_Components { get; set; }

	public Dictionary<int, int> m_ComponentsToReturn { get; set; }

	public ItemID m_RequiredTool { get; set; }

	public List<ItemID> m_HarvestingResultItems { get; set; }

	public Vector3 m_HudInfoDisplayOffset { get; set; }

	public float m_CreationTime { get; set; }

	public bool m_CanBeDamaged { get; set; }

	public bool m_DestroyOnDrop { get; set; }

	public string m_GrabSound { get; set; }

	public float m_MakeFireStaminaConsumptionMul { get; set; }

	public DamageType m_DamageType { get; set; }

	public int m_ReceiveDamageType { get; set; }

	public string m_IconName { get; set; }

	public ItemAdditionalIcon m_AdditionalIcon { get; set; }

	public bool m_Static { get; set; }

	public string m_DestroyedPrefabName { get; set; }

	public List<ItemID> m_ItemsToBackpackOnDestroy { get; set; }

	public bool m_DestroyedIsFalling { get; set; }

	public List<CutData> m_CutData { get; set; }

	public bool m_ActiveInNotepad { get; set; }

	public float m_InventoryScale { get; set; }

	public string m_ParticleOnHit { get; set; }

	public float m_ThrowForce { get; set; }

	public float m_ThrowTorque { get; set; }

	public int m_PoisonDebuff { get; set; }

	public float m_HealingTimeDec { get; set; }

	public float m_MakeFireDuration { get; set; }

	public string m_DestroyableFallSound { get; set; }

	public int m_DestroyableFallSoundHash { get; set; }

	public string m_DestroyableDestroySound { get; set; }

	public int m_DestroyableDestroySoundHash { get; set; }

	public float m_ThrowDamage { get; set; }

	public bool m_DestroyByItemsManager { get; set; }

	public bool m_UsedForCrafting { get; set; }

	public string m_LockedInfoID { get; set; }

	public float m_AddFirecamBurningTime { get; set; }

	public float m_AddForgeBurningTime { get; set; }

	public AIManager.BloodFXType m_BloodFXType { get; set; }

	public bool m_CanBePlacedInStorage { get; set; }

	public float m_CoalValue { get; set; }

	public bool m_CantDestroy { get; set; }

	public bool m_FakeItem { get; set; }

	public float m_PadCursorSpeedMul { get; set; }

	public float m_FertilizeAmount { get; set; }

	public float m_AcreDehydration { get; set; }

	public float m_AcreDefertilization { get; set; }

	public bool m_CanPlaceGhostOnTop { get; set; }

	public ItemInfo()
	{
		this.m_ID = ItemID.None;
		this.m_Type = ItemType.None;
		this.m_CanBeAddedToInventory = false;
		this.m_CanBeRemovedFromInventory = true;
		this.m_CanBeFocusedInInventory = true;
		this.m_CantBeDraggedInInventory = false;
		this.m_Harvestable = false;
		this.m_Eatable = false;
		this.m_Drinkable = false;
		this.m_Mass = 0f;
		this.m_CanEquip = false;
		this.m_InventoryCellsGroup = null;
		this.m_PrevInventoryCellsGroup = null;
		this.m_InventoryRotated = false;
		this.m_Health = 100f;
		this.m_MaxHealth = 100f;
		this.m_HealthLossPerSec = 0f;
		this.m_DamageSelf = 0f;
		this.m_Components = new Dictionary<int, int>();
		this.m_ComponentsToReturn = new Dictionary<int, int>();
		this.m_RequiredTool = ItemID.None;
		this.m_HarvestingResultItems = new List<ItemID>();
		this.m_HudInfoDisplayOffset = default(Vector3);
		this.m_CreationTime = 0f;
		this.m_CanBeDamaged = false;
		this.m_DestroyOnDrop = false;
		this.m_GrabSound = string.Empty;
		this.m_BackpackPocket = BackpackPocket.Main;
		this.m_MakeFireStaminaConsumptionMul = 1f;
		this.m_DamageType = DamageType.None;
		this.m_ReceiveDamageType = 0;
		this.m_IconName = string.Empty;
		this.m_AdditionalIcon = ItemAdditionalIcon.None;
		this.m_Static = false;
		this.m_DestroyedPrefabName = string.Empty;
		this.m_ItemsToBackpackOnDestroy = new List<ItemID>();
		this.m_DestroyedIsFalling = false;
		this.m_CutData = null;
		this.m_ActiveInNotepad = false;
		this.m_InventoryScale = 1f;
		this.m_ParticleOnHit = string.Empty;
		this.m_Craftable = false;
		this.m_ThrowForce = 0f;
		this.m_ThrowTorque = 0f;
		this.m_PoisonDebuff = 0;
		this.m_HealingTimeDec = 0f;
		this.m_MakeFireDuration = 5f;
		this.m_DestroyableDestroySound = string.Empty;
		this.m_DestroyableFallSound = string.Empty;
		this.m_ThrowDamage = 0f;
		this.m_DestroyByItemsManager = false;
		this.m_UsedForCrafting = false;
		this.m_LockedInfoID = string.Empty;
		this.m_AddFirecamBurningTime = 0f;
		this.m_AddForgeBurningTime = 0f;
		this.m_BloodFXType = AIManager.BloodFXType.Blunt;
		this.m_CanBePlacedInStorage = false;
		this.m_CoalValue = 40f;
		this.m_CantDestroy = false;
		this.m_FakeItem = false;
		this.m_PadCursorSpeedMul = 0.25f;
		this.m_FertilizeAmount = 0f;
		this.m_AcreDehydration = 0.01f;
		this.m_AcreDefertilization = 0.01f;
		this.m_CanPlaceGhostOnTop = false;
	}

	public ItemInfo ShallowCopy()
	{
		return (ItemInfo)base.MemberwiseClone();
	}

	public virtual bool IsWeapon()
	{
		return false;
	}

	public virtual bool IsTool()
	{
		return false;
	}

	public virtual bool IsDressing()
	{
		return false;
	}

	public virtual bool IsLiquidContainer()
	{
		return false;
	}

	public virtual bool IsFood()
	{
		return false;
	}

	public virtual bool IsConsumable()
	{
		return false;
	}

	public virtual bool IsConstruction()
	{
		return false;
	}

	public virtual bool IsHeavyObject()
	{
		return false;
	}

	public virtual bool IsReadableItem()
	{
		return false;
	}

	public virtual bool IsBowl()
	{
		return false;
	}

	public virtual bool IsDestroyableObject()
	{
		return false;
	}

	public virtual bool IsBow()
	{
		return false;
	}

	public virtual bool IsArrow()
	{
		return false;
	}

	public virtual bool IsBlowpipeArrow()
	{
		return false;
	}

	public virtual bool IsTorch()
	{
		return false;
	}

	public virtual bool IsSpear()
	{
		return false;
	}

	public virtual bool IsStand()
	{
		return false;
	}

	public virtual bool IsArmor()
	{
		return false;
	}

	public virtual bool IsParasite()
	{
		return false;
	}

	public virtual bool IsSeed()
	{
		return false;
	}

	public static bool IsStoneRing(ItemID id)
	{
		return id == ItemID.Stone_Ring;
	}

	public static bool IsFirecamp(ItemID id)
	{
		return id == ItemID.Small_Fire || id == ItemID.Campfire || id == ItemID.Campfire_fireside || id == ItemID.campfire_ayuhasca;
	}

	public static bool IsFirecampRack(ItemID id)
	{
		return id == ItemID.Campfire_Rack || id == ItemID.Campfire_RackBowl || id == ItemID.mud_bamboo_wall_fireside || id == ItemID.mud_wall_fireside || id == ItemID.ayuhasca_rack;
	}

	public static bool IsSmoker(ItemID id)
	{
		return id == ItemID.Smoker || id == ItemID.Bamboo_Smoker;
	}

	public static bool IsTrap(ItemID id)
	{
		return id == ItemID.Big_Stick_Fish_Trap || id == ItemID.Cage_Trap || id == ItemID.Fish_Rod_Trap || id == ItemID.Human_Killer_Trap || id == ItemID.Killer_Trap || id == ItemID.shrimp_trap || id == ItemID.Snare_Trap || id == ItemID.Stick_Fish_Trap || id == ItemID.Stone_Trap || id == ItemID.Tribe_Bow_Trap || id == ItemID.tribe_spike_trap;
	}

	public bool IsShelter()
	{
		return this.m_ID == ItemID.Hut_Shelter || this.m_ID == ItemID.Medium_Bamboo_Shelter || this.m_ID == ItemID.Medium_Shelter || this.m_ID == ItemID.Small_Bamboo_Shelter || this.m_ID == ItemID.Small_Shelter;
	}

	public bool IsHammock()
	{
		return this.m_ID == ItemID.Hammock || this.m_ID == ItemID.hammock_a || this.m_ID == ItemID.village_hammock_a || this.m_ID == ItemID.village_hammock_b;
	}

	public bool IsFishingRod()
	{
		return this.m_ID == ItemID.Fishing_Rod || this.m_ID == ItemID.Bamboo_Fishing_Rod || this.m_ID == ItemID.Bamboo_Fishing_Rod_Bone || this.m_ID == ItemID.Fishing_Rod_Bone;
	}

	public bool IsAxe()
	{
		return this.m_ID == ItemID.Axe || this.m_ID == ItemID.Bone_Axe || this.m_ID == ItemID.Stone_Axe || this.m_ID == ItemID.Blade_Axe || this.m_ID == ItemID.Obsidian_Axe || this.m_ID == ItemID.Rusted_Axe || this.m_ID == ItemID.Tribe_Axe || this.m_ID == ItemID.Axe_professional || this.m_ID == ItemID.metal_axe;
	}

	public bool IsKnife()
	{
		return this.m_ID == ItemID.Bone_Knife || this.m_ID == ItemID.Obsidian_Blade || this.m_ID == ItemID.Obsidian_Bone_Blade || this.m_ID == ItemID.Stick_Blade || this.m_ID == ItemID.Stone_Blade || this.m_ID == ItemID.metal_blade_weapon;
	}

	public bool IsMachete()
	{
		return this.m_ID == ItemID.Machete || this.m_ID == ItemID.MacheteToPickUp || this.m_ID == ItemID.Rusted_Machete;
	}

	public bool IsStone()
	{
		return this.m_ID == ItemID.Stone || this.m_ID == ItemID.Obsidian_Stone;
	}

	public bool IsWall()
	{
		return this.m_ID == ItemID.building_bamboo_wall || this.m_ID == ItemID.building_wall || this.m_ID == ItemID.wooden_doorway || this.m_ID == ItemID.bamboo_doorway;
	}

	public virtual bool CanCookOnFire()
	{
		return false;
	}

	public virtual float GetMass()
	{
		return this.m_Mass;
	}

	public void Load(Key key)
	{
		if (key == null)
		{
			return;
		}
		this.m_ID = (ItemID)Enum.Parse(typeof(ItemID), key.GetVariable(0).SValue);
		this.m_Type = (ItemType)Enum.Parse(typeof(ItemType), key.GetVariable(1).SValue);
		for (int i = 0; i < key.GetKeysCount(); i++)
		{
			this.LoadParams(key.GetKey(i));
		}
	}

	protected virtual void LoadParams(Key key)
	{
		if (key.GetName() == "CanBeAddedToInventory")
		{
			this.m_CanBeAddedToInventory = (key.GetVariable(0).IValue != 0);
			return;
		}
		if (key.GetName() == "CanBeRemovedFromInventory")
		{
			this.m_CanBeRemovedFromInventory = (key.GetVariable(0).IValue != 0);
			return;
		}
		if (key.GetName() == "CanBeFocusedInInventory")
		{
			this.m_CanBeFocusedInInventory = (key.GetVariable(0).IValue != 0);
			return;
		}
		if (key.GetName() == "CantBeDraggedInInventory")
		{
			this.m_CantBeDraggedInInventory = (key.GetVariable(0).IValue != 0);
			return;
		}
		if (key.GetName() == "HarvestringResult")
		{
			string[] array = key.GetVariable(0).SValue.Split(new char[]
			{
				';'
			});
			for (int i = 0; i < array.Length; i++)
			{
				this.m_HarvestingResultItems.Add((ItemID)Enum.Parse(typeof(ItemID), array[i]));
			}
			if (this.m_HarvestingResultItems.Count > 0)
			{
				this.m_Harvestable = true;
				return;
			}
		}
		else
		{
			if (key.GetName() == "Eatable")
			{
				this.m_Eatable = (key.GetVariable(0).IValue != 0);
				return;
			}
			if (key.GetName() == "Drinkable")
			{
				this.m_Drinkable = (key.GetVariable(0).IValue != 0);
				return;
			}
			if (key.GetName() == "CanEquip")
			{
				this.m_CanEquip = (key.GetVariable(0).IValue != 0);
				return;
			}
			if (key.GetName() == "Mass")
			{
				this.m_Mass = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "Components")
			{
				string[] array2 = key.GetVariable(0).SValue.Split(new char[]
				{
					';'
				});
				for (int j = 0; j < array2.Length; j++)
				{
					string[] array3 = array2[j].Split(new char[]
					{
						'*'
					});
					ItemID key2 = (ItemID)Enum.Parse(typeof(ItemID), array3[0]);
					this.m_Components[(int)key2] = ((array3.Length > 1) ? int.Parse(array3[1]) : 1);
				}
				return;
			}
			if (key.GetName() == "ComponentsToReturn")
			{
				string[] array4 = key.GetVariable(0).SValue.Split(new char[]
				{
					';'
				});
				for (int k = 0; k < array4.Length; k++)
				{
					string[] array5 = array4[k].Split(new char[]
					{
						'*'
					});
					ItemID key3 = (ItemID)Enum.Parse(typeof(ItemID), array5[0]);
					this.m_ComponentsToReturn[(int)key3] = ((array5.Length > 1) ? int.Parse(array5[1]) : 1);
				}
				return;
			}
			if (key.GetName() == "RequiredTool")
			{
				this.m_RequiredTool = (ItemID)Enum.Parse(typeof(ItemID), key.GetVariable(0).SValue);
				return;
			}
			if (key.GetName() == "HudInfoDisplayOffset")
			{
				string[] array6 = key.GetVariable(0).SValue.Split(new char[]
				{
					';'
				});
				this.m_HudInfoDisplayOffset = new Vector3(float.Parse(array6[0]), float.Parse(array6[1]), float.Parse(array6[2]));
				return;
			}
			if (key.GetName() == "Health")
			{
				this.m_Health = key.GetVariable(0).FValue;
				this.m_MaxHealth = this.m_Health;
				return;
			}
			if (key.GetName() == "HealthLossPerSec")
			{
				this.m_HealthLossPerSec = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "DamageSelf")
			{
				this.m_DamageSelf = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "CanBeDamaged")
			{
				this.m_CanBeDamaged = (key.GetVariable(0).IValue != 0);
				return;
			}
			if (key.GetName() == "DestroyOnDrop")
			{
				this.m_DestroyOnDrop = (key.GetVariable(0).IValue != 0);
				return;
			}
			if (key.GetName() == "GrabSound")
			{
				this.m_GrabSound = key.GetVariable(0).SValue;
				return;
			}
			if (key.GetName() == "BackpackPocket")
			{
				this.m_BackpackPocket = (BackpackPocket)Enum.Parse(typeof(BackpackPocket), key.GetVariable(0).SValue);
				return;
			}
			if (key.GetName() == "MakeFireStaminaConsumptionMul")
			{
				this.m_MakeFireStaminaConsumptionMul = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "DamageType")
			{
				this.m_DamageType = (DamageType)Enum.Parse(typeof(DamageType), key.GetVariable(0).SValue);
				return;
			}
			if (key.GetName() == "ReceiveDamageType")
			{
				string[] array7 = key.GetVariable(0).SValue.Split(new char[]
				{
					';'
				});
				for (int l = 0; l < array7.Length; l++)
				{
					int num = this.m_ReceiveDamageType;
					num |= (int)Enum.Parse(typeof(DamageType), array7[l]);
					this.m_ReceiveDamageType = num;
				}
				return;
			}
			if (key.GetName() == "Icon")
			{
				this.m_IconName = key.GetVariable(0).SValue;
				return;
			}
			if (key.GetName() == "AdditionalIcon")
			{
				this.m_AdditionalIcon = (ItemAdditionalIcon)Enum.Parse(typeof(ItemAdditionalIcon), key.GetVariable(0).SValue);
				return;
			}
			if (key.GetName() == "Static")
			{
				this.m_Static = (key.GetVariable(0).IValue != 0);
				return;
			}
			if (key.GetName() == "Prefab_Destroyed")
			{
				this.m_DestroyedPrefabName = key.GetVariable(0).SValue;
				return;
			}
			if (key.GetName() == "ItemsToBackpackOnDestroy")
			{
				string[] array8 = key.GetVariable(0).SValue.Split(new char[]
				{
					';'
				});
				for (int m = 0; m < array8.Length; m++)
				{
					string[] array9 = array8[m].Split(new char[]
					{
						'*'
					});
					ItemID item = (ItemID)Enum.Parse(typeof(ItemID), array9[0]);
					this.m_ItemsToBackpackOnDestroy.Add(item);
				}
				return;
			}
			if (key.GetName() == "Destroyed_Falling")
			{
				this.m_DestroyedIsFalling = (key.GetVariable(0).IValue != 0);
				return;
			}
			if (key.GetName() == "CutSystem")
			{
				this.m_CutData = new List<CutData>();
				string[] array10 = key.GetVariable(0).SValue.Split(new char[]
				{
					';'
				});
				for (int n = 0; n < array10.Length; n++)
				{
					CutData cutData = new CutData();
					string[] array11 = array10[n].Split(new char[]
					{
						','
					});
					cutData.m_DummyName = array11[0];
					int.TryParse(array11[1], out cutData.m_BlendShapeIndex);
					float.TryParse(array11[2], out cutData.m_MaxHealth);
					cutData.m_Health = cutData.m_MaxHealth;
					cutData.m_DestroyedPrefabName = array11[3];
					this.m_CutData.Add(cutData);
				}
				return;
			}
			if (key.GetName() == "ActiveInNotepad")
			{
				this.m_ActiveInNotepad = (key.GetVariable(0).IValue != 0);
				return;
			}
			if (key.GetName() == "InventoryScale")
			{
				this.m_InventoryScale = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "ParticleOnHit")
			{
				this.m_ParticleOnHit = key.GetVariable(0).SValue;
				return;
			}
			if (key.GetName() == "Craftable")
			{
				this.m_Craftable = (key.GetVariable(0).IValue != 0);
				return;
			}
			if (key.GetName() == "ThrowForce")
			{
				this.m_ThrowForce = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "ThrowTorque")
			{
				this.m_ThrowTorque = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "PoisonDebuff")
			{
				this.m_PoisonDebuff = key.GetVariable(0).IValue;
				return;
			}
			if (key.GetName() == "HealingTimeDec")
			{
				this.m_HealingTimeDec = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "MakeFireDuration")
			{
				this.m_MakeFireDuration = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "DestroyableDestroySound")
			{
				this.m_DestroyableDestroySound = key.GetVariable(0).SValue;
				this.m_DestroyableDestroySoundHash = Animator.StringToHash(this.m_DestroyableDestroySound);
				return;
			}
			if (key.GetName() == "DestroyableFallSound")
			{
				this.m_DestroyableFallSound = key.GetVariable(0).SValue;
				this.m_DestroyableFallSoundHash = Animator.StringToHash(this.m_DestroyableFallSound);
				return;
			}
			if (key.GetName() == "ThrowDamage")
			{
				this.m_ThrowDamage = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "DestroyByItemsManager")
			{
				this.m_DestroyByItemsManager = (key.GetVariable(0).IValue != 0);
				return;
			}
			if (key.GetName() == "LockedInfoID")
			{
				this.m_LockedInfoID = key.GetVariable(0).SValue;
				return;
			}
			if (key.GetName() == "AddFirecamBurningTime")
			{
				this.m_AddFirecamBurningTime = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "AddForgeBurningTime")
			{
				this.m_AddForgeBurningTime = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "BloodFXType")
			{
				this.m_BloodFXType = (AIManager.BloodFXType)Enum.Parse(typeof(AIManager.BloodFXType), key.GetVariable(0).SValue);
				return;
			}
			if (key.GetName() == "CanBePlacedInStorage")
			{
				this.m_CanBePlacedInStorage = (key.GetVariable(0).IValue != 0);
				return;
			}
			if (key.GetName() == "CoalValue")
			{
				this.m_CoalValue = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "CantDestroy")
			{
				this.m_CantDestroy = (key.GetVariable(0).IValue != 0);
				return;
			}
			if (key.GetName() == "FakeItem")
			{
				this.m_FakeItem = (key.GetVariable(0).IValue != 0);
				return;
			}
			if (key.GetName() == "FertilizeAmount")
			{
				this.m_FertilizeAmount = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "AcreDehydration")
			{
				this.m_AcreDehydration = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "AcreDefertilization")
			{
				this.m_AcreDefertilization = key.GetVariable(0).FValue;
				return;
			}
			if (key.GetName() == "CanPlaceGhostOnTop")
			{
				this.m_CanPlaceGhostOnTop = (key.GetVariable(0).IValue != 0);
			}
		}
	}

	public virtual void GetInfoText(ref string result)
	{
		result = string.Concat(new string[]
		{
			"Health = ",
			this.m_Health.ToString(),
			"/",
			this.m_MaxHealth.ToString(),
			"\n"
		});
		result = result + "Mass = " + this.GetMass().ToString() + "\n";
	}

	public virtual string GetNameToDisplayLocalized()
	{
		if (this.m_LockedInfoID != string.Empty && !ItemsManager.Get().m_UnlockedItemInfos.Contains(this.m_ID))
		{
			return GreenHellGame.Instance.GetLocalization().Get(this.m_LockedInfoID, true);
		}
		return GreenHellGame.Instance.GetLocalization().Get(this.m_ID.ToString(), true);
	}

	public virtual bool CanDrink()
	{
		return false;
	}

	public Item m_Item;
}
