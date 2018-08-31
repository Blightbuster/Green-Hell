using System;
using UnityEngine;

public class _appControlerShaderOnTriplanarMesh : MonoBehaviour
{
	private void Awake()
	{
		this.panel_enabled = true;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			this.panel_enabled = !this.panel_enabled;
		}
	}

	private void OnGUI()
	{
		GUILayout.Space(10f);
		GUILayout.BeginVertical("box", new GUILayoutOption[0]);
		GUILayout.Label(string.Empty + FPSmeter.fps, new GUILayoutOption[0]);
		if (this.panel_enabled)
		{
			this.shadows = GUILayout.Toggle(this.shadows, "disable Unity's shadows", new GUILayoutOption[0]);
			Light component = GameObject.Find("Directional light").GetComponent<Light>();
			component.shadows = ((!this.shadows) ? LightShadows.Soft : LightShadows.None);
			this.forward_path = GUILayout.Toggle(this.forward_path, "forward rendering", new GUILayoutOption[0]);
			Camera component2 = GameObject.Find("Main Camera").GetComponent<Camera>();
			component2.renderingPath = ((!this.forward_path) ? RenderingPath.DeferredShading : RenderingPath.Forward);
			GUILayout.Label("  Drag model/env to rotate", new GUILayoutOption[0]);
			GUILayout.Label("  Wheel - zoom camera", new GUILayoutOption[0]);
		}
		GUILayout.Label("  P - toggle panel", new GUILayoutOption[0]);
		GUILayout.EndVertical();
	}

	public bool shadows;

	public bool forward_path = true;

	private bool panel_enabled;

	public float model_dir;
}
