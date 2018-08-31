using System;
using UnityEngine;

namespace RootMotion
{
	public class DemoGUIMessage : MonoBehaviour
	{
		private void OnGUI()
		{
			GUI.color = this.color;
			GUILayout.Label(this.text, new GUILayoutOption[0]);
			GUI.color = Color.white;
		}

		public string text;

		public Color color = Color.white;
	}
}
