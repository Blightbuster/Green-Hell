using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class TransformModule : AIModule
	{
		public override void Initialize(Being being)
		{
			base.Initialize(being);
			this.m_AI.m_BoxCollider = this.m_AI.GetComponent<BoxCollider>();
			this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("NoCollisionWithPlayer"));
			this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("AI"));
			this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("SmallPlant"));
			this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("Player"));
			this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("Item"));
			this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("Water"));
			this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("CollisionsOnlyWithPlayer"));
		}

		private void OnAnimatorMove()
		{
			if (Time.deltaTime == 0f || !this.m_AI || (!this.m_AI.m_PathModule && !this.m_AI.IsKidRunner()))
			{
				return;
			}
			base.transform.rotation *= this.m_AI.m_Animator.deltaRotation;
			Vector3 vector = this.m_AI.m_Animator.rootPosition;
			if (!this.m_AI.IsKidRunner() && (this.m_AI.m_Params.m_Human || this.m_AI.IsCat()) && this.m_AI.m_Animator.deltaPosition.magnitude > 0.01f)
			{
				Vector3 vector2 = Player.Get().transform.TransformPoint(Player.Get().m_CharacterController.center);
				Vector3 vector3 = vector;
				vector3.y = vector2.y;
				float num = this.m_AI.m_Radius + Player.Get().m_Radius + this.m_AdditionalDistToPlayer;
				if (vector3.Distance(vector2) <= num)
				{
					vector = vector2 + (vector3 - vector2).normalized * num;
				}
			}
			base.transform.position = vector;
			if (this.m_AI.m_PathModule)
			{
				this.m_AI.m_PathModule.m_Agent.nextPosition = vector;
			}
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdateDirection();
		}

		private void UpdateDirection()
		{
			if (this.m_WantedDirection == Vector3.zero)
			{
				return;
			}
			float num = 1f;
			if (this.m_AI.m_MoveStyle == AIMoveStyle.Walk)
			{
				num = this.m_AI.m_Params.m_WalkAddRotPower;
			}
			else if (this.m_AI.m_MoveStyle == AIMoveStyle.Trot)
			{
				num = this.m_AI.m_Params.m_TrotAddRotPower;
			}
			else if (this.m_AI.m_MoveStyle == AIMoveStyle.Run)
			{
				num = this.m_AI.m_Params.m_RunAddRotPower;
			}
			else if (this.m_AI.m_MoveStyle == AIMoveStyle.Swim)
			{
				num = this.m_AI.m_Params.m_SwimAddRotPower;
			}
			else if (this.m_AI.m_MoveStyle == AIMoveStyle.Sneak)
			{
				num = this.m_AI.m_Params.m_WalkAddRotPower;
			}
			if (num > 0f)
			{
				this.m_AI.transform.rotation = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(this.m_WantedDirection), num * Time.deltaTime);
			}
			this.m_WantedDirection = Vector3.zero;
		}

		public override void OnLateUpdate()
		{
			base.OnLateUpdate();
			if (this.m_AI.IsSwimming())
			{
				this.UpdateSwimming();
				if (this.m_AI.m_GoalsModule.m_ActiveGoal != null && this.m_AI.m_GoalsModule.m_ActiveGoal.m_Type == AIGoalType.CaimanAttack)
				{
					Vector3 normalized = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).normalized;
					Vector3 upwards = Vector3.Cross(normalized, this.m_AI.transform.right);
					this.m_AI.transform.rotation = Quaternion.Slerp(this.m_LastRotation, Quaternion.LookRotation(normalized, upwards), 4f * Time.deltaTime);
				}
			}
			else
			{
				this.AllignToTerrain(false);
			}
			this.m_LastPosition = base.transform.position;
			this.m_LastRotation = base.transform.rotation;
		}

		private void UpdateSwimming()
		{
			this.m_SwimmingLastTime = Time.time;
			this.AllignToWaterNavmesh(true);
			this.AllignToTerrain(true);
			Vector3 rhs = this.m_TempTerrainPosFront - (this.m_TempSwimmingPos + Vector3.up * 0.3f);
			rhs.Normalize();
			Vector3 forward = base.transform.forward;
			forward.y = 0f;
			forward.Normalize();
			float b = Vector3.Dot(forward, rhs);
			float num = CJTools.Math.GetProportionalClamp(0f, 1f, b, 1f, 0.96f);
			if (this.m_TempTerrainPosFront.y < this.m_TempSwimmingPos.y)
			{
				num = 0f;
			}
			Vector3 tempSwimmingPos = this.m_TempSwimmingPos;
			tempSwimmingPos.y += CJTools.Math.GetProportionalClamp(0f, -0.8f, num, 0f, 1f);
			base.transform.position = tempSwimmingPos;
			base.transform.rotation = Quaternion.Slerp(this.m_TempSwimmingRot, this.m_TempTerrainRot, num);
		}

		private void AllignToWaterNavmesh(bool use_temp_transform)
		{
			NavMeshHit navMeshHit;
			if (!NavMesh.SamplePosition(this.m_AI.m_SwimmingPos, out navMeshHit, 1f, AIManager.s_WaterAreaMask))
			{
				return;
			}
			Vector3 normalized2D = (this.m_AI.m_PathModule.m_Agent.steeringTarget - this.m_AI.m_PathModule.m_Agent.nextPosition).GetNormalized2D();
			if (use_temp_transform)
			{
				this.m_TempSwimmingRot = Quaternion.Slerp(this.m_LastRotation, Quaternion.LookRotation(normalized2D, Vector3.up), 2f * Time.deltaTime);
			}
			else
			{
				this.m_AI.transform.rotation = Quaternion.Slerp(this.m_LastRotation, Quaternion.LookRotation(normalized2D, Vector3.up), 2f * Time.deltaTime);
			}
			Vector3 vector = this.m_AI.transform.position;
			float num = navMeshHit.position.y - this.m_AI.m_BoxCollider.size.y;
			vector.y = this.m_LastPosition.y * 0.1f + num * 0.9f;
			if (this.m_AI.m_GoalsModule.m_ActiveGoal == null || this.m_AI.m_GoalsModule.m_ActiveGoal.m_Type != AIGoalType.CaimanAttack)
			{
				vector += this.m_AI.transform.forward.GetNormalized2D() * 1.5f * Time.deltaTime;
			}
			if (use_temp_transform)
			{
				this.m_TempSwimmingPos = vector;
				return;
			}
			this.m_AI.transform.position = vector;
		}

		private void AllignToTerrain(bool use_temp_transform)
		{
			Vector3 position = this.m_AI.transform.position;
			bool autoSyncTransforms = Physics.autoSyncTransforms;
			Physics.autoSyncTransforms = false;
			this.GetRaycastHit(position + Vector3.up * this.m_AI.m_BoxCollider.size.y);
			position.y = Mathf.Max(TransformModule.s_RaycastHit.point.y, MainLevel.GetTerrainY(position));
			this.m_TempTerrainPos = position;
			if (Time.time - this.m_SwimmingLastTime < 1f)
			{
				float num = (Time.time - this.m_SwimmingLastTime) / 1f;
				this.m_AI.transform.position = this.m_LastPosition * (1f - num) + position * num;
			}
			else
			{
				this.m_AI.transform.position = position;
			}
			if (this.m_AI.m_Params.m_Human)
			{
				Physics.autoSyncTransforms = autoSyncTransforms;
				return;
			}
			if (this.m_AI.IsVisible())
			{
				Vector3 normalized2D = this.m_AI.transform.forward.GetNormalized2D();
				Vector3 vector = this.m_AI.m_BoxCollider.bounds.center + normalized2D * this.m_AI.m_BoxCollider.size.z + Vector3.up * this.m_AI.m_BoxCollider.size.y;
				this.GetRaycastHit(vector);
				vector = TransformModule.s_RaycastHit.point;
				if (use_temp_transform)
				{
					this.m_TempTerrainPosFront = vector;
				}
				Vector3 vector2 = this.m_AI.m_BoxCollider.bounds.center - normalized2D * this.m_AI.m_BoxCollider.size.z + Vector3.up * this.m_AI.m_BoxCollider.size.y;
				this.GetRaycastHit(vector2);
				vector2 = TransformModule.s_RaycastHit.point;
				Vector3 lhs = vector - vector2;
				Vector3 normalized2D2 = this.m_AI.transform.right.GetNormalized2D();
				Vector3 vector3 = Vector3.zero;
				if (this.m_AI.m_Params.m_AllignToTerrainFull)
				{
					Vector3 vector4 = this.m_AI.m_BoxCollider.bounds.center + normalized2D2 * this.m_AI.m_BoxCollider.size.x + Vector3.up * this.m_AI.m_BoxCollider.size.y;
					this.GetRaycastHit(vector4);
					vector4 = TransformModule.s_RaycastHit.point;
					Vector3 vector5 = this.m_AI.m_BoxCollider.bounds.center - normalized2D2 * this.m_AI.m_BoxCollider.size.x + Vector3.up * this.m_AI.m_BoxCollider.size.y;
					this.GetRaycastHit(vector5);
					vector5 = TransformModule.s_RaycastHit.point;
					Vector3 vector6 = vector4 - vector5;
					vector3 = Vector3.Cross(lhs.normalized, vector6.normalized);
				}
				else
				{
					vector3 = Vector3.Cross(lhs, normalized2D2);
				}
				if (!vector3.IsZero())
				{
					if (use_temp_transform)
					{
						this.m_TempTerrainRot = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(lhs.normalized, vector3.normalized), 1f);
					}
					else
					{
						this.m_AI.transform.rotation = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(lhs.normalized, vector3.normalized), 1f);
					}
				}
			}
			Physics.autoSyncTransforms = autoSyncTransforms;
		}

		private void GetRaycastHit(Vector3 pos)
		{
			float terrainY = MainLevel.GetTerrainY(pos);
			pos.y = Mathf.Max(pos.y, terrainY + 0.1f);
			int num = Physics.RaycastNonAlloc(pos, -Vector3.up, TransformModule.s_RaycastHitCache, pos.y - terrainY + 0.1f);
			this.m_RaycastOrig = pos;
			Array.Sort<RaycastHit>(TransformModule.s_RaycastHitCache, 0, num, TransformModule.s_DistComparer);
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = TransformModule.s_RaycastHitCache[i];
				if (!(raycastHit.collider.gameObject == base.gameObject) && !raycastHit.collider.isTrigger && !this.m_LayerMasksToIgnore.Contains(raycastHit.collider.gameObject.layer))
				{
					TransformModule.s_RaycastHit = raycastHit;
					return;
				}
			}
		}

		private Vector3 m_RaycastOrig = Vector3.zero;

		private List<int> m_LayerMasksToIgnore = new List<int>();

		[HideInInspector]
		public Vector3 m_WantedDirection = Vector3.zero;

		private static CompareListByDist s_DistComparer = new CompareListByDist();

		private float m_AdditionalDistToPlayer = 0.4f;

		private Vector3 m_LastPosition = Vector3.zero;

		private Quaternion m_LastRotation = Quaternion.identity;

		public Vector3 m_TempTerrainPos = Vector3.zero;

		private Vector3 m_TempTerrainPosFront = Vector3.zero;

		private Quaternion m_TempTerrainRot = Quaternion.identity;

		private Vector3 m_TempSwimmingPos = Vector3.zero;

		private Quaternion m_TempSwimmingRot = Quaternion.identity;

		private float m_SwimmingLastTime = float.MinValue;

		private static RaycastHit s_RaycastHit = default(RaycastHit);

		private static RaycastHit[] s_RaycastHitCache = new RaycastHit[20];
	}
}
