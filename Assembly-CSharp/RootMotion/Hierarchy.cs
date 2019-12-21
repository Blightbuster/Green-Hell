using System;
using UnityEngine;

namespace RootMotion
{
	public class Hierarchy
	{
		public static bool HierarchyIsValid(Transform[] bones)
		{
			for (int i = 1; i < bones.Length; i++)
			{
				if (!Hierarchy.IsAncestor(bones[i], bones[i - 1]))
				{
					return false;
				}
			}
			return true;
		}

		public static UnityEngine.Object ContainsDuplicate(UnityEngine.Object[] objects)
		{
			for (int i = 0; i < objects.Length; i++)
			{
				for (int j = 0; j < objects.Length; j++)
				{
					if (i != j && objects[i] == objects[j])
					{
						return objects[i];
					}
				}
			}
			return null;
		}

		public static bool IsAncestor(Transform transform, Transform ancestor)
		{
			return transform == null || ancestor == null || (!(transform.parent == null) && (transform.parent == ancestor || Hierarchy.IsAncestor(transform.parent, ancestor)));
		}

		public static bool ContainsChild(Transform transform, Transform child)
		{
			if (transform == child)
			{
				return true;
			}
			Transform[] componentsInChildren = transform.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i] == child)
				{
					return true;
				}
			}
			return false;
		}

		public static void AddAncestors(Transform transform, Transform blocker, ref Transform[] array)
		{
			if (transform.parent != null && transform.parent != blocker)
			{
				if (transform.parent.position != transform.position && transform.parent.position != blocker.position)
				{
					Array.Resize<Transform>(ref array, array.Length + 1);
					array[array.Length - 1] = transform.parent;
				}
				Hierarchy.AddAncestors(transform.parent, blocker, ref array);
			}
		}

		public static Transform GetAncestor(Transform transform, int minChildCount)
		{
			if (transform == null)
			{
				return null;
			}
			if (!(transform.parent != null))
			{
				return null;
			}
			if (transform.parent.childCount >= minChildCount)
			{
				return transform.parent;
			}
			return Hierarchy.GetAncestor(transform.parent, minChildCount);
		}

		public static Transform GetFirstCommonAncestor(Transform t1, Transform t2)
		{
			if (t1 == null)
			{
				return null;
			}
			if (t2 == null)
			{
				return null;
			}
			if (t1.parent == null)
			{
				return null;
			}
			if (t2.parent == null)
			{
				return null;
			}
			if (Hierarchy.IsAncestor(t2, t1.parent))
			{
				return t1.parent;
			}
			return Hierarchy.GetFirstCommonAncestor(t1.parent, t2);
		}

		public static Transform GetFirstCommonAncestor(Transform[] transforms)
		{
			if (transforms == null)
			{
				Debug.LogWarning("Transforms is null.");
				return null;
			}
			if (transforms.Length == 0)
			{
				Debug.LogWarning("Transforms.Length is 0.");
				return null;
			}
			for (int i = 0; i < transforms.Length; i++)
			{
				if (transforms[i] == null)
				{
					return null;
				}
				if (Hierarchy.IsCommonAncestor(transforms[i], transforms))
				{
					return transforms[i];
				}
			}
			return Hierarchy.GetFirstCommonAncestorRecursive(transforms[0], transforms);
		}

		public static Transform GetFirstCommonAncestorRecursive(Transform transform, Transform[] transforms)
		{
			if (transform == null)
			{
				Debug.LogWarning("Transform is null.");
				return null;
			}
			if (transforms == null)
			{
				Debug.LogWarning("Transforms is null.");
				return null;
			}
			if (transforms.Length == 0)
			{
				Debug.LogWarning("Transforms.Length is 0.");
				return null;
			}
			if (Hierarchy.IsCommonAncestor(transform, transforms))
			{
				return transform;
			}
			if (transform.parent == null)
			{
				return null;
			}
			return Hierarchy.GetFirstCommonAncestorRecursive(transform.parent, transforms);
		}

		public static bool IsCommonAncestor(Transform transform, Transform[] transforms)
		{
			if (transform == null)
			{
				Debug.LogWarning("Transform is null.");
				return false;
			}
			for (int i = 0; i < transforms.Length; i++)
			{
				if (transforms[i] == null)
				{
					Debug.Log("Transforms[" + i + "] is null.");
					return false;
				}
				if (!Hierarchy.IsAncestor(transforms[i], transform) && transforms[i] != transform)
				{
					return false;
				}
			}
			return true;
		}
	}
}
