using System;
using UnityEngine;

namespace MirzaBeig.ParticleSystems
{
	[RequireComponent(typeof(Renderer))]
	[ExecuteInEditMode]
	public class RendererSortingOrder : MonoBehaviour
	{
		private void Awake()
		{
		}

		private void Start()
		{
			base.GetComponent<Renderer>().sortingOrder = this.sortingOrder;
		}

		private void Update()
		{
		}

		public int sortingOrder;
	}
}
