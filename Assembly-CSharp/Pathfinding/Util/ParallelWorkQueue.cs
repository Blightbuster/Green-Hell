using System;
using System.Collections.Generic;
using System.Threading;

namespace Pathfinding.Util
{
	public class ParallelWorkQueue<T>
	{
		public ParallelWorkQueue(Queue<T> queue)
		{
			this.queue = queue;
			this.initialCount = queue.Count;
			this.threadCount = Math.Min(this.initialCount, Math.Max(1, AstarPath.CalculateThreadCount(ThreadCount.AutomaticHighLoad)));
		}

		public IEnumerable<int> Run(int progressTimeoutMillis)
		{
			if (this.initialCount != this.queue.Count)
			{
				throw new InvalidOperationException("Queue has been modified since the constructor");
			}
			if (this.initialCount == 0)
			{
				yield break;
			}
			this.waitEvents = new ManualResetEvent[this.threadCount];
			for (int i = 0; i < this.waitEvents.Length; i++)
			{
				this.waitEvents[i] = new ManualResetEvent(false);
				ThreadPool.QueueUserWorkItem(delegate(object threadIndex)
				{
					this.RunTask((int)threadIndex);
				}, i);
			}
			for (;;)
			{
				WaitHandle[] waitHandles = this.waitEvents;
				if (WaitHandle.WaitAll(waitHandles, progressTimeoutMillis))
				{
					break;
				}
				Queue<T> obj = this.queue;
				int count;
				lock (obj)
				{
					count = this.queue.Count;
				}
				yield return this.initialCount - count;
			}
			if (this.innerException != null)
			{
				throw this.innerException;
			}
			yield break;
			yield break;
		}

		private void RunTask(int threadIndex)
		{
			try
			{
				for (;;)
				{
					Queue<T> obj = this.queue;
					T arg;
					lock (obj)
					{
						if (this.queue.Count == 0)
						{
							break;
						}
						arg = this.queue.Dequeue();
					}
					this.action(arg, threadIndex);
				}
			}
			catch (Exception ex)
			{
				this.innerException = ex;
				Queue<T> obj = this.queue;
				lock (obj)
				{
					this.queue.Clear();
				}
			}
			finally
			{
				this.waitEvents[threadIndex].Set();
			}
		}

		public Action<T, int> action;

		public readonly int threadCount;

		private readonly Queue<T> queue;

		private readonly int initialCount;

		private ManualResetEvent[] waitEvents;

		private Exception innerException;
	}
}
