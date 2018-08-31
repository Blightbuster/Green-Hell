using System;
using UnityEngine;

public class WeightList
{
	public WeightList()
	{
		this.weights = new VertexWeight[4];
	}

	public Transform[] transforms = new Transform[4];

	public VertexWeight[] weights;

	public float rotation;
}
