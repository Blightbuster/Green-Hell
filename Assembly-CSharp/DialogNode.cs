using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogNode
{
	public DialogNode(Dialog dialog, string name)
	{
		this.m_Dialog = dialog;
		this.m_Name = name;
		this.m_WalkieTalkie = (this.m_Name.LastIndexOf("WT_") == 0);
	}

	public void Load(Key key)
	{
		if (key.GetVariablesCount() >= 11)
		{
			this.m_Wait = key.GetVariable(10).FValue;
		}
		if (key.GetVariablesCount() >= 12)
		{
			this.m_ObjectName = key.GetVariable(11).SValue;
			this.m_Object = GameObject.Find(this.m_ObjectName);
		}
		if (this.m_Wait > 0f)
		{
			this.m_Name = "Wait";
		}
		else if (Application.isPlaying)
		{
			if (DialogsManager.Get().m_AllClips.ContainsKey(this.m_Name.ToLower()))
			{
				this.m_Clip = DialogsManager.Get().m_AllClips[this.m_Name.ToLower()];
			}
			else if (DialogsManager.Get().m_DebugLogMissingClips)
			{
				Debug.Log("[DialogNode:Load] ERROR - can't find AudioClip - " + this.m_Name);
			}
		}
		if (!Application.isPlaying && key.GetVariablesCount() >= 5)
		{
			this.m_Rect.Set(key.GetVariable(1).FValue, key.GetVariable(2).FValue, key.GetVariable(3).FValue, key.GetVariable(4).FValue);
		}
		if (!Application.isPlaying && key.GetVariablesCount() >= 10)
		{
			this.m_Color.r = key.GetVariable(6).FValue;
			this.m_Color.g = key.GetVariable(7).FValue;
			this.m_Color.b = key.GetVariable(8).FValue;
			this.m_Color.a = key.GetVariable(9).FValue;
		}
		if (key.GetVariablesCount() >= 6 && key.GetVariable(5).BValue)
		{
			this.m_Additional = true;
		}
		for (int i = 0; i < key.GetKeysCount(); i++)
		{
			Key key2 = key.GetKey(i);
			if (key2.GetName() == "Next")
			{
				this.m_NextsIDs.Add(key2.GetVariable(0).IValue);
			}
			else if (key2.GetName() == "ID")
			{
				this.m_ID = key2.GetVariable(0).IValue;
			}
			else if (key2.GetName() == "WTSoundBefore")
			{
				this.m_WTSoundBefore = (key2.GetVariable(0).IValue != 0);
			}
			else if (key2.GetName() == "WTSoundAfter")
			{
				this.m_WTSoundAfter = (key2.GetVariable(0).IValue != 0);
			}
			else if (key2.GetName() == "ShowReply")
			{
				this.m_ShowReply = (key2.GetVariable(0).IValue != 0);
			}
			else if (key2.GetName() == "ReplyTime")
			{
				this.m_ReplyTime = key2.GetVariable(0).FValue;
			}
			else if (key2.GetName() == "FinishIfNoReply")
			{
				this.m_FinishIfNoReply = (key2.GetVariable(0).IValue != 0);
			}
			else if (key2.GetName() == "IconName")
			{
				this.m_IconName = key2.GetVariable(0).SValue;
			}
			else if (key2.GetName() == "WTSoundAfterName")
			{
				this.m_WTSoundAfterName = key2.GetVariable(0).SValue;
			}
			else if (key2.GetName() == "WTSoundBeforeName")
			{
				this.m_WTSoundBeforeName = key2.GetVariable(0).SValue;
			}
		}
	}

	public void Setup(bool editor)
	{
		foreach (int key in this.m_NextsIDs)
		{
			DialogNode dialogNode = this.m_Dialog.m_NodesMap[key];
			if (dialogNode.m_Additional)
			{
				this.m_Additionals.Add(dialogNode);
			}
			if (!dialogNode.m_Additional || editor)
			{
				this.m_Nexts.Insert(0, dialogNode);
			}
			if (!dialogNode.m_Prevs.Contains(this))
			{
				dialogNode.m_Prevs.Add(this);
			}
		}
	}

	public int m_ID = -1;

	private Dialog m_Dialog;

	public string m_ObjectName = string.Empty;

	public GameObject m_Object;

	public string m_Name = string.Empty;

	public string m_LongName = string.Empty;

	public AudioClip m_Clip;

	private List<int> m_NextsIDs = new List<int>();

	public List<DialogNode> m_Prevs = new List<DialogNode>();

	public List<DialogNode> m_Nexts = new List<DialogNode>();

	public List<DialogNode> m_Additionals = new List<DialogNode>();

	public Rect m_Rect = default(Rect);

	public Color m_Color = Color.white;

	public bool m_Additional;

	public int m_AdditionalIndex;

	public bool m_WalkieTalkie;

	public bool m_WTSoundBefore;

	public string m_WTSoundBeforeName = string.Empty;

	public bool m_WTSoundAfter;

	public string m_WTSoundAfterName = string.Empty;

	public string m_IconName = string.Empty;

	public bool m_ShowReply;

	public float m_ReplyTime;

	public bool m_FinishIfNoReply;

	public float m_Wait;
}
