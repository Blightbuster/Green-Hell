﻿using System;
using UnityEngine;

namespace Pathfinding
{
	public class FloodPathTracer : ABPath
	{
		protected override bool hasEndPoint
		{
			get
			{
				return false;
			}
		}

		public static FloodPathTracer Construct(Vector3 start, FloodPath flood, OnPathDelegate callback = null)
		{
			FloodPathTracer path = PathPool.GetPath<FloodPathTracer>();
			path.Setup(start, flood, callback);
			return path;
		}

		protected void Setup(Vector3 start, FloodPath flood, OnPathDelegate callback)
		{
			this.flood = flood;
			if (flood == null || flood.PipelineState < PathState.Returned)
			{
				throw new ArgumentException("You must supply a calculated FloodPath to the 'flood' argument");
			}
			base.Setup(start, flood.originalStartPoint, callback);
			this.nnConstraint = new FloodPathConstraint(flood);
		}

		protected override void Reset()
		{
			base.Reset();
			this.flood = null;
		}

		protected override void Initialize()
		{
			if (this.startNode != null && this.flood.HasPathTo(this.startNode))
			{
				this.Trace(this.startNode);
				base.CompleteState = PathCompleteState.Complete;
			}
			else
			{
				base.Error();
				base.LogError("Could not find valid start node");
			}
		}

		protected override void CalculateStep(long targetTick)
		{
			if (!base.IsDone())
			{
				base.Error();
				base.LogError("Something went wrong. At this point the path should be completed");
			}
		}

		public void Trace(GraphNode from)
		{
			GraphNode graphNode = from;
			int num = 0;
			while (graphNode != null)
			{
				this.path.Add(graphNode);
				this.vectorPath.Add((Vector3)graphNode.position);
				graphNode = this.flood.GetParent(graphNode);
				num++;
				if (num > 1024)
				{
					Debug.LogWarning("Inifinity loop? >1024 node path. Remove this message if you really have that long paths (FloodPathTracer.cs, Trace function)");
					break;
				}
			}
		}

		protected FloodPath flood;
	}
}
