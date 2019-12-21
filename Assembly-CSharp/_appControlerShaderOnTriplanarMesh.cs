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
		GUILayout.BeginVertical("box", Array.Empty<GUILayoutOption>());
		GUILayout.Label(string.Concat(FPSmeter.fps), Array.Empty<GUILayoutOption>());
		if (this.panel_enabled)
		{
			this.shadows = GUILayout.Toggle(this.shadows, "disable Unity's shadows", Array.Empty<GUILayoutOption>());
			GameObject.Find("Directional light").GetComponent<Light>().shadows = (this.shadows ? LightShadows.None : LightShadows.Soft);
			this.forward_path = GUILayout.Toggle(this.forward_path, "forward rendering", Array.Empty<GUILayoutOption>());
			GameObject.Find("Main Camera").GetComponent<Camera>().renderingPath = (this.forward_path ? RenderingPath.Forward : RenderingPath.DeferredShading);
			GUILayout.Label("  Drag model/env to rotate", Array.Empty<GUILayoutOption>());
			GUILayout.Label("  Wheel - zoom camera", Array.Empty<GUILayoutOption>());
		}
		GUILayout.Label("  P - toggle panel", Array.Empty<GUILayoutOption>());
		GUILayout.EndVertical();
	}

	public bool shadows;

	public bool forward_path = true;

	private bool panel_enabled;

	public float model_dir;
}
