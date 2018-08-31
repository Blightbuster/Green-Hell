using System;
using UnityEngine;

namespace UltimateWater
{
	public struct WaterWave : IComparable<WaterWave>
	{
		public WaterWave(byte scaleIndex, float offsetX, float offsetZ, ushort u, ushort v, float kx, float kz, float k, float w, float amplitude)
		{
			this._ScaleIndex = scaleIndex;
			this._DotOffset = offsetX * kx + offsetZ * kz;
			this._U = u;
			this._V = v;
			this._Kx = kx;
			this._Kz = kz;
			this._Nkx = ((k == 0f) ? 0.707107f : (kx / k));
			this._Nky = ((k == 0f) ? 0.707107f : (kz / k));
			this._Amplitude = 2f * amplitude;
			this._Offset = 0f;
			this._W = w;
			this._CPUPriority = ((amplitude < 0f) ? (-amplitude) : amplitude);
		}

		public float K
		{
			get
			{
				return Mathf.Sqrt(this._Kx * this._Kx + this._Kz * this._Kz);
			}
		}

		public int CompareTo(WaterWave other)
		{
			return other._CPUPriority.CompareTo(this._CPUPriority);
		}

		public float GetHeightAt(float x, float z, float t)
		{
			float num = this._Kx * x + this._Kz * z;
			return this._Amplitude * Mathf.Sin(num + t * this._W + this._Offset);
		}

		public void GetForceAndHeightAt(float x, float z, float t, ref Vector4 result)
		{
			float num = this._Kx * x + this._Kz * z;
			int num2 = (int)((num + t * this._W + this._Offset) * 325.949f) & 2047;
			float num3 = FastMath.Sines[num2];
			float num4 = FastMath.Cosines[num2];
			float num5 = this._Amplitude * num3;
			float num6 = this._Amplitude * num4;
			result.x += this._Nkx * num5;
			result.z += this._Nky * num5;
			result.y += num6;
			result.w += num5;
		}

		public Vector2 GetRawHorizontalDisplacementAt(float x, float z, float t)
		{
			float num = this._Kx * x + this._Kz * z;
			float num2 = this._Amplitude * Mathf.Cos(num + t * this._W + this._Offset);
			return new Vector2(this._Nkx * num2, this._Nky * num2);
		}

		public Vector3 GetDisplacementAt(float x, float z, float t)
		{
			float num = this._Kx * x + this._Kz * z;
			float num2;
			float num3;
			FastMath.SinCos2048(num + t * this._W + this._Offset, out num2, out num3);
			num3 *= this._Amplitude;
			return new Vector3(this._Nkx * num3, num2 * this._Amplitude, this._Nky * num3);
		}

		public void UpdateSpectralValues(Vector3[][] spectrum, Vector2 windDirection, float directionalityInv, int resolution, float horizontalScale)
		{
			Vector3 vector = spectrum[(int)this._ScaleIndex][(int)this._U * resolution + (int)this._V];
			float num = windDirection.x * this._Nkx + windDirection.y * this._Nky;
			float num2 = Mathf.Acos(num * 0.999f);
			float num3 = Mathf.Sqrt(1f + vector.z * Mathf.Cos(2f * num2));
			if (num < 0f)
			{
				num3 *= directionalityInv;
			}
			float num4 = vector.x * num3;
			float num5 = vector.y * num3;
			this._Amplitude = 2f * Mathf.Sqrt(num4 * num4 + num5 * num5);
			this._Offset = Mathf.Atan2(Mathf.Abs(num4), Mathf.Abs(num5));
			if (num5 > 0f)
			{
				this._Amplitude = -this._Amplitude;
				this._Offset = -this._Offset;
			}
			if (num4 < 0f)
			{
				this._Offset = -this._Offset;
			}
			this._Offset += this._DotOffset;
			this._CPUPriority = ((this._Amplitude < 0f) ? (-this._Amplitude) : this._Amplitude);
		}

		private readonly ushort _U;

		private readonly ushort _V;

		private readonly float _Kx;

		private readonly float _Kz;

		private readonly float _DotOffset;

		internal readonly float _Nkx;

		internal readonly float _Nky;

		internal readonly float _W;

		internal readonly byte _ScaleIndex;

		internal float _Amplitude;

		internal float _CPUPriority;

		internal float _Offset;
	}
}
