using System;
using System.Collections.Generic;
using UnityEngine;

namespace IESLights.Demo
{
	public class ToggleIES : MonoBehaviour
	{
		private void Start()
		{
			foreach (Light light in base.GetComponentsInChildren<Light>())
			{
				this._spotsToCookies.Add(light, light.cookie);
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				foreach (Light light in this._spotsToCookies.Keys)
				{
					if (light.cookie == null)
					{
						light.cookie = this._spotsToCookies[light];
						light.intensity = 0.7f;
					}
					else
					{
						light.cookie = null;
						light.intensity = 0.4f;
					}
				}
			}
		}

		private Dictionary<Light, Texture> _spotsToCookies = new Dictionary<Light, Texture>();
	}
}
