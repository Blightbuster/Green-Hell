using System;
using UnityEngine;

namespace AmplifyBloom
{
	[Serializable]
	public class AmplifyBokehData
	{
		public AmplifyBokehData(Vector4[] offsets)
		{
			this.Offsets = offsets;
		}

		public void Destroy()
		{
			if (this.BokehRenderTexture != null)
			{
				AmplifyUtils.ReleaseTempRenderTarget(this.BokehRenderTexture);
				this.BokehRenderTexture = null;
			}
			this.Offsets = null;
		}

		internal RenderTexture BokehRenderTexture;

		internal Vector4[] Offsets;
	}
}
