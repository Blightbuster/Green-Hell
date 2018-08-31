using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	[Serializable]
	public struct NormalMapAnimation
	{
		public NormalMapAnimation(float speed, float deviation, float intensity, Vector2 tiling)
		{
			this._Speed = speed;
			this._Deviation = deviation;
			this._Intensity = intensity;
			this._Tiling = tiling;
		}

		public float Speed
		{
			get
			{
				return this._Speed;
			}
		}

		public float Deviation
		{
			get
			{
				return this._Deviation;
			}
		}

		public float Intensity
		{
			get
			{
				return this._Intensity;
			}
		}

		public Vector2 Tiling
		{
			get
			{
				return this._Tiling;
			}
		}

		public static NormalMapAnimation operator *(NormalMapAnimation a, float w)
		{
			return new NormalMapAnimation(a._Speed * w, a._Deviation * w, a._Intensity * w, a._Tiling * w);
		}

		public static NormalMapAnimation operator +(NormalMapAnimation a, NormalMapAnimation b)
		{
			return new NormalMapAnimation(a._Speed + b._Speed, a._Deviation + b._Deviation, a._Intensity + b._Intensity, a._Tiling + b._Tiling);
		}

		[SerializeField]
		[FormerlySerializedAs("speed")]
		private float _Speed;

		[SerializeField]
		[Tooltip("Angular deviation from the wind direction.")]
		[FormerlySerializedAs("deviation")]
		private float _Deviation;

		[Range(0f, 4f)]
		[FormerlySerializedAs("intensity")]
		[SerializeField]
		private float _Intensity;

		[SerializeField]
		[FormerlySerializedAs("tiling")]
		private Vector2 _Tiling;
	}
}
