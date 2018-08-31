using System;
using UnityEngine;
using UnityEngine.UI;

namespace MirzaBeig.ParticleSystems.Demos
{
	public class FPSDisplay : MonoBehaviour
	{
		private void Awake()
		{
		}

		private void Start()
		{
			this.fpsText = base.GetComponent<Text>();
		}

		private void Update()
		{
			this.frameCount++;
			this.timer += Time.deltaTime;
			this.fpsAccum += 1f / Time.deltaTime;
			if (this.timer >= this.updateTime)
			{
				this.timer = 0f;
				int num = Mathf.RoundToInt(this.fpsAccum / (float)this.frameCount);
				this.fpsText.text = "Average FPS: " + num;
				this.frameCount = 0;
				this.fpsAccum = 0f;
			}
		}

		private float timer;

		public float updateTime = 1f;

		private int frameCount;

		private float fpsAccum;

		private Text fpsText;
	}
}
