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
		List<MeshCollider> list = new List<MeshCollider>();
		Dictionary<ItemReplacer, ItemReplacer> dictionary = new Dictionary<ItemReplacer, ItemReplacer>();
		for (int i = 0; i < this.m_Chunks.Count; i++)
		{
			if (!(this.m_Chunks[i] == null) && this.m_Chunks[i].activeSelf)
			{
				PlantFruit component = this.m_Chunks[i].GetComponent<PlantFruit>();
				if (component)
				{
					ItemID item_id = (ItemID)Enum.Parse(typeof(ItemID), component.m_InfoName);
					ItemsManager.Get().CreateItem(item_id, true, this.m_Chunks[i].transform);
				}
				else
				{
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_Chunks[i], this.m_Chunks[i].transform.position, this.m_Chunks[i].transform.rotation);
					if (!this.m_DontDestroy)
					{
						gameObject.AddComponent<TinyPhysicsObject>();
					}
					gameObject.AddComponent<Rigidbody>();
					MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
					meshCollider.convex = true;
					this.ReplaceShader(gameObject);
					ObjectMaterial objectMaterial = gameObject.AddComponent<ObjectMaterial>();
					objectMaterial.m_ObjectMaterial = EObjectMaterial.Bush;
					gameObject.layer = DestroyablePlant.m_Layer;
					list.Add(meshCollider);
					if (this.m_CopyScripts)
					{
						this.CopyComponents(this.m_Chunks[i], gameObject);
					}
					ItemReplacer component2 = this.m_Chunks[i].GetComponent<ItemReplacer>();
					ItemReplacer component3 = gameObject.GetComponent<ItemReplacer>();
					if (component2 && component3)
					{
						dictionary[component2] = component3;
					}
					this.DisableCollisionWithPlayer(meshCollider);
				}
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			for (int k = j + 1; k < list.Count; k++)
			{
				Physics.IgnoreCollision(list[j], list[k], true);
			}
		}
		this.SetupItemReplacer(dictionary);
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
			this.m_TempListCollider.Clear();
			Player.Get().GetComponents<Collider>(this.m_TempListCollider);
			if (this.m_TempListCollider.Count > 0)
			{
				this.m_PlayerCollider = this.m_TempListCollider[0];
			}
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
			itemReplacer2.m_DestroyOnReplace = new List<GameObject>();
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
		MonoBehaviour[] components = destination.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour obj in components)
		{
			UnityEngine.Object.Destroy(obj);
		}
		components = source.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour original in components)
		{
			MonoBehaviour monoBehaviour = (MonoBehaviour)this.CopyComponent(original, destination);
			if (this.m_ActivateScripts)
			{
				monoBehaviour.enabled = true;
			}
			if (typeof(Trigger).IsAssignableFrom(monoBehaviour.GetType()))
			{
				((Trigger)monoBehaviour).m_Collider = monoBehaviour.gameObject.GetComponent<Collider>();
			}
		}
	}

	private Component CopyComponent(Component original, GameObject destination)
	{
		Type type = original.GetType();
		Component component = destination.AddComponent(type);
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			fieldInfo.SetValue(component, fieldInfo.GetValue(original));
		}
		return component;
	}

	public List<GameObject> m_Chunks = new List<GameObject>();

	private static Shader s_StandardShader;

	private static int m_Layer = -1;

	public bool m_CopyScripts;

	public bool m_ActivateScripts;

	public bool m_DontDestroy;

	private List<Collider> m_TempListCollider = new List<Collider>(3);

	private Collider m_PlayerCollider;
}
