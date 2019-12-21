using System;
using System.Collections.Generic;
using UnityEngine;

public class LootBox : MonoBehaviour, ITriggerOwner
{
	private void Awake()
	{
		LootBox.s_AllLootBoxes.Add(this);
	}

	private void OnDestroy()
	{
		LootBox.s_AllLootBoxes.Remove(this);
	}

	private void Start()
	{
		this.m_Trigger.SetOwner(this);
	}

	public bool CanTrigger(Trigger trigger)
	{
		return this.m_ClosedObject.activeSelf && (!this.m_Trigger.m_OneTime || !this.m_Trigger.m_WasTriggered);
	}

	public void OnExecute(Trigger trigger, TriggerAction.TYPE action)
	{
		this.m_Trigger.m_BoxCollider.enabled = false;
		this.m_ClosedObject.SetActive(false);
		this.m_OpenObject.SetActive(true);
		this.m_Trigger.TryPlayExecuteSound();
		this.m_State = LootBox.State.Open;
	}

	public void GetActions(Trigger trigger, List<TriggerAction.TYPE> actions)
	{
		actions.Add(this.m_Trigger.m_DefaultAction);
	}

	public string GetTriggerInfoLocalized(Trigger trigger)
	{
		return GreenHellGame.Instance.GetLocalization().Get(this.m_DisplayNameID, true);
	}

	public string GetIconName(Trigger trigger)
	{
		if (this.m_Trigger.m_DefaultIconName.Length > 0)
		{
			return this.m_Trigger.m_DefaultIconName;
		}
		return string.Empty;
	}

	public void Save()
	{
		string str = (base.transform.position.x + base.transform.position.y + base.transform.position.z).ToString("F2");
		SaveGame.SaveVal("LootBox" + str, (int)this.m_State);
	}

	public void Load()
	{
		string str = (base.transform.position.x + base.transform.position.y + base.transform.position.z).ToString("F2");
		this.m_State = (LootBox.State)SaveGame.LoadIVal("LootBox" + str);
		RandomLootSpawner componentInChildren = base.gameObject.GetComponentInChildren<RandomLootSpawner>();
		LootBox.State state = this.m_State;
		if (state != LootBox.State.Closed)
		{
			if (state != LootBox.State.Open)
			{
				return;
			}
			this.m_Trigger.m_BoxCollider.enabled = false;
			this.m_ClosedObject.SetActive(false);
			this.m_OpenObject.SetActive(true);
			if (componentInChildren != null)
			{
				componentInChildren.gameObject.SetActive(false);
			}
		}
		else
		{
			this.m_Trigger.m_BoxCollider.enabled = true;
			this.m_Trigger.m_WasTriggered = false;
			this.m_Trigger.m_FirstTriggerTime = 0f;
			this.m_ClosedObject.SetActive(true);
			this.m_OpenObject.SetActive(false);
			if (componentInChildren != null)
			{
				componentInChildren.gameObject.SetActive(true);
				return;
			}
		}
	}

	private LootBox.State m_State;

	public Trigger m_Trigger;

	public GameObject m_OpenObject;

	public GameObject m_ClosedObject;

	public string m_DisplayNameID = string.Empty;

	public static List<LootBox> s_AllLootBoxes = new List<LootBox>();

	private enum State
	{
		Closed,
		Open
	}
}
