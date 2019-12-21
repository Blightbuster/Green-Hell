using System;
using System.Collections.Generic;

namespace Enums
{
	public static class ItemIDHelpers
	{
		public static void Initialize()
		{
			if (ItemIDHelpers.s_Initialized)
			{
				return;
			}
			foreach (object obj in Enum.GetValues(typeof(ItemID)))
			{
				ItemID item = (ItemID)obj;
				string text = item.ToString().ToLower();
				if (text.Contains("plant") && !text.Contains("plantain"))
				{
					ItemIDHelpers.s_Plants.Add(item);
				}
				if (text.Contains("tree"))
				{
					ItemIDHelpers.s_Tree.Add(item);
				}
				if (text.Contains("leaf"))
				{
					ItemIDHelpers.s_Leaf.Add(item);
				}
				if (text.EndsWith("_fallen"))
				{
					ItemIDHelpers.s_Fallen.Add(item);
				}
				if (text.Contains("questitem"))
				{
					ItemIDHelpers.s_QuestItem.Add(item);
				}
			}
			ItemIDHelpers.s_Initialized = true;
		}

		public static bool IsPlant(this ItemID type)
		{
			ItemIDHelpers.Initialize();
			return ItemIDHelpers.s_Plants.Contains(type);
		}

		public static bool IsTree(this ItemID type)
		{
			ItemIDHelpers.Initialize();
			return ItemIDHelpers.s_Tree.Contains(type);
		}

		public static bool IsLeaf(this ItemID type)
		{
			ItemIDHelpers.Initialize();
			return ItemIDHelpers.s_Leaf.Contains(type);
		}

		public static bool IsFallen(this ItemID type)
		{
			ItemIDHelpers.Initialize();
			return ItemIDHelpers.s_Fallen.Contains(type);
		}

		public static bool IsQuestItem(this ItemID type)
		{
			ItemIDHelpers.Initialize();
			return ItemIDHelpers.s_QuestItem.Contains(type);
		}

		private static HashSet<ItemID> s_Plants = new HashSet<ItemID>();

		private static HashSet<ItemID> s_Tree = new HashSet<ItemID>();

		private static HashSet<ItemID> s_Leaf = new HashSet<ItemID>();

		private static HashSet<ItemID> s_Fallen = new HashSet<ItemID>();

		private static HashSet<ItemID> s_QuestItem = new HashSet<ItemID>();

		private static bool s_Initialized = false;
	}
}
