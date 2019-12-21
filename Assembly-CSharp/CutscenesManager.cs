using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutscenesManager : MonoBehaviour
{
	public static CutscenesManager Get()
	{
		return CutscenesManager.s_Instance;
	}

	private void Awake()
	{
		CutscenesManager.s_Instance = this;
		PlayableDirector[] componentsInChildren = base.transform.GetComponentsInChildren<PlayableDirector>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			this.m_Cutscenes.Add(componentsInChildren[i].gameObject.name.ToLower(), componentsInChildren[i]);
		}
	}

	private void Start()
	{
		this.m_CharacterController = Player.Get().GetComponent<CharacterControllerProxy>();
	}

	public PlayableDirector GetCutscene(string name)
	{
		PlayableDirector playableDirector = null;
		this.m_Cutscenes.TryGetValue(name.ToLower(), out playableDirector);
		if (!playableDirector)
		{
			DebugUtils.Assert("ERROR - Can't find cutscene - " + name, true, DebugUtils.AssertType.Info);
			return null;
		}
		return playableDirector;
	}

	public void PlayCutscene(string name)
	{
		if (Scenario.Get().m_IsLoading)
		{
			return;
		}
		PlayableDirector cutscene = this.GetCutscene(name);
		if (!cutscene)
		{
			return;
		}
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().Deactivate();
		}
		this.m_CurrentCutscene = cutscene;
		this.m_CurrentCutscene.Play();
		this.m_PlayTime = 0f;
		Watch.Get().gameObject.SetActive(true);
		Player.Get().StartController(PlayerControllerType.PlayerCutscene);
		PlayableDirectorEx component = cutscene.gameObject.GetComponent<PlayableDirectorEx>();
		if (component != null)
		{
			this.m_PlayableDirectorEx = component;
			if (component.m_FollowOffsetHelper)
			{
				this.m_StoreStartPosition = true;
				return;
			}
		}
		else
		{
			DebugUtils.Assert("Missing PlayableDirectorEx in cutscene - " + name, true, DebugUtils.AssertType.Info);
		}
	}

	private void LateUpdate()
	{
		if (this.m_StoreStartPosition && this.m_PlayableDirectorEx && this.m_PlayableDirectorEx.m_FollowOffsetHelper)
		{
			this.m_StartPos = this.m_PlayableDirectorEx.m_Transform.position - this.m_PlayableDirectorEx.m_RefTransform.position;
			this.m_StoreStartPosition = false;
			this.m_PrevPos = this.m_StartPos;
			this.m_LastPos = this.m_StartPos;
			this.m_StartRot = Quaternion.Inverse(this.m_PlayableDirectorEx.m_RefTransform.rotation) * this.m_PlayableDirectorEx.m_Transform.rotation;
			this.m_PrevRot = this.m_StartRot;
			this.m_LastRot = this.m_StartRot;
			this.m_CharacterController.detectCollisions = false;
		}
		if (this.m_PlayableDirectorEx && this.m_PlayableDirectorEx.m_FollowOffsetHelper && this.m_CurrentCutscene.time >= (double)this.m_PlayTime)
		{
			this.m_PrevPos = this.m_LastPos;
			this.m_LastPos = this.m_PlayableDirectorEx.m_Transform.position - this.m_PlayableDirectorEx.m_RefTransform.position;
			this.m_PrevRot = this.m_LastRot;
			this.m_LastRot = Quaternion.Inverse(this.m_PlayableDirectorEx.m_RefTransform.rotation) * this.m_PlayableDirectorEx.m_Transform.rotation;
			Vector3 b = this.m_LastPos - this.m_PrevPos;
			this.m_CharacterController.gameObject.transform.position += b;
			this.m_CharacterController.gameObject.transform.rotation *= Quaternion.Inverse(this.m_PrevRot) * this.m_LastRot;
			float num = Vector3.Angle(this.m_CharacterController.transform.forward, Vector3.forward);
			Vector2 zero = Vector2.zero;
			if (Vector3.Dot(Vector3.right, this.m_CharacterController.transform.forward) < 0f)
			{
				num *= -1f;
			}
			zero.x = num;
			Player.Get().m_FPPController.SetLookDev(zero);
		}
		if (this.m_PlayableDirectorEx && !this.m_PlayableDirectorEx.m_Looped && this.m_CurrentCutscene && (this.m_CurrentCutscene.time >= this.m_CurrentCutscene.duration || this.m_CurrentCutscene.time < (double)this.m_PlayTime))
		{
			this.StopCutscene();
		}
		if (this.m_CurrentCutscene)
		{
			this.m_PlayTime = (float)this.m_CurrentCutscene.time;
		}
	}

	public void StopCutscene()
	{
		if (!this.m_CurrentCutscene)
		{
			return;
		}
		this.m_CurrentCutscene.Stop();
		this.m_CurrentCutscene = null;
		if (this.m_PlayableDirectorEx != null && this.m_PlayableDirectorEx.m_FollowOffsetHelper)
		{
			this.m_CharacterController.detectCollisions = true;
		}
		this.m_PlayableDirectorEx = null;
		Watch.Get().gameObject.SetActive(false);
		Player.Get().StopController(PlayerControllerType.PlayerCutscene);
		Player.Get().m_IsInAir = false;
		Player.Get().m_LastPosOnGround = Player.Get().transform.position;
		Player.Get().m_LastTimeOnGround = Time.time;
	}

	public bool IsCutscenePlaying()
	{
		return this.m_CurrentCutscene;
	}

	public bool IsCutsceneTimeGreaterOrEqual(string cutscene_name, float time)
	{
		return this.m_CurrentCutscene && !(this.m_CurrentCutscene.name != cutscene_name) && this.m_CurrentCutscene.time >= (double)time;
	}

	private Dictionary<string, PlayableDirector> m_Cutscenes = new Dictionary<string, PlayableDirector>();

	[HideInInspector]
	public PlayableDirector m_CurrentCutscene;

	private float m_PlayTime;

	private static CutscenesManager s_Instance;

	private PlayableDirectorEx m_PlayableDirectorEx;

	private Vector3 m_StartPos = Vector3.zero;

	private Vector3 m_PrevPos = Vector3.zero;

	private Vector3 m_LastPos = Vector3.zero;

	private bool m_StoreStartPosition;

	private Quaternion m_StartRot = Quaternion.identity;

	private Quaternion m_PrevRot = Quaternion.identity;

	private Quaternion m_LastRot = Quaternion.identity;

	private CharacterControllerProxy m_CharacterController;
}
