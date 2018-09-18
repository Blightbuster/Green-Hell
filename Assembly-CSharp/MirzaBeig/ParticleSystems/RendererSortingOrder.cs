using System;
using UnityEngine;

namespace MirzaBeig.ParticleSystems
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Renderer))]
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
