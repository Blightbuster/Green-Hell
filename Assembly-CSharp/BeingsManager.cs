using System;
using System.Collections.Generic;
using UnityEngine;

public class BeingsManager : MonoBehaviour
{
	public static void RegisterBeing(Being being)
	{
		if (!BeingsManager.m_Beings.Contains(being))
		{
			BeingsManager.m_Beings.Add(being);
		}
	}

	public static void UnregisterBeing(Being being)
	{
		if (BeingsManager.m_Beings.Contains(being))
		{
			BeingsManager.m_Beings.Remove(being);
		}
	}

	public static List<Being> GetAllBeings()
	{
		return BeingsManager.m_Beings;
	}

	public static List<Being> GetBeingsInRange(Vector3 position, float range)
	{
		List<Being> list = new List<Being>();
		for (int i = 0; i < BeingsManager.m_Beings.Count; i++)
		{
			if (!BeingsManager.m_Beings[i].IsDead() && Vector3.Distance(BeingsManager.m_Beings[i].transform.position, position) < range)
			{
				list.Add(BeingsManager.m_Beings[i]);
			}
		}
		return list;
	}

	private static List<Being> m_Beings = new List<Being>();
}
