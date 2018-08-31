using System;
using System.Collections.Generic;
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
		List<BeingModule> list = new List<BeingModule>();
		base.GetComponents<BeingModule>(list);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].Initialize();
		}
		this.m_ModulesInitialized = true;
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
		return (!(this.m_LEye != null)) ? base.transform : this.m_LEye.transform;
	}

	public virtual Transform GetREyeTransform()
	{
		return (!(this.m_REye != null)) ? base.transform : this.m_REye.transform;
	}

	public virtual Transform GetHeadTransform()
	{
		return (!(this.m_Head != null)) ? base.transform : this.m_Head.transform;
	}

	public virtual Transform GetCamTransform()
	{
		return (!(this.m_Cam != null)) ? base.transform : this.m_Cam.transform;
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
		List<BeingModule> list = new List<BeingModule>();
		base.GetComponents<BeingModule>(list);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].OnTakeDamage(info);
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
		Vector3 position = base.transform.position;
		RaycastHit[] array = Physics.RaycastAll(position, Vector3.down * 0.5f);
		float num = float.MaxValue;
		GameObject gameObject = null;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].distance < num)
			{
				num = array[i].distance;
				gameObject = array[i].collider.gameObject;
			}
		}
		if (gameObject == null)
		{
			return EObjectMaterial.Unknown;
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
		Terrain activeTerrain = Terrain.activeTerrain;
		TerrainData terrainData = activeTerrain.terrainData;
		int num2 = (int)(position.x / terrainData.size.x * (float)terrainData.alphamapWidth);
		int num3 = (int)(position.z / terrainData.size.z * (float)terrainData.alphamapHeight);
		if (num2 > terrainData.alphamapWidth || num3 > terrainData.alphamapHeight)
		{
			return EObjectMaterial.Grass;
		}
		float[,,] alphamaps = terrainData.GetAlphamaps(num2, num3, 1, 1);
		int num4 = -1;
		float num5 = float.MinValue;
		for (int j = 0; j < terrainData.alphamapLayers; j++)
		{
			if (alphamaps[0, 0, j] > num5)
			{
				num5 = alphamaps[0, 0, j];
				num4 = j;
			}
		}
		if (num4 < 0)
		{
			return EObjectMaterial.Unknown;
		}
		switch (num4)
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
}
