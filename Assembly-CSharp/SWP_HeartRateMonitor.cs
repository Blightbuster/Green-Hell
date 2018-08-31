using System;
using System.Collections;
using UnityEngine;

public class SWP_HeartRateMonitor : MonoBehaviour
{
	private void Start()
	{
		this.BeatsPerSecond = 60f / (float)this.BeatsPerMinute;
		this.BlipOffset = new Vector3(base.transform.position.x - this.BlipMonitorWidth / 2f, base.transform.position.y, base.transform.position.z);
		this.DisplayXEnd = this.BlipOffset.x + this.BlipMonitorWidth;
		this.CreateClone();
		this.TrailTime = this.NewClone.GetComponentInChildren<TrailRenderer>().time;
	}

	private void Update()
	{
		this.BeatsPerSecond = 60f / (float)this.BeatsPerMinute;
		this.BlipOffset = new Vector3(base.transform.position.x - this.BlipMonitorWidth / 2f, base.transform.position.y, base.transform.position.z);
		this.DisplayXEnd = this.BlipOffset.x + this.BlipMonitorWidth;
		if (this.NewClone.transform.position.x > this.DisplayXEnd)
		{
			if (this.NewClone != null)
			{
				GameObject newClone = this.NewClone;
				base.StartCoroutine(this.WaitThenDestroy(newClone));
			}
			this.CreateClone();
		}
		else if (!this.FlatLine)
		{
			this.NewClone.transform.position += new Vector3(this.BlipMonitorWidth * Time.deltaTime * this.LineSpeed, UnityEngine.Random.Range(-0.05f, 0.05f), 0f);
		}
		else
		{
			this.NewClone.transform.position += new Vector3(this.BlipMonitorWidth * Time.deltaTime * this.LineSpeed, 0f, 0f);
			if (!this.bFlatLinePlayed)
			{
				this.PlayHeartSound(SWP_HeartRateMonitor.SoundType.Flatline, this.SoundVolume);
				this.bFlatLinePlayed = true;
			}
		}
		if (this.BeatsPerMinute <= 0 || this.FlatLine)
		{
			this.LastUpdate = Time.time;
		}
		else if (Time.time - this.LastUpdate >= this.BeatsPerSecond)
		{
			this.LastUpdate = Time.time;
			base.StartCoroutine(this.PerformBlip());
		}
	}

	private IEnumerator PerformBlip()
	{
		if (this.bFlatLinePlayed)
		{
			this.bFlatLinePlayed = false;
		}
		if (!this.bFlatLinePlayed)
		{
			this.PlayHeartSound(SWP_HeartRateMonitor.SoundType.HeartBeat1, this.SoundVolume);
		}
		this.NewClone.transform.position = new Vector3(this.NewClone.transform.position.x, 10f * this.BlipMonitorHeightModifier + UnityEngine.Random.Range(0f, 2f * this.BlipMonitorHeightModifier) + this.BlipOffset.y, this.BlipOffset.z);
		yield return new WaitForSeconds(0.03f);
		this.NewClone.transform.position = new Vector3(this.NewClone.transform.position.x, -5f * this.BlipMonitorHeightModifier - UnityEngine.Random.Range(0f, 3f * this.BlipMonitorHeightModifier) + this.BlipOffset.y, this.BlipOffset.z);
		yield return new WaitForSeconds(0.02f);
		this.NewClone.transform.position = new Vector3(this.NewClone.transform.position.x, 3f * this.BlipMonitorHeightModifier + UnityEngine.Random.Range(0f, 2f * this.BlipMonitorHeightModifier) + this.BlipOffset.y, this.BlipOffset.z);
		yield return new WaitForSeconds(0.02f);
		this.NewClone.transform.position = new Vector3(this.NewClone.transform.position.x, 2f * this.BlipMonitorHeightModifier + UnityEngine.Random.Range(0f, 1f * this.BlipMonitorHeightModifier) + this.BlipOffset.y, this.BlipOffset.z);
		yield return new WaitForSeconds(0.02f);
		this.NewClone.transform.position = new Vector3(this.NewClone.transform.position.x, this.BlipOffset.y, this.BlipOffset.z);
		yield return new WaitForSeconds(0.2f);
		if (!this.bFlatLinePlayed)
		{
			this.PlayHeartSound(SWP_HeartRateMonitor.SoundType.HeartBeat2, this.SoundVolume);
		}
		yield break;
	}

	private void CreateClone()
	{
		this.NewClone = UnityEngine.Object.Instantiate<GameObject>(this.Blip, new Vector3(this.BlipOffset.x, this.BlipOffset.y, this.BlipOffset.z), Quaternion.identity);
		this.NewClone.transform.parent = base.gameObject.transform;
		this.NewClone.GetComponentInChildren<TrailRenderer>().startWidth = this.BlipTrailStartSize;
		this.NewClone.GetComponentInChildren<TrailRenderer>().endWidth = this.BlipTrailEndSize;
		this.NewClone.transform.localScale = new Vector3(this.BlipSize, this.BlipSize, this.BlipSize);
		if (this.ShowBlip)
		{
			this.NewClone.GetComponent<MeshRenderer>().enabled = true;
		}
		else
		{
			this.NewClone.GetComponent<MeshRenderer>().enabled = false;
		}
	}

	private IEnumerator WaitThenDestroy(GameObject OldClone)
	{
		OldClone.GetComponent<MeshRenderer>().enabled = false;
		yield return new WaitForSeconds(this.TrailTime);
		UnityEngine.Object.DestroyObject(OldClone);
		yield break;
	}

	private void PlayHeartSound(SWP_HeartRateMonitor.SoundType _SoundType, float fSoundVolume)
	{
		if (!this.EnableSound)
		{
			return;
		}
		if (_SoundType == SWP_HeartRateMonitor.SoundType.HeartBeat1)
		{
			base.GetComponent<AudioSource>().PlayOneShot(this.Heart1Sound, fSoundVolume);
		}
		else if (_SoundType == SWP_HeartRateMonitor.SoundType.HeartBeat2)
		{
			base.GetComponent<AudioSource>().PlayOneShot(this.Heart2Sound, fSoundVolume);
		}
		else if (_SoundType == SWP_HeartRateMonitor.SoundType.Flatline)
		{
			base.GetComponent<AudioSource>().PlayOneShot(this.FlatlineSound, fSoundVolume);
		}
	}

	public void SetHeartRateColour(Color _NewColour)
	{
		if (this.MainMaterial == null)
		{
			throw new ArgumentException("You are trying to change the colour without having the 'MainMaterial' set in the control.  It must be set to the 'BlipMaterial' in order to use the colour changer.");
		}
		this.MainMaterial.SetColor("_TintColor", _NewColour);
	}

	public int BeatsPerMinute = 90;

	public bool FlatLine;

	public bool ShowBlip = true;

	public GameObject Blip;

	public float BlipSize = 1f;

	public float BlipTrailStartSize = 0.2f;

	public float BlipTrailEndSize = 0.1f;

	public float BlipMonitorWidth = 40f;

	public float BlipMonitorHeightModifier = 1f;

	public bool EnableSound = true;

	public float SoundVolume = 1f;

	public AudioClip Heart1Sound;

	public AudioClip Heart2Sound;

	public AudioClip FlatlineSound;

	private bool bFlatLinePlayed;

	private float LineSpeed = 0.3f;

	private GameObject NewClone;

	private float TrailTime;

	private float BeatsPerSecond;

	private float LastUpdate;

	private Vector3 BlipOffset = Vector3.zero;

	private float DisplayXEnd;

	public Material MainMaterial;

	public Color NormalColour = new Color(0f, 1f, 0f, 1f);

	public Color MediumColour = new Color(1f, 1f, 0f, 1f);

	public Color BadColour = new Color(1f, 0f, 0f, 1f);

	public Color FlatlineColour = new Color(1f, 0f, 0f, 1f);

	private enum SoundType
	{
		HeartBeat1,
		HeartBeat2,
		Flatline
	}
}
