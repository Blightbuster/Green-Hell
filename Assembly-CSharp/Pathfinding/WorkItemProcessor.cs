using System;
using UnityEngine;

namespace Pathfinding
{
	internal class WorkItemProcessor : IWorkItemContext
	{
		public WorkItemProcessor(AstarPath astar)
		{
			this.astar = astar;
		}

		public bool workItemsInProgressRightNow { get; private set; }

		public bool workItemsInProgress { get; private set; }

		void IWorkItemContext.QueueFloodFill()
		{
			this.queuedWorkItemFloodFill = true;
		}

		public void EnsureValidFloodFill()
		{
			if (this.queuedWorkItemFloodFill)
			{
				this.astar.FloodFill();
			}
		}

		public void OnFloodFill()
		{
			this.queuedWorkItemFloodFill = false;
		}

		public void AddWorkItem(AstarWorkItem itm)
		{
			this.workItems.Enqueue(itm);
		}

		public bool ProcessWorkItems(bool force)
		{
			if (this.workItemsInProgressRightNow)
			{
				throw new Exception("Processing work items recursively. Please do not wait for other work items to be completed inside work items. If you think this is not caused by any of your scripts, this might be a bug.");
			}
			this.workItemsInProgressRightNow = true;
			this.astar.data.LockGraphStructure(true);
			while (this.workItems.Count > 0)
			{
				if (!this.workItemsInProgress)
				{
					this.workItemsInProgress = true;
					this.queuedWorkItemFloodFill = false;
				}
				AstarWorkItem value = this.workItems[0];
				if (value.init != null)
				{
					value.init();
					value.init = null;
				}
				if (value.initWithContext != null)
				{
					value.initWithContext(this);
					value.initWithContext = null;
				}
				this.workItems[0] = value;
				bool flag;
				try
				{
					if (value.update != null)
					{
						flag = value.update(force);
					}
					else
					{
						flag = (value.updateWithContext == null || value.updateWithContext(this, force));
					}
				}
				catch
				{
					this.workItems.Dequeue();
					this.workItemsInProgressRightNow = false;
					this.astar.data.UnlockGraphStructure();
					throw;
				}
				if (!flag)
				{
					if (force)
					{
						Debug.LogError("Misbehaving WorkItem. 'force'=true but the work item did not complete.\nIf force=true is passed to a WorkItem it should always return true.");
					}
					this.workItemsInProgressRightNow = false;
					this.astar.data.UnlockGraphStructure();
					return false;
				}
				this.workItems.Dequeue();
			}
			this.EnsureValidFloodFill();
			this.workItemsInProgressRightNow = false;
			this.workItemsInProgress = false;
			this.astar.data.UnlockGraphStructure();
			return true;
		}

		private readonly AstarPath astar;

		private readonly WorkItemProcessor.IndexedQueue<AstarWorkItem> workItems = new WorkItemProcessor.IndexedQueue<AstarWorkItem>();

		private bool queuedWorkItemFloodFill;

		private class IndexedQueue<T>
		{
			public T this[int index]
			{
				get
				{
					if (index < 0 || index >= this.length)
					{
						throw new IndexOutOfRangeException();
					}
					return this.buffer[(this.start + index) % this.buffer.Length];
				}
				set
				{
					if (index < 0 || index >= this.length)
					{
						throw new IndexOutOfRangeException();
					}
					this.buffer[(this.start + index) % this.buffer.Length] = value;
				}
			}

			public int Count
			{
				get
				{
					return this.length;
				}
			}

			public void Enqueue(T item)
			{
				if (this.length == this.buffer.Length)
				{
					T[] array = new T[this.buffer.Length * 2];
					for (int i = 0; i < this.length; i++)
					{
						array[i] = this[i];
					}
					this.buffer = array;
					this.start = 0;
				}
				this.buffer[(this.start + this.length) % this.buffer.Length] = item;
				this.length++;
			}

			public T Dequeue()
			{
				if (this.length == 0)
				{
					throw new InvalidOperationException();
				}
				T result = this.buffer[this.start];
				this.start = (this.start + 1) % this.buffer.Length;
				this.length--;
				return result;
			}

			private T[] buffer = new T[4];

			private int start;

			private int length;
		}
	}
}
