using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;

public class PlayerDiseasesModule : PlayerModule, ISaveLoad
{
	public static PlayerDiseasesModule Get()
	{
		return PlayerDiseasesModule.s_Instance;
	}

	private void Awake()
	{
		PlayerDiseasesModule.s_Instance = this;
	}

	public override void Initialize(Being being)
	{
		base.Initialize(being);
		this.LoadScript();
	}

	private void LoadScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Player/Player_Diseases.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Disease")
			{
				Type type = Type.GetType(key.GetVariable(0).SValue);
				if (type == null)
				{
					DebugUtils.Assert(DebugUtils.AssertType.Info);
				}
				else
				{
					Disease disease = Activator.CreateInstance(type) as Disease;
					string svalue = key.GetVariable(0).SValue;
					disease.m_Type = (ConsumeEffect)Enum.Parse(typeof(ConsumeEffect), svalue);
					for (int j = 0; j < key.GetKeysCount(); j++)
					{
						Key key2 = key.GetKey(j);
						disease.Load(key2);
					}
					this.m_Diseases.Add((int)disease.GetDiseaseType(), disease);
				}
			}
			else if (key.GetName() == "Symptom")
			{
				Type type2 = Type.GetType(key.GetVariable(0).SValue);
				if (type2 == null)
				{
					DebugUtils.Assert(DebugUtils.AssertType.Info);
				}
				else
				{
					global::DiseaseSymptom diseaseSymptom = Activator.CreateInstance(type2) as global::DiseaseSymptom;
					diseaseSymptom.Initialize();
					diseaseSymptom.SetPlayerDiseasesModule(this);
					diseaseSymptom.ParseKey(key);
					this.m_Symptoms.Add((int)diseaseSymptom.GetSymptomType(), diseaseSymptom);
				}
			}
		}
	}

	public bool ScenarioHasSymptom(string symptom_type)
	{
		Enums.DiseaseSymptom item = (Enums.DiseaseSymptom)Enum.Parse(typeof(Enums.DiseaseSymptom), symptom_type);
		using (Dictionary<int, Disease>.ValueCollection.Enumerator enumerator = this.m_Diseases.Values.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_Symptoms.Contains(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public Dictionary<int, Disease> GetAllDiseases()
	{
		return this.m_Diseases;
	}

	public Disease GetDisease(ConsumeEffect type)
	{
		foreach (KeyValuePair<int, Disease> keyValuePair in this.m_Diseases)
		{
			if (keyValuePair.Key == (int)type)
			{
				Dictionary<int, Disease>.Enumerator enumerator;
				keyValuePair = enumerator.Current;
				return keyValuePair.Value;
			}
		}
		return null;
	}

	public Dictionary<int, global::DiseaseSymptom> GetAllSymptoms()
	{
		return this.m_Symptoms;
	}

	public global::DiseaseSymptom GetSymptom(Enums.DiseaseSymptom type)
	{
		global::DiseaseSymptom result = null;
		this.m_Symptoms.TryGetValue((int)type, out result);
		return result;
	}

	public override void Update()
	{
		base.Update();
		this.UpdateRequests();
		this.UpdateSymptoms();
		this.UpdateDiseases();
		this.UpdateDebug();
	}

	private void UpdateDebug()
	{
		bool debug = GreenHellGame.DEBUG;
	}

	private void UpdateRequests()
	{
		int i = 0;
		while (i < this.m_Requests.Count)
		{
			DiseaseRequest diseaseRequest = this.m_Requests[i];
			if (Time.time - diseaseRequest.m_RequestTime >= diseaseRequest.m_Delay)
			{
				if (diseaseRequest.m_Disease != null)
				{
					diseaseRequest.m_Disease.Activate(diseaseRequest);
				}
				this.m_Requests.Remove(diseaseRequest);
			}
			else
			{
				i++;
			}
		}
	}

	private void UpdateSymptoms()
	{
		foreach (KeyValuePair<int, global::DiseaseSymptom> keyValuePair in this.m_Symptoms)
		{
			global::DiseaseSymptom value = keyValuePair.Value;
			if (value.IsActive())
			{
				bool flag = true;
				foreach (KeyValuePair<int, Disease> keyValuePair2 in this.m_Diseases)
				{
					Disease value2 = keyValuePair2.Value;
					if (value2.IsActive() && value2.GetAllSymptoms().Contains(value.m_Type))
					{
						flag = false;
					}
				}
				if (flag)
				{
					value.Deactivate();
				}
			}
		}
		foreach (KeyValuePair<int, global::DiseaseSymptom> keyValuePair in this.m_Symptoms)
		{
			global::DiseaseSymptom value3 = keyValuePair.Value;
			if (value3.IsActive())
			{
				value3.Update();
			}
		}
	}

	private void UpdateDiseases()
	{
		foreach (KeyValuePair<int, Disease> keyValuePair in this.m_Diseases)
		{
			Disease value = keyValuePair.Value;
			if (value.IsActive())
			{
				value.Update();
			}
			else
			{
				value.Check();
			}
		}
	}

	public void RequestDisease(ConsumeEffect type, float delay, int level)
	{
		if (this.m_Diseases[(int)type].IsActive())
		{
			return;
		}
		Disease disease = this.GetDisease(type);
		if (disease == null)
		{
			return;
		}
		DiseaseRequest diseaseRequest = new DiseaseRequest();
		diseaseRequest.m_RequestTime = Time.time;
		diseaseRequest.m_Disease = disease;
		diseaseRequest.m_Delay = delay;
		diseaseRequest.m_Level = level;
		this.m_Requests.Add(diseaseRequest);
		this.UpdateRequests();
	}

	private bool IsRequested(Disease disease)
	{
		if (disease == null)
		{
			return false;
		}
		for (int i = 0; i < this.m_Requests.Count; i++)
		{
			if (this.m_Requests[i].m_Disease == disease)
			{
				return true;
			}
		}
		return false;
	}

	public void OnEat(ConsumableInfo info)
	{
		using (Dictionary<int, Disease>.KeyCollection.Enumerator enumerator = this.m_Diseases.Keys.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ConsumeEffect key = (ConsumeEffect)enumerator.Current;
				Disease disease = this.m_Diseases[(int)key];
				if (disease.IsActive() || this.IsRequested(disease))
				{
					disease.OnEat(info);
				}
			}
		}
		using (Dictionary<int, global::DiseaseSymptom>.KeyCollection.Enumerator enumerator2 = this.m_Symptoms.Keys.GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				Enums.DiseaseSymptom key2 = (Enums.DiseaseSymptom)enumerator2.Current;
				global::DiseaseSymptom diseaseSymptom = this.m_Symptoms[(int)key2];
				if (diseaseSymptom.IsActive())
				{
					diseaseSymptom.OnEat(info);
				}
			}
		}
	}

	public void OnDrink(LiquidType liquid_type, float hydration_amount)
	{
		using (Dictionary<int, Disease>.KeyCollection.Enumerator enumerator = this.m_Diseases.Keys.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ConsumeEffect key = (ConsumeEffect)enumerator.Current;
				Disease disease = this.m_Diseases[(int)key];
				if (disease.IsActive())
				{
					disease.OnDrink(liquid_type);
				}
			}
		}
		using (Dictionary<int, global::DiseaseSymptom>.KeyCollection.Enumerator enumerator2 = this.m_Symptoms.Keys.GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				Enums.DiseaseSymptom key2 = (Enums.DiseaseSymptom)enumerator2.Current;
				global::DiseaseSymptom diseaseSymptom = this.m_Symptoms[(int)key2];
				if (diseaseSymptom.IsActive())
				{
					diseaseSymptom.OnDrink(liquid_type, hydration_amount);
				}
			}
		}
	}

	public bool IsAnyDiseaseActive()
	{
		using (Dictionary<int, Disease>.KeyCollection.Enumerator enumerator = this.m_Diseases.Keys.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ConsumeEffect key = (ConsumeEffect)enumerator.Current;
				if (this.m_Diseases[(int)key].IsActive())
				{
					return true;
				}
			}
		}
		return false;
	}

	public float GetFoodPoisonLevel()
	{
		if (!this.m_Diseases[0].IsActive())
		{
			return 0f;
		}
		float currentTimeMinutes = MainLevel.Instance.GetCurrentTimeMinutes();
		float startTime = this.m_Diseases[0].m_StartTime;
		float num = this.m_Diseases[0].m_AutoHealTime * (float)this.m_Diseases[0].m_Level;
		if (num == 0f)
		{
			return 0f;
		}
		return 1f - Mathf.Clamp01((currentTimeMinutes - startTime) / num);
	}

	public void Load()
	{
		int num = 0;
		this.m_Requests.Clear();
		SaveGame.LoadVal("PDMReqCount", out num, false);
		for (int i = 0; i < num; i++)
		{
			DiseaseRequest diseaseRequest = new DiseaseRequest();
			SaveGame.LoadVal("PDMReq" + i.ToString() + "Delay", out diseaseRequest.m_Delay, false);
			int index = 0;
			SaveGame.LoadVal("PDMReq" + i.ToString() + "DisIndex", out index, false);
			diseaseRequest.m_Disease = this.m_Diseases.ElementAt(index).Value;
			SaveGame.LoadVal("PDMReq" + i.ToString() + "Level", out diseaseRequest.m_Level, false);
			SaveGame.LoadVal("PDMReq" + i.ToString() + "ReqTime", out diseaseRequest.m_RequestTime, false);
			this.m_Requests.Add(diseaseRequest);
		}
		for (int j = 0; j < this.m_Diseases.Count; j++)
		{
			if (j < 4)
			{
				this.m_Diseases.ElementAt(j).Value.Load(j);
			}
			else if (j == 4 && SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate11)
			{
				this.m_Diseases.ElementAt(j).Value.Load(j);
			}
		}
	}

	public void Save()
	{
		SaveGame.SaveVal("PDMReqCount", this.m_Requests.Count);
		for (int i = 0; i < this.m_Requests.Count; i++)
		{
			SaveGame.SaveVal("PDMReq" + i.ToString() + "Delay", this.m_Requests[i].m_Delay);
			int val = 0;
			for (int j = 0; j < this.m_Diseases.Count; j++)
			{
				if (this.m_Diseases.ElementAt(j).Value == this.m_Requests[i].m_Disease)
				{
					val = j;
					break;
				}
			}
			SaveGame.SaveVal("PDMReq" + i.ToString() + "DisIndex", val);
			SaveGame.SaveVal("PDMReq" + i.ToString() + "Level", this.m_Requests[i].m_Level);
			SaveGame.SaveVal("PDMReq" + i.ToString() + "ReqTime", this.m_Requests[i].m_RequestTime);
		}
		for (int k = 0; k < this.m_Diseases.Count; k++)
		{
			this.m_Diseases.ElementAt(k).Value.Save(k);
		}
	}

	public void UnlockKnownSymptom(Enums.DiseaseSymptom symptom_type)
	{
		if (!this.m_KnownSymptoms.Contains(symptom_type))
		{
			this.m_KnownSymptoms.Add(symptom_type);
		}
	}

	public void UnlockKnownSymptomFromScenario(string disease_type_name)
	{
		Enums.DiseaseSymptom symptom_type = (Enums.DiseaseSymptom)Enum.Parse(typeof(Enums.DiseaseSymptom), disease_type_name);
		this.UnlockKnownSymptom(symptom_type);
	}

	public void UnlockKnownSymptomTreatment(NotepadKnownSymptomTreatment symptom_treatment)
	{
		if (!this.m_KnownSymptomTreatments.Contains(symptom_treatment))
		{
			this.m_KnownSymptomTreatments.Add(symptom_treatment);
		}
	}

	public void UnlockKnownSymptomTreatmentFromScenario(string symptom_type_name)
	{
		NotepadKnownSymptomTreatment symptom_treatment = (NotepadKnownSymptomTreatment)Enum.Parse(typeof(NotepadKnownSymptomTreatment), symptom_type_name);
		this.UnlockKnownSymptomTreatment(symptom_treatment);
	}

	public bool IsSymptomUnlocked(Enums.DiseaseSymptom symptom_type)
	{
		return this.m_KnownSymptoms.Contains(symptom_type);
	}

	public bool IsSymptomTreatmentUnlocked(NotepadKnownSymptomTreatment symptom_treatment)
	{
		return this.m_KnownSymptomTreatments.Contains(symptom_treatment);
	}

	public void UnlockKnownDisease(ConsumeEffect disease_type)
	{
		if (!this.m_KnownDiseases.Contains(disease_type))
		{
			this.m_KnownDiseases.Add(disease_type);
			HUDInfoLog hudinfoLog = (HUDInfoLog)HUDManager.Get().GetHUD(typeof(HUDInfoLog));
			string title = GreenHellGame.Instance.GetLocalization().Get("HUD_InfoLog_NewEntry", true);
			string key = string.Empty;
			switch (disease_type)
			{
			case ConsumeEffect.FoodPoisoning:
				key = "Food Poisoning";
				break;
			case ConsumeEffect.Fever:
				key = "Fever";
				break;
			case ConsumeEffect.ParasiteSickness:
				key = "Parasite Sickness";
				break;
			case ConsumeEffect.Insomnia:
				key = "Insomnia";
				break;
			case ConsumeEffect.DirtSickness:
				key = "Dirt Sickness";
				break;
			}
			string text = GreenHellGame.Instance.GetLocalization().Get(key, true);
			hudinfoLog.AddInfo(title, text, HUDInfoLogTextureType.Notepad);
		}
	}

	public void UnlockKnownDiseaseFromScenario(string disease_type_name)
	{
		ConsumeEffect disease_type = (ConsumeEffect)Enum.Parse(typeof(ConsumeEffect), disease_type_name);
		this.UnlockKnownDisease(disease_type);
	}

	public void UnlockKnownDiseaseTreatment(NotepadKnownDiseaseTreatment symptom_treatment)
	{
		if (!this.m_KnownDiseaseTreatments.Contains(symptom_treatment))
		{
			this.m_KnownDiseaseTreatments.Add(symptom_treatment);
		}
	}

	public void UnlockKnownDiseaseTreatmentFromScenario(string symptom_type_name)
	{
		NotepadKnownDiseaseTreatment symptom_treatment = (NotepadKnownDiseaseTreatment)Enum.Parse(typeof(NotepadKnownDiseaseTreatment), symptom_type_name);
		this.UnlockKnownDiseaseTreatment(symptom_treatment);
	}

	public bool IsDiseaseUnlocked(ConsumeEffect disease_type)
	{
		return this.m_KnownDiseases.Contains(disease_type);
	}

	public bool IsDiseaseTreatmentUnlocked(NotepadKnownDiseaseTreatment disease_treatment)
	{
		return this.m_KnownDiseaseTreatments.Contains(disease_treatment);
	}

	public void UnlockAllSymptomsInNotepad()
	{
		Array values = Enum.GetValues(typeof(Enums.DiseaseSymptom));
		for (int i = 0; i < values.Length; i++)
		{
			this.UnlockKnownSymptom((Enums.DiseaseSymptom)values.GetValue(i));
		}
	}

	public void UnlockAllSymptomTreatmentsInNotepad()
	{
		Array values = Enum.GetValues(typeof(NotepadKnownSymptomTreatment));
		for (int i = 0; i < values.Length; i++)
		{
			this.UnlockKnownSymptomTreatment((NotepadKnownSymptomTreatment)values.GetValue(i));
		}
	}

	public void UnlockAllDiseasesInNotepad()
	{
		Array values = Enum.GetValues(typeof(ConsumeEffect));
		for (int i = 0; i < values.Length; i++)
		{
			this.UnlockKnownDisease((ConsumeEffect)values.GetValue(i));
		}
	}

	public void UnlockAllDiseasesTratmentInNotepad()
	{
		Array values = Enum.GetValues(typeof(NotepadKnownDiseaseTreatment));
		for (int i = 0; i < values.Length; i++)
		{
			this.UnlockKnownDiseaseTreatment((NotepadKnownDiseaseTreatment)values.GetValue(i));
		}
	}

	public void HealAllDiseases()
	{
		using (Dictionary<int, Disease>.KeyCollection.Enumerator enumerator = this.m_Diseases.Keys.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ConsumeEffect key = (ConsumeEffect)enumerator.Current;
				Disease disease = this.m_Diseases[(int)key];
				disease.Deactivate();
				disease.m_Level = 0;
				disease.DeactivateSymptoms();
			}
		}
	}

	private Dictionary<int, Disease> m_Diseases = new Dictionary<int, Disease>();

	private Dictionary<int, global::DiseaseSymptom> m_Symptoms = new Dictionary<int, global::DiseaseSymptom>();

	private List<DiseaseRequest> m_Requests = new List<DiseaseRequest>();

	private static PlayerDiseasesModule s_Instance;

	[HideInInspector]
	public List<Enums.DiseaseSymptom> m_KnownSymptoms = new List<Enums.DiseaseSymptom>();

	[HideInInspector]
	public List<NotepadKnownSymptomTreatment> m_KnownSymptomTreatments = new List<NotepadKnownSymptomTreatment>();

	[HideInInspector]
	public List<ConsumeEffect> m_KnownDiseases = new List<ConsumeEffect>();

	[HideInInspector]
	public List<NotepadKnownDiseaseTreatment> m_KnownDiseaseTreatments = new List<NotepadKnownDiseaseTreatment>();
}
