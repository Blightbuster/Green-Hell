using System;
using UnityEngine;

namespace AmplifyBloom
{
	[Serializable]
	public class StarDefData
	{
		public StarDefData()
		{
		}

		public StarDefData(StarLibType starType, string starName, int starLinesCount, int passCount, float sampleLength, float attenuation, float inclination, float rotation, float longAttenuation = 0f, float customIncrement = -1f)
		{
			this.m_starType = starType;
			this.m_starName = starName;
			this.m_passCount = passCount;
			this.m_sampleLength = sampleLength;
			this.m_attenuation = attenuation;
			this.m_starlinesCount = starLinesCount;
			this.m_inclination = inclination;
			this.m_rotation = rotation;
			this.m_customIncrement = customIncrement;
			this.m_longAttenuation = longAttenuation;
			this.CalculateStarData();
		}

		public void Destroy()
		{
			this.m_starLinesArr = null;
		}

		public void CalculateStarData()
		{
			if (this.m_starlinesCount == 0)
			{
				return;
			}
			this.m_starLinesArr = new StarLineData[this.m_starlinesCount];
			float num = (this.m_customIncrement <= 0f) ? (180f / (float)this.m_starlinesCount) : this.m_customIncrement;
			num *= 0.0174532924f;
			for (int i = 0; i < this.m_starlinesCount; i++)
			{
				this.m_starLinesArr[i] = new StarLineData();
				this.m_starLinesArr[i].PassCount = this.m_passCount;
				this.m_starLinesArr[i].SampleLength = this.m_sampleLength;
				if (this.m_longAttenuation > 0f)
				{
					this.m_starLinesArr[i].Attenuation = ((i % 2 != 0) ? this.m_attenuation : this.m_longAttenuation);
				}
				else
				{
					this.m_starLinesArr[i].Attenuation = this.m_attenuation;
				}
				this.m_starLinesArr[i].Inclination = num * (float)i;
			}
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

		public string StarName
		{
			get
			{
				return this.m_starName;
			}
			set
			{
				this.m_starName = value;
			}
		}

		public int StarlinesCount
		{
			get
			{
				return this.m_starlinesCount;
			}
			set
			{
				this.m_starlinesCount = value;
				this.CalculateStarData();
			}
		}

		public int PassCount
		{
			get
			{
				return this.m_passCount;
			}
			set
			{
				this.m_passCount = value;
				this.CalculateStarData();
			}
		}

		public float SampleLength
		{
			get
			{
				return this.m_sampleLength;
			}
			set
			{
				this.m_sampleLength = value;
				this.CalculateStarData();
			}
		}

		public float Attenuation
		{
			get
			{
				return this.m_attenuation;
			}
			set
			{
				this.m_attenuation = value;
				this.CalculateStarData();
			}
		}

		public float Inclination
		{
			get
			{
				return this.m_inclination;
			}
			set
			{
				this.m_inclination = value;
				this.CalculateStarData();
			}
		}

		public float CameraRotInfluence
		{
			get
			{
				return this.m_rotation;
			}
			set
			{
				this.m_rotation = value;
			}
		}

		public StarLineData[] StarLinesArr
		{
			get
			{
				return this.m_starLinesArr;
			}
		}

		public float CustomIncrement
		{
			get
			{
				return this.m_customIncrement;
			}
			set
			{
				this.m_customIncrement = value;
				this.CalculateStarData();
			}
		}

		public float LongAttenuation
		{
			get
			{
				return this.m_longAttenuation;
			}
			set
			{
				this.m_longAttenuation = value;
				this.CalculateStarData();
			}
		}

		[SerializeField]
		private StarLibType m_starType;

		[SerializeField]
		private string m_starName = string.Empty;

		[SerializeField]
		private int m_starlinesCount = 2;

		[SerializeField]
		private int m_passCount = 4;

		[SerializeField]
		private float m_sampleLength = 1f;

		[SerializeField]
		private float m_attenuation = 0.85f;

		[SerializeField]
		private float m_inclination;

		[SerializeField]
		private float m_rotation;

		[SerializeField]
		private StarLineData[] m_starLinesArr;

		[SerializeField]
		private float m_customIncrement = 90f;

		[SerializeField]
		private float m_longAttenuation;
	}
}
