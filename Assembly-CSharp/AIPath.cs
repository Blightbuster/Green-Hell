using System;
using System.Collections;
using Pathfinding;
using Pathfinding.RVO;
using Pathfinding.Util;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Seeker))]
[AddComponentMenu("Pathfinding/AI/AIPath (2D,3D)")]
[HelpURL("http://arongranberg.com/astar/docs/class_a_i_path.php")]
public class AIPath : AIBase
{
	public bool TargetReached { get; protected set; }

	[Obsolete("This field has been renamed to #rotationSpeed and is now in degrees per second instead of a damping factor")]
	public float turningSpeed
	{
		get
		{
			return this.rotationSpeed / 90f;
		}
		set
		{
			this.rotationSpeed = value * 90f;
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
		if (this.startHasRun)
		{
			this.lastRepath = float.NegativeInfinity;
			base.StartCoroutine(this.RepeatTrySearchPath());
		}
	}

	public void OnDisable()
	{
		this.seeker.CancelCurrentPathRequest(true);
		if (this.path != null)
		{
			this.path.Release(this, false);
		}
		this.path = null;
		Seeker seeker = this.seeker;
		seeker.pathCallback = (OnPathDelegate)Delegate.Remove(seeker.pathCallback, new OnPathDelegate(this.OnPathComplete));
	}

	protected IEnumerator RepeatTrySearchPath()
	{
		/*
An exception occurred when decompiling this method (06002577)

ICSharpCode.Decompiler.DecompilerException: Error decompiling System.Collections.IEnumerator AIPath::RepeatTrySearchPath()
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

	public float TrySearchPath()
	{
		if (Time.time - this.lastRepath >= this.repathRate && this.canSearchAgain && this.canSearch && this.target != null)
		{
			this.SearchPath();
			return this.repathRate;
		}
		float num = this.repathRate - (Time.time - this.lastRepath);
		if (num >= 0f)
		{
			return num;
		}
		return 0f;
	}

	public virtual void SearchPath()
	{
		if (this.target == null)
		{
			throw new InvalidOperationException("Target is null");
		}
		this.lastRepath = Time.time;
		Vector3 position = this.target.position;
		this.canSearchAgain = false;
		this.seeker.StartPath(this.GetFeetPosition(), position);
	}

	public virtual void OnTargetReached()
	{
	}

	public virtual void OnPathComplete(Path _p)
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
		if (this.path.vectorPath.Count == 1)
		{
			this.path.vectorPath.Add(this.path.vectorPath[0]);
		}
		this.interpolator.SetPath(this.path.vectorPath);
		ITransformedGraph transformedGraph = AstarData.GetGraph(this.path.path[0]) as ITransformedGraph;
		this.movementPlane = ((transformedGraph != null) ? transformedGraph.transform : GraphTransform.identityTransform);
		this.TargetReached = false;
		this.interpolator.MoveToLocallyClosestPoint((this.GetFeetPosition() + abpath.originalStartPoint) * 0.5f, true, true);
		this.interpolator.MoveToLocallyClosestPoint(this.GetFeetPosition(), true, true);
	}

	public virtual Vector3 GetFeetPosition()
	{
		if (this.rvoController != null && this.rvoController.enabled && this.rvoController.movementPlane == MovementPlane.XZ)
		{
			return this.tr.position + this.tr.up * (this.rvoController.center - this.rvoController.height * 0.5f);
		}
		if (this.controller != null && this.controller.enabled)
		{
			return this.tr.TransformPoint(this.controller.center) - Vector3.up * this.controller.height * 0.5f;
		}
		return this.tr.position;
	}

	protected override void MovementUpdate(float deltaTime)
	{
		if (!this.canMove)
		{
			return;
		}
		if (!this.interpolator.valid)
		{
			this.velocity2D = Vector3.zero;
			return;
		}
		Vector3 position = this.tr.position;
		this.interpolator.MoveToLocallyClosestPoint(position, true, false);
		this.interpolator.MoveToCircleIntersection2D(position, this.pickNextWaypointDist, this.movementPlane);
		this.targetPoint = this.interpolator.position;
		Vector2 deltaPosition = this.movementPlane.ToPlane(this.targetPoint - position);
		float num = deltaPosition.magnitude + this.interpolator.remainingDistance;
		float num2 = (this.slowdownDistance > 0f) ? (num / this.slowdownDistance) : 1f;
		float acceleration = this.speed / 0.4f;
		this.velocity2D += MovementUtilities.CalculateAccelerationToReachPoint(deltaPosition, deltaPosition.normalized * this.speed, this.velocity2D, acceleration, this.speed) * deltaTime;
		this.velocity2D = MovementUtilities.ClampVelocity(this.velocity2D, this.speed, num2, true, this.movementPlane.ToPlane(this.rotationIn2D ? this.tr.up : this.tr.forward));
		base.ApplyGravity(deltaTime);
		if (num <= this.endReachedDistance && !this.TargetReached)
		{
			this.TargetReached = true;
			this.OnTargetReached();
		}
		float num3 = this.rotationSpeed * Mathf.Clamp01((Mathf.Sqrt(num2) - 0.3f) / 0.7f);
		this.RotateTowards(this.velocity2D, num3 * deltaTime);
		if (this.rvoController != null && this.rvoController.enabled)
		{
			Vector3 pos = position + this.movementPlane.ToWorld(Vector2.ClampMagnitude(this.velocity2D, num), 0f);
			this.rvoController.SetTarget(pos, this.velocity2D.magnitude, this.speed);
		}
		Vector2 p = base.CalculateDeltaToMoveThisFrame(this.movementPlane.ToPlane(position), num, deltaTime);
		base.Move(position, this.movementPlane.ToWorld(p, this.verticalVelocity * deltaTime));
		this.velocity = this.movementPlane.ToWorld(this.velocity2D, this.verticalVelocity);
	}

	[Obsolete("Only exists for compatibility reasons.")]
	public Vector3 targetDirection
	{
		get
		{
			return (this.targetPoint - this.tr.position).normalized;
		}
	}

	[Obsolete("This method no longer calculates the velocity. Use the velocity property instead")]
	public Vector3 CalculateVelocity(Vector3 position)
	{
		return this.velocity;
	}

	protected override int OnUpgradeSerializedData(int version)
	{
		if (version < 1)
		{
			this.rotationSpeed *= 90f;
		}
		return 1;
	}

	public float repathRate = 0.5f;

	public Transform target;

	public bool canSearch = true;

	public bool canMove = true;

	public float speed = 3f;

	[FormerlySerializedAs("turningSpeed")]
	public float rotationSpeed = 360f;

	public float slowdownDistance = 0.6f;

	public float pickNextWaypointDist = 2f;

	public float endReachedDistance = 0.2f;

	public bool alwaysDrawGizmos;

	protected float lastRepath = -9999f;

	protected Path path;

	protected PathInterpolator interpolator = new PathInterpolator();

	protected bool canSearchAgain = true;

	private bool startHasRun;

	protected Vector3 targetPoint;

	protected Vector3 velocity;
}
