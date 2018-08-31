using System;
using System.Collections.Generic;
using AIs;
using Enums;
using UnityEngine;

public class ItemInfo
{
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
		this.m_LockedInfoID = string.Empty;
		this.m_AddFirecamBurningTime = 0f;
		this.m_BloodFXType = AIManager.BloodFXType.Blunt;
	}

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

	public string m_LockedInfoID { get; set; }

	public float m_AddFirecamBurningTime { get; set; }

	public AIManager.BloodFXType m_BloodFXType { get; set; }

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

	public virtual bool IsTrap()
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

	public static bool IsFirecamp(ItemID id)
	{
		return id == ItemID.Small_Fire || id == ItemID.Campfire;
	}

	public static bool IsFirecampRack(ItemID id)
	{
		return id == ItemID.Campfire_Rack || id == ItemID.Campfire_RackBowl;
	}

	public static bool IsSmoker(ItemID id)
	{
		return id == ItemID.Smoker || id == ItemID.Bamboo_Smoker;
	}

	public bool IsShelter()
	{
		return this.m_ID == ItemID.Hut_Shelter || this.m_ID == ItemID.Medium_Bamboo_Shelter || this.m_ID == ItemID.Medium_Shelter || this.m_ID == ItemID.Small_Bamboo_Shelter || this.m_ID == ItemID.Small_Shelter;
	}

	public bool IsFishingRod()
	{
		return this.m_ID == ItemID.Fishing_Rod || this.m_ID == ItemID.Bamboo_Fishing_Rod || this.m_ID == ItemID.Bamboo_Fishing_Rod_Bone || this.m_ID == ItemID.Fishing_Rod_Bone;
	}

	public bool IsAxe()
	{
		return this.m_ID == ItemID.Axe || this.m_ID == ItemID.Axe_professional;
	}

	public bool IsKnife()
	{
		return this.m_ID == ItemID.Bone_Knife || this.m_ID == ItemID.Obsidian_Blade || this.m_ID == ItemID.Obsidian_Bone_Blade || this.m_ID == ItemID.Stick_Blade || this.m_ID == ItemID.Stone_Blade;
	}

	public bool IsStone()
	{
		return this.m_ID == ItemID.Stone || this.m_ID == ItemID.Obsidian_Stone;
	}

	public virtual bool CanCookOnFire()
	{
		return false;
	}

	public float GetMass()
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
		}
		else if (key.GetName() == "CanBeRemovedFromInventory")
		{
			this.m_CanBeRemovedFromInventory = (key.GetVariable(0).IValue != 0);
		}
		else if (key.GetName() == "CanBeFocusedInInventory")
		{
			this.m_CanBeFocusedInInventory = (key.GetVariable(0).IValue != 0);
		}
		else if (key.GetName() == "CantBeDraggedInInventory")
		{
			this.m_CantBeDraggedInInventory = (key.GetVariable(0).IValue != 0);
		}
		else if (key.GetName() == "HarvestringResult")
		{
			string svalue = key.GetVariable(0).SValue;
			string[] array = svalue.Split(new char[]
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
			}
		}
		else if (key.GetName() == "Eatable")
		{
			this.m_Eatable = (key.GetVariable(0).IValue != 0);
		}
		else if (key.GetName() == "Drinkable")
		{
			this.m_Drinkable = (key.GetVariable(0).IValue != 0);
		}
		else if (key.GetName() == "CanEquip")
		{
			this.m_CanEquip = (key.GetVariable(0).IValue != 0);
		}
		else if (key.GetName() == "Mass")
		{
			this.m_Mass = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "Components")
		{
			string svalue2 = key.GetVariable(0).SValue;
			string[] array2 = svalue2.Split(new char[]
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
				this.m_Components[(int)key2] = ((array3.Length <= 1) ? 1 : int.Parse(array3[1]));
			}
		}
		else if (key.GetName() == "RequiredTool")
		{
			this.m_RequiredTool = (ItemID)Enum.Parse(typeof(ItemID), key.GetVariable(0).SValue);
		}
		else if (key.GetName() == "HudInfoDisplayOffset")
		{
			string svalue3 = key.GetVariable(0).SValue;
			string[] array4 = svalue3.Split(new char[]
			{
				';'
			});
			this.m_HudInfoDisplayOffset = new Vector3(float.Parse(array4[0]), float.Parse(array4[1]), float.Parse(array4[2]));
		}
		else if (key.GetName() == "Health")
		{
			this.m_Health = key.GetVariable(0).FValue;
			this.m_MaxHealth = this.m_Health;
		}
		else if (key.GetName() == "HealthLossPerSec")
		{
			this.m_HealthLossPerSec = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "DamageSelf")
		{
			this.m_DamageSelf = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "CanBeDamaged")
		{
			this.m_CanBeDamaged = (key.GetVariable(0).IValue != 0);
		}
		else if (key.GetName() == "DestroyOnDrop")
		{
			this.m_DestroyOnDrop = (key.GetVariable(0).IValue != 0);
		}
		else if (key.GetName() == "GrabSound")
		{
			this.m_GrabSound = key.GetVariable(0).SValue;
		}
		else if (key.GetName() == "BackpackPocket")
		{
			this.m_BackpackPocket = (BackpackPocket)Enum.Parse(typeof(BackpackPocket), key.GetVariable(0).SValue);
		}
		else if (key.GetName() == "MakeFireStaminaConsumptionMul")
		{
			this.m_MakeFireStaminaConsumptionMul = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "DamageType")
		{
			this.m_DamageType = (DamageType)Enum.Parse(typeof(DamageType), key.GetVariable(0).SValue);
		}
		else if (key.GetName() == "ReceiveDamageType")
		{
			string[] array5 = key.GetVariable(0).SValue.Split(new char[]
			{
				';'
			});
			for (int k = 0; k < array5.Length; k++)
			{
				int num = this.m_ReceiveDamageType;
				num |= (int)Enum.Parse(typeof(DamageType), array5[k]);
				this.m_ReceiveDamageType = num;
			}
		}
		else if (key.GetName() == "Icon")
		{
			this.m_IconName = key.GetVariable(0).SValue;
		}
		else if (key.GetName() == "AdditionalIcon")
		{
			this.m_AdditionalIcon = (ItemAdditionalIcon)Enum.Parse(typeof(ItemAdditionalIcon), key.GetVariable(0).SValue);
		}
		else if (key.GetName() == "Static")
		{
			this.m_Static = (key.GetVariable(0).IValue != 0);
		}
		else if (key.GetName() == "Prefab_Destroyed")
		{
			this.m_DestroyedPrefabName = key.GetVariable(0).SValue;
		}
		else if (key.GetName() == "ItemsToBackpackOnDestroy")
		{
			string[] array6 = key.GetVariable(0).SValue.Split(new char[]
			{
				';'
			});
			for (int l = 0; l < array6.Length; l++)
			{
				string[] array7 = array6[l].Split(new char[]
				{
					'*'
				});
				ItemID item = (ItemID)Enum.Parse(typeof(ItemID), array7[0]);
				this.m_ItemsToBackpackOnDestroy.Add(item);
			}
		}
		else if (key.GetName() == "Destroyed_Falling")
		{
			this.m_DestroyedIsFalling = (key.GetVariable(0).IValue != 0);
		}
		else if (key.GetName() == "CutSystem")
		{
			this.m_CutData = new List<CutData>();
			string svalue4 = key.GetVariable(0).SValue;
			string[] array8 = svalue4.Split(new char[]
			{
				';'
			});
			for (int m = 0; m < array8.Length; m++)
			{
				CutData cutData = new CutData();
				string[] array9 = array8[m].Split(new char[]
				{
					','
				});
				cutData.m_DummyName = array9[0];
				int.TryParse(array9[1], out cutData.m_BlendShapeIndex);
				float.TryParse(array9[2], out cutData.m_MaxHealth);
				cutData.m_Health = cutData.m_MaxHealth;
				cutData.m_DestroyedPrefabName = array9[3];
				this.m_CutData.Add(cutData);
			}
		}
		else if (key.GetName() == "ActiveInNotepad")
		{
			this.m_ActiveInNotepad = (key.GetVariable(0).IValue != 0);
		}
		else if (key.GetName() == "InventoryScale")
		{
			this.m_InventoryScale = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "ParticleOnHit")
		{
			this.m_ParticleOnHit = key.GetVariable(0).SValue;
		}
		else if (key.GetName() == "Craftable")
		{
			this.m_Craftable = (key.GetVariable(0).IValue != 0);
		}
		else if (key.GetName() == "ThrowForce")
		{
			this.m_ThrowForce = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "ThrowTorque")
		{
			this.m_ThrowTorque = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "PoisonDebuff")
		{
			this.m_PoisonDebuff = key.GetVariable(0).IValue;
		}
		else if (key.GetName() == "HealingTimeDec")
		{
			this.m_HealingTimeDec = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "MakeFireDuration")
		{
			this.m_MakeFireDuration = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "DestroyableDestroySound")
		{
			this.m_DestroyableDestroySound = key.GetVariable(0).SValue;
			this.m_DestroyableDestroySoundHash = Animator.StringToHash(this.m_DestroyableDestroySound);
		}
		else if (key.GetName() == "DestroyableFallSound")
		{
			this.m_DestroyableFallSound = key.GetVariable(0).SValue;
			this.m_DestroyableFallSoundHash = Animator.StringToHash(this.m_DestroyableFallSound);
		}
		else if (key.GetName() == "ThrowDamage")
		{
			this.m_ThrowDamage = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "DestroyByItemsManager")
		{
			this.m_DestroyByItemsManager = (key.GetVariable(0).IValue != 0);
		}
		else if (key.GetName() == "LockedInfoID")
		{
			this.m_LockedInfoID = key.GetVariable(0).SValue;
		}
		else if (key.GetName() == "AddFirecamBurningTime")
		{
			this.m_AddFirecamBurningTime = key.GetVariable(0).FValue;
		}
		else if (key.GetName() == "BloodFXType")
		{
			this.m_BloodFXType = (AIManager.BloodFXType)Enum.Parse(typeof(AIManager.BloodFXType), key.GetVariable(0).SValue);
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
			return GreenHellGame.Instance.GetLocalization().Get(this.m_LockedInfoID);
		}
		return GreenHellGame.Instance.GetLocalization().Get(this.m_ID.ToString());
	}

	public virtual bool CanDrink()
	{
		return false;
	}

	public Item m_Item;
}
