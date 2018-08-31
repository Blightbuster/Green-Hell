using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	public class RichPath
	{
		public void Clear()
		{
			this.parts.Clear();
			this.currentPart = 0;
		}

		public void Initialize(Seeker s, Path p, bool mergePartEndpoints, bool simplificationMode)
		{
			if (p.error)
			{
				throw new ArgumentException("Path has an error");
			}
			List<GraphNode> path = p.path;
			if (path.Count == 0)
			{
				throw new ArgumentException("Path traverses no nodes");
			}
			this.seeker = s;
			for (int i = 0; i < this.parts.Count; i++)
			{
				RichFunnel richFunnel = this.parts[i] as RichFunnel;
				RichSpecial richSpecial = this.parts[i] as RichSpecial;
				if (richFunnel != null)
				{
					ObjectPool<RichFunnel>.Release(ref richFunnel);
				}
				else if (richSpecial != null)
				{
					ObjectPool<RichSpecial>.Release(ref richSpecial);
				}
			}
			this.parts.Clear();
			this.currentPart = 0;
			for (int j = 0; j < path.Count; j++)
			{
				if (path[j] is TriangleMeshNode)
				{
					NavGraph graph = AstarData.GetGraph(path[j]);
					RichFunnel richFunnel2 = ObjectPool<RichFunnel>.Claim().Initialize(this, graph);
					richFunnel2.funnelSimplification = simplificationMode;
					int num = j;
					uint graphIndex = path[num].GraphIndex;
					while (j < path.Count)
					{
						if (path[j].GraphIndex != graphIndex && !(path[j] is NodeLink3Node))
						{
							break;
						}
						j++;
					}
					j--;
					if (num == 0)
					{
						richFunnel2.exactStart = p.vectorPath[0];
					}
					else
					{
						richFunnel2.exactStart = (Vector3)path[(!mergePartEndpoints) ? num : (num - 1)].position;
					}
					if (j == path.Count - 1)
					{
						richFunnel2.exactEnd = p.vectorPath[p.vectorPath.Count - 1];
					}
					else
					{
						richFunnel2.exactEnd = (Vector3)path[(!mergePartEndpoints) ? j : (j + 1)].position;
					}
					richFunnel2.BuildFunnelCorridor(path, num, j);
					this.parts.Add(richFunnel2);
				}
				else if (NodeLink2.GetNodeLink(path[j]) != null)
				{
					NodeLink2 nodeLink = NodeLink2.GetNodeLink(path[j]);
					int num2 = j;
					uint graphIndex2 = path[num2].GraphIndex;
					for (j++; j < path.Count; j++)
					{
						if (path[j].GraphIndex != graphIndex2)
						{
							break;
						}
					}
					j--;
					if (j - num2 > 1)
					{
						throw new Exception("NodeLink2 path length greater than two (2) nodes. " + (j - num2));
					}
					if (j - num2 != 0)
					{
						RichSpecial item = ObjectPool<RichSpecial>.Claim().Initialize(nodeLink, path[num2]);
						this.parts.Add(item);
					}
				}
			}
		}

		public bool CompletedAllParts
		{
			get
			{
				return this.currentPart >= this.parts.Count;
			}
		}

		public bool IsLastPart
		{
			get
			{
				return this.currentPart >= this.parts.Count - 1;
			}
		}

		public void NextPart()
		{
			this.currentPart = Mathf.Min(this.currentPart + 1, this.parts.Count);
		}

		public RichPathPart GetCurrentPart()
		{
			if (this.parts.Count == 0)
			{
				return null;
			}
			return (this.currentPart >= this.parts.Count) ? this.parts[this.parts.Count - 1] : this.parts[this.currentPart];
		}

		private int currentPart;

		private readonly List<RichPathPart> parts = new List<RichPathPart>();

		public Seeker seeker;

		public ITransform transform;
	}
}
