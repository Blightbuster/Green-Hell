using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
	private void Start()
	{
		this.lt = new List<Transform>();
		this.prefabs = this.prefabHolder.GetComponentsInChildren<Transform>(true);
		foreach (Transform transform in this.prefabs)
		{
			if (transform.parent == this.prefabHolder)
			{
				this.lt.Add(transform);
			}
		}
		this.prefabs = this.lt.ToArray();
		this.EnableActive();
	}

	public void EnableActive()
	{
		for (int i = 0; i < this.prefabs.Length; i++)
		{
			if (i == this.activeNumber)
			{
				this.prefabs[i].gameObject.SetActive(true);
				this.text.text = this.prefabs[i].name;
			}
			else
			{
				this.prefabs[i].gameObject.SetActive(false);
			}
		}
	}

	public void ChangeEffect(bool bo)
	{
		if (bo)
		{
			this.activeNumber++;
			if (this.activeNumber == this.prefabs.Length)
			{
				this.activeNumber = 0;
			}
		}
		else
		{
			this.activeNumber--;
			if (this.activeNumber == -1)
			{
				this.activeNumber = this.prefabs.Length - 1;
			}
		}
		this.EnableActive();
	}

	public void SetDay()
	{
		this.directionalLight.enabled = true;
		RenderSettings.skybox = this.daySkyboxMaterial;
		this.reflectionProbe.RenderProbe();
	}

	public void SetNight()
	{
		this.directionalLight.enabled = false;
		RenderSettings.skybox = this.nightSkyboxMaterial;
		this.reflectionProbe.RenderProbe();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Q))
		{
			this.SetDay();
		}
		if (Input.GetKeyDown(KeyCode.E))
		{
			this.SetNight();
		}
		if (Input.GetKeyDown(KeyCode.X))
		{
			this.ChangeEffect(true);
		}
		if (Input.GetKeyDown(KeyCode.Z))
		{
			this.ChangeEffect(false);
		}
	}

	public Light directionalLight;

	public ReflectionProbe reflectionProbe;

	public Material daySkyboxMaterial;

	public Material nightSkyboxMaterial;

	public Transform prefabHolder;

	public Text text;

	private Transform[] prefabs;

	private List<Transform> lt;

	private int activeNumber;
}
