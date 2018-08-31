using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AmplifyColor
{
	[Serializable]
	public class VolumeEffectFlags
	{
		public VolumeEffectFlags()
		{
			this.components = new List<VolumeEffectComponentFlags>();
		}

		public void AddComponent(Component c)
		{
			VolumeEffectComponentFlags volumeEffectComponentFlags;
			if ((volumeEffectComponentFlags = this.components.Find((VolumeEffectComponentFlags s) => s.componentName == c.GetType() + string.Empty)) != null)
			{
				volumeEffectComponentFlags.UpdateComponentFlags(c);
			}
			else
			{
				this.components.Add(new VolumeEffectComponentFlags(c));
			}
		}

		public void UpdateFlags(VolumeEffect effectVol)
		{
			using (List<VolumeEffectComponent>.Enumerator enumerator = effectVol.components.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					VolumeEffectComponent comp = enumerator.Current;
					VolumeEffectComponentFlags volumeEffectComponentFlags;
					if ((volumeEffectComponentFlags = this.components.Find((VolumeEffectComponentFlags s) => s.componentName == comp.componentName)) == null)
					{
						this.components.Add(new VolumeEffectComponentFlags(comp));
					}
					else
					{
						volumeEffectComponentFlags.UpdateComponentFlags(comp);
					}
				}
			}
		}

		public static void UpdateCamFlags(AmplifyColorBase[] effects, AmplifyColorVolumeBase[] volumes)
		{
			foreach (AmplifyColorBase amplifyColorBase in effects)
			{
				amplifyColorBase.EffectFlags = new VolumeEffectFlags();
				foreach (AmplifyColorVolumeBase amplifyColorVolumeBase in volumes)
				{
					VolumeEffect volumeEffect = amplifyColorVolumeBase.EffectContainer.FindVolumeEffect(amplifyColorBase);
					if (volumeEffect != null)
					{
						amplifyColorBase.EffectFlags.UpdateFlags(volumeEffect);
					}
				}
			}
		}

		public VolumeEffect GenerateEffectData(AmplifyColorBase go)
		{
			VolumeEffect volumeEffect = new VolumeEffect(go);
			foreach (VolumeEffectComponentFlags volumeEffectComponentFlags in this.components)
			{
				if (volumeEffectComponentFlags.blendFlag)
				{
					Component component = go.GetComponent(volumeEffectComponentFlags.componentName);
					if (component != null)
					{
						volumeEffect.AddComponent(component, volumeEffectComponentFlags);
					}
				}
			}
			return volumeEffect;
		}

		public VolumeEffectComponentFlags FindComponentFlags(string compName)
		{
			for (int i = 0; i < this.components.Count; i++)
			{
				if (this.components[i].componentName == compName)
				{
					return this.components[i];
				}
			}
			return null;
		}

		public string[] GetComponentNames()
		{
			return (from r in this.components
			where r.blendFlag
			select r.componentName).ToArray<string>();
		}

		public List<VolumeEffectComponentFlags> components;
	}
}
