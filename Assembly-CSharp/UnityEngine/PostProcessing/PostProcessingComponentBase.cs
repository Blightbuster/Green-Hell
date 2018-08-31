using System;

namespace UnityEngine.PostProcessing
{
	public abstract class PostProcessingComponentBase
	{
		public virtual DepthTextureMode GetCameraFlags()
		{
			return DepthTextureMode.None;
		}

		public abstract bool active { get; }

		public virtual void OnEnable()
		{
		}

		public virtual void OnDisable()
		{
		}

		public abstract PostProcessingModel GetModel();

		public PostProcessingContext context;
	}
}
