using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class DialogsManager : MonoBehaviour, ISaveLoad
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
		base.transform.position = Vector3.zero;
		this.Initialize();
	}

	public void Initialize()
	{
		List<string> list = new List<string>();
		list.Add("Sounds/Chatters/");
		list.Add("Sounds/TempSounds/Chatters/");
		for (int i = 0; i < list.Count; i++)
		{
			foreach (AudioClip audioClip in Resources.LoadAll<AudioClip>(list[i]))
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
		this.m_Groups.Sort();
		this.m_Groups.Insert(0, DialogsManager.s_GroupALL);
	}

	public void UnlockAllDialogs()
	{
		foreach (string name in this.m_Dialogs.Keys)
		{
			this.UnlockScenarioDialog(name);
		}
	}

	public void UnlockScenarioDialog(string name)
	{
		Dialog dialog = this.m_Dialogs[name];
		if (!this.m_ScenarioDialogs.ContainsKey(dialog.m_Group))
		{
			this.m_ScenarioDialogs.Add(dialog.m_Group, new List<Dialog>());
		}
		this.m_ScenarioDialogs[dialog.m_Group].Add(dialog);
		if (HUDManager.Get() == null)
		{
			Debug.Log("DialogsManager UnlockScenarioDialog no HUDManager");
			return;
		}
		if (SaveGame.m_State == SaveGame.State.None)
		{
			((HUDInfoLog)HUDManager.Get().GetHUD(typeof(HUDInfoLog))).AddInfo(GreenHellGame.Instance.GetLocalization().Get("HUD_InfoLog_NewDialog", true), string.Empty, HUDInfoLogTextureType.WT);
			if (!ScenarioManager.Get().IsDreamOrPreDream())
			{
				PlayerAudioModule.Get().PlayNotepadEntrySound();
			}
			HUDWalkieTalkie.Get().UpdateNewDialogsCounter();
		}
	}

	public void LockScenarioDialog(string name)
	{
		Dialog dialog = this.m_Dialogs[name];
		List<Dialog> list = null;
		if (!this.m_ScenarioDialogs.TryGetValue(dialog.m_Group, out list))
		{
			return;
		}
		list.Remove(dialog);
		if (list.Count == 0)
		{
			this.m_ScenarioDialogs.Remove(dialog.m_Group);
		}
		HUDWalkieTalkie.Get().UpdateNewDialogsCounter();
	}

	public void LockAllScenarioDialogs()
	{
		this.m_ScenarioDialogs.Clear();
		HUDWalkieTalkie.Get().UpdateNewDialogsCounter();
	}

	private bool CanStartDialog()
	{
		return !ConsciousnessController.Get().IsActive() && !HarvestingAnimalController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && !TriggerController.Get().IsGrabInProgress();
	}

	public void ScenarioStartDialogIfNonPlaying(string dialog_name)
	{
		if (this.IsAnyDialogPlaying())
		{
			return;
		}
		this.StartDialog(dialog_name);
	}

	public void StartDialog(string dialog_name)
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			return;
		}
		string b = dialog_name.ToLower();
		Dialog dialog = null;
		foreach (string text in this.m_Dialogs.Keys)
		{
			if (text.ToLower() == b)
			{
				dialog = this.m_Dialogs[text];
				dialog_name = text;
				break;
			}
		}
		if (dialog == null)
		{
			Debug.Log("[DialogManager:Play] Can't find dialog - " + dialog_name);
			return;
		}
		if (!this.CanStartDialog())
		{
			this.m_DialogsQueue.Add(dialog);
			return;
		}
		this.StopDialog();
		this.m_CurrentDialog = dialog;
		this.m_CurrentDialog.Start();
	}

	public void StartDialogWithWT(string dialog_name)
	{
		this.StartDialog(dialog_name);
		if (this.IsAnyDialogPlaying() && (!Player.Get().m_ActiveFightController || !Player.Get().m_ActiveFightController.IsAttack()))
		{
			Player.Get().m_ShouldStartWalkieTalkieController = true;
			if (Player.Get().GetCurrentItem(Hand.Left) == null)
			{
				Player.Get().StartController(PlayerControllerType.WalkieTalkie);
			}
		}
		PlayerWalkieTalkieModule.Get().OnCall();
	}

	public void StopDialog()
	{
		if (this.m_CurrentDialog != null)
		{
			this.m_CurrentDialog.Stop(false);
			this.m_CurrentDialog = null;
		}
	}

	private void Update()
	{
		if (Player.Get().IsDead())
		{
			if (this.m_CurrentDialog != null)
			{
				this.m_CurrentDialog.Stop(false);
				this.m_CurrentDialog = null;
			}
			return;
		}
		if (this.m_DialogsQueue.Count > 0 && !this.IsAnyDialogPlaying())
		{
			Dialog dialog = this.m_DialogsQueue[0];
			this.m_DialogsQueue.RemoveAt(0);
			this.StartDialog(dialog.m_Name);
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
			WalkieTalkieController.Get().UpdateWTLight();
		}
	}

	public void OnSelectNode(DialogNode node)
	{
		if (this.m_CurrentDialog != null)
		{
			this.m_CurrentDialog.OnSelectNode(node);
		}
	}

	public bool IsAnyDialogPlaying()
	{
		return this.m_CurrentDialog != null;
	}

	public bool IsDialogPlaying(string name)
	{
		return this.m_CurrentDialog != null && this.m_CurrentDialog.m_Name.ToLower() == name.ToLower();
	}

	public void Save()
	{
		foreach (Dialog dialog in this.m_Dialogs.Values)
		{
			SaveGame.SaveVal("DialogShown" + dialog.m_Name, dialog.m_ShownInSelectDialog);
		}
		int val = 0;
		foreach (List<Dialog> list in this.m_ScenarioDialogs.Values)
		{
			foreach (Dialog dialog2 in list)
			{
				SaveGame.SaveVal("ScenarioDialog" + val++, dialog2.m_Name);
			}
		}
		SaveGame.SaveVal("ScenarioDialogCount", val);
	}

	public void Load()
	{
		this.m_AudioSource.transform.parent = base.transform;
		if (this.m_CurrentDialog != null)
		{
			this.m_CurrentDialog.Stop(false);
			this.m_CurrentDialog = null;
		}
		foreach (Dialog dialog in this.m_Dialogs.Values)
		{
			dialog.m_ShownInSelectDialog = SaveGame.LoadBVal("DialogShown" + dialog.m_Name);
		}
		this.LockAllScenarioDialogs();
		int num = SaveGame.LoadIVal("ScenarioDialogCount");
		for (int i = 0; i < num; i++)
		{
			this.UnlockScenarioDialog(SaveGame.LoadSVal("ScenarioDialog" + i));
		}
	}

	public void OnStartNode(DialogNode node)
	{
		if (WalkieTalkieController.Get())
		{
			WalkieTalkieController.Get().OnStartNode(node);
		}
	}

	public void OnStopDialog(Dialog dialog, bool finish)
	{
		if (WalkieTalkieController.Get())
		{
			WalkieTalkieController.Get().OnStopDialog(dialog);
		}
		if (finish)
		{
			this.LockScenarioDialog(dialog.m_Name);
		}
	}

	public bool CanSelectDialog()
	{
		return !this.IsAnyDialogPlaying() && this.m_ScenarioDialogs.Count != 0 && PlayerWalkieTalkieModule.Get().CanCall();
	}

	public bool IsPlayerSpeaking()
	{
		Dialog currentDialog = this.m_CurrentDialog;
		return currentDialog != null && currentDialog.IsPlayerSpeaking();
	}

	private static DialogsManager s_Instance = null;

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

	[HideInInspector]
	public Dictionary<string, List<Dialog>> m_ScenarioDialogs = new Dictionary<string, List<Dialog>>();

	private List<Dialog> m_DialogsQueue = new List<Dialog>();
}
