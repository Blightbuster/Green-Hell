using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ChallengesManager : MonoBehaviour
{
	public static ChallengesManager Get()
	{
		return ChallengesManager.s_Instance;
	}

	private void Awake()
	{
		ChallengesManager.s_Instance = this;
		this.LoadScript();
		this.LoadScores();
	}

	private void LoadScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Challenges.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Challenge")
			{
				Type type = Type.GetType(key.GetVariable(1).SValue);
				if (!DebugUtils.Assert(type != null, "ERROR - Can't create goal " + key.GetVariable(0).SValue, true, DebugUtils.AssertType.Info))
				{
					Challenge challenge = Activator.CreateInstance(type) as Challenge;
					challenge.Load(key);
					this.m_Challenges.Add(challenge);
				}
			}
		}
	}

	public void FailChallenge()
	{
		if (this.m_ActiveChallenge != null)
		{
			this.m_ActiveChallenge.Fail();
		}
	}

	public bool IsChallengeActive()
	{
		return this.m_ActiveChallenge != null;
	}

	private string GetScoreFileName()
	{
		return Application.persistentDataPath + "/Challenges.dat";
	}

	private void LoadScores()
	{
		if (!File.Exists(this.GetScoreFileName()))
		{
			return;
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Open(this.GetScoreFileName(), FileMode.Open);
		for (int i = 0; i < 99; i++)
		{
			if (i >= this.m_Challenges.Count)
			{
				break;
			}
			this.m_Challenges[i].m_BestScore = (float)binaryFormatter.Deserialize(fileStream);
		}
		fileStream.Close();
	}

	public void ResetChallenges()
	{
		foreach (Challenge challenge in this.m_Challenges)
		{
			challenge.m_BestScore = -1f;
		}
		this.SaveScores();
	}

	public void SaveScores()
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Create(this.GetScoreFileName());
		for (int i = 0; i < 99; i++)
		{
			if (i < this.m_Challenges.Count)
			{
				binaryFormatter.Serialize(fileStream, this.m_Challenges[i].m_BestScore);
			}
			else
			{
				binaryFormatter.Serialize(fileStream, -1f);
			}
		}
		fileStream.Close();
	}

	public void OnLevelLoaded()
	{
		this.m_ActiveChallenge = null;
		this.m_Parent = GameObject.FindGameObjectWithTag("Challenges");
		if (!this.m_Parent)
		{
			return;
		}
		if (this.m_ChallengeToActivate == string.Empty)
		{
			UnityEngine.Object.Destroy(this.m_Parent);
			return;
		}
		for (int i = 0; i < this.m_Parent.transform.childCount; i++)
		{
			this.m_Parent.transform.GetChild(i).gameObject.SetActive(false);
		}
		foreach (Challenge challenge in this.m_Challenges)
		{
			if (challenge.m_Name == this.m_ChallengeToActivate)
			{
				Transform exists = this.m_Parent.transform.Find(challenge.m_ParentName);
				if (!exists)
				{
					return;
				}
				challenge.Activate(this.m_Parent.transform.Find(challenge.m_ParentName).gameObject);
				this.m_ActiveChallenge = challenge;
				if (BalanceSystem.Get())
				{
					BalanceSystem.Get().m_HumanAIStaticCooldown = 0f;
					BalanceSystem.Get().m_HumanAIWaveCooldown = 0f;
					this.m_HumanAICooldownSet = true;
				}
				break;
			}
		}
		this.m_ChallengeToActivate = string.Empty;
	}

	private void Update()
	{
		if (this.m_ActiveChallenge != null)
		{
			if (!MainLevel.Instance)
			{
				this.m_ActiveChallenge = null;
				return;
			}
			if (!this.m_HumanAICooldownSet && BalanceSystem.Get())
			{
				BalanceSystem.Get().m_HumanAIStaticCooldown = 0f;
				BalanceSystem.Get().m_HumanAIWaveCooldown = 0f;
				this.m_HumanAICooldownSet = true;
			}
			this.m_ActiveChallenge.Update();
		}
	}

	public void OnFinishChallenge(bool success)
	{
		this.SaveScores();
		this.m_ActiveChallenge = null;
	}

	public string DateTimeToLocalizedString(DateTime date, bool two_lines)
	{
		string text = date.Hour.ToString();
		text += ":";
		text += date.Minute;
		text += ((!two_lines) ? " " : "\n");
		text += ((date.Day >= 10) ? date.Day.ToString() : ("0" + date.Day.ToString()));
		text += " ";
		string key = string.Empty;
		switch (date.Month)
		{
		case 1:
			key = "Watch_January";
			break;
		case 2:
			key = "Watch_February";
			break;
		case 3:
			key = "Watch_March";
			break;
		case 4:
			key = "Watch_April";
			break;
		case 5:
			key = "Watch_May";
			break;
		case 6:
			key = "Watch_June";
			break;
		case 7:
			key = "Watch_July";
			break;
		case 8:
			key = "Watch_August";
			break;
		case 9:
			key = "Watch_September";
			break;
		case 10:
			key = "Watch_October";
			break;
		case 11:
			key = "Watch_November";
			break;
		case 12:
			key = "Watch_December";
			break;
		}
		return text + GreenHellGame.Instance.GetLocalization().Get(key);
	}

	[HideInInspector]
	public List<Challenge> m_Challenges = new List<Challenge>();

	private Challenge m_ActiveChallenge;

	[HideInInspector]
	public string m_ChallengeToActivate = string.Empty;

	private GameObject m_Parent;

	private static ChallengesManager s_Instance;

	private bool m_HumanAICooldownSet;
}
