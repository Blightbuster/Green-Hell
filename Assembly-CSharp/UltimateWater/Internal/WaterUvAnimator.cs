using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	[Serializable]
	public sealed class WaterUvAnimator : WaterModule
	{
		public Vector2 WindOffset
		{
			get
			{
				return new Vector2(this._WindOffset1X, this._WindOffset1Y);
			}
		}

		public NormalMapAnimation NormalMapAnimation1
		{
			get
			{
				return this._NormalMapAnimation1;
			}
			set
			{
				this._NormalMapAnimation1 = value;
				this._WindVectorsDirty = true;
				this._UvTransform1.x = this._NormalMapAnimation1.Tiling.x;
				this._UvTransform1.y = this._NormalMapAnimation1.Tiling.y;
			}
		}

		public NormalMapAnimation NormalMapAnimation2
		{
			get
			{
				return this._NormalMapAnimation2;
			}
			set
			{
				this._NormalMapAnimation2 = value;
				this._WindVectorsDirty = true;
				this._UvTransform2.x = this._NormalMapAnimation2.Tiling.x;
				this._UvTransform2.y = this._NormalMapAnimation2.Tiling.y;
			}
		}

		internal override void Start(Water water)
		{
			this._Water = water;
			this._WindWaves = water.WindWaves;
			this._HasWindWaves = (this._WindWaves != null);
		}

		internal override void Update()
		{
			float time = this._Water.Time;
			float num = time - this._LastTime;
			this._LastTime = time;
			if (this._WindVectorsDirty || this.HasWindSpeedChanged())
			{
				this.PrecomputeWindVectors();
				this._WindVectorsDirty = false;
			}
			this._WindOffset1X += this._WindSpeed1.x * num;
			this._WindOffset1Y += this._WindSpeed1.y * num;
			this._WindOffset2X += this._WindSpeed2.x * num;
			this._WindOffset2Y += this._WindSpeed2.y * num;
			this._UvTransform1.z = -this._WindOffset1X * this._UvTransform1.x;
			this._UvTransform1.w = -this._WindOffset1Y * this._UvTransform1.y;
			this._UvTransform2.z = -this._WindOffset2X * this._UvTransform2.x;
			this._UvTransform2.w = -this._WindOffset2Y * this._UvTransform2.y;
			MaterialPropertyBlock propertyBlock = this._Water.Renderer.PropertyBlock;
			propertyBlock.SetVector(ShaderVariables.BumpMapST, this._UvTransform1);
			propertyBlock.SetVector(ShaderVariables.DetailAlbedoMapST, this._UvTransform2);
		}

		private void PrecomputeWindVectors()
		{
			this._WindSpeed = this.GetWindSpeed();
			this._WindSpeed1 = FastMath.Rotate(this._WindSpeed, this._NormalMapAnimation1.Deviation * 0.0174532924f) * (this._NormalMapAnimation1.Speed * 0.001365f);
			this._WindSpeed2 = FastMath.Rotate(this._WindSpeed, this._NormalMapAnimation2.Deviation * 0.0174532924f) * (this._NormalMapAnimation2.Speed * 0.00084f);
		}

		private Vector2 GetWindSpeed()
		{
			return (!this._HasWindWaves) ? new Vector2(1f, 0f) : this._WindWaves.WindSpeed;
		}

		private bool HasWindSpeedChanged()
		{
			return this._HasWindWaves && this._WindWaves.WindSpeedChanged;
		}

		private NormalMapAnimation _NormalMapAnimation1 = new NormalMapAnimation(1f, -10f, 1f, new Vector2(1f, 1f));

		private NormalMapAnimation _NormalMapAnimation2 = new NormalMapAnimation(-0.55f, 20f, 0.74f, new Vector2(1.5f, 1.5f));

		private float _WindOffset1X;

		private float _WindOffset1Y;

		private float _WindOffset2X;

		private float _WindOffset2Y;

		private Vector2 _WindSpeed1;

		private Vector2 _WindSpeed2;

		private Vector2 _WindSpeed;

		private Water _Water;

		private WindWaves _WindWaves;

		private bool _HasWindWaves;

		private Vector4 _UvTransform1;

		private Vector4 _UvTransform2;

		private bool _WindVectorsDirty = true;

		private float _LastTime;
	}
}
