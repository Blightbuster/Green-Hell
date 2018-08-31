using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

namespace AIs
{
	public class DeadBody : Trigger, ITriggerThrough
	{
		protected override void Awake()
		{
			base.Awake();
			AIManager.Get().m_DeadBodies.Add(this);
			this.m_Renderer = base.GetComponentInChildren<SkinnedMeshRenderer>();
			this.m_VisModule = base.GetComponent<VisModule>();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			AIManager.Get().m_DeadBodies.Remove(this);
		}

		protected override void Start()
		{
			base.Start();
			this.m_StartTime = MainLevel.s_GameTime;
			this.m_CanTakeToInventory = base.gameObject.GetComponent<AIInTrapTrigger>();
			this.m_HasBody = GreenHellGame.Instance.GetPrefab(this.m_AIID.ToString() + "_Body");
			if (this.m_HasBody)
			{
				ItemID id = (ItemID)Enum.Parse(typeof(ItemID), this.m_AIID.ToString() + "_Body");
				this.m_ItemInfo = ItemsManager.Get().GetInfo(id);
			}
		}

		public override void GetActions(List<TriggerAction.TYPE> actions)
		{
			if (HeavyObjectController.Get().IsActive())
			{
				return;
			}
			base.GetActions(actions);
			if (this.m_CanTakeToInventory || this.m_HasBody)
			{
				actions.Add(TriggerAction.TYPE.Take);
			}
			if (this.m_Trap && this.m_Trap.m_Info.m_ID == ItemID.Snare_Trap)
			{
				actions.Add(TriggerAction.TYPE.RemoveFromSnareTrap);
			}
			else if (!GreenHellGame.ROADSHOW_DEMO)
			{
				actions.Add(TriggerAction.TYPE.Harvest);
			}
		}

		public override void OnExecute(TriggerAction.TYPE action)
		{
			base.OnExecute(action);
			if (action == TriggerAction.TYPE.Take)
			{
				Item item = ItemsManager.Get().CreateItem(this.m_AIID.ToString() + "_Body", false);
				item.transform.rotation = base.transform.rotation;
				item.transform.position = base.transform.position;
				if (this.m_AIID == AI.AIID.PoisonDartFrog)
				{
					List<Renderer> componentsDeepChild = General.GetComponentsDeepChild<Renderer>(base.gameObject);
					Material material = null;
					for (int i = 0; i < componentsDeepChild.Count; i++)
					{
						material = componentsDeepChild[i].material;
					}
					item.ApplyMaterial(material);
				}
				item.Take();
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else if (action == TriggerAction.TYPE.Harvest)
			{
				Player.Get().HideWeapon();
				HarvestingAnimalController.Get().SetBody(this);
				Player.Get().StartController(PlayerControllerType.HarvestingAnimal);
			}
			else if (action == TriggerAction.TYPE.RemoveFromSnareTrap && this.m_AI)
			{
				this.m_AI.StartRagdoll();
				UnityEngine.Object.Destroy(this.m_AI.GetComponent<AI>());
				UnityEngine.Object.Destroy(this.m_AI.GetComponent<Animator>());
				this.m_Trap = null;
			}
		}

		public void Harvest()
		{
			AIParams aiparams = AIManager.Get().m_AIParamsMap[(int)this.m_AIID];
			foreach (GameObject gameObject in aiparams.m_HarvestingResult)
			{
				Item component = gameObject.GetComponent<Item>();
				DebugUtils.Assert(component != null, "[DeadBody:OnExecute] Harvesting result list contains object without Item component - " + gameObject.name, true, DebugUtils.AssertType.Info);
				ItemID itemID = (ItemID)Enum.Parse(typeof(ItemID), component.m_InfoName);
				if (MainLevel.s_GameTime - this.m_StartTime >= aiparams.m_DeadBodyFoodSpoilTime)
				{
					ItemInfo info = ItemsManager.Get().GetInfo(itemID);
					if (info.IsFood())
					{
						FoodInfo foodInfo = (FoodInfo)info;
						if (foodInfo.m_SpoilEffectID != ItemID.None)
						{
							itemID = foodInfo.m_SpoilEffectID;
						}
					}
				}
				for (int i = 0; i < Skill.Get<HarvestingAnimalsSkill>().GetItemsCountMul(); i++)
				{
					Item item = ItemsManager.Get().CreateItem(itemID, false, base.transform);
					item.Take();
				}
			}
			if (this.m_AddHarvestingItem != ItemID.None)
			{
				Item item2 = ItemsManager.Get().CreateItem(this.m_AddHarvestingItem, false, base.transform);
				item2.Take();
			}
			Item[] componentsInChildren = base.transform.GetComponentsInChildren<Item>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].transform.parent = null;
				componentsInChildren[j].StaticPhxRequestRemove();
			}
			UnityEngine.Object.Destroy(base.gameObject);
			if (!AI.IsHuman(this.m_AIID))
			{
				Skill.Get<HarvestingAnimalsSkill>().OnSkillAction();
			}
		}

		protected override void Update()
		{
			base.Update();
			if (this.m_RagdollBones != null && this.m_RagdollBones.Count > 0)
			{
				Bounds bounds = new Bounds(this.m_RagdollBones[0].position, Vector3.zero);
				foreach (Transform transform in this.m_RagdollBones)
				{
					bounds.Encapsulate(transform.position);
				}
				this.m_BoxCollider.size = bounds.size * 1.5f;
				this.m_BoxCollider.center = base.transform.InverseTransformPoint(bounds.center);
			}
			else
			{
				this.m_BoxCollider.center = Vector3.zero;
				Vector3 vector = this.m_Renderer.sharedMesh.bounds.size;
				vector = Vector3.Scale(vector, this.m_Renderer.gameObject.transform.localScale);
				this.m_BoxCollider.size = vector;
			}
		}

		public override string GetName()
		{
			return this.m_AIID.ToString();
		}

		public override string GetIconName()
		{
			if (this.m_AI && this.m_AI.m_Trap && this.m_AI.m_Trap.m_Info.m_ID == ItemID.Snare_Trap)
			{
				return "HUD_cut_off_trap";
			}
			if (!this.m_CanTakeToInventory && !this.m_HasBody)
			{
				return "HUDTrigger_Harvest";
			}
			return (this.m_ItemInfo == null) ? string.Empty : this.m_ItemInfo.m_IconName;
		}

		public void OnTakeDamage(DamageInfo info)
		{
			if (this.m_VisModule)
			{
				this.m_VisModule.OnTakeDamage(info);
			}
		}

		public AI.AIID m_AIID = AI.AIID.None;

		public ItemID m_AddHarvestingItem = ItemID.None;

		[HideInInspector]
		public AI m_AI;

		[HideInInspector]
		public Trap m_Trap;

		[HideInInspector]
		public bool m_Ragdoll;

		private float m_StartTime;

		private bool m_CanTakeToInventory;

		private bool m_HasBody;

		private SkinnedMeshRenderer m_Renderer;

		public List<Transform> m_RagdollBones;

		private ItemInfo m_ItemInfo;

		private VisModule m_VisModule;

		public float m_DeactivationTime;
	}
}
