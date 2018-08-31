using System;
using System.Collections.Generic;
using CJTools;
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
		this.m_CutDummies.Clear();
		for (int i = 0; i < 10; i++)
		{
			string name = "Cut0" + i.ToString();
			Transform transform = base.gameObject.transform.FindDeepChild(name);
			if (transform != null)
			{
				this.m_CutDummies.Add(transform);
			}
		}
		if (this.m_Item.m_Info.m_CutData != null && this.m_Item.m_Info.m_CutData.Count > 0)
		{
			for (int j = 0; j < this.m_Item.m_Info.m_CutData.Count; j++)
			{
				CutData cutData = new CutData();
				cutData.m_BlendFactor = this.m_Item.m_Info.m_CutData[j].m_BlendFactor;
				cutData.m_BlendShapeIndex = this.m_Item.m_Info.m_CutData[j].m_BlendShapeIndex;
				cutData.m_DestroyedPrefabName = this.m_Item.m_Info.m_CutData[j].m_DestroyedPrefabName;
				cutData.m_DummyName = this.m_Item.m_Info.m_CutData[j].m_DummyName;
				cutData.m_Health = this.m_Item.m_Info.m_CutData[j].m_Health;
				cutData.m_MaxHealth = this.m_Item.m_Info.m_CutData[j].m_MaxHealth;
				this.m_CutData.Add(cutData);
			}
		}
		SkinnedMeshRenderer component = base.GetComponent<SkinnedMeshRenderer>();
		if (component)
		{
			for (int k = 0; k < this.m_CutData.Count; k++)
			{
				Transform transform2 = null;
				for (int l = 0; l < this.m_CutDummies.Count; l++)
				{
					if (this.m_CutDummies[l].name == this.m_CutData[k].m_DummyName)
					{
						transform2 = this.m_CutDummies[l];
						break;
					}
				}
				if (transform2 == null)
				{
					DebugUtils.Assert(DebugUtils.AssertType.Info);
				}
				else
				{
					this.m_CutData[k].m_VerticesToMorph = new List<Vector3>();
					this.m_CutData[k].m_VerticesMorphTarget = new List<Vector3>();
					this.m_CutData[k].m_VerticesToMorphIndex = new List<int>();
					Vector3[] array = new Vector3[component.sharedMesh.vertexCount];
					Vector3[] deltaNormals = new Vector3[component.sharedMesh.vertexCount];
					Vector3[] deltaTangents = new Vector3[component.sharedMesh.vertexCount];
					component.sharedMesh.GetBlendShapeFrameVertices(this.m_CutData[k].m_BlendShapeIndex, 0, array, deltaNormals, deltaTangents);
					for (int m = 0; m < component.sharedMesh.vertices.Length; m++)
					{
						Vector3 item = component.sharedMesh.vertices[m];
						if (Mathf.Abs(item.y - transform2.localPosition.y) < 0.3f)
						{
							this.m_CutData[k].m_VerticesToMorph.Add(item);
							this.m_CutData[k].m_VerticesMorphTarget.Add(array[m]);
							this.m_CutData[k].m_VerticesToMorphIndex.Add(m);
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
			this.DestroyMe(damage_info, string.Empty);
		}
		else if (base.GetComponent<PushLeaves>())
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
			float f = Vector3.Dot(lhs, rhs);
			if (Mathf.Abs(f) < 0.5f)
			{
				return transform;
			}
		}
		return result;
	}

	protected void DestroyMe(DamageInfo damage_info, string destroyed_prefab_name = "")
	{
		if (base.gameObject.GetComponent<DestroyablePlant>() != null)
		{
			base.gameObject.GetComponent<DestroyablePlant>().OnDestroyPlant();
		}
		string text = (destroyed_prefab_name.Length <= 0) ? this.m_Item.m_Info.m_DestroyedPrefabName : destroyed_prefab_name;
		if (text.Length > 0)
		{
			GameObject prefab = GreenHellGame.Instance.GetPrefab(text);
			if (!prefab)
			{
				DebugUtils.Assert("[:DestroyMe] Can't find destroyed prefab - " + text, true, DebugUtils.AssertType.Info);
				return;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, base.transform.position, base.transform.rotation);
		}
		for (int i = 0; i < this.m_Item.m_Info.m_ItemsToBackpackOnDestroy.Count; i++)
		{
			Item item = ItemsManager.Get().CreateItem(this.m_Item.m_Info.m_ItemsToBackpackOnDestroy[i], false, Vector3.zero, Quaternion.identity);
			if (item && !item.Take())
			{
				Inventory3DManager.Get().DropItem(item);
			}
		}
		StaticObjectsManager.Get().ObjectDestroyed(base.gameObject);
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
}
