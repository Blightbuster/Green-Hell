using System;
using UnityEngine;

namespace RootMotion
{
	public class LargeHeader : PropertyAttribute
	{
		public LargeHeader(string name)
		{
			this.name = name;
			this.color = "white";
		}

		public LargeHeader(string name, string color)
		{
			this.name = name;
			this.color = color;
		}

		public string name;

		public string color = "white";
	}
}
