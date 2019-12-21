﻿using System;
using UnityEngine;

namespace AmplifyMotion
{
	[Serializable]
	public class VersionInfo
	{
		public static string StaticToString()
		{
			return string.Format("{0}.{1}.{2}", 1, 8, 2) + VersionInfo.StageSuffix + VersionInfo.TrialSuffix;
		}

		public override string ToString()
		{
			return string.Format("{0}.{1}.{2}", this.m_major, this.m_minor, this.m_release) + VersionInfo.StageSuffix + VersionInfo.TrialSuffix;
		}

		public int Number
		{
			get
			{
				return this.m_major * 100 + this.m_minor * 10 + this.m_release;
			}
		}

		private VersionInfo()
		{
			this.m_major = 1;
			this.m_minor = 8;
			this.m_release = 2;
		}

		private VersionInfo(byte major, byte minor, byte release)
		{
			this.m_major = (int)major;
			this.m_minor = (int)minor;
			this.m_release = (int)release;
		}

		public static VersionInfo Current()
		{
			return new VersionInfo(1, 8, 2);
		}

		public static bool Matches(VersionInfo version)
		{
			return 1 == version.m_major && 8 == version.m_minor && 2 == version.m_release;
		}

		public const byte Major = 1;

		public const byte Minor = 8;

		public const byte Release = 2;

		private static string StageSuffix = "_dev002";

		private static string TrialSuffix = "";

		[SerializeField]
		private int m_major;

		[SerializeField]
		private int m_minor;

		[SerializeField]
		private int m_release;
	}
}
