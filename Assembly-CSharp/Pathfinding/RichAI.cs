using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	[RequireComponent(typeof(Seeker))]
	[AddComponentMenu("Pathfinding/AI/RichAI (3D, for navmesh)")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_rich_a_i.php")]
	public class RichAI : AIBase
	{
		public Vector3 Velocity
		{
			get
			{
				return this.realVelocity;
			}
		}

		public bool TraversingSpecial
		{
			get
			{
				return this.traversingSpecialPath;
			}
		}

		public Vector3 NextWaypoint
		{
			get
			{
				return this.waypoint;
			}
		}

		public float DistanceToNextWaypoint
		{
			get
			{
				return this.distanceToWaypoint;
			}
		}

		public bool TargetReached
		{
			get
			{
				return this.ApproachingPathEndpoint && this.DistanceToNextWaypoint < this.endReachedDistance;
			}
		}

		public bool PathPending
		{
			get
			{
				return this.waitingForPathCalc || this.delayUpdatePath;
			}
		}

		public bool ApproachingPartEndpoint
		{
			get
			{
				return this.lastCorner && this.nextCorners.Count == 1;
			}
		}

		public bool ApproachingPathEndpoint
		{
			get
			{
				return this.ApproachingPartEndpoint && this.richPath.IsLastPart;
			}
		}

		[Obsolete("This property has been renamed to NextWaypoint")]
		public Vector3 TargetPoint
		{
			get
			{
				return this.NextWaypoint;
			}
		}

		protected virtual void Start()
		{
			this.startHasRun = true;
			this.Init();
		}

		protected virtual void OnEnable()
		{
			Seeker seeker = this.seeker;
			seeker.pathCallback = (OnPathDelegate)Delegate.Combine(seeker.pathCallback, new OnPathDelegate(this.OnPathComplete));
			this.Init();
		}

		private void Init()
		{
			if (this.prevPosition != this.tr.position)
			{
				this.Teleport(this.tr.position);
			}
			if (this.startHasRun)
			{
				this.lastRepath = float.NegativeInfinity;
				base.StartCoroutine(this.SearchPaths());
			}
		}

		public void Teleport(Vector3 newPosition)
		{
			this.CancelCurrentPathRequest();
			NNInfo nearest = AstarPath.active.GetNearest(newPosition);
			float elevation;
			this.movementPlane.ToPlane(newPosition, out elevation);
			this.prevPosition = (this.tr.position = this.movementPlane.ToWorld(this.movementPlane.ToPlane((nearest.node != null) ? nearest.position : newPosition), elevation));
			this.richPath.Clear();
			if (this.rvoController != null)
			{
				this.rvoController.Move(Vector3.zero);
			}
		}

		protected void CancelCurrentPathRequest()
		{
			this.canSearchPath = true;
			this.waitingForPathCalc = false;
			if (this.seeker != null)
			{
				this.seeker.CancelCurrentPathRequest(true);
			}
		}

		protected virtual void OnDisable()
		{
			this.CancelCurrentPathRequest();
			this.velocity2D = Vector3.zero;
			this.verticalVelocity = 0f;
			this.lastCorner = false;
			this.distanceToWaypoint = float.PositiveInfinity;
			Seeker seeker = this.seeker;
			seeker.pathCallback = (OnPathDelegate)Delegate.Remove(seeker.pathCallback, new OnPathDelegate(this.OnPathComplete));
		}

		public virtual void UpdatePath()
		{
			this.CancelCurrentPathRequest();
			this.waitingForPathCalc = true;
			this.lastRepath = Time.time;
			this.seeker.StartPath(this.tr.position, this.target.position);
		}

		private IEnumerator SearchPaths()
		{
			/*
An exception occurred when decompiling this method (06003046)

ICSharpCode.Decompiler.DecompilerException: Error decompiling System.Collections.IEnumerator Pathfinding.RichAI::SearchPaths()
 ---> System.ArgumentOutOfRangeException: Der Index lag außerhalb des Bereichs. Er darf nicht negativ und kleiner als die Sammlung sein.
Parametername: index
   bei System.ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument argument, ExceptionResource resource)
   bei ICSharpCode.Decompiler.ILAst.StateRangeAnalysis.CreateLabelRangeMapping(List`1 body, Int32 pos, Int32 bodyLength, LabelRangeMapping result, Boolean onlyInitialLabels) in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\ILAst\StateRange.cs:Zeile 326.
   bei ICSharpCode.Decompiler.ILAst.MicrosoftYieldReturnDecompiler.AnalyzeMoveNext() in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\ILAst\MicrosoftYieldReturnDecompiler.cs:Zeile 347.
   bei ICSharpCode.Decompiler.ILAst.YieldReturnDecompiler.Run() in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\ILAst\YieldReturnDecompiler.cs:Zeile 93.
   bei ICSharpCode.Decompiler.ILAst.YieldReturnDecompiler.Run(DecompilerContext context, ILBlock method, AutoPropertyProvider autoPropertyProvider, List`1 list_ILNode, Func`2 getILInlining, List`1 listExpr, List`1 listBlock, Dictionary`2 labelRefCount) in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\ILAst\YieldReturnDecompiler.cs:Zeile 69.
   bei ICSharpCode.Decompiler.ILAst.ILAstOptimizer.Optimize(DecompilerContext context, ILBlock method, AutoPropertyProvider autoPropertyProvider, ILAstOptimizationStep abortBeforeStep) in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\ILAst\ILAstOptimizer.cs:Zeile 233.
   bei ICSharpCode.Decompiler.Ast.AstMethodBodyBuilder.CreateMethodBody(IEnumerable`1 parameters, MethodDebugInfoBuilder& builder) in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\Ast\AstMethodBodyBuilder.cs:Zeile 118.
   bei ICSharpCode.Decompiler.Ast.AstMethodBodyBuilder.CreateMethodBody(MethodDef methodDef, DecompilerContext context, AutoPropertyProvider autoPropertyProvider, IEnumerable`1 parameters, Boolean valueParameterIsKeyword, StringBuilder sb, MethodDebugInfoBuilder& stmtsBuilder) in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\Ast\AstMethodBodyBuilder.cs:Zeile 88.
   --- Ende der internen Ausnahmestapelüberwachung ---
   bei ICSharpCode.Decompiler.Ast.AstMethodBodyBuilder.CreateMethodBody(MethodDef methodDef, DecompilerContext context, AutoPropertyProvider autoPropertyProvider, IEnumerable`1 parameters, Boolean valueParameterIsKeyword, StringBuilder sb, MethodDebugInfoBuilder& stmtsBuilder) in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\Ast\AstMethodBodyBuilder.cs:Zeile 92.
   bei ICSharpCode.Decompiler.Ast.AstBuilder.CreateMethodBody(MethodDef method, IEnumerable`1 parameters, Boolean valueParameterIsKeyword, MethodKind methodKind, MethodDebugInfoBuilder& builder) in C:\projects\dnspy\Extensions\ILSpy.Decompiler\ICSharpCode.Decompiler\ICSharpCode.Decompiler\Ast\AstBuilder.cs:Zeile 1427.
*/;
		}

		private void OnPathComplete(Path p)
		{
			this.waitingForPathCalc = false;
			p.Claim(this);
			if (p.error)
			{
				p.Release(this, false);
				return;
			}
			if (this.traversingSpecialPath)
			{
				this.delayUpdatePath = true;
			}
			else
			{
				this.richPath.Initialize(this.seeker, p, true, this.funnelSimplification);
				RichFunnel richFunnel = this.richPath.GetCurrentPart() as RichFunnel;
				if (richFunnel != null)
				{
					Vector2 b = this.movementPlane.ToPlane(this.UpdateTarget(richFunnel));
					if (this.lastCorner && this.nextCorners.Count == 1)
					{
						Vector2 a = this.waypoint = this.movementPlane.ToPlane(this.nextCorners[0]);
						this.distanceToWaypoint = (a - b).magnitude;
						if (this.distanceToWaypoint <= this.endReachedDistance)
						{
							this.NextPart();
						}
					}
				}
			}
			p.Release(this, false);
		}

		protected void NextPart()
		{
			if (!this.richPath.CompletedAllParts)
			{
				if (!this.richPath.IsLastPart)
				{
					this.lastCorner = false;
				}
				this.richPath.NextPart();
				if (this.richPath.CompletedAllParts)
				{
					this.OnTargetReached();
				}
			}
		}

		protected virtual void OnTargetReached()
		{
		}

		protected virtual Vector3 UpdateTarget(RichFunnel fn)
		{
			this.nextCorners.Clear();
			bool flag;
			Vector3 result = fn.Update(this.tr.position, this.nextCorners, 2, out this.lastCorner, out flag);
			if (flag && !this.waitingForPathCalc)
			{
				this.UpdatePath();
			}
			return result;
		}

		protected override void MovementUpdate(float deltaTime)
		{
			RichPathPart currentPart = this.richPath.GetCurrentPart();
			RichFunnel richFunnel = currentPart as RichFunnel;
			if (richFunnel != null)
			{
				this.TraverseFunnel(richFunnel, deltaTime);
			}
			else if (currentPart is RichSpecial)
			{
				if (!this.traversingSpecialPath)
				{
					base.StartCoroutine(this.TraverseSpecial(currentPart as RichSpecial));
				}
			}
			else
			{
				if (this.rvoController != null && this.rvoController.enabled)
				{
					this.rvoController.Move(Vector3.zero);
				}
				base.Move(this.tr.position, Vector3.zero);
			}
			Vector3 position = this.tr.position;
			this.realVelocity = ((deltaTime > 0f) ? ((position - this.prevPosition) / deltaTime) : Vector3.zero);
			this.prevPosition = position;
		}

		private void TraverseFunnel(RichFunnel fn, float deltaTime)
		{
			float elevation;
			Vector2 vector = this.movementPlane.ToPlane(this.UpdateTarget(fn), out elevation);
			if (Time.frameCount % 5 == 0 && this.wallForce > 0f && this.wallDist > 0f)
			{
				this.wallBuffer.Clear();
				fn.FindWalls(this.wallBuffer, this.wallDist);
			}
			Vector2 vector2 = this.waypoint = this.movementPlane.ToPlane(this.nextCorners[0]);
			Vector2 vector3 = vector2 - vector;
			object obj = this.lastCorner && this.nextCorners.Count == 1;
			Vector2 vector4 = VectorMath.Normalize(vector3, out this.distanceToWaypoint);
			Vector2 a = this.CalculateWallForce(vector, elevation, vector4);
			object obj2 = obj;
			Vector2 targetVelocity;
			if (obj2 != null)
			{
				targetVelocity = ((this.slowdownTime > 0f) ? Vector2.zero : (vector4 * this.maxSpeed));
				a *= Math.Min(this.distanceToWaypoint / 0.5f, 1f);
				if (this.distanceToWaypoint <= this.endReachedDistance)
				{
					this.NextPart();
				}
			}
			else
			{
				targetVelocity = (((this.nextCorners.Count > 1) ? this.movementPlane.ToPlane(this.nextCorners[1]) : (vector + 2f * vector3)) - vector2).normalized * this.maxSpeed;
			}
			Vector2 a2 = MovementUtilities.CalculateAccelerationToReachPoint(vector2 - vector, targetVelocity, this.velocity2D, this.acceleration, this.maxSpeed);
			this.velocity2D += (a2 + a * this.wallForce) * deltaTime;
			float distanceToEndOfPath = fn.DistanceToEndOfPath;
			float num = (this.slowdownTime > 0f) ? (distanceToEndOfPath / (this.maxSpeed * this.slowdownTime)) : 1f;
			this.velocity2D = MovementUtilities.ClampVelocity(this.velocity2D, this.maxSpeed, num, this.slowWhenNotFacingTarget, this.movementPlane.ToPlane(this.rotationIn2D ? this.tr.up : this.tr.forward));
			base.ApplyGravity(deltaTime);
			if (this.rvoController != null && this.rvoController.enabled)
			{
				Vector3 pos = this.movementPlane.ToWorld(vector + Vector2.ClampMagnitude(this.velocity2D, distanceToEndOfPath), elevation);
				this.rvoController.SetTarget(pos, this.velocity2D.magnitude, this.maxSpeed);
			}
			Vector2 vector5 = base.CalculateDeltaToMoveThisFrame(vector, distanceToEndOfPath, deltaTime);
			float num2 = (obj2 != null) ? Mathf.Clamp01(1.1f * num - 0.1f) : 1f;
			this.RotateTowards(vector5, this.rotationSpeed * num2 * deltaTime);
			base.Move(this.movementPlane.ToWorld(vector, elevation), this.movementPlane.ToWorld(vector5, this.verticalVelocity * deltaTime));
		}

		protected override Vector3 ClampToNavmesh(Vector3 position)
		{
			if (this.richPath != null)
			{
				RichFunnel richFunnel = this.richPath.GetCurrentPart() as RichFunnel;
				if (richFunnel != null)
				{
					return richFunnel.ClampToNavmesh(position);
				}
			}
			return position;
		}

		private Vector2 CalculateWallForce(Vector2 position, float elevation, Vector2 directionToTarget)
		{
			if (this.wallForce <= 0f || this.wallDist <= 0f)
			{
				return Vector2.zero;
			}
			float num = 0f;
			float num2 = 0f;
			Vector3 vector = this.movementPlane.ToWorld(position, elevation);
			for (int i = 0; i < this.wallBuffer.Count; i += 2)
			{
				float sqrMagnitude = (VectorMath.ClosestPointOnSegment(this.wallBuffer[i], this.wallBuffer[i + 1], vector) - vector).sqrMagnitude;
				if (sqrMagnitude <= this.wallDist * this.wallDist)
				{
					Vector2 normalized = this.movementPlane.ToPlane(this.wallBuffer[i + 1] - this.wallBuffer[i]).normalized;
					float num3 = Vector2.Dot(directionToTarget, normalized);
					float num4 = 1f - Math.Max(0f, 2f * (sqrMagnitude / (this.wallDist * this.wallDist)) - 1f);
					if (num3 > 0f)
					{
						num2 = Math.Max(num2, num3 * num4);
					}
					else
					{
						num = Math.Max(num, -num3 * num4);
					}
				}
			}
			return new Vector2(directionToTarget.y, -directionToTarget.x) * (num2 - num);
		}

		protected override void OnDrawGizmos()
		{
			base.OnDrawGizmos();
			if (this.tr != null)
			{
				Gizmos.color = RichAI.GizmoColorPath;
				Vector3 from = this.tr.position;
				for (int i = 0; i < this.nextCorners.Count; i++)
				{
					Gizmos.DrawLine(from, this.nextCorners[i]);
					from = this.nextCorners[i];
				}
			}
		}

		protected virtual IEnumerator TraverseSpecial(RichSpecial rs)
		{
			this.traversingSpecialPath = true;
			this.velocity2D = Vector3.zero;
			AnimationLink link = rs.nodeLink as AnimationLink;
			if (link == null)
			{
				Debug.LogError("Unhandled RichSpecial");
				yield break;
			}
			while (Vector2.Angle(this.movementPlane.ToPlane(this.rotationIn2D ? this.tr.up : this.tr.forward), this.movementPlane.ToPlane(rs.first.forward)) > 5f)
			{
				this.RotateTowards(this.movementPlane.ToPlane(rs.first.forward), this.rotationSpeed * Time.deltaTime);
				yield return null;
			}
			this.tr.parent.position = this.tr.position;
			this.tr.parent.rotation = this.tr.rotation;
			this.tr.localPosition = Vector3.zero;
			this.tr.localRotation = Quaternion.identity;
			if (rs.reverse && link.reverseAnim)
			{
				this.anim[link.clip].speed = -link.animSpeed;
				this.anim[link.clip].normalizedTime = 1f;
				this.anim.Play(link.clip);
				this.anim.Sample();
			}
			else
			{
				this.anim[link.clip].speed = link.animSpeed;
				this.anim.Rewind(link.clip);
				this.anim.Play(link.clip);
			}
			this.tr.parent.position -= this.tr.position - this.tr.parent.position;
			yield return new WaitForSeconds(Mathf.Abs(this.anim[link.clip].length / link.animSpeed));
			this.traversingSpecialPath = false;
			this.NextPart();
			if (this.delayUpdatePath)
			{
				this.delayUpdatePath = false;
				this.UpdatePath();
			}
			yield break;
			yield break;
		}

		public Transform target;

		public bool repeatedlySearchPaths;

		public float repathRate = 0.5f;

		public float maxSpeed = 1f;

		public float acceleration = 5f;

		public float rotationSpeed = 360f;

		public float slowdownTime = 0.5f;

		public float endReachedDistance = 0.01f;

		public float wallForce = 3f;

		public float wallDist = 1f;

		public bool funnelSimplification;

		public Animation anim;

		public bool slowWhenNotFacingTarget = true;

		protected Vector3 prevPosition = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

		protected Vector3 realVelocity;

		protected readonly RichPath richPath = new RichPath();

		private Vector3 waypoint;

		protected bool waitingForPathCalc;

		protected bool canSearchPath;

		protected bool delayUpdatePath;

		protected bool traversingSpecialPath;

		protected bool lastCorner;

		protected float distanceToWaypoint = float.PositiveInfinity;

		protected readonly List<Vector3> nextCorners = new List<Vector3>();

		protected readonly List<Vector3> wallBuffer = new List<Vector3>();

		private bool startHasRun;

		protected float lastRepath = float.NegativeInfinity;

		protected static readonly Color GizmoColorPath = new Color(0.03137255f, 0.305882365f, 0.7607843f);
	}
}
