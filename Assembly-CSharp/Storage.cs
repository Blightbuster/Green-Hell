using System;
using System.Collections.Generic;
using UnityEngine;

public class Storage : Construction
{
	protected override void Awake()
	{
		base.Awake();
		this.m_AudioSource = base.gameObject.GetComponent<AudioSource>();
		Storage.s_AllStorages.Add(this);
		this.m_Animator = base.gameObject.GetComponent<Animator>();
		this.m_Animator.SetInteger(this.m_StateHash, -1);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Storage.s_AllStorages.Remove(this);
	}

	public void Open()
	{
		this.m_Animator.SetInteger(this.m_StateHash, 1);
		this.m_AudioSource.PlayOneShot(this.m_OpenSound);
	}

	public void Close()
	{
		this.m_Animator.SetInteger(this.m_StateHash, 0);
		this.m_AudioSource.PlayOneShot(this.m_CloseSound);
	}

	public override bool IsStorage()
	{
		return true;
	}

	public override bool CanTrigger()
	{
		return !this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying();
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
			Storage3D.Get().Activate(this);
		}
	}

	public InsertResult InsertItem(Item item, ItemSlot slot, InventoryCellsGroup group, bool notify_if_cant, bool drop_if_cant)
	{
		if (this.m_Items.Contains(item))
		{
			return InsertResult.CantInsert;
		}
		bool isStatic = item.gameObject.isStatic;
		item.gameObject.isStatic = false;
		item.transform.parent = null;
		InsertResult insertResult = InsertResult.None;
		if (Storage3D.Get().GetGrid(this.m_Type).InsertItem(item, slot, group, true, true, this))
		{
			insertResult = InsertResult.Ok;
		}
		else
		{
			insertResult = InsertResult.NoSpace;
		}
		if (insertResult == InsertResult.Ok)
		{
			this.m_Items.Add(item);
			if (!item.m_CurrentSlot)
			{
				item.transform.parent = Storage3D.Get().transform;
			}
			item.OnAddToStorage(this);
			item.gameObject.SetActive(Inventory3DManager.Get().gameObject.activeSelf && item.m_Info.m_BackpackPocket == Inventory3DManager.Get().m_ActivePocket);
			using (List<Item>.Enumerator enumerator = this.m_Items.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item item2 = enumerator.Current;
					item2.UpdatePhx();
					item2.UpdateScale(false);
					item.UpdateLayer();
				}
				return insertResult;
			}
		}
		item.gameObject.isStatic = isStatic;
		if (notify_if_cant)
		{
			((HUDMessages)HUDManager.Get().GetHUD(typeof(HUDMessages))).AddMessage(GreenHellGame.Instance.GetLocalization().Get("HUD_NoSpaceInStorage", true), new Color?(Color.red), HUDMessageIcon.None, "", null);
			HUDManager.Get().PlaySound("Sounds/HUD/GH_Inventory_Full");
		}
		if (drop_if_cant)
		{
			Inventory3DManager.Get().DropItem(item);
		}
		return insertResult;
	}

	public void RemoveItem(Item item, bool from_destroy = false)
	{
		if (!this.m_Items.Contains(item))
		{
			return;
		}
		if (item.m_CurrentSlot)
		{
			if (item.m_CurrentSlot.IsStack())
			{
				item.m_CurrentSlot.RemoveItem(item, false);
			}
			else
			{
				item.m_CurrentSlot.RemoveItem();
			}
		}
		else if (item.m_InventorySlot && item.m_InventorySlot.m_Items.Count > 0)
		{
			item.m_InventorySlot.RemoveItem(item, from_destroy);
		}
		if (!item.m_IsBeingDestroyed)
		{
			item.transform.parent = null;
		}
		if (item.m_Info.m_InventoryCellsGroup != null)
		{
			item.m_Info.m_InventoryCellsGroup.Remove(item);
			item.m_Info.m_InventoryCellsGroup = null;
		}
		this.m_Items.Remove(item);
		item.OnRemoveFromStorage();
		foreach (Item item2 in this.m_Items)
		{
			item2.UpdatePhx();
			item2.UpdateScale(false);
			item.UpdateLayer();
		}
	}

	public bool Contains(Item item)
	{
		return this.m_Items.Contains(item);
	}

	public override void DestroyMe(bool check_connected = true)
	{
		while (this.m_Items.Count > 0)
		{
			Item item = this.m_Items[0];
			item.transform.position = base.transform.position;
			this.RemoveItem(item, false);
			item.StaticPhxRequestReset();
			item.UpdatePhx();
		}
		base.DestroyMe(check_connected);
	}

	protected override void Update()
	{
		base.Update();
		if (Input.GetKey(KeyCode.L))
		{
			this.DestroyMe(true);
		}
	}

	[HideInInspector]
	public List<Item> m_Items = new List<Item>();

	[HideInInspector]
	public Animator m_Animator;

	private int m_StateHash = Animator.StringToHash("State");

	public static List<Storage> s_AllStorages = new List<Storage>();

	public AudioClip m_OpenSound;

	public AudioClip m_CloseSound;

	private AudioSource m_AudioSource;

	public Storage3D.StorageType m_Type;
}
