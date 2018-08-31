using System;
using Enums;
using UnityEngine;
using UnityEngine.Playables;

public class TwitchDemoManager
{
	public TwitchDemoManager()
	{
		TwitchDemoManager.s_Instance = this;
		this.m_StartTime = Time.time;
	}

	public static TwitchDemoManager Get()
	{
		return TwitchDemoManager.s_Instance;
	}

	public void Destroy()
	{
		TwitchDemoManager.s_Instance = null;
	}

	public void Update()
	{
		if (!this.m_EndDemo && Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.G))
		{
			PlayableDirector cutscene = CutscenesManager.Get().GetCutscene("DemoEnd_Cutscene");
			cutscene.transform.rotation = Player.Get().transform.rotation;
			cutscene.transform.position = Player.Get().transform.position;
			Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
			if (currentItem != null)
			{
				Player.Get().SetWantedItem(Hand.Right, null, true);
				InventoryBackpack.Get().InsertItem(currentItem, null, null, true, true, true, true, true);
			}
			currentItem = Player.Get().GetCurrentItem(Hand.Left);
			if (currentItem != null)
			{
				Player.Get().SetWantedItem(Hand.Left, null, true);
				InventoryBackpack.Get().InsertItem(currentItem, null, null, true, true, true, true, true);
			}
			Player.Get().BlockMoves();
			Player.Get().BlockRotation();
			PlayableDirector cutscene2 = CutscenesManager.Get().GetCutscene("DemoEnd_Cutscene");
			this.m_CutsceneDuration = (float)cutscene2.duration - 4.5f;
			CutscenesManager.Get().PlayCutscene("DemoEnd_Cutscene");
			this.m_StartCutsceneTime = Time.time;
			this.m_EndDemo = true;
			Player.Get().StartController(PlayerControllerType.PlayerCutscene);
		}
		if (this.m_EndDemo)
		{
			if (!this.m_FirstHit && Time.time - this.m_StartCutsceneTime >= this.m_FirstHitTime)
			{
				PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Blood, 1f);
				PlayerConditionModule.Get().DecreaseEnergy(PlayerConditionModule.Get().GetEnergy() * 0.5f);
				this.m_FirstHit = true;
			}
			if (!this.m_SecondHit && Time.time - this.m_StartCutsceneTime >= this.m_SecondHitTime)
			{
				PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Blood, 1f);
				HUDManager.Get().SetActiveGroup(HUDManager.HUDGroup.TwitchDemo);
				HUDEndDemo.Get().gameObject.SetActive(false);
				this.m_SecondHit = true;
			}
			if (this.m_SecondHit)
			{
				PostProcessManager.Get().SetWeight(PostProcessManager.Effect.Blood, 1f);
			}
			if (Time.time - this.m_StartCutsceneTime >= this.m_CutsceneDuration)
			{
				HUDManager.Get().ShowDemoEnd();
				this.m_EndDemo = true;
			}
		}
	}

	private static TwitchDemoManager s_Instance;

	public float m_StartTime;

	public float m_Duration = 1200f;

	public bool m_EndDemo;

	private float m_StartCutsceneTime;

	private float m_CutsceneDuration;

	private bool m_FirstHit;

	private float m_FirstHitTime = 1.68f;

	private bool m_SecondHit;

	private float m_SecondHitTime = 5.63f;
}
