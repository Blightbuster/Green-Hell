using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class PlayerArmorModule : PlayerModule
{
	public static PlayerArmorModule Get()
	{
		return PlayerArmorModule.s_Instance;
	}

	private void Awake()
	{
		PlayerArmorModule.s_Instance = this;
	}

	public override void Initialize(Being being)
	{
		base.Initialize(being);
		this.m_LimbMap.Clear();
		ArmorData armorData = new ArmorData();
		armorData.m_AttachedArmorCollCenter = new Vector3(-0.005201378f, 0.07581946f, -0.002843004f);
		armorData.m_AttachedArmorCollSize = new Vector3(0.0610777f, 0.1354455f, 0.08637088f);
		this.m_LimbMap.Add(0, armorData);
		armorData = new ArmorData();
		armorData.m_AttachedArmorCollCenter = new Vector3(0.003011873f, 0.07581922f, -0.002842651f);
		armorData.m_AttachedArmorCollSize = new Vector3(0.07750419f, 0.1354455f, 0.08637088f);
		this.m_LimbMap.Add(1, armorData);
		armorData = new ArmorData();
		armorData.m_AttachedArmorCollCenter = new Vector3(0.03389587f, 0.007923566f, 0.009612092f);
		armorData.m_AttachedArmorCollSize = new Vector3(0.1124193f, 0.2000117f, 0.09803453f);
		this.m_LimbMap.Add(2, armorData);
		armorData = new ArmorData();
		armorData.m_AttachedArmorCollCenter = new Vector3(0.0454613f, 0.01937103f, 0.0103231808f);
		armorData.m_AttachedArmorCollSize = new Vector3(0.1015261f, 0.1771168f, 0.09945565f);
		this.m_LimbMap.Add(3, armorData);
		this.SetArmorSlots();
		this.SetMeshes();
	}

	public ArmorData GetArmorData(Limb limb)
	{
		return this.m_LimbMap[(int)limb];
	}

	private void SetArmorSlots()
	{
		Transform transform = this.m_Player.transform.FindDeepChild("mixamorig:ArmTwist2.L");
		DebugUtils.Assert(transform, true);
		ArmorSlot armorSlot = transform.gameObject.AddComponent<ArmorSlot>();
		armorSlot.m_Limb = Limb.LArm;
		this.m_ArmorSlots.Add(armorSlot);
		this.m_LimbMap[0].m_Slot = armorSlot;
		Transform transform2 = this.m_Player.transform.FindDeepChild("mixamorig:ArmTwist2.R");
		DebugUtils.Assert(transform2, true);
		armorSlot = transform2.gameObject.AddComponent<ArmorSlot>();
		armorSlot.m_Limb = Limb.RArm;
		this.m_ArmorSlots.Add(armorSlot);
		this.m_LimbMap[1].m_Slot = armorSlot;
		Transform transform3 = this.m_Player.transform.FindDeepChild("Wound17");
		DebugUtils.Assert(transform3, true);
		armorSlot = transform3.gameObject.AddComponent<ArmorSlot>();
		armorSlot.m_Limb = Limb.LLeg;
		this.m_ArmorSlots.Add(armorSlot);
		this.m_LimbMap[2].m_Slot = armorSlot;
		Transform transform4 = this.m_Player.transform.FindDeepChild("Wound34");
		DebugUtils.Assert(transform4, true);
		armorSlot = transform4.gameObject.AddComponent<ArmorSlot>();
		armorSlot.m_Limb = Limb.RLeg;
		this.m_ArmorSlots.Add(armorSlot);
		this.m_LimbMap[3].m_Slot = armorSlot;
	}

	private void SetMeshes()
	{
		Transform transform = base.transform.FindDeepChild("leaf_armor00");
		this.m_LimbMap[0].m_MeshMap.Add(1, transform.gameObject);
		transform = base.transform.FindDeepChild("stick_armor00");
		this.m_LimbMap[0].m_MeshMap.Add(2, transform.gameObject);
		transform = base.transform.FindDeepChild("bamboo_armor00");
		this.m_LimbMap[0].m_MeshMap.Add(3, transform.gameObject);
		transform = base.transform.FindDeepChild("bone_armor00");
		this.m_LimbMap[0].m_MeshMap.Add(4, transform.gameObject);
		transform = base.transform.FindDeepChild("armadillo_armor00");
		this.m_LimbMap[0].m_MeshMap.Add(5, transform.gameObject);
		transform = base.transform.FindDeepChild("metal_armor00");
		this.m_LimbMap[0].m_MeshMap.Add(6, transform.gameObject);
		transform = base.transform.FindDeepChild("leaf_armor01");
		this.m_LimbMap[1].m_MeshMap.Add(1, transform.gameObject);
		transform = base.transform.FindDeepChild("stick_armor01");
		this.m_LimbMap[1].m_MeshMap.Add(2, transform.gameObject);
		transform = base.transform.FindDeepChild("bamboo_armor01");
		this.m_LimbMap[1].m_MeshMap.Add(3, transform.gameObject);
		transform = base.transform.FindDeepChild("bone_armor01");
		this.m_LimbMap[1].m_MeshMap.Add(4, transform.gameObject);
		transform = base.transform.FindDeepChild("armadillo_armor01");
		this.m_LimbMap[1].m_MeshMap.Add(5, transform.gameObject);
		transform = base.transform.FindDeepChild("metal_armor01");
		this.m_LimbMap[1].m_MeshMap.Add(6, transform.gameObject);
		transform = base.transform.FindDeepChild("leaf_armor02");
		this.m_LimbMap[2].m_MeshMap.Add(1, transform.gameObject);
		transform = base.transform.FindDeepChild("stick_armor02");
		this.m_LimbMap[2].m_MeshMap.Add(2, transform.gameObject);
		transform = base.transform.FindDeepChild("bamboo_armor02");
		this.m_LimbMap[2].m_MeshMap.Add(3, transform.gameObject);
		transform = base.transform.FindDeepChild("bone_armor02");
		this.m_LimbMap[2].m_MeshMap.Add(4, transform.gameObject);
		transform = base.transform.FindDeepChild("armadillo_armor02");
		this.m_LimbMap[2].m_MeshMap.Add(5, transform.gameObject);
		transform = base.transform.FindDeepChild("metal_armor02");
		this.m_LimbMap[2].m_MeshMap.Add(6, transform.gameObject);
		transform = base.transform.FindDeepChild("leaf_armor03");
		this.m_LimbMap[3].m_MeshMap.Add(1, transform.gameObject);
		transform = base.transform.FindDeepChild("stick_armor03");
		this.m_LimbMap[3].m_MeshMap.Add(2, transform.gameObject);
		transform = base.transform.FindDeepChild("bamboo_armor03");
		this.m_LimbMap[3].m_MeshMap.Add(3, transform.gameObject);
		transform = base.transform.FindDeepChild("bone_armor03");
		this.m_LimbMap[3].m_MeshMap.Add(4, transform.gameObject);
		transform = base.transform.FindDeepChild("armadillo_armor03");
		this.m_LimbMap[3].m_MeshMap.Add(5, transform.gameObject);
		transform = base.transform.FindDeepChild("metal_armor03");
		this.m_LimbMap[3].m_MeshMap.Add(6, transform.gameObject);
		transform = base.transform.FindDeepChild("Destro_armor00");
		this.m_LimbMap[0].m_DestroyedMesh = transform.gameObject;
		this.m_LimbMap[0].m_Cloth = transform.gameObject.GetComponentInChildren<Cloth>();
		transform = base.transform.FindDeepChild("Destro_armor01");
		this.m_LimbMap[1].m_DestroyedMesh = transform.gameObject;
		this.m_LimbMap[1].m_Cloth = transform.gameObject.GetComponentInChildren<Cloth>();
		transform = base.transform.FindDeepChild("Destro_armor02");
		this.m_LimbMap[2].m_DestroyedMesh = transform.gameObject;
		this.m_LimbMap[2].m_Cloth = transform.gameObject.GetComponentInChildren<Cloth>();
		transform = base.transform.FindDeepChild("Destro_armor03");
		this.m_LimbMap[3].m_DestroyedMesh = transform.gameObject;
		this.m_LimbMap[3].m_Cloth = transform.gameObject.GetComponentInChildren<Cloth>();
	}

	public void ArmorInsert(Limb limb, Item item)
	{
		this.OnArmorInsert(this.m_LimbMap[(int)limb].m_Slot, item);
	}

	public void OnArmorInsert(ArmorSlot slot, Item item)
	{
		((Armor)item).m_Limb = slot.m_Limb;
		item.ItemsManagerUnregister();
		item.transform.SetParent(slot.transform);
		Physics.IgnoreCollision(this.m_Player.m_Collider, item.GetComponent<Collider>());
		Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(item.gameObject);
		for (int i = 0; i < componentsDeepChild.Length; i++)
		{
			componentsDeepChild[i].enabled = false;
		}
		ArmorInfo armorInfo = (ArmorInfo)item.m_Info;
		ArmorData armorData = this.m_LimbMap[(int)slot.m_Limb];
		armorData.m_MeshMap[(int)armorInfo.m_ArmorType].SetActive(true);
		armorData.m_Mesh = armorData.m_MeshMap[(int)armorInfo.m_ArmorType];
		armorData.m_AttachedArmor = item;
		armorData.m_AttachedArmorOrigCollCenter = armorData.m_AttachedArmor.m_BoxCollider.center;
		armorData.m_AttachedArmorOrigCollSize = armorData.m_AttachedArmor.m_BoxCollider.size;
		armorData.m_AttachedArmor.m_BoxCollider.center = armorData.m_AttachedArmorCollCenter;
		armorData.m_AttachedArmor.m_BoxCollider.size = armorData.m_AttachedArmorCollSize;
		armorData.m_ArmorType = armorInfo.m_ArmorType;
		armorData.m_Absorption = armorInfo.m_Absorption;
		armorData.m_Health = armorInfo.m_Health;
		HUDBodyInspection.Get().m_ArmorEnabled = true;
		this.SetMeshesVisible(HUDBodyInspection.Get().m_ArmorEnabled);
		BodyInspectionController.Get().OnArmorAttached(slot.m_Limb);
	}

	public void OnDragItemToSlot(ArmorSlot slot, Item item)
	{
		ArmorData armorData = this.m_LimbMap[(int)slot.m_Limb];
		ArmorInfo armorInfo = (ArmorInfo)item.m_Info;
		armorData.m_MeshMap[(int)armorInfo.m_ArmorType].SetActive(true);
		Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(item.gameObject);
		for (int i = 0; i < componentsDeepChild.Length; i++)
		{
			componentsDeepChild[i].enabled = false;
		}
		BodyInspectionController.Get().OnArmorAttached(slot.m_Limb);
	}

	public void OnRemoveItemFromSlot(ArmorSlot slot, Item item)
	{
		if (!item)
		{
			return;
		}
		ArmorData armorData = this.m_LimbMap[(int)slot.m_Limb];
		ArmorInfo armorInfo = (ArmorInfo)item.m_Info;
		armorData.m_MeshMap[(int)armorInfo.m_ArmorType].SetActive(false);
		Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(item.gameObject);
		for (int i = 0; i < componentsDeepChild.Length; i++)
		{
			componentsDeepChild[i].enabled = true;
		}
		BodyInspectionController.Get().OnArmorRemoved(slot.m_Limb);
	}

	public void ArmorCarryStarted(Item item)
	{
		((Armor)item).m_Limb = Limb.None;
		ArmorData armorData = null;
		foreach (KeyValuePair<int, ArmorData> keyValuePair in this.m_LimbMap)
		{
			if (keyValuePair.Value.m_AttachedArmor == item)
			{
				Dictionary<int, ArmorData>.Enumerator enumerator;
				keyValuePair = enumerator.Current;
				armorData = keyValuePair.Value;
				break;
			}
		}
		if (armorData == null)
		{
			return;
		}
		item.ItemsManagerRegister(false);
		Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(item.gameObject);
		for (int i = 0; i < componentsDeepChild.Length; i++)
		{
			componentsDeepChild[i].enabled = true;
		}
		Limb limb = armorData.m_Slot.m_Limb;
		item.m_BoxCollider.center = armorData.m_AttachedArmorOrigCollCenter;
		item.m_BoxCollider.size = armorData.m_AttachedArmorOrigCollSize;
		armorData.m_Slot.m_Item = null;
		armorData.m_Mesh.SetActive(false);
		armorData.m_DestroyedMesh.SetActive(false);
		armorData.Reset();
		this.OnArmorRemoved(limb);
	}

	public void OnArmorRemoved(Limb limb)
	{
		BodyInspectionController.Get().OnArmorRemoved(limb);
	}

	public void SetMeshesVisible(bool set)
	{
		foreach (KeyValuePair<int, ArmorData> keyValuePair in this.m_LimbMap)
		{
			ArmorData value = keyValuePair.Value;
			if (value.m_ArmorType != ArmorType.None)
			{
				if (set)
				{
					if (value.IsArmorDestroyed())
					{
						value.m_MeshMap[(int)value.m_ArmorType].SetActive(false);
						value.m_DestroyedMesh.SetActive(true);
						if (value.m_Cloth)
						{
							value.m_Cloth.enabled = false;
							value.m_Cloth.enabled = true;
						}
					}
					else
					{
						value.m_MeshMap[(int)value.m_ArmorType].SetActive(true);
						value.m_DestroyedMesh.SetActive(false);
					}
				}
				else
				{
					value.m_MeshMap[(int)value.m_ArmorType].SetActive(false);
					value.m_DestroyedMesh.SetActive(false);
				}
			}
		}
	}

	public bool IsItemAttached(Item item)
	{
		foreach (KeyValuePair<int, ArmorData> keyValuePair in this.m_LimbMap)
		{
			if (keyValuePair.Value.m_AttachedArmor == item)
			{
				return true;
			}
		}
		return false;
	}

	public float GetAbsorption(Limb limb)
	{
		ArmorData armorData = this.m_LimbMap[(int)limb];
		if (armorData.m_ArmorType != ArmorType.None && !armorData.IsArmorDestroyed())
		{
			return CJTools.Math.GetProportionalClamp(armorData.m_MinAbsorption, 1f, armorData.m_AttachedArmor.m_Info.m_Health, 0f, armorData.m_AttachedArmor.m_Info.m_MaxHealth) * armorData.m_Absorption;
		}
		return 0f;
	}

	public void SetPhaseCompleted(ArmorTakeDamagePhase phase)
	{
		this.m_ArmorTakeDamagePhaseCompleted |= (int)phase;
	}

	public override void OnTakeDamage(DamageInfo info)
	{
		base.OnTakeDamage(info);
		if (info.m_FromInjury)
		{
			return;
		}
		Limb limb = EnumTools.ConvertInjuryPlaceToLimb(info.m_InjuryPlace);
		if (limb == Limb.None)
		{
			limb = Limb.LArm;
		}
		if (this.m_LimbMap[(int)limb].m_ArmorType == ArmorType.None || this.m_LimbMap[(int)limb].IsArmorDestroyed())
		{
			return;
		}
		this.m_ScheduleTakeDamage = true;
		this.m_DamageToTake = info.m_Damage * this.GetAbsorption(limb);
		if (info.m_DamageType == DamageType.SnakePoison || info.m_DamageType == DamageType.VenomPoison)
		{
			this.m_DamageToTake *= this.m_PoisonDamageMul;
		}
		this.m_DamageLimb = limb;
	}

	public override void LateUpdate()
	{
		if (Debug.isDebugBuild)
		{
			this.UpdateDebug();
		}
		base.LateUpdate();
		bool flag = ScenarioManager.Get().IsDream();
		if (flag && !this.m_WasDreamActive)
		{
			this.SetMeshesVisible(false);
		}
		else if (!flag && this.m_WasDreamActive)
		{
			this.SetMeshesVisible(true);
		}
		this.m_WasDreamActive = flag;
		if ((this.m_ScheduleTakeDamage && (this.m_ArmorTakeDamagePhaseCompleted & 2) != 0 && (this.m_ArmorTakeDamagePhaseCompleted & 1) != 0) || (Debug.isDebugBuild && Input.GetKey(KeyCode.RightAlt) && Input.GetKey(KeyCode.P)))
		{
			if (this.m_DamageLimb == Limb.None)
			{
				this.m_DamageLimb = Limb.LArm;
				this.m_DamageToTake = 100f;
			}
			ArmorData armorData = this.m_LimbMap[(int)this.m_DamageLimb];
			if (armorData != null)
			{
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.m_Damage = this.m_DamageToTake;
				armorData.m_AttachedArmor.TakeDamage(damageInfo);
				if (armorData.m_AttachedArmor.m_Info.m_Health <= 0f)
				{
					this.OnArmorDestroyed(armorData);
				}
			}
			this.m_ScheduleTakeDamage = false;
			this.m_ArmorTakeDamagePhaseCompleted = 0;
			this.m_DamageLimb = Limb.None;
		}
		Dictionary<int, ArmorData>.Enumerator enumerator = this.m_LimbMap.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<int, ArmorData> keyValuePair = enumerator.Current;
			ArmorData value = keyValuePair.Value;
			if (value.m_Destroyed && value.m_AttachedArmor)
			{
				value.m_AttachedArmor.transform.localRotation = Quaternion.identity;
				value.m_AttachedArmor.transform.localPosition = Vector3.zero;
			}
		}
		enumerator.Dispose();
	}

	public void InsertArmorDesstroyed(Limb limb)
	{
		this.OnArmorDestroyed(this.m_LimbMap[(int)limb]);
	}

	private void OnArmorDestroyed(ArmorData armor_data)
	{
		if (armor_data.m_AttachedArmor != null)
		{
			UnityEngine.Object.Destroy(armor_data.m_AttachedArmor.gameObject);
		}
		Item item = ItemsManager.Get().CreateItem(ItemID.broken_armor, false, armor_data.m_Slot.transform);
		item.transform.SetParent(armor_data.m_Slot.transform);
		Physics.IgnoreCollision(this.m_Player.m_Collider, item.GetComponent<Collider>());
		Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(item.gameObject);
		for (int i = 0; i < componentsDeepChild.Length; i++)
		{
			componentsDeepChild[i].enabled = false;
		}
		armor_data.m_Slot.m_Item = item;
		if (armor_data.m_Mesh)
		{
			armor_data.m_Mesh.SetActive(false);
		}
		if (armor_data.m_DestroyedMesh)
		{
			armor_data.m_DestroyedMesh.SetActive(true);
			if (armor_data.m_Cloth)
			{
				armor_data.m_Cloth.enabled = false;
				armor_data.m_Cloth.enabled = true;
			}
		}
		armor_data.m_Mesh = armor_data.m_DestroyedMesh;
		armor_data.m_AttachedArmor = item;
		armor_data.m_AttachedArmorOrigCollCenter = armor_data.m_AttachedArmor.m_BoxCollider.center;
		armor_data.m_AttachedArmorOrigCollSize = armor_data.m_AttachedArmor.m_BoxCollider.size;
		armor_data.m_AttachedArmor.m_BoxCollider.center = armor_data.m_AttachedArmorCollCenter;
		armor_data.m_AttachedArmor.m_BoxCollider.size = armor_data.m_AttachedArmorCollSize;
		armor_data.m_Destroyed = true;
		((Armor)item).m_Limb = armor_data.m_Slot.m_Limb;
		ArmorInfo armorInfo = (ArmorInfo)item.m_Info;
		armor_data.m_ArmorType = armorInfo.m_ArmorType;
		item.transform.localPosition = Vector3.zero;
	}

	public bool IsAnyArmorActive()
	{
		foreach (KeyValuePair<int, ArmorData> keyValuePair in this.m_LimbMap)
		{
			if (keyValuePair.Value.m_ArmorType != ArmorType.None)
			{
				Dictionary<int, ArmorData>.Enumerator enumerator;
				keyValuePair = enumerator.Current;
				if (!keyValuePair.Value.IsArmorDestroyed())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsArmorActive(Limb limb)
	{
		foreach (KeyValuePair<int, ArmorData> keyValuePair in this.m_LimbMap)
		{
			if (keyValuePair.Value.m_ArmorType != ArmorType.None)
			{
				Dictionary<int, ArmorData>.Enumerator enumerator;
				keyValuePair = enumerator.Current;
				if (keyValuePair.Value.m_Slot.m_Limb == limb)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsArmorActive(Limb limb, ArmorType type)
	{
		foreach (KeyValuePair<int, ArmorData> keyValuePair in this.m_LimbMap)
		{
			if (keyValuePair.Value.m_ArmorType == type)
			{
				Dictionary<int, ArmorData>.Enumerator enumerator;
				keyValuePair = enumerator.Current;
				if (keyValuePair.Value.m_Slot.m_Limb == limb)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsArmorDestroyed(Limb limb)
	{
		foreach (KeyValuePair<int, ArmorData> keyValuePair in this.m_LimbMap)
		{
			if (keyValuePair.Value.m_ArmorType != ArmorType.None)
			{
				Dictionary<int, ArmorData>.Enumerator enumerator;
				keyValuePair = enumerator.Current;
				if (keyValuePair.Value.m_Slot.m_Limb == limb)
				{
					keyValuePair = enumerator.Current;
					if (keyValuePair.Value.m_Destroyed)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void ResetArmor()
	{
		foreach (KeyValuePair<int, ArmorData> keyValuePair in this.m_LimbMap)
		{
			Dictionary<int, ArmorData>.Enumerator enumerator;
			if (keyValuePair.Value.m_AttachedArmor != null)
			{
				keyValuePair = enumerator.Current;
				keyValuePair.Value.m_AttachedArmor.transform.SetParent(null);
				keyValuePair = enumerator.Current;
				keyValuePair.Value.m_AttachedArmor = null;
			}
			keyValuePair = enumerator.Current;
			keyValuePair.Value.Reset();
		}
	}

	public bool IsgameObjectEquipedArmor(GameObject go)
	{
		foreach (KeyValuePair<int, ArmorData> keyValuePair in this.m_LimbMap)
		{
			ArmorData value = keyValuePair.Value;
			if (value.m_AttachedArmor != null && go == value.m_AttachedArmor.gameObject)
			{
				return true;
			}
		}
		return false;
	}

	public bool NoArmorAfterDamage(DamageInfo damage_info)
	{
		Limb limb = EnumTools.ConvertInjuryPlaceToLimb(damage_info.m_InjuryPlace);
		if (limb == Limb.None)
		{
			return true;
		}
		if (this.m_LimbMap[(int)limb].m_ArmorType == ArmorType.None || this.m_LimbMap[(int)limb].IsArmorDestroyed())
		{
			return true;
		}
		float num = damage_info.m_Damage * this.GetAbsorption(limb);
		if (damage_info.m_DamageType == DamageType.SnakePoison || damage_info.m_DamageType == DamageType.VenomPoison)
		{
			num *= this.m_PoisonDamageMul;
		}
		return num >= this.m_LimbMap[(int)limb].m_AttachedArmor.m_Info.m_Health;
	}

	private void UpdateDebug()
	{
		if (Input.GetKeyDown(KeyCode.Alpha0) && Input.GetKey(KeyCode.LeftAlt))
		{
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.m_Damage = 10f;
			damageInfo.m_HitDir = base.transform.up * -1f;
			Player.Get().TakeDamage(damageInfo);
			DebugRender.DrawLine(base.transform.position, base.transform.position + damageInfo.m_HitDir, Color.cyan, 50f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha1) && Input.GetKey(KeyCode.LeftAlt))
		{
			DamageInfo damageInfo2 = new DamageInfo();
			damageInfo2.m_Damage = 10f;
			damageInfo2.m_HitDir = base.transform.up * -1f;
			damageInfo2.m_HitDir += base.transform.right * 1f;
			damageInfo2.m_HitDir.Normalize();
			Player.Get().TakeDamage(damageInfo2);
			DebugRender.DrawLine(base.transform.position, base.transform.position + damageInfo2.m_HitDir, Color.cyan, 50f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2) && Input.GetKey(KeyCode.LeftAlt))
		{
			DamageInfo damageInfo3 = new DamageInfo();
			damageInfo3.m_Damage = 10f;
			damageInfo3.m_HitDir = base.transform.right * 1f;
			damageInfo3.m_HitDir.Normalize();
			Player.Get().TakeDamage(damageInfo3);
			DebugRender.DrawLine(base.transform.position, base.transform.position + damageInfo3.m_HitDir, Color.cyan, 50f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3) && Input.GetKey(KeyCode.LeftAlt))
		{
			DamageInfo damageInfo4 = new DamageInfo();
			damageInfo4.m_Damage = 10f;
			damageInfo4.m_HitDir = base.transform.right * 1f;
			damageInfo4.m_HitDir += base.transform.up * 1.3f;
			damageInfo4.m_HitDir.Normalize();
			Player.Get().TakeDamage(damageInfo4);
			DebugRender.DrawLine(base.transform.position, base.transform.position + damageInfo4.m_HitDir, Color.cyan, 50f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha4) && Input.GetKey(KeyCode.LeftAlt))
		{
			DamageInfo damageInfo5 = new DamageInfo();
			damageInfo5.m_Damage = 10f;
			damageInfo5.m_HitDir = base.transform.up;
			damageInfo5.m_HitDir.Normalize();
			Player.Get().TakeDamage(damageInfo5);
			DebugRender.DrawLine(base.transform.position, base.transform.position + damageInfo5.m_HitDir, Color.cyan, 50f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha5) && Input.GetKey(KeyCode.LeftAlt))
		{
			DamageInfo damageInfo6 = new DamageInfo();
			damageInfo6.m_Damage = 10f;
			damageInfo6.m_HitDir = base.transform.up * 1.2f;
			damageInfo6.m_HitDir += base.transform.right * -1f;
			damageInfo6.m_HitDir.Normalize();
			Player.Get().TakeDamage(damageInfo6);
			DebugRender.DrawLine(base.transform.position, base.transform.position + damageInfo6.m_HitDir, Color.cyan, 50f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha6) && Input.GetKey(KeyCode.LeftAlt))
		{
			DamageInfo damageInfo7 = new DamageInfo();
			damageInfo7.m_Damage = 10f;
			damageInfo7.m_HitDir = base.transform.right * -1f;
			damageInfo7.m_HitDir.Normalize();
			Player.Get().TakeDamage(damageInfo7);
			DebugRender.DrawLine(base.transform.position, base.transform.position + damageInfo7.m_HitDir, Color.cyan, 50f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha7) && Input.GetKey(KeyCode.LeftAlt))
		{
			DamageInfo damageInfo8 = new DamageInfo();
			damageInfo8.m_Damage = 10f;
			damageInfo8.m_HitDir = base.transform.up * -1f;
			damageInfo8.m_HitDir += base.transform.right * -1f;
			damageInfo8.m_HitDir.Normalize();
			Player.Get().TakeDamage(damageInfo8);
			DebugRender.DrawLine(base.transform.position, base.transform.position + damageInfo8.m_HitDir, Color.cyan, 50f);
		}
	}

	public bool IsAnyArmorDamaged()
	{
		foreach (KeyValuePair<int, ArmorData> keyValuePair in this.m_LimbMap)
		{
			if (keyValuePair.Value.m_Destroyed)
			{
				return true;
			}
		}
		return false;
	}

	public Dictionary<int, ArmorData> m_LimbMap = new Dictionary<int, ArmorData>();

	private static PlayerArmorModule s_Instance;

	private List<ArmorSlot> m_ArmorSlots = new List<ArmorSlot>();

	private int m_ArmorTakeDamagePhaseCompleted;

	private bool m_ScheduleTakeDamage;

	private float m_DamageToTake;

	private Limb m_DamageLimb = Limb.None;

	private float m_PoisonDamageMul = 1f;

	private bool m_WasDreamActive;
}
