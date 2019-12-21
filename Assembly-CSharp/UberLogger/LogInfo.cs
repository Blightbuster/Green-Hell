using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UberLogger
{
	[Serializable]
	public class LogInfo
	{
		public string GetRelativeTimeStampAsString()
		{
			return this.RelativeTimeStampAsString;
		}

		public string GetAbsoluteTimeStampAsString()
		{
			return this.AbsoluteTimeStampAsString;
		}

		public LogInfo(UnityEngine.Object source, string channel, LogSeverity severity, List<LogStackFrame> callstack, LogStackFrame originatingSourceLocation, object message, params object[] par)
		{
			this.Source = source;
			this.Channel = channel;
			this.Severity = severity;
			this.Message = "";
			this.OriginatingSourceLocation = originatingSourceLocation;
			string text = message as string;
			if (text != null)
			{
				if (par.Length != 0)
				{
					this.Message = string.Format(text, par);
				}
				else
				{
					this.Message = text;
				}
			}
			else if (message != null)
			{
				this.Message = message.ToString();
			}
			this.Callstack = callstack;
			this.RelativeTimeStamp = UberLogger.Logger.GetRelativeTime();
			this.AbsoluteTimeStamp = DateTime.UtcNow;
			this.RelativeTimeStampAsString = string.Format("{0:0.0000}", this.RelativeTimeStamp);
			this.AbsoluteTimeStampAsString = this.AbsoluteTimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
		}

		public UnityEngine.Object Source;

		public string Channel;

		public LogSeverity Severity;

		public string Message;

		public List<LogStackFrame> Callstack;

		public LogStackFrame OriginatingSourceLocation;

		public double RelativeTimeStamp;

		private string RelativeTimeStampAsString;

		public DateTime AbsoluteTimeStamp;

		private string AbsoluteTimeStampAsString;
	}
}
