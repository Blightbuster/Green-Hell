using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	public abstract class NavmeshClipper : VersionedMonoBehaviour
	{
		public NavmeshClipper()
		{
			this.node = new LinkedListNode<NavmeshClipper>(this);
		}

		public static void AddEnableCallback(Action<NavmeshClipper> onEnable, Action<NavmeshClipper> onDisable)
		{
			NavmeshClipper.OnEnableCallback = (Action<NavmeshClipper>)Delegate.Combine(NavmeshClipper.OnEnableCallback, onEnable);
			NavmeshClipper.OnDisableCallback = (Action<NavmeshClipper>)Delegate.Combine(NavmeshClipper.OnDisableCallback, onDisable);
			for (LinkedListNode<NavmeshClipper> linkedListNode = NavmeshClipper.all.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				onEnable(linkedListNode.Value);
			}
		}

		public static void RemoveEnableCallback(Action<NavmeshClipper> onEnable, Action<NavmeshClipper> onDisable)
		{
			NavmeshClipper.OnEnableCallback = (Action<NavmeshClipper>)Delegate.Remove(NavmeshClipper.OnEnableCallback, onEnable);
			NavmeshClipper.OnDisableCallback = (Action<NavmeshClipper>)Delegate.Remove(NavmeshClipper.OnDisableCallback, onDisable);
			for (LinkedListNode<NavmeshClipper> linkedListNode = NavmeshClipper.all.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				onDisable(linkedListNode.Value);
			}
		}

		public static bool AnyEnableListeners
		{
			get
			{
				return NavmeshClipper.OnEnableCallback != null;
			}
		}

		protected virtual void OnEnable()
		{
			NavmeshClipper.all.AddFirst(this.node);
			if (NavmeshClipper.OnEnableCallback != null)
			{
				NavmeshClipper.OnEnableCallback(this);
			}
		}

		protected virtual void OnDisable()
		{
			if (NavmeshClipper.OnDisableCallback != null)
			{
				NavmeshClipper.OnDisableCallback(this);
			}
			NavmeshClipper.all.Remove(this.node);
		}

		internal abstract void NotifyUpdated();

		internal abstract Rect GetBounds(GraphTransform transform);

		public abstract bool RequiresUpdate();

		public abstract void ForceUpdate();

		private static Action<NavmeshClipper> OnEnableCallback;

		private static Action<NavmeshClipper> OnDisableCallback;

		private static readonly LinkedList<NavmeshClipper> all = new LinkedList<NavmeshClipper>();

		private readonly LinkedListNode<NavmeshClipper> node;
	}
}
