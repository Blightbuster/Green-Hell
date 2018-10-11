using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Serialization;

namespace UnityEngine.AI
{
	[AddComponentMenu("Navigation/NavMeshSurface", 30)]
	[HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
	[ExecuteInEditMode]
	[DefaultExecutionOrder(-102)]
	public class NavMeshSurface : MonoBehaviour
	{
		public int agentTypeID
		{
			get
			{
				return this.m_AgentTypeID;
			}
			set
			{
				this.m_AgentTypeID = value;
			}
		}

		public CollectObjects collectObjects
		{
			get
			{
				return this.m_CollectObjects;
			}
			set
			{
				this.m_CollectObjects = value;
			}
		}

		public Vector3 size
		{
			get
			{
				return this.m_Size;
			}
			set
			{
				this.m_Size = value;
			}
		}

		public Vector3 center
		{
			get
			{
				return this.m_Center;
			}
			set
			{
				this.m_Center = value;
			}
		}

		public LayerMask layerMask
		{
			get
			{
				return this.m_LayerMask;
			}
			set
			{
				this.m_LayerMask = value;
			}
		}

		public NavMeshCollectGeometry useGeometry
		{
			get
			{
				return this.m_UseGeometry;
			}
			set
			{
				this.m_UseGeometry = value;
			}
		}

		public int defaultArea
		{
			get
			{
				return this.m_DefaultArea;
			}
			set
			{
				this.m_DefaultArea = value;
			}
		}

		public bool ignoreNavMeshAgent
		{
			get
			{
				return this.m_IgnoreNavMeshAgent;
			}
			set
			{
				this.m_IgnoreNavMeshAgent = value;
			}
		}

		public bool ignoreNavMeshObstacle
		{
			get
			{
				return this.m_IgnoreNavMeshObstacle;
			}
			set
			{
				this.m_IgnoreNavMeshObstacle = value;
			}
		}

		public bool overrideTileSize
		{
			get
			{
				return this.m_OverrideTileSize;
			}
			set
			{
				this.m_OverrideTileSize = value;
			}
		}

		public int tileSize
		{
			get
			{
				return this.m_TileSize;
			}
			set
			{
				this.m_TileSize = value;
			}
		}

		public bool overrideVoxelSize
		{
			get
			{
				return this.m_OverrideVoxelSize;
			}
			set
			{
				this.m_OverrideVoxelSize = value;
			}
		}

		public float voxelSize
		{
			get
			{
				return this.m_VoxelSize;
			}
			set
			{
				this.m_VoxelSize = value;
			}
		}

		public bool buildHeightMesh
		{
			get
			{
				return this.m_BuildHeightMesh;
			}
			set
			{
				this.m_BuildHeightMesh = value;
			}
		}

		public NavMeshData navMeshData
		{
			get
			{
				return this.m_NavMeshData;
			}
			set
			{
				this.m_NavMeshData = value;
			}
		}

		public static List<NavMeshSurface> activeSurfaces
		{
			get
			{
				return NavMeshSurface.s_NavMeshSurfaces;
			}
		}

		private void OnEnable()
		{
			NavMeshSurface.Register(this);
			this.AddData();
		}

		private void OnDisable()
		{
			this.RemoveData();
			NavMeshSurface.Unregister(this);
		}

		public void AddData()
		{
			if (this.m_NavMeshDataInstance.valid)
			{
				return;
			}
			if (this.m_NavMeshData != null)
			{
				this.m_NavMeshDataInstance = NavMesh.AddNavMeshData(this.m_NavMeshData, base.transform.position, base.transform.rotation);
				this.m_NavMeshDataInstance.owner = this;
			}
			this.m_LastPosition = base.transform.position;
			this.m_LastRotation = base.transform.rotation;
		}

		public void RemoveData()
		{
			this.m_NavMeshDataInstance.Remove();
			this.m_NavMeshDataInstance = default(NavMeshDataInstance);
		}

		public NavMeshBuildSettings GetBuildSettings()
		{
			NavMeshBuildSettings settingsByID = NavMesh.GetSettingsByID(this.m_AgentTypeID);
			if (settingsByID.agentTypeID == -1)
			{
				Debug.LogWarning("No build settings for agent type ID " + this.agentTypeID, this);
				settingsByID.agentTypeID = this.m_AgentTypeID;
			}
			if (this.overrideTileSize)
			{
				settingsByID.overrideTileSize = true;
				settingsByID.tileSize = this.tileSize;
			}
			if (this.overrideVoxelSize)
			{
				settingsByID.overrideVoxelSize = true;
				settingsByID.voxelSize = this.voxelSize;
			}
			return settingsByID;
		}

		public void BuildNavMesh()
		{
			List<NavMeshBuildSource> sources = this.CollectSources();
			Bounds localBounds = new Bounds(this.m_Center, NavMeshSurface.Abs(this.m_Size));
			if (this.m_CollectObjects == CollectObjects.All || this.m_CollectObjects == CollectObjects.Children)
			{
				localBounds = this.CalculateWorldBounds(sources);
			}
			NavMeshData navMeshData = NavMeshBuilder.BuildNavMeshData(this.GetBuildSettings(), sources, localBounds, base.transform.position, base.transform.rotation);
			if (navMeshData != null)
			{
				navMeshData.name = base.gameObject.name;
				this.RemoveData();
				this.m_NavMeshData = navMeshData;
				if (base.isActiveAndEnabled)
				{
					this.AddData();
				}
			}
		}

		public AsyncOperation UpdateNavMesh(NavMeshData data)
		{
			List<NavMeshBuildSource> sources = this.CollectSources();
			Bounds localBounds = new Bounds(this.m_Center, NavMeshSurface.Abs(this.m_Size));
			if (this.m_CollectObjects == CollectObjects.All || this.m_CollectObjects == CollectObjects.Children)
			{
				localBounds = this.CalculateWorldBounds(sources);
			}
			return NavMeshBuilder.UpdateNavMeshDataAsync(data, this.GetBuildSettings(), sources, localBounds);
		}

		private static void Register(NavMeshSurface surface)
		{
			if (NavMeshSurface.s_NavMeshSurfaces.Count == 0)
			{
				Delegate onPreUpdate = NavMesh.onPreUpdate;
				if (NavMeshSurface.<>f__mg$cache0 == null)
				{
					NavMeshSurface.<>f__mg$cache0 = new NavMesh.OnNavMeshPreUpdate(NavMeshSurface.UpdateActive);
				}
				NavMesh.onPreUpdate = (NavMesh.OnNavMeshPreUpdate)Delegate.Combine(onPreUpdate, NavMeshSurface.<>f__mg$cache0);
			}
			if (!NavMeshSurface.s_NavMeshSurfaces.Contains(surface))
			{
				NavMeshSurface.s_NavMeshSurfaces.Add(surface);
			}
		}

		private static void Unregister(NavMeshSurface surface)
		{
			NavMeshSurface.s_NavMeshSurfaces.Remove(surface);
			if (NavMeshSurface.s_NavMeshSurfaces.Count == 0)
			{
				Delegate onPreUpdate = NavMesh.onPreUpdate;
				if (NavMeshSurface.<>f__mg$cache1 == null)
				{
					NavMeshSurface.<>f__mg$cache1 = new NavMesh.OnNavMeshPreUpdate(NavMeshSurface.UpdateActive);
				}
				NavMesh.onPreUpdate = (NavMesh.OnNavMeshPreUpdate)Delegate.Remove(onPreUpdate, NavMeshSurface.<>f__mg$cache1);
			}
		}

		private static void UpdateActive()
		{
			for (int i = 0; i < NavMeshSurface.s_NavMeshSurfaces.Count; i++)
			{
				NavMeshSurface.s_NavMeshSurfaces[i].UpdateDataIfTransformChanged();
			}
		}

		private void AppendModifierVolumes(ref List<NavMeshBuildSource> sources)
		{
			List<NavMeshModifierVolume> list;
			if (this.m_CollectObjects == CollectObjects.Children)
			{
				list = new List<NavMeshModifierVolume>(base.GetComponentsInChildren<NavMeshModifierVolume>());
				list.RemoveAll((NavMeshModifierVolume x) => !x.isActiveAndEnabled);
			}
			else
			{
				list = NavMeshModifierVolume.activeModifiers;
			}
			foreach (NavMeshModifierVolume navMeshModifierVolume in list)
			{
				if ((this.m_LayerMask & 1 << navMeshModifierVolume.gameObject.layer) != 0)
				{
					if (navMeshModifierVolume.AffectsAgentType(this.m_AgentTypeID))
					{
						Vector3 pos = navMeshModifierVolume.transform.TransformPoint(navMeshModifierVolume.center);
						Vector3 lossyScale = navMeshModifierVolume.transform.lossyScale;
						Vector3 size = new Vector3(navMeshModifierVolume.size.x * Mathf.Abs(lossyScale.x), navMeshModifierVolume.size.y * Mathf.Abs(lossyScale.y), navMeshModifierVolume.size.z * Mathf.Abs(lossyScale.z));
						NavMeshBuildSource item = default(NavMeshBuildSource);
						item.shape = NavMeshBuildSourceShape.ModifierBox;
						item.transform = Matrix4x4.TRS(pos, navMeshModifierVolume.transform.rotation, Vector3.one);
						item.size = size;
						item.area = navMeshModifierVolume.area;
						sources.Add(item);
					}
				}
			}
		}

		private List<NavMeshBuildSource> CollectSources()
		{
			List<NavMeshBuildSource> list = new List<NavMeshBuildSource>();
			List<NavMeshBuildMarkup> list2 = new List<NavMeshBuildMarkup>();
			List<NavMeshModifier> list3;
			if (this.m_CollectObjects == CollectObjects.Children)
			{
				list3 = new List<NavMeshModifier>(base.GetComponentsInChildren<NavMeshModifier>());
				list3.RemoveAll((NavMeshModifier x) => !x.isActiveAndEnabled);
			}
			else
			{
				list3 = NavMeshModifier.activeModifiers;
			}
			foreach (NavMeshModifier navMeshModifier in list3)
			{
				if ((this.m_LayerMask & 1 << navMeshModifier.gameObject.layer) != 0)
				{
					if (navMeshModifier.AffectsAgentType(this.m_AgentTypeID))
					{
						list2.Add(new NavMeshBuildMarkup
						{
							root = navMeshModifier.transform,
							overrideArea = navMeshModifier.overrideArea,
							area = navMeshModifier.area,
							ignoreFromBuild = navMeshModifier.ignoreFromBuild
						});
					}
				}
			}
			if (this.m_CollectObjects == CollectObjects.All)
			{
				NavMeshBuilder.CollectSources(null, this.m_LayerMask, this.m_UseGeometry, this.m_DefaultArea, list2, list);
			}
			else if (this.m_CollectObjects == CollectObjects.Children)
			{
				NavMeshBuilder.CollectSources(base.transform, this.m_LayerMask, this.m_UseGeometry, this.m_DefaultArea, list2, list);
			}
			else if (this.m_CollectObjects == CollectObjects.Volume)
			{
				Matrix4x4 mat = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
				Bounds worldBounds = NavMeshSurface.GetWorldBounds(mat, new Bounds(this.m_Center, this.m_Size));
				NavMeshBuilder.CollectSources(worldBounds, this.m_LayerMask, this.m_UseGeometry, this.m_DefaultArea, list2, list);
			}
			if (this.m_IgnoreNavMeshAgent)
			{
				list.RemoveAll((NavMeshBuildSource x) => x.component != null && x.component.gameObject.GetComponent<NavMeshAgent>() != null);
			}
			if (this.m_IgnoreNavMeshObstacle)
			{
				list.RemoveAll((NavMeshBuildSource x) => x.component != null && x.component.gameObject.GetComponent<NavMeshObstacle>() != null);
			}
			this.AppendModifierVolumes(ref list);
			return list;
		}

		private static Vector3 Abs(Vector3 v)
		{
			return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
		}

		private static Bounds GetWorldBounds(Matrix4x4 mat, Bounds bounds)
		{
			Vector3 a = NavMeshSurface.Abs(mat.MultiplyVector(Vector3.right));
			Vector3 a2 = NavMeshSurface.Abs(mat.MultiplyVector(Vector3.up));
			Vector3 a3 = NavMeshSurface.Abs(mat.MultiplyVector(Vector3.forward));
			Vector3 center = mat.MultiplyPoint(bounds.center);
			Vector3 size = a * bounds.size.x + a2 * bounds.size.y + a3 * bounds.size.z;
			return new Bounds(center, size);
		}

		private Bounds CalculateWorldBounds(List<NavMeshBuildSource> sources)
		{
			Matrix4x4 lhs = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
			lhs = lhs.inverse;
			Bounds result = default(Bounds);
			foreach (NavMeshBuildSource navMeshBuildSource in sources)
			{
				switch (navMeshBuildSource.shape)
				{
				case NavMeshBuildSourceShape.Mesh:
				{
					Mesh mesh = navMeshBuildSource.sourceObject as Mesh;
					result.Encapsulate(NavMeshSurface.GetWorldBounds(lhs * navMeshBuildSource.transform, mesh.bounds));
					break;
				}
				case NavMeshBuildSourceShape.Terrain:
				{
					TerrainData terrainData = navMeshBuildSource.sourceObject as TerrainData;
					result.Encapsulate(NavMeshSurface.GetWorldBounds(lhs * navMeshBuildSource.transform, new Bounds(0.5f * terrainData.size, terrainData.size)));
					break;
				}
				case NavMeshBuildSourceShape.Box:
				case NavMeshBuildSourceShape.Sphere:
				case NavMeshBuildSourceShape.Capsule:
				case NavMeshBuildSourceShape.ModifierBox:
					result.Encapsulate(NavMeshSurface.GetWorldBounds(lhs * navMeshBuildSource.transform, new Bounds(Vector3.zero, navMeshBuildSource.size)));
					break;
				}
			}
			result.Expand(0.1f);
			return result;
		}

		private bool HasTransformChanged()
		{
			return this.m_LastPosition != base.transform.position || this.m_LastRotation != base.transform.rotation;
		}

		private void UpdateDataIfTransformChanged()
		{
			if (this.HasTransformChanged())
			{
				this.RemoveData();
				this.AddData();
			}
		}

		[SerializeField]
		private int m_AgentTypeID;

		[SerializeField]
		private CollectObjects m_CollectObjects;

		[SerializeField]
		private Vector3 m_Size = new Vector3(10f, 10f, 10f);

		[SerializeField]
		private Vector3 m_Center = new Vector3(0f, 2f, 0f);

		[SerializeField]
		private LayerMask m_LayerMask = -1;

		[SerializeField]
		private NavMeshCollectGeometry m_UseGeometry;

		[SerializeField]
		private int m_DefaultArea;

		[SerializeField]
		private bool m_IgnoreNavMeshAgent = true;

		[SerializeField]
		private bool m_IgnoreNavMeshObstacle = true;

		[SerializeField]
		private bool m_OverrideTileSize;

		[SerializeField]
		private int m_TileSize = 256;

		[SerializeField]
		private bool m_OverrideVoxelSize;

		[SerializeField]
		private float m_VoxelSize;

		[SerializeField]
		private bool m_BuildHeightMesh;

		[SerializeField]
		[FormerlySerializedAs("m_BakedNavMeshData")]
		private NavMeshData m_NavMeshData;

		private NavMeshDataInstance m_NavMeshDataInstance;

		private Vector3 m_LastPosition = Vector3.zero;

		private Quaternion m_LastRotation = Quaternion.identity;

		private static readonly List<NavMeshSurface> s_NavMeshSurfaces = new List<NavMeshSurface>();

		[CompilerGenerated]
		private static NavMesh.OnNavMeshPreUpdate <>f__mg$cache0;

		[CompilerGenerated]
		private static NavMesh.OnNavMeshPreUpdate <>f__mg$cache1;
	}
}
