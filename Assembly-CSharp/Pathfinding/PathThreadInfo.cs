using System;

namespace Pathfinding
{
	public struct PathThreadInfo
	{
		public PathThreadInfo(int index, AstarPath astar, PathHandler runData)
		{
			this.threadIndex = index;
			this.astar = astar;
			this.runData = runData;
		}

		public readonly int threadIndex;

		public readonly AstarPath astar;

		public readonly PathHandler runData;
	}
}
