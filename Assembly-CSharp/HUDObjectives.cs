using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class HUDObjectives : HUDBase, IObjectivesManagerObserver, ISaveLoad
{
	public static HUDObjectives Get()
	{
		return HUDObjectives.s_Instance;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override void Awake()
	{
		base.Awake();
		HUDObjectives.s_Instance = this;
		this.m_TextGen = new TextGenerator();
	}

	protected override bool ShouldShow()
	{
		return !ScenarioManager.Get().IsDreamOrPreDream() && (this.m_ObjectivesElements.Count > 0 || this.m_Queue.Count > 0 || MapController.Get().IsActive());
	}

	public void OnObjectiveActivated(Objective obj)
	{
		this.AddHUDObjective(obj, ref this.m_ObjectivesElements);
		if (this.m_MapHUDObjective == null)
		{
			List<HUDObjective> list = null;
			this.m_MapHUDObjective = this.AddHUDObjective(obj, ref list);
			this.m_MapObjective = obj;
		}
		else if (obj != this.m_MapObjective)
		{
			this.m_MapObjective = obj;
			base.RemoveElement(this.m_MapHUDObjective.m_HudElem);
			this.m_MapHUDObjective = null;
			List<HUDObjective> list2 = null;
			this.m_MapHUDObjective = this.AddHUDObjective(obj, ref list2);
		}
		this.UpdateElements();
	}

	public void OnObjectiveCompleted(Objective obj)
	{
		HUDObjective hud_obj = this.FindHUDObjective(obj, ref this.m_ObjectivesElements);
		this.RemoveObjectiveElement(hud_obj, ref this.m_ObjectivesElements);
		if (obj == this.m_MapObjective)
		{
			this.m_MapObjective = null;
			if (this.m_MapHUDObjective != null)
			{
				base.RemoveElement(this.m_MapHUDObjective.m_HudElem);
				this.m_MapHUDObjective = null;
			}
		}
		this.UpdateElements();
	}

	public HUDObjective AddHUDObjective(Objective obj, ref List<HUDObjective> list)
	{
		if (this.m_ObjectivesElements.Count > 0 && list == this.m_ObjectivesElements)
		{
			this.m_Queue.Add(obj);
			return null;
		}
		GameObject gameObject = base.AddElement("HUDObjectiveElement");
		if (gameObject != null)
		{
			gameObject.transform.SetParent(base.transform, false);
			HUDObjective hudobjective = new HUDObjective();
			hudobjective.m_Objective = obj;
			hudobjective.m_HudElem = gameObject;
			hudobjective.m_TextComponent = gameObject.GetComponentInChildren<Text>();
			hudobjective.m_TextComponent.text = GreenHellGame.Instance.GetLocalization().Get(hudobjective.m_Objective.m_TextID, true);
			hudobjective.m_TextComponent.transform.position = base.transform.position;
			hudobjective.m_BG = gameObject.transform.FindDeepChild("BG").GetComponent<RawImage>();
			hudobjective.m_BG.transform.position = base.transform.position;
			hudobjective.m_Icon = gameObject.transform.FindDeepChild("Icon").GetComponent<RawImage>();
			this.SetupTargetPosition(hudobjective);
			this.PlaceOutsideOfScreen(hudobjective);
			if (list != null)
			{
				list.Add(hudobjective);
			}
			return hudobjective;
		}
		return null;
	}

	private void PlaceOutsideOfScreen(HUDObjective hud_obj)
	{
		float x = ((RectTransform)HUDManager.Get().m_CanvasGameObject.transform).localScale.x;
		Vector3 position = hud_obj.m_BG.transform.position;
		position.x = (float)Screen.width;
		hud_obj.m_BG.transform.position = position;
		position.x += hud_obj.m_BG.rectTransform.sizeDelta.x * x;
		position.x -= this.m_TextXOffset * (float)Screen.width;
		hud_obj.m_TextComponent.transform.position = position;
	}

	private void SetupTargetPosition(HUDObjective hud_obj)
	{
		float x = ((RectTransform)HUDManager.Get().m_CanvasGameObject.transform).localScale.x;
		Vector3 position = hud_obj.m_TextComponent.transform.position;
		position.x = (float)Screen.width - hud_obj.m_BG.rectTransform.sizeDelta.x * x;
		hud_obj.m_BGTargetPosition = position;
		position.x += hud_obj.m_BG.rectTransform.sizeDelta.x * x;
		position.x -= this.m_TextXOffset * (float)Screen.width;
		hud_obj.m_TextTargetPosition = position;
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_Queue.Count > 0 && this.m_ObjectivesElements.Count == 0)
		{
			this.AddHUDObjective(this.m_Queue[0], ref this.m_ObjectivesElements);
			this.m_Queue.RemoveAt(0);
		}
		this.UpdateElements();
	}

	private void UpdateState()
	{
		if (MapController.Get().IsActive())
		{
			this.m_State = HUDObjectiveState.Map;
			return;
		}
		this.m_State = HUDObjectiveState.Normal;
	}

	private void UpdateElements()
	{
		if (this.m_State == HUDObjectiveState.Normal)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			if (this.m_ObjectivesElements.Count > 0)
			{
				num3 = this.m_ObjectivesElements[0].m_HudElem.transform.localPosition.y;
			}
			for (int i = 0; i < this.m_ObjectivesElements.Count; i++)
			{
				HUDObjective hudobjective = this.m_ObjectivesElements[i];
				this.SetupTargetPosition(hudobjective);
				Vector3 zero = Vector3.zero;
				zero.Set(hudobjective.m_HudElem.transform.localPosition.x, num3 - num, hudobjective.m_HudElem.transform.localPosition.z);
				hudobjective.m_HudElem.transform.localPosition = zero;
				Vector3 position = hudobjective.m_BG.transform.position;
				position.x += (hudobjective.m_BGTargetPosition.x - hudobjective.m_BG.transform.position.x) * Time.deltaTime * 6f;
				hudobjective.m_BG.transform.position = position;
				position = hudobjective.m_TextComponent.transform.position;
				position.x += (hudobjective.m_TextTargetPosition.x - hudobjective.m_TextComponent.transform.position.x) * Time.deltaTime * 6f;
				hudobjective.m_TextComponent.transform.position = position;
				Vector3 localPosition = hudobjective.m_TextComponent.transform.localPosition;
				num += hudobjective.m_BG.rectTransform.sizeDelta.y + num2;
				float num4 = (this.m_ObjectiveDuration == 0f) ? 1f : Mathf.Clamp01((this.m_ObjectiveDuration - (Time.time - hudobjective.m_StartTime)) / 0.5f);
				Color color = hudobjective.m_TextComponent.color;
				color.a = (ChallengesManager.Get().IsChallengeActive() ? 1f : num4);
				hudobjective.m_TextComponent.color = color;
				color = hudobjective.m_BG.color;
				color.a = (ChallengesManager.Get().IsChallengeActive() ? 1f : num4);
				hudobjective.m_BG.color = color;
				Vector3 localScale = ((RectTransform)HUDManager.Get().m_CanvasGameObject.transform).localScale;
				color = Color.white;
				color.a = Mathf.Abs(Mathf.Sin((Time.time - hudobjective.m_StartTime) * 5f));
				if (Time.time - hudobjective.m_StartTime > 5f)
				{
					color.a = 1f;
				}
				hudobjective.m_Icon.color = color;
			}
			int j = 0;
			while (j < this.m_ObjectivesElements.Count)
			{
				HUDObjective hudobjective2 = this.m_ObjectivesElements[j];
				bool flag = (this.m_ObjectiveDuration > 0f) ? (Time.time > hudobjective2.m_StartTime + this.m_ObjectiveDuration) : (hudobjective2.m_Objective.GetState() == ObjectiveState.Completed);
				if (GreenHellGame.ROADSHOW_DEMO)
				{
					flag = (hudobjective2.m_Objective.GetState() == ObjectiveState.Completed);
				}
				else if (ChallengesManager.Get().IsChallengeActive())
				{
					flag = false;
				}
				if (flag)
				{
					base.RemoveElement(hudobjective2.m_HudElem);
					this.m_ObjectivesElements.RemoveAt(j);
				}
				else
				{
					j++;
				}
			}
			for (int k = 0; k < this.m_ObjectivesElements.Count; k++)
			{
				this.m_ObjectivesElements[k].m_HudElem.SetActive(true);
			}
			if (this.m_MapHUDObjective != null)
			{
				this.m_MapHUDObjective.m_HudElem.SetActive(false);
				return;
			}
		}
		else if (this.m_State == HUDObjectiveState.Map)
		{
			if (this.m_MapObjective != null)
			{
				if (this.m_MapHUDObjective != null)
				{
					this.m_MapHUDObjective.m_HudElem.SetActive(true);
					HUDObjective mapHUDObjective = this.m_MapHUDObjective;
					Vector3 position2 = mapHUDObjective.m_BG.transform.position;
					position2.x += (mapHUDObjective.m_BGTargetPosition.x - mapHUDObjective.m_BG.transform.position.x) * Time.deltaTime * 6f;
					mapHUDObjective.m_BG.transform.position = position2;
					position2 = mapHUDObjective.m_TextComponent.transform.position;
					position2.x += (mapHUDObjective.m_TextTargetPosition.x - mapHUDObjective.m_TextComponent.transform.position.x) * Time.deltaTime * 6f;
					mapHUDObjective.m_TextComponent.transform.position = position2;
				}
			}
			else if (this.m_MapHUDObjective != null)
			{
				this.m_MapHUDObjective.m_HudElem.SetActive(false);
			}
			for (int l = 0; l < this.m_ObjectivesElements.Count; l++)
			{
				this.m_ObjectivesElements[l].m_HudElem.SetActive(false);
			}
		}
	}

	public void OnObjectiveRemoved(Objective obj)
	{
		HUDObjective hud_obj = this.FindHUDObjective(obj, ref this.m_ObjectivesElements);
		this.RemoveObjectiveElement(hud_obj, ref this.m_ObjectivesElements);
		if (obj == this.m_MapObjective)
		{
			this.m_MapObjective = null;
			if (this.m_MapHUDObjective != null)
			{
				base.RemoveElement(this.m_MapHUDObjective.m_HudElem);
				this.m_MapHUDObjective = null;
			}
		}
		this.UpdateElements();
	}

	private void RemoveObjectiveElement(HUDObjective hud_obj, ref List<HUDObjective> list)
	{
		if (hud_obj == null)
		{
			return;
		}
		base.RemoveElement(hud_obj.m_HudElem);
		list.Remove(hud_obj);
	}

	private HUDObjective FindHUDObjective(Objective obj, ref List<HUDObjective> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].m_Objective == obj)
			{
				return list[i];
			}
		}
		return null;
	}

	public void SetObjectiveDuration(float duration)
	{
		this.m_ObjectiveDuration = duration;
	}

	public void Save()
	{
		SaveGame.SaveVal("HUDObjectiveDur", this.m_ObjectiveDuration);
	}

	public void Load()
	{
		this.m_ObjectiveDuration = SaveGame.LoadFVal("HUDObjectiveDur");
	}

	private List<Objective> m_Queue = new List<Objective>();

	private List<HUDObjective> m_ObjectivesElements = new List<HUDObjective>();

	private HUDObjective m_MapHUDObjective;

	private Objective m_MapObjective;

	private const float DURATION_TIME = 10f;

	private HUDObjectiveState m_State;

	private TextGenerator m_TextGen;

	private float m_ObjectiveDuration = 5f;

	private float m_TextXOffset = 0.012f;

	private static HUDObjectives s_Instance;
}
