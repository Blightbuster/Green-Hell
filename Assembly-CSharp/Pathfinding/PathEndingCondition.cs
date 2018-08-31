using System;

namespace Pathfinding
{
	public abstract class PathEndingCondition
	{
		protected PathEndingCondition()
		{
		}

		public PathEndingCondition(Path p)
		{
			if (p == null)
			{
				throw new ArgumentNullException("p");
			}
			this.path = p;
		}

		public abstract bool TargetFound(PathNode node);

		protected Path path;
	}
}
