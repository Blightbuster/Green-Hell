using System;

namespace Pathfinding
{
	public interface IPathModifier
	{
		int Order { get; }

		void Apply(Path p);

		void PreProcess(Path p);
	}
}
