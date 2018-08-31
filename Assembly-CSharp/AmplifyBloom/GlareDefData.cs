using System;
using UnityEngine;

namespace AmplifyBloom
{
	[Serializable]
	public class GlareDefData
	{
		public GlareDefData()
		{
			this.m_customStarData = new StarDefData();
		}

		public GlareDefData(StarLibType starType, float starInclination, float chromaticAberration)
		{
			this.m_starType = starType;
			this.m_starInclination = starInclination;
			this.m_chromaticAberration = chromaticAberration;
		}

		public StarLibType StarType
		{
			get
			{
				return this.m_starType;
			}
			set
			{
				this.m_starType = value;
			}
		}

		public float StarInclination
		{
			get
			{
				return this.m_starInclination;
			}
			set
			{
				this.m_starInclination = value;
			}
		}

		public float StarInclinationDeg
		{
			get
			{
				return this.m_starInclination * 57.29578f;
			}
			set
			{
				this.m_starInclination = value * 0.0174532924f;
			}
		}

		public float ChromaticAberration
		{
			get
			{
				return this.m_chromaticAberration;
			}
			set
			{
				this.m_chromaticAberration = value;
			}
		}

		public StarDefData CustomStarData
		{
			get
			{
				return this.m_customStarData;
			}
			set
			{
				this.m_customStarData = value;
			}
		}

		public bool FoldoutValue = true;

		[SerializeField]
		private StarLibType m_starType;

		[SerializeField]
		private float m_starInclination;

		[SerializeField]
		private float m_chromaticAberration;

		[SerializeField]
		private StarDefData m_customStarData;
	}
}
