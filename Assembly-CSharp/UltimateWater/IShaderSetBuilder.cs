using System;
using UnityEngine;

namespace UltimateWater
{
	public interface IShaderSetBuilder
	{
		Shader BuildShaderVariant(string[] localKeywords, string[] sharedKeywords, string additionalCode, string keywordsString, bool volume, bool useForwardPasses, bool useDeferredPass);
	}
}
