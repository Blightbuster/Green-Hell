using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogsManager : MonoBehaviour
{
	public static DialogsManager Get()
	{
		return DialogsManager.s_Instance;
	}

	private void Awake()
	{
		DialogsManager.s_Instance = this;
	}

	private void Start()
	{
		this.Initialize();
	}

	public void Initialize()
	{
		List<string> list = new List<string>();
		list.Add("Sounds/Chatters/");
		list.Add("Sounds/TempSounds/Chatters/");
		for (int i = 0; i < list.Count; i++)
		{
			string path = list[i];
			foreach (AudioClip audioClip in Resources.LoadAll<AudioClip>(path))
			{
				string key = audioClip.name.ToLower();
				if (this.m_AllClips.ContainsKey(key))
				{
					Debug.Log("[DialogsManager:Start] ERROR - duplicated clip names " + audioClip.name);
				}
				else
				{
					this.m_AllClips.Add(key, audioClip);
				}
			}
		}
		this.m_AudioSource = base.transform.GetComponentInChildren<AudioSource>();
		this.m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
		this.m_AudioSource.maxDistance = 10f;
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("WTSounds.txt", true);
		for (int k = 0; k < scriptParser.GetKeysCount(); k++)
		{
			Key key2 = scriptParser.GetKey(k);
			if (key2.GetName() == "Before")
			{
				string[] array2 = key2.GetVariable(0).SValue.Split(new char[]
				{
					';'
				});
				for (int l = 0; l < array2.Length; l++)
				{
					AudioClip audioClip2 = Resources.Load<AudioClip>("Sounds/WT/" + array2[l]);
					if (!audioClip2)
					{
						audioClip2 = Resources.Load<AudioClip>("Sounds/TempSounds/WT/" + array2[l]);
					}
					if (!audioClip2)
					{
						DebugUtils.Assert("Can't find sound - " + array2[l], true, DebugUtils.AssertType.Info);
					}
					else
					{
						this.m_WTBeforeSounds.Add(audioClip2);
					}
				}
			}
			else if (key2.GetName() == "After")
			{
				string[] array3 = key2.GetVariable(0).SValue.Split(new char[]
				{
					';'
				});
				for (int m = 0; m < array3.Length; m++)
				{
					AudioClip audioClip3 = Resources.Load<AudioClip>("Sounds/WT/" + array3[m]);
					if (!audioClip3)
					{
						audioClip3 = Resources.Load<AudioClip>("Sounds/TempSounds/WT/" + array3[m]);
					}
					if (!audioClip3)
					{
						DebugUtils.Assert("Can't find sound - " + array3[m], true, DebugUtils.AssertType.Info);
					}
					else
					{
						this.m_WTAfterSounds.Add(audioClip3);
					}
				}
			}
		}
		this.LoadScript(false);
	}

	public void LoadScript(bool editor)
	{
		this.m_Dialogs.Clear();
		this.m_Groups.Clear();
		this.m_Groups.Add(DialogsManager.s_GroupALL);
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Dialogs.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Dialog")
			{
				Dialog dialog = new Dialog(key.GetVariable(0).SValue, key.GetVariable(1).SValue, this.m_AudioSource);
				dialog.Load(key, editor);
				if (this.m_Dialogs.ContainsKey(dialog.m_Name))
				{
					Debug.Log("[DialogsManager:LoadScript] ERROR - duplicated dialog names " + dialog.m_Name);
				}
				else
				{
					this.m_Dialogs.Add(dialog.m_Name, dialog);
				}
			}
			else if (key.GetName() == "Group")
			{
				this.m_Groups.Add(key.GetVariable(0).SValue);
			}
		}
	}

	public void StartDialog(string dialog_name)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			return;
		}
		this.StopDialog();
		string b = dialog_name.ToLower();
		Dialog dialog = null;
		foreach (string text in this.m_Dialogs.Keys)
		{
			if (text.ToLower() == b)
			{
				dialog = this.m_Dialogs[text];
				break;
			}
		}
		if (dialog == null)
		{
			DebugUtils.Assert("[DialogManager:Play] Can't find dialog - " + dialog_name, true, DebugUtils.AssertType.Info);
			return;
		}
		this.m_CurrentDialog = dialog;
		this.m_CurrentDialog.Start();
	}

	public void StopDialog()
	{
		if (this.m_CurrentDialog != null)
		{
			this.m_CurrentDialog.Stop();
		}
	}

	private void Update()
	{
		if (Player.Get().IsDead())
		{
			if (this.m_CurrentDialog != null)
			{
				this.m_CurrentDialog.Stop();
				this.m_CurrentDialog = null;
			}
			return;
		}
		if (this.m_CurrentDialog != null)
		{
			this.m_CurrentDialog.Update();
			if (this.m_CurrentDialog.IsFinished())
			{
				this.m_CurrentDialog = null;
			}
		}
		else if (GreenHellGame.DEBUG && Input.GetKeyDown(KeyCode.Keypad1) && this.m_DebugDialogName != string.Empty)
		{
			this.StartDialog(this.m_DebugDialogName);
		}
		if (this.m_CurrentDialog != null)
		{
			if (LoadingScreen.Get().m_Active && !this.m_CurrentDialog.m_Paused)
			{
				this.m_CurrentDialog.Pause();
			}
			else if (!LoadingScreen.Get().m_Active && this.m_CurrentDialog.m_Paused)
			{
				this.m_CurrentDialog.UnPause();
			}
		}
	}

	public void OnSelectNode(DialogNode node)
	{
		if (this.m_CurrentDialog != null)
		{
			this.m_CurrentDialog.OnSelectNode(node);
		}
	}

	public bool IsPlaying()
	{
		return this.m_CurrentDialog != null;
	}

	private static DialogsManager s_Instance;

	public Dictionary<string, Dialog> m_Dialogs = new Dictionary<string, Dialog>();

	[HideInInspector]
	public List<string> m_Groups = new List<string>();

	public static string s_GroupALL = "ALL";

	public Dialog m_CurrentDialog;

	public Dictionary<string, AudioClip> m_AllClips = new Dictionary<string, AudioClip>();

	[HideInInspector]
	public AudioSource m_AudioSource;

	public bool m_DebugLogMissingClips;

	public float m_DefaultReplyTime = 10f;

	[HideInInspector]
	public string m_DebugDialogName = string.Empty;

	[HideInInspector]
	public List<AudioClip> m_WTBeforeSounds = new List<AudioClip>();

	[HideInInspector]
	public List<AudioClip> m_WTAfterSounds = new List<AudioClip>();
}
