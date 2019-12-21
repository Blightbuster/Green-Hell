using System;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSounds : ScriptableObject
{
	public float m_AmbientIntervalMin = 35f;

	public float m_AmbientIntervalMax = 45f;

	public float m_AudibilityExtraDistance = 15f;

	public List<AmbientSounds.AmbientDefinition> m_AmbientDefinitionsDay;

	public List<AmbientSounds.AmbientDefinition> m_AmbientDefinitionsNight;

	public float m_BigTreeAdditionalY = 6f;

	public List<string> m_BigTreeTags;

	public List<string> m_BushTags;

	public enum EAmbientPositionType
	{
		Only2D,
		Ground,
		Bush,
		HighInAir,
		TreeTops
	}

	[Serializable]
	public class AmbientDefinition
	{
		public bool m_Enabled;

		public AudioClip m_Clip;

		public AmbientSounds.EAmbientPositionType m_Position;

		public bool m_Spatialize;

		public float m_SpatialBlend;

		public float m_DistanceMin;

		public float m_DistanceMax;
	}
}
