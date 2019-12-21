using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class WalkieTalkieController : PlayerController
{
	public static WalkieTalkieController Get()
	{
		return WalkieTalkieController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		WalkieTalkieController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.WalkieTalkie;
		if (this.m_WalkieTalkieObject == null)
		{
			this.m_WalkieTalkieObject = this.m_Player.transform.FindDeepChild("Walkie_TalkieInHand").gameObject;
		}
		if (this.m_WalkieTalkieAnimator == null)
		{
			this.m_WalkieTalkieAnimator = this.m_WalkieTalkieObject.GetComponent<Animator>();
		}
		this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
		this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
		this.m_ShowClips.Add((AudioClip)Resources.Load("Sounds/Player/inspect_arm_left_start_01"));
		this.m_HideClips.Add((AudioClip)Resources.Load("Sounds/Player/watch_hide_01"));
		this.m_ForceLightOnNodes.Add("Dream_01_GiftPickup_Mia_03_WT");
		this.m_ForceLightOnNodes.Add("AfterDreamFirstCall_Girl_01");
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.Show();
		this.SetState(WalkieTalkieControllerState.TakeOut);
		Player.Get().m_ShouldStartWalkieTalkieController = false;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.Hide();
		this.SetState(WalkieTalkieControllerState.None);
		Player.Get().m_ShouldStartWalkieTalkieController = false;
		this.HideWalkieTalkieObject();
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(2);
		if (currentAnimatorStateInfo.shortNameHash == this.m_WalkieTalkieTakeOut && this.m_Animator.GetInteger(this.m_IWalkieTalkie) != 7)
		{
			if (currentAnimatorStateInfo.normalizedTime > 0.7f || this.m_Animator.GetInteger(this.m_IWalkieTalkie) > 1)
			{
				this.m_WalkieTalkieObject.SetActive(true);
			}
			this.m_Animator.SetInteger(this.m_IWalkieTalkie, 2);
		}
		else if (currentAnimatorStateInfo.shortNameHash == this.m_WalkieTalkieHide)
		{
			this.Stop();
		}
		this.UpdateWTLight();
		if (this.m_WalkieTalkieObject && this.m_WalkieTalkieObject.activeSelf && this.m_Animator.GetCurrentAnimatorStateInfo(2).shortNameHash == this.m_IdleHash && !this.m_Animator.IsInTransition(2))
		{
			this.m_WalkieTalkieObject.SetActive(false);
		}
	}

	public void UpdateWTLight()
	{
		if (!this.m_WalkieTalkieObject.activeSelf)
		{
			return;
		}
		if (this.m_WalkieTalkieAnimator.GetBool(this.m_OnHash) != (this.m_WTState == WalkieTalkieController.WTState.On))
		{
			this.m_WalkieTalkieAnimator.SetBool(this.m_OnHash, this.m_WTState == WalkieTalkieController.WTState.On);
		}
	}

	private void SetState(WalkieTalkieControllerState state)
	{
		this.m_State = state;
		this.OnSetState(state);
	}

	private void OnSetState(WalkieTalkieControllerState state)
	{
		if (state == WalkieTalkieControllerState.None)
		{
			this.m_WalkieTalkieObject.SetActive(false);
			this.m_Animator.SetInteger(this.m_IWalkieTalkie, 0);
		}
	}

	public void OnStartNode(DialogNode node)
	{
		if (this.m_ForceLightOnNodes.Contains(node.m_Name))
		{
			this.m_WTState = WalkieTalkieController.WTState.On;
			this.UpdateWTLight();
		}
		if (!this.IsActive())
		{
			return;
		}
		if (node != null && node.m_ShowWalkieTalkie)
		{
			this.m_Animator.SetInteger(this.m_IWalkieTalkie, 3);
			this.SetState(WalkieTalkieControllerState.Talk);
		}
		else
		{
			this.m_Animator.SetInteger(this.m_IWalkieTalkie, 2);
			this.SetState(WalkieTalkieControllerState.Listen);
		}
		this.m_WTState = WalkieTalkieController.WTState.On;
		this.UpdateWTLight();
		if (node != null)
		{
			if (node.m_HideWT && this.m_Animator.GetInteger(this.m_IWalkieTalkie) != 0)
			{
				this.Hide();
				return;
			}
			if (!node.m_HideWT && this.m_Animator.GetInteger(this.m_IWalkieTalkie) == 0)
			{
				this.Show();
			}
		}
	}

	public void OnStopDialog(Dialog dialog)
	{
		this.m_Animator.SetInteger(this.m_IWalkieTalkie, 7);
		this.SetState(WalkieTalkieControllerState.Hide);
		this.m_WTState = WalkieTalkieController.WTState.Off;
		this.UpdateWTLight();
		if (dialog != null && (dialog.m_Name == "WakeUP" || dialog.m_Name == "Tutorial_LastDayCall_Player" || dialog.m_Name == "afterall_Player_04" || dialog.m_Name == "afterall_Player_10" || dialog.m_Name == "Refugess_IslandMap"))
		{
			this.HideWalkieTalkieObject();
			this.Stop();
		}
	}

	private void Show()
	{
		this.m_AudioSource.PlayOneShot(this.m_ShowClips[UnityEngine.Random.Range(0, this.m_ShowClips.Count)]);
		this.m_Animator.SetInteger(this.m_IWalkieTalkie, 1);
	}

	private void Hide()
	{
		this.m_AudioSource.PlayOneShot(this.m_HideClips[UnityEngine.Random.Range(0, this.m_HideClips.Count)]);
		this.m_Animator.SetInteger(this.m_IWalkieTalkie, 0);
		this.m_WTState = WalkieTalkieController.WTState.Off;
		this.UpdateWTLight();
	}

	public void HideWalkieTalkieObject()
	{
		if (this.m_WalkieTalkieObject)
		{
			this.m_WalkieTalkieObject.SetActive(false);
		}
	}

	public void UpdateWTActivity()
	{
		if (!this.IsActive() && !CutscenesManager.Get().IsCutscenePlaying() && !ScenarioManager.Get().IsDreamOrPreDream() && this.m_WalkieTalkieObject && this.m_WalkieTalkieObject.activeSelf)
		{
			this.m_WalkieTalkieObject.SetActive(false);
		}
	}

	private int m_IWalkieTalkie = Animator.StringToHash("WalkieTalkie");

	private int m_WalkieTalkieTakeOut = Animator.StringToHash("Walkie_Talkie_TakeOut");

	private int m_WalkieTalkieHide = Animator.StringToHash("WalkieTalkie_Hide");

	private int m_OnHash = Animator.StringToHash("On");

	private int m_IdleHash = Animator.StringToHash("Idle");

	private WalkieTalkieControllerState m_State;

	private GameObject m_WalkieTalkieObject;

	private static WalkieTalkieController s_Instance;

	private Animator m_WalkieTalkieAnimator;

	private AudioSource m_AudioSource;

	private List<AudioClip> m_ShowClips = new List<AudioClip>();

	private List<AudioClip> m_HideClips = new List<AudioClip>();

	private List<string> m_ForceLightOnNodes = new List<string>();

	private WalkieTalkieController.WTState m_WTState = WalkieTalkieController.WTState.Off;

	private enum WTState
	{
		On,
		Off
	}
}
