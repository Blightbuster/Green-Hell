using System;

namespace UltimateWater.Internal
{
	public abstract class WaterModule
	{
		internal virtual void Start(Water water)
		{
		}

		internal virtual void Enable()
		{
		}

		internal virtual void Disable()
		{
		}

		internal virtual void Destroy()
		{
		}

		internal virtual void Validate()
		{
		}

		internal virtual void Update()
		{
		}

		internal virtual void OnWaterRender(WaterCamera camera)
		{
		}

		internal virtual void OnWaterPostRender(WaterCamera camera)
		{
		}
	}
}
