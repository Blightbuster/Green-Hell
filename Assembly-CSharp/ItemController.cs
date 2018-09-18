using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class ItemController : PlayerController
{
	public static ItemController Get()
	{
		return ItemController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		ItemController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.Item;
		this.SetupAudio();
	}

	private void SetupAudio()
	{
		this.m_ThrowSoundClipsDict[-1] = new List<AudioClip>();
		AudioClip item = (AudioClip)Resources.Load("Sounds/Weapon/axe_throw_01");
		this.m_ThrowSoundClipsDict[-1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_throw_02");
		this.m_ThrowSoundClipsDict[-1].Add(item);
		this.m_ThrowSoundClipsDict[291] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_throw_01");
		this.m_ThrowSoundClipsDict[291].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_throw_02");
		this.m_ThrowSoundClipsDict[291].Add(item);
		this.m_ThrowSoundClipsDict[288] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/stoneblade_throw_01");
		this.m_ThrowSoundClipsDict[288].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/stoneblade_throw_02");
		this.m_ThrowSoundClipsDict[288].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/stoneblade_throw_03");
		this.m_ThrowSoundClipsDict[288].Add(item);
		this.m_SwingSoundClipsDict[-1] = new List<AudioClip>();
		AudioClip item2 = (AudioClip)Resources.Load("Sounds/Items/stone_swing_01");
		this.m_SwingSoundClipsDict[-1].Add(item2);
		item2 = (AudioClip)Resources.Load("Sounds/Items/stone_swing_02");
		this.m_SwingSoundClipsDict[-1].Add(item2);
		item2 = (AudioClip)Resources.Load("Sounds/Items/stone_swing_03");
		this.m_SwingSoundClipsDict[-1].Add(item2);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Item = Player.Get().GetCurrentItem(Hand.Right);
		DebugUtils.Assert(this.m_Item != null, "[ItemController:OnEnable] Missing current item!", true, DebugUtils.AssertType.Info);
		this.SetAnimatorParameters();
		this.m_StoneThrowing = this.m_Item.m_Info.IsStone();
		if (this.m_StoneThrowing)
		{
			this.SetState(ItemController.State.StoneAim);
		}
	}

	private void SetAnimatorParameters()
	{
		if (this.m_Item.m_Info.m_ID == ItemID.Fire)
		{
			this.m_Animator.SetBool(this.m_FireHash, true);
		}
		else if (!this.m_Item.m_Info.IsWeapon())
		{
			this.m_Animator.SetBool(this.m_ObjectHash, true);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetBool(this.m_ObjectHash, false);
		this.m_Animator.SetBool(this.m_FireHash, false);
		this.m_StoneThrowing = false;
		this.SetState(ItemController.State.None);
		this.m_Item = null;
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateState();
		if (this.m_Player.GetCurrentItem(Hand.Right) == null)
		{
			this.Stop();
		}
	}

	private void UpdateState()
	{
		switch (this.m_State)
		{
		case ItemController.State.None:
			this.SetAnimatorParameters();
			break;
		case ItemController.State.FinishSwing:
			this.SetState(ItemController.State.None);
			break;
		case ItemController.State.Aim:
			if (this.m_Item.m_Info.IsStone())
			{
				if (!InputsManager.Get().IsActionActive(InputsManager.InputAction.ThrowStone))
				{
					this.SetState(ItemController.State.Throw);
				}
			}
			else if (!InputsManager.Get().IsActionActive(InputsManager.InputAction.ItemAim) || TriggerController.Get().IsGrabInProgress() || Inventory3DManager.Get().gameObject.activeSelf)
			{
				this.SetState(ItemController.State.None);
			}
			break;
		case ItemController.State.StoneAim:
			if (this.m_Animator.GetCurrentAnimatorStateInfo(this.m_SpineLayerIndex).shortNameHash == this.m_ObjectAimHash && !this.m_Animator.IsInTransition(this.m_SpineLayerIndex) && !InputsManager.Get().IsActionActive(InputsManager.InputAction.ThrowStone))
			{
				Player.Get().m_StopAimCameraMtx.SetColumn(0, Camera.main.transform.right);
				Player.Get().m_StopAimCameraMtx.SetColumn(1, Camera.main.transform.up);
				Player.Get().m_StopAimCameraMtx.SetColumn(2, Camera.main.transform.forward);
				Player.Get().m_StopAimCameraMtx.SetColumn(3, Camera.main.transform.position);
				Player.Get().m_AimPower = 0f;
				this.SetState(ItemController.State.Throw);
			}
			break;
		}
	}

	public override void OnInputAction(InputsManager.InputAction action)
	{
		switch (action)
		{
		case InputsManager.InputAction.ItemSwing:
			if (this.CanSwing())
			{
				this.SetState(ItemController.State.Swing);
			}
			break;
		case InputsManager.InputAction.ItemAim:
			if (this.CanThrow())
			{
				this.SetState(ItemController.State.Aim);
			}
			break;
		case InputsManager.InputAction.ItemCancelAim:
			if (this.m_State == ItemController.State.Aim || this.m_State == ItemController.State.StoneAim)
			{
				if (this.m_Item.m_Info.IsStone())
				{
					InventoryBackpack.Get().InsertItem(this.m_Item, null, null, true, true, true, true, true);
					Player.Get().SetWantedItem(Hand.Right, null, true);
				}
				this.SetState(ItemController.State.None);
			}
			break;
		case InputsManager.InputAction.ItemThrow:
			if (this.m_State == ItemController.State.Aim)
			{
				this.SetState(ItemController.State.Throw);
			}
			break;
		}
	}

	private void Throw()
	{
		Player.Get().ThrowItem(Hand.Right);
		this.SetState(ItemController.State.None);
	}

	private void SetState(ItemController.State state)
	{
		if (this.m_State == state)
		{
			return;
		}
		this.OnExitState();
		this.m_State = state;
		this.OnEnterState();
	}

	private void OnEnterState()
	{
		this.m_EnterStateTime = Time.time;
		switch (this.m_State)
		{
		case ItemController.State.None:
			this.m_Animator.SetBool(this.m_ObjectAimHash, false);
			this.m_Player.StopAim();
			break;
		case ItemController.State.Swing:
			this.m_Animator.SetTrigger(this.m_ObjectSwingHash);
			this.m_Item.OnStartSwing();
			this.PlaySwingSound();
			break;
		case ItemController.State.FinishSwing:
		{
			this.m_Item.OnStopSwing();
			float num = this.m_Player.GetStaminaDecrease(StaminaDecreaseReason.Swing);
			if (this.m_Item.IsKnife())
			{
				num *= Skill.Get<BladeSkill>().GetStaminaMul();
			}
			else if (this.m_Item.IsAxe())
			{
				num *= Skill.Get<AxeSkill>().GetStaminaMul();
			}
			else if (this.m_Item.IsMachete())
			{
				num *= Skill.Get<MacheteSkill>().GetStaminaMul();
			}
			this.m_Player.DecreaseStamina(num);
			break;
		}
		case ItemController.State.Aim:
			this.m_Animator.SetBool(this.m_ObjectAimHash, true);
			this.m_Player.StartAim(Player.AimType.Item);
			break;
		case ItemController.State.Throw:
			this.m_Animator.SetTrigger(this.m_ObjectThrowHash);
			this.m_Animator.SetBool(this.m_ObjectAimHash, false);
			this.PlayThrowSound();
			break;
		case ItemController.State.StoneAim:
			this.m_Player.GetCurrentItem(Hand.Right).gameObject.SetActive(true);
			this.m_Animator.SetBool(this.m_ObjectAimHash, true);
			this.m_Player.StartAim(Player.AimType.Item);
			break;
		}
	}

	private void OnExitState()
	{
		ItemController.State state = this.m_State;
		if (state == ItemController.State.Aim || state == ItemController.State.StoneAim)
		{
			this.m_Player.StopAim();
		}
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		if (id == AnimEventID.ObjectSwingEnd)
		{
			this.SetState(ItemController.State.FinishSwing);
		}
		else if (id == AnimEventID.ObjectThrow && this.m_State == ItemController.State.Throw)
		{
			this.Throw();
		}
		else if (id == AnimEventID.FireIgnite)
		{
			if (this.m_FirecampToIgnite)
			{
				this.m_FirecampToIgnite.StartBurning();
				Player.Get().SetWantedItem(Hand.Right, null, true);
				UnityEngine.Object.Destroy(this.m_Item.gameObject);
				this.m_FirecampToIgnite = null;
			}
		}
		else
		{
			base.OnAnimEvent(id);
		}
	}

	private bool CanSwing()
	{
		return this.m_Item.GetInfoID() != ItemID.Fire && (this.m_State == ItemController.State.None && !WeaponMeleeController.Get().IsActive()) && !Inventory3DManager.Get().isActiveAndEnabled;
	}

	private bool CanThrow()
	{
		return this.m_State == ItemController.State.None && this.m_Item.GetInfoID() != ItemID.Fire && !Inventory3DManager.Get().gameObject.activeSelf && (!Player.Get().m_ActiveFightController || !Player.Get().m_ActiveFightController.IsBlock()) && (!Player.Get().m_ActiveFightController || !PlayerConditionModule.Get().IsStaminaLevel(Player.Get().m_ActiveFightController.m_BlockAttackStaminaLevel));
	}

	public override void GetInputActions(ref List<int> actions)
	{
		ItemController.State state = this.m_State;
		if (state != ItemController.State.None)
		{
			if (state != ItemController.State.Aim)
			{
				if (state == ItemController.State.StoneAim)
				{
					actions.Add(48);
					actions.Add(4);
				}
			}
			else
			{
				actions.Add(5);
				actions.Add(4);
			}
		}
		else if (this.CanThrow())
		{
			actions.Add(3);
		}
	}

	public void IgniteFirecamp(Firecamp firecamp)
	{
		this.m_Animator.SetTrigger(this.m_TCarriedFireIgnite);
		this.m_FirecampToIgnite = firecamp;
	}

	public override string ReplaceClipsGetItemName()
	{
		if (this.m_Player.GetCurrentItem(Hand.Right) == null)
		{
			return string.Empty;
		}
		if (this.m_Player.GetCurrentItem(Hand.Right).m_Info == null)
		{
			return string.Empty;
		}
		return this.m_Player.GetCurrentItem(Hand.Right).m_Info.m_ID.ToString();
	}

	private void PlayThrowSound()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		ItemInfo info = this.m_Player.GetCurrentItem(Hand.Right).m_Info;
		List<AudioClip> list = null;
		if (this.m_ThrowSoundClipsDict.TryGetValue((int)info.m_ID, out list))
		{
			this.m_AudioSource.PlayOneShot(list[UnityEngine.Random.Range(0, list.Count)]);
		}
		else if (this.m_ThrowSoundClipsDict.TryGetValue(-1, out list))
		{
			this.m_AudioSource.PlayOneShot(list[UnityEngine.Random.Range(0, list.Count)]);
		}
	}

	private void PlaySwingSound()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		ItemInfo info = this.m_Player.GetCurrentItem(Hand.Right).m_Info;
		List<AudioClip> list = null;
		if (this.m_SwingSoundClipsDict.TryGetValue((int)info.m_ID, out list))
		{
			this.m_AudioSource.PlayOneShot(list[UnityEngine.Random.Range(0, list.Count)]);
		}
		else if (this.m_SwingSoundClipsDict.TryGetValue(-1, out list))
		{
			this.m_AudioSource.PlayOneShot(list[UnityEngine.Random.Range(0, list.Count)]);
		}
	}

	public override bool SetupActiveControllerOnStop()
	{
		return false;
	}

	private int m_ObjectHash = Animator.StringToHash("CarriedObject");

	public int m_FireHash = Animator.StringToHash("CarriedFire");

	private int m_ObjectSwingHash = Animator.StringToHash("ObjectSwing");

	private int m_ObjectAimHash = Animator.StringToHash("ObjectAim");

	private int m_ObjectThrowHash = Animator.StringToHash("ObjectThrow");

	private int m_TCarriedFireIgnite = Animator.StringToHash("CarriedFireIgnite");

	private Firecamp m_FirecampToIgnite;

	private AudioSource m_AudioSource;

	private Dictionary<int, List<AudioClip>> m_ThrowSoundClipsDict = new Dictionary<int, List<AudioClip>>();

	private Dictionary<int, List<AudioClip>> m_SwingSoundClipsDict = new Dictionary<int, List<AudioClip>>();

	private ItemController.State m_State;

	private float m_EnterStateTime;

	public float m_ShakePower = 5f;

	private Item m_Item;

	public bool m_StoneThrowing;

	private static ItemController s_Instance;

	private enum State
	{
		None,
		Swing,
		FinishSwing,
		Aim,
		Throw,
		StoneAim
	}
}
