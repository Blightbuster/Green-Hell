using System;
using UnityEngine;

namespace UltimateWater
{
	[AddComponentMenu("Ultimate Water/Water Profile Blend (simple)")]
	public class WaterProfileBlendSimple : MonoBehaviour
	{
		public Water Water
		{
			get
			{
				return this._Water;
			}
			set
			{
				this._Water = value;
				this.UpdateProfiles();
			}
		}

		public WaterProfile First
		{
			get
			{
				return this._First;
			}
			set
			{
				this._First = value;
				this.UpdateProfiles();
			}
		}

		public WaterProfile Second
		{
			get
			{
				return this._Second;
			}
			set
			{
				this._Second = value;
				this.UpdateProfiles();
			}
		}

		public float Factor
		{
			get
			{
				return this._Factor;
			}
			set
			{
				this._Factor = Mathf.Clamp01(value);
				this.UpdateProfiles();
			}
		}

		private void Awake()
		{
			if (this.Water == null)
			{
				this.Water = Utilities.GetWaterReference();
			}
		}

		private void Start()
		{
			this.UpdateProfiles();
		}

		private void OnValidate()
		{
			if (!Application.isPlaying || this.Water == null || this.Water.WindWaves == null)
			{
				return;
			}
			this.UpdateProfiles();
		}

		private void Reset()
		{
			if (this.Water == null)
			{
				this.Water = Utilities.GetWaterReference();
			}
		}

		private void UpdateProfiles()
		{
			if (this.Water == null || this.First == null || this.Second == null)
			{
				return;
			}
			this._Profiles[0] = new Water.WeightedProfile(this.First, 1f - this.Factor);
			this._Profiles[1] = new Water.WeightedProfile(this.Second, this.Factor);
			this.Water.ProfilesManager.SetProfiles(this._Profiles);
		}

		[SerializeField]
		private Water _Water;

		[SerializeField]
		[Header("Profiles")]
		private WaterProfile _First;

		[SerializeField]
		private WaterProfile _Second;

		[Range(0f, 1f)]
		[Header("Blend:")]
		[SerializeField]
		private float _Factor;

		private readonly Water.WeightedProfile[] _Profiles = new Water.WeightedProfile[2];
	}
}
