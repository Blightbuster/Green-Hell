using System;
using UnityEngine;

public class ShaderWarm : MonoBehaviour
{
	private void Start()
	{
		Shader.WarmupAllShaders();
	}
}
