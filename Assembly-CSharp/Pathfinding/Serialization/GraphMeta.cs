using System;
using System.Collections.Generic;
using Pathfinding.WindowsStore;

namespace Pathfinding.Serialization
{
	public class GraphMeta
	{
		public Type GetGraphType(int i)
		{
			if (string.IsNullOrEmpty(this.typeNames[i]))
			{
				return null;
			}
			Type type = WindowsStoreCompatibility.GetTypeInfo(typeof(AstarPath)).Assembly.GetType(this.typeNames[i]);
			if (!object.Equals(type, null))
			{
				return type;
			}
			throw new Exception("No graph of type '" + this.typeNames[i] + "' could be created, type does not exist");
		}

		public Version version;

		public int graphs;

		public List<string> guids;

		public List<string> typeNames;
	}
}
