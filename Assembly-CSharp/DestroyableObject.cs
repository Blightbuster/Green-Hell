using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class DestroyableObject : MonoBehaviour
{
	private void Start()
	{
		this.InitCutSystem();
		DebugUtils.Assert(this.m_Item != null, "DestroyableObject - " + base.name + ". Error, item is not set!", true, DebugUtils.AssertType.Info);
	}

	private void InitCutSystem()
	{
		if (DestroyableObject.s_DummyNames == null)
		{
			DestroyableObject.s_DummyNames = new string[10];
			for (int i = 0; i < 10; i++)
			{
				DestroyableObject.s_DummyNames[i] = "Cut0" + i.ToString();
			}
		}
		this.m_CutDummies.Clear();
		for (int j = 0; j < 10; j++)
		{
			Transform transform = base.gameObject.transform.FindDeepChild(DestroyableObject.s_DummyNames[j]);
			if (transform != null)
			{
				this.m_CutDummies.Add(transform);
			}
		}
		if (this.m_Item.m_Info.m_CutData != null && this.m_Item.m_Info.m_CutData.Count > 0)
		{
			for (int k = 0; k < this.m_Item.m_Info.m_CutData.Count; k++)
			{
				CutData cutData = new CutData();
				cutData.m_BlendFactor = this.m_Item.m_Info.m_CutData[k].m_BlendFactor;
				cutData.m_BlendShapeIndex = this.m_Item.m_Info.m_CutData[k].m_BlendShapeIndex;
				cutData.m_DestroyedPrefabName = this.m_Item.m_Info.m_CutData[k].m_DestroyedPrefabName;
				cutData.m_DummyName = this.m_Item.m_Info.m_CutData[k].m_DummyName;
				cutData.m_Health = this.m_Item.m_Info.m_CutData[k].m_Health;
				cutData.m_MaxHealth = this.m_Item.m_Info.m_CutData[k].m_MaxHealth;
				this.m_CutData.Add(cutData);
			}
		}
		SkinnedMeshRenderer component = base.GetComponent<SkinnedMeshRenderer>();
		if (component)
		{
			for (int l = 0; l < this.m_CutData.Count; l++)
			{
				Transform transform2 = null;
				for (int m = 0; m < this.m_CutDummies.Count; m++)
				{
					if (this.m_CutDummies[m].name == this.m_CutData[l].m_DummyName)
					{
						transform2 = this.m_CutDummies[m];
						break;
					}
				}
				if (transform2 == null)
				{
					DebugUtils.Assert(DebugUtils.AssertType.Info);
				}
				else
				{
					this.m_CutData[l].m_VerticesToMorph = new List<Vector3>();
					this.m_CutData[l].m_VerticesMorphTarget = new List<Vector3>();
					this.m_CutData[l].m_VerticesToMorphIndex = new List<int>();
					Vector3[] array = new Vector3[component.sharedMesh.vertexCount];
					Vector3[] deltaNormals = new Vector3[component.sharedMesh.vertexCount];
					Vector3[] deltaTangents = new Vector3[component.sharedMesh.vertexCount];
					component.sharedMesh.GetBlendShapeFrameVertices(this.m_CutData[l].m_BlendShapeIndex, 0, array, deltaNormals, deltaTangents);
					for (int n = 0; n < component.sharedMesh.vertices.Length; n++)
					{
						Vector3 vector = component.sharedMesh.vertices[n];
						if (Mathf.Abs(vector.y - transform2.localPosition.y) < 0.3f)
						{
							this.m_CutData[l].m_VerticesToMorph.Add(vector);
							this.m_CutData[l].m_VerticesMorphTarget.Add(array[n]);
							this.m_CutData[l].m_VerticesToMorphIndex.Add(n);
						}
					}
				}
			}
		}
	}

	public virtual void TakeDamage(DamageInfo damage_info)
	{
		if (this.m_CutData != null && this.m_CutData.Count > 0)
		{
			Transform transform = this.FindClosestCutDummy(damage_info.m_Position);
			if (transform != null)
			{
				for (int i = 0; i < this.m_CutData.Count; i++)
				{
					CutData cutData = this.m_CutData[i];
					if (cutData.m_DummyName == transform.name)
					{
						cutData.m_Health -= damage_info.m_Damage;
						if (cutData.m_Health < 0f)
						{
							cutData.m_Health = 0f;
						}
						if (cutData.m_Health == 0f)
						{
							this.DestroyMe(damage_info, cutData.m_DestroyedPrefabName);
						}
						SkinnedMeshRenderer component = base.GetComponent<SkinnedMeshRenderer>();
						Mesh mesh = new Mesh();
						component.BakeMesh(mesh);
						if (component)
						{
							Vector3[] vertices = mesh.vertices;
							for (int j = 0; j < cutData.m_VerticesToMorph.Count; j++)
							{
								int num = cutData.m_VerticesToMorphIndex[j];
								if (Vector3.Dot(damage_info.m_HitDir, component.transform.TransformVector(mesh.normals[num])) < -0.3f)
								{
									vertices[num] = cutData.m_VerticesToMorph[j] + cutData.m_VerticesMorphTarget[j] * (1f - cutData.m_Health / cutData.m_MaxHealth);
								}
							}
							mesh.vertices = vertices;
							component.sharedMesh = mesh;
							component.sharedMesh.RecalculateNormals();
						}
					}
				}
			}
			return;
		}
		this.m_Item.m_Info.m_Health -= damage_info.m_Damage;
		if (this.m_Item.m_Info.m_Health < 0f)
		{
			this.m_Item.m_Info.m_Health = 0f;
		}
		if (this.m_Item.m_Info.m_Health == 0f)
		{
			this.DestroyMe(damage_info, "");
			return;
		}
		if (base.GetComponent<PushLeaves>())
		{
			LeavesPusher.Get().PushHit(base.gameObject, damage_info.m_HitDir);
		}
	}

	private Transform FindClosestCutDummy(Vector3 position)
	{
		Transform result = null;
		for (int i = 0; i < this.m_CutDummies.Count; i++)
		{
			Transform transform = this.m_CutDummies[i];
			Vector3 lhs = Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one).GetRow(1);
			lhs.Normalize();
			Vector3 rhs = position - transform.position;
			if (Mathf.Abs(Vector3.Dot(lhs, rhs)) < 0.5f)
			{
				return transform;
			}
		}
		return result;
	}

	public void DestroyMe(DamageInfo damage_info = null, string destroyed_prefab_name = "")
	{
		if (!this.ReplIsOwner())
		{
			return;
		}
		if (base.gameObject.GetComponent<DestroyablePlant>() != null)
		{
			base.gameObject.GetComponent<DestroyablePlant>().OnDestroyPlant();
		}
		string text = (destroyed_prefab_name.Length > 0) ? destroyed_prefab_name : this.m_Item.m_Info.m_DestroyedPrefabName;
		if (text.Length > 0)
		{
			GameObject prefab = GreenHellGame.Instance.GetPrefab(text);
			if (!prefab)
			{
				DebugUtils.Assert("[:DestroyMe] Can't find destroyed prefab - " + text, true, DebugUtils.AssertType.Info);
				return;
			}
			Vector3 position = base.transform.position;
			if (this.m_Item && this.m_Item.m_Info != null && this.m_Item.m_Info.m_ID == ItemID.small_plant_01_acres_grown && text == "Malanga_Bulb")
			{
				position.y += 0.2f;
			}
			else if (this.m_Item && this.m_Item.m_Info != null && this.m_Item.m_Info.m_ID == ItemID.Coconut)
			{
				position.y += 0.1f;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, position, base.transform.rotation);
			Item componentDeepChild = General.GetComponentDeepChild<Item>(gameObject);
			if (componentDeepChild != null)
			{
				Vector3 ang_vel = Vector3.zero;
				if (damage_info != null)
				{
					if (damage_info.m_Damager == null)
					{
						ang_vel = -Vector3.Cross(damage_info.m_HitDir, Vector3.up);
					}
					else if (Vector3.Dot(damage_info.m_Damager.transform.right, damage_info.m_HitDir) > 0f)
					{
						ang_vel = -damage_info.m_Damager.transform.forward;
					}
					else
					{
						ang_vel = damage_info.m_Damager.transform.forward;
					}
				}
				componentDeepChild.AddAngularVelocityOnStart(ang_vel, 10f);
			}
			if (this.m_Item != null && this.m_Item.m_Acre != null && gameObject.GetComponent<ObjectWithTrunk>() != null)
			{
				this.m_Item.m_Acre.m_ObjectWithTrunk = gameObject.GetComponent<ObjectWithTrunk>();
			}
		}
		for (int i = 0; i < this.m_Item.m_Info.m_ItemsToBackpackOnDestroy.Count; i++)
		{
			Item item = ItemsManager.Get().CreateItem(this.m_Item.m_Info.m_ItemsToBackpackOnDestroy[i], false, Vector3.zero, Quaternion.identity);
			if (item)
			{
				if (!item.Take())
				{
					Inventory3DManager.Get().DropItem(item);
				}
				else if (item.m_Info != null)
				{
					PlayerAudioModule.Get().PlayItemSound(item.m_Info.m_GrabSound);
				}
			}
		}
		StaticObjectsManager.Get().ObjectDestroyed(base.gameObject);
		ReplicatedStaticObjects local = ReplicatedStaticObjects.GetLocal();
		if (local != null)
		{
			local.AddDestroyedObject(base.gameObject);
		}
		this.PlayDestroySound();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void PlayDestroySound()
	{
		AudioClip destroyableDestroySound = ItemsManager.Get().GetDestroyableDestroySound(this.m_Item.m_Info.m_DestroyableDestroySoundHash);
		if (destroyableDestroySound != null)
		{
			Player.Get().GetComponent<PlayerAudioModule>().PlaySound(destroyableDestroySound, 1f, false, Noise.Type.None);
		}
	}

	private List<Transform> m_CutDummies = new List<Transform>();

	private List<CutData> m_CutData = new List<CutData>();

	public Item m_Item;

	private const int CUT_NUMBER = 10;

	private static string[] s_DummyNames;
}
