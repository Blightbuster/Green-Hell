using System;
using AIs;
using CJTools;
using UnityEngine;
using UnityEngine.AI;

public class StalkerManager : MonoBehaviour
{
	public static StalkerManager Get()
	{
		return StalkerManager.s_Instance;
	}

	private void Awake()
	{
		StalkerManager.s_Instance = this;
		this.m_TempPath = new NavMeshPath();
	}

	private void Update()
	{
		if (this.ShouldDestroyStalker())
		{
			UnityEngine.Object.Destroy(this.m_Stalker.gameObject);
			this.m_Stalker = null;
		}
		if (this.CanSpawnStalker())
		{
			this.SpawnStalker();
		}
	}

	private bool ShouldDestroyStalker()
	{
		return this.m_Stalker && Vector3.Distance(this.m_Stalker.transform.position, Player.Get().transform.position) >= this.m_MoveAwayRange && !this.m_Stalker.m_Visible;
	}

	private bool CanSpawnStalker()
	{
		return false;
	}

	private void SpawnStalker()
	{
		Vector3 forward = Camera.main.transform.forward;
		Vector3 vector = Player.Get().GetHeadTransform().position - forward * this.m_MoveAwayRange;
		vector.y = MainLevel.GetTerrainY(vector);
		NavMeshHit navMeshHit;
		if (NavMesh.SamplePosition(vector, out navMeshHit, this.m_MoveAwayRange / 3f, AIManager.s_WalkableAreaMask) && NavMesh.CalculatePath(vector, Player.Get().transform.position, AIManager.s_WalkableAreaMask, this.m_TempPath) && this.m_TempPath.status == NavMeshPathStatus.PathComplete)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_StalkerPrefab, vector, Player.Get().transform.rotation);
			this.m_Stalker = gameObject.GetComponent<Stalker>();
		}
	}

	public bool CanAttack()
	{
		float b = (float)PlayerSanityModule.Get().m_Sanity;
		float b2 = (float)PlayerSanityModule.Get().m_StalkerAttackSanityLevel;
		float proportionalClamp = CJTools.Math.GetProportionalClamp(this.m_AttackIntervalMin, this.m_AttackIntervalMax, b, b2, 1f);
		return Time.time - this.m_LastAttackTime >= proportionalClamp;
	}

	public void OnStalkerStartAttack()
	{
		this.m_LastAttackTime = Time.time;
	}

	public void OnStalkerDestroy()
	{
		this.m_Stalker = null;
		this.m_LastStalkerTime = Time.time;
	}

	public GameObject m_StalkerPrefab;

	private Stalker m_Stalker;

	public float m_MoveAroundMinRange = 4f;

	public float m_MoveAroundRange = 6f;

	public float m_MoveAwayRange = 10f;

	public float m_AttackIntervalMin = 6f;

	public float m_AttackIntervalMax = 36f;

	private float m_LastAttackTime;

	private float m_LastStalkerTime;

	private NavMeshPath m_TempPath;

	private static StalkerManager s_Instance;
}
