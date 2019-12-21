using System;
using System.Collections.Generic;
using System.Text;
using CJTools;
using Enums;
using UnityEngine;

public class GhostSlot : Trigger, IItemSlotParent
{
	protected override void Awake()
	{
		base.Awake();
		this.m_IgnoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
		this.m_Parent = base.transform.parent.gameObject.GetComponent<ConstructionGhost>();
		DebugUtils.Assert(this.m_Parent, true);
		this.m_Renderer = base.gameObject.GetComponent<Renderer>();
		this.m_ItemID = EnumUtils<ItemID>.GetValue(this.m_ItemName);
		if (base.m_Collider)
		{
			base.m_Collider.enabled = false;
			this.m_ColliderTrigger = base.m_Collider.isTrigger;
			this.m_ColliderEnabled = false;
		}
		base.enabled = false;
	}

	public void Init()
	{
		if (this.m_Fulfilled || !this.m_Parent)
		{
			return;
		}
		this.m_Renderer.material = this.m_Parent.m_InactiveMaterial;
		this.m_RendererMaterialNameHash = Animator.StringToHash(this.m_Parent.m_InactiveMaterial.name);
	}

	public override string GetName()
	{
		return EnumUtils<ItemID>.GetName((int)this.m_ItemID);
	}

	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && !ReplicatedPlayerTriggerHelper.IsTriggerActiveForOtherPlayer(this) && base.enabled;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		if (currentItem && currentItem.GetInfoID() == this.m_ItemID)
		{
			actions.Add(TriggerAction.TYPE.Insert);
			return;
		}
		if (InventoryBackpack.Get().Contains(this.m_ItemID))
		{
			actions.Add(TriggerAction.TYPE.Insert);
		}
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Insert)
		{
			Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
			if (currentItem && this.m_ItemID == currentItem.GetInfoID() && currentItem.m_Info.IsHeavyObject())
			{
				HeavyObject heavyObject = (HeavyObject)currentItem;
				if (heavyObject.m_Attached.Count > 0)
				{
					this.Fulfill(false);
					heavyObject.DetachHeavyObject(0, true);
					return;
				}
				HeavyObjectController.Get().InsertToGhostSlot(this);
				return;
			}
			else
			{
				this.Fulfill(false);
				InventoryBackpack.Get().RemoveItem(this.m_ItemID, 1);
			}
		}
	}

	public void Reset()
	{
		this.m_Fulfilled = false;
		if (base.m_Collider)
		{
			base.m_Collider.isTrigger = this.m_ColliderTrigger;
			base.m_Collider.enabled = this.m_ColliderEnabled;
		}
		this.SetLayer(base.transform, this.m_DefaultLayer);
	}

	public void Fulfill(bool from_save = false)
	{
		if (!this.ReplIsBeingDeserialized(true))
		{
			this.ReplRequestOwnership(true);
		}
		if (this.m_ResultMaterials != null && this.m_ResultMaterials.Length != 0)
		{
			this.m_Renderer.materials = this.m_ResultMaterials;
		}
		else
		{
			Transform transform = this.m_Parent.m_ResultPrefab.transform.FindDeepChild(base.name);
			if (transform)
			{
				Renderer component = transform.gameObject.GetComponent<Renderer>();
				this.m_Renderer.materials = component.sharedMaterials;
			}
			else
			{
				DebugUtils.Assert(string.Concat(new string[]
				{
					"[GhostSlot::Fulfill] Can't set slot material because can't find element ",
					base.name,
					" in ",
					this.m_Parent.m_ResultPrefab.name,
					"."
				}), true, DebugUtils.AssertType.Info);
			}
		}
		this.m_Fulfilled = true;
		this.m_Parent.OnGhostFulfill(this.m_ItemID, from_save);
		if (base.m_Collider)
		{
			base.m_Collider.isTrigger = false;
			base.m_Collider.enabled = true;
		}
		if (base.m_Collider && this.m_NoCollisionWhenFilled)
		{
			base.m_Collider.enabled = false;
		}
		this.SetLayer(base.transform, this.m_IgnoreRaycastLayer);
		base.enabled = false;
		PlayerSanityModule.Get().OnWhispersEvent(PlayerSanityModule.WhisperType.GhostSlot);
	}

	public override void UpdateLayer()
	{
	}

	public void Activate()
	{
		if (this.m_Fulfilled)
		{
			return;
		}
		this.m_Renderer.material = this.m_Parent.m_ActiveMaterial;
		this.m_RendererMaterialNameHash = Animator.StringToHash(this.m_Parent.m_ActiveMaterialName);
		if (base.m_Collider)
		{
			base.m_Collider.enabled = true;
			base.m_Collider.isTrigger = true;
		}
		base.enabled = true;
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateMaterial();
	}

	private void UpdateMaterial()
	{
		if (!this.m_Fulfilled)
		{
			Material material;
			string name;
			if (TriggerController.Get().GetBestTrigger() == this)
			{
				material = this.m_Parent.m_HighlightedMaterial;
				name = this.m_Parent.m_HighlightedMaterialName;
			}
			else
			{
				material = this.m_Parent.m_ActiveMaterial;
				name = this.m_Parent.m_ActiveMaterialName;
			}
			if (this.m_RendererMaterialNameHash != Animator.StringToHash(name))
			{
				this.m_Renderer.material = material;
				this.m_RendererMaterialNameHash = Animator.StringToHash(name);
			}
		}
	}

	public void SetColor(Color color)
	{
		foreach (Material material in base.gameObject.GetComponent<Renderer>().materials)
		{
			Color color2 = material.color;
			color2.a = color.a;
			material.color = color2;
		}
	}

	public void OnRemoveItem(ItemSlot slot)
	{
	}

	public bool CanInsertItem(Item item)
	{
		return true;
	}

	public void OnInsertItem(ItemSlot slot)
	{
	}

	public void Deconstruct()
	{
		if (this.m_Fulfilled)
		{
			Vector3 position = base.transform.position;
			float terrainY = MainLevel.GetTerrainY(position);
			position.y = Mathf.Max(position.y, terrainY);
			ItemsManager.Get().CreateItem(this.m_ItemID, true, position, Quaternion.identity);
		}
	}

	public override string GetIconName()
	{
		return "Put_into_construction_icon";
	}

	public override string GetTriggerInfoLocalized()
	{
		int num = InventoryBackpack.Get().GetItemsCount(this.m_ItemID);
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		if (currentItem && currentItem.GetInfoID() == this.m_ItemID)
		{
			num++;
			if (currentItem.m_Info.IsHeavyObject())
			{
				num += ((HeavyObject)currentItem).m_Attached.Count;
			}
		}
		int itemsCountInCurrentStep = this.m_Parent.GetItemsCountInCurrentStep(this.m_ItemID);
		int insertedItemsCountInCurrentStep = this.m_Parent.GetInsertedItemsCountInCurrentStep(this.m_ItemID);
		return this.m_TriggerInfoHolder.GetText(this.m_ItemID, insertedItemsCountInCurrentStep, itemsCountInCurrentStep, num);
	}

	private ConstructionGhost m_Parent;

	[HideInInspector]
	public bool m_Fulfilled;

	public string m_ItemName = string.Empty;

	[HideInInspector]
	public ItemID m_ItemID = ItemID.None;

	private Renderer m_Renderer;

	private int m_RendererMaterialNameHash;

	public Material[] m_ResultMaterials;

	public GameObject m_PositionDummy;

	public bool m_NoCollisionWhenFilled;

	private int m_IgnoreRaycastLayer;

	private bool m_ColliderTrigger;

	private bool m_ColliderEnabled;

	private GhostSlot.TriggerInfoHolder m_TriggerInfoHolder;

	private struct TriggerInfoHolder
	{
		public string GetText(ItemID item, int inserted, int all_count, int count)
		{
			if (this.text == null || this.item != item || this.inserted != inserted || this.all_count != all_count || this.count != count)
			{
				this.item = item;
				this.inserted = inserted;
				this.all_count = all_count;
				this.count = count;
				StringBuilder stringBuilder = new StringBuilder(GreenHellGame.Instance.GetLocalization().Get(EnumUtils<ItemID>.GetName((int)item), true));
				stringBuilder.Append(" ");
				stringBuilder.Append(inserted);
				stringBuilder.Append("/");
				stringBuilder.Append(all_count);
				stringBuilder.Append(" (");
				stringBuilder.Append(count);
				stringBuilder.Append(")");
				this.text = stringBuilder.ToString();
			}
			return this.text;
		}

		public ItemID item;

		public int inserted;

		public int all_count;

		public int count;

		private string text;
	}
}
