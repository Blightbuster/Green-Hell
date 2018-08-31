using System;

namespace Pathfinding
{
	[Serializable]
	public abstract class PathModifier : IPathModifier
	{
		public abstract int Order { get; }

		public void Awake(Seeker s)
		{
			this.seeker = s;
			if (s != null)
			{
				s.RegisterModifier(this);
			}
		}

		public void OnDestroy(Seeker s)
		{
			if (s != null)
			{
				s.DeregisterModifier(this);
			}
		}

		public virtual void PreProcess(Path p)
		{
		}

		public abstract void Apply(Path p);

		[NonSerialized]
		public Seeker seeker;
	}
}
