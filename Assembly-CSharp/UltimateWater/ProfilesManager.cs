using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	[Serializable]
	public class ProfilesManager
	{
		public Water.WeightedProfile[] Profiles { get; private set; }

		public Water.WaterEvent Changed
		{
			get
			{
				return this._Changed;
			}
		}

		public void CacheProfiles(params WaterProfileData[] profiles)
		{
			WindWaves windWaves = this._Water.WindWaves;
			if (windWaves != null)
			{
				for (int i = 0; i < profiles.Length; i++)
				{
					windWaves.SpectrumResolver.CacheSpectrum(profiles[i].Spectrum);
				}
			}
		}

		public float EvaluateProfilesParameter(Func<WaterProfileData, float> func)
		{
			float num = 0f;
			Water.WeightedProfile[] profiles = this.Profiles;
			for (int i = profiles.Length - 1; i >= 0; i--)
			{
				num += func(profiles[i].Profile) * profiles[i].Weight;
			}
			return num;
		}

		public void SetProfiles(params Water.WeightedProfile[] profiles)
		{
			for (int i = 0; i < profiles.Length; i++)
			{
				this.CacheProfiles(new WaterProfileData[]
				{
					profiles[i].Profile
				});
			}
			ProfilesManager.CheckProfiles(profiles);
			this.Profiles = profiles;
			this._ProfilesDirty = true;
		}

		public void ValidateProfiles()
		{
			bool flag = false;
			foreach (Water.WeightedProfile weightedProfile in this.Profiles)
			{
				flag |= weightedProfile.Profile.Dirty;
			}
			if (flag || this._ProfilesDirty)
			{
				this._ProfilesDirty = false;
				this._Changed.Invoke(this._Water);
			}
		}

		internal void Awake(Water water)
		{
			this._Water = water;
			if (this._Changed == null)
			{
				this._Changed = new Water.WaterEvent();
			}
			if (this.Profiles == null)
			{
				if (this._InitialProfile != null)
				{
					this.SetProfiles(new Water.WeightedProfile[]
					{
						new Water.WeightedProfile(this._InitialProfile, 1f)
					});
				}
				else
				{
					this.Profiles = new Water.WeightedProfile[0];
				}
			}
			WaterQualitySettings.Instance.Changed -= this.OnQualitySettingsChanged;
			WaterQualitySettings.Instance.Changed += this.OnQualitySettingsChanged;
		}

		internal void OnEnable()
		{
			this._ProfilesDirty = true;
		}

		internal void OnDisable()
		{
			this._ProfilesDirty = false;
		}

		internal void OnDestroy()
		{
			WaterQualitySettings.Instance.Changed -= this.OnQualitySettingsChanged;
		}

		internal void Update()
		{
			this.ValidateProfiles();
		}

		internal void OnValidate()
		{
			if (this.Profiles != null && this.Profiles.Length != 0 && (this._InitialProfileCopy == this._InitialProfile || this._InitialProfileCopy == null))
			{
				this._InitialProfileCopy = this._InitialProfile;
				this._ProfilesDirty = true;
			}
			else if (this._InitialProfile != null)
			{
				this._InitialProfileCopy = this._InitialProfile;
				this.Profiles = new Water.WeightedProfile[]
				{
					new Water.WeightedProfile(this._InitialProfile, 1f)
				};
				this._ProfilesDirty = true;
			}
		}

		private void OnQualitySettingsChanged()
		{
			this._ProfilesDirty = true;
		}

		private static void CheckProfiles(IList<Water.WeightedProfile> profiles)
		{
			if (profiles == null)
			{
				return;
			}
			if (profiles.Count == 0)
			{
				throw new ArgumentException("Water has to use at least one profile.");
			}
			float tileSize = profiles[0].Profile.TileSize;
			for (int i = 1; i < profiles.Count; i++)
			{
				if (profiles[i].Profile.TileSize != tileSize)
				{
					Debug.LogError("TileSize varies between used water profiles. It is the only parameter that you should keep equal on all profiles used at a time.");
					break;
				}
			}
		}

		[FormerlySerializedAs("initialProfile")]
		[SerializeField]
		private WaterProfile _InitialProfile;

		[FormerlySerializedAs("changed")]
		[SerializeField]
		private Water.WaterEvent _Changed = new Water.WaterEvent();

		private Water _Water;

		private bool _ProfilesDirty;

		private WaterProfile _InitialProfileCopy;
	}
}
