using System;
using UnityEngine;

namespace Pathfinding
{
	public struct NNInfo
	{
		[Obsolete("This field has been renamed to 'position'")]
		public Vector3 clampedPosition
		{
			get
			{
				return this.position;
			}
		}

		public NNInfo(NNInfoInternal internalInfo)
		{
			this.node = internalInfo.node;
			this.position = internalInfo.clampedPosition;
		}

		public static explicit operator Vector3(NNInfo ob)
		{
			return ob.position;
		}

		public static explicit operator GraphNode(NNInfo ob)
		{
			return ob.node;
		}

		public readonly GraphNode node;

		public readonly Vector3 position;
	}
}
