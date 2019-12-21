using System;
using System.Diagnostics;
using System.Reflection;

namespace UberLogger
{
	[Serializable]
	public class LogStackFrame
	{
		public LogStackFrame(StackFrame frame)
		{
			MethodBase method = frame.GetMethod();
			this.MethodName = method.Name;
			this.DeclaringType = method.DeclaringType.FullName;
			ParameterInfo[] parameters = method.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				this.ParameterSig += string.Format("{0} {1}", parameters[i].ParameterType, parameters[i].Name);
				if (i + 1 < parameters.Length)
				{
					this.ParameterSig += ", ";
				}
			}
			this.FileName = frame.GetFileName();
			this.LineNumber = frame.GetFileLineNumber();
			this.MakeFormattedNames();
		}

		public LogStackFrame(string unityStackFrame)
		{
			if (Logger.ExtractInfoFromUnityStackInfo(unityStackFrame, ref this.DeclaringType, ref this.MethodName, ref this.FileName, ref this.LineNumber))
			{
				this.MakeFormattedNames();
				return;
			}
			this.FormattedMethodNameWithFileName = unityStackFrame;
			this.FormattedMethodName = unityStackFrame;
			this.FormattedFileName = unityStackFrame;
		}

		public LogStackFrame(string message, string filename, int lineNumber)
		{
			this.FileName = filename;
			this.LineNumber = lineNumber;
			this.FormattedMethodNameWithFileName = message;
			this.FormattedMethodName = message;
			this.FormattedFileName = message;
		}

		public string GetFormattedMethodNameWithFileName()
		{
			return this.FormattedMethodNameWithFileName;
		}

		public string GetFormattedMethodName()
		{
			return this.FormattedMethodName;
		}

		public string GetFormattedFileName()
		{
			return this.FormattedFileName;
		}

		private void MakeFormattedNames()
		{
			this.FormattedMethodName = string.Format("{0}.{1}({2})", this.DeclaringType, this.MethodName, this.ParameterSig);
			string arg = this.FileName;
			if (!string.IsNullOrEmpty(this.FileName))
			{
				int num = this.FileName.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
				if (num > 0)
				{
					arg = this.FileName.Substring(num);
				}
			}
			this.FormattedFileName = string.Format("{0}:{1}", arg, this.LineNumber);
			this.FormattedMethodNameWithFileName = string.Format("{0} (at {1})", this.FormattedMethodName, this.FormattedFileName);
		}

		public string MethodName;

		public string DeclaringType;

		public string ParameterSig;

		public int LineNumber;

		public string FileName;

		private string FormattedMethodNameWithFileName;

		private string FormattedMethodName;

		private string FormattedFileName;
	}
}
