using System;
using System.Collections;
using Pathfinding;
using Pathfinding.Util;
using UnityEngine;

[HelpURL("http://arongranberg.com/astar/docs/class_a_i_lerp.php")]
[RequireComponent(typeof(Seeker))]
[AddComponentMenu("Pathfinding/AI/AILerp (2D,3D)")]
public class AILerp : VersionedMonoBehaviour
{
	public bool targetReached { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		this.tr = base.transform;
		this.seeker = base.GetComponent<Seeker>();
		this.seeker.startEndModifier.adjustStartPoint = (() => this.tr.position);
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
		if (this.seeker != null)
		{
			this.seeker.CancelCurrentPathRequest(true);
		}
		this.canSearchAgain = true;
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
		for (;;)
		{
			float v = this.TrySearchPath();
			yield return new WaitForSeconds(v);
		}
		yield break;
	}

	public float TrySearchPath()
	{
		if (Time.time - this.lastRepath >= this.repathRate && this.canSearchAgain && this.canSearch && this.target != null)
		{
			this.SearchPath();
			return this.repathRate;
		}
		return Mathf.Max(0f, this.repathRate - (Time.time - this.lastRepath));
	}

	public virtual void SearchPath()
	{
		this.ForceSearchPath();
	}

	public virtual void ForceSearchPath()
	{
		if (this.target == null)
		{
			throw new InvalidOperationException("Target is null");
		}
		this.lastRepath = Time.time;
		Vector3 position = this.target.position;
		Vector3 start = this.GetFeetPosition();
		if (this.interpolator.valid)
		{
			float distance = this.interpolator.distance;
			this.interpolator.MoveToSegment(this.interpolator.segmentIndex, 1f);
			start = this.interpolator.position;
			this.interpolator.distance = distance;
		}
		this.canSearchAgain = false;
		this.seeker.StartPath(start, position);
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
		if (this.interpolatePathSwitches)
		{
			this.ConfigurePathSwitchInterpolation();
		}
		if (this.path != null)
		{
			this.path.Release(this, false);
		}
		this.path = abpath;
		this.targetReached = false;
		if (this.path.vectorPath != null && this.path.vectorPath.Count == 1)
		{
			this.path.vectorPath.Insert(0, this.GetFeetPosition());
		}
		this.ConfigureNewPath();
	}

	protected virtual void ConfigurePathSwitchInterpolation()
	{
		bool flag = this.interpolator.valid && this.interpolator.remainingDistance < 0.0001f;
		if (this.interpolator.valid && !flag)
		{
			this.previousMovementOrigin = this.interpolator.position;
			this.previousMovementDirection = this.interpolator.tangent.normalized * this.interpolator.remainingDistance;
			this.previousMovementStartTime = Time.time;
		}
		else
		{
			this.previousMovementOrigin = Vector3.zero;
			this.previousMovementDirection = Vector3.zero;
			this.previousMovementStartTime = -9999f;
		}
	}

	public virtual Vector3 GetFeetPosition()
	{
		return this.tr.position;
	}

	protected virtual void ConfigureNewPath()
	{
		bool valid = this.interpolator.valid;
		Vector3 vector = (!valid) ? Vector3.zero : this.interpolator.tangent;
		this.interpolator.SetPath(this.path.vectorPath);
		this.interpolator.MoveToClosestPoint(this.GetFeetPosition());
		if (this.interpolatePathSwitches && this.switchPathInterpolationSpeed > 0.01f && valid)
		{
			float num = Mathf.Max(-Vector3.Dot(vector.normalized, this.interpolator.tangent.normalized), 0f);
			this.interpolator.distance -= this.speed * num * (1f / this.switchPathInterpolationSpeed);
		}
	}

	protected virtual void Update()
	{
		if (this.canMove)
		{
			Vector3 vector;
			Vector3 position = this.CalculateNextPosition(out vector);
			if (this.enableRotation && vector != Vector3.zero)
			{
				if (this.rotationIn2D)
				{
					float b = Mathf.Atan2(vector.x, -vector.y) * 57.29578f + 180f;
					Vector3 eulerAngles = this.tr.eulerAngles;
					eulerAngles.z = Mathf.LerpAngle(eulerAngles.z, b, Time.deltaTime * this.rotationSpeed);
					this.tr.eulerAngles = eulerAngles;
				}
				else
				{
					Quaternion rotation = this.tr.rotation;
					Quaternion b2 = Quaternion.LookRotation(vector);
					this.tr.rotation = Quaternion.Slerp(rotation, b2, Time.deltaTime * this.rotationSpeed);
				}
			}
			this.tr.position = position;
		}
	}

	protected virtual Vector3 CalculateNextPosition(out Vector3 direction)
	{
		if (!this.interpolator.valid)
		{
			direction = Vector3.zero;
			return this.tr.position;
		}
		this.interpolator.distance += Time.deltaTime * this.speed;
		if (this.interpolator.remainingDistance < 0.0001f && !this.targetReached)
		{
			this.targetReached = true;
			this.OnTargetReached();
		}
		direction = this.interpolator.tangent;
		float num = this.switchPathInterpolationSpeed * (Time.time - this.previousMovementStartTime);
		if (this.interpolatePathSwitches && num < 1f)
		{
			Vector3 a = this.previousMovementOrigin + Vector3.ClampMagnitude(this.previousMovementDirection, this.speed * (Time.time - this.previousMovementStartTime));
			return Vector3.Lerp(a, this.interpolator.position, num);
		}
		return this.interpolator.position;
	}

	public float repathRate = 0.5f;

	public Transform target;

	public bool canSearch = true;

	public bool canMove = true;

	public float speed = 3f;

	public bool enableRotation = true;

	public bool rotationIn2D;

	public float rotationSpeed = 10f;

	public bool interpolatePathSwitches = true;

	public float switchPathInterpolationSpeed = 5f;

	protected Seeker seeker;

	protected Transform tr;

	protected float lastRepath = -9999f;

	protected ABPath path;

	protected bool canSearchAgain = true;

	protected Vector3 previousMovementOrigin;

	protected Vector3 previousMovementDirection;

	protected float previousMovementStartTime = -9999f;

	protected PathInterpolator interpolator = new PathInterpolator();

	private bool startHasRun;
}
