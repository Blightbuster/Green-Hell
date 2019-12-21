using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class Being : CJObject
{
	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		if (!this.IsAI())
		{
			this.m_LFoot = base.gameObject.transform.FindDeepChild("mixamorig:Foot.L");
			if (!this.m_LFoot)
			{
				this.m_LFoot = base.gameObject.transform.FindDeepChild("L_foot");
			}
			this.m_RFoot = base.gameObject.transform.FindDeepChild("mixamorig:Foot.R");
			if (!this.m_RFoot)
			{
				this.m_RFoot = base.gameObject.transform.FindDeepChild("R_foot");
			}
			this.m_Head = base.gameObject.transform.FindDeepChild("mixamorig:Head");
			if (!this.m_Head)
			{
				this.m_Head = base.gameObject.transform.FindDeepChild("head");
			}
		}
		this.InitializeModules();
		this.m_AnimationEventsReceiver = base.GetComponent<AnimationEventsReceiver>();
		if (this.m_AnimationEventsReceiver)
		{
			this.m_AnimationEventsReceiver.Initialize(this);
		}
	}

	public virtual void InitializeModules()
	{
		if (this.m_ModulesInitialized)
		{
			return;
		}
		base.GetComponents<BeingModule>(this.m_Modules);
		for (int i = 0; i < this.m_Modules.Count; i++)
		{
			this.m_Modules[i].Initialize(this);
		}
		this.m_ModulesInitialized = true;
		for (int j = 0; j < this.m_Modules.Count; j++)
		{
			this.m_Modules[j].PostInitialize();
		}
	}

	public void OnDestroyModule(BeingModule module)
	{
		this.m_Modules.Remove(module);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		BeingsManager.RegisterBeing(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		BeingsManager.UnregisterBeing(this);
	}

	public virtual float GetHealth()
	{
		return 0f;
	}

	public virtual bool IsDead()
	{
		return false;
	}

	public virtual Transform GetLEyeTransform()
	{
		if (!(this.m_LEye != null))
		{
			return base.transform;
		}
		return this.m_LEye.transform;
	}

	public virtual Transform GetREyeTransform()
	{
		if (!(this.m_REye != null))
		{
			return base.transform;
		}
		return this.m_REye.transform;
	}

	public virtual Transform GetHeadTransform()
	{
		if (!(this.m_Head != null))
		{
			return base.transform;
		}
		return this.m_Head.transform;
	}

	public virtual Transform GetCamTransform()
	{
		if (!(this.m_Cam != null))
		{
			return base.transform;
		}
		return this.m_Cam.transform;
	}

	public bool IsInSafeZone()
	{
		return this.m_CurrentSafeZonesCount > 0;
	}

	public void SetInSafeZone()
	{
		this.m_CurrentSafeZonesCount++;
	}

	public void ResetInSafeZone()
	{
		this.m_CurrentSafeZonesCount--;
	}

	public bool GiveDamage(GameObject damager, Item damage_item, float damage, Vector3 hit_dir, DamageType damage_type = DamageType.None, int poison_level = 0, bool critical_hit = false)
	{
		return this.TakeDamage(new DamageInfo
		{
			m_Damager = damager,
			m_Damage = damage,
			m_DamageType = damage_type,
			m_HitDir = hit_dir,
			m_PoisonLevel = poison_level,
			m_CriticalHit = critical_hit,
			m_Normal = -hit_dir,
			m_DamageItem = damage_item
		});
	}

	public override bool TakeDamage(DamageInfo info)
	{
		if (Cheats.m_GodMode && this.IsPlayer())
		{
			return false;
		}
		bool result = base.TakeDamage(info);
		for (int i = 0; i < this.m_Modules.Count; i++)
		{
			this.m_Modules[i].OnTakeDamage(info);
		}
		if (this.m_Hallucination)
		{
			base.Disappear(true);
		}
		return result;
	}

	public virtual bool IsWalking()
	{
		return false;
	}

	public virtual bool IsRunning()
	{
		return false;
	}

	public bool IsMoving()
	{
		return this.IsWalking() || this.IsRunning();
	}

	public virtual bool IsDuck()
	{
		return false;
	}

	public EObjectMaterial GetMaterial()
	{
		Vector3 vector = base.transform.position + Vector3.up * 0.2f;
		int num = Physics.RaycastNonAlloc(vector, Vector3.down * 0.5f, Being.s_RaycastCache);
		float num2 = float.MaxValue;
		GameObject gameObject = null;
		for (int i = 0; i < num; i++)
		{
			if (Being.s_RaycastCache[i].distance < num2)
			{
				num2 = Being.s_RaycastCache[i].distance;
				gameObject = Being.s_RaycastCache[i].collider.gameObject;
			}
		}
		if (gameObject == null)
		{
			return EObjectMaterial.Unknown;
		}
		if (gameObject.layer == LayerMask.NameToLayer("Water"))
		{
			return EObjectMaterial.Water;
		}
		ObjectMaterial component = gameObject.GetComponent<ObjectMaterial>();
		if (component != null)
		{
			return component.m_ObjectMaterial;
		}
		if (gameObject.GetComponent<Terrain>() == null)
		{
			return EObjectMaterial.Unknown;
		}
		TerrainData terrainData = Terrain.activeTerrain.terrainData;
		int num3 = (int)(vector.x / terrainData.size.x * (float)terrainData.alphamapWidth);
		int num4 = (int)(vector.z / terrainData.size.z * (float)terrainData.alphamapHeight);
		if (num3 > terrainData.alphamapWidth || num4 > terrainData.alphamapHeight)
		{
			return EObjectMaterial.Grass;
		}
		float[,,] alphamaps = terrainData.GetAlphamaps(num3, num4, 1, 1);
		int num5 = -1;
		float num6 = float.MinValue;
		for (int j = 0; j < terrainData.alphamapLayers; j++)
		{
			if (alphamaps[0, 0, j] > num6)
			{
				num6 = alphamaps[0, 0, j];
				num5 = j;
			}
		}
		if (num5 < 0)
		{
			return EObjectMaterial.Unknown;
		}
		switch (num5)
		{
		case 0:
			return EObjectMaterial.DryLeaves;
		case 1:
			return EObjectMaterial.Grass;
		case 2:
			return EObjectMaterial.Unknown;
		case 3:
			return EObjectMaterial.Grass;
		case 4:
			return EObjectMaterial.Mud;
		case 5:
			return EObjectMaterial.Soil;
		case 6:
			return EObjectMaterial.Stone;
		case 7:
			return EObjectMaterial.Sand;
		default:
			return EObjectMaterial.Unknown;
		}
	}

	public virtual bool IsInWater()
	{
		return false;
	}

	public virtual void Load()
	{
		this.m_CurrentSafeZonesCount = 0;
	}

	[HideInInspector]
	public bool m_DebugIsSelected;

	protected Transform m_LEye;

	protected Transform m_REye;

	protected Transform m_Head;

	protected Transform m_Cam;

	public string m_ColliderBoxesScript = string.Empty;

	[HideInInspector]
	public float m_Radius;

	protected bool m_ModulesInitialized;

	[HideInInspector]
	public AnimationEventsReceiver m_AnimationEventsReceiver;

	public Transform m_LFoot;

	public Transform m_RFoot;

	[HideInInspector]
	public int m_CurrentSafeZonesCount;

	protected List<BeingModule> m_Modules = new List<BeingModule>();

	private static RaycastHit[] s_RaycastCache = new RaycastHit[20];
}
