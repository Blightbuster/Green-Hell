using System;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
	public static LoadingScreen Get()
	{
		return LoadingScreen.s_Instance;
	}

	private void Awake()
	{
		LoadingScreen.s_Instance = this;
		this.m_CameraObject = new GameObject();
		this.m_CameraObject.name = "LoadingCamera";
		this.m_CameraObject.transform.parent = base.gameObject.transform;
		this.m_Camera = this.m_CameraObject.AddComponent<Camera>();
		this.m_Camera.depth = -1f;
		this.m_CameraObject.AddComponent<GUILayer>();
		base.gameObject.AddComponent<CanvasScaler>();
		base.gameObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
		this.m_CameraObject.SetActive(false);
		this.m_ProgressObject = base.gameObject.FindChild("LoadingText");
		this.m_ProgressText = this.m_ProgressObject.GetComponent<Text>();
		this.m_Image = base.gameObject.FindChild("LoadingBG").GetComponent<RawImage>();
		RectTransform component = this.m_ProgressObject.GetComponent<RectTransform>();
		Vector3 position = component.position;
		position.x = (float)Screen.width * 0.92f;
		position.y = (float)Screen.height * 0.04f;
		component.position = position;
		this.m_ProgressText.color = new Color(1f, 1f, 1f, 0.4f);
		Vector2 zero = Vector2.zero;
		zero.Set(0.0725f * (float)Screen.width, 0.11588f * (float)Screen.height);
		component.sizeDelta = zero;
		this.m_ProgressObject.SetActive(false);
	}

	public void Show(LoadingScreenState state)
	{
		this.m_State = state;
		this.m_Active = true;
		this.UpdateSize();
	}

	public void Hide()
	{
		this.m_State = LoadingScreenState.None;
		this.m_Image.gameObject.SetActive(false);
		this.m_Image.enabled = false;
		this.m_CameraObject.SetActive(false);
		this.m_ProgressObject.SetActive(false);
		this.m_Active = false;
		Music.Get().Stop(0.1f);
	}

	private void Update()
	{
		if (this.m_Active)
		{
			this.m_ProgressText.text = GreenHellGame.Instance.GetLocalization().Get("Loading");
			Color color = this.m_ProgressText.color;
			color.a = Mathf.Abs(Mathf.Sin(Time.time * 3f));
			this.m_ProgressText.color = color;
			this.UpdateSize();
		}
	}

	private void UpdateSize()
	{
		Vector2 zero = Vector2.zero;
		zero.Set((float)Screen.width, (float)Screen.height);
		this.m_Image.rectTransform.sizeDelta = zero;
		this.m_Image.gameObject.SetActive(true);
		this.m_Image.enabled = true;
		this.m_CameraObject.SetActive(true);
		this.m_ProgressObject.SetActive(true);
		this.m_ProgressText.text = GreenHellGame.Instance.GetLocalization().Get("Loading");
		RectTransform component = this.m_ProgressObject.GetComponent<RectTransform>();
		Vector3 position = component.position;
		position.x = (float)Screen.width * 0.92f;
		position.y = (float)Screen.height * 0.04f;
		component.position = position;
		this.m_ProgressText.color = new Color(1f, 1f, 1f, 0.4f);
		zero.Set(0.0725f * (float)Screen.width, 0.11588f * (float)Screen.height);
		component.sizeDelta = zero;
	}

	public static LoadingScreen s_Instance;

	public LoadingScreenState m_State;

	[HideInInspector]
	public RawImage m_Image;

	[HideInInspector]
	public Text m_ProgressText;

	[HideInInspector]
	public Camera m_Camera;

	[HideInInspector]
	public GameObject m_CameraObject;

	[HideInInspector]
	public GameObject m_ProgressObject;

	[HideInInspector]
	public bool m_Active;

	private const float m_ProgressX = 0.92f;

	private const float m_ProgressY = 0.04f;

	private const float m_ProgressWidth = 0.0725f;

	private const float m_ProgressHeight = 0.11588f;

	private Vector3 m_ProgressRotationVector = Vector3.zero;
}
