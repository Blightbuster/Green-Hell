using System;
using System.Collections.Generic;
using UnityEngine;

public class FirecampGroupsManager : MonoBehaviour
{
	public static FirecampGroupsManager Get()
	{
		if (FirecampGroupsManager.s_Instance == null)
		{
			FirecampGroupsManager.s_Instance = UnityEngine.Object.FindObjectOfType<FirecampGroupsManager>();
		}
		return FirecampGroupsManager.s_Instance;
	}

	private void Awake()
	{
		FirecampGroupsManager.s_Instance = this;
		this.m_BurningFactorIncreaseSpeed = new float[3];
		this.m_BurningFactorDecreaseSpeed = new float[3];
	}

	public void OnCreateFirecamp(IFireObject firecamp)
	{
		if (firecamp.GetConstruction().IsSceneObject())
		{
			return;
		}
		this.m_TempFirecampGroups.Clear();
		foreach (FirecampGroup firecampGroup in this.m_FirecampGroups)
		{
			if (firecampGroup.CanAdd(firecamp))
			{
				firecampGroup.Add(firecamp);
				this.m_TempFirecampGroups.Add(firecampGroup);
			}
		}
		if (this.m_TempFirecampGroups.Count == 0)
		{
			FirecampGroup firecampGroup2 = new FirecampGroup();
			firecampGroup2.Add(firecamp);
			this.m_FirecampGroups.Add(firecampGroup2);
			return;
		}
		if (this.m_TempFirecampGroups.Count > 1)
		{
			float num = 0f;
			foreach (FirecampGroup firecampGroup3 in this.m_TempFirecampGroups)
			{
				num = Mathf.Max(num, firecampGroup3.m_BurningFactor);
			}
			FirecampGroup firecampGroup4 = this.m_TempFirecampGroups[0];
			firecampGroup4.m_BurningFactor = num;
			while (this.m_TempFirecampGroups.Count > 1)
			{
				firecampGroup4.Add(this.m_TempFirecampGroups[1]);
				this.m_FirecampGroups.Remove(this.m_TempFirecampGroups[1]);
				this.m_TempFirecampGroups.RemoveAt(1);
			}
		}
	}

	public void OnDestroyFirecamp(IFireObject firecamp)
	{
		if (firecamp.GetConstruction().IsSceneObject())
		{
			return;
		}
		foreach (FirecampGroup firecampGroup in this.m_FirecampGroups)
		{
			if (firecampGroup.Contains(firecamp))
			{
				firecampGroup.Remove(firecamp);
				if (firecampGroup.GetAllCount() == 0)
				{
					this.m_FirecampGroups.Remove(firecampGroup);
					break;
				}
				break;
			}
		}
	}

	public void OnCreateConstruction(Construction construction)
	{
		if (construction.IsFirecamp())
		{
			return;
		}
		foreach (FirecampGroup firecampGroup in this.m_FirecampGroups)
		{
			firecampGroup.TryAddConstruction(construction);
		}
	}

	public void OnDestroyConstruction(Construction construction)
	{
		if (construction.IsFirecamp())
		{
			return;
		}
		foreach (FirecampGroup firecampGroup in this.m_FirecampGroups)
		{
			firecampGroup.TryRemoveConstruction(construction);
		}
	}

	public FirecampGroup GetGroupToAttack()
	{
		FirecampGroup result = null;
		float num = float.MaxValue;
		foreach (FirecampGroup firecampGroup in this.m_FirecampGroupsWithFullFactor)
		{
			float num2 = Player.Get().transform.position.Distance(firecampGroup.GetCenterPos());
			if (num2 < num)
			{
				num = num2;
				result = firecampGroup;
			}
		}
		return result;
	}

	public void ResetBurningFactors()
	{
		foreach (FirecampGroup firecampGroup in this.m_FirecampGroups)
		{
			firecampGroup.m_BurningFactor = 0f;
		}
	}

	private void Update()
	{
		this.m_FirecampGroupsWithFullFactor.Clear();
		int i = 0;
		while (i < this.m_FirecampGroups.Count)
		{
			FirecampGroup firecampGroup = this.m_FirecampGroups[i];
			firecampGroup.Update();
			if (firecampGroup.GetFirecampsCount() == 0 && firecampGroup.m_BurningFactor == 0f)
			{
				this.m_FirecampGroups.Remove(firecampGroup);
			}
			else
			{
				if (firecampGroup.m_BurningFactor == 1f)
				{
					this.m_FirecampGroupsWithFullFactor.Add(firecampGroup);
				}
				i++;
			}
		}
		if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.X))
		{
			this.m_DebugDrawFactor = !this.m_DebugDrawFactor;
		}
	}

	private List<FirecampGroup> m_FirecampGroups = new List<FirecampGroup>();

	private List<FirecampGroup> m_FirecampGroupsWithFullFactor = new List<FirecampGroup>();

	private List<FirecampGroup> m_TempFirecampGroups = new List<FirecampGroup>();

	[HideInInspector]
	public float[] m_BurningFactorIncreaseSpeed;

	[HideInInspector]
	public float[] m_BurningFactorDecreaseSpeed;

	public bool m_DebugDrawFactor;

	private static FirecampGroupsManager s_Instance;
}
