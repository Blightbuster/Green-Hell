using System;

namespace Pathfinding
{
	public struct AstarWorkItem
	{
		public AstarWorkItem(Func<bool, bool> update)
		{
			this.init = null;
			this.initWithContext = null;
			this.updateWithContext = null;
			this.update = update;
		}

		public AstarWorkItem(Func<IWorkItemContext, bool, bool> update)
		{
			this.init = null;
			this.initWithContext = null;
			this.updateWithContext = update;
			this.update = null;
		}

		public AstarWorkItem(Action init, Func<bool, bool> update = null)
		{
			this.init = init;
			this.initWithContext = null;
			this.update = update;
			this.updateWithContext = null;
		}

		public AstarWorkItem(Action<IWorkItemContext> init, Func<IWorkItemContext, bool, bool> update = null)
		{
			this.init = null;
			this.initWithContext = init;
			this.update = null;
			this.updateWithContext = update;
		}

		public Action init;

		public Action<IWorkItemContext> initWithContext;

		public Func<bool, bool> update;

		public Func<IWorkItemContext, bool, bool> updateWithContext;
	}
}
