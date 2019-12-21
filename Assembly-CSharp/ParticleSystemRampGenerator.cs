using System;
using UnityEngine;

public class ParticleSystemRampGenerator : MonoBehaviour
{
	private void Start()
	{
		this.psr = base.GetComponent<ParticleSystemRenderer>();
		if (this.procedrualGradientEnabled)
		{
			this.UpdateRampTexture();
		}
	}

	private void Update()
	{
		if (this.procedrualGradientEnabled && this.updateEveryFrame)
		{
			this.UpdateRampTexture();
		}
	}

	private Texture2D GenerateTextureFromGradient(Gradient grad)
	{
		float num = 256f;
		float num2 = 1f;
		Texture2D texture2D = new Texture2D((int)num, (int)num2);
		int num3 = 0;
		while ((float)num3 < num)
		{
			int num4 = 0;
			while ((float)num4 < num2)
			{
				Color color = grad.Evaluate(0f + (float)num3 / num);
				texture2D.SetPixel(num3, num4, color);
				num4++;
			}
			num3++;
		}
		texture2D.wrapMode = TextureWrapMode.Clamp;
		texture2D.Apply();
		return texture2D;
	}

	public void UpdateRampTexture()
	{
		this.rampTexture = this.GenerateTextureFromGradient(this.procedrualGradientRamp);
		this.psr.material.SetTexture("_Ramp", this.rampTexture);
	}

	public Gradient procedrualGradientRamp;

	public bool procedrualGradientEnabled;

	public bool updateEveryFrame;

	private ParticleSystemRenderer psr;

	private Texture2D rampTexture;
}
