using System;
using UnityEngine;

namespace TFHC_Shader_Samples
{
	public class highlightAnimated : MonoBehaviour
	{
		private void Start()
		{
			this.mat = base.GetComponent<Renderer>().material;
		}

		private void OnMouseEnter()
		{
			this.switchhighlighted(true);
		}

		private void OnMouseExit()
		{
			this.switchhighlighted(false);
		}

		private void switchhighlighted(bool highlighted)
		{
			this.mat.SetFloat("_Highlighted", (!highlighted) ? 0f : 1f);
		}

		private Material mat;
	}
}
