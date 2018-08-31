using System;
using System.Collections.Generic;
using UnityEngine;

namespace RootMotion
{
	public static class LayerMaskExtensions
	{
		public static bool Contains(LayerMask mask, int layer)
		{
			return mask == (mask | 1 << layer);
		}

		public static LayerMask Create(params string[] layerNames)
		{
			return LayerMaskExtensions.NamesToMask(layerNames);
		}

		public static LayerMask Create(params int[] layerNumbers)
		{
			return LayerMaskExtensions.LayerNumbersToMask(layerNumbers);
		}

		public static LayerMask NamesToMask(params string[] layerNames)
		{
			LayerMask layerMask = 0;
			foreach (string layerName in layerNames)
			{
				layerMask |= 1 << LayerMask.NameToLayer(layerName);
			}
			return layerMask;
		}

		public static LayerMask LayerNumbersToMask(params int[] layerNumbers)
		{
			LayerMask layerMask = 0;
			foreach (int num in layerNumbers)
			{
				layerMask |= 1 << num;
			}
			return layerMask;
		}

		public static LayerMask Inverse(this LayerMask original)
		{
			return ~original;
		}

		public static LayerMask AddToMask(this LayerMask original, params string[] layerNames)
		{
			return original | LayerMaskExtensions.NamesToMask(layerNames);
		}

		public static LayerMask RemoveFromMask(this LayerMask original, params string[] layerNames)
		{
			LayerMask mask = ~original;
			return ~(mask | LayerMaskExtensions.NamesToMask(layerNames));
		}

		public static string[] MaskToNames(this LayerMask original)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < 32; i++)
			{
				int num = 1 << i;
				if ((original & num) == num)
				{
					string text = LayerMask.LayerToName(i);
					if (!string.IsNullOrEmpty(text))
					{
						list.Add(text);
					}
				}
			}
			return list.ToArray();
		}

		public static int[] MaskToNumbers(this LayerMask original)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < 32; i++)
			{
				int num = 1 << i;
				if ((original & num) == num)
				{
					list.Add(i);
				}
			}
			return list.ToArray();
		}

		public static string MaskToString(this LayerMask original)
		{
			return original.MaskToString(", ");
		}

		public static string MaskToString(this LayerMask original, string delimiter)
		{
			return string.Join(delimiter, original.MaskToNames());
		}
	}
}
