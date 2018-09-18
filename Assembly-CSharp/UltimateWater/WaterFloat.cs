using System;
using UnityEngine;

namespace UltimateWater
{
	public sealed class WaterFloat : MonoBehaviour
	{
		private void Start()
		{
			if (this._Water == null)
			{
				this._Water = Utilities.GetWaterReference();
			}
			if (this._Water.IsNullReference(this))
			{
				return;
			}
			this._InitialPosition = base.transform.position;
			this._PreviousPosition = this._InitialPosition;
			this._Sample = new WaterSample(this._Water, (WaterSample.DisplacementMode)this._DisplacementMode, this._Precision);
			this._Sample.Start(base.transform.position);
		}

		private void OnDisable()
		{
			this._Sample.Stop();
		}

		private void LateUpdate()
		{
			this._InitialPosition += base.transform.position - this._PreviousPosition;
			Vector3 andReset = this._Sample.GetAndReset(this._InitialPosition.x, this._InitialPosition.z, WaterSample.ComputationsMode.ForceCompletion);
			andReset.y += this._HeightBonus;
			base.transform.position = andReset;
			this._PreviousPosition = andReset;
		}

		private void Reset()
		{
			this._Water = Utilities.GetWaterReference();
		}

		[SerializeField]
		private WaterFloat.DisplacementMode _DisplacementMode = WaterFloat.DisplacementMode.Displacement;

		[SerializeField]
		private float _HeightBonus;

		[Range(0.04f, 1f)]
		[SerializeField]
		private float _Precision = 0.2f;

		[SerializeField]
		private Water _Water;

		private Vector3 _InitialPosition;

		private Vector3 _PreviousPosition;

		private WaterSample _Sample;

		public enum DisplacementMode
		{
			Height,
			Displacement
		}
	}
}
