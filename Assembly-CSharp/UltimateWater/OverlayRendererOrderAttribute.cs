using System;

namespace UltimateWater
{
	public class OverlayRendererOrderAttribute : Attribute
	{
		public OverlayRendererOrderAttribute(int priority)
		{
			this._Priority = priority;
		}

		public int Priority
		{
			get
			{
				return this._Priority;
			}
		}

		private readonly int _Priority;
	}
}
