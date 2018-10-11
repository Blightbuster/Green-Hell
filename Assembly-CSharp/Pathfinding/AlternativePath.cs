using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
	[AddComponentMenu("Pathfinding/Modifiers/Alternative Path")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_alternative_path.php")]
	[Serializable]
	public class AlternativePath : MonoModifier
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
			if (this == null)
			{
				return;
			}
			this.ApplyNow(p.path);
		}

		public override void OnDestroy()
		{
			this.destroyed = true;
			this.ClearOnDestroy();
			base.OnDestroy();
		}

		private void ClearOnDestroy()
		{
			this.InversePrevious();
		}

		private void InversePrevious()
		{
			if (this.prevNodes != null)
			{
				bool flag = false;
				for (int i = 0; i < this.prevNodes.Count; i++)
				{
					if ((ulong)this.prevNodes[i].Penalty < (ulong)((long)this.prevPenalty))
					{
						flag = true;
						this.prevNodes[i].Penalty = 0u;
					}
					else
					{
						this.prevNodes[i].Penalty = (uint)((ulong)this.prevNodes[i].Penalty - (ulong)((long)this.prevPenalty));
					}
				}
				if (flag)
				{
					Debug.LogWarning("Penalty for some nodes has been reset while the AlternativePath modifier was active (possibly because of a graph update). Some penalties might be incorrect (they may be lower than expected for the affected nodes)");
				}
			}
		}

		private void ApplyNow(List<GraphNode> nodes)
		{
			this.InversePrevious();
			this.prevNodes.Clear();
			if (this.destroyed)
			{
				return;
			}
			if (nodes != null)
			{
				int num = this.rnd.Next(this.randomStep);
				for (int i = num; i < nodes.Count; i += this.rnd.Next(1, this.randomStep))
				{
					nodes[i].Penalty = (uint)((ulong)nodes[i].Penalty + (ulong)((long)this.penalty));
					this.prevNodes.Add(nodes[i]);
				}
			}
			this.prevPenalty = this.penalty;
		}

		public int penalty = 1000;

		public int randomStep = 10;

		private List<GraphNode> prevNodes = new List<GraphNode>();

		private int prevPenalty;

		private readonly System.Random rnd = new System.Random();

		private bool destroyed;
	}
}
