using System;

namespace UberLogger
{
	[AttributeUsage(AttributeTargets.Method)]
	public class StackTraceIgnore : Attribute
	{
	}
}
