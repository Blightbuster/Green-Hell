using System;
using UltimateWater.Internal;
using UnityEngine;

namespace UltimateWater
{
	public sealed class WaterSample
	{
		public WaterSample(Water water, WaterSample.DisplacementMode displacementMode = WaterSample.DisplacementMode.Height, float precision = 1f)
		{
			if (water == null)
			{
				throw new ArgumentException("Argument 'water' is null.");
			}
			if (precision <= 0f || precision > 1f)
			{
				throw new ArgumentException("Precision has to be between 0.0 and 1.0.");
			}
			this._Water = water;
			this._DisplacementMode = displacementMode;
			this._PreviousResult.x = float.NaN;
		}

		public bool Finished
		{
			get
			{
				return this._Finished;
			}
		}

		public Vector2 Position
		{
			get
			{
				return new Vector2(this._X, this._Z);
			}
		}

		public void Start(Vector3 origin)
		{
			this._Finished = true;
			this._PreviousResult = (this._Displaced = origin);
			this._PreviousForces = (this._Forces = default(Vector3));
			this.GetAndReset(origin.x, origin.z, WaterSample.ComputationsMode.Normal);
		}

		public void Start(float x, float z)
		{
			this._Finished = true;
			this._PreviousResult = (this._Displaced = new Vector3(x, this._Water.transform.position.y, z));
			this._PreviousForces = (this._Forces = default(Vector3));
			this.GetAndReset(x, z, WaterSample.ComputationsMode.Normal);
		}

		public Vector3 GetAndReset(Vector3 origin, WaterSample.ComputationsMode mode = WaterSample.ComputationsMode.Normal)
		{
			return this.GetAndReset(origin.x, origin.z, mode);
		}

		public Vector3 GetAndReset(float x, float z, WaterSample.ComputationsMode mode = WaterSample.ComputationsMode.Normal)
		{
			Vector3 vector;
			return this.GetAndReset(x, z, mode, out vector);
		}

		public Vector3 GetAndReset(float x, float z, WaterSample.ComputationsMode mode, out Vector3 forces)
		{
			if (mode != WaterSample.ComputationsMode.ForceCompletion)
			{
				if (mode == WaterSample.ComputationsMode.Normal)
				{
					if (!this._Finished && !float.IsNaN(this._PreviousResult.x))
					{
						forces = this._PreviousForces;
						return this._PreviousResult;
					}
					this._PreviousResult = this._Displaced;
					this._PreviousForces = this._Forces;
				}
			}
			else if (!this._Finished)
			{
				this._Finished = true;
				this.ComputationStep(true);
			}
			this._Finished = true;
			if (!this._Enqueued)
			{
				WaterAsynchronousTasks.Instance.AddWaterSampleComputations(this);
				this._Enqueued = true;
				this._Water.OnSamplingStarted();
			}
			Vector3 displaced = this._Displaced;
			displaced.y += this._Water.transform.position.y;
			forces = this._Forces;
			this._X = x;
			this._Z = z;
			this._Displaced.x = x;
			this._Displaced.y = 0f;
			this._Displaced.z = z;
			this._Forces.x = 0f;
			this._Forces.y = 0f;
			this._Forces.z = 0f;
			this._Time = this._Water.Time;
			this._Finished = false;
			return displaced;
		}

		public void GetAndResetFast(float x, float z, float time, out Vector3 result, out Vector3 forces)
		{
			if (!this._Finished)
			{
				forces = this._PreviousForces;
				result = this._PreviousResult;
				return;
			}
			this._PreviousResult = this._Displaced;
			this._PreviousForces = this._Forces;
			result = this._Displaced;
			result.y += this._Water.transform.position.y;
			forces = this._Forces;
			this._X = x;
			this._Z = z;
			this._Displaced.x = x;
			this._Displaced.y = 0f;
			this._Displaced.z = z;
			this._Forces.x = 0f;
			this._Forces.y = 0f;
			this._Forces.z = 0f;
			this._Time = time;
			this._Finished = false;
		}

		public void GetAndResetFast(float x, float z, float time, out float result, out Vector3 forces)
		{
			if (!this._Finished)
			{
				forces = this._PreviousForces;
				result = this._PreviousResult.y;
				return;
			}
			this._PreviousResult = this._Displaced;
			this._PreviousForces = this._Forces;
			result = this._Displaced.y + this._Water.transform.position.y;
			forces = this._Forces;
			this._X = x;
			this._Z = z;
			this._Displaced.x = x;
			this._Displaced.y = 0f;
			this._Displaced.z = z;
			this._Forces.x = 0f;
			this._Forces.y = 0f;
			this._Forces.z = 0f;
			this._Time = time;
			this._Finished = false;
		}

		public Vector3 Stop()
		{
			if (this._Enqueued)
			{
				if (WaterAsynchronousTasks.HasInstance)
				{
					WaterAsynchronousTasks.Instance.RemoveWaterSampleComputations(this);
				}
				this._Enqueued = false;
				if (this._Water != null)
				{
					this._Water.OnSamplingStopped();
				}
			}
			return this._Displaced;
		}

		internal void ComputationStep(bool ignoreFinishedFlag = false)
		{
			if (!this._Finished || ignoreFinishedFlag)
			{
				if (this._DisplacementMode == WaterSample.DisplacementMode.Height || this._DisplacementMode == WaterSample.DisplacementMode.HeightAndForces)
				{
					this._Water.CompensateHorizontalDisplacement(ref this._X, ref this._Z, this._Time);
					if (this._DisplacementMode == WaterSample.DisplacementMode.Height)
					{
						float uncompensatedHeightAt = this._Water.GetUncompensatedHeightAt(this._X, this._Z, this._Time);
						this._Displaced.y = this._Displaced.y + uncompensatedHeightAt;
					}
					else
					{
						Vector4 uncompensatedHeightAndForcesAt = this._Water.GetUncompensatedHeightAndForcesAt(this._X, this._Z, this._Time);
						this._Displaced.y = this._Displaced.y + uncompensatedHeightAndForcesAt.w;
						this._Forces.x = this._Forces.x + uncompensatedHeightAndForcesAt.x;
						this._Forces.y = this._Forces.y + uncompensatedHeightAndForcesAt.y;
						this._Forces.z = this._Forces.z + uncompensatedHeightAndForcesAt.z;
					}
				}
				else
				{
					Vector3 b = (this._Water.WaterId == -1) ? default(Vector3) : this._Water.GetUncompensatedDisplacementAt(this._X, this._Z, this._Time);
					this._Displaced += b;
				}
				this._Finished = true;
			}
		}

		private readonly Water _Water;

		private float _X;

		private float _Z;

		private float _Time;

		private Vector3 _Displaced;

		private Vector3 _PreviousResult;

		private Vector3 _Forces;

		private Vector3 _PreviousForces;

		private bool _Finished;

		private bool _Enqueued;

		private readonly WaterSample.DisplacementMode _DisplacementMode;

		public enum DisplacementMode
		{
			Height,
			Displacement,
			HeightAndForces
		}

		public enum ComputationsMode
		{
			Normal,
			ForceCompletion = 2
		}
	}
}
