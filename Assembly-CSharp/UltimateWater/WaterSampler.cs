using System;
using UnityEngine;
using UnityEngine.Events;

namespace UltimateWater
{
	[AddComponentMenu("Ultimate Water/Water Sampler")]
	public class WaterSampler : MonoBehaviour
	{
		public float Height { get; private set; }

		public float Velocity { get; private set; }

		public WaterSampler.SubmersionState State { get; private set; }

		public bool IsInitialized
		{
			get
			{
				return this._Water != null && this._Sample != null;
			}
		}

		private void Start()
		{
			if (this._Water == null)
			{
				this._Water = Utilities.GetWaterReference();
			}
			this._Sample = new WaterSample(this._Water, WaterSample.DisplacementMode.Height, 1f);
		}

		private void Update()
		{
			Vector3 andReset = this._Sample.GetAndReset(base.transform.position, WaterSample.ComputationsMode.Normal);
			float num = (base.transform.position.y - this._PreviousObjectHeight) / Time.deltaTime;
			float num2 = (andReset.y - this._PreviousWaterHeight) / Time.deltaTime;
			this.Velocity = Mathf.Abs(num - num2);
			this.Height = base.transform.position.y - andReset.y;
			if (this.State != WaterSampler.GetState(this.Height) && Mathf.Abs(this.Height) > this.Hysteresis)
			{
				this.State = ((this.Height <= 0f) ? WaterSampler.SubmersionState.Under : WaterSampler.SubmersionState.Above);
				this.OnSubmersionStateChanged.Invoke(this.State);
			}
			this._PreviousObjectHeight = base.transform.position.y;
			this._PreviousWaterHeight = andReset.y;
			base.transform.rotation = Quaternion.identity;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(base.transform.position, 0.2f);
		}

		private void Reset()
		{
			this._Water = Utilities.GetWaterReference();
		}

		private static WaterSampler.SubmersionState GetState(float height)
		{
			return (height <= 0f) ? WaterSampler.SubmersionState.Under : WaterSampler.SubmersionState.Above;
		}

		[SerializeField]
		[Header("References")]
		private Water _Water;

		public float Hysteresis = 0.1f;

		[Header("Events")]
		public WaterSampler.WaterSubmersionEvent OnSubmersionStateChanged;

		private WaterSample _Sample;

		private float _PreviousWaterHeight;

		private float _PreviousObjectHeight;

		[Serializable]
		public class WaterSubmersionEvent : UnityEvent<WaterSampler.SubmersionState>
		{
		}

		public enum SubmersionState
		{
			Under,
			Above
		}
	}
}
