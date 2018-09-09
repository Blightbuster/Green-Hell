using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;

namespace UltimateWater
{
	[Serializable]
	public class WaterVolume : WaterModule
	{
		public bool Boundless
		{
			get
			{
				return this._Boundless;
			}
		}

		public bool HasRenderableAdditiveVolumes
		{
			get
			{
				for (int i = this._Volumes.Count - 1; i >= 0; i--)
				{
					if (this._Volumes[i].RenderMode != WaterVolumeRenderMode.None)
					{
						return true;
					}
				}
				return false;
			}
		}

		public List<WaterVolumeAdd> GetVolumesDirect()
		{
			return this._Volumes;
		}

		public List<WaterVolumeSubtract> GetSubtractiveVolumesDirect()
		{
			return this._Subtractors;
		}

		public void Dispose()
		{
		}

		public void EnableRenderers(bool forBorderRendering = false)
		{
			for (int i = 0; i < this._Volumes.Count; i++)
			{
				this._Volumes[i].EnableRenderers(forBorderRendering);
			}
			for (int j = 0; j < this._Subtractors.Count; j++)
			{
				this._Subtractors[j].EnableRenderers(forBorderRendering);
			}
		}

		public void DisableRenderers()
		{
			for (int i = 0; i < this._Volumes.Count; i++)
			{
				this._Volumes[i].DisableRenderers();
			}
			for (int j = 0; j < this._Subtractors.Count; j++)
			{
				this._Subtractors[j].DisableRenderers();
			}
		}

		public bool IsPointInside(Vector3 point, WaterVolumeSubtract[] exclusions, float radius = 0f)
		{
			for (int i = this._Subtractors.Count - 1; i >= 0; i--)
			{
				WaterVolumeSubtract waterVolumeSubtract = this._Subtractors[i];
				if (waterVolumeSubtract.EnablePhysics && waterVolumeSubtract.IsPointInside(point) && !WaterVolume.Contains(exclusions, waterVolumeSubtract))
				{
					return false;
				}
			}
			if (this._Boundless)
			{
				return point.y - radius <= this._Water.transform.position.y + this._Water.MaxVerticalDisplacement;
			}
			for (int j = this._Volumes.Count - 1; j >= 0; j--)
			{
				WaterVolumeAdd waterVolumeAdd = this._Volumes[j];
				if (waterVolumeAdd.EnablePhysics && waterVolumeAdd.IsPointInside(point))
				{
					return true;
				}
			}
			return false;
		}

		internal override void Start(Water water)
		{
			this._Water = water;
		}

		internal override void Enable()
		{
			if (!this._CollidersAdded && Application.isPlaying)
			{
				foreach (Collider collider in this._Water.GetComponentsInChildren<Collider>(true))
				{
					WaterVolumeSubtract component = collider.GetComponent<WaterVolumeSubtract>();
					if (component == null)
					{
						WaterVolumeAdd component2 = collider.GetComponent<WaterVolumeAdd>();
						this.AddVolume((!(component2 != null)) ? collider.gameObject.AddComponent<WaterVolumeAdd>() : component2);
					}
				}
				this._CollidersAdded = true;
			}
			this.EnableRenderers(false);
		}

		internal override void Disable()
		{
			this.Dispose();
			this.DisableRenderers();
		}

		internal void AddVolume(WaterVolumeAdd volume)
		{
			this._Volumes.Add(volume);
			volume.AssignTo(this._Water);
		}

		internal void RemoveVolume(WaterVolumeAdd volume)
		{
			this._Volumes.Remove(volume);
		}

		internal void AddSubtractor(WaterVolumeSubtract volume)
		{
			this._Subtractors.Add(volume);
			volume.AssignTo(this._Water);
		}

		internal void RemoveSubtractor(WaterVolumeSubtract volume)
		{
			this._Subtractors.Remove(volume);
		}

		private static bool Contains(WaterVolumeSubtract[] array, WaterVolumeSubtract element)
		{
			if (array == null)
			{
				return false;
			}
			for (int i = array.Length - 1; i >= 0; i--)
			{
				if (array[i] == element)
				{
					return true;
				}
			}
			return false;
		}

		internal bool IsPointInsideMainVolume(Vector3 point, float radius = 0f)
		{
			return this._Boundless && point.y - radius <= this._Water.transform.position.y + this._Water.MaxVerticalDisplacement;
		}

		internal override void Update()
		{
		}

		internal override void Destroy()
		{
		}

		internal override void Validate()
		{
		}

		[SerializeField]
		[Tooltip("Makes water volume be infinite in horizontal directions and infinitely deep. It is still reduced by subtractive colliders tho. Check that if this is an ocean, sea or if this water spans through most of the scene. If you will uncheck this, you will need to add some child colliders to define where water should display.")]
		private bool _Boundless = true;

		private bool _CollidersAdded;

		private Water _Water;

		private readonly List<WaterVolumeAdd> _Volumes = new List<WaterVolumeAdd>();

		private readonly List<WaterVolumeSubtract> _Subtractors = new List<WaterVolumeSubtract>();
	}
}
