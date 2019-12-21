using System;
using System.Collections.Generic;
using UnityEngine;

namespace IESLights.Demo
{
	public class ToggleCameraPosition : MonoBehaviour
	{
		private void Start()
		{
			base.transform.position = this.Positions[this._positionIndex].position;
			base.transform.rotation = this.Positions[this._positionIndex].rotation;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				this._positionIndex++;
				this._positionIndex %= this.Positions.Count;
			}
			base.transform.position = this.Positions[this._positionIndex].position;
			base.transform.rotation = this.Positions[this._positionIndex].rotation;
		}

		public List<Transform> Positions;

		private int _positionIndex;
	}
}
