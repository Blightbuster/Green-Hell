using System;
using CJTools;
using UnityEngine;

public class PreDreamWrongWaySoundController : MonoBehaviour
{
	private void Awake()
	{
		this.m_AudioSource = base.GetComponent<AudioSource>();
	}

	private void OnEnable()
	{
		this.m_AudioSource.Play();
	}

	private void OnDisable()
	{
		this.m_AudioSource.Stop();
	}

	private void Update()
	{
		this.UpdateVolume();
	}

	private void UpdateVolume()
	{
		if (this.m_RefObject == null)
		{
			return;
		}
		float b = Player.Get().transform.position.Distance(this.m_RefObject.transform.position);
		float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, b, this.m_MinVolumeDist, this.m_MaxVolumeDist);
		this.m_AudioSource.volume = proportionalClamp;
	}

	private AudioSource m_AudioSource;

	public GameObject m_RefObject;

	public float m_MaxVolumeDist = 30f;

	public float m_MinVolumeDist = 20f;
}
