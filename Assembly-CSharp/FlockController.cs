using System;
using System.Collections.Generic;
using AIs;
using UnityEngine;

public class FlockController : MonoBehaviour
{
	public void Start()
	{
		this._thisT = base.transform;
		if (this._positionSphereDepth == -1f)
		{
			this._positionSphereDepth = this._positionSphere;
		}
		if (this._spawnSphereDepth == -1f)
		{
			this._spawnSphereDepth = this._spawnSphere;
		}
		this._posBuffer = this._thisT.position + this._startPosOffset;
		this.m_Bounds = new Bounds(this._posBuffer, new Vector3(this._spawnSphere * 2f, this._spawnSphereHeight * 2f, this._spawnSphereDepth * 2f));
		this.m_StoredChildAmount = this._childAmount;
	}

	public void AddChild(int amount)
	{
		if (this._groupChildToNewTransform)
		{
			this.InstantiateGroup();
		}
		for (int i = 0; i < amount; i++)
		{
			FlockChild flockChild = UnityEngine.Object.Instantiate<FlockChild>(this._childPrefab);
			flockChild.name = this._childPrefab.name;
			flockChild._spawner = this;
			this._roamers.Add(flockChild);
			this.AddChildToParent(flockChild.transform);
		}
	}

	public void AddChildToParent(Transform obj)
	{
		if (this._groupChildToFlock)
		{
			obj.parent = base.transform;
			return;
		}
		if (this._groupChildToNewTransform)
		{
			obj.parent = this._groupTransform;
			return;
		}
	}

	public void RemoveChild(FlockChild child)
	{
		this._roamers.Remove(child);
		UnityEngine.Object.Destroy(child.gameObject);
		this._childAmount--;
	}

	public void RemoveChild(int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			if (this._roamers.Count > 0)
			{
				FlockChild flockChild = this._roamers[this._roamers.Count - 1];
				this._roamers.RemoveAt(this._roamers.Count - 1);
				UnityEngine.Object.Destroy(flockChild.gameObject);
			}
		}
	}

	public void Update()
	{
		if (!AIManager.Get())
		{
			return;
		}
		this.UpdateActivity();
		if (!this.m_Active)
		{
			return;
		}
		if (this._activeChildren > 0f)
		{
			if (this._updateDivisor > 1)
			{
				this._updateCounter++;
				this._updateCounter %= this._updateDivisor;
				this._newDelta = Time.deltaTime * (float)this._updateDivisor;
			}
			else
			{
				this._newDelta = Time.deltaTime;
			}
		}
		this.UpdateChildAmount();
		this.UpdateTimeOfDay();
	}

	private void UpdateTimeOfDay()
	{
		bool flag = false;
		if (MainLevel.Instance.IsNight() && this._childPrefab.m_TimeOfDay == FlockChild.TimeOfDay.Day)
		{
			flag = true;
		}
		if (!MainLevel.Instance.IsNight() && this._childPrefab.m_TimeOfDay == FlockChild.TimeOfDay.Night)
		{
			flag = true;
		}
		if (flag)
		{
			int i = 0;
			while (i < this._roamers.Count)
			{
				FlockChild flockChild = this._roamers[i];
				if (flockChild.m_InvisibleDuration > 1f)
				{
					this._roamers.Remove(flockChild);
					UnityEngine.Object.Destroy(flockChild.gameObject);
					this._childAmount--;
				}
				else
				{
					i++;
				}
			}
		}
		else if (this._childAmount != this.m_StoredChildAmount)
		{
			this._childAmount = this.m_StoredChildAmount;
		}
	}

	private void UpdateActivity()
	{
		Vector3 vector = this.m_Bounds.ClosestPoint(Player.Get().transform.position);
		float num = vector.Distance(Player.Get().transform.position);
		bool flag = num <= AIManager.Get().m_FlocksActivationRange;
		if (this.m_Active != flag)
		{
			if (flag)
			{
				if (!this._slowSpawn)
				{
					this.AddChild(this._childAmount);
				}
				if (this._randomPositionTimer > 0f)
				{
					base.InvokeRepeating("SetFlockRandomPosition", this._randomPositionTimer, this._randomPositionTimer);
				}
			}
			else
			{
				this.RemoveChild(this._childAmount);
			}
			this.m_Active = flag;
		}
	}

	public void InstantiateGroup()
	{
		if (this._groupTransform != null)
		{
			return;
		}
		GameObject gameObject = new GameObject("FlockGroup");
		this._groupTransform = gameObject.transform;
		this._groupTransform.position = this._thisT.position;
		if (this._groupName != string.Empty)
		{
			gameObject.name = this._groupName;
			return;
		}
		gameObject.name = this._thisT.name + " Fish Container";
	}

	public void UpdateChildAmount()
	{
		if (this._childAmount >= 0 && this._childAmount < this._roamers.Count)
		{
			this.RemoveChild(1);
			return;
		}
		if (this._childAmount > this._roamers.Count)
		{
			this.AddChild(1);
		}
	}

	public void OnDrawGizmos()
	{
		if (this._thisT == null)
		{
			this._thisT = base.transform;
		}
		if (!Application.isPlaying && this._posBuffer != this._thisT.position + this._startPosOffset)
		{
			this._posBuffer = this._thisT.position + this._startPosOffset;
		}
		if (this._positionSphereDepth == -1f)
		{
			this._positionSphereDepth = this._positionSphere;
		}
		if (this._spawnSphereDepth == -1f)
		{
			this._spawnSphereDepth = this._spawnSphere;
		}
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(this._posBuffer, new Vector3(this._spawnSphere * 2f, this._spawnSphereHeight * 2f, this._spawnSphereDepth * 2f));
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube(this._thisT.position, new Vector3(this._positionSphere * 2f + this._spawnSphere * 2f, this._positionSphereHeight * 2f + this._spawnSphereHeight * 2f, this._positionSphereDepth * 2f + this._spawnSphereDepth * 2f));
	}

	public void SetFlockRandomPosition()
	{
		Vector3 zero = Vector3.zero;
		zero.x = UnityEngine.Random.Range(-this._positionSphere, this._positionSphere) + this._thisT.position.x;
		zero.z = UnityEngine.Random.Range(-this._positionSphereDepth, this._positionSphereDepth) + this._thisT.position.z;
		zero.y = UnityEngine.Random.Range(-this._positionSphereHeight, this._positionSphereHeight) + this._thisT.position.y;
		this._posBuffer = zero;
		if (this._forceChildWaypoints)
		{
			for (int i = 0; i < this._roamers.Count; i++)
			{
				this._roamers[i].Wander(UnityEngine.Random.value * this._forcedRandomDelay);
			}
		}
	}

	public void destroyBirds()
	{
		for (int i = 0; i < this._roamers.Count; i++)
		{
			UnityEngine.Object.Destroy(this._roamers[i].gameObject);
		}
		this._childAmount = 0;
		this._roamers.Clear();
	}

	public FlockChild _childPrefab;

	public int _childAmount = 250;

	private int m_StoredChildAmount;

	public bool _slowSpawn;

	public float _spawnSphere = 3f;

	public float _spawnSphereHeight = 3f;

	public float _spawnSphereDepth = -1f;

	public float _minSpeed = 6f;

	public float _maxSpeed = 10f;

	public float _minScale = 0.7f;

	public float _maxScale = 1f;

	public float _soarFrequency;

	public string _soarAnimation = "Soar";

	public string _flapAnimation = "Flap";

	public string _idleAnimation = "Idle";

	public string _deathAnimation = "Death";

	public float _diveValue = 7f;

	public float _diveFrequency = 0.5f;

	public float _minDamping = 1f;

	public float _maxDamping = 2f;

	public float _waypointDistance = 1f;

	public float _minAnimationSpeed = 2f;

	public float _maxAnimationSpeed = 4f;

	public float _randomPositionTimer = 10f;

	public float _positionSphere = 25f;

	public float _positionSphereHeight = 25f;

	public float _positionSphereDepth = -1f;

	public bool _childTriggerPos;

	public bool _forceChildWaypoints;

	public float _forcedRandomDelay = 1.5f;

	public bool _flatFly;

	public bool _flatSoar;

	public bool _birdAvoid;

	public int _birdAvoidHorizontalForce = 1000;

	public bool _birdAvoidDown;

	public bool _birdAvoidUp;

	public int _birdAvoidVerticalForce = 300;

	public float _birdAvoidDistanceMax = 4.5f;

	public float _birdAvoidDistanceMin = 5f;

	public float _soarMaxTime;

	public LayerMask _avoidanceMask = -1;

	public List<FlockChild> _roamers;

	public Vector3 _posBuffer;

	public int _updateDivisor = 1;

	public float _newDelta;

	public int _updateCounter;

	public float _activeChildren;

	public bool _groupChildToNewTransform;

	public Transform _groupTransform;

	public string _groupName = string.Empty;

	public bool _groupChildToFlock;

	public Vector3 _startPosOffset;

	public Transform _thisT;

	private Bounds m_Bounds;

	private bool m_Active;
}
