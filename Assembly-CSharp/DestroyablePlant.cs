using System;
using System.Collections.Generic;
using System.Reflection;
using Enums;
using UnityEngine;

public class DestroyablePlant : MonoBehaviour
{
	public void OnDestroyPlant()
	{
		if (DestroyablePlant.s_StandardShader == null)
		{
			DestroyablePlant.s_StandardShader = Shader.Find("Custom/WindLeavesC2Standard_not_moving");
		}
		if (DestroyablePlant.m_Layer == -1)
		{
			DestroyablePlant.m_Layer = LayerMask.NameToLayer("SmallPlant");
		}
		new Dictionary<ItemReplacer, ItemReplacer>();
		for (int i = 0; i < this.m_Chunks.Count; i++)
		{
			if (!(this.m_Chunks[i] == null) && this.m_Chunks[i].activeSelf)
			{
				PlantFruit component = this.m_Chunks[i].GetComponent<PlantFruit>();
				if (component)
				{
					ItemID item_id = (ItemID)Enum.Parse(typeof(ItemID), component.m_InfoName);
					ItemsManager.Get().CreateItem(item_id, true, this.m_Chunks[i].transform.position + component.m_BoxCollider.center, this.m_Chunks[i].transform.rotation);
				}
				else
				{
					ItemReplacer component2 = this.m_Chunks[i].GetComponent<ItemReplacer>();
					if (component2)
					{
						component2.m_FromPlant = true;
					}
					DestroyablePlantChunk component3 = this.m_Chunks[i].GetComponent<DestroyablePlantChunk>();
					if (component3 != null)
					{
						component3.Init(this.m_DontDestroy, this.m_ActivateScripts, DestroyablePlant.m_Layer);
						component3.OnPlantDestroy();
					}
				}
			}
		}
	}

	private void DisableCollisionWithPlayer(Collider mc)
	{
		Item currentItem = Player.Get().GetCurrentItem(Hand.Left);
		Item currentItem2 = Player.Get().GetCurrentItem(Hand.Right);
		if (currentItem)
		{
			this.m_TempListCollider.Clear();
			currentItem.gameObject.GetComponents<Collider>(this.m_TempListCollider);
			if (this.m_TempListCollider.Count > 0)
			{
				Physics.IgnoreCollision(mc, this.m_TempListCollider[0]);
			}
		}
		if (currentItem2)
		{
			this.m_TempListCollider.Clear();
			currentItem2.gameObject.GetComponents<Collider>(this.m_TempListCollider);
			if (this.m_TempListCollider.Count > 0)
			{
				Physics.IgnoreCollision(mc, this.m_TempListCollider[0]);
			}
		}
		if (this.m_PlayerCollider == null)
		{
			this.m_PlayerCollider = Player.Get().m_Collider;
		}
		if (this.m_PlayerCollider)
		{
			Physics.IgnoreCollision(mc, this.m_PlayerCollider);
		}
	}

	private void SetupItemReplacer(Dictionary<ItemReplacer, ItemReplacer> ir_map)
	{
		foreach (ItemReplacer itemReplacer in ir_map.Keys)
		{
			ItemReplacer itemReplacer2 = ir_map[itemReplacer];
			if (itemReplacer2.m_DestroyOnReplace == null)
			{
				itemReplacer2.m_DestroyOnReplace = new List<GameObject>();
			}
			for (int i = 0; i < itemReplacer.m_DestroyOnReplace.Count; i++)
			{
				GameObject item = this.FindNewByName(itemReplacer.m_DestroyOnReplace[i].name, ir_map);
				itemReplacer2.m_DestroyOnReplace.Add(item);
			}
		}
	}

	private GameObject FindNewByName(string name, Dictionary<ItemReplacer, ItemReplacer> ir_map)
	{
		foreach (ItemReplacer itemReplacer in ir_map.Values)
		{
			if (itemReplacer.name.StartsWith(name))
			{
				return itemReplacer.gameObject;
			}
		}
		return null;
	}

	private void ReplaceShader(GameObject go)
	{
		Renderer[] components = go.GetComponents<Renderer>();
		for (int i = 0; i < components.Length; i++)
		{
			for (int j = 0; j < components[i].materials.Length; j++)
			{
				components[i].materials[j].shader = DestroyablePlant.s_StandardShader;
			}
		}
	}

	private void CopyComponents(GameObject source, GameObject destination)
	{
		foreach (MonoBehaviour monoBehaviour in destination.GetComponents<MonoBehaviour>())
		{
			if (monoBehaviour.CanCopy())
			{
				UnityEngine.Object.Destroy(monoBehaviour);
			}
		}
		foreach (MonoBehaviour monoBehaviour2 in source.GetComponents<MonoBehaviour>())
		{
			if (monoBehaviour2.CanCopy())
			{
				MonoBehaviour monoBehaviour3 = (MonoBehaviour)this.CopyComponent(monoBehaviour2, destination);
				if (!(monoBehaviour3 == null))
				{
					if (this.m_ActivateScripts)
					{
						monoBehaviour3.enabled = true;
					}
					if (typeof(Trigger).IsAssignableFrom(monoBehaviour3.GetType()))
					{
						((Trigger)monoBehaviour3).m_Collider = monoBehaviour3.gameObject.GetComponent<Collider>();
					}
				}
			}
		}
	}

	private Component CopyComponent(Component original, GameObject destination)
	{
		Type type = original.GetType();
		Component component = destination.AddComponent(type);
		if (component != null)
		{
			foreach (FieldInfo fieldInfo in type.GetFields())
			{
				fieldInfo.SetValue(component, fieldInfo.GetValue(original));
			}
		}
		return component;
	}

	public List<GameObject> m_Chunks = new List<GameObject>();

	private static Shader s_StandardShader = null;

	private static int m_Layer = -1;

	public bool m_CopyScripts;

	public bool m_ActivateScripts;

	public bool m_DontDestroy;

	private List<Collider> m_TempListCollider = new List<Collider>(3);

	private Collider m_PlayerCollider;
}
