using System;
using UnityEngine;

[AddComponentMenu("Relief Terrain/Helpers/Resolve Hole Collision for multiple child colliders")]
public class ResolveHoleCollisionMultiple : MonoBehaviour
{
	private void Awake()
	{
		this._rigidbody = base.GetComponent<Rigidbody>();
		for (int i = 0; i < this.entranceTriggers.Length; i++)
		{
			if (this.entranceTriggers[i] != null)
			{
				this.entranceTriggers[i].isTrigger = true;
			}
		}
		if (this._rigidbody != null && this.StartBelowGroundSurface)
		{
			for (int j = 0; j < this.terrainColliders.Length; j++)
			{
				for (int k = 0; k < this.childColliders.Length; k++)
				{
					if (this.terrainColliders[j] != null && this.childColliders[k] != null)
					{
						Physics.IgnoreCollision(this.childColliders[k], this.terrainColliders[j], true);
					}
				}
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		for (int i = 0; i < this.entranceTriggers.Length; i++)
		{
			if (this.entranceTriggers[i] == other)
			{
				for (int j = 0; j < this.terrainColliders.Length; j++)
				{
					for (int k = 0; k < this.childColliders.Length; k++)
					{
						if (this.childColliders[k] != null && this.terrainColliders[j] != null)
						{
							Physics.IgnoreCollision(this.childColliders[k], this.terrainColliders[j], true);
						}
					}
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if (this.terrainColliderForUpdate)
		{
			RaycastHit raycastHit = default(RaycastHit);
			if (this.terrainColliderForUpdate.Raycast(new Ray(base.transform.position + Vector3.up * this.checkOffset, Vector3.down), out raycastHit, float.PositiveInfinity))
			{
				for (int i = 0; i < this.terrainColliders.Length; i++)
				{
					for (int j = 0; j < this.childColliders.Length; j++)
					{
						if (this.childColliders[j] != null && this.terrainColliders[i] != null)
						{
							Physics.IgnoreCollision(this.childColliders[j], this.terrainColliders[i], false);
						}
					}
				}
			}
			this.terrainColliderForUpdate = null;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		for (int i = 0; i < this.entranceTriggers.Length; i++)
		{
			if (this.entranceTriggers[i] == other)
			{
				for (int j = 0; j < this.terrainColliders.Length; j++)
				{
					for (int k = 0; k < this.childColliders.Length; k++)
					{
						if (this.childColliders[k] != null && this.terrainColliders[j] != null)
						{
							Physics.IgnoreCollision(this.childColliders[k], this.terrainColliders[j], false);
						}
					}
				}
			}
		}
	}

	public Collider[] childColliders;

	public Collider[] entranceTriggers;

	public TerrainCollider[] terrainColliders;

	public float checkOffset = 1f;

	public bool StartBelowGroundSurface;

	private TerrainCollider terrainColliderForUpdate;

	private Rigidbody _rigidbody;
}
