using System;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
	public static HUDManager Get()
	{
		return HUDManager.s_Instance;
	}

	private void Awake()
	{
		HUDManager.s_Instance = this;
	}

	private void Init()
	{
		this.m_CanvasGameObject = GameObject.Find("HUD");
		DebugUtils.Assert(this.m_CanvasGameObject, true);
		this.m_CanvasGameObject.SetActive(this.m_Active);
		this.m_HUDList = new HUDBase[this.m_CanvasGameObject.transform.childCount];
		for (int i = 0; i < this.m_CanvasGameObject.transform.childCount; i++)
		{
			GameObject gameObject = this.m_CanvasGameObject.transform.GetChild(i).gameObject;
			HUDBase component = gameObject.GetComponent<HUDBase>();
			this.m_HUDList[i] = component;
		}
		DebugUtils.Assert(this.m_HUDList.Length > 0, true);
		this.m_Initialized = true;
		this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
		this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
	}

	private void Start()
	{
		foreach (HUDBase hudbase in this.m_HUDList)
		{
			IInputsReceiver component = hudbase.gameObject.GetComponent<IInputsReceiver>();
			if (component != null)
			{
				InputsManager.Get().RegisterReceiver(component);
			}
			IObjectivesManagerObserver component2 = hudbase.gameObject.GetComponent<IObjectivesManagerObserver>();
			if (component2 != null)
			{
				ObjectivesManager.Get().RegisterObserver(component2);
			}
		}
	}

	public HUDBase GetHUD(Type type)
	{
		if (!this.m_Initialized)
		{
			this.Init();
		}
		foreach (HUDBase hudbase in this.m_HUDList)
		{
			if (hudbase.GetType() == type)
			{
				return hudbase;
			}
		}
		DebugUtils.Assert("[HUDBase GetHUD] Can't find find hud " + type.ToString(), true, DebugUtils.AssertType.Info);
		return null;
	}

	public void Update()
	{
		if (this.m_HUDList == null)
		{
			return;
		}
		foreach (HUDBase hudbase in this.m_HUDList)
		{
			if (hudbase.gameObject.activeSelf)
			{
				hudbase.ConstantUpdate();
			}
		}
		if (GreenHellGame.DEBUG)
		{
			if (Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.H))
			{
				this.m_DebugHideHUD = !this.m_DebugHideHUD;
			}
			if (Input.GetKeyDown(KeyCode.P))
			{
				HUDObjective3D.Get().m_Active = !HUDObjective3D.Get().m_Active;
			}
		}
	}

	private void LateUpdate()
	{
		if ((LoadingScreen.Get() != null && LoadingScreen.Get().m_Active) || this.m_DebugHideHUD)
		{
			this.ShowHUD(false);
		}
		else
		{
			this.ShowHUD(true);
		}
	}

	public void UpdateAfterCamera()
	{
		if (this.m_HUDList == null)
		{
			return;
		}
		foreach (HUDBase hudbase in this.m_HUDList)
		{
			if (hudbase.gameObject.activeSelf && hudbase.enabled)
			{
				hudbase.UpdateAfterCamera();
			}
		}
	}

	public void ShowHUD(bool show)
	{
		if (this.m_Active == show)
		{
			return;
		}
		this.m_Active = show;
		this.m_CanvasGameObject.SetActive(this.m_Active);
	}

	public void SetActiveGroup(HUDManager.HUDGroup group)
	{
		if (!this.m_Initialized)
		{
			this.Init();
		}
		foreach (HUDBase hudbase in this.m_HUDList)
		{
			bool flag = hudbase.IsInGroup(group);
			hudbase.gameObject.SetActive(flag);
			hudbase.OnSetGroup(flag);
		}
	}

	public void ShowDemoEnd()
	{
		this.GetHUD(typeof(HUDEndDemo)).gameObject.SetActive(true);
	}

	public void PlaySound(AudioClip clip)
	{
		if (this.m_AudioSource == null)
		{
			return;
		}
		this.m_AudioSource.clip = clip;
		this.m_AudioSource.Play();
	}

	public void PlaySound(string path)
	{
		AudioClip audioClip = (AudioClip)Resources.Load(path);
		if (audioClip != null)
		{
			this.m_AudioSource.clip = audioClip;
			this.m_AudioSource.Play();
		}
	}

	public void PlaySoundDelayed(string path, float delay)
	{
		AudioClip audioClip = (AudioClip)Resources.Load(path);
		if (audioClip != null)
		{
			this.m_AudioSource.clip = audioClip;
			this.m_AudioSource.PlayDelayed(delay);
		}
	}

	private bool m_Active = true;

	public GameObject m_CanvasGameObject;

	private HUDBase[] m_HUDList;

	private bool m_Initialized;

	private static HUDManager s_Instance;

	[HideInInspector]
	public bool m_DebugHideHUD;

	private AudioSource m_AudioSource;

	public enum HUDGroup
	{
		None,
		Game,
		Inventory,
		ItemsCombine = 4,
		Inventory3D = 8,
		FireCooking = 16,
		Parameters = 32,
		InspectionMinigame = 64,
		TwitchDemo = 128,
		Movie = 256,
		All = 2147483647
	}
}
