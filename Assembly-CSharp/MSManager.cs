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

	public MSMultiSample PlayMultiSample(string ms_name, float fade_in)
	{
		MSMultiSample msmultiSample = this.FindMultisample(ms_name);
		if (msmultiSample != null)
		{
			msmultiSample.Play(fade_in);
			return msmultiSample;
		}
		return null;
	}

	public void StopMultiSample(MSMultiSample multisample, float fade_out)
	{
		multisample.Stop(fade_out);
	}

	private MSMultiSample FindMultisample(string name)
	{
		for (int i = 0; i < this.m_MultiSamples.Count; i++)
		{
			MSMultiSample msmultiSample = this.m_MultiSamples[i];
			if (msmultiSample.m_Name == name)
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
