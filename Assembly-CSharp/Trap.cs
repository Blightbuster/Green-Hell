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
		if (this.m_AIIDs.Count == 0)
		{
			DebugUtils.Assert("[Trap:Start] ERROR - AI list in trap " + base.name + " is empty! Trap will be destroyed.", true, DebugUtils.AssertType.Info);
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		TrapAIDummy[] componentsInChildren = base.GetComponentsInChildren<TrapAIDummy>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			this.m_SpecificAIDummies.Add((int)componentsInChildren[i].m_ID, componentsInChildren[i].transform);
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
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		TrapsManager.Get().UnregisterTrap(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_LastCheck = Time.time;
	}

	protected override void Update()
	{
		base.Update();
	}

	public void UpdateEffect()
	{
		if (this.m_AI && !this.m_AI.IsFish())
		{
			int num = 0;
			Trap.Effect effect = this.m_Effect;
			if (effect != Trap.Effect.Block)
			{
				if (effect == Trap.Effect.Kill)
				{
					if (this.m_Info.m_ID == ItemID.Snare_Trap)
					{
						num = this.m_HangHash;
					}
					else
					{
						num = this.m_KillHash;
					}
				}
			}
			else
			{
				num = this.m_BlockHash;
			}
			if (this.m_AI.m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != num)
			{
				this.m_AI.m_Animator.CrossFade(num, 0f, 0, 1f);
			}
		}
	}

	public void ConstantUpdate()
	{
		this.TryCatch();
	}

	private void TryCatch()
	{
		if (!this.m_Armed)
		{
			return;
		}
		if (GreenHellGame.ROADSHOW_DEMO)
		{
			float num = Vector3.Distance(Player.Get().transform.position, base.transform.position);
			if (num < 15f)
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
		float num2 = (!this.m_Bait || !this.m_Bait.m_Item) ? this.m_ChanceToCatch : this.m_ChanceToCatchWithBait;
		if (UnityEngine.Random.Range(0f, 1f) <= num2)
		{
			this.Catch();
		}
		this.m_LastCheck = Time.time;
	}

	public void Catch()
	{
		if (this.m_AI)
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
							goto IL_C5;
						}
					}
					if (this.m_AIIDs.Contains(aispawner.m_ID))
					{
						list.Add(aispawner.m_ID);
					}
				}
				IL_C5:;
			}
			if (list.Count > 0)
			{
				aiid = list[UnityEngine.Random.Range(0, list.Count)];
			}
			else if (UnityEngine.Random.Range(0f, 1f) < this.m_ChanceToCatchOutsideSpawner)
			{
				aiid = this.m_AIIDs[UnityEngine.Random.Range(0, this.m_AIIDs.Count)];
			}
		}
		else
		{
			aiid = this.m_AIIDs[UnityEngine.Random.Range(0, this.m_AIIDs.Count)];
		}
		if (aiid != AI.AIID.None)
		{
			this.Catch(aiid);
		}
		else
		{
			if (this.m_Bait && this.m_Bait.m_Item)
			{
				this.m_Bait.DeleteItem();
			}
			this.SetArmed(false);
			this.m_ChanceToCatchOutsideSpawner += this.m_ChanceToCatchOutsideSpawnerChange;
		}
	}

	private void Catch(AI.AIID id)
	{
		if (id != AI.AIID.None)
		{
			GameObject prefab = GreenHellGame.Instance.GetPrefab(id.ToString());
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
			gameObject.name = prefab.name;
			this.m_AI = gameObject.GetComponent<AI>();
			this.m_AI.m_Trap = this;
			Component[] components = gameObject.GetComponents(typeof(Component));
			for (int i = 0; i < components.Length; i++)
			{
				Type type = components[i].GetType();
				if (type != typeof(Transform) && type != typeof(BoxCollider) && type != typeof(Animator) && type != typeof(AI) && !type.IsSubclassOf(typeof(AI)) && type != typeof(SkinnedMeshRenderer) && type != typeof(AnimationEventsReceiver))
				{
					UnityEngine.Object.Destroy(components[i]);
				}
			}
			this.m_AI.m_BoxCollider.isTrigger = true;
			this.m_AI.m_Trap = this;
			if (GreenHellGame.Instance.GetPrefab(this.m_AI.m_ID.ToString() + "_Body"))
			{
				AIInTrapTrigger aiinTrapTrigger = gameObject.AddComponent<AIInTrapTrigger>();
				aiinTrapTrigger.m_AI = this.m_AI;
			}
			else
			{
				this.m_AI.AddDeadBodyComponent();
			}
			Transform transform;
			if (this.m_SpecificAIDummies.ContainsKey((int)this.m_AI.m_ID))
			{
				transform = this.m_SpecificAIDummies[(int)this.m_AI.m_ID];
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
			this.UpdateEffect();
		}
		if (this.m_Bait && this.m_Bait.m_Item)
		{
			this.m_Bait.DeleteItem();
		}
		this.SetArmed(false);
		this.m_ChanceToCatchOutsideSpawner = 0f;
	}

	public override bool CheckDot()
	{
		return false;
	}

	public override string GetIconName()
	{
		return "HUD_arming_trap";
	}

	public override bool CanTrigger()
	{
		return !this.m_Armed && !this.m_AI;
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

	private void SetArmed(bool set)
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
		SaveGame.SaveVal("TrapAI" + index, (int)((!this.m_AI) ? AI.AIID.None : this.m_AI.m_ID));
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.SetArmed(SaveGame.LoadBVal("TrapArmed" + index));
		AI.AIID aiid = (AI.AIID)SaveGame.LoadIVal("TrapAI" + index);
		if (aiid != AI.AIID.None)
		{
			this.Catch(aiid);
		}
	}

	public override bool TriggerThrough()
	{
		return this.m_AI && (this.m_AlwaysArmed || !this.m_Armed);
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

	public Trap.Effect m_Effect;

	public List<AI.AIID> m_AIIDs = new List<AI.AIID>();

	public bool m_ArmOnStart = true;

	private bool m_Armed;

	public bool m_AlwaysArmed;

	private AI m_AI;

	public float m_CheckInterval = 10f;

	private float m_LastCheck;

	public float m_ChanceToCatch = 1f;

	public float m_ChanceToCatchWithBait = 1f;

	public GameObject m_ArmedObj;

	public GameObject m_UnarmedObj;

	public ItemSlot m_Bait;

	public Transform m_AIDummy;

	private Dictionary<int, Transform> m_SpecificAIDummies = new Dictionary<int, Transform>();

	private int m_BlockHash = Animator.StringToHash("TrapBlock");

	private int m_HangHash = Animator.StringToHash("TrapHang");

	private int m_KillHash = Animator.StringToHash("TrapKill");

	private float m_AdditionalDist = 20f;

	private bool m_FishTrap;

	private AudioSource m_AudioSource;

	private List<AudioClip> m_ArmSoundClips = new List<AudioClip>();

	private float m_ChanceToCatchOutsideSpawner;

	private float m_ChanceToCatchOutsideSpawnerChange = 0.1f;

	public enum Effect
	{
		Kill,
		Block
	}
}
