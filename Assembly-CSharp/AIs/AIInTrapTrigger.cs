using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

namespace AIs
{
	public class AIInTrapTrigger : Trigger
	{
		public override void GetActions(List<TriggerAction.TYPE> actions)
		{
			if (HeavyObjectController.Get().IsActive())
			{
				return;
			}
			actions.Add(TriggerAction.TYPE.Take);
		}

		public override void OnExecute(TriggerAction.TYPE action)
		{
			base.OnExecute(action);
			AI.AIID id = this.m_AI.m_ID;
			Item item = ItemsManager.Get().CreateItem(id.ToString() + "_Body", false);
			if (id == AI.AIID.PoisonDartFrog)
			{
				List<Renderer> componentsDeepChild = General.GetComponentsDeepChild<Renderer>(base.gameObject);
				Material material = null;
				for (int i = 0; i < componentsDeepChild.Count; i++)
				{
					material = componentsDeepChild[i].material;
				}
				item.ApplyMaterial(material);
			}
			if (!item.Take())
			{
				Inventory3DManager.Get().DropItem(item);
			}
			UnityEngine.Object.Destroy(this.m_AI.gameObject);
			this.m_AI = null;
		}

		public override string GetName()
		{
			return (!this.m_AI) ? string.Empty : this.m_AI.GetName();
		}

		[HideInInspector]
		public AI m_AI;
	}
}
