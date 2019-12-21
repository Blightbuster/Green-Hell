using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UILoadingStreamer : MonoBehaviour
{
	private void Awake()
	{
		foreach (Streamer streamer in this.streamers)
		{
			streamer.loadingStreamer = this;
			streamer.showLoadingScreen = true;
		}
		this.progressImg.fillAmount = 0f;
	}

	private void Update()
	{
		if (this.streamers.Length != 0)
		{
			bool flag = true;
			this.progressImg.fillAmount = 0f;
			foreach (Streamer streamer in this.streamers)
			{
				this.progressImg.fillAmount += streamer.LoadingProgress / (float)this.streamers.Length;
				flag = (flag && streamer.initialized);
			}
			if (flag && this.progressImg.fillAmount >= 1f)
			{
				if (this.onDone != null)
				{
					this.onDone.Invoke();
				}
				base.StartCoroutine(this.TurnOff());
				return;
			}
		}
		else
		{
			Debug.Log("No streamer Attached");
		}
	}

	public IEnumerator TurnOff()
	{
		yield return new WaitForSeconds(this.waitTime);
		base.gameObject.SetActive(false);
		yield break;
		yield break;
	}

	public void Show()
	{
		this.progressImg.fillAmount = 0f;
		base.gameObject.SetActive(true);
	}

	[Tooltip("List of streamers objects that should affect loading screen. Drag and drop here all your streamer objects from scene hierarchy which should be used in loading screen.")]
	public Streamer[] streamers;

	public Image progressImg;

	[Tooltip("Time in seconds that you give your loading screen to get data from whole streamers about scene that they must load before loading screen will be switched off.")]
	public float waitTime = 2f;

	public UnityEvent onDone;
}
