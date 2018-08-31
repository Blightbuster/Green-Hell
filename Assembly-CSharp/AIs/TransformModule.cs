using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace AIs
{
	public class TransformModule : AIModule
	{
		public override void Initialize()
		{
			base.Initialize();
			this.m_AI.m_BoxCollider = this.m_AI.GetComponent<BoxCollider>();
			this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("NoCollisionWithPlayer"));
			this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("AI"));
			this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("SmallPlant"));
			this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("Player"));
			this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("Item"));
			this.m_LayerMasksToIgnore.Add(LayerMask.NameToLayer("Water"));
		}

		private void OnAnimatorMove()
		{
			if (Time.deltaTime == 0f || !this.m_AI)
			{
				return;
			}
			base.transform.rotation *= this.m_AI.m_Animator.deltaRotation;
			Vector3 vector = this.m_AI.m_Animator.rootPosition;
			if ((this.m_AI.m_Params.m_Human || this.m_AI.m_ID == AI.AIID.Jaguar) && this.m_AI.m_Animator.deltaPosition.magnitude > 0.01f)
			{
				Vector3 vector2 = Player.Get().transform.TransformPoint(Player.Get().m_CharacterController.center);
				Vector3 vector3 = vector;
				vector3.y = vector2.y;
				float num = this.m_AI.m_Radius + Player.Get().m_Radius + this.m_AdditionalDistToPlayer;
				float num2 = vector3.Distance(vector2);
				if (num2 <= num)
				{
					Vector3 vector4 = vector3 - vector2;
					vector = vector2 + vector4.normalized * num;
				}
			}
			base.transform.position = vector;
			this.m_AI.m_PathModule.m_Agent.nextPosition = vector;
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
			this.AllignToTerrain();
		}

		private void AllignToTerrain()
		{
			Vector3 position = this.m_AI.transform.position;
			this.GetRaycastHit(position + Vector3.up * this.m_AI.m_BoxCollider.size.y);
			position.y = Mathf.Max(TransformModule.s_RaycastHit.point.y, MainLevel.GetTerrainY(position));
			this.m_AI.transform.position = position;
			if (this.m_AI.m_Params.m_Human)
			{
				return;
			}
			if (this.m_AI.m_Visible)
			{
				Vector3 normalized2D = this.m_AI.transform.forward.GetNormalized2D();
				Vector3 vector = this.m_AI.m_BoxCollider.bounds.center + normalized2D * this.m_AI.m_BoxCollider.size.z + Vector3.up * this.m_AI.m_BoxCollider.size.y;
				this.GetRaycastHit(vector);
				vector = TransformModule.s_RaycastHit.point;
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
					this.m_AI.transform.rotation = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(lhs.normalized, vector3.normalized), 1f);
				}
			}
			this.m_AI.m_BoxCollider.enabled = true;
		}

		private void GetRaycastHit(Vector3 pos)
		{
			pos.y = Mathf.Max(pos.y, MainLevel.GetTerrainY(pos) + 0.1f);
			int num = Physics.RaycastNonAlloc(pos, -Vector3.up, TransformModule.s_RaycastHitCache);
			this.m_RaycastOrig = pos;
			Array.Sort<RaycastHit>(TransformModule.s_RaycastHitCache, 0, num, TransformModule.s_DistComparer);
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = TransformModule.s_RaycastHitCache[i];
				if (!(raycastHit.collider.gameObject == base.gameObject))
				{
					if (!raycastHit.collider.isTrigger)
					{
						if (!this.m_LayerMasksToIgnore.Contains(raycastHit.collider.gameObject.layer))
						{
							TransformModule.s_RaycastHit = raycastHit;
							break;
						}
					}
				}
			}
		}

		private Vector3 m_RaycastOrig = Vector3.zero;

		private List<int> m_LayerMasksToIgnore = new List<int>();

		[HideInInspector]
		public Vector3 m_WantedDirection = Vector3.zero;

		private static CompareListByDist s_DistComparer = new CompareListByDist();

		private float m_AdditionalDistToPlayer = 0.4f;

		private static RaycastHit s_RaycastHit = default(RaycastHit);

		private static RaycastHit[] s_RaycastHitCache = new RaycastHit[20];
	}
}
