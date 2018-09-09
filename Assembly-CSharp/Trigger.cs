using System;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : CJObject
{
	protected override void Awake()
	{
		base.Awake();
		this.m_BoxCollider = base.gameObject.GetComponent<BoxCollider>();
		this.m_Collider = base.gameObject.GetComponent<Collider>();
		if (this.m_ExecuteSound != null)
		{
			this.m_ExecuteAudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_ExecuteAudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
			this.m_ExecuteAudioSource.playOnAwake = false;
			this.m_ExecuteAudioSource.clip = this.m_ExecuteSound;
		}
		this.m_IsCut = (this.GetName().EndsWith("_cut") || this.GetName().EndsWith("_cut_Fallen"));
		this.m_DefaultLayer = base.gameObject.layer;
		this.m_InventoryLayer = LayerMask.NameToLayer("3DInventory");
		this.m_OutlineLayer = LayerMask.NameToLayer("Outline");
		Trigger.s_AllTriggers.Add(this);
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

	public virtual void OnExecute(TriggerAction.TYPE action)
	{
		if (!this.m_WasTriggered)
		{
			this.m_WasTriggered = true;
			this.m_FirstTriggerTime = MainLevel.Instance.m_TODSky.Cycle.GameTime;
		}
		if (this.m_ExecuteAudioSource)
		{
			this.m_ExecuteAudioSource.Play();
		}
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

	public virtual void GetActions(List<TriggerAction.TYPE> actions)
	{
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

	public virtual bool CanTrigger()
	{
		return (!this.m_OneTime) ? (true && !this.m_HallucinationDisappearing) : (!this.m_WasTriggered);
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
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			return true;
		}
		float magnitude = (Player.Get().transform.position - this.m_Collider.ClosestPointOnBounds(Player.Get().transform.position)).magnitude;
		return magnitude < Mathf.Max(this.m_TriggerUseRange, Player.Get().GetParams().GetTriggerUseRange());
	}

	public virtual Vector3 GetIconPos()
	{
		return ((!this.m_Collider || !this.m_Collider.enabled) ? ((!this.m_IsBeingDestroyed) ? base.gameObject.transform.position : Vector3.zero) : this.m_Collider.bounds.center) + Vector3.up * 0.15f;
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
		return action == TriggerAction.TYPE.Take || action == TriggerAction.TYPE.TakeHold || action == TriggerAction.TYPE.DrinkHold || action == TriggerAction.TYPE.PickUp;
	}

	public virtual bool IsAdditionalCollider(Collider coll)
	{
		return false;
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateLayer();
	}

	protected virtual void UpdateLayer()
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

	public void SetLayer(Transform trans, int layer)
	{
		trans.gameObject.layer = layer;
		for (int i = 0; i < trans.childCount; i++)
		{
			this.SetLayer(trans.GetChild(i), layer);
		}
	}

	public void Save()
	{
		if (this.m_GUID != string.Empty)
		{
			SaveGame.SaveVal(this.m_GUID, this.m_WasTriggered);
		}
	}

	public void Load()
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

	public bool m_ShowInfoOnHUD;

	public float m_TriggerUseRange;

	public float m_TriggerCheckRange;

	[HideInInspector]
	public bool m_WasTriggered;

	[HideInInspector]
	public float m_FirstTriggerTime;

	public bool m_OneTime;

	[HideInInspector]
	public BoxCollider m_BoxCollider;

	public Collider m_Collider;

	public TriggerAction.TYPE m_DefaultAction = TriggerAction.TYPE.None;

	public static HashSet<Trigger> s_AllTriggers = new HashSet<Trigger>();

	public static List<Trigger> s_ActiveTriggers = new List<Trigger>();

	public AudioClip m_ExecuteSound;

	private AudioSource m_ExecuteAudioSource;

	[HideInInspector]
	public bool m_IsCut;

	protected int m_InventoryLayer;

	[HideInInspector]
	public int m_OutlineLayer;

	[HideInInspector]
	public int m_DefaultLayer;

	[HideInInspector]
	public int m_ForcedLayer;

	public bool m_CanBeOutlined;

	public string m_GUID = string.Empty;

	public FallenObjectGenerator m_FallenObjectGenerator;

	[HideInInspector]
	public bool m_FallenObject;

	[HideInInspector]
	public float m_FallenObjectCreationTime;
}
