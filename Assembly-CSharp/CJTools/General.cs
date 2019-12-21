using System;
using System.Collections;
using Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CJTools
{
	public static class General
	{
		public static bool IsNumber(this object value)
		{
			return value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong || value is float || value is double || value is decimal;
		}

		public static string StrikeThrough(string s)
		{
			string text = "";
			for (int i = 0; i < s.Length; i++)
			{
				text = text + s[i].ToString() + "̶";
			}
			return text;
		}

		public static GameObject FindObject(string name)
		{
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				Scene sceneAt = SceneManager.GetSceneAt(i);
				if (sceneAt.isLoaded)
				{
					GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
					for (int j = 0; j < rootGameObjects.Length; j++)
					{
						if (rootGameObjects[j].name == name)
						{
							return rootGameObjects[j];
						}
						Transform[] componentsInChildren = rootGameObjects[j].GetComponentsInChildren<Transform>(true);
						for (int k = 0; k < componentsInChildren.Length; k++)
						{
							if (componentsInChildren[k].name == name)
							{
								return componentsInChildren[k].gameObject;
							}
						}
					}
				}
			}
			return null;
		}

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

		public static T GetComponentUpRecursive<T>(GameObject go)
		{
			T t = go.GetComponent<T>();
			if (t != null && !t.Equals(null))
			{
				return t;
			}
			if (go.transform.parent != null)
			{
				t = General.GetComponentUpRecursive<T>(go.transform.parent.gameObject);
			}
			return t;
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

		public static T[] GetComponentsDeepChild<T>(GameObject go)
		{
			return go.GetComponentsInChildren<T>(true);
		}

		public static Transform FindDeepChild(this Transform parent, string name)
		{
			Transform transform = parent.Find(name);
			if (transform != null)
			{
				return transform;
			}
			for (int i = 0; i < parent.childCount; i++)
			{
				transform = parent.GetChild(i).FindDeepChild(name);
				if (transform != null)
				{
					return transform;
				}
			}
			return null;
		}

		public static Transform FindDeepChild(this Transform parent, int name_hash)
		{
			for (int i = 0; i < parent.childCount; i++)
			{
				Transform child = parent.GetChild(i);
				if (child.name.GetHashCode() == name_hash)
				{
					return child;
				}
			}
			for (int j = 0; j < parent.childCount; j++)
			{
				Transform transform = parent.GetChild(j);
				transform = transform.FindDeepChild(name_hash);
				if (transform != null)
				{
					return transform;
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
			new ArrayList();
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

		public static bool IsFloorConstruction(ItemID item)
		{
			return item == ItemID.building_bamboo_frame_floor || item == ItemID.building_frame_floor;
		}

		public static bool IsRoofConstruction(ItemID item)
		{
			return item == ItemID.building_roof || item == ItemID.building_banana_leaf_roof;
		}
	}
}
