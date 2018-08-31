using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class GhostSlot : Trigger, IItemSlotParent
{
	protected override void Awake()
	{
		base.Awake();
		this.m_Parent = base.transform.parent.gameObject.GetComponent<ConstructionGhost>();
		DebugUtils.Assert(this.m_Parent, true);
		this.m_Renderer = base.gameObject.GetComponent<Renderer>();
		this.m_ItemID = (ItemID)Enum.Parse(typeof(ItemID), this.m_ItemName);
		if (this.m_Collider)
		{
			this.m_Collider.enabled = false;
		}
		base.enabled = false;
	}

	public void Init()
	{
		if (this.m_Fulfilled)
		{
			return;
		}
		this.m_Renderer.material = this.m_Parent.m_InactiveMaterial;
		this.m_RendererMaterialNameHash = Animator.StringToHash(this.m_Parent.m_InactiveMaterial.name);
	}

	public override string GetName()
	{
		return this.m_ItemID.ToString();
	}

	public override bool CanTrigger()
	{
		return base.enabled;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		if (currentItem && currentItem.GetInfoID() == this.m_ItemID)
		{
			actions.Add(TriggerAction.TYPE.Insert);
		}
		else if (InventoryBackpack.Get().Contains(this.m_ItemID))
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
				}
				else
				{
					HeavyObjectController.Get().InsertToGhostSlot(this);
				}
			}
			else
			{
				this.Fulfill(false);
				InventoryBackpack.Get().RemoveItem(this.m_ItemID, 1);
			}
		}
	}

	public void Fulfill(bool from_save = false)
	{
		if (this.m_ResultMaterials != null && this.m_ResultMaterials.Length > 0)
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
		if (this.m_Collider)
		{
			this.m_Collider.isTrigger = false;
		}
		if (this.m_Collider && this.m_NoCollisionWhenFilled)
		{
			this.m_Collider.enabled = false;
		}
		base.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		base.enabled = false;
		PlayerSanityModule.Get().OnWhispersEvent(PlayerSanityModule.WhisperType.GhostSlot);
	}

	public void Activate()
	{
		if (this.m_Fulfilled)
		{
			return;
		}
		this.m_Renderer.material = this.m_Parent.m_ActiveMaterial;
		this.m_RendererMaterialNameHash = Animator.StringToHash(this.m_Parent.m_ActiveMaterial.name);
		if (this.m_Collider)
		{
			this.m_Collider.enabled = true;
			this.m_Collider.isTrigger = true;
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
			if (TriggerController.Get().GetBestTrigger() == this)
			{
				material = this.m_Parent.m_HighlightedMaterial;
			}
			else
			{
				material = this.m_Parent.m_ActiveMaterial;
			}
			if (this.m_RendererMaterialNameHash != Animator.StringToHash(material.name))
			{
				this.m_Renderer.material = material;
				this.m_RendererMaterialNameHash = Animator.StringToHash(material.name);
			}
		}
	}

	public void SetColor(Color color)
	{
		Renderer component = base.gameObject.GetComponent<Renderer>();
		foreach (Material material in component.materials)
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
			ItemsManager.Get().CreateItem(this.m_ItemID, true, base.transform.position, Quaternion.identity);
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
		return string.Concat(new object[]
		{
			GreenHellGame.Instance.GetLocalization().Get(this.GetName()),
			" (",
			num,
			")"
		});
	}

	private ConstructionGhost m_Parent;

	[HideInInspector]
	public bool m_Fulfilled;

	public string m_ItemName = string.Empty;

	private ItemID m_ItemID = ItemID.None;

	private Renderer m_Renderer;

	private int m_RendererMaterialNameHash;

	public Material[] m_ResultMaterials;

	public GameObject m_PositionDummy;

	public bool m_NoCollisionWhenFilled;
}
