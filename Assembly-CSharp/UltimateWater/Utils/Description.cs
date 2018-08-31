using System;
using UnityEngine;

namespace UltimateWater.Utils
{
	[AddComponentMenu("Ultimate Water/Utils/Description")]
	public class Description : MonoBehaviour
	{
		[ContextMenu("Toggle Edit")]
		private void ToggleEdit()
		{
			Description.Editable = !Description.Editable;
		}

		[Multiline]
		public string Text;

		public static bool Editable;
	}
}
