using System;
using Pathfinding.Util;

namespace Pathfinding
{
	public interface ITransformedGraph
	{
		GraphTransform transform { get; }
	}
}
