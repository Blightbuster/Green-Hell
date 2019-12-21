using System;
using System.Collections.Generic;
using AIs;
using Enums;
using UnityEngine;

public class Trap : Construction
{
	protected override void Awake()
	{
		base.Awake();
		base.RegisterConstantUpdateItem();
		if (this.IsShrimpTrap())
		{
			return;
		}
		if (this.m_AIIDs.Count == 0)
		{
			DebugUtils.Assert("[Trap:Start] ERROR - AI list in trap " + base.name + " is empty! Trap will be destroyed.", true, DebugUtils.AssertType.Info);
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		TrapAIDummy[] componentsInChildren = base.GetComponentsInChildren<TrapAIDummy>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!this.m_SpecificAIDummies.ContainsKey((int)componentsInChildren[i].m_ID))
			{
				this.m_SpecificAIDummies.Add((int)componentsInChildren[i].m_ID, new List<Transform>());
			}
			this.m_SpecificAIDummies[(int)componentsInChildren[i].m_ID].Add(componentsInChildren[i].transform);
		}
		this.SetArmed(this.m_ArmOnStart);
	}

	protected override void Start()
	{
		base.Start();
		TrapsManager.Get().RegisterTrap(this);
		if (this.m_Info.m_ID == ItemID.Cage_Trap)
		{
			this.m_ArmSoundClips.Add((AudioClip)Resources.Load("Sounds/Traps/cage_trap_arm_01"));
			this.m_ArmSoundClips.Add((AudioClip)Resources.Load("Sounds/Traps/cage_trap_arm_03"));
			this.m_ArmSoundClips.Add((AudioClip)Resources.Load("Sounds/Traps/cage_trap_arm_04"));
			this.m_ArmSoundClips.Add((AudioClip)Resources.Load("Sounds/Traps/cage_trap_arm_05"));
		}
		else if (this.m_Info.m_ID == ItemID.Killer_Trap)
		{
			this.m_ArmSoundClips.Add((AudioClip)Resources.Load("Sounds/Traps/killer_trap_arm_01"));
			this.m_ArmSoundClips.Add((AudioClip)Resources.Load("Sounds/Traps/killer_trap_arm_02"));
			this.m_ArmSoundClips.Add((AudioClip)Resources.Load("Sounds/Traps/killer_trap_arm_04"));
		}
		else if (this.m_Info.m_ID == ItemID.Snare_Trap || this.m_Info.m_ID == ItemID.Fish_Rod_Trap || this.m_Info.m_ID == ItemID.Stick_Fish_Trap)
		{
			this.m_ArmSoundClips.Add((AudioClip)Resources.Load("Sounds/Traps/snare_trap_arm_02"));
			this.m_ArmSoundClips.Add((AudioClip)Resources.Load("Sounds/Traps/snare_trap_arm_03"));
		}
		else if (this.m_Info.m_ID == ItemID.Stone_Trap)
		{
			this.m_ArmSoundClips.Add((AudioClip)Resources.Load("Sounds/Traps/stone_trap_arm_02"));
			this.m_ArmSoundClips.Add((AudioClip)Resources.Load("Sounds/Traps/stone_trap_arm_03"));
		}
		if (this.m_Info.m_ID == ItemID.Stick_Fish_Trap || this.m_Info.m_ID == ItemID.Fish_Rod_Trap || this.m_Info.m_ID == ItemID.Big_Stick_Fish_Trap)
		{
			this.m_FishTrap = true;
		}
		if (this.m_Info.m_ID == ItemID.Stick_Fish_Trap || this.m_Info.m_ID == ItemID.Big_Stick_Fish_Trap)
		{
			Vector3 center = this.m_BoxCollider.bounds.center;
			Vector3 halfExtents = this.m_BoxCollider.size * 0.5f;
			int num = Physics.OverlapBoxNonAlloc(center, halfExtents, Trap.s_ColliderOverlapsTmp, this.m_BoxCollider.transform.rotation);
			for (int i = 0; i < num; i++)
			{
				if (Trap.s_ColliderOverlapsTmp[i].gameObject.IsWater())
				{
					this.m_WaterColl = (BoxCollider)Trap.s_ColliderOverlapsTmp[i];
					return;
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		TrapsManager.Get().UnregisterTrap(this);
		base.UnregisterConstantUpdateItem();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_LastCheck = Time.time;
	}

	protected virtual bool IsShrimpTrap()
	{
		return false;
	}

	protected override void Update()
	{
		base.Update();
	}

	public virtual void UpdateEffect()
	{
		foreach (AI ai in this.m_AIs)
		{
			if (ai && !ai.IsFish())
			{
				int num = 0;
				Trap.Effect effect = this.m_Effect;
				if (effect != Trap.Effect.Kill)
				{
					if (effect == Trap.Effect.Block)
					{
						num = this.m_BlockHash;
					}
				}
				else if (this.m_Info.m_ID == ItemID.Snare_Trap)
				{
					num = this.m_HangHash;
				}
				else
				{
					num = this.m_KillHash;
				}
				if (ai.m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != num)
				{
					ai.m_Animator.CrossFade(num, 0f, 0, 1f);
				}
			}
		}
	}

	public override void ConstantUpdate()
	{
		if (ScenarioManager.Get().IsDreamOrPreDream())
		{
			return;
		}
		int i = 0;
		while (i < this.m_AIs.Count)
		{
			if (this.m_AIs[i] == null)
			{
				this.m_AIs.RemoveAt(i);
			}
			else
			{
				if (!this.m_AIs[i].gameObject.activeSelf && base.gameObject.activeSelf)
				{
					this.m_AIs[i].gameObject.SetActive(true);
				}
				i++;
			}
		}
		this.TryCatch();
	}

	private void TryCatch()
	{
		if (!this.m_Armed)
		{
			return;
		}
		if (this.m_UpperLevel)
		{
			return;
		}
		if (GreenHellGame.ROADSHOW_DEMO)
		{
			if (Vector3.Distance(Player.Get().transform.position, base.transform.position) < 15f)
			{
				return;
			}
		}
		else if (base.gameObject.activeSelf)
		{
			return;
		}
		if (Time.time - this.m_LastCheck < this.m_CheckInterval)
		{
			return;
		}
		float num = (this.m_Bait && this.m_Bait.m_Item) ? this.m_ChanceToCatchWithBait : this.m_ChanceToCatch;
		if (UnityEngine.Random.Range(0f, 1f) <= num)
		{
			this.Catch();
		}
		this.m_LastCheck = Time.time;
	}

	public virtual void Catch()
	{
		if (this.m_AIs.Count > 0)
		{
			return;
		}
		AI.AIID aiid = AI.AIID.None;
		if (!this.m_FishTrap)
		{
			List<AI.AIID> list = new List<AI.AIID>();
			for (int i = 0; i < AIManager.Get().m_Spawners.Count; i++)
			{
				AISpawner aispawner = AIManager.Get().m_Spawners[i];
				if (aispawner.enabled)
				{
					if (!aispawner.m_Bounds.Contains(base.transform.position))
					{
						Vector3 to = aispawner.m_Bounds.ClosestPoint(base.transform.position);
						if (base.transform.position.Distance(to) > this.m_AdditionalDist)
						{
							goto IL_BA;
						}
					}
					if (this.m_AIIDs.Contains(aispawner.m_ID) && DifficultySettings.IsAIIDEnabled(aispawner.m_ID))
					{
						list.Add(aispawner.m_ID);
					}
				}
				IL_BA:;
			}
			if (list.Count > 0)
			{
				aiid = list[UnityEngine.Random.Range(0, list.Count)];
			}
			else
			{
				if (UnityEngine.Random.Range(0f, 1f) < this.m_ChanceToCatchOutsideSpawner)
				{
					aiid = this.m_AIIDs[UnityEngine.Random.Range(0, this.m_AIIDs.Count)];
				}
				if (!DifficultySettings.IsAIIDEnabled(aiid))
				{
					aiid = AI.AIID.None;
				}
			}
		}
		else
		{
			OccurringFishes occurringFishes = this.m_WaterColl ? this.m_WaterColl.GetComponent<OccurringFishes>() : null;
			if (occurringFishes)
			{
				List<AI.AIID> list2 = new List<AI.AIID>();
				foreach (AI.AIID aiid2 in this.m_AIIDs)
				{
					if (occurringFishes.m_IDs.Contains(aiid2) && DifficultySettings.IsAIIDEnabled(aiid2))
					{
						list2.Add(aiid2);
					}
				}
				if (list2.Count > 0)
				{
					aiid = list2[UnityEngine.Random.Range(0, list2.Count)];
				}
				else
				{
					if (UnityEngine.Random.Range(0f, 1f) < this.m_ChanceToCatchOutsideSpawner)
					{
						aiid = this.m_AIIDs[UnityEngine.Random.Range(0, this.m_AIIDs.Count)];
					}
					if (!DifficultySettings.IsAIIDEnabled(aiid))
					{
						aiid = AI.AIID.None;
					}
				}
			}
			else
			{
				aiid = this.m_AIIDs[UnityEngine.Random.Range(0, this.m_AIIDs.Count)];
				if (!DifficultySettings.IsAIIDEnabled(aiid))
				{
					aiid = AI.AIID.None;
				}
			}
		}
		if (aiid == AI.AIID.None)
		{
			if (this.m_Bait && this.m_Bait.m_Item)
			{
				this.m_Bait.DeleteItem();
			}
			this.SetArmed(false);
			this.m_ChanceToCatchOutsideSpawner += this.m_ChanceToCatchOutsideSpawnerChange;
			return;
		}
		if (this.m_SpecificAIDummies.ContainsKey((int)aiid))
		{
			for (int j = 0; j < this.m_SpecificAIDummies[(int)aiid].Count; j++)
			{
				this.Catch(aiid, j);
			}
			return;
		}
		this.Catch(aiid, 0);
	}

	private void Catch(AI.AIID id, int index)
	{
		if (id != AI.AIID.None)
		{
			GameObject prefab = GreenHellGame.Instance.GetPrefab(id.ToString());
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
			gameObject.name = prefab.name;
			AI component = gameObject.GetComponent<AI>();
			component.m_Trap = this;
			Behaviour[] components = gameObject.GetComponents<Behaviour>();
			for (int i = 0; i < components.Length; i++)
			{
				Type type = components[i].GetType();
				if (type != typeof(Transform) && type != typeof(BoxCollider) && type != typeof(Animator) && type != typeof(AI) && !type.IsSubclassOf(typeof(AI)) && type != typeof(SkinnedMeshRenderer) && type != typeof(AnimationEventsReceiver) && type != typeof(GuidComponent) && type != typeof(ReplicationComponent) && type != typeof(Relevance))
				{
					if (components[i] is IReplicatedBehaviour)
					{
						components[i].enabled = false;
					}
					else
					{
						UnityEngine.Object.Destroy(components[i]);
					}
				}
			}
			component.m_BoxCollider.isTrigger = true;
			component.m_Trap = this;
			if ((this.m_Effect == Trap.Effect.Block || this.m_Info.m_ID == ItemID.Snare_Trap) && component.m_SoundModule)
			{
				component.m_SoundModule.RequestSound(AISoundType.Panic);
			}
			if (GreenHellGame.Instance.GetPrefab(component.m_ID.ToString() + "_Body"))
			{
				gameObject.AddComponent<AIInTrapTrigger>().m_AI = component;
			}
			else
			{
				component.AddDeadBodyComponent();
			}
			Transform transform;
			if (this.m_SpecificAIDummies.ContainsKey((int)component.m_ID))
			{
				transform = this.m_SpecificAIDummies[(int)component.m_ID][index];
			}
			else if (this.m_AIDummy)
			{
				transform = this.m_AIDummy;
			}
			else
			{
				transform = base.transform;
			}
			gameObject.transform.position = transform.position;
			gameObject.transform.rotation = transform.rotation;
			if (this.m_WaterColl)
			{
				Vector3 position = gameObject.transform.position;
				position.y = this.m_WaterColl.bounds.max.y - component.m_BoxCollider.size.y * 0.5f;
				gameObject.transform.position = position;
			}
			this.UpdateEffect();
			this.m_AIs.Add(component);
		}
		if (this.m_Bait && this.m_Bait.m_Item)
		{
			this.m_Bait.DeleteItem();
		}
		this.SetArmed(false);
		this.m_ChanceToCatchOutsideSpawner = 0f;
	}

	public override string GetIconName()
	{
		return "HUD_arming_trap";
	}

	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && !this.m_Armed && this.m_AIs.Count == 0;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		actions.Add(TriggerAction.TYPE.Arm);
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		this.PlayArmSound();
		this.SetArmed(true);
		this.m_LastCheck = Time.time;
	}

	protected void SetArmed(bool set)
	{
		if (this.m_AlwaysArmed && !set)
		{
			return;
		}
		this.m_Armed = set;
		if (this.m_ArmedObj)
		{
			this.m_ArmedObj.SetActive(this.m_Armed);
		}
		if (this.m_UnarmedObj)
		{
			this.m_UnarmedObj.SetActive(!this.m_Armed);
		}
		if (this.m_Bait)
		{
			this.m_Bait.gameObject.SetActive(this.m_Armed);
		}
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("TrapArmed" + index, this.m_Armed);
		SaveGame.SaveVal("TrapCount" + index, this.m_AIs.Count);
		for (int i = 0; i < this.m_AIs.Count; i++)
		{
			SaveGame.SaveVal(string.Concat(new object[]
			{
				"TrapAI",
				index,
				"Count",
				i
			}), (int)this.m_AIs[i].m_ID);
		}
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.SetArmed(SaveGame.LoadBVal("TrapArmed" + index));
		int num = SaveGame.LoadIVal("TrapCount" + index);
		for (int i = 0; i < num; i++)
		{
			AI.AIID aiid = (AI.AIID)SaveGame.LoadIVal(string.Concat(new object[]
			{
				"TrapAI",
				index,
				"Count",
				i
			}));
			if (aiid != AI.AIID.None)
			{
				this.Catch(aiid, i);
			}
		}
	}

	public override bool TriggerThrough()
	{
		return (this.m_Bait && this.m_Bait.m_Item) || (this.m_AIs.Count > 0 && (this.m_AlwaysArmed || !this.m_Armed));
	}

	private void PlayArmSound()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
		}
		this.m_AudioSource.PlayOneShot(this.m_ArmSoundClips[UnityEngine.Random.Range(0, this.m_ArmSoundClips.Count - 1)]);
	}

	public override void DestroyMe(bool check_connected = true)
	{
		if (this.m_Info.m_ID == ItemID.Snare_Trap)
		{
			foreach (AI ai in this.m_AIs)
			{
				DeadBody component = ai.GetComponent<DeadBody>();
				if (component)
				{
					component.RemoveFromSnareTrap();
				}
			}
		}
		base.DestroyMe(check_connected);
	}

	public Trap.Effect m_Effect;

	public List<AI.AIID> m_AIIDs = new List<AI.AIID>();

	public bool m_ArmOnStart = true;

	private bool m_Armed;

	public bool m_AlwaysArmed;

	private List<AI> m_AIs = new List<AI>();

	public float m_CheckInterval = 10f;

	private float m_LastCheck;

	public float m_ChanceToCatch = 1f;

	public float m_ChanceToCatchWithBait = 1f;

	public GameObject m_ArmedObj;

	public GameObject m_UnarmedObj;

	public ItemSlot m_Bait;

	public Transform m_AIDummy;

	private Dictionary<int, List<Transform>> m_SpecificAIDummies = new Dictionary<int, List<Transform>>();

	private int m_BlockHash = Animator.StringToHash("TrapBlock");

	private int m_HangHash = Animator.StringToHash("TrapHang");

	private int m_KillHash = Animator.StringToHash("TrapKill");

	private float m_AdditionalDist = 20f;

	private bool m_FishTrap;

	private AudioSource m_AudioSource;

	private List<AudioClip> m_ArmSoundClips = new List<AudioClip>();

	private float m_ChanceToCatchOutsideSpawner;

	private float m_ChanceToCatchOutsideSpawnerChange = 0.1f;

	private BoxCollider m_WaterColl;

	private static Collider[] s_ColliderOverlapsTmp = new Collider[20];

	public enum Effect
	{
		Kill,
		Block
	}
}
