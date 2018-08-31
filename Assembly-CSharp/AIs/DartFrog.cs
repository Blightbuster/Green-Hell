using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

namespace AIs
{
	public class DartFrog : AI
	{
		protected override void Start()
		{
			base.Start();
			if (!DartFrog.s_MaterialsLoaded)
			{
				this.LoadMaterials();
			}
			this.ApplyRandomMaterial();
		}

		private void LoadMaterials()
		{
			DartFrog.s_Materials = new List<Material>();
			Material item = Resources.Load("AI/PoisonDartFrog/Materials/BlueePoisonDirtFrogDetail") as Material;
			DartFrog.s_Materials.Add(item);
			item = (Resources.Load("AI/PoisonDartFrog/Materials/GreenPoisonDirtFrogDetail") as Material);
			DartFrog.s_Materials.Add(item);
			item = (Resources.Load("AI/PoisonDartFrog/Materials/OrangePoisonDirtFrogDetail") as Material);
			DartFrog.s_Materials.Add(item);
			item = (Resources.Load("AI/PoisonDartFrog/Materials/RedPoisonDirtFrogDetail") as Material);
			DartFrog.s_Materials.Add(item);
			item = (Resources.Load("AI/PoisonDartFrog/Materials/YellowPoisonDirtFrogDetail") as Material);
			DartFrog.s_Materials.Add(item);
			DartFrog.s_MaterialsLoaded = true;
		}

		private void ApplyRandomMaterial()
		{
			List<SkinnedMeshRenderer> componentsDeepChild = General.GetComponentsDeepChild<SkinnedMeshRenderer>(base.gameObject);
			Material material = DartFrog.s_Materials[UnityEngine.Random.Range(0, DartFrog.s_Materials.Count)];
			for (int i = 0; i < componentsDeepChild.Count; i++)
			{
				componentsDeepChild[i].material = material;
			}
		}

		private static bool s_MaterialsLoaded;

		private static List<Material> s_Materials;
	}
}
