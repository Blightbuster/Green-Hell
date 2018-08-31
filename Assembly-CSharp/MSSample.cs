using System;
using System.Collections.Generic;
using UnityEngine;

public class MSSample
{
	public void Reload()
	{
		List<MSCurveData> list = new List<MSCurveData>();
		foreach (MSCurveData mscurveData in this.m_Curves.Keys)
		{
			if (!this.MSCurveHashContains(this.m_MultiSample.m_Params, mscurveData.m_Name))
			{
				list.Add(mscurveData);
			}
		}
		foreach (MSCurveData key in list)
		{
			this.m_Curves.Remove(key);
		}
	}

	private bool MSCurveHashContains(List<MSCurveHash> list, string name)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].m_Name == name)
			{
				return true;
			}
		}
		return false;
	}

	public static AnimationCurve MSCurveDataGetCurve(Dictionary<MSCurveData, AnimationCurve> dict, string name)
	{
		foreach (KeyValuePair<MSCurveData, AnimationCurve> keyValuePair in dict)
		{
			if (keyValuePair.Key.m_Name == name)
			{
				return keyValuePair.Value;
			}
		}
		return null;
	}

	public void Load(Key key)
	{
		this.m_WavName = key.GetVariable(0).SValue;
		if (Application.isPlaying)
		{
			this.m_AudioSource = MainLevel.Instance.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
			this.m_AudioSource.clip = (Resources.Load(MSSample.s_SamplesPath + this.m_WavName) as AudioClip);
			this.m_AudioSource.loop = true;
		}
		for (int i = 0; i < key.GetKeysCount(); i++)
		{
			Key key2 = key.GetKey(i);
			if (key2.GetName() == "Curve")
			{
				string svalue = key2.GetVariable(0).SValue;
				AnimationCurve animationCurve = MSSample.MSCurveDataGetCurve(this.m_Curves, svalue);
				if (animationCurve == null)
				{
					animationCurve = new AnimationCurve();
				}
				else
				{
					DebugUtils.Assert(DebugUtils.AssertType.Info);
				}
				animationCurve.preWrapMode = (WrapMode)Enum.Parse(typeof(WrapMode), key2.GetVariable(1).SValue);
				animationCurve.postWrapMode = (WrapMode)Enum.Parse(typeof(WrapMode), key2.GetVariable(2).SValue);
				for (int j = 0; j < key2.GetKeysCount(); j++)
				{
					Key key3 = key2.GetKey(j);
					if (key3.GetName() == "Key")
					{
						animationCurve.AddKey(new Keyframe(key3.GetVariable(0).FValue, key3.GetVariable(1).FValue, key3.GetVariable(2).FValue, key3.GetVariable(3).FValue));
					}
				}
				MSCurveData mscurveData = new MSCurveData();
				mscurveData.m_Name = svalue;
				mscurveData.m_Hash = Animator.StringToHash(mscurveData.m_Name);
				mscurveData.m_Volume = mscurveData.m_Name.EndsWith("_Vol");
				mscurveData.m_Pitch = mscurveData.m_Name.EndsWith("_Pitch");
				this.m_Curves.Add(mscurveData, animationCurve);
			}
		}
	}

	public void Save(Key key)
	{
		CJVariable cjvariable = key.AddVariable();
		cjvariable.SValue = this.m_WavName;
		foreach (MSCurveData mscurveData in this.m_Curves.Keys)
		{
			Key key2 = key.AddKey("Curve");
			cjvariable = key2.AddVariable();
			cjvariable.SValue = mscurveData.m_Name;
			cjvariable = key2.AddVariable();
			cjvariable.SValue = this.m_Curves[mscurveData].preWrapMode.ToString();
			cjvariable = key2.AddVariable();
			cjvariable.SValue = this.m_Curves[mscurveData].postWrapMode.ToString();
			foreach (Keyframe keyframe in this.m_Curves[mscurveData].keys)
			{
				Key key3 = key2.AddKey("Key");
				cjvariable = key3.AddVariable();
				cjvariable.FValue = keyframe.time;
				cjvariable = key3.AddVariable();
				cjvariable.FValue = keyframe.value;
				cjvariable = key3.AddVariable();
				cjvariable.FValue = keyframe.inTangent;
				cjvariable = key3.AddVariable();
				cjvariable.FValue = keyframe.outTangent;
			}
		}
	}

	public void Play()
	{
		this.m_AudioSource.Play();
	}

	public void Stop()
	{
		this.m_AudioSource.Stop();
	}

	public void Update(float volume_mul = 1f)
	{
		this.UpdateVolume(volume_mul);
		this.UpdatePitchShift();
	}

	private void UpdateVolume(float volume_mul)
	{
		float num = 1f;
		foreach (KeyValuePair<MSCurveData, AnimationCurve> keyValuePair in this.m_Curves)
		{
			MSCurveData key = keyValuePair.Key;
			if (key.m_Volume)
			{
				AnimationCurve animationCurve = this.m_Curves[key];
				num *= animationCurve.Evaluate(this.m_MultiSample.m_ParamsMap[key.m_Hash]);
			}
		}
		num = Mathf.Clamp01(num);
		this.m_AudioSource.volume = num * volume_mul;
	}

	private void UpdatePitchShift()
	{
		float num = 1f;
		foreach (KeyValuePair<MSCurveData, AnimationCurve> keyValuePair in this.m_Curves)
		{
			MSCurveData key = keyValuePair.Key;
			if (key.m_Pitch)
			{
				AnimationCurve animationCurve = this.m_Curves[key];
				num *= animationCurve.Evaluate(this.m_MultiSample.m_ParamsMap[key.m_Hash]);
			}
		}
		this.m_AudioSource.pitch = num;
	}

	private static string s_DefaultWavName = "Wav_Name";

	public string m_WavName = MSSample.s_DefaultWavName;

	public static string s_SamplesPath = "Sounds/Ambience/";

	private AudioSource m_AudioSource;

	public Dictionary<MSCurveData, AnimationCurve> m_Curves = new Dictionary<MSCurveData, AnimationCurve>();

	public MSMultiSample m_MultiSample;
}
