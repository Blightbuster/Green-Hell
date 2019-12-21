using System;
using System.Collections.Generic;
using AIs;
using CJTools;
using Enums;
using UnityEngine;

public class DartFrogTrigger : Trigger
{
	protected override void Start()
	{
		base.Start();
		this.m_AI = base.gameObject.GetComponent<AI>();
		this.m_Name = this.m_AI.m_ID.ToString();
	}

	public override bool CanTrigger()
	{
		return !this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying();
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		actions.Add(TriggerAction.TYPE.Take);
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Take)
		{
			Item item = ItemsManager.Get().CreateItem("PoisonDartFrog_Alive", false);
			item.transform.position = base.transform.position;
			item.transform.rotation = base.transform.rotation;
			Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(base.gameObject);
			Material material = null;
			for (int i = 0; i < componentsDeepChild.Length; i++)
			{
				material = componentsDeepChild[i].material;
			}
			item.ApplyMaterial(material);
			item.Take();
			UnityEngine.Object.Destroy(base.gameObject);
			Player.Get().GiveDamage(base.gameObject, null, this.m_Damage, Vector3.up, this.m_DamageType, this.m_PoisonLevel, false);
		}
	}

	public override string GetName()
	{
		return this.m_Name;
	}

	private AI m_AI;

	private string m_Name = string.Empty;

	public float m_Damage = 50f;

	public DamageType m_DamageType;

	public int m_PoisonLevel = 1;
}
