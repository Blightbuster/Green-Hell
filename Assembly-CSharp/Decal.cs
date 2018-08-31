using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Decal : MonoBehaviour
{
	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}

	public Bounds GetBounds()
	{
		Vector3 lossyScale = base.transform.lossyScale;
		Vector3 vector = -lossyScale / 2f;
		Vector3 vector2 = lossyScale / 2f;
		Vector3[] array = new Vector3[]
		{
			new Vector3(vector.x, vector.y, vector.z),
			new Vector3(vector2.x, vector.y, vector.z),
			new Vector3(vector.x, vector2.y, vector.z),
			new Vector3(vector2.x, vector2.y, vector.z),
			new Vector3(vector.x, vector.y, vector2.z),
			new Vector3(vector2.x, vector.y, vector2.z),
			new Vector3(vector.x, vector2.y, vector2.z),
			new Vector3(vector2.x, vector2.y, vector2.z)
		};
		for (int i = 0; i < 8; i++)
		{
			array[i] = base.transform.TransformDirection(array[i]);
		}
		vector2 = (vector = array[0]);
		foreach (Vector3 rhs in array)
		{
			vector = Vector3.Min(vector, rhs);
			vector2 = Vector3.Max(vector2, rhs);
		}
		return new Bounds(base.transform.position, vector2 - vector);
	}

	public Material material;

	public Sprite sprite;

	public float maxAngle = 90f;

	public float pushDistance = 0.009f;

	public LayerMask affectedLayers = -1;
}
