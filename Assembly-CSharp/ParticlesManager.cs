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

	public GameObject Spawn(string name, Vector3 pos, Quaternion q, Vector3 add_vel, Transform parent = null, float attach_duration = -1f, bool keep_y = false)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(GreenHellGame.Instance.GetPrefab(name), Vector3.zero, Quaternion.identity);
		gameObject.transform.SetParent(parent, false);
		gameObject.transform.position = pos;
		gameObject.transform.rotation = q;
		ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
		if (add_vel.magnitude > 0f)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				int particleCount = componentsInChildren[i].particleCount;
				ParticleSystem.Particle[] array = new ParticleSystem.Particle[particleCount];
				componentsInChildren[i].GetParticles(array);
				for (int j = 0; j < particleCount; j++)
				{
					ParticleSystem.Particle[] array2 = array;
					int num = j;
					array2[num].velocity = array2[num].velocity + add_vel;
				}
				componentsInChildren[i].SetParticles(array, particleCount);
			}
		}
		ParticlesManager.ParticlesData particlesData = new ParticlesManager.ParticlesData();
		particlesData.m_ParticleSystem = componentsInChildren;
		particlesData.m_AttachDuration = attach_duration;
		particlesData.m_AttachTime = Time.time;
		particlesData.m_KeepY = keep_y;
		particlesData.m_Y = pos.y;
		particlesData.m_LocalPos = gameObject.transform.localPosition;
		particlesData.m_LocalRot = gameObject.transform.localRotation;
		this.m_PartcleSystems.Add(gameObject, particlesData);
		return gameObject;
	}

	public GameObject Spawn(GameObject obj, Vector3 pos, Quaternion q, Vector3 add_vel, Transform parent = null, float attach_duration = -1f, bool keep_y = false)
	{
		obj.transform.SetParent(parent, false);
		obj.transform.position = pos;
		obj.transform.rotation = q;
		ParticleSystem[] componentsInChildren = obj.GetComponentsInChildren<ParticleSystem>();
		if (add_vel.magnitude > 0f)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				int particleCount = componentsInChildren[i].particleCount;
				ParticleSystem.Particle[] array = new ParticleSystem.Particle[particleCount];
				componentsInChildren[i].GetParticles(array);
				for (int j = 0; j < particleCount; j++)
				{
					ParticleSystem.Particle[] array2 = array;
					int num = j;
					array2[num].velocity = array2[num].velocity + add_vel;
				}
				componentsInChildren[i].SetParticles(array, particleCount);
			}
		}
		ParticlesManager.ParticlesData particlesData = new ParticlesManager.ParticlesData();
		particlesData.m_ParticleSystem = componentsInChildren;
		particlesData.m_AttachDuration = attach_duration;
		particlesData.m_AttachTime = Time.time;
		particlesData.m_KeepY = keep_y;
		particlesData.m_Y = pos.y;
		particlesData.m_LocalPos = obj.transform.localPosition;
		particlesData.m_LocalRot = obj.transform.localRotation;
		this.m_PartcleSystems.Add(obj, particlesData);
		return obj;
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
			KeyValuePair<GameObject, ParticlesManager.ParticlesData> keyValuePair = this.m_PartcleSystems.ElementAt(i);
			bool flag = false;
			for (int j = 0; j < keyValuePair.Value.m_ParticleSystem.Length; j++)
			{
				if (keyValuePair.Value.m_ParticleSystem[j] == null)
				{
					flag = false;
					break;
				}
				if (keyValuePair.Value.m_ParticleSystem[j].IsAlive())
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
				if (keyValuePair.Value.m_AttachDuration > 0f && Time.time - keyValuePair.Value.m_AttachTime > keyValuePair.Value.m_AttachDuration)
				{
					keyValuePair.Key.transform.SetParent(null, false);
				}
				i++;
			}
		}
	}

	private void LateUpdate()
	{
		for (int i = 0; i < this.m_PartcleSystems.Count; i++)
		{
			KeyValuePair<GameObject, ParticlesManager.ParticlesData> keyValuePair = this.m_PartcleSystems.ElementAt(i);
			if (keyValuePair.Value.m_KeepY)
			{
				Vector3 position = keyValuePair.Key.transform.position;
				position.y = keyValuePair.Value.m_Y;
				keyValuePair.Key.transform.position = position;
			}
			if (keyValuePair.Key.transform.parent)
			{
				keyValuePair.Key.transform.localPosition = keyValuePair.Value.m_LocalPos;
				keyValuePair.Key.transform.localRotation = keyValuePair.Value.m_LocalRot;
			}
		}
	}

	private Dictionary<GameObject, ParticlesManager.ParticlesData> m_PartcleSystems = new Dictionary<GameObject, ParticlesManager.ParticlesData>();

	private Dictionary<int, List<string>> m_FootstepsMap = new Dictionary<int, List<string>>();

	private static ParticlesManager s_Instance;

	private class ParticlesData
	{
		public ParticleSystem[] m_ParticleSystem;

		public float m_AttachDuration = -1f;

		public float m_AttachTime;

		public bool m_KeepY;

		public float m_Y;

		public Vector3 m_LocalPos = Vector3.zero;

		public Quaternion m_LocalRot = Quaternion.identity;
	}
}
