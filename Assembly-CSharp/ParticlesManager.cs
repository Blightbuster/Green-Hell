using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticlesManager : MonoBehaviour
{
	public static ParticlesManager Get()
	{
		return ParticlesManager.s_Instance;
	}

	private void Awake()
	{
		ParticlesManager.s_Instance = this;
	}

	public GameObject Spawn(string name, Vector3 pos, Quaternion q, Transform parent = null)
	{
		GameObject prefab = GreenHellGame.Instance.GetPrefab(name);
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity);
		gameObject.transform.SetParent(parent, false);
		gameObject.transform.position = pos;
		gameObject.transform.rotation = q;
		ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
		this.m_PartcleSystems.Add(gameObject, componentsInChildren);
		return gameObject;
	}

	public void Remove(GameObject go)
	{
		if (!go || !this.m_PartcleSystems.ContainsKey(go))
		{
			return;
		}
		this.m_PartcleSystems.Remove(go);
		UnityEngine.Object.Destroy(go);
	}

	private void Update()
	{
		int i = 0;
		while (i < this.m_PartcleSystems.Count)
		{
			KeyValuePair<GameObject, ParticleSystem[]> keyValuePair = this.m_PartcleSystems.ElementAt(i);
			bool flag = false;
			for (int j = 0; j < keyValuePair.Value.Length; j++)
			{
				if (keyValuePair.Value[j] == null)
				{
					flag = false;
					break;
				}
				if (keyValuePair.Value[j].IsAlive())
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				this.m_PartcleSystems.Remove(keyValuePair.Key);
				UnityEngine.Object.Destroy(keyValuePair.Key);
			}
			else
			{
				i++;
			}
		}
	}

	private Dictionary<GameObject, ParticleSystem[]> m_PartcleSystems = new Dictionary<GameObject, ParticleSystem[]>();

	private Dictionary<int, List<string>> m_FootstepsMap = new Dictionary<int, List<string>>();

	private static ParticlesManager s_Instance;
}
