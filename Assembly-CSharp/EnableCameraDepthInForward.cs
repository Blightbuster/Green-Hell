using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class EnableCameraDepthInForward : MonoBehaviour
{
	private void Start()
	{
		this.Set();
	}

	private void Set()
	{
		if (base.GetComponent<Camera>().depthTextureMode == DepthTextureMode.None)
		{
			base.GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
		}
	}
}
