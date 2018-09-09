using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public sealed class WaterVolumeProbe : MonoBehaviour
	{
		public Water CurrentWater
		{
			get
			{
				return this._CurrentWater;
			}
		}

		public UnityEvent Enter
		{
			get
			{
				return (this._Enter == null) ? (this._Enter = new UnityEvent()) : this._Enter;
			}
		}

		public UnityEvent Leave
		{
			get
			{
				return (this._Leave == null) ? (this._Leave = new UnityEvent()) : this._Leave;
			}
		}

		public static WaterVolumeProbe CreateProbe(Transform target, float size = 0f)
		{
			GameObject gameObject = new GameObject("Water Volume Probe")
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			gameObject.transform.position = target.position;
			gameObject.layer = WaterProjectSettings.Instance.WaterCollidersLayer;
			SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
			sphereCollider.radius = size;
			sphereCollider.isTrigger = true;
			Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;
			rigidbody.mass = 1E-07f;
			WaterVolumeProbe waterVolumeProbe = gameObject.AddComponent<WaterVolumeProbe>();
			waterVolumeProbe._Target = target;
			waterVolumeProbe._Targetted = true;
			waterVolumeProbe._Size = size;
			waterVolumeProbe._Exclusions = target.GetComponentsInChildren<WaterVolumeSubtract>(true);
			return waterVolumeProbe;
		}

		private void Start()
		{
			this.ScanWaters();
		}

		private void FixedUpdate()
		{
			if (this._Targetted)
			{
				if (this._Target == null)
				{
					UnityEngine.Object.Destroy(base.gameObject);
					return;
				}
				base.transform.position = this._Target.position;
			}
			if (this._CurrentWater != null && this._CurrentWater.Volume.Boundless)
			{
				if (!this._CurrentWater.Volume.IsPointInsideMainVolume(base.transform.position, 0f) && !this._CurrentWater.Volume.IsPointInside(base.transform.position, this._Exclusions, this._Size))
				{
					this.LeaveCurrentWater();
				}
			}
			else if (this._CurrentWater == null)
			{
				this.ScanBoundlessWaters();
			}
		}

		private void OnDestroy()
		{
			this._CurrentWater = null;
			if (this._Enter != null)
			{
				this._Enter.RemoveAllListeners();
				this._Enter = null;
			}
			if (this._Leave != null)
			{
				this._Leave.RemoveAllListeners();
				this._Leave = null;
			}
		}

		public void OnTriggerEnter(Collider other)
		{
			if (this._CurrentWater != null)
			{
				WaterVolumeBase waterVolume = WaterVolumeBase.GetWaterVolume<WaterVolumeSubtract>(other);
				if (waterVolume != null && waterVolume.EnablePhysics)
				{
					this.LeaveCurrentWater();
				}
			}
			else
			{
				WaterVolumeBase waterVolume2 = WaterVolumeBase.GetWaterVolume<WaterVolumeAdd>(other);
				if (waterVolume2 != null && waterVolume2.EnablePhysics)
				{
					this.EnterWater(waterVolume2.Water);
				}
			}
		}

		public void OnTriggerExit(Collider other)
		{
			if (this._CurrentWater == null)
			{
				WaterVolumeBase waterVolume = WaterVolumeBase.GetWaterVolume<WaterVolumeSubtract>(other);
				if (waterVolume != null && waterVolume.EnablePhysics)
				{
					this.ScanWaters();
				}
			}
			else
			{
				WaterVolumeBase waterVolume2 = WaterVolumeBase.GetWaterVolume<WaterVolumeAdd>(other);
				if (waterVolume2 != null && waterVolume2.Water == this._CurrentWater && waterVolume2.EnablePhysics)
				{
					this.LeaveCurrentWater();
				}
			}
		}

		[ContextMenu("Refresh Probe")]
		private void ScanWaters()
		{
			Vector3 position = base.transform.position;
			List<Water> waters = ApplicationSingleton<WaterSystem>.Instance.Waters;
			int count = waters.Count;
			for (int i = 0; i < count; i++)
			{
				if (waters[i].Volume.IsPointInside(position, this._Exclusions, this._Size))
				{
					this.EnterWater(waters[i]);
					return;
				}
			}
			this.LeaveCurrentWater();
		}

		private void ScanBoundlessWaters()
		{
			Vector3 position = base.transform.position;
			List<Water> boundlessWaters = ApplicationSingleton<WaterSystem>.Instance.BoundlessWaters;
			int count = boundlessWaters.Count;
			for (int i = 0; i < count; i++)
			{
				Water water = boundlessWaters[i];
				if (water.Volume.IsPointInsideMainVolume(position, 0f) && water.Volume.IsPointInside(position, this._Exclusions, this._Size))
				{
					this.EnterWater(water);
					return;
				}
			}
		}

		private void EnterWater(Water water)
		{
			if (this._CurrentWater == water)
			{
				return;
			}
			if (this._CurrentWater != null)
			{
				this.LeaveCurrentWater();
			}
			this._CurrentWater = water;
			if (this._Enter != null)
			{
				this._Enter.Invoke();
			}
		}

		private void LeaveCurrentWater()
		{
			if (this._CurrentWater != null)
			{
				if (this._Leave != null)
				{
					this._Leave.Invoke();
				}
				this._CurrentWater = null;
			}
		}

		[SerializeField]
		[FormerlySerializedAs("enter")]
		private UnityEvent _Enter;

		[SerializeField]
		[FormerlySerializedAs("leave")]
		private UnityEvent _Leave;

		private Water _CurrentWater;

		private Transform _Target;

		private bool _Targetted;

		private WaterVolumeSubtract[] _Exclusions;

		private float _Size;
	}
}
