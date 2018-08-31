using System;

namespace Pathfinding
{
	[Serializable]
	public abstract class MonoModifier : VersionedMonoBehaviour, IPathModifier
	{
		public void OnEnable()
		{
		}

		public void OnDisable()
		{
		}

		public abstract int Order { get; }

		protected override void Awake()
		{
			base.Awake();
			this.seeker = base.GetComponent<Seeker>();
			if (this.seeker != null)
			{
				this.seeker.RegisterModifier(this);
			}
		}

		public virtual void OnDestroy()
		{
			if (this.seeker != null)
			{
				this.seeker.DeregisterModifier(this);
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
