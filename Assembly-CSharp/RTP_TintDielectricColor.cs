using System;
using UnityEngine;

[AddComponentMenu("Relief Terrain/Helpers/Tint dielectric color")]
public class RTP_TintDielectricColor : MonoBehaviour
{
	private void Awake()
	{
		this.SetDielectricColorTint();
	}

	private void OnValidate()
	{
		this.SetDielectricColorTint();
	}

	public void SetDielectricColorTint()
	{
		Shader.SetGlobalColor("RTP_ColorSpaceDielectricSpecTint", this.DielectricTint);
	}

	[ColorUsage(false)]
	[Tooltip("You can reduce/increase reflectivity by tinting default unity_ColorSpaceDielectricSpec.rgb color")]
	public Color DielectricTint = new Color(0.2f, 0.2f, 0.2f, 1f);
}
