using System;
using UnityEngine;

public class VertexWeight
{
	public VertexWeight(int i, Vector3 p, Vector3 n, Vector4 tangent, float w)
	{
		this.index = i;
		this.localPosition = p;
		this.localNormal = n;
		this.localTangent = tangent;
		this.weight = w;
	}

	public int index;

	public Vector3 localPosition;

	public Vector3 localNormal;

	public Vector4 localTangent;

	public float weight;
}
