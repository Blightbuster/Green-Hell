using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class HUDMovie : HUDBase, IInputsReceiver
{
	public static HUDMovie Get()
	{
		return HUDMovie.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDMovie.s_Instance = this;
		this.m_VideoPlayer = base.GetComponent<VideoPlayer>();
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Movie);
	}

	protected override bool ShouldShow()
	{
		return this.m_Type != MovieType.None;
	}

	protected override void OnShow()
	{
		this.m_TextSkip.gameObject.SetActive(false);
		Inventory3DManager.Get().Deactivate();
	}

	public override void OnSetGroup(bool in_group)
	{
		if (in_group)
		{
			this.m_TextSkip.gameObject.SetActive(false);
		}
	}

	public override void ConstantUpdate()
	{
		base.ConstantUpdate();
		if (this.m_Type != MovieType.None)
		{
			if (this.m_Type == MovieType.Simple)
			{
				if (this.m_VideoPlayer.isPlaying)
				{
					this.m_VideoJustStarted = false;
				}
				if (this.m_PlayingMovieClip != null && !this.m_VideoPlayer.isPlaying && !this.m_VideoJustStarted)
				{
					this.StopMovie();
				}
			}
			else if (this.m_Type == MovieType.WithFade)
			{
				this.UpdateMovieWithFadeState();
			}
		}
	}

	private void UpdateMovieWithFadeState()
	{
		switch (this.m_State)
		{
		case MovieWithFadeState.PreFadeOut:
		{
			Color color = this.m_FadeImage.color;
			color.a += Time.unscaledDeltaTime;
			this.m_FadeImage.color = color;
			this.m_FadeImage.gameObject.SetActive(true);
			if (color.a > 1f)
			{
				this.SetState(MovieWithFadeState.Movie);
			}
			break;
		}
		case MovieWithFadeState.Movie:
			if (this.m_VideoPlayer.frame > 2L)
			{
				Color color2 = this.m_FadeImage.color;
				color2.a = 0f;
				this.m_FadeImage.color = color2;
				this.m_FadeImage.gameObject.SetActive(false);
			}
			if (this.m_VideoPlayer.isPlaying)
			{
				this.m_VideoJustStarted = false;
			}
			if (this.m_PlayingMovieClip != null && !this.m_VideoPlayer.isPlaying && !this.m_VideoJustStarted)
			{
				this.SetState(MovieWithFadeState.PostFadeOut);
			}
			break;
		case MovieWithFadeState.PostFadeOut:
		{
			Color color3 = this.m_FadeImage.color;
			color3.a += Time.unscaledDeltaTime;
			this.m_FadeImage.color = color3;
			this.m_FadeImage.gameObject.SetActive(true);
			if (color3.a > 1f)
			{
				this.StopMovieWithFade();
				this.SetState(MovieWithFadeState.PostFadeIn);
			}
			break;
		}
		case MovieWithFadeState.PostFadeIn:
		{
			Color color4 = this.m_FadeImage.color;
			color4.a -= Time.unscaledDeltaTime;
			this.m_FadeImage.color = color4;
			if (color4.a < 0f)
			{
				this.SetState(MovieWithFadeState.None);
				this.m_FadeImage.gameObject.SetActive(false);
			}
			break;
		}
		}
	}

	public bool PlayMovie(string movie_name)
	{
		this.m_Type = MovieType.Simple;
		this.m_TextSkip.text = GreenHellGame.Instance.GetLocalization().Get("HUDSkipMovie");
		VideoClip videoClip = Resources.Load("Movies/" + movie_name) as VideoClip;
		if (videoClip == null)
		{
			return false;
		}
		this.m_VideoPlayer.clip = videoClip;
		this.m_VideoPlayer.SetTargetAudioSource(0, base.GetComponent<AudioSource>());
		this.m_VideoPlayer.Play();
		this.m_Texture.enabled = true;
		this.m_Texture.gameObject.SetActive(true);
		this.m_BG.gameObject.SetActive(true);
		this.m_PlayingMovieClip = videoClip;
		this.m_MainCamera = Camera.main;
		this.m_MainCamera.enabled = false;
		Inventory3DManager.Get().m_Camera.enabled = false;
		this.m_Camera.enabled = true;
		this.m_VideoJustStarted = true;
		return true;
	}

	public bool PlayMovieWithFade(string movie_name)
	{
		this.m_Type = MovieType.WithFade;
		this.m_MovieName = movie_name;
		this.SetState(MovieWithFadeState.PreFadeOut);
		return true;
	}

	private void PlayMovieWithFadeInternal()
	{
		this.m_TextSkip.text = GreenHellGame.Instance.GetLocalization().Get("HUDSkipMovie");
		VideoClip videoClip = Resources.Load("Movies/" + this.m_MovieName) as VideoClip;
		if (videoClip == null)
		{
			this.SetState(MovieWithFadeState.PreFadeOut);
			return;
		}
		this.m_VideoPlayer.clip = videoClip;
		this.m_VideoPlayer.SetTargetAudioSource(0, base.GetComponent<AudioSource>());
		this.m_VideoPlayer.Play();
		this.m_Texture.enabled = true;
		this.m_Texture.gameObject.SetActive(true);
		this.m_BG.gameObject.SetActive(true);
		this.m_PlayingMovieClip = videoClip;
		this.m_MainCamera = Camera.main;
		this.m_MainCamera.enabled = false;
		Inventory3DManager.Get().m_Camera.enabled = false;
		this.m_Camera.enabled = true;
		this.m_VideoJustStarted = true;
	}

	private void SetState(MovieWithFadeState state)
	{
		this.m_State = state;
		this.OnSetState(state);
	}

	private void OnSetState(MovieWithFadeState state)
	{
		switch (state)
		{
		case MovieWithFadeState.None:
			this.m_Type = MovieType.None;
			MainLevel.Instance.OnStopMovie();
			HUDManager.Get().SetActiveGroup(HUDManager.HUDGroup.Game);
			break;
		case MovieWithFadeState.PreFadeOut:
		{
			Color color = this.m_FadeImage.color;
			color.a = 0f;
			this.m_FadeImage.color = color;
			this.m_Texture.enabled = false;
			this.m_Texture.gameObject.SetActive(false);
			this.m_BG.gameObject.SetActive(false);
			HUDManager.Get().SetActiveGroup(HUDManager.HUDGroup.Movie);
			break;
		}
		case MovieWithFadeState.Movie:
			this.m_Type = MovieType.WithFade;
			this.PlayMovieWithFadeInternal();
			break;
		case MovieWithFadeState.PostFadeOut:
		{
			Color color2 = this.m_FadeImage.color;
			color2.a = 0f;
			this.m_FadeImage.color = color2;
			break;
		}
		case MovieWithFadeState.PostFadeIn:
		{
			Color color3 = this.m_FadeImage.color;
			color3.a = 1f;
			this.m_FadeImage.color = color3;
			this.m_Texture.enabled = false;
			this.m_Texture.gameObject.SetActive(false);
			this.m_BG.gameObject.SetActive(false);
			this.m_MainCamera.enabled = true;
			Inventory3DManager.Get().enabled = true;
			this.m_Camera.enabled = false;
			break;
		}
		}
	}

	private void StopMovie()
	{
		this.m_Type = MovieType.None;
		if (this.m_PlayingMovieClip != null)
		{
			this.m_VideoPlayer.Stop();
			this.m_PlayingMovieClip = null;
			this.m_Texture.enabled = false;
			this.m_Texture.gameObject.SetActive(false);
			this.m_BG.gameObject.SetActive(false);
			this.m_MainCamera.enabled = true;
			Inventory3DManager.Get().enabled = true;
			this.m_Camera.enabled = false;
			MainLevel.Instance.OnStopMovie();
			AudioSource component = base.gameObject.GetComponent<AudioSource>();
			component.Stop();
			component.clip = null;
			return;
		}
	}

	private void StopMovieWithFade()
	{
		if (this.m_PlayingMovieClip != null)
		{
			this.m_VideoPlayer.Stop();
			this.m_PlayingMovieClip = null;
		}
		AudioSource component = base.gameObject.GetComponent<AudioSource>();
		component.Stop();
		component.clip = null;
	}

	public void OnInputAction(InputsManager.InputAction action)
	{
		if (this.m_Type == MovieType.Simple)
		{
			if (action == InputsManager.InputAction.SkipMovie)
			{
				if (!this.m_TextSkip.gameObject.activeSelf)
				{
					this.m_TextSkip.gameObject.SetActive(true);
				}
				else
				{
					this.StopMovie();
				}
			}
		}
		else if (this.m_Type == MovieType.WithFade && action == InputsManager.InputAction.SkipMovie && this.m_State == MovieWithFadeState.Movie)
		{
			if (!this.m_TextSkip.gameObject.activeSelf)
			{
				this.m_TextSkip.gameObject.SetActive(true);
			}
			else
			{
				this.SetState(MovieWithFadeState.PostFadeOut);
			}
		}
	}

	public bool CanReceiveAction()
	{
		return this.m_PlayingMovieClip != null;
	}

	public MovieWithFadeState GetState()
	{
		return this.m_State;
	}

	public MovieType GetMovieType()
	{
		return this.m_Type;
	}

	private static HUDMovie s_Instance;

	public RawImage m_Texture;

	public RawImage m_BG;

	private VideoClip m_PlayingMovieClip;

	public Camera m_Camera;

	public Camera m_MainCamera;

	public Text m_TextSkip;

	public RawImage m_FadeImage;

	private MovieWithFadeState m_State;

	private MovieType m_Type;

	private string m_MovieName = string.Empty;

	private VideoPlayer m_VideoPlayer;

	private bool m_VideoJustStarted;
}
