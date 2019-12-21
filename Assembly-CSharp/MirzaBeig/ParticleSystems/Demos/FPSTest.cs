using System;
using UnityEngine;

namespace MirzaBeig.ParticleSystems.Demos
{
	public class FPSTest : MonoBehaviour
	{
		private void Awake()
		{
		}

		private void Start()
		{
		}

		private void Update()
		{
			if (Input.GetKey(KeyCode.Space))
			{
				Application.targetFrameRate = this.targetFPS2;
				this.previousVSyncCount = QualitySettings.vSyncCount;
				QualitySettings.vSyncCount = 0;
				return;
			}
			if (Input.GetKeyUp(KeyCode.Space))
			{
				Application.targetFrameRate = this.targetFPS1;
				QualitySettings.vSyncCount = this.previousVSyncCount;
			}
		}

		public int targetFPS1 = 60;

		public int targetFPS2 = 10;

		private int previousVSyncCount;
	}
}
