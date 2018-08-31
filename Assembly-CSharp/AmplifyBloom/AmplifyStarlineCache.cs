using System;
using UnityEngine;

namespace AmplifyBloom
{
	[Serializable]
	public class AmplifyStarlineCache
	{
		public AmplifyStarlineCache()
		{
			this.Passes = new AmplifyPassCache[4];
			for (int i = 0; i < 4; i++)
			{
				this.Passes[i] = new AmplifyPassCache();
			}
		}

		public void Destroy()
		{
			for (int i = 0; i < 4; i++)
			{
				this.Passes[i].Destroy();
			}
			this.Passes = null;
		}

		[SerializeField]
		internal AmplifyPassCache[] Passes;
	}
}
