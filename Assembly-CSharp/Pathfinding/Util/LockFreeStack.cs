using System;
using System.Threading;

namespace Pathfinding.Util
{
	public class LockFreeStack
	{
		public void Push(Path p)
		{
			do
			{
				p.next = this.head;
			}
			while (Interlocked.CompareExchange<Path>(ref this.head, p, p.next) != p.next);
		}

		public Path PopAll()
		{
			return Interlocked.Exchange<Path>(ref this.head, null);
		}

		public Path head;
	}
}
