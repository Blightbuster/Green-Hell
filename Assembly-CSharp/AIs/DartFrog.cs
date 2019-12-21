using System;
using CJTools;
using UnityEngine;

namespace AIs
{
	public class DartFrog : AI
	{
		protected override void Start()
		{
			base.Start();
			if (!this.m_MaterialApplied)
			{
				this.ApplyRandomMaterial();
			}
		}

		private void ApplyRandomMaterial()
		{
			if (DartFrog.s_Materials == null)
			{
				DartFrog.s_Materials = new Material[DartFrog.s_MaterialNames.Length];
			}
			int num = UnityEngine.Random.Range(0, DartFrog.s_Materials.Length);
			if (DartFrog.s_Materials[num] == null)
			{
				DartFrog.s_Materials[num] = Resources.Load<Material>(DartFrog.s_MaterialNames[num]);
			}
			SkinnedMeshRenderer[] componentsDeepChild = General.GetComponentsDeepChild<SkinnedMeshRenderer>(base.gameObject);
			for (int i = 0; i < componentsDeepChild.Length; i++)
			{
				componentsDeepChild[i].material = DartFrog.s_Materials[num];
			}
		}

		private static bool s_MaterialsLoaded = false;

		private static Material[] s_Materials = null;

		private static string[] s_MaterialNames = new string[]
		{
			"AI/PoisonDartFrog/Materials/BlueePoisonDirtFrogDetail",
			"AI/PoisonDartFrog/Materials/GreenPoisonDirtFrogDetail",
			"AI/PoisonDartFrog/Materials/OrangePoisonDirtFrogDetail",
			"AI/PoisonDartFrog/Materials/RedPoisonDirtFrogDetail",
			"AI/PoisonDartFrog/Materials/YellowPoisonDirtFrogDetail"
		};

		[HideInInspector]
		public bool m_MaterialApplied;
	}
}
