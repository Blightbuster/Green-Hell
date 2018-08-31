using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SensorCompound : MonoBehaviour
{
	private void Update()
	{
		bool flag = false;
		for (int i = 0; i < this.m_List.Count; i++)
		{
			if (this.m_List[i].m_IsInside)
			{
				flag = true;
				break;
			}
		}
		if (flag != this.m_IsInside)
		{
			if (flag)
			{
				this.OnEnter();
			}
			else
			{
				this.OnExit();
			}
		}
		this.m_IsInside = flag;
	}

	private void OnEnter()
	{
		MonoBehaviour[] components = base.gameObject.GetComponents<MonoBehaviour>();
		for (int i = 0; i < components.Length; i++)
		{
			MethodInfo method = components[i].GetType().GetMethod("OnTriggerEnter");
			if (method != null)
			{
				method.Invoke(components[i], new object[]
				{
					Player.Get().GetComponent<Collider>()
				});
			}
		}
	}

	private void OnExit()
	{
		MonoBehaviour[] components = base.gameObject.GetComponents<MonoBehaviour>();
		for (int i = 0; i < components.Length; i++)
		{
			MethodInfo method = components[i].GetType().GetMethod("OnTriggerExit");
			if (method != null)
			{
				method.Invoke(components[i], new object[]
				{
					Player.Get().GetComponent<Collider>()
				});
			}
		}
	}

	public List<SensorBase> m_List = new List<SensorBase>();

	[HideInInspector]
	public bool m_IsInside;
}
