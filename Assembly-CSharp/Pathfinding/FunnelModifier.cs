using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	[AddComponentMenu("Pathfinding/Modifiers/Funnel")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_funnel_modifier.php")]
	[Serializable]
	public class FunnelModifier : MonoModifier
	{
		public override int Order
		{
			get
			{
				return 10;
			}
		}

		public override void Apply(Path p)
		{
			if (p.path == null || p.path.Count == 0 || p.vectorPath == null || p.vectorPath.Count == 0)
			{
				return;
			}
			List<Vector3> list = ListPool<Vector3>.Claim();
			List<Funnel.PathPart> list2 = Funnel.SplitIntoParts(p);
			for (int i = 0; i < list2.Count; i++)
			{
				Funnel.PathPart pathPart = list2[i];
				if (!pathPart.isLink)
				{
					List<Vector3> list3 = Funnel.Calculate(Funnel.ConstructFunnelPortals(p.path, pathPart), this.unwrap, this.splitAtEveryPortal);
					list.AddRange(list3);
					ListPool<Vector3>.Release(list3);
				}
			}
			ListPool<Funnel.PathPart>.Release(list2);
			ListPool<Vector3>.Release(p.vectorPath);
			p.vectorPath = list;
		}

		public bool unwrap = true;

		public bool splitAtEveryPortal;
	}
}
