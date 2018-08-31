using System;
using UnityEngine;

namespace UltimateWater.Utils
{
	[AttributeUsage(AttributeTargets.Field)]
	public class InspectorWarningAttribute : PropertyAttribute
	{
		public InspectorWarningAttribute(string methodName, InspectorWarningAttribute.InfoType type)
		{
			this.MethodName = methodName;
			this.Type = type;
		}

		public readonly string MethodName;

		public readonly InspectorWarningAttribute.InfoType Type;

		public enum InfoType
		{
			Note,
			Warning,
			Error
		}
	}
}
