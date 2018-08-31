using System;
using UnityEngine;

namespace AmplifyBloom
{
	[Serializable]
	public class VersionInfo
	{
		private VersionInfo()
		{
			this.m_major = 1;
			this.m_minor = 1;
			this.m_release = 1;
		}

		private VersionInfo(byte major, byte minor, byte release)
		{
			this.m_major = (int)major;
			this.m_minor = (int)minor;
			this.m_release = (int)release;
		}

		public static string StaticToString()
		{
			return string.Format("{0}.{1}.{2}", 1, 1, 1) + VersionInfo.StageSuffix;
		}

		public override string ToString()
		{
			return string.Format("{0}.{1}.{2}", this.m_major, this.m_minor, this.m_release) + VersionInfo.StageSuffix;
		}

		public int Number
		{
			get
			{
				return this.m_major * 100 + this.m_minor * 10 + this.m_release;
			}
		}

		public static VersionInfo Current()
		{
			return new VersionInfo(1, 1, 1);
		}

		public static bool Matches(VersionInfo version)
		{
			return version.m_major == 1 && version.m_minor == 1 && 1 == version.m_release;
		}

		public const byte Major = 1;

		public const byte Minor = 1;

		public const byte Release = 1;

		private static string StageSuffix = "_dev001";

		[SerializeField]
		private int m_major;

		[SerializeField]
		private int m_minor;

		[SerializeField]
		private int m_release;
	}
}
