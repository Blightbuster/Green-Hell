using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UltimateWater.Internal
{
	public class VersionCompatibility
	{
		static VersionCompatibility()
		{
			VersionCompatibility.CalculateVersion();
			VersionCompatibility.CheckVersion();
		}

		public static int Version
		{
			get
			{
				if (VersionCompatibility._Version == -1)
				{
					VersionCompatibility._Version = VersionCompatibility.CalculateVersion();
				}
				return VersionCompatibility._Version;
			}
		}

		private static int CalculateVersion()
		{
			string text = Application.unityVersion;
			text = Regex.Replace(text, "[^0-9.]+[0-9]*", string.Empty);
			string[] array = text.Split(new char[]
			{
				'.'
			});
			int num = int.Parse(array[0]);
			int num2 = int.Parse(array[1]);
			int num3 = int.Parse(array[2]);
			return num * 100 + num2 * 10 + num3;
		}

		private static void CheckVersion()
		{
		}

		private static bool IsSupported(string version)
		{
			foreach (string text in VersionCompatibility._Unsupported)
			{
				if (text.Contains(version))
				{
					return false;
				}
			}
			return true;
		}

		private static bool IsBuggy(string version)
		{
			foreach (string text in VersionCompatibility._Bugged)
			{
				if (text.Contains(version))
				{
					return true;
				}
			}
			return false;
		}

		private static string BugInfo(string version)
		{
			for (int i = 0; i < VersionCompatibility._Bugged.Length; i++)
			{
				if (VersionCompatibility._Bugged[i].Contains(version))
				{
					return VersionCompatibility._BugInfo[i];
				}
			}
			return string.Empty;
		}

		private static int _Version = -1;

		private static readonly string[] _Unsupported = new string[]
		{
			"5.0.0",
			"5.0.1",
			"5.0.2",
			"5.0.3",
			"5.0.4",
			"5.1.0",
			"5.1.1",
			"5.1.2",
			"5.1.3",
			"5.1.4",
			"5.1.5",
			"5.2.0",
			"5.2.1",
			"5.2.2",
			"5.2.3",
			"5.2.4",
			"5.2.5",
			"5.3.0",
			"5.3.1",
			"5.3.2",
			"5.3.3",
			"5.3.4",
			"5.3.5",
			"5.4.0",
			"5.4.1",
			"5.4.2",
			"5.4.4"
		};

		private static readonly string[] _Bugged = new string[]
		{
			"5.6.0",
			"5.6.1"
		};

		private static readonly string[] _BugInfo = new string[]
		{
			"This Unity version introduces bugs in depth computations, please use earlier (5.5.x-) or later (5.6.2+) versions",
			"This Unity version introduces bugs in depth computations, please use earlier (5.5.x-) or later (5.6.2+) versions"
		};
	}
}
