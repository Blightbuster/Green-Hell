using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public class WaterQualitySettings : ScriptableObjectSingleton
	{
		public WaterQualityLevel CurrentQualityLevel
		{
			get
			{
				return this._CurrentQualityLevel;
			}
		}

		public event Action Changed;

		public string[] Names
		{
			get
			{
				string[] array = new string[this._QualityLevels.Length];
				for (int i = 0; i < this._QualityLevels.Length; i++)
				{
					array[i] = this._QualityLevels[i].Name;
				}
				return array;
			}
		}

		public WaterWavesMode WavesMode
		{
			get
			{
				return this._CurrentQualityLevel.WavesMode;
			}
			set
			{
				if (this._CurrentQualityLevel.WavesMode == value)
				{
					return;
				}
				this._CurrentQualityLevel.WavesMode = value;
				this.OnChange();
			}
		}

		public int MaxSpectrumResolution
		{
			get
			{
				return this._CurrentQualityLevel.MaxSpectrumResolution;
			}
			set
			{
				if (this._CurrentQualityLevel.MaxSpectrumResolution == value)
				{
					return;
				}
				this._CurrentQualityLevel.MaxSpectrumResolution = value;
				this.OnChange();
			}
		}

		public float TileSizeScale
		{
			get
			{
				return this._CurrentQualityLevel.TileSizeScale;
			}
			set
			{
				if (this._CurrentQualityLevel.TileSizeScale == value)
				{
					return;
				}
				this._CurrentQualityLevel.TileSizeScale = value;
				this.OnChange();
			}
		}

		public bool AllowHighPrecisionTextures
		{
			get
			{
				return this._CurrentQualityLevel.AllowHighPrecisionTextures;
			}
			set
			{
				if (this._CurrentQualityLevel.AllowHighPrecisionTextures == value)
				{
					return;
				}
				this._CurrentQualityLevel.AllowHighPrecisionTextures = value;
				this.OnChange();
			}
		}

		public bool AllowHighQualityNormalMaps
		{
			get
			{
				return this._CurrentQualityLevel.AllowHighQualityNormalMaps;
			}
			set
			{
				if (this._CurrentQualityLevel.AllowHighQualityNormalMaps == value)
				{
					return;
				}
				this._CurrentQualityLevel.AllowHighQualityNormalMaps = value;
				this.OnChange();
			}
		}

		public float MaxTesselationFactor
		{
			get
			{
				return this._CurrentQualityLevel.MaxTesselationFactor;
			}
			set
			{
				if (this._CurrentQualityLevel.MaxTesselationFactor == value)
				{
					return;
				}
				this._CurrentQualityLevel.MaxTesselationFactor = value;
				this.OnChange();
			}
		}

		public int MaxVertexCount
		{
			get
			{
				return this._CurrentQualityLevel.MaxVertexCount;
			}
			set
			{
				if (this._CurrentQualityLevel.MaxVertexCount == value)
				{
					return;
				}
				this._CurrentQualityLevel.MaxVertexCount = value;
				this.OnChange();
			}
		}

		public int MaxTesselatedVertexCount
		{
			get
			{
				return this._CurrentQualityLevel.MaxTesselatedVertexCount;
			}
			set
			{
				if (this._CurrentQualityLevel.MaxTesselatedVertexCount == value)
				{
					return;
				}
				this._CurrentQualityLevel.MaxTesselatedVertexCount = value;
				this.OnChange();
			}
		}

		public bool AllowAlphaBlending
		{
			get
			{
				return this._CurrentQualityLevel.AllowAlphaBlending;
			}
			set
			{
				if (this._CurrentQualityLevel.AllowAlphaBlending == value)
				{
					return;
				}
				this._CurrentQualityLevel.AllowAlphaBlending = value;
				this.OnChange();
			}
		}

		public static WaterQualitySettings Instance
		{
			get
			{
				if (WaterQualitySettings._Instance == null || WaterQualitySettings._Instance == null)
				{
					WaterQualitySettings._Instance = ScriptableObjectSingleton.LoadSingleton<WaterQualitySettings>();
					WaterQualitySettings._Instance.Changed = null;
					WaterQualitySettings._Instance._WaterQualityIndex = -1;
					WaterQualitySettings._Instance.SynchronizeQualityLevel();
				}
				return WaterQualitySettings._Instance;
			}
		}

		public bool SynchronizeWithUnity
		{
			get
			{
				return this._SynchronizeWithUnity;
			}
		}

		public int GetQualityLevel()
		{
			return this._WaterQualityIndex;
		}

		public void SetQualityLevel(int index)
		{
			if (!Application.isPlaying)
			{
				this._SavedCustomQualityLevel = index;
			}
			this._CurrentQualityLevel = this._QualityLevels[index];
			this._WaterQualityIndex = index;
			this.OnChange();
		}

		public void SynchronizeQualityLevel()
		{
			int num = -1;
			if (this._SynchronizeWithUnity)
			{
				num = this.FindQualityLevel(QualitySettings.names[QualitySettings.GetQualityLevel()]);
			}
			if (num == -1)
			{
				num = this._SavedCustomQualityLevel;
			}
			num = Mathf.Clamp(num, 0, this._QualityLevels.Length - 1);
			if (num != this._WaterQualityIndex)
			{
				this.SetQualityLevel(num);
			}
		}

		internal WaterQualityLevel[] GetQualityLevelsDirect()
		{
			return this._QualityLevels;
		}

		private void OnChange()
		{
			if (this.Changed != null)
			{
				this.Changed();
			}
		}

		private int FindQualityLevel(string levelName)
		{
			for (int i = 0; i < this._QualityLevels.Length; i++)
			{
				if (this._QualityLevels[i].Name == levelName)
				{
					return i;
				}
			}
			return -1;
		}

		private void SynchronizeLevelNames()
		{
		}

		public WaterRipplesData Ripples;

		[FormerlySerializedAs("qualityLevels")]
		[SerializeField]
		private WaterQualityLevel[] _QualityLevels;

		[SerializeField]
		[FormerlySerializedAs("synchronizeWithUnity")]
		private bool _SynchronizeWithUnity = true;

		[FormerlySerializedAs("savedCustomQualityLevel")]
		[SerializeField]
		private int _SavedCustomQualityLevel;

		private int _WaterQualityIndex;

		private WaterQualityLevel _CurrentQualityLevel;

		private static WaterQualitySettings _Instance;
	}
}
