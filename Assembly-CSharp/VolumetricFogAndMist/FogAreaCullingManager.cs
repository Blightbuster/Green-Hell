using System;
using UnityEngine;

namespace VolumetricFogAndMist
{
	public class FogAreaCullingManager : MonoBehaviour
	{
		private void OnEnable()
		{
			if (this.fog == null)
			{
				this.fog = base.GetComponent<VolumetricFog>();
				if (this.fog == null)
				{
					this.fog = base.gameObject.AddComponent<VolumetricFog>();
				}
			}
		}

		private void OnBecameVisible()
		{
			if (this.fog != null)
			{
				this.fog.enabled = true;
			}
		}

		private void OnBecameInvisible()
		{
			if (this.fog != null)
			{
				this.fog.enabled = false;
			}
		}

		public VolumetricFog fog;
	}
}
