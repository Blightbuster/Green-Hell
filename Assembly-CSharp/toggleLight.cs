using System;
using UnityEngine;

public class toggleLight : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return) && !this.lightOn)
		{
			this.lightOn = true;
			this.playerLight.SetActive(true);
			return;
		}
		if (this.lightOn && Input.GetKeyDown(KeyCode.Return))
		{
			this.lightOn = false;
			this.playerLight.SetActive(false);
		}
	}

	public GameObject playerLight;

	public bool lightOn;
}
