using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public interface IPoint2D
	{
		Vector2 Position { get; }

		void Destroy();
	}
}
