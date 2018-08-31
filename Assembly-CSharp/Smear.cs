using System;
using System.Collections.Generic;
using UnityEngine;

public class Smear : MonoBehaviour
{
	private Material InstancedMaterial
	{
		get
		{
			return this.m_instancedMaterial;
		}
		set
		{
			this.m_instancedMaterial = value;
		}
	}

	private void Start()
	{
		this.InstancedMaterial = this.Renderer.material;
	}

	private void LateUpdate()
	{
		if (this.m_recentPositions.Count > this.FramesBufferSize)
		{
			this.InstancedMaterial.SetVector("_PrevPosition", this.m_recentPositions.Dequeue());
		}
		this.InstancedMaterial.SetVector("_Position", base.transform.position);
		this.m_recentPositions.Enqueue(base.transform.position);
	}

	private Queue<Vector3> m_recentPositions = new Queue<Vector3>();

	public int FramesBufferSize;

	public Renderer Renderer;

	private Material m_instancedMaterial;
}
