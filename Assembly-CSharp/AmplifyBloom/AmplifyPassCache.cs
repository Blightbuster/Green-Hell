using System;
using UnityEngine;

namespace AmplifyBloom
{
	[Serializable]
	public class AmplifyPassCache
	{
		public AmplifyPassCache()
		{
			this.Offsets = new Vector4[16];
			this.Weights = new Vector4[16];
		}

		public void Destroy()
		{
			this.Offsets = null;
			this.Weights = null;
		}

		[SerializeField]
		internal Vector4[] Offsets;

		[SerializeField]
		internal Vector4[] Weights;
	}
}
