using System;
using Enums;

public class MenuDebugSelectMode : MenuScreen
{
	public void OnStory()
	{
		MainLevel.Instance.m_GameMode = GameMode.Story;
		GreenHellGame.Instance.m_GameMode = GameMode.Story;
		MainLevel.Instance.Initialize();
		MenuInGameManager.Get().HideMenu();
		this.StartRainforestAmbienceMultisample();
	}

	public void OnSurvival()
	{
		ScenarioManager.Get().m_SkipTutorial = true;
		MainLevel.Instance.m_GameMode = GameMode.Survival;
		GreenHellGame.Instance.m_GameMode = GameMode.Survival;
		MainLevel.Instance.Initialize();
		MenuInGameManager.Get().HideMenu();
		this.StartRainforestAmbienceMultisample();
	}

	public void OnTutorial()
	{
		MainLevel.Instance.m_GameMode = GameMode.Survival;
		GreenHellGame.Instance.m_GameMode = GameMode.Survival;
		MainLevel.Instance.Initialize();
		MenuInGameManager.Get().HideMenu();
		this.StartRainforestAmbienceMultisample();
	}

	public void OnDebug()
	{
		Player.Get().UnlockMap();
		Player.Get().UnlockNotepad();
		Player.Get().UnlockWatch();
		ItemsManager.Get().UnlockAllItemsInNotepad();
		PlayerDiseasesModule.Get().UnlockAllDiseasesInNotepad();
		PlayerDiseasesModule.Get().UnlockAllDiseasesTratmentInNotepad();
		PlayerDiseasesModule.Get().UnlockAllSymptomsInNotepad();
		PlayerDiseasesModule.Get().UnlockAllSymptomTreatmentsInNotepad();
		PlayerInjuryModule.Get().UnlockAllInjuryState();
		PlayerInjuryModule.Get().UnlockAllInjuryStateTreatment();
		MainLevel.Instance.m_GameMode = GameMode.Debug;
		GreenHellGame.Instance.m_GameMode = GameMode.None;
		MainLevel.Instance.Initialize();
		MenuInGameManager.Get().HideMenu();
		this.StartRainforestAmbienceMultisample();
	}

	public void OnResetChallenges()
	{
		ChallengesManager.Get().ResetChallenges();
	}

	public void OnFirecampChallenge()
	{
		this.OnChallenge("Firecamp");
	}

	public void OnFirecampHardChallenge()
	{
		this.OnChallenge("FirecampHard");
	}

	public void OnBoatChallenge()
	{
		this.OnChallenge("Boat");
	}

	public void OnCampChallenge()
	{
		this.OnChallenge("Camp");
	}

	public void OnTribeRadioChallenge()
	{
		this.OnChallenge("TribeRadio");
	}

	public void OnTribeRunawayChallenge()
	{
		this.OnChallenge("TribeRunaway");
	}

	public void OnHunterChallenge()
	{
		this.OnChallenge("Hunter");
	}

	public void OnChallenge(string name)
	{
		ChallengesManager.Get().m_ChallengeToActivate = name;
		ScenarioManager.Get().m_SkipTutorial = true;
		MainLevel.Instance.m_GameMode = GameMode.Survival;
		GreenHellGame.Instance.m_GameMode = GameMode.Survival;
		MainLevel.Instance.Initialize();
		MenuInGameManager.Get().HideMenu();
		this.StartRainforestAmbienceMultisample();
	}

	private void StartRainforestAmbienceMultisample()
	{
		MainLevel.Instance.StartRainForestAmbienceMultisample();
	}
}
