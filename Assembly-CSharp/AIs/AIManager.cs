using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class AIManager : MonoBehaviour, ISaveLoad
	{
		public static AIManager Get()
		{
			return AIManager.s_Instance;
		}

		private void Awake()
		{
			AIManager.s_Instance = this;
			this.InitFleshHitSounds();
			this.InitHumanSoundsParser();
			this.InitializeAnimEventsParsers();
			this.InitializeAnimatorDataParsers();
			this.InitializeGoalParsers();
			this.InitializeAIParams();
			this.InitializeBloodFXData();
			this.InitFootstepsData();
			AIManager.s_WalkableAreaMask = 1 << NavMesh.GetAreaFromName("Walkable");
			this.m_AIWaveManager = new AIWavesManager();
		}

		private void InitFleshHitSounds()
		{
			AudioClip[] collection = Resources.LoadAll<AudioClip>("Sounds/AI/FleshHit");
			this.m_FleshHitSounds = new List<AudioClip>(collection);
		}

		private void InitHumanSoundsParser()
		{
			TextAsset textAsset = Resources.Load("Scripts/AI/HumanSounds") as TextAsset;
			if (textAsset)
			{
				TextAssetParser textAssetParser = new TextAssetParser(textAsset);
				for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
				{
					Key key = textAssetParser.GetKey(i);
					if (key.GetName() == "Sound")
					{
						HumanAISoundModule.SoundType key2 = (HumanAISoundModule.SoundType)Enum.Parse(typeof(HumanAISoundModule.SoundType), key.GetVariable(0).SValue);
						this.m_Tribe0Sounds[(int)key2] = new List<AudioClip>();
						this.m_Tribe1Sounds[(int)key2] = new List<AudioClip>();
						this.m_Tribe2Sounds[(int)key2] = new List<AudioClip>();
						string svalue = key.GetVariable(1).SValue;
						string path = "Sounds/" + key.GetVariable(2).SValue + "/Tribe0/" + svalue;
						AudioClip audioClip = Resources.Load<AudioClip>(path);
						if (audioClip)
						{
							this.m_Tribe0Sounds[(int)key2].Add(audioClip);
						}
						else
						{
							for (int j = 1; j < 99; j++)
							{
								path = string.Concat(new string[]
								{
									"Sounds/",
									key.GetVariable(2).SValue,
									"/Tribe0/",
									svalue,
									(j >= 10) ? string.Empty : "0",
									j.ToString()
								});
								audioClip = Resources.Load<AudioClip>(path);
								if (!audioClip)
								{
									break;
								}
								this.m_Tribe0Sounds[(int)key2].Add(audioClip);
							}
						}
						path = "Sounds/" + key.GetVariable(2).SValue + "/Tribe1/" + svalue;
						audioClip = Resources.Load<AudioClip>(path);
						if (audioClip)
						{
							this.m_Tribe1Sounds[(int)key2].Add(audioClip);
						}
						else
						{
							for (int k = 1; k < 99; k++)
							{
								path = string.Concat(new string[]
								{
									"Sounds/",
									key.GetVariable(2).SValue,
									"/Tribe1/",
									svalue,
									(k >= 10) ? string.Empty : "0",
									k.ToString()
								});
								audioClip = Resources.Load<AudioClip>(path);
								if (!audioClip)
								{
									break;
								}
								this.m_Tribe1Sounds[(int)key2].Add(audioClip);
							}
						}
						path = "Sounds/" + key.GetVariable(2).SValue + "/Tribe2/" + svalue;
						audioClip = Resources.Load<AudioClip>(path);
						if (audioClip)
						{
							this.m_Tribe2Sounds[(int)key2].Add(audioClip);
						}
						else
						{
							for (int l = 1; l < 99; l++)
							{
								path = string.Concat(new string[]
								{
									"Sounds/",
									key.GetVariable(2).SValue,
									"/Tribe2/",
									svalue,
									(l >= 10) ? string.Empty : "0",
									l.ToString()
								});
								audioClip = Resources.Load<AudioClip>(path);
								if (!audioClip)
								{
									break;
								}
								this.m_Tribe2Sounds[(int)key2].Add(audioClip);
							}
						}
					}
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				DebugUtils.Assert("Missing HumanSounds script!", true, DebugUtils.AssertType.Info);
			}
		}

		private void InitFootstepsData()
		{
			for (int i = 0; i < 13; i++)
			{
				this.m_FootstepDatas.Add(i, new AIFootstepData());
			}
			TextAssetParser textAssetParser = new TextAssetParser(this.m_FootstepDataScript);
			for (int j = 0; j < textAssetParser.GetKeysCount(); j++)
			{
				Key key = textAssetParser.GetKey(j);
				EObjectMaterial key2 = (EObjectMaterial)Enum.Parse(typeof(EObjectMaterial), key.GetName());
				for (int k = 0; k < key.GetKeysCount(); k++)
				{
					AIFootstepData aifootstepData = this.m_FootstepDatas[(int)key2];
					Key key3 = key.GetKey(k);
					if (key3.GetName() == "FX")
					{
						string[] array = key3.GetVariable(0).SValue.Split(new char[]
						{
							';'
						});
						for (int l = 0; l < array.Length; l++)
						{
							aifootstepData.m_FXNames.Add(array[l]);
						}
					}
					else if (key3.GetName() == "Sound")
					{
						string svalue = key3.GetVariable(0).SValue;
						string[] array2 = key3.GetVariable(1).SValue.Split(new char[]
						{
							';'
						});
						for (int m = 0; m < array2.Length; m++)
						{
							aifootstepData.m_Sounds.Add(Resources.Load<AudioClip>(svalue + array2[m]));
						}
					}
				}
			}
		}

		public string GetFootstepFX(EObjectMaterial mat)
		{
			List<string> fxnames = this.m_FootstepDatas[(int)mat].m_FXNames;
			if (fxnames.Count == 0)
			{
				return string.Empty;
			}
			return fxnames[UnityEngine.Random.Range(0, fxnames.Count)];
		}

		public AudioClip GetFootstepSound(EObjectMaterial mat)
		{
			List<AudioClip> sounds = this.m_FootstepDatas[(int)mat].m_Sounds;
			if (sounds.Count == 0)
			{
				return null;
			}
			return sounds[UnityEngine.Random.Range(0, sounds.Count)];
		}

		private void InitializeGoalParsers()
		{
			for (int i = 0; i < 40; i++)
			{
				List<TextAssetParser> list = new List<TextAssetParser>();
				AI.AIID aiid = (AI.AIID)i;
				string text = aiid.ToString() + "Goals";
				string path = "Scripts/AI/" + text;
				TextAsset textAsset = Resources.Load(path) as TextAsset;
				if (textAsset)
				{
					TextAssetParser textAssetParser = new TextAssetParser(textAsset);
					list.Add(textAssetParser);
					this.m_GoalParsersByName.Add(text, textAssetParser);
					Resources.UnloadAsset(textAsset);
				}
				else
				{
					for (int j = 0; j < 9999; j++)
					{
						AI.AIID aiid2 = (AI.AIID)i;
						text = aiid2.ToString() + "Goals_" + j.ToString();
						path = "Scripts/AI/" + text;
						textAsset = (Resources.Load(path) as TextAsset);
						if (!textAsset)
						{
							break;
						}
						TextAssetParser textAssetParser2 = new TextAssetParser(textAsset);
						list.Add(textAssetParser2);
						this.m_GoalParsersByName.Add(text, textAssetParser2);
						Resources.UnloadAsset(textAsset);
					}
				}
				this.m_GoalParsers.Add(i, list);
			}
		}

		private void InitializeAnimEventsParsers()
		{
			for (int i = 0; i < 40; i++)
			{
				List<TextAssetParser> list = new List<TextAssetParser>();
				AI.AIID aiid = (AI.AIID)i;
				string text = aiid.ToString() + "AnimEvents";
				string path = "Scripts/AI/" + text;
				TextAsset textAsset = Resources.Load(path) as TextAsset;
				if (textAsset)
				{
					TextAssetParser textAssetParser = new TextAssetParser(textAsset);
					list.Add(textAssetParser);
					this.m_AnimEventsParsers.Add(text, textAssetParser);
					Resources.UnloadAsset(textAsset);
				}
				else
				{
					Debug.Log("Can't load anim evets script - " + text);
				}
			}
		}

		private void InitializeAnimatorDataParsers()
		{
			for (int i = 0; i < 40; i++)
			{
				List<TextAssetParser> list = new List<TextAssetParser>();
				AI.AIID aiid = (AI.AIID)i;
				string text = aiid.ToString() + "AnimatorData";
				string path = "Scripts/AI/" + text;
				TextAsset textAsset = Resources.Load(path) as TextAsset;
				if (textAsset)
				{
					TextAssetParser textAssetParser = new TextAssetParser(textAsset);
					list.Add(textAssetParser);
					this.m_AnimatorDataParsers.Add(text, textAssetParser);
					Resources.UnloadAsset(textAsset);
				}
				else
				{
					Debug.Log("Can't load animator data script - " + text);
				}
			}
		}

		private void InitializeAIParams()
		{
			string path = "Scripts/AI/AIData";
			TextAsset textAsset = Resources.Load(path) as TextAsset;
			if (!textAsset)
			{
				DebugUtils.Assert("Can't load AIData script!", true, DebugUtils.AssertType.Info);
				return;
			}
			TextAssetParser textAssetParser = new TextAssetParser(textAsset);
			for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
			{
				Key key = textAssetParser.GetKey(i);
				if (key.GetName() == "AI")
				{
					AIParams aiparams = new AIParams();
					aiparams.Load(key);
					this.m_AIParamsMap.Add((int)Enum.Parse(typeof(AI.AIID), key.GetVariable(0).SValue), aiparams);
				}
			}
			Resources.UnloadAsset(textAsset);
		}

		private void InitializeBloodFXData()
		{
			string path = "Scripts/AI/BloodFXData";
			TextAsset textAsset = Resources.Load(path) as TextAsset;
			if (!textAsset)
			{
				DebugUtils.Assert("Can't load BloodFXData script!", true, DebugUtils.AssertType.Info);
				return;
			}
			TextAssetParser textAssetParser = new TextAssetParser(textAsset);
			for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
			{
				Key key = textAssetParser.GetKey(i);
				if (key.GetName() == "Blunt")
				{
					List<string> value = new List<string>(key.GetVariable(0).SValue.Split(new char[]
					{
						';'
					}));
					this.m_BloodFXNames.Add(0, value);
				}
				else if (key.GetName() == "Sharp")
				{
					List<string> value2 = new List<string>(key.GetVariable(0).SValue.Split(new char[]
					{
						';'
					}));
					this.m_BloodFXNames.Add(1, value2);
				}
			}
			Resources.UnloadAsset(textAsset);
		}

		public void RegisterAI(AI new_ai)
		{
			if (!this.m_AllAIs.Contains(new_ai))
			{
				this.m_AllAIs.Add(new_ai);
				this.UpdateActivity(true);
				if (new_ai.IsHuman())
				{
					this.m_AllHumanAIs.Add(new_ai);
				}
				else
				{
					this.m_AllAnimalAIs.Add(new_ai);
				}
			}
		}

		public void UnregisterAI(AI ai)
		{
			if (this.m_AllAIs.Contains(ai))
			{
				this.m_AllAIs.Remove(ai);
				if (ai.IsHuman())
				{
					this.m_AllHumanAIs.Add(ai);
				}
				else
				{
					this.m_AllAnimalAIs.Remove(ai);
				}
			}
		}

		public void RegisterSpawner(AISpawner spawner)
		{
			if (!this.m_Spawners.Contains(spawner))
			{
				this.m_Spawners.Add(spawner);
			}
		}

		public void UnregisterSpawner(AISpawner spawner)
		{
			if (this.m_Spawners.Contains(spawner))
			{
				this.m_Spawners.Remove(spawner);
			}
		}

		public void OnEnableAI(AI ai)
		{
			this.m_ActiveAIs.Add(ai);
		}

		public void OnDisableAI(AI ai)
		{
			this.m_ActiveAIs.Remove(ai);
		}

		public TextAssetParser GetGoalParser(string name)
		{
			TextAssetParser result = null;
			this.m_GoalParsersByName.TryGetValue(name, out result);
			return result;
		}

		public TextAssetParser GetRandomGoalsParser(AI.AIID id)
		{
			List<TextAssetParser> list = this.m_GoalParsers[(int)id];
			return list[UnityEngine.Random.Range(0, list.Count)];
		}

		public FishTank GetFishTankWithPointInside(Vector3 point)
		{
			foreach (FishTank fishTank in FishTank.s_FishTanks)
			{
				if (fishTank.gameObject.activeSelf)
				{
					if (fishTank.IsPointInside(point))
					{
						return fishTank;
					}
				}
			}
			return null;
		}

		public AI GetClosestAI(Vector3 position, float max_range = 3.40282347E+38f)
		{
			AI result = null;
			float num = float.MaxValue;
			foreach (AI ai in this.m_ActiveAIs)
			{
				float num2 = Vector3.Distance(ai.transform.position, position);
				if (num2 < num && num2 <= max_range)
				{
					result = ai;
					num = num2;
				}
			}
			return result;
		}

		public void OnPlayerStartAttack()
		{
			foreach (AI ai in this.m_ActiveAIs)
			{
				ai.OnPlayerStartAttack();
			}
		}

		public void OnPlayerDie()
		{
			foreach (AI ai in this.m_ActiveAIs)
			{
				ai.OnPlayerDie();
			}
		}

		private void Update()
		{
			this.m_AIWaveManager.Update();
			this.UpdateActivity(false);
			this.UpdateFishTanksActivity();
			this.UpdateSpawners();
			this.UpdateDebug();
		}

		private void UpdateSpawners()
		{
			for (int i = 0; i < 10; i++)
			{
				if (this.m_SpawnerIdx >= this.m_Spawners.Count)
				{
					this.m_SpawnerIdx = 0;
				}
				if (this.m_Spawners.Count > 0 && this.m_SpawnerIdx < this.m_Spawners.Count && this.m_Spawners[this.m_SpawnerIdx] != null)
				{
					this.m_Spawners[this.m_SpawnerIdx].UpdateSpawner();
				}
				this.m_SpawnerIdx++;
			}
		}

		private void UpdateActivity(bool force = false)
		{
			if (!force && Time.time - this.m_LastUpdateActivity < this.m_UpdateActivityInterval)
			{
				return;
			}
			Vector3 position = Player.Get().transform.position;
			foreach (AI ai in this.m_AllAnimalAIs)
			{
				if (!ai.m_Trap)
				{
					if (ai.CheckActivityByDistance())
					{
						float num = Vector3.Distance(position, ai.transform.position);
						if (num > this.m_AIActivationRange)
						{
							UnityEngine.Object.Destroy(ai.gameObject);
						}
					}
				}
			}
			foreach (DeadBody deadBody in this.m_DeadBodies)
			{
				float num2 = Vector3.Distance(position, deadBody.transform.position);
				if (deadBody.gameObject.activeSelf)
				{
					if (num2 > this.m_AIActivationRange)
					{
						deadBody.gameObject.SetActive(false);
						deadBody.m_DeactivationTime = Time.time;
					}
				}
				else if (Time.time - deadBody.m_DeactivationTime > 300f)
				{
					UnityEngine.Object.Destroy(deadBody.gameObject);
				}
				else if (num2 <= this.m_AIActivationRange * 0.75f)
				{
					deadBody.gameObject.SetActive(true);
				}
			}
			this.m_LastUpdateActivity = Time.time;
		}

		private void UpdateFishTanksActivity()
		{
			for (int i = 0; i < FishTank.s_FishTanks.Count; i++)
			{
				FishTank.s_FishTanks[i].ConstantUpdate();
			}
		}

		private void UpdateDebug()
		{
			if (!GreenHellGame.DEBUG)
			{
				return;
			}
			if (Input.GetKey(KeyCode.RightAlt) && Input.GetKey(KeyCode.K))
			{
				this.DebugGiveDamageToAllAIs(float.MaxValue);
			}
			if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.L))
			{
				this.DebugGiveDamageToAllAIs(10f);
			}
			if (this.m_DebugSpawnID != AI.AIID.None && Input.GetKeyDown(KeyCode.Keypad0))
			{
				this.DebugSpawnAI(this.m_DebugSpawnID.ToString(), false, false);
			}
			else if (this.m_DebugSpawnID != AI.AIID.None && Input.GetKeyDown(KeyCode.Keypad2))
			{
				this.DebugSpawnAI(this.m_DebugSpawnID.ToString(), false, true);
			}
			if (GreenHellGame.TWITCH_DEMO && Input.GetKeyDown(KeyCode.Keypad5))
			{
				GameObject prefab = GreenHellGame.Instance.GetPrefab("Jaguar");
				Vector3 forward = Camera.main.transform.forward;
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, JaguarTwitchDemo.s_Object.transform.Find("Spawner").position, JaguarTwitchDemo.s_Object.transform.Find("Spawner").rotation);
				AI component = gameObject.GetComponent<AI>();
				component.m_Attractor = JaguarTwitchDemo.s_Object.transform.Find("Attr_0").GetComponent<AIAttractor>();
			}
			if (this.m_DebugSpawnID != AI.AIID.None && Input.GetKeyDown(KeyCode.Keypad3))
			{
				this.DebugSpawnAI(this.m_DebugSpawnID.ToString(), true, false);
			}
		}

		private void OnGUI()
		{
		}

		private void DebugSpawnAI(string name, bool idle_mode = false, bool hallucination = false)
		{
			GameObject prefab = GreenHellGame.Instance.GetPrefab(name);
			if (!prefab)
			{
				return;
			}
			Vector3 forward = Camera.main.transform.forward;
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, Player.Get().GetHeadTransform().position + forward * 10f, Quaternion.LookRotation(-Camera.main.transform.forward, Vector3.up));
			AI component = gameObject.GetComponent<AI>();
			component.m_Hallucination = hallucination;
		}

		private void DebugGiveDamageToAllAIs(float damage)
		{
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.m_Damage = damage;
			damageInfo.m_Damager = base.gameObject;
			damageInfo.m_Normal = Vector3.up;
			int i = 0;
			while (i < this.m_ActiveAIs.Count)
			{
				if (this.m_ActiveAIs[i] == null)
				{
					this.m_ActiveAIs.RemoveAt(i);
				}
				else if (this.m_ActiveAIs[i].IsFish())
				{
					i++;
				}
				else
				{
					damageInfo.m_Position = this.m_ActiveAIs[i].transform.position;
					this.m_ActiveAIs[i].TakeDamage(damageInfo);
					i++;
				}
			}
		}

		public void Save()
		{
			SaveGame.SaveVal("HAIGroupsCount", HumanAIGroup.s_AIGroups.Count);
			for (int i = 0; i < HumanAIGroup.s_AIGroups.Count; i++)
			{
				HumanAIGroup.s_AIGroups[i].Save(i);
			}
			for (int j = 0; j < FishTank.s_FishTanks.Count; j++)
			{
				FishTank.s_FishTanks[j].Save(j);
			}
			this.m_AIWaveManager.Save();
		}

		public void Load()
		{
			for (int i = 0; i < this.m_AllAIs.Count; i++)
			{
				UnityEngine.Object.Destroy(this.m_AllAIs[i].gameObject);
			}
			this.m_AllAIs.Clear();
			this.m_AllAnimalAIs.Clear();
			this.m_AllHumanAIs.Clear();
			for (int j = 0; j < this.m_DeadBodies.Count; j++)
			{
				UnityEngine.Object.Destroy(this.m_DeadBodies[j].gameObject);
			}
			this.m_DeadBodies.Clear();
			for (int k = 0; k < HumanAIGroup.s_AIGroups.Count; k++)
			{
				HumanAIGroup.s_AIGroups[k].Load(k);
			}
			for (int l = 0; l < FishTank.s_FishTanks.Count; l++)
			{
				FishTank.s_FishTanks[l].Load(l);
			}
			this.m_AIWaveManager.Load();
		}

		private List<AI> m_AllAnimalAIs = new List<AI>();

		[HideInInspector]
		public List<AI> m_AllHumanAIs = new List<AI>();

		private List<AI> m_AllAIs = new List<AI>();

		public List<AI> m_ActiveAIs = new List<AI>();

		public List<DeadBody> m_DeadBodies = new List<DeadBody>();

		private float m_LastUpdateActivity;

		private float m_UpdateActivityInterval = 1f;

		public float m_AIActivationRange = 40f;

		public float m_FlocksActivationRange = 40f;

		public float m_FishActivationRange = 10f;

		public float m_FishDeactivationRange = 15f;

		public List<AudioClip> m_FleshHitSounds;

		public Dictionary<int, List<AudioClip>> m_Tribe0Sounds = new Dictionary<int, List<AudioClip>>();

		public Dictionary<int, List<AudioClip>> m_Tribe1Sounds = new Dictionary<int, List<AudioClip>>();

		public Dictionary<int, List<AudioClip>> m_Tribe2Sounds = new Dictionary<int, List<AudioClip>>();

		public Dictionary<string, TextAssetParser> m_AnimEventsParsers = new Dictionary<string, TextAssetParser>();

		public Dictionary<string, TextAssetParser> m_AnimatorDataParsers = new Dictionary<string, TextAssetParser>();

		public Dictionary<int, List<TextAssetParser>> m_GoalParsers = new Dictionary<int, List<TextAssetParser>>();

		public Dictionary<string, TextAssetParser> m_GoalParsersByName = new Dictionary<string, TextAssetParser>();

		public Dictionary<int, AIParams> m_AIParamsMap = new Dictionary<int, AIParams>();

		private Dictionary<int, AIFootstepData> m_FootstepDatas = new Dictionary<int, AIFootstepData>();

		public TextAsset m_FootstepDataScript;

		public List<AISpawner> m_Spawners = new List<AISpawner>();

		[HideInInspector]
		public AI.AIID m_DebugSpawnID = AI.AIID.None;

		public static int s_WalkableAreaMask;

		private AIWavesManager m_AIWaveManager;

		[HideInInspector]
		public Dictionary<int, List<string>> m_BloodFXNames = new Dictionary<int, List<string>>();

		[HideInInspector]
		public List<AI.AIID> m_ProcessedAnimEventsReceivers = new List<AI.AIID>();

		private static AIManager s_Instance;

		private int m_SpawnerIdx;

		public enum BloodFXType
		{
			Blunt,
			Sharp
		}
	}
}
