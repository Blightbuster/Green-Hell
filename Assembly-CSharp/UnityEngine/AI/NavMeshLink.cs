using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityEngine.AI
{
	[HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
	[AddComponentMenu("Navigation/NavMeshLink", 33)]
	[ExecuteInEditMode]
	[DefaultExecutionOrder(-101)]
	public class NavMeshLink : MonoBehaviour
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
				this.UpdateLink();
			}
		}

		public Vector3 startPoint
		{
			get
			{
				return this.m_StartPoint;
			}
			set
			{
				this.m_StartPoint = value;
				this.UpdateLink();
			}
		}

		public Vector3 endPoint
		{
			get
			{
				return this.m_EndPoint;
			}
			set
			{
				this.m_EndPoint = value;
				this.UpdateLink();
			}
		}

		public float width
		{
			get
			{
				return this.m_Width;
			}
			set
			{
				this.m_Width = value;
				this.UpdateLink();
			}
		}

		public int costModifier
		{
			get
			{
				return this.m_CostModifier;
			}
			set
			{
				this.m_CostModifier = value;
				this.UpdateLink();
			}
		}

		public bool bidirectional
		{
			get
			{
				return this.m_Bidirectional;
			}
			set
			{
				this.m_Bidirectional = value;
				this.UpdateLink();
			}
		}

		public bool autoUpdate
		{
			get
			{
				return this.m_AutoUpdatePosition;
			}
			set
			{
				this.SetAutoUpdate(value);
			}
		}

		public int area
		{
			get
			{
				return this.m_Area;
			}
			set
			{
				this.m_Area = value;
				this.UpdateLink();
			}
		}

		private void OnEnable()
		{
			this.AddLink();
			if (this.m_AutoUpdatePosition && this.m_LinkInstance.valid)
			{
				NavMeshLink.AddTracking(this);
			}
		}

		private void OnDisable()
		{
			NavMeshLink.RemoveTracking(this);
			this.m_LinkInstance.Remove();
		}

		public void UpdateLink()
		{
			this.m_LinkInstance.Remove();
			this.AddLink();
		}

		private static void AddTracking(NavMeshLink link)
		{
			if (NavMeshLink.s_Tracked.Count == 0)
			{
				Delegate onPreUpdate = NavMesh.onPreUpdate;
				if (NavMeshLink.<>f__mg$cache0 == null)
				{
					NavMeshLink.<>f__mg$cache0 = new NavMesh.OnNavMeshPreUpdate(NavMeshLink.UpdateTrackedInstances);
				}
				NavMesh.onPreUpdate = (NavMesh.OnNavMeshPreUpdate)Delegate.Combine(onPreUpdate, NavMeshLink.<>f__mg$cache0);
			}
			NavMeshLink.s_Tracked.Add(link);
		}

		private static void RemoveTracking(NavMeshLink link)
		{
			NavMeshLink.s_Tracked.Remove(link);
			if (NavMeshLink.s_Tracked.Count == 0)
			{
				Delegate onPreUpdate = NavMesh.onPreUpdate;
				if (NavMeshLink.<>f__mg$cache1 == null)
				{
					NavMeshLink.<>f__mg$cache1 = new NavMesh.OnNavMeshPreUpdate(NavMeshLink.UpdateTrackedInstances);
				}
				NavMesh.onPreUpdate = (NavMesh.OnNavMeshPreUpdate)Delegate.Remove(onPreUpdate, NavMeshLink.<>f__mg$cache1);
			}
		}

		private void SetAutoUpdate(bool value)
		{
			if (this.m_AutoUpdatePosition == value)
			{
				return;
			}
			this.m_AutoUpdatePosition = value;
			if (value)
			{
				NavMeshLink.AddTracking(this);
			}
			else
			{
				NavMeshLink.RemoveTracking(this);
			}
		}

		private void AddLink()
		{
			this.m_LinkInstance = NavMesh.AddLink(new NavMeshLinkData
			{
				startPosition = this.m_StartPoint,
				endPosition = this.m_EndPoint,
				width = this.m_Width,
				costModifier = (float)this.m_CostModifier,
				bidirectional = this.m_Bidirectional,
				area = this.m_Area,
				agentTypeID = this.m_AgentTypeID
			}, base.transform.position, base.transform.rotation);
			if (this.m_LinkInstance.valid)
			{
				this.m_LinkInstance.owner = this;
			}
			this.m_LastPosition = base.transform.position;
			this.m_LastRotation = base.transform.rotation;
		}

		private bool HasTransformChanged()
		{
			return this.m_LastPosition != base.transform.position || this.m_LastRotation != base.transform.rotation;
		}

		private void OnDidApplyAnimationProperties()
		{
			this.UpdateLink();
		}

		private static void UpdateTrackedInstances()
		{
			foreach (NavMeshLink navMeshLink in NavMeshLink.s_Tracked)
			{
				if (navMeshLink.HasTransformChanged())
				{
					navMeshLink.UpdateLink();
				}
			}
		}

		[SerializeField]
		private int m_AgentTypeID;

		[SerializeField]
		private Vector3 m_StartPoint = new Vector3(0f, 0f, -2.5f);

		[SerializeField]
		private Vector3 m_EndPoint = new Vector3(0f, 0f, 2.5f);

		[SerializeField]
		private float m_Width;

		[SerializeField]
		private int m_CostModifier = -1;

		[SerializeField]
		private bool m_Bidirectional = true;

		[SerializeField]
		private bool m_AutoUpdatePosition;

		[SerializeField]
		private int m_Area;

		private NavMeshLinkInstance m_LinkInstance = default(NavMeshLinkInstance);

		private Vector3 m_LastPosition = Vector3.zero;

		private Quaternion m_LastRotation = Quaternion.identity;

		private static readonly List<NavMeshLink> s_Tracked = new List<NavMeshLink>();

		[CompilerGenerated]
		private static NavMesh.OnNavMeshPreUpdate <>f__mg$cache0;

		[CompilerGenerated]
		private static NavMesh.OnNavMeshPreUpdate <>f__mg$cache1;
	}
}
