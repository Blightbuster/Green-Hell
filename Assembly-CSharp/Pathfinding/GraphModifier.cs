using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
	[ExecuteInEditMode]
	public abstract class GraphModifier : VersionedMonoBehaviour
	{
		protected static List<T> GetModifiersOfType<T>() where T : GraphModifier
		{
			GraphModifier graphModifier = GraphModifier.root;
			List<T> list = new List<T>();
			while (graphModifier != null)
			{
				T t = graphModifier as T;
				if (t != null)
				{
					list.Add(t);
				}
				graphModifier = graphModifier.next;
			}
			return list;
		}

		public static void FindAllModifiers()
		{
			GraphModifier[] array = UnityEngine.Object.FindObjectsOfType(typeof(GraphModifier)) as GraphModifier[];
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].enabled)
				{
					array[i].OnEnable();
				}
			}
		}

		public static void TriggerEvent(GraphModifier.EventType type)
		{
			if (!Application.isPlaying)
			{
				GraphModifier.FindAllModifiers();
			}
			GraphModifier graphModifier = GraphModifier.root;
			if (type <= GraphModifier.EventType.PreUpdate)
			{
				switch (type)
				{
				case GraphModifier.EventType.PostScan:
					while (graphModifier != null)
					{
						graphModifier.OnPostScan();
						graphModifier = graphModifier.next;
					}
					return;
				case GraphModifier.EventType.PreScan:
					while (graphModifier != null)
					{
						graphModifier.OnPreScan();
						graphModifier = graphModifier.next;
					}
					return;
				case (GraphModifier.EventType)3:
					break;
				case GraphModifier.EventType.LatePostScan:
					while (graphModifier != null)
					{
						graphModifier.OnLatePostScan();
						graphModifier = graphModifier.next;
					}
					return;
				default:
					if (type != GraphModifier.EventType.PreUpdate)
					{
						return;
					}
					while (graphModifier != null)
					{
						graphModifier.OnGraphsPreUpdate();
						graphModifier = graphModifier.next;
					}
					return;
				}
			}
			else
			{
				if (type == GraphModifier.EventType.PostUpdate)
				{
					while (graphModifier != null)
					{
						graphModifier.OnGraphsPostUpdate();
						graphModifier = graphModifier.next;
					}
					return;
				}
				if (type != GraphModifier.EventType.PostCacheLoad)
				{
					return;
				}
				while (graphModifier != null)
				{
					graphModifier.OnPostCacheLoad();
					graphModifier = graphModifier.next;
				}
			}
		}

		protected virtual void OnEnable()
		{
			this.RemoveFromLinkedList();
			this.AddToLinkedList();
			this.ConfigureUniqueID();
		}

		protected virtual void OnDisable()
		{
			this.RemoveFromLinkedList();
		}

		protected override void Awake()
		{
			base.Awake();
			this.ConfigureUniqueID();
		}

		private void ConfigureUniqueID()
		{
			GraphModifier x;
			if (GraphModifier.usedIDs.TryGetValue(this.uniqueID, out x) && x != this)
			{
				this.Reset();
			}
			GraphModifier.usedIDs[this.uniqueID] = this;
		}

		private void AddToLinkedList()
		{
			if (GraphModifier.root == null)
			{
				GraphModifier.root = this;
				return;
			}
			this.next = GraphModifier.root;
			GraphModifier.root.prev = this;
			GraphModifier.root = this;
		}

		private void RemoveFromLinkedList()
		{
			if (GraphModifier.root == this)
			{
				GraphModifier.root = this.next;
				if (GraphModifier.root != null)
				{
					GraphModifier.root.prev = null;
				}
			}
			else
			{
				if (this.prev != null)
				{
					this.prev.next = this.next;
				}
				if (this.next != null)
				{
					this.next.prev = this.prev;
				}
			}
			this.prev = null;
			this.next = null;
		}

		protected virtual void OnDestroy()
		{
			GraphModifier.usedIDs.Remove(this.uniqueID);
		}

		public virtual void OnPostScan()
		{
		}

		public virtual void OnPreScan()
		{
		}

		public virtual void OnLatePostScan()
		{
		}

		public virtual void OnPostCacheLoad()
		{
		}

		public virtual void OnGraphsPreUpdate()
		{
		}

		public virtual void OnGraphsPostUpdate()
		{
		}

		private void Reset()
		{
			ulong num = (ulong)((long)UnityEngine.Random.Range(0, int.MaxValue));
			ulong num2 = (ulong)((ulong)((long)UnityEngine.Random.Range(0, int.MaxValue)) << 32);
			this.uniqueID = (num | num2);
			GraphModifier.usedIDs[this.uniqueID] = this;
		}

		private static GraphModifier root;

		private GraphModifier prev;

		private GraphModifier next;

		[SerializeField]
		[HideInInspector]
		protected ulong uniqueID;

		protected static Dictionary<ulong, GraphModifier> usedIDs = new Dictionary<ulong, GraphModifier>();

		public enum EventType
		{
			PostScan = 1,
			PreScan,
			LatePostScan = 4,
			PreUpdate = 8,
			PostUpdate = 16,
			PostCacheLoad = 32
		}
	}
}
