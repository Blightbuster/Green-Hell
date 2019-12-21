using System;
using System.Collections.Generic;
using UnityEngine;

namespace LuxWater
{
	public class LuxWater_Projector : MonoBehaviour
	{
		private void OnEnable()
		{
			if (base.GetComponent<Renderer>() != null)
			{
				this.m_Rend = base.GetComponent<Renderer>();
				this.m_Mat = this.m_Rend.sharedMaterials[0];
				this.m_Rend.enabled = false;
				if (this.Type == LuxWater_Projector.ProjectorType.FoamProjector)
				{
					LuxWater_Projector.FoamProjectors.Add(this);
				}
				else
				{
					LuxWater_Projector.NormalProjectors.Add(this);
				}
				this.added = true;
			}
		}

		private void OnDisable()
		{
			if (this.added)
			{
				if (this.Type == LuxWater_Projector.ProjectorType.FoamProjector)
				{
					LuxWater_Projector.FoamProjectors.Remove(this);
				}
				else
				{
					LuxWater_Projector.NormalProjectors.Remove(this);
				}
				this.m_Rend.enabled = true;
			}
		}

		[Space(8f)]
		public LuxWater_Projector.ProjectorType Type;

		[NonSerialized]
		public static List<LuxWater_Projector> FoamProjectors = new List<LuxWater_Projector>();

		[NonSerialized]
		public static List<LuxWater_Projector> NormalProjectors = new List<LuxWater_Projector>();

		[NonSerialized]
		public Renderer m_Rend;

		[NonSerialized]
		public Material m_Mat;

		private bool added;

		public enum ProjectorType
		{
			FoamProjector,
			NormalProjector
		}
	}
}
