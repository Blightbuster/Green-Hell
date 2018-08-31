using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Legacy
{
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_legacy_1_1_legacy_a_i_path.php")]
	[RequireComponent(typeof(Seeker))]
	[AddComponentMenu("Pathfinding/Legacy/AI/Legacy AIPath (3D)")]
	public class LegacyAIPath : AIPath
	{
		protected override void Awake()
		{
			base.Awake();
			if (this.rvoController != null)
			{
				if (this.rvoController is LegacyRVOController)
				{
					(this.rvoController as LegacyRVOController).enableRotation = false;
				}
				else
				{
					Debug.LogError("The LegacyAIPath component only works with the legacy RVOController, not the latest one. Please upgrade this component", this);
				}
			}
		}

		public override void OnPathComplete(Path _p)
		{
			ABPath abpath = _p as ABPath;
			if (abpath == null)
			{
				throw new Exception("This function only handles ABPaths, do not use special path types");
			}
			this.canSearchAgain = true;
			abpath.Claim(this);
			if (abpath.error)
			{
				abpath.Release(this, false);
				return;
			}
			if (this.path != null)
			{
				this.path.Release(this, false);
			}
			this.path = abpath;
			this.currentWaypointIndex = 0;
			base.TargetReached = false;
			if (this.closestOnPathCheck)
			{
				Vector3 vector = (Time.time - this.lastFoundWaypointTime >= 0.3f) ? abpath.originalStartPoint : this.lastFoundWaypointPosition;
				Vector3 feetPosition = this.GetFeetPosition();
				Vector3 vector2 = feetPosition - vector;
				float magnitude = vector2.magnitude;
				vector2 /= magnitude;
				int num = (int)(magnitude / this.pickNextWaypointDist);
				for (int i = 0; i <= num; i++)
				{
					this.CalculateVelocity(vector);
					vector += vector2;
				}
			}
		}

		protected override void Update()
		{
			if (!this.canMove)
			{
				return;
			}
			Vector3 vector = this.CalculateVelocity(this.GetFeetPosition());
			this.RotateTowards(this.targetDirection);
			if (this.rvoController != null)
			{
				this.rvoController.Move(vector);
			}
			else if (this.controller != null)
			{
				this.controller.SimpleMove(vector);
			}
			else if (this.rigid != null)
			{
				this.rigid.AddForce(vector);
			}
			else
			{
				this.tr.Translate(vector * Time.deltaTime, Space.World);
			}
		}

		protected float XZSqrMagnitude(Vector3 a, Vector3 b)
		{
			float num = b.x - a.x;
			float num2 = b.z - a.z;
			return num * num + num2 * num2;
		}

		protected new Vector3 CalculateVelocity(Vector3 currentPosition)
		{
			if (this.path == null || this.path.vectorPath == null || this.path.vectorPath.Count == 0)
			{
				return Vector3.zero;
			}
			List<Vector3> vectorPath = this.path.vectorPath;
			if (vectorPath.Count == 1)
			{
				vectorPath.Insert(0, currentPosition);
			}
			if (this.currentWaypointIndex >= vectorPath.Count)
			{
				this.currentWaypointIndex = vectorPath.Count - 1;
			}
			if (this.currentWaypointIndex <= 1)
			{
				this.currentWaypointIndex = 1;
			}
			while (this.currentWaypointIndex < vectorPath.Count - 1)
			{
				float num = this.XZSqrMagnitude(vectorPath[this.currentWaypointIndex], currentPosition);
				if (num < this.pickNextWaypointDist * this.pickNextWaypointDist)
				{
					this.lastFoundWaypointPosition = currentPosition;
					this.lastFoundWaypointTime = Time.time;
					this.currentWaypointIndex++;
				}
				else
				{
					IL_FB:
					Vector3 vector = vectorPath[this.currentWaypointIndex] - vectorPath[this.currentWaypointIndex - 1];
					Vector3 vector2 = this.CalculateTargetPoint(currentPosition, vectorPath[this.currentWaypointIndex - 1], vectorPath[this.currentWaypointIndex]);
					vector = vector2 - currentPosition;
					vector.y = 0f;
					float magnitude = vector.magnitude;
					float num2 = Mathf.Clamp01(magnitude / this.slowdownDistance);
					this.targetDirection = vector;
					this.targetPoint = vector2;
					if (this.currentWaypointIndex == vectorPath.Count - 1 && magnitude <= this.endReachedDistance)
					{
						if (!base.TargetReached)
						{
							base.TargetReached = true;
							this.OnTargetReached();
						}
						return Vector3.zero;
					}
					Vector3 forward = this.tr.forward;
					float a = Vector3.Dot(vector.normalized, forward);
					float num3 = this.speed * Mathf.Max(a, this.minMoveScale) * num2;
					if (Time.deltaTime > 0f)
					{
						num3 = Mathf.Clamp(num3, 0f, magnitude / (Time.deltaTime * 2f));
					}
					return forward * num3;
				}
			}
			goto IL_FB;
		}

		protected void RotateTowards(Vector3 dir)
		{
			if (dir == Vector3.zero)
			{
				return;
			}
			Quaternion quaternion = this.tr.rotation;
			Quaternion b = Quaternion.LookRotation(dir);
			Vector3 eulerAngles = Quaternion.Slerp(quaternion, b, base.turningSpeed * Time.deltaTime).eulerAngles;
			eulerAngles.z = 0f;
			eulerAngles.x = 0f;
			quaternion = Quaternion.Euler(eulerAngles);
			this.tr.rotation = quaternion;
		}

		protected Vector3 CalculateTargetPoint(Vector3 p, Vector3 a, Vector3 b)
		{
			a.y = p.y;
			b.y = p.y;
			float magnitude = (a - b).magnitude;
			if (magnitude == 0f)
			{
				return a;
			}
			float num = Mathf.Clamp01(VectorMath.ClosestPointOnLineFactor(a, b, p));
			Vector3 a2 = (b - a) * num + a;
			float magnitude2 = (a2 - p).magnitude;
			float num2 = Mathf.Clamp(this.forwardLook - magnitude2, 0f, this.forwardLook);
			float num3 = num2 / magnitude;
			num3 = Mathf.Clamp(num3 + num, 0f, 1f);
			return (b - a) * num3 + a;
		}

		public float forwardLook = 1f;

		public bool closestOnPathCheck = true;

		protected float minMoveScale = 0.05f;

		protected int currentWaypointIndex;

		protected Vector3 lastFoundWaypointPosition;

		protected float lastFoundWaypointTime = -9999f;

		protected new Vector3 targetDirection;
	}
}
