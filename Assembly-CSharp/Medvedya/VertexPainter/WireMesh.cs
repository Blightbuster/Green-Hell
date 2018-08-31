using System;
using UnityEngine;

namespace Medvedya.VertexPainter
{
	[Serializable]
	public class WireMesh
	{
		public void Do(IPainting obj)
		{
			if (this.active)
			{
				Transform transform = obj.modifyInfo.transform;
				Gizmos.color = this.color;
				Gizmos.DrawWireMesh(obj.modifyInfo.vertexPainter.meshFilter.sharedMesh, transform.position, transform.rotation, transform.localScale);
			}
		}

		public Color color = new Color(0f, 0f, 1f, 1f);

		public bool active;
	}
}
