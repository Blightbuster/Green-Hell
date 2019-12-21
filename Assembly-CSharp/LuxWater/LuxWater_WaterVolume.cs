using System;
using System.Collections.Generic;
using UnityEngine;

namespace LuxWater
{
	public class LuxWater_WaterVolume : MonoBehaviour
	{
		private void OnEnable()
		{
			if (this.WaterVolumeMesh == null)
			{
				Debug.Log("No WaterVolumeMesh assigned.");
				return;
			}
			base.Invoke("Register", 0f);
			Material sharedMaterial = base.GetComponent<Renderer>().sharedMaterial;
			sharedMaterial.EnableKeyword("USINGWATERVOLUME");
			sharedMaterial.SetFloat("_WaterSurfaceYPos", base.transform.position.y);
		}

		private void OnDisable()
		{
			if (this.waterrendermanager)
			{
				this.waterrendermanager.DeRegisterWaterVolume(this);
			}
			this.readyToGo = false;
			base.GetComponent<Renderer>().sharedMaterial.DisableKeyword("USINGWATERVOLUME");
		}

		private void Register()
		{
			this.waterrendermanager = LuxWater_UnderWaterRendering.instance;
			this.waterrendermanager.RegisterWaterVolume(this);
			this.readyToGo = true;
		}

		private void OnTriggerEnter(Collider other)
		{
			LuxWater_WaterVolumeTrigger component = other.GetComponent<LuxWater_WaterVolumeTrigger>();
			if (component != null && this.waterrendermanager != null && this.readyToGo && component.active)
			{
				this.waterrendermanager.EnteredWaterVolume(this);
			}
		}

		private void OnTriggerStay(Collider other)
		{
			other.GetComponents<LuxWater_WaterVolumeTrigger>(this.m_TriggerCache);
			if (this.m_TriggerCache.Count == 0)
			{
				return;
			}
			LuxWater_WaterVolumeTrigger luxWater_WaterVolumeTrigger = this.m_TriggerCache[0];
			if (luxWater_WaterVolumeTrigger != null && this.waterrendermanager != null && this.readyToGo && luxWater_WaterVolumeTrigger.active)
			{
				this.waterrendermanager.EnteredWaterVolume(this);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			LuxWater_WaterVolumeTrigger component = other.GetComponent<LuxWater_WaterVolumeTrigger>();
			if (component != null && this.waterrendermanager != null && this.readyToGo && component.active)
			{
				this.waterrendermanager.LeftWaterVolume(this);
			}
		}

		public Mesh WaterVolumeMesh;

		private LuxWater_UnderWaterRendering waterrendermanager;

		private bool readyToGo;

		private List<LuxWater_WaterVolumeTrigger> m_TriggerCache = new List<LuxWater_WaterVolumeTrigger>(10);
	}
}
