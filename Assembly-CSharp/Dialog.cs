using System;
using System.Collections.Generic;
using UnityEngine;

public class Dialog : IInputsReceiver
{
	public Dialog(string name, string group, AudioSource source)
	{
		this.m_Name = name;
		this.m_Group = ((group == null || group == string.Empty) ? DialogsManager.s_GroupALL : group);
		this.m_AudioSource = source;
	}

	public void Load(Key key, bool editor)
	{
		for (int i = 0; i < key.GetKeysCount(); i++)
		{
			Key key2 = key.GetKey(i);
			if (key2.GetName() == "Node")
			{
				DialogNode dialogNode = new DialogNode(this, key2.GetVariable(0).SValue);
				dialogNode.Load(key2);
				if (this.m_NodesMap.ContainsKey(dialogNode.m_ID))
				{
					Debug.Log("[Dialog:Load] ERROR - duplicated DialogNode names " + dialogNode.m_Name + " in Dialog " + this.m_Name);
				}
				else
				{
					this.m_Nodes.Add(dialogNode);
					this.m_NodesMap.Add(dialogNode.m_ID, dialogNode);
				}
			}
		}
		foreach (DialogNode dialogNode2 in this.m_Nodes)
		{
			dialogNode2.Setup(editor);
		}
		if (!editor)
		{
			foreach (DialogNode dialogNode3 in this.m_Nodes)
			{
				if (!dialogNode3.m_Additional && dialogNode3.m_Additionals.Count > 0)
				{
					DialogNode dialogNode4 = dialogNode3.m_Additionals[0];
					while (dialogNode4.m_Additionals.Count > 0)
					{
						dialogNode4 = dialogNode4.m_Additionals[0];
						dialogNode3.m_Additionals.Add(dialogNode4);
					}
				}
			}
		}
		foreach (DialogNode dialogNode5 in this.m_Nodes)
		{
			if (dialogNode5.m_Prevs.Count == 0)
			{
				this.m_RootNodes.Add(dialogNode5);
			}
		}
	}

	public void Start()
	{
		if (this.m_RootNodes.Count == 0)
		{
			Debug.Log("ERROR - dialog " + this.m_Name + " does not have root node!!!");
			this.SetState(Dialog.State.Finished);
			return;
		}
		if (this.m_RootNodes.Count == 1)
		{
			this.PlayNode(this.m_RootNodes[0]);
		}
		else
		{
			HUDSelectDialogNode.Get().ShowNodeSelection(this.m_RootNodes, DialogsManager.Get().m_DefaultReplyTime);
			this.SetState(Dialog.State.WaitingForDecision);
		}
		InputsManager.Get().RegisterReceiver(this);
	}

	private float GetNodeLength(DialogNode node)
	{
		if (node.m_Clip != null)
		{
			return node.m_Clip.length;
		}
		if (node.m_Wait <= 0f)
		{
			return Mathf.Max(2f, (float)GreenHellGame.Instance.GetLocalization().Get(node.m_Name, true).Length / 20f);
		}
		return node.m_Wait;
	}

	private void PlayNode(DialogNode node)
	{
		if (node.m_Object == null && node.m_ObjectName.Length > 0)
		{
			node.m_Object = MainLevel.Instance.GetUniqueObject(node.m_ObjectName);
			node.m_Animator = (node.m_Object ? node.m_Object.GetComponent<Animator>() : null);
			DebugUtils.Assert(node.m_Object != null, "Missing dialog object '" + node.m_ObjectName + "'!", true, DebugUtils.AssertType.Info);
		}
		if (this.m_AudioSource)
		{
			if (node.m_Object)
			{
				this.m_AudioSource.transform.position = node.m_Object.transform.position;
				this.m_AudioSource.spatialBlend = 1f;
			}
			else
			{
				this.m_AudioSource.transform.position = Player.Get().transform.position;
				this.m_AudioSource.spatialBlend = 0f;
			}
		}
		if (node.m_WTSoundBefore)
		{
			if (this.m_AudioSource)
			{
				this.m_AudioSource.clip = null;
			}
			if (node.m_WTSoundBeforeName != string.Empty)
			{
				foreach (AudioClip audioClip in DialogsManager.Get().m_WTBeforeSounds)
				{
					if (audioClip.name == node.m_WTSoundBeforeName)
					{
						if (this.m_AudioSource)
						{
							this.m_AudioSource.clip = audioClip;
							break;
						}
						break;
					}
				}
			}
			if (this.m_AudioSource)
			{
				if (!this.m_AudioSource.clip)
				{
					this.m_AudioSource.clip = DialogsManager.Get().m_WTBeforeSounds[UnityEngine.Random.Range(0, DialogsManager.Get().m_WTBeforeSounds.Count)];
				}
				if (!this.m_AudioSource.clip)
				{
					this.m_AudioSource.clip = DialogsManager.Get().m_WTAfterSounds[UnityEngine.Random.Range(0, DialogsManager.Get().m_WTAfterSounds.Count)];
				}
			}
			this.m_CurrentNodeLength = 1f;
			this.SetState(Dialog.State.PlayingWTSoundBefore);
		}
		else
		{
			if (this.m_AudioSource)
			{
				this.m_AudioSource.clip = node.m_Clip;
			}
			this.m_CurrentNodeLength = this.GetNodeLength(node);
			this.SetState(Dialog.State.Playing);
			if (node.m_Animator)
			{
				node.m_Animator.CrossFadeInFixedTime(node.m_Clip.name, 0f, 1);
			}
			if (node.m_PlayerAnimTrigger != -1)
			{
				Player.Get().m_Animator.SetTrigger(node.m_PlayerAnimTrigger);
			}
		}
		DialogsManager.Get().OnStartNode(node);
		if (node.m_ValueName != string.Empty)
		{
			ScenarioManager.Get().SetBoolVariable(node.m_ValueName, node.m_Value);
		}
		if (this.m_AudioSource)
		{
			this.m_AudioSource.Play();
		}
		this.m_CurrentNode = node;
		this.m_StartNodeTime = Time.time;
		this.CheckIfPlayerSpeaking();
	}

	public void Stop(bool finish)
	{
		if (this.m_AudioSource != null)
		{
			this.m_AudioSource.Stop();
			this.m_AudioSource.clip = null;
		}
		this.m_CurrentNode = null;
		this.SetState(Dialog.State.Finished);
		InputsManager.Get().UnregisterReceiver(this);
		HUDSelectDialogNode.Get().HideNodeSelection();
		DialogsManager.Get().OnStopDialog(this, finish);
	}

	public bool CanReceiveAction()
	{
		return this.m_State == Dialog.State.Playing || this.m_State == Dialog.State.PlayingWTSoundAfter || this.m_State == Dialog.State.PlayingWTSoundBefore;
	}

	public bool CanReceiveActionPaused()
	{
		return false;
	}

	public void OnInputAction(InputActionData action_data)
	{
		if (GreenHellGame.DEBUG && action_data.m_Action == InputsManager.InputAction.SkipDialogNode && this.m_CurrentNode != null)
		{
			this.FinishNode();
		}
	}

	private void ResetAdditional()
	{
		this.m_AdditionalStarted = false;
		this.m_CurrentAdditionalNode = null;
		this.m_AdditionalStartTime = 0f;
	}

	public void Update()
	{
		if (this.m_Paused)
		{
			this.m_StartNodeTime += Time.deltaTime;
			this.m_StartStateTime += Time.deltaTime;
			this.m_AdditionalStartTime += Time.deltaTime;
			return;
		}
		if (this.m_AudioSource.isPlaying)
		{
			return;
		}
		if (this.m_State == Dialog.State.WaitingForDecision && this.m_PrevNode != null)
		{
			float num = (this.m_PrevNode != null) ? this.m_PrevNode.m_ReplyTime : DialogsManager.Get().m_DefaultReplyTime;
			if (num == 0f)
			{
				num = float.MaxValue;
			}
			if (Time.time - this.m_StartStateTime >= num)
			{
				if (this.m_PrevNode.m_FinishIfNoReply)
				{
					this.Stop(true);
					return;
				}
				HUDSelectDialogNode.Get().OnSelect(this.m_PrevNode.m_Nexts.Count - 1);
				return;
			}
			else if (this.m_PrevNode.m_Additionals.Count > 0)
			{
				if (this.m_AdditionalStarted)
				{
					float nodeLength = this.GetNodeLength(this.m_CurrentAdditionalNode);
					if (Time.time - this.m_AdditionalStartTime >= nodeLength)
					{
						this.ResetAdditional();
					}
				}
				if (!this.m_AdditionalStarted)
				{
					if (this.m_PrevNode.m_Object)
					{
						this.m_AudioSource.transform.position = this.m_PrevNode.m_Object.transform.position;
						this.m_AudioSource.spatialBlend = 1f;
					}
					else
					{
						this.m_AudioSource.transform.position = Player.Get().transform.position;
						this.m_AudioSource.spatialBlend = 0f;
					}
					this.m_CurrentAdditionalNode = this.m_PrevNode.m_Additionals[this.m_PrevNode.m_AdditionalIndex];
					this.m_AudioSource.clip = this.m_CurrentAdditionalNode.m_Clip;
					this.m_AudioSource.Play();
					this.m_PrevNode.m_AdditionalIndex++;
					if (this.m_PrevNode.m_AdditionalIndex >= this.m_PrevNode.m_Additionals.Count)
					{
						this.m_PrevNode.m_AdditionalIndex = 0;
					}
					this.m_AdditionalStarted = true;
					this.m_AdditionalStartTime = Time.time;
					return;
				}
			}
		}
		else if (Time.time - this.m_StartNodeTime >= this.m_CurrentNodeLength)
		{
			if (this.m_State == Dialog.State.PlayingWTSoundBefore)
			{
				this.m_AudioSource.clip = this.m_CurrentNode.m_Clip;
				this.m_AudioSource.Play();
				this.m_StartNodeTime = Time.time;
				string text = GreenHellGame.Instance.GetLocalization().Get(this.m_CurrentNode.m_Name, true);
				this.m_CurrentNodeLength = ((this.m_AudioSource.clip != null) ? this.m_AudioSource.clip.length : ((this.m_CurrentNode.m_Wait > 0f) ? this.m_CurrentNode.m_Wait : Mathf.Max(2f, (float)text.Length / 20f)));
				this.SetState(Dialog.State.Playing);
				return;
			}
			if (this.m_State == Dialog.State.Playing && this.m_CurrentNode.m_WTSoundAfter)
			{
				this.m_AudioSource.clip = null;
				if (this.m_CurrentNode.m_WTSoundAfterName != string.Empty)
				{
					foreach (AudioClip audioClip in DialogsManager.Get().m_WTAfterSounds)
					{
						if (audioClip.name == this.m_CurrentNode.m_WTSoundAfterName)
						{
							this.m_AudioSource.clip = audioClip;
							break;
						}
					}
				}
				if (!this.m_AudioSource.clip)
				{
					this.m_AudioSource.clip = DialogsManager.Get().m_WTAfterSounds[UnityEngine.Random.Range(0, DialogsManager.Get().m_WTAfterSounds.Count)];
				}
				if (!this.m_AudioSource.clip)
				{
					this.m_AudioSource.clip = DialogsManager.Get().m_WTBeforeSounds[UnityEngine.Random.Range(0, DialogsManager.Get().m_WTBeforeSounds.Count)];
				}
				this.m_CurrentNodeLength = 1f;
				this.SetState(Dialog.State.PlayingWTSoundAfter);
				this.m_AudioSource.Play();
				this.m_StartNodeTime = Time.time;
				return;
			}
			this.FinishNode();
		}
	}

	public void FinishNode()
	{
		this.m_PrevNode = this.m_CurrentNode;
		this.m_CurrentNode = null;
		if (this.m_Queue.Count > 0)
		{
			this.PlayNode(this.m_Queue[0]);
			this.m_Queue.RemoveAt(0);
			return;
		}
		if (this.m_PrevNode != null)
		{
			if (this.m_PrevNode.m_Additional)
			{
				this.m_PrevNode = this.m_PrevNode.m_Prevs[0];
				this.SetState(Dialog.State.WaitingForDecision);
				return;
			}
			int count = this.m_PrevNode.m_Nexts.Count;
			if (count == 0)
			{
				this.Stop(true);
				return;
			}
			if (count != 1)
			{
				HUDSelectDialogNode.Get().ShowNodeSelection(this.m_PrevNode.m_Nexts, this.m_PrevNode.m_ReplyTime);
				this.SetState(Dialog.State.WaitingForDecision);
			}
			else
			{
				if (this.m_PrevNode.m_ShowReply)
				{
					HUDSelectDialogNode.Get().ShowNodeSelection(this.m_PrevNode.m_Nexts, this.m_PrevNode.m_ReplyTime);
					this.SetState(Dialog.State.WaitingForDecision);
					return;
				}
				this.PlayNode(this.m_PrevNode.m_Nexts[0]);
				return;
			}
		}
	}

	public void OnSelectNode(DialogNode node)
	{
		if (this.m_State == Dialog.State.WaitingForDecision)
		{
			this.PlayNode(node);
			return;
		}
		this.m_Queue.Add(node);
	}

	public bool IsFinished()
	{
		return this.m_State == Dialog.State.Finished;
	}

	private void SetState(Dialog.State state)
	{
		this.m_State = state;
		this.m_StartStateTime = Time.time;
		this.ResetAdditional();
	}

	public void Pause()
	{
		this.m_Paused = true;
		if (this.m_AudioSource.isPlaying)
		{
			this.m_AudioSource.Pause();
			this.m_AudioPaused = true;
		}
	}

	public void UnPause()
	{
		this.m_Paused = false;
		if (this.m_AudioPaused)
		{
			this.m_AudioSource.UnPause();
			this.m_AudioPaused = false;
		}
	}

	private void CheckIfPlayerSpeaking()
	{
		this.m_LastNodePlayerSpeaking = false;
		using (List<DialogTextData>.Enumerator enumerator = this.m_CurrentNode.m_TextDatas.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_Text.Contains("Player"))
				{
					this.m_LastNodePlayerSpeaking = true;
					break;
				}
			}
		}
	}

	public bool IsPlayerSpeaking()
	{
		return this.m_LastNodePlayerSpeaking && this.m_AudioSource && this.m_AudioSource.isPlaying;
	}

	public string m_Name = string.Empty;

	public string m_Group = string.Empty;

	public Dictionary<int, DialogNode> m_NodesMap = new Dictionary<int, DialogNode>();

	public List<DialogNode> m_Nodes = new List<DialogNode>();

	public List<DialogNode> m_RootNodes = new List<DialogNode>();

	public List<DialogNode> m_Queue = new List<DialogNode>();

	public bool m_ShownInSelectDialog;

	public DialogNode m_CurrentNode;

	public DialogNode m_PrevNode;

	private float m_CurrentNodeLength;

	private float m_StartNodeTime;

	private AudioSource m_AudioSource;

	private float m_StartStateTime;

	public DialogNode m_CurrentAdditionalNode;

	private bool m_AdditionalStarted;

	private float m_AdditionalStartTime;

	public bool m_Paused;

	private bool m_AudioPaused;

	private bool m_LastNodePlayerSpeaking;

	private Dialog.State m_State;

	private enum State
	{
		None,
		PlayingWTSoundBefore,
		Playing,
		PlayingWTSoundAfter,
		WaitingForDecision,
		Finished
	}
}
