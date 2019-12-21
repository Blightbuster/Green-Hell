using System;
using System.Collections.Generic;
using UnityEngine;

public class MSManager : MonoBehaviour
{
	public static MSManager Get()
	{
		return MSManager.s_Instance;
	}

	private void Awake()
	{
		MSManager.s_Instance = this;
	}

	private void Start()
	{
		this.LoadMultisamples();
	}

	private void LoadMultisamples()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("MultiSample/MultiSamples", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "MultiSample")
			{
				MSMultiSample msmultiSample = new MSMultiSample();
				msmultiSample.Load(key);
				this.m_MultiSamples.Add(msmultiSample);
			}
		}
	}

	public MSMultiSample PlayMultiSample(Component requester, string ms_name, float fade_in, bool change_rainforest_ambient_volume = false, float rainforest_ambient_volume = 1f)
	{
		MSMultiSample msmultiSample = this.FindMultisample(ms_name);
		if (msmultiSample != null)
		{
			msmultiSample.m_ChangeRainforestAmbientVolume = change_rainforest_ambient_volume;
			msmultiSample.m_RainforestAmbientVolume = rainforest_ambient_volume;
			msmultiSample.Play(requester, fade_in);
			return msmultiSample;
		}
		return null;
	}

	public MSMultiSample PlayMultiSample(Component requester, string ms_name, float fade_in)
	{
		MSMultiSample msmultiSample = this.FindMultisample(ms_name);
		if (msmultiSample != null)
		{
			msmultiSample.Play(requester, fade_in);
			return msmultiSample;
		}
		return null;
	}

	public void StopAllMultiSamples()
	{
		for (int i = 0; i < this.m_MultiSamples.Count; i++)
		{
			this.m_MultiSamples[i].ForceStop();
		}
	}

	public void StopMultiSample(Component requester, MSMultiSample multisample, float fade_out)
	{
		if (multisample != null)
		{
			multisample.Stop(requester, fade_out);
		}
	}

	public MSMultiSample FindMultisample(string name)
	{
		for (int i = 0; i < this.m_MultiSamples.Count; i++)
		{
			MSMultiSample msmultiSample = this.m_MultiSamples[i];
			if (msmultiSample.m_Name.GetHashCode() == name.GetHashCode() && msmultiSample.m_Name == name)
			{
				return msmultiSample;
			}
		}
		return null;
	}

	public void Update()
	{
		for (int i = 0; i < this.m_MultiSamples.Count; i++)
		{
			MSMultiSample msmultiSample = this.m_MultiSamples[i];
			if (msmultiSample.m_Playing)
			{
				msmultiSample.Update();
			}
		}
	}

	private List<MSMultiSample> m_MultiSamples = new List<MSMultiSample>();

	private static MSManager s_Instance;
}
