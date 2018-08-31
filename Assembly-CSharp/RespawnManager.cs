using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
	private RespawnManager()
	{
		RespawnManager.s_Instance = this;
	}

	public static RespawnManager Get()
	{
		return RespawnManager.s_Instance;
	}

	public void RegisterRespawnPoint(RespawnPoint point)
	{
		this.m_RespawnPoints.Add(point);
	}

	public void UnregisterRespawnPoint(RespawnPoint point)
	{
		this.m_RespawnPoints.Remove(point);
	}

	public RespawnPoint GetBestRespawnPoint(Vector3 pos)
	{
		if (this.m_RespawnPoints.Count <= 0)
		{
			return null;
		}
		float num = float.MaxValue;
		RespawnPoint respawnPoint = null;
		for (int i = 0; i < this.m_RespawnPoints.Count; i++)
		{
			float num2 = Vector3.Distance(pos, this.m_RespawnPoints[i].transform.position);
			if (num2 < num)
			{
				num = num2;
				respawnPoint = this.m_RespawnPoints[i];
			}
		}
		return (!respawnPoint) ? this.m_RespawnPoints[UnityEngine.Random.Range(0, this.m_RespawnPoints.Count)] : respawnPoint;
	}

	public void Respawn(Being obj)
	{
		if (!obj)
		{
			return;
		}
		RespawnPoint bestRespawnPoint = this.GetBestRespawnPoint(obj.transform.position);
		DebugUtils.Assert(bestRespawnPoint, true);
		if (bestRespawnPoint)
		{
			obj.transform.position = bestRespawnPoint.transform.position;
		}
		Collider componentDeepChild = General.GetComponentDeepChild<Collider>(obj.gameObject);
		Vector3 position = obj.transform.position;
		position.y += ((!componentDeepChild) ? 0f : componentDeepChild.bounds.size.y);
		RaycastHit raycastHit;
		if (Physics.Raycast(position, Vector3.down, out raycastHit))
		{
			obj.transform.position = raycastHit.point;
		}
		if (obj.IsPlayer())
		{
			Vector3 vector = (!bestRespawnPoint) ? Vector3.forward : bestRespawnPoint.transform.forward;
			vector.y = 0f;
			float num = Vector3.Angle(vector, Vector3.forward);
			Vector2 zero = Vector2.zero;
			if (Vector3.Dot(Vector3.right, vector) < 0f)
			{
				num *= -1f;
			}
			zero.x = num;
			FPPController fppcontroller = Player.Get().m_FPPController;
			fppcontroller.SetLookDev(zero);
			Vector3 position2 = obj.transform.position;
			position2.y += Player.Get().GetComponent<CharacterController>().height * 0.5f;
			obj.transform.position = position2;
		}
	}

	private List<RespawnPoint> m_RespawnPoints = new List<RespawnPoint>();

	private static RespawnManager s_Instance;
}
