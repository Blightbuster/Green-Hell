using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

[DisallowMultipleComponent]
public class DestroyablePlantChunk : ReplicatedBehaviour
{
	private void Awake()
	{
		base.enabled = false;
	}

	public void Init(bool dont_destroy, bool activate_scripts, int layer)
	{
		this.m_DontDestroy = dont_destroy;
		this.m_Layer = layer;
		this.m_ActivateScripts = activate_scripts;
	}

	public void OnPlantDestroy()
	{
		base.enabled = true;
		if (!this.m_DontDestroy)
		{
			base.gameObject.AddComponent<TinyPhysicsObject>();
		}
		Rigidbody rigidbody = base.gameObject.AddComponentWithEvent<Rigidbody>();
		if (rigidbody)
		{
			rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		}
		Collider collider = base.gameObject.GetComponent<Collider>();
		if (collider == null)
		{
			if (this.m_UseApproxBoxCollider)
			{
				BoxCollider boxCollider = base.gameObject.AddComponent<BoxCollider>();
				boxCollider.size = base.GetComponent<Renderer>().bounds.size * 0.5f;
				boxCollider.center -= new Vector3(0f, boxCollider.size.y * 0.5f, 0f);
				collider = boxCollider;
			}
			else
			{
				MeshCollider meshCollider = base.gameObject.AddComponent<MeshCollider>();
				meshCollider.convex = true;
				collider = meshCollider;
				PhysicsDampener physicsDampener = base.gameObject.AddComponent<PhysicsDampener>();
				physicsDampener.m_MaxYUpVelocity = 1f;
				physicsDampener.m_MaxVelocityMagnitude = 2f;
			}
		}
		this.ReplaceShader(base.gameObject);
		ObjectMaterial objectMaterial = base.GetComponent<ObjectMaterial>();
		if (objectMaterial == null)
		{
			objectMaterial = base.gameObject.AddComponent<ObjectMaterial>();
		}
		objectMaterial.m_ObjectMaterial = EObjectMaterial.Bush;
		base.gameObject.layer = this.m_Layer;
		if (this.m_ActivateScripts)
		{
			MonoBehaviour[] components = base.GetComponents<MonoBehaviour>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].enabled = true;
			}
		}
		else
		{
			ReplicationComponent component = base.GetComponent<ReplicationComponent>();
			if (component)
			{
				component.enabled = true;
			}
		}
		base.transform.parent = null;
		this.DisableCollisionWithPlayer(collider);
		this.DisableCollisionWithOtherChunks(collider);
		base.gameObject.SetActive(true);
		if (base.ReplIsOwner())
		{
			this.ReplSetDirty();
		}
		this.m_PlantDestroyed = true;
	}

	public override void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state)
	{
		writer.Write(this.m_PlantDestroyed);
		if (this.m_PlantDestroyed)
		{
			writer.Write(this.m_ActivateScripts);
			writer.Write(this.m_DontDestroy);
			writer.Write(this.m_Layer);
			ItemReplacer component = base.GetComponent<ItemReplacer>();
			if (component != null)
			{
				writer.WritePackedUInt32((uint)component.m_DestroyOnReplace.Count);
				for (int i = 0; i < component.m_DestroyOnReplace.Count; i++)
				{
					writer.Write(component.m_DestroyOnReplace[i]);
				}
				return;
			}
			writer.WritePackedUInt32(0u);
		}
	}

	public override void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
	{
		this.m_PlantDestroyed = reader.ReadBoolean();
		if (this.m_PlantDestroyed)
		{
			this.m_ActivateScripts = reader.ReadBoolean();
			this.m_DontDestroy = reader.ReadBoolean();
			this.m_Layer = reader.ReadInt32();
			uint num = reader.ReadPackedUInt32();
			if (num > 0u && this.m_DestroyOnReplace == null)
			{
				this.m_DestroyOnReplace = new List<ReplicatedGameObject>((int)num);
			}
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				this.m_DestroyOnReplace.Add(reader.ReadReplicatedGameObject());
				num2++;
			}
		}
	}

	public override void OnReplicationResolve()
	{
		this.OnPlantDestroy();
		this.UpdateDestroyOnReplaceObjects();
	}

	private void UpdateDestroyOnReplaceObjects()
	{
		if (this.m_DestroyOnReplace == null)
		{
			return;
		}
		int i = 0;
		while (i < this.m_DestroyOnReplace.Count)
		{
			GameObject gameObject = this.m_DestroyOnReplace[i].ResolveGameObject();
			if (gameObject)
			{
				ItemReplacer component = base.GetComponent<ItemReplacer>();
				if (component != null)
				{
					component.m_DestroyOnReplace.Add(gameObject);
				}
				this.m_DestroyOnReplace.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
	}

	private void Update()
	{
		this.UpdateDestroyOnReplaceObjects();
	}

	private void ReplaceShader(GameObject go)
	{
		if (DestroyablePlantChunk.s_StandardShader == null)
		{
			DestroyablePlantChunk.s_StandardShader = Shader.Find("Custom/WindLeavesC2Standard_not_moving");
		}
		Renderer[] components = go.GetComponents<Renderer>();
		for (int i = 0; i < components.Length; i++)
		{
			Material[] materials = components[i].materials;
			for (int j = 0; j < materials.Length; j++)
			{
				materials[j] = UnityEngine.Object.Instantiate<Material>(materials[j]);
				materials[j].shader = DestroyablePlantChunk.s_StandardShader;
			}
			components[i].materials = materials;
		}
	}

	private void DisableCollisionWithPlayer(Collider mc)
	{
		List<Collider> list = new List<Collider>(3);
		Item currentItem = Player.Get().GetCurrentItem(Hand.Left);
		Item currentItem2 = Player.Get().GetCurrentItem(Hand.Right);
		if (currentItem)
		{
			list.Clear();
			currentItem.gameObject.GetComponents<Collider>(list);
			if (list.Count > 0)
			{
				Physics.IgnoreCollision(mc, list[0]);
			}
		}
		if (currentItem2)
		{
			list.Clear();
			currentItem2.gameObject.GetComponents<Collider>(list);
			if (list.Count > 0)
			{
				Physics.IgnoreCollision(mc, list[0]);
			}
		}
		Physics.IgnoreCollision(mc, Player.Get().m_Collider);
		for (int i = 0; i < ReplicatedLogicalPlayer.s_AllLogicalPlayers.Count; i++)
		{
			ReplicatedLogicalPlayer replicatedLogicalPlayer = ReplicatedLogicalPlayer.s_AllLogicalPlayers[i];
			Collider collider = (replicatedLogicalPlayer != null) ? replicatedLogicalPlayer.GetComponent<Collider>() : null;
			if (collider != null)
			{
				Physics.IgnoreCollision(mc, collider);
			}
		}
	}

	private void DisableCollisionWithOtherChunks(Collider mc)
	{
		int num = Physics.OverlapSphereNonAlloc(base.transform.position, 5f, DestroyablePlantChunk.s_OverlapCollidersTmp, 1 << this.m_Layer);
		for (int i = 0; i < num; i++)
		{
			Physics.IgnoreCollision(mc, DestroyablePlantChunk.s_OverlapCollidersTmp[i]);
		}
	}

	private static Shader s_StandardShader = null;

	private const bool INSTANTIATE_MATERIALS = true;

	public bool m_UseApproxBoxCollider;

	private bool m_PlantDestroyed;

	[NonSerialized]
	public bool m_ActivateScripts;

	[NonSerialized]
	public bool m_DontDestroy;

	[NonSerialized]
	public int m_Layer;

	[NonSerialized]
	public List<ReplicatedGameObject> m_DestroyOnReplace;

	private static Collider[] s_OverlapCollidersTmp = new Collider[50];
}
