using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateWater
{
	[AddComponentMenu("Ultimate Water/Water Profile Blend")]
	public class WaterProfileBlend : MonoBehaviour
	{
		private void Awake()
		{
			if (this.Water == null)
			{
				this.Water = Utilities.GetWaterReference();
				if (this.Water.IsNullReference(this))
				{
					return;
				}
			}
			this._Profiles.RemoveAll((WaterProfile x) => x == null);
			if (this._Profiles.Count == 0)
			{
				Debug.LogError("[WaterProfileBlend] : no valid profiles found");
				base.enabled = false;
			}
		}

		private void Start()
		{
			this.Water.ProfilesManager.SetProfiles(WaterProfileBlend.CreateProfiles(this._Profiles, this._Weights));
		}

		private void OnValidate()
		{
			if (Application.isPlaying && this.Water != null && this.Water.WindWaves != null)
			{
				this.Water.ProfilesManager.SetProfiles(WaterProfileBlend.CreateProfiles(this._Profiles, this._Weights));
			}
			if (WaterProfileBlend.WeightSum(this._Weights) == 0f)
			{
				this._Weights[0] = 1f;
			}
		}

		private void Reset()
		{
			if (this.Water == null)
			{
				this.Water = Utilities.GetWaterReference();
			}
		}

		private static Water.WeightedProfile[] CreateProfiles(List<WaterProfile> profiles, List<float> weights)
		{
			List<float> list = WaterProfileBlend.NormalizeWeights(weights);
			int count = profiles.Count;
			Water.WeightedProfile[] array = new Water.WeightedProfile[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = new Water.WeightedProfile(profiles[i], list[i]);
			}
			return array;
		}

		private static float WeightSum(List<float> weights)
		{
			float num = 0f;
			for (int i = 0; i < weights.Count; i++)
			{
				num += weights[i];
			}
			return num;
		}

		private static List<float> NormalizeWeights(List<float> weights)
		{
			List<float> list = new List<float>(weights.Count);
			float num = WaterProfileBlend.WeightSum(weights);
			for (int i = 0; i < weights.Count; i++)
			{
				list.Add(weights[i] / num);
			}
			return list;
		}

		public Water Water;

		[SerializeField]
		private List<WaterProfile> _Profiles = new List<WaterProfile>
		{
			null
		};

		[SerializeField]
		private List<float> _Weights = new List<float>
		{
			1f
		};
	}
}
