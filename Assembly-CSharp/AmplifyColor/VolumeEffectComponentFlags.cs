using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AmplifyColor
{
	[Serializable]
	public class VolumeEffectComponentFlags
	{
		public VolumeEffectComponentFlags(string name)
		{
			this.componentName = name;
			this.componentFields = new List<VolumeEffectFieldFlags>();
		}

		public VolumeEffectComponentFlags(VolumeEffectComponent comp) : this(comp.componentName)
		{
			this.blendFlag = true;
			foreach (VolumeEffectField volumeEffectField in comp.fields)
			{
				if (VolumeEffectField.IsValidType(volumeEffectField.fieldType))
				{
					this.componentFields.Add(new VolumeEffectFieldFlags(volumeEffectField));
				}
			}
		}

		public VolumeEffectComponentFlags(Component c) : this(c.GetType() + string.Empty)
		{
			FieldInfo[] fields = c.GetType().GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				if (VolumeEffectField.IsValidType(fieldInfo.FieldType.FullName))
				{
					this.componentFields.Add(new VolumeEffectFieldFlags(fieldInfo));
				}
			}
		}

		public void UpdateComponentFlags(VolumeEffectComponent comp)
		{
			using (List<VolumeEffectField>.Enumerator enumerator = comp.fields.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					VolumeEffectField field = enumerator.Current;
					if (this.componentFields.Find((VolumeEffectFieldFlags s) => s.fieldName == field.fieldName) == null && VolumeEffectField.IsValidType(field.fieldType))
					{
						this.componentFields.Add(new VolumeEffectFieldFlags(field));
					}
				}
			}
		}

		public void UpdateComponentFlags(Component c)
		{
			FieldInfo[] fields = c.GetType().GetFields();
			FieldInfo[] array = fields;
			for (int i = 0; i < array.Length; i++)
			{
				FieldInfo pi = array[i];
				if (!this.componentFields.Exists((VolumeEffectFieldFlags s) => s.fieldName == pi.Name) && VolumeEffectField.IsValidType(pi.FieldType.FullName))
				{
					this.componentFields.Add(new VolumeEffectFieldFlags(pi));
				}
			}
		}

		public string[] GetFieldNames()
		{
			return (from r in this.componentFields
			where r.blendFlag
			select r.fieldName).ToArray<string>();
		}

		public string componentName;

		public List<VolumeEffectFieldFlags> componentFields;

		public bool blendFlag;
	}
}
