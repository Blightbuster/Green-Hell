using System;
using Pathfinding.Util;

namespace Pathfinding
{
	internal class PathReturnQueue
	{
		public PathReturnQueue(object pathsClaimedSilentlyBy)
		{
			this.pathsClaimedSilentlyBy = pathsClaimedSilentlyBy;
		}

		public void Enqueue(Path path)
		{
			this.pathReturnStack.Push(path);
		}

		public void ReturnPaths(bool timeSlice)
		{
			Path next = this.pathReturnStack.PopAll();
			if (this.pathReturnPop == null)
			{
				this.pathReturnPop = next;
			}
			else
			{
				Path next2 = this.pathReturnPop;
				while (next2.next != null)
				{
					next2 = next2.next;
				}
				next2.next = next;
			}
			long num = (!timeSlice) ? 0L : (DateTime.UtcNow.Ticks + 10000L);
			int num2 = 0;
			while (this.pathReturnPop != null)
			{
				Path path = this.pathReturnPop;
				this.pathReturnPop = this.pathReturnPop.next;
				path.next = null;
				((IPathInternals)path).ReturnPath();
				((IPathInternals)path).AdvanceState(PathState.Returned);
				path.Release(this.pathsClaimedSilentlyBy, true);
				num2++;
				if (num2 > 5 && timeSlice)
				{
					num2 = 0;
					if (DateTime.UtcNow.Ticks >= num)
					{
						return;
					}
				}
			}
		}

		private LockFreeStack pathReturnStack = new LockFreeStack();

		private Path pathReturnPop;

		private object pathsClaimedSilentlyBy;
	}
}
