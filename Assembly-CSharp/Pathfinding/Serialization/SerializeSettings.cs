using System;

namespace Pathfinding.Serialization
{
	public class SerializeSettings
	{
		public static SerializeSettings Settings
		{
			get
			{
				return new SerializeSettings
				{
					nodes = false
				};
			}
		}

		public bool nodes = true;

		[Obsolete("There is no support for pretty printing the json anymore")]
		public bool prettyPrint;

		public bool editorSettings;
	}
}
