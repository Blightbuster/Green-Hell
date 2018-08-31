﻿using System;
using UnityEngine;

namespace Pathfinding
{
	public class RichSpecial : RichPathPart
	{
		public override void OnEnterPool()
		{
			this.nodeLink = null;
		}

		public RichSpecial Initialize(NodeLink2 nodeLink, GraphNode first)
		{
			this.nodeLink = nodeLink;
			if (first == nodeLink.startNode)
			{
				this.first = nodeLink.StartTransform;
				this.second = nodeLink.EndTransform;
				this.reverse = false;
			}
			else
			{
				this.first = nodeLink.EndTransform;
				this.second = nodeLink.StartTransform;
				this.reverse = true;
			}
			return this;
		}

		public NodeLink2 nodeLink;

		public Transform first;

		public Transform second;

		public bool reverse;
	}
}
