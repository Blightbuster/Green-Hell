using System;

namespace Pathfinding
{
	public struct Connection
	{
		public override int GetHashCode()
		{
			return this.node.GetHashCode() ^ (int)this.cost;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			Connection connection = (Connection)obj;
			return connection.node == this.node && connection.cost == this.cost;
		}

		public GraphNode node;

		public uint cost;
	}
}
