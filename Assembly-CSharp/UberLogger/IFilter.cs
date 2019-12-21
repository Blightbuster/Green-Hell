using System;
using UnityEngine;

namespace UberLogger
{
	public interface IFilter
	{
		bool ApplyFilter(string channel, UnityEngine.Object source, LogSeverity severity, object message, params object[] par);
	}
}
