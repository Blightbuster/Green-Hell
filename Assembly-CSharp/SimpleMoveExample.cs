using System;
using UnityEngine;

public class SimpleMoveExample : MonoBehaviour
{
	private void Start()
	{
		this.m_originalPosition = base.transform.position;
		this.m_previous = base.transform.position;
		this.m_target = base.transform.position;
	}

	private void Update()
	{
		base.transform.position = Vector3.Slerp(this.m_previous, this.m_target, Time.deltaTime * this.Speed);
		this.m_previous = base.transform.position;
		if (Vector3.Distance(this.m_target, base.transform.position) < 0.1f)
		{
			this.m_target = base.transform.position + UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(0.7f, 4f);
			this.m_target.Set(Mathf.Clamp(this.m_target.x, this.m_originalPosition.x - this.BoundingVolume.x, this.m_originalPosition.x + this.BoundingVolume.x), Mathf.Clamp(this.m_target.y, this.m_originalPosition.y - this.BoundingVolume.y, this.m_originalPosition.y + this.BoundingVolume.y), Mathf.Clamp(this.m_target.z, this.m_originalPosition.z - this.BoundingVolume.z, this.m_originalPosition.z + this.BoundingVolume.z));
		}
	}

	private Vector3 m_previous;

	private Vector3 m_target;

	private Vector3 m_originalPosition;

	public Vector3 BoundingVolume = new Vector3(3f, 1f, 3f);

	public float Speed = 10f;
}
