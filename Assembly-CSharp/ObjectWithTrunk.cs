using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectWithTrunk : MonoBehaviour
{
	private void Awake()
	{
		ObjectWithTrunk.s_ObjectsWithTrunk.Add(this);
	}

	public static void OnLoad()
	{
		ObjectWithTrunk.DestroyObjects();
	}

	public static void OnSave()
	{
		ObjectWithTrunk.DestroyObjects();
	}

	private static void DestroyObjects()
	{
		int i = 0;
		while (i < ObjectWithTrunk.s_ObjectsWithTrunk.Count)
		{
			if (ObjectWithTrunk.s_ObjectsWithTrunk[i] == null)
			{
				ObjectWithTrunk.s_ObjectsWithTrunk.RemoveAt(i);
			}
			else
			{
				int j = 0;
				while (j < ObjectWithTrunk.s_ObjectsWithTrunk[i].gameObject.transform.childCount)
				{
					Transform child = ObjectWithTrunk.s_ObjectsWithTrunk[i].gameObject.transform.GetChild(j);
					if (child.gameObject != ObjectWithTrunk.s_ObjectsWithTrunk[i].m_Trunk)
					{
						child.parent = null;
					}
					else
					{
						j++;
					}
				}
				UnityEngine.Object.Destroy(ObjectWithTrunk.s_ObjectsWithTrunk[i].gameObject);
				ObjectWithTrunk.s_ObjectsWithTrunk.RemoveAt(i);
			}
		}
	}

	public static List<ObjectWithTrunk> s_ObjectsWithTrunk = new List<ObjectWithTrunk>();

	public GameObject m_Trunk;
}
