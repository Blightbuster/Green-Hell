using System;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public class WeatherSystem : MonoBehaviour
	{
		private void Start()
		{
			this._SpectrumData = new WaterWavesSpectrumData(this._Water, this._Water.WindWaves, this._Profile.Data.Spectrum);
			this.LateUpdate();
			this._Water.WindWaves.SpectrumResolver.AddSpectrum(this._SpectrumData);
		}

		private void OnEnable()
		{
			if (this._SpectrumData != null && !this._Water.WindWaves.SpectrumResolver.ContainsSpectrum(this._SpectrumData))
			{
				this._Water.WindWaves.SpectrumResolver.AddSpectrum(this._SpectrumData);
			}
		}

		private void OnDisable()
		{
			this._Water.WindWaves.SpectrumResolver.RemoveSpectrum(this._SpectrumData);
		}

		private void LateUpdate()
		{
			Vector3 vector = this._Water.transform.InverseTransformPoint(base.transform.position);
			Vector2 vector2 = new Vector2(vector.x, vector.z);
			Vector3 forward = base.transform.forward;
			Vector2 vector3 = new Vector2(forward.x, forward.z);
			Vector2 normalized = vector3.normalized;
			if (normalized != this._LastWindDirection || vector2 != this._LastOffset || this._Radius != this._LastRadius || this._Weight != this._LastWeight)
			{
				this._SpectrumData.WindDirection = (this._LastWindDirection = normalized);
				this._SpectrumData.WeatherSystemOffset = (this._LastOffset = vector2);
				this._SpectrumData.WeatherSystemRadius = (this._LastRadius = this._Radius);
				this._SpectrumData.Weight = (this._LastWeight = this._Weight);
				this._Water.WindWaves.SpectrumResolver.SetDirectionalSpectrumDirty();
			}
		}

		[FormerlySerializedAs("water")]
		[SerializeField]
		private Water _Water;

		[FormerlySerializedAs("profile")]
		[SerializeField]
		private WaterProfile _Profile;

		[FormerlySerializedAs("radius")]
		[Tooltip("Describes how big the weather system is. Common values range from 10000 to 150000, assuming that the scene units are used as meters.")]
		[SerializeField]
		private float _Radius = 10000f;

		[FormerlySerializedAs("weight")]
		[Range(0f, 1f)]
		[SerializeField]
		private float _Weight = 1f;

		private WaterWavesSpectrumData _SpectrumData;

		private Vector2 _LastOffset;

		private Vector2 _LastWindDirection;

		private float _LastRadius;

		private float _LastWeight;
	}
}
