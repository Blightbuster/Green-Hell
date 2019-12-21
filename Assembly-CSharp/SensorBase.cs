using System;
using UnityEngine;

public class SensorBase : MonoBehaviour, ITriggerThrough
{
	protected virtual void Start()
	{
	}

	protected virtual void Awake()
	{
		this.m_MeshRenderer = base.GetComponent<MeshRenderer>();
		if (!this.m_MeshRenderer)
		{
			base.enabled = false;
			return;
		}
		this.m_IsInside = false;
		this.m_MeshRenderer.enabled = false;
		SensorManager.Get().RegisterSensor(this);
	}

	private void OnDestroy()
	{
		SensorManager.Get().UnregisterSensor(this);
	}

	protected virtual void Update()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((this.m_Activator != null) ? (other.gameObject == this.m_Activator) : other.gameObject.IsPlayer())
		{
			this.OnEnter();
			this.m_WasInside = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if ((this.m_Activator != null) ? (other.gameObject == this.m_Activator) : other.gameObject.IsPlayer())
		{
			this.OnExit();
		}
	}

	protected virtual void OnEnter()
	{
		this.m_IsInside = true;
	}

	protected virtual void OnExit()
	{
		this.m_IsInside = false;
		if (this.m_OneTimeUse && this.m_WasInside)
		{
			base.gameObject.SetActive(false);
		}
	}

	public bool IsInside()
	{
		return this.m_IsInside;
	}

	public void Save()
	{
		if (this.m_OneTimeUse)
		{
			SaveGame.SaveVal(base.transform.position.sqrMagnitude.ToString("F10") + base.name, this.m_WasInside);
			SaveGame.SaveVal(base.transform.position.sqrMagnitude.ToString("F10") + base.name + "act", base.gameObject.activeSelf);
		}
	}

	public void Load()
	{
		if (this.m_OneTimeUse)
		{
			bool wasInside = this.m_WasInside;
			this.m_WasInside = SaveGame.LoadBVal(base.transform.position.sqrMagnitude.ToString("F10") + base.name);
			base.gameObject.SetActive(SaveGame.LoadBVal(base.transform.position.sqrMagnitude.ToString("F10") + base.name + "act"));
			if (!base.gameObject.activeSelf && !this.m_WasInside)
			{
				base.gameObject.SetActive(true);
			}
		}
	}

	public void SetWasInside(bool inside)
	{
		this.m_WasInside = inside;
	}

	protected MeshRenderer m_MeshRenderer;

	[HideInInspector]
	public bool m_IsInside;

	private bool m_WasInside;

	public GameObject m_Activator;

	public bool m_OneTimeUse = true;
}
