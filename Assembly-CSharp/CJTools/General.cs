using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace CJTools
{
	public static class General
	{
		public static float LinearToDecibel(float linear)
		{
			float result;
			if (linear != 0f)
			{
				result = 20f * Mathf.Log10(linear);
			}
			else
			{
				result = -144f;
			}
			return result;
		}

		public static float DecibelToLinear(float dB)
		{
			return Mathf.Pow(10f, dB / 20f);
		}

		public static T GetComponentDeepChild<T>(GameObject go)
		{
			T component = go.GetComponent<T>();
			if (component != null && !component.Equals(null))
			{
				return component;
			}
			return go.GetComponentInChildren<T>();
		}

		public static List<T> GetComponentsDeepChild<T>(GameObject go)
		{
			List<T> list = new List<T>();
			T component = go.GetComponent<T>();
			if (component != null && !component.Equals(null))
			{
				list.Add(component);
			}
			T[] componentsInChildren = go.GetComponentsInChildren<T>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				list.Add(componentsInChildren[i]);
			}
			return list;
		}

		public static Transform FindDeepChild(this Transform parent, string name)
		{
			Transform transform = parent.Find(name);
			if (transform != null)
			{
				return transform;
			}
			IEnumerator enumerator = parent.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform parent2 = (Transform)obj;
					transform = parent2.FindDeepChild(name);
					if (transform != null)
					{
						return transform;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			return null;
		}

		public static Bounds GetCoumpoundObjectBounds(GameObject obj)
		{
			Bounds result = default(Bounds);
			Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (i == 0)
				{
					result = componentsInChildren[i].bounds;
				}
				else
				{
					result.Encapsulate(componentsInChildren[i].bounds);
				}
			}
			return result;
		}

		public static T[] GetAtPath<T>(string path)
		{
			ArrayList arrayList = new ArrayList();
			return null;
		}

		public static bool IsItemCooked(ItemID item)
		{
			return item == ItemID.Fat_Meat_Cooked || item == ItemID.Fish_Meat_Cooked || item == ItemID.Lean_Meat_Cooked || item == ItemID.Meat_Cooked || item == ItemID.Larva_Coocked;
		}

		public static bool IsItemRawMeat(ItemID item)
		{
			return item == ItemID.Fat_Meat_Raw || item == ItemID.Fish_Meat_Raw || item == ItemID.Lean_Meat_Raw || item == ItemID.Meat_Raw || item == ItemID.Prawn_Meat_Raw || item == ItemID.Larva;
		}
	}
}
