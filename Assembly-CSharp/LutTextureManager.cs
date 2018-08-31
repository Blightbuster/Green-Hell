using System;
using System.Collections.Generic;
using UnityEngine;

public class LutTextureManager : MonoBehaviour
{
	public static LutTextureManager Get()
	{
		return LutTextureManager.s_Instance;
	}

	private void Awake()
	{
		LutTextureManager.s_Instance = this;
		this.LoadScript();
		this.m_AmplifyColorEffect = Camera.main.GetComponent<AmplifyColorEffect>();
		DebugUtils.Assert(this.m_AmplifyColorEffect != null, "Lut Texture manager - Missing AmplifyColorEffect component!", true, DebugUtils.AssertType.Info);
	}

	private void LoadScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse(this.m_LutTextureScript, true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Effect")
			{
				LutTextureData lutTextureData = new LutTextureData();
				lutTextureData.m_Effect = (LutTextureManager.LutEffect)Enum.Parse(typeof(LutTextureManager.LutEffect), key.GetVariable(0).SValue);
				lutTextureData.m_LutTexture = key.GetVariable(1).SValue;
				lutTextureData.m_LutBlendTexture = key.GetVariable(2).SValue;
				lutTextureData.m_MaskTexture = key.GetVariable(3).SValue;
				lutTextureData.m_BlendAmount = key.GetVariable(4).FValue;
				this.m_Datas.Add(lutTextureData.m_Effect, lutTextureData);
			}
		}
	}

	public void ResetEffect()
	{
		this.SetEffect(LutTextureManager.LutEffect.Default);
	}

	public void SetEffect(LutTextureManager.LutEffect effect)
	{
		LutTextureData lutTextureData;
		if (!this.m_Datas.TryGetValue(effect, out lutTextureData))
		{
			DebugUtils.Assert("Can't find effect - " + effect.ToString(), true, DebugUtils.AssertType.Info);
			return;
		}
		if (lutTextureData.m_LutTexture != null)
		{
			Resources.UnloadAsset(this.m_AmplifyColorEffect.LutTexture);
			this.m_AmplifyColorEffect.LutTexture = (Resources.Load("Textures/" + lutTextureData.m_LutTexture) as Texture);
		}
		else
		{
			this.m_AmplifyColorEffect.LutTexture = null;
		}
		if (lutTextureData.m_LutBlendTexture != null)
		{
			Resources.UnloadAsset(this.m_AmplifyColorEffect.LutBlendTexture);
			this.m_AmplifyColorEffect.LutBlendTexture = (Resources.Load("Textures/" + lutTextureData.m_LutBlendTexture) as Texture);
		}
		else
		{
			this.m_AmplifyColorEffect.LutBlendTexture = null;
		}
		if (lutTextureData.m_MaskTexture != null)
		{
			Resources.UnloadAsset(this.m_AmplifyColorEffect.MaskTexture);
			this.m_AmplifyColorEffect.MaskTexture = (Resources.Load("Textures/" + lutTextureData.m_MaskTexture) as Texture);
		}
		else
		{
			this.m_AmplifyColorEffect.MaskTexture = null;
		}
		this.m_AmplifyColorEffect.BlendAmount = lutTextureData.m_BlendAmount;
		this.m_CurrentEffect = effect;
	}

	public void SetBlendAmount(LutTextureManager.LutEffect effect, float blend)
	{
		if (effect != this.m_CurrentEffect)
		{
			DebugUtils.Assert("Different effect - effect = " + effect.ToString() + "current effect = " + this.m_CurrentEffect.ToString(), true, DebugUtils.AssertType.Info);
			return;
		}
		this.m_AmplifyColorEffect.BlendAmount = blend;
	}

	public string m_LutTextureScript = string.Empty;

	private Dictionary<LutTextureManager.LutEffect, LutTextureData> m_Datas = new Dictionary<LutTextureManager.LutEffect, LutTextureData>();

	private AmplifyColorEffect m_AmplifyColorEffect;

	public LutTextureManager.LutEffect m_CurrentEffect;

	private static LutTextureManager s_Instance;

	public enum LutEffect
	{
		Default,
		Dream,
		Panic
	}
}
