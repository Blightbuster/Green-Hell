using System;
using UnityEngine;

public class StaticBatching : MonoBehaviour
{
	private void OnEnable()
	{
		this.m_Object = GameObject.FindGameObjectWithTag("Stone_05");
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.I))
		{
			if (this.m_Object.activeSelf)
			{
				this.m_Object.SetActive(false);
				return;
			}
			this.m_Object.SetActive(true);
		}
	}

	private GameObject m_Object;
}
