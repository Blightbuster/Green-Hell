using System;

namespace UnityEngine.PostProcessing
{
	[Serializable]
	public class FogModel : PostProcessingModel
	{
		public FogModel.Settings settings
		{
			get
			{
				return this.m_Settings;
			}
			set
			{
				this.m_Settings = value;
			}
		}

		public override void Reset()
		{
			this.m_Settings = FogModel.Settings.defaultSettings;
		}

		[SerializeField]
		private FogModel.Settings m_Settings = FogModel.Settings.defaultSettings;

		[Serializable]
		public struct Settings
		{
			public static FogModel.Settings defaultSettings
			{
				get
				{
					return new FogModel.Settings
					{
						excludeSkybox = true
					};
				}
			}

			[Tooltip("Should the fog affect the skybox?")]
			public bool excludeSkybox;
		}
	}
}
