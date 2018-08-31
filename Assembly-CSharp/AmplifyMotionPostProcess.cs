using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("")]
public sealed class AmplifyMotionPostProcess : MonoBehaviour
{
	public AmplifyMotionEffectBase Instance
	{
		get
		{
			return this.m_instance;
		}
		set
		{
			this.m_instance = value;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (this.m_instance != null)
		{
			this.m_instance.PostProcess(source, destination);
		}
	}

	private AmplifyMotionEffectBase m_instance;
}
