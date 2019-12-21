using System;
using UnityEngine;

public class PlayerLeavesPusherHelper : MonoBehaviour
{
	private void Update()
	{
		if (this.m_LastUpdateTime > Time.time - 0.5f)
		{
			return;
		}
		if (!this.ReplIsOwner() && Player.Get().transform.position.Distance(base.transform.position) < 30f)
		{
			LeavesPusher.Get().Push(base.gameObject, 0.5f, new Vector3?(Vector3.up * 0.5f));
		}
	}

	private float m_LastUpdateTime;

	private const float MAX_DISTANCE_TO_LOCAL_PLAYER = 30f;

	private const float UPDATE_INTERVAL = 0.5f;
}
