using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class DestroyableChunkSource : MonoBehaviour
{
	private void Start()
	{
		if (this.m_RootChunks.Count > 0)
		{
			this.m_IKTargetPos = this.m_RootChunks[0].transform.position;
			return;
		}
		DebugUtils.Assert(DebugUtils.AssertType.Info);
	}

	public void Hit(Vector3 pos, Vector3 dir, Item item)
	{
		float num = 0.3f;
		if (item.m_DamagerStart && item.m_DamagerEnd)
		{
			Vector3 position = item.m_DamagerStart.transform.position;
			Vector3 position2 = item.m_DamagerEnd.transform.position;
			if (CJTools.Math.ProjectPointOnSegment(item.m_DamagerStart.transform.position, item.m_DamagerEnd.transform.position, this.m_IKTargetPos).Distance2D(this.m_IKTargetPos) > num)
			{
				return;
			}
		}
		else if (pos.Distance2D(this.m_IKTargetPos) > num)
		{
			return;
		}
		if (Mathf.Abs(dir.y) < 0.5f)
		{
			this.m_HitDir = Quaternion.Euler(0f, 30f, 0f) * dir;
		}
		else
		{
			this.m_HitDir = dir;
		}
		this.m_HitDir.y = 0f;
		this.m_HitDir.Normalize();
		if (this.m_MultipleChunks)
		{
			this.FindChunksToDeactivate(pos);
			for (int i = 0; i < this.m_ChunksToDeactivate.Count; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_ChunksToDeactivate[i], this.m_ChunksToDeactivate[i].transform.position, this.m_ChunksToDeactivate[i].transform.rotation);
				gameObject.AddComponent<TinyPhysicsObject>();
				gameObject.AddComponent<Rigidbody>().velocity = -this.m_HitDir * UnityEngine.Random.Range(0.4f, 0.7f);
				Physics.IgnoreCollision(gameObject.AddComponent<BoxCollider>(), Player.Get().m_Collider);
				this.m_ChunksToDeactivate[i].SetActive(false);
			}
			return;
		}
		this.FindChunkToDeactivate(pos);
		if (this.m_ClosestChunk == null)
		{
			return;
		}
		this.m_ClosestChunk.SetActive(false);
	}

	private void FindChunkToDeactivate(Vector3 pos)
	{
		this.m_ClosestChunk = null;
		this.m_ClosestChunkDot = float.MaxValue;
		for (int i = 0; i < this.m_RootChunks.Count; i++)
		{
			this.GetDeepestActive(this.m_RootChunks[i], pos);
		}
	}

	private void GetDeepestActive(GameObject go, Vector3 pos)
	{
		if (go.transform.childCount < 1)
		{
			if (Vector3.Dot(this.m_HitDir, go.transform.right) < this.m_ClosestChunkDot)
			{
				this.m_ClosestChunk = go;
				this.m_ClosestChunkDot = Vector3.Dot(this.m_HitDir, go.transform.right);
			}
			return;
		}
		bool flag = true;
		for (int i = 0; i < go.transform.childCount; i++)
		{
			if (go.transform.GetChild(i).gameObject.activeSelf)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			if (Vector3.Dot(this.m_HitDir, go.transform.right) < this.m_ClosestChunkDot)
			{
				this.m_ClosestChunk = go;
				this.m_ClosestChunkDot = Vector3.Dot(this.m_HitDir, go.transform.right);
			}
			return;
		}
		for (int j = 0; j < go.transform.childCount; j++)
		{
			this.GetDeepestActive(go.transform.GetChild(j).gameObject, pos);
		}
	}

	private void FindChunksToDeactivate(Vector3 pos)
	{
		this.m_ChunksToDeactivate.Clear();
		for (int i = 0; i < this.m_RootChunks.Count; i++)
		{
			this.AddDeepestActive(this.m_RootChunks[i], pos);
		}
	}

	private void AddDeepestActive(GameObject go, Vector3 pos)
	{
		if (go.transform.childCount < 1)
		{
			if (Vector3.Dot(this.m_HitDir, go.transform.right) < -0.6f && go.activeSelf)
			{
				this.m_ChunksToDeactivate.Add(go);
			}
			return;
		}
		bool flag = true;
		for (int i = 0; i < go.transform.childCount; i++)
		{
			if (go.transform.GetChild(i).gameObject.activeSelf)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			if (Vector3.Dot(this.m_HitDir, go.transform.right) < -0.6f && go.activeSelf)
			{
				this.m_ChunksToDeactivate.Add(go);
			}
			return;
		}
		for (int j = 0; j < go.transform.childCount; j++)
		{
			this.AddDeepestActive(go.transform.GetChild(j).gameObject, pos);
		}
	}

	public List<GameObject> m_RootChunks = new List<GameObject>();

	private GameObject m_ClosestChunk;

	private float m_ClosestChunkDot = float.MaxValue;

	private Vector3 m_HitDir = Vector3.right;

	private bool m_MultipleChunks = true;

	private List<GameObject> m_ChunksToDeactivate = new List<GameObject>();

	private const float m_MaxDot = -0.6f;

	[HideInInspector]
	public Vector3 m_IKTargetPos = Vector3.zero;
}
