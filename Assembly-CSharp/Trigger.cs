using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class Trigger : CJObject
{
	public Collider m_Collider
	{
		get
		{
			if (this.m_ColliderInternal == null)
			{
				return null;
			}
			return this.m_ColliderInternal.GetValueOrDefault().Get();
		}
		set
		{
			this.m_ColliderInternal = new CachedComponent<Collider>?(value);
		}
	}

	[HideInInspector]
	public int m_ForcedLayer
	{
		get
		{
			return this.m_ForcedLayerProp;
		}
		set
		{
			this.m_ForcedLayerProp = value;
			this.UpdateLayer();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		this.m_WasTriggered = false;
		this.m_BoxCollider = base.gameObject.GetComponent<BoxCollider>();
		this.m_ColliderInternal = new CachedComponent<Collider>?(new CachedComponent<Collider>(base.gameObject));
		this.m_IsCut = (this.GetName().EndsWith("_cut", StringComparison.Ordinal) || this.GetName().EndsWith("_cut_Fallen", StringComparison.Ordinal));
		this.m_DefaultLayer = base.gameObject.layer;
		this.m_InventoryLayer = LayerMask.NameToLayer("3DInventory");
		this.m_OutlineLayer = LayerMask.NameToLayer("Outline");
		Trigger.s_AllTriggers.Add(this);
	}

	protected override void Start()
	{
		base.Start();
		this.m_Initialized = true;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Trigger.s_ActiveTriggers.Add(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Trigger.s_ActiveTriggers.Remove(this);
	}

	public string GetGUID()
	{
		return this.m_GUID;
	}

	public void SetOwner(ITriggerOwner owner)
	{
		this.m_Owner = owner;
	}

	public virtual void OnExecute(TriggerAction.TYPE action)
	{
		if (this.m_RequiredItems.Count > 0 && this.m_DestroyRequiredItemsOnExecute)
		{
			foreach (ItemID item_id in this.m_RequiredItems)
			{
				Item item = InventoryBackpack.Get().FindItem(item_id);
				if (item)
				{
					if (item.m_InventorySlot && item.m_InventorySlot.m_Items.Count > 0)
					{
						UnityEngine.Object.Destroy(item.m_InventorySlot.m_Items[0].gameObject);
					}
					else
					{
						UnityEngine.Object.Destroy(item.gameObject);
					}
				}
			}
		}
		if (!this.m_WasTriggered)
		{
			this.m_WasTriggered = true;
			this.m_FirstTriggerTime = MainLevel.Instance.m_TODSky.Cycle.GameTime;
		}
		if (this.m_Owner != null)
		{
			this.m_Owner.OnExecute(this, action);
			return;
		}
		this.TryPlayExecuteSound();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Trigger.s_AllTriggers.Remove(this);
		if (base.transform.parent != null)
		{
			DestroyIfNoChildren component = base.transform.parent.GetComponent<DestroyIfNoChildren>();
			if (component)
			{
				component.OnObjectDestroyed();
			}
		}
	}

	public void TryPlayExecuteSound()
	{
		if (this.m_ExecuteSound != null)
		{
			if (this.m_ExecuteAudioSource == null)
			{
				this.m_ExecuteAudioSource = base.gameObject.AddComponent<AudioSource>();
				this.m_ExecuteAudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
				this.m_ExecuteAudioSource.playOnAwake = false;
				this.m_ExecuteAudioSource.clip = this.m_ExecuteSound;
			}
			this.m_ExecuteAudioSource.Play();
		}
	}

	public virtual void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (this.m_Owner != null)
		{
			this.m_Owner.GetActions(this, actions);
			return;
		}
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		if (this.m_DefaultAction != TriggerAction.TYPE.None)
		{
			actions.Add(this.m_DefaultAction);
		}
	}

	public virtual void GetInfoText(ref string result)
	{
	}

	public virtual string GetIconName()
	{
		if (this.m_Owner != null)
		{
			return this.m_Owner.GetIconName(this);
		}
		if (this.m_DefaultIconName.Length > 0)
		{
			return this.m_DefaultIconName;
		}
		return string.Empty;
	}

	public virtual ItemAdditionalIcon GetAdditionalIcon()
	{
		return ItemAdditionalIcon.None;
	}

	public virtual bool IsLiquidSource()
	{
		return false;
	}

	public virtual bool IsLadder()
	{
		return false;
	}

	public virtual bool IsFreeHandsLadder()
	{
		return false;
	}

	public virtual bool CanTrigger()
	{
		if (this.m_CantTriggerDuringDialog && DialogsManager.Get().IsAnyDialogPlaying())
		{
			return false;
		}
		if (this.m_Owner != null)
		{
			return this.m_Owner.CanTrigger(this);
		}
		if (!this.m_OneTime)
		{
			return !this.m_HallucinationDisappearing;
		}
		return !this.m_WasTriggered;
	}

	public virtual bool CheckRange()
	{
		return true;
	}

	public virtual bool CheckDot()
	{
		return true;
	}

	public virtual bool CheckInsideCollider()
	{
		return false;
	}

	public virtual bool IsAdditionalTrigger()
	{
		return false;
	}

	public virtual bool OnlyInCrosshair()
	{
		return false;
	}

	public virtual bool CanExecuteActions()
	{
		if (this.m_CantExecuteActionsDuringDialog && DialogsManager.Get().IsAnyDialogPlaying())
		{
			return false;
		}
		if (this.m_RequiredItems.Count > 0 && (this.m_RequiredBoolValue == string.Empty || !ScenarioManager.Get().IsBoolVariableTrue(this.m_RequiredBoolValue)))
		{
			foreach (ItemID id in this.m_RequiredItems)
			{
				if (!Player.Get().HaveItem(id))
				{
					return false;
				}
			}
		}
		return Inventory3DManager.Get().gameObject.activeSelf || (Player.Get().transform.position - this.m_Collider.ClosestPointOnBounds(Player.Get().transform.position)).magnitude < Mathf.Max(this.m_TriggerUseRange, Player.Get().GetParams().GetTriggerUseRange());
	}

	public virtual Vector3 GetIconPos()
	{
		return ((this.m_Collider && this.m_Collider.enabled) ? this.m_Collider.bounds.center : (this.m_IsBeingDestroyed ? Vector3.zero : base.gameObject.transform.position)) + Vector3.up * 0.15f;
	}

	public virtual Vector3 GetHudInfoDisplayOffset()
	{
		return Vector3.zero;
	}

	public bool WasTriggered()
	{
		return this.m_WasTriggered;
	}

	public void ResetTrigger()
	{
		this.m_WasTriggered = false;
	}

	public virtual void UpdateBestTrigger()
	{
	}

	public virtual bool TriggerThrough()
	{
		return false;
	}

	public virtual bool CanBeOutlined()
	{
		return this.m_CanBeOutlined && !HUDManager.Get().m_DebugHideHUD && !this.m_HallucinationDisappearing;
	}

	public virtual bool PlayGrabAnimOnExecute(TriggerAction.TYPE action)
	{
		return this.m_PlayGrabAnimOnExecute && (action == TriggerAction.TYPE.Take || action == TriggerAction.TYPE.TakeHold || action == TriggerAction.TYPE.TakeHoldLong || action == TriggerAction.TYPE.DrinkHold || action == TriggerAction.TYPE.PickUp);
	}

	public virtual bool IsAdditionalCollider(Collider coll)
	{
		return false;
	}

	protected override void Update()
	{
		base.Update();
	}

	public virtual void UpdateLayer()
	{
		if (this.m_ForcedLayer != 0)
		{
			if (base.gameObject.layer != this.m_ForcedLayer)
			{
				this.SetLayer(base.transform, this.m_ForcedLayer);
			}
			return;
		}
		int num = this.m_DefaultLayer;
		if (this == TriggerController.Get().GetBestTrigger() && this.CanBeOutlined())
		{
			num = this.m_OutlineLayer;
		}
		if (base.gameObject.layer != num)
		{
			this.SetLayer(base.transform, num);
		}
	}

	public virtual void SetLayer(Transform trans, int layer)
	{
		trans.gameObject.layer = layer;
		for (int i = 0; i < trans.childCount; i++)
		{
			this.SetLayer(trans.GetChild(i), layer);
		}
	}

	public virtual void Save()
	{
		if (this.m_GUID != string.Empty)
		{
			SaveGame.SaveVal(this.m_GUID, this.m_WasTriggered);
		}
	}

	public virtual void Load()
	{
		if (this.m_GUID != string.Empty)
		{
			this.m_WasTriggered = SaveGame.LoadBVal(this.m_GUID);
			if (this.m_WasTriggered)
			{
				this.m_FirstTriggerTime = MainLevel.Instance.m_TODSky.Cycle.GameTime;
			}
		}
	}

	public void TryRemoveFromFallenObjectsMan()
	{
		if (this.m_FallenObject)
		{
			FallenObjectsManager.Get().RemoveItem(this);
		}
	}

	public virtual bool ShowAdditionalInfo()
	{
		return false;
	}

	public virtual string GetAdditionalInfoLocalized()
	{
		return string.Empty;
	}

	public virtual bool RequiresToolToHarvest()
	{
		return false;
	}

	public override string GetTriggerInfoLocalized()
	{
		if (this.m_Owner != null)
		{
			return this.m_Owner.GetTriggerInfoLocalized(this);
		}
		return base.GetTriggerInfoLocalized();
	}

	public virtual bool IsFIrecamp()
	{
		return false;
	}

	public virtual bool IsCharcoalFurnace()
	{
		return false;
	}

	public virtual bool IsForge()
	{
		return false;
	}

	public virtual bool IsAcre()
	{
		return false;
	}

	public bool m_ShowInfoOnHUD;

	public float m_TriggerUseRange;

	public float m_TriggerCheckRange;

	[HideInInspector]
	public bool m_WasTriggered;

	[HideInInspector]
	public float m_FirstTriggerTime;

	[HideInInspector]
	public bool m_Initialized;

	public bool m_OneTime;

	[HideInInspector]
	public BoxCollider m_BoxCollider;

	private CachedComponent<Collider>? m_ColliderInternal;

	public TriggerAction.TYPE m_DefaultAction = TriggerAction.TYPE.None;

	public string m_DefaultIconName = string.Empty;

	public static HashSet<Trigger> s_AllTriggers = new HashSet<Trigger>();

	public static HashSet<Trigger> s_ActiveTriggers = new HashSet<Trigger>();

	public AudioClip m_ExecuteSound;

	[HideInInspector]
	private AudioSource m_ExecuteAudioSource;

	[HideInInspector]
	public bool m_IsCut;

	protected int m_InventoryLayer;

	[HideInInspector]
	public int m_OutlineLayer;

	[HideInInspector]
	public int m_DefaultLayer;

	private int m_ForcedLayerProp;

	public bool m_CanBeOutlined;

	public string m_GUID = string.Empty;

	public FallenObjectGenerator m_FallenObjectGenerator;

	[HideInInspector]
	public bool m_FallenObject;

	[HideInInspector]
	public float m_FallenObjectCreationTime;

	public bool m_PlayGrabAnimOnExecute = true;

	private ITriggerOwner m_Owner;

	public float m_TriggerMaxDot = -1f;

	public List<ItemID> m_RequiredItems = new List<ItemID>();

	public string m_RequiredBoolValue = string.Empty;

	public bool m_DestroyRequiredItemsOnExecute;

	public bool m_SetWasTriggeredWhenLookAt;

	public bool m_HideHUD;

	public bool m_CantTriggerDuringDialog;

	public bool m_CantExecuteActionsDuringDialog;
}
