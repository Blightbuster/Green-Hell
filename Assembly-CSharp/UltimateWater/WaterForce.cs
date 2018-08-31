using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateWater
{
	[AddComponentMenu("Ultimate Water/Dynamic/Water Force")]
	public sealed class WaterForce : MonoBehaviour
	{
		private void FixedUpdate()
		{
			WaterForce.Data item;
			item.Position = base.transform.position;
			item.Force = this.Force * Time.fixedDeltaTime;
			WaterForce._ForceData.Clear();
			WaterForce._ForceData.Add(item);
			WaterRipples.AddForce(WaterForce._ForceData, this.Radius);
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green * 0.8f + Color.gray * 0.2f;
			Gizmos.DrawLine(base.transform.position + Vector3.up * 2f, base.transform.position - Vector3.up * 2f);
			Gizmos.DrawLine(base.transform.position + (Vector3.forward + Vector3.left), base.transform.position - (Vector3.forward + Vector3.left));
			Gizmos.DrawLine(base.transform.position + (Vector3.forward - Vector3.left), base.transform.position - (Vector3.forward - Vector3.left));
		}

		[Tooltip("Force affecting water surface")]
		public float Force = 0.01f;

		[Tooltip("Area of water displacement")]
		public float Radius = 1f;

		private static readonly List<WaterForce.Data> _ForceData = new List<WaterForce.Data>(1);

		public struct Data
		{
			public Vector3 Position;

			public float Force;
		}
	}
}
