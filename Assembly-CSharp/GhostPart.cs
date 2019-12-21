using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class GhostPart : Trigger
{
	protected override void Awake()
	{
		base.Awake();
		this.m_IgnoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
		this.m_Parent = base.transform.GetComponentInParent<IGhostPartParent>();
		DebugUtils.Assert(this.m_Parent != null, true);
		this.m_Renderer = base.gameObject.GetComponent<Renderer>();
		this.m_DefaultMaterials = this.m_Renderer.materials;
		this.m_ItemID = (ItemID)Enum.Parse(typeof(ItemID), this.m_ItemName);
		if (base.m_Collider)
		{
			base.m_Collider.enabled = false;
		}
		base.enabled = false;
	}

	public void Init()
	{
		if (this.m_Fulfilled)
		{
			return;
		}
		this.m_Renderer.material = this.m_Parent.GetActiveMaterial();
		this.m_RendererMaterialNameHash = Animator.StringToHash(this.m_Parent.GetActiveMaterial().name);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Renderer.material = this.m_Parent.GetActiveMaterial();
		this.m_RendererMaterialNameHash = Animator.StringToHash(this.m_Parent.GetActiveMaterial().name);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Renderer.materials = this.m_DefaultMaterials;
	}

	public override string GetName()
	{
		return this.m_ItemID.ToString();
	}

	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && base.enabled;
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
				HeavyObjectController.Get().InsertToGhostPart(this);
				return;
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
		this.m_Renderer.materials = this.m_DefaultMaterials;
		this.m_Fulfilled = true;
		this.m_Parent.OnGhostFulfill(from_save);
		if (base.m_Collider)
		{
			base.m_Collider.isTrigger = false;
			base.m_Collider.enabled = true;
		}
		this.SetLayer(base.transform, this.m_IgnoreRaycastLayer);
		base.enabled = false;
		PlayerSanityModule.Get().OnWhispersEvent(PlayerSanityModule.WhisperType.GhostSlot);
	}

	public override void UpdateLayer()
	{
	}

	public void Reset()
	{
		this.m_Fulfilled = false;
		this.SetLayer(base.transform, this.m_DefaultLayer);
		if (base.m_Collider)
		{
			base.m_Collider.isTrigger = true;
			base.m_Collider.enabled = false;
		}
	}

	public void Activate()
	{
		if (this.m_Fulfilled)
		{
			return;
		}
		this.m_Renderer.material = this.m_Parent.GetActiveMaterial();
		this.m_RendererMaterialNameHash = Animator.StringToHash(this.m_Parent.GetActiveMaterial().name);
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
			if (TriggerController.Get().GetBestTrigger() == this)
			{
				material = this.m_Parent.GetHighlightedMaterial();
			}
			else
			{
				material = this.m_Parent.GetActiveMaterial();
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
			ItemsManager.Get().CreateItem(this.m_ItemID, true, base.transform.position, Quaternion.identity);
		}
	}

	public override string GetIconName()
	{
		return "Put_into_construction_icon";
	}

	public override string GetTriggerInfoLocalized()
	{
		return GreenHellGame.Instance.GetLocalization().Get(this.GetName(), true);
	}

	private IGhostPartParent m_Parent;

	[HideInInspector]
	public bool m_Fulfilled;

	public string m_ItemName = string.Empty;

	[HideInInspector]
	public ItemID m_ItemID = ItemID.None;

	private Renderer m_Renderer;

	private int m_RendererMaterialNameHash;

	private Material[] m_DefaultMaterials;

	private int m_IgnoreRaycastLayer;
}
