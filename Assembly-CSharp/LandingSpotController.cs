using System;
using System.Collections;
using UnityEngine;

public class LandingSpotController : MonoBehaviour
{
	public void Start()
	{
		if (this._thisT == null)
		{
			this._thisT = base.transform;
		}
		if (this._flock == null)
		{
			this._flock = (FlockController)UnityEngine.Object.FindObjectOfType(typeof(FlockController));
			Debug.Log(this + " has no assigned FlockController, a random FlockController has been assigned");
		}
		if (this._landOnStart)
		{
			base.StartCoroutine(this.InstantLandOnStart(0.1f));
		}
	}

	private void LateUpdate()
	{
		this.CheckPlayer();
	}

	private void CheckPlayer()
	{
		for (int i = 0; i < this._thisT.childCount; i++)
		{
			LandingSpot component = this._thisT.GetChild(i).GetComponent<LandingSpot>();
			if (component != null && component.transform.position.Distance(Player.Get().transform.position) < UnityEngine.Random.Range(this.m_ScareDistMin, this.m_ScareDistMax))
			{
				component.ReleaseFlockChild();
			}
		}
	}

	public void ScareAll()
	{
		this.ScareAll(0f, 1f);
	}

	public void ScareAll(float minDelay, float maxDelay)
	{
		for (int i = 0; i < this._thisT.childCount; i++)
		{
			if (this._thisT.GetChild(i).GetComponent<LandingSpot>() != null)
			{
				LandingSpot component = this._thisT.GetChild(i).GetComponent<LandingSpot>();
				component.Invoke("ReleaseFlockChild", UnityEngine.Random.Range(minDelay, maxDelay));
			}
		}
	}

	public void LandAll()
	{
		for (int i = 0; i < this._thisT.childCount; i++)
		{
			if (this._thisT.GetChild(i).GetComponent<LandingSpot>() != null)
			{
				LandingSpot component = this._thisT.GetChild(i).GetComponent<LandingSpot>();
				base.StartCoroutine(component.GetFlockChild(0f, 2f));
			}
		}
	}

	public IEnumerator InstantLandOnStart(float delay)
	{
		yield return new WaitForSeconds(delay);
		for (int i = 0; i < this._thisT.childCount; i++)
		{
			if (this._thisT.GetChild(i).GetComponent<LandingSpot>() != null)
			{
				LandingSpot component = this._thisT.GetChild(i).GetComponent<LandingSpot>();
				component.InstantLand();
			}
		}
		yield break;
	}

	public IEnumerator InstantLand(float delay)
	{
		yield return new WaitForSeconds(delay);
		for (int i = 0; i < this._thisT.childCount; i++)
		{
			if (this._thisT.GetChild(i).GetComponent<LandingSpot>() != null)
			{
				LandingSpot component = this._thisT.GetChild(i).GetComponent<LandingSpot>();
				component.InstantLand();
			}
		}
		yield break;
	}

	public bool _randomRotate = true;

	public Vector2 _autoCatchDelay = new Vector2(10f, 20f);

	public Vector2 _autoDismountDelay = new Vector2(10f, 20f);

	public float _maxBirdDistance = 20f;

	public float _minBirdDistance = 5f;

	public bool _takeClosest;

	public FlockController _flock;

	public bool _landOnStart;

	public bool _soarLand = true;

	public bool _onlyBirdsAbove;

	public float _landingSpeedModifier = 0.5f;

	public float _landingTurnSpeedModifier = 5f;

	public Transform _featherPS;

	public Transform _thisT;

	public int _activeLandingSpots;

	public float m_ScareDistMin = 3f;

	public float m_ScareDistMax = 6f;

	public float _snapLandDistance = 0.1f;

	public float _landedRotateSpeed = 0.01f;
}
