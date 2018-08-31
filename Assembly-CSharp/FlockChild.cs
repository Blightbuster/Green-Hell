using System;
using System.Collections;
using Enums;
using UnityEngine;

public class FlockChild : MonoBehaviour
{
	public void Start()
	{
		this.FindRequiredComponents();
		this.Wander(0f);
		this.SetRandomScale();
		this._thisT.position = this.findWaypoint();
		this.RandomizeStartAnimationFrame();
		this.InitAvoidanceValues();
		this._speed = this._spawner._minSpeed;
		this._spawner._activeChildren += 1f;
		this._instantiated = true;
		if (this._spawner._updateDivisor > 1)
		{
			int num = this._spawner._updateDivisor - 1;
			FlockChild._updateNextSeed++;
			this._updateSeed = FlockChild._updateNextSeed;
			FlockChild._updateNextSeed %= num;
		}
		this.m_Renderer = base.gameObject.GetComponentInChildren<Renderer>();
	}

	private Animation GetAnimation()
	{
		if (!this.m_Animation)
		{
			this.m_Animation = this._model.GetComponent<Animation>();
		}
		return this.m_Animation;
	}

	public void Update()
	{
		if (this._spawner._updateDivisor <= 1 || this._spawner._updateCounter == this._updateSeed)
		{
			this.SoarTimeLimit();
			this.CheckForDistanceToWaypoint();
			this.RotationBasedOnWaypointOrAvoidance();
			this.LimitRotationOfModel();
		}
		if (!this.m_Renderer.isVisible)
		{
			this.m_InvisibleDuration += Time.deltaTime;
		}
		else
		{
			this.m_InvisibleDuration = 0f;
		}
	}

	public void OnDisable()
	{
		base.CancelInvoke();
		this._spawner._activeChildren -= 1f;
	}

	public void OnEnable()
	{
		if (this._instantiated)
		{
			this._spawner._activeChildren += 1f;
			if (this._landing)
			{
				this.GetAnimation().Play(this._spawner._idleAnimation);
			}
			else
			{
				this.GetAnimation().Play(this._spawner._flapAnimation);
			}
		}
	}

	public void FindRequiredComponents()
	{
		if (this._thisT == null)
		{
			this._thisT = base.transform;
		}
		if (this._model == null)
		{
			this._model = this._thisT.Find("Model").gameObject;
		}
		if (this._modelT == null)
		{
			this._modelT = this._model.transform;
		}
	}

	public void RandomizeStartAnimationFrame()
	{
		IEnumerator enumerator = this.GetAnimation().GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				AnimationState animationState = (AnimationState)obj;
				animationState.time = UnityEngine.Random.value * animationState.length;
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
	}

	public void InitAvoidanceValues()
	{
		this._avoidValue = UnityEngine.Random.Range(0.3f, 0.1f);
		if (this._spawner._birdAvoidDistanceMax != this._spawner._birdAvoidDistanceMin)
		{
			this._avoidDistance = UnityEngine.Random.Range(this._spawner._birdAvoidDistanceMax, this._spawner._birdAvoidDistanceMin);
			return;
		}
		this._avoidDistance = this._spawner._birdAvoidDistanceMin;
	}

	public void SetRandomScale()
	{
		float num = UnityEngine.Random.Range(this._spawner._minScale, this._spawner._maxScale);
		this._thisT.localScale = new Vector3(num, num, num);
	}

	public void SoarTimeLimit()
	{
		if (this._soar && this._spawner._soarMaxTime > 0f)
		{
			if (this._soarTimer > this._spawner._soarMaxTime)
			{
				this.Flap();
				this._soarTimer = 0f;
			}
			else
			{
				this._soarTimer += this._spawner._newDelta;
			}
		}
	}

	public void CheckForDistanceToWaypoint()
	{
		if (!this._landing && (this._thisT.position - this._wayPoint).magnitude < this._spawner._waypointDistance + this._stuckCounter)
		{
			this.Wander(0f);
			this._stuckCounter = 0f;
		}
		else if (!this._landing)
		{
			this._stuckCounter += this._spawner._newDelta;
		}
		else
		{
			this._stuckCounter = 0f;
		}
	}

	public void RotationBasedOnWaypointOrAvoidance()
	{
		Vector3 vector = this._wayPoint - this._thisT.position;
		if (this._targetSpeed > -1f && vector != Vector3.zero)
		{
			Quaternion b = Quaternion.LookRotation(vector);
			this._thisT.rotation = Quaternion.Slerp(this._thisT.rotation, b, this._spawner._newDelta * this._damping);
		}
		if (this._spawner._childTriggerPos && (this._thisT.position - this._spawner._posBuffer).magnitude < 1f)
		{
			this._spawner.SetFlockRandomPosition();
		}
		this._speed = Mathf.Lerp(this._speed, this._targetSpeed, this._spawner._newDelta * 2.5f);
		if (this._move)
		{
			this._thisT.position += this._thisT.forward * this._speed * this._spawner._newDelta;
			if (this._avoid && this._spawner._birdAvoid)
			{
				this.Avoidance();
			}
		}
	}

	public bool Avoidance()
	{
		RaycastHit raycastHit = default(RaycastHit);
		Vector3 forward = this._modelT.forward;
		bool result = false;
		Quaternion rotation = Quaternion.identity;
		Vector3 eulerAngles = Vector3.zero;
		Vector3 position = Vector3.zero;
		position = this._thisT.position;
		rotation = this._thisT.rotation;
		eulerAngles = this._thisT.rotation.eulerAngles;
		if (Physics.Raycast(this._thisT.position, forward + this._modelT.right * this._avoidValue, out raycastHit, this._avoidDistance, this._spawner._avoidanceMask))
		{
			eulerAngles.y -= (float)this._spawner._birdAvoidHorizontalForce * this._spawner._newDelta * this._damping;
			rotation.eulerAngles = eulerAngles;
			this._thisT.rotation = rotation;
			result = true;
		}
		else if (Physics.Raycast(this._thisT.position, forward + this._modelT.right * -this._avoidValue, out raycastHit, this._avoidDistance, this._spawner._avoidanceMask))
		{
			eulerAngles.y += (float)this._spawner._birdAvoidHorizontalForce * this._spawner._newDelta * this._damping;
			rotation.eulerAngles = eulerAngles;
			this._thisT.rotation = rotation;
			result = true;
		}
		if (this._spawner._birdAvoidDown && !this._landing && Physics.Raycast(this._thisT.position, -Vector3.up, out raycastHit, this._avoidDistance, this._spawner._avoidanceMask))
		{
			eulerAngles.x -= (float)this._spawner._birdAvoidVerticalForce * this._spawner._newDelta * this._damping;
			rotation.eulerAngles = eulerAngles;
			this._thisT.rotation = rotation;
			position.y += (float)this._spawner._birdAvoidVerticalForce * this._spawner._newDelta * 0.01f;
			this._thisT.position = position;
			result = true;
		}
		else if (this._spawner._birdAvoidUp && !this._landing && Physics.Raycast(this._thisT.position, Vector3.up, out raycastHit, this._avoidDistance, this._spawner._avoidanceMask))
		{
			eulerAngles.x += (float)this._spawner._birdAvoidVerticalForce * this._spawner._newDelta * this._damping;
			rotation.eulerAngles = eulerAngles;
			this._thisT.rotation = rotation;
			position.y -= (float)this._spawner._birdAvoidVerticalForce * this._spawner._newDelta * 0.01f;
			this._thisT.position = position;
			result = true;
		}
		return result;
	}

	public void LimitRotationOfModel()
	{
		Quaternion localRotation = Quaternion.identity;
		Vector3 eulerAngles = Vector3.zero;
		localRotation = this._modelT.localRotation;
		eulerAngles = localRotation.eulerAngles;
		if ((((this._soar && this._spawner._flatSoar) || (this._spawner._flatFly && !this._soar)) && this._wayPoint.y > this._thisT.position.y) || this._landing)
		{
			eulerAngles.x = Mathf.LerpAngle(this._modelT.localEulerAngles.x, -this._thisT.localEulerAngles.x, this._spawner._newDelta * 1.75f);
			localRotation.eulerAngles = eulerAngles;
			this._modelT.localRotation = localRotation;
		}
		else
		{
			eulerAngles.x = Mathf.LerpAngle(this._modelT.localEulerAngles.x, 0f, this._spawner._newDelta * 1.75f);
			localRotation.eulerAngles = eulerAngles;
			this._modelT.localRotation = localRotation;
		}
	}

	public void Wander(float delay)
	{
		if (!this._landing)
		{
			this._damping = UnityEngine.Random.Range(this._spawner._minDamping, this._spawner._maxDamping);
			this._targetSpeed = UnityEngine.Random.Range(this._spawner._minSpeed, this._spawner._maxSpeed);
			base.Invoke("SetRandomMode", delay);
		}
	}

	public void SetRandomMode()
	{
		base.CancelInvoke("SetRandomMode");
		if (!this._dived && UnityEngine.Random.value < this._spawner._soarFrequency)
		{
			this.Soar();
		}
		else if (!this._dived && UnityEngine.Random.value < this._spawner._diveFrequency)
		{
			this.Dive();
		}
		else
		{
			this.Flap();
		}
	}

	public void Flap()
	{
		if (this._move)
		{
			if (this._model != null)
			{
				this.GetAnimation().CrossFade(this._spawner._flapAnimation, 0.5f);
			}
			this._soar = false;
			this.animationSpeed();
			this._wayPoint = this.findWaypoint();
			this._dived = false;
		}
	}

	public Vector3 findWaypoint()
	{
		Vector3 zero = Vector3.zero;
		zero.x = UnityEngine.Random.Range(-this._spawner._spawnSphere, this._spawner._spawnSphere) + this._spawner._posBuffer.x;
		zero.z = UnityEngine.Random.Range(-this._spawner._spawnSphereDepth, this._spawner._spawnSphereDepth) + this._spawner._posBuffer.z;
		zero.y = UnityEngine.Random.Range(-this._spawner._spawnSphereHeight, this._spawner._spawnSphereHeight) + this._spawner._posBuffer.y;
		return zero;
	}

	public void Soar()
	{
		if (this._move)
		{
			this.GetAnimation().CrossFade(this._spawner._soarAnimation, 1.5f);
			this._wayPoint = this.findWaypoint();
			this._soar = true;
		}
	}

	public void Dive()
	{
		if (this._spawner._soarAnimation != null)
		{
			this.GetAnimation().CrossFade(this._spawner._soarAnimation, 1.5f);
		}
		else
		{
			IEnumerator enumerator = this.GetAnimation().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					AnimationState animationState = (AnimationState)obj;
					if (this._thisT.position.y < this._wayPoint.y + 25f)
					{
						animationState.speed = 0.1f;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}
		this._wayPoint = this.findWaypoint();
		this._wayPoint.y = this._wayPoint.y - this._spawner._diveValue;
		this._dived = true;
	}

	public void Hit(Vector3 hit_dir)
	{
		string value = base.name + "_Body";
		ItemID item_id = (ItemID)Enum.Parse(typeof(ItemID), value);
		Item item = ItemsManager.Get().CreateItem(item_id, true, base.transform.position, UnityEngine.Random.rotation);
		item.ApplyImpulse(new Vector3(0f, 10f, 0f), new Vector3(0f, 0f, UnityEngine.Random.Range(-100f, 100f)));
		this._spawner.RemoveChild(this);
	}

	public void animationSpeed()
	{
		IEnumerator enumerator = this.GetAnimation().GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				AnimationState animationState = (AnimationState)obj;
				if (!this._dived && !this._landing)
				{
					animationState.speed = UnityEngine.Random.Range(this._spawner._minAnimationSpeed, this._spawner._maxAnimationSpeed);
				}
				else
				{
					animationState.speed = this._spawner._maxAnimationSpeed;
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
	}

	[HideInInspector]
	public FlockController _spawner;

	[HideInInspector]
	public Vector3 _wayPoint;

	public float _speed;

	[HideInInspector]
	public bool _dived = true;

	[HideInInspector]
	public float _stuckCounter;

	[HideInInspector]
	public float _damping;

	[HideInInspector]
	public bool _soar = true;

	[HideInInspector]
	public bool _landing;

	[HideInInspector]
	public float _targetSpeed;

	[HideInInspector]
	public bool _move = true;

	public GameObject _model;

	public Transform _modelT;

	[HideInInspector]
	public float _avoidValue;

	[HideInInspector]
	public float _avoidDistance;

	private float _soarTimer;

	private bool _instantiated;

	private static int _updateNextSeed;

	private int _updateSeed = -1;

	[HideInInspector]
	public bool _avoid = true;

	public Transform _thisT;

	public Vector3 _landingPosOffset;

	private Renderer m_Renderer;

	[HideInInspector]
	public float m_InvisibleDuration;

	private Animation m_Animation;
}
