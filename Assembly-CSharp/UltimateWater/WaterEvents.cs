using System;

namespace UltimateWater
{
	public static class WaterEvents
	{
		public static void AddListener(Action action, WaterEvents.GlobalEventType type)
		{
			if (type == WaterEvents.GlobalEventType.OnQualityChanged)
			{
				WaterQualitySettings instance = WaterQualitySettings.Instance;
				if (instance == null)
				{
					return;
				}
				WaterQualitySettings.Instance.Changed -= action;
				WaterQualitySettings.Instance.Changed += action;
			}
		}

		public static void RemoveListener(Action action, WaterEvents.GlobalEventType type)
		{
			if (type == WaterEvents.GlobalEventType.OnQualityChanged)
			{
				WaterQualitySettings instance = WaterQualitySettings.Instance;
				if (instance == null)
				{
					return;
				}
				WaterQualitySettings.Instance.Changed -= action;
			}
		}

		public enum GlobalEventType
		{
			OnQualityChanged
		}
	}
}
