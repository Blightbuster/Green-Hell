using System;
using System.Collections.Generic;
using AIs;
using CJTools;
using Enums;
using UnityEngine;

public class WeaponController : FightController
{
	protected override void Awake()
	{
		base.Awake();
		this.SetupAudio();
		this.SetupHitSpecificSounds();
	}

	private void SetupAudio()
	{
		this.m_AudioModule = base.gameObject.GetComponent<PlayerAudioModule>();
		DebugUtils.Assert(this.m_AudioModule, true);
		for (int i = 0; i < 12; i++)
		{
			this.m_AudioClipsHit[i] = new List<AudioClip>();
			for (int j = 1; j < 10; j++)
			{
				string str = string.Empty;
				switch (i)
				{
				case 0:
					str = "axe_unknown_0" + j.ToString();
					break;
				case 1:
					str = "axe_wood_0" + j.ToString();
					break;
				case 2:
					str = "axe_bush_0" + j.ToString();
					break;
				case 3:
					str = "axe_stone_0" + j.ToString();
					break;
				case 8:
					str = "axe_flesh_0" + j.ToString();
					break;
				}
				AudioClip audioClip = (AudioClip)Resources.Load("Sounds/Hit/" + str);
				if (!(audioClip != null))
				{
					break;
				}
				this.m_AudioClipsHit[i].Add(audioClip);
			}
		}
		this.m_SwingSoundClipsDict[-1] = new List<AudioClip>();
		AudioClip item = (AudioClip)Resources.Load("Sounds/Weapon/axe_swing_01");
		this.m_SwingSoundClipsDict[-1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_swing_02");
		this.m_SwingSoundClipsDict[-1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_swing_03");
		this.m_SwingSoundClipsDict[-1].Add(item);
		this.m_SwingSoundClipsDict[291] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_swing_01");
		this.m_SwingSoundClipsDict[291].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_swing_02");
		this.m_SwingSoundClipsDict[291].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_swing_03");
		this.m_SwingSoundClipsDict[291].Add(item);
		this.m_SwingSoundClipsDict[312] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_whoosh_01");
		this.m_SwingSoundClipsDict[312].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_whoosh_02");
		this.m_SwingSoundClipsDict[312].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_whoosh_03");
		this.m_SwingSoundClipsDict[312].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_whoosh_04");
		this.m_SwingSoundClipsDict[312].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_whoosh_05");
		this.m_SwingSoundClipsDict[312].Add(item);
		this.m_SwingSoundClipsDict[288] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/stoneblade_swing_01");
		this.m_SwingSoundClipsDict[288].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/stoneblade_swing_02");
		this.m_SwingSoundClipsDict[288].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/stoneblade_swing_03");
		this.m_SwingSoundClipsDict[288].Add(item);
		this.m_SwingSoundClipsDict[308] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_attack_swing_01");
		this.m_SwingSoundClipsDict[308].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_attack_swing_02");
		this.m_SwingSoundClipsDict[308].Add(item);
		this.m_SwingSoundClipsDict[303] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_attack_swing_01");
		this.m_SwingSoundClipsDict[303].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_attack_swing_02");
		this.m_SwingSoundClipsDict[303].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_attack_swing_03");
		this.m_SwingSoundClipsDict[303].Add(item);
		this.m_SwingSoundClipsDict[313] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_whoosh_01");
		this.m_SwingSoundClipsDict[313].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_whoosh_01");
		this.m_SwingSoundClipsDict[313].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_whoosh_01");
		this.m_SwingSoundClipsDict[313].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_whoosh_01");
		this.m_SwingSoundClipsDict[313].Add(item);
	}

	private void SetupHitSpecificSounds()
	{
		this.m_HitSpecificSounds[312] = new Dictionary<int, List<AudioClip>>();
		this.m_HitSpecificSounds[312][1] = new List<AudioClip>();
		AudioClip item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_hit_01");
		this.m_HitSpecificSounds[312][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_hit_02");
		this.m_HitSpecificSounds[312][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_hit_03");
		this.m_HitSpecificSounds[312][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_hit_04");
		this.m_HitSpecificSounds[312][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_hit_05");
		this.m_HitSpecificSounds[312][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/axe_professional_tree_hit_06");
		this.m_HitSpecificSounds[312][1].Add(item);
		this.m_HitSpecificSounds[313] = new Dictionary<int, List<AudioClip>>();
		this.m_HitSpecificSounds[313][2] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_branch_hit_01");
		this.m_HitSpecificSounds[313][2].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_branch_hit_02");
		this.m_HitSpecificSounds[313][2].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_branch_hit_03");
		this.m_HitSpecificSounds[313][2].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_branch_hit_04");
		this.m_HitSpecificSounds[313][2].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_branch_hit_05");
		this.m_HitSpecificSounds[313][2].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_branch_hit_06");
		this.m_HitSpecificSounds[313][2].Add(item);
		this.m_HitSpecificSounds[313][1] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_tree_hit_01");
		this.m_HitSpecificSounds[313][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_tree_hit_02");
		this.m_HitSpecificSounds[313][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_tree_hit_03");
		this.m_HitSpecificSounds[313][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_tree_hit_04");
		this.m_HitSpecificSounds[313][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_tree_hit_05");
		this.m_HitSpecificSounds[313][1].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/machete_tree_hit_06");
		this.m_HitSpecificSounds[313][1].Add(item);
		this.m_HitSpecificSounds[308] = new Dictionary<int, List<AudioClip>>();
		this.m_HitSpecificSounds[308][8] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_flesh_hit_01");
		this.m_HitSpecificSounds[308][8].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_flesh_hit_02");
		this.m_HitSpecificSounds[308][8].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_flesh_hit_03");
		this.m_HitSpecificSounds[308][8].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_flesh_hit_04");
		this.m_HitSpecificSounds[308][8].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/spear_flesh_hit_05");
		this.m_HitSpecificSounds[308][8].Add(item);
		this.m_HitSpecificSounds[307] = this.m_HitSpecificSounds[308];
		this.m_HitSpecificSounds[306] = this.m_HitSpecificSounds[308];
		this.m_HitSpecificSounds[305] = this.m_HitSpecificSounds[308];
		this.m_HitSpecificSounds[303] = this.m_HitSpecificSounds[308];
		this.m_HitSpecificSounds[327] = this.m_HitSpecificSounds[308];
	}

	private AudioClip GetSpecificSound(ItemID item_id, EObjectMaterial material)
	{
		Dictionary<int, List<AudioClip>> dictionary;
		if (!this.m_HitSpecificSounds.TryGetValue((int)item_id, out dictionary))
		{
			return null;
		}
		List<AudioClip> list;
		if (!dictionary.TryGetValue((int)material, out list))
		{
			return null;
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		DebugUtils.Assert(currentItem && currentItem.m_Info.IsWeapon(), true);
		if (!Inventory3DManager.Get().gameObject.activeSelf)
		{
			currentItem.gameObject.SetActive(false);
		}
		this.m_WasActivated = false;
		DebugUtils.Assert(currentItem.m_DamagerStart, true);
		DebugUtils.Assert(currentItem.m_DamagerEnd, true);
		WeaponType weaponType = ((Weapon)currentItem).GetWeaponType();
		this.m_Animator.SetInteger(this.m_IWeaponType, (int)weaponType);
		this.SetState(WeaponControllerState.None);
		this.m_AlreadyHit = false;
		this.m_HitObjects.Clear();
		this.m_HandleEndTransform = this.m_Player.transform.FindDeepChild("mixamorig:Arm.R");
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetInteger(this.m_IWeaponType, 0);
		this.SetState(WeaponControllerState.None);
		if (this.m_MovesBlocked)
		{
			this.m_Player.UnblockMoves();
			this.m_Player.UnblockRotation();
			this.m_MovesBlocked = false;
		}
		this.m_Animator.SetBool(this.m_BRemoveWeaponFromObstacle, false);
	}

	protected bool CanAttack()
	{
		return !MainLevel.Instance.IsPause() && !this.m_Player.GetRotationBlocked() && !Inventory3DManager.Get().gameObject.activeSelf && !HitReactionController.Get().IsActive() && !base.IsBlock() && !this.IsAttack() && !this.m_Player.m_Aim && !PlayerConditionModule.Get().IsStaminaLevel(this.m_BlockAttackStaminaLevel);
	}

	protected virtual void EndAttack()
	{
		this.m_DamagerLastPosEnd = Vector3.zero;
		this.m_DamagerLastPosStart = Vector3.zero;
		this.m_HandleLastPosEnd = Vector3.zero;
		this.SetState(WeaponControllerState.Return);
	}

	protected virtual void EndAttackNonStop()
	{
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		if (currentItem == null)
		{
			return;
		}
		if (!currentItem.m_Info.IsWeapon())
		{
			return;
		}
		WeaponType weaponType = ((Weapon)currentItem).GetWeaponType();
		this.m_Animator.SetInteger(this.m_IWeaponType, (int)weaponType);
		if (this.m_AnimationStopped)
		{
			this.UpdateStoppedAnimation();
		}
		if (!this.m_WasActivated && !currentItem.gameObject.activeSelf)
		{
			int shortNameHash = this.m_Animator.GetCurrentAnimatorStateInfo(this.m_SpineLayerIndex).shortNameHash;
			if (shortNameHash == this.m_MeleeIdleHash || shortNameHash == this.m_TakeOutItemHash || shortNameHash == this.m_SpearIdleHash)
			{
				currentItem.gameObject.SetActive(true);
				this.m_WasActivated = true;
			}
		}
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		if (MainLevel.Instance.IsPause())
		{
			return;
		}
		if (!this.m_AnimationStopped)
		{
			this.UpdateAttack();
		}
		else
		{
			this.UpdateStoppedAnimationPose();
		}
	}

	private void UpdateAttack()
	{
		if (!this.IsAttack())
		{
			this.m_DamagerLastPosEnd = Vector3.zero;
			this.m_DamagerLastPosStart = Vector3.zero;
			this.m_HandleLastPosEnd = Vector3.zero;
			this.m_AlreadyHit = false;
			this.m_HitObjects.Clear();
			return;
		}
		if (this.m_AlreadyHit)
		{
			return;
		}
		if (this.m_State != WeaponControllerState.Swing)
		{
			return;
		}
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		if (!currentItem)
		{
			return;
		}
		if (!currentItem.m_DamagerStart || !currentItem.m_DamagerEnd)
		{
			return;
		}
		Vector3 position = currentItem.m_DamagerStart.transform.position;
		Vector3 position2 = currentItem.m_DamagerEnd.transform.position;
		Vector3 position3 = this.m_HandleEndTransform.position;
		Vector3 normalized = (position - this.m_DamagerLastPosStart).normalized;
		if (this.m_DamagerLastPosEnd.magnitude < 0.1f)
		{
			this.m_DamagerLastPosEnd = Vector3.Cross(position2 - position, Vector3.up) * 0.1f;
			this.m_DamagerLastPosEnd += position2;
		}
		if (this.m_DamagerLastPosStart.magnitude < 0.1f)
		{
			this.m_DamagerLastPosStart = Vector3.Cross(position2 - position, Vector3.up) * 0.1f;
			this.m_DamagerLastPosStart += position;
		}
		if (this.m_HandleLastPosEnd.magnitude < 0.1f)
		{
			this.m_HandleLastPosEnd = Vector3.Cross(position3 - position2, Vector3.up) * 0.1f;
			this.m_HandleLastPosEnd += this.m_HandleEndTransform.position;
		}
		this.m_BladeT0.p0 = position;
		this.m_BladeT0.p1 = this.m_DamagerLastPosStart;
		this.m_BladeT0.p2 = position2;
		this.m_BladeT1.p0 = this.m_DamagerLastPosStart;
		this.m_BladeT1.p1 = this.m_DamagerLastPosEnd;
		this.m_BladeT1.p2 = position2;
		this.m_DamagerLastPosStart = position;
		this.m_DamagerLastPosEnd = position2;
		this.m_HandleLastPosEnd = position3;
		AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(1);
		AnimatorClipInfo[] currentAnimatorClipInfo = this.m_Animator.GetCurrentAnimatorClipInfo(1);
		bool damage_window = true;
		float num = float.MinValue;
		int num2 = -1;
		for (int i = 0; i < currentAnimatorClipInfo.Length; i++)
		{
			AnimatorClipInfo animatorClipInfo = currentAnimatorClipInfo[i];
			if (animatorClipInfo.weight > num)
			{
				num2 = i;
				num = animatorClipInfo.weight;
			}
		}
		float num3 = -1f;
		float num4 = -1f;
		if (num2 >= 0)
		{
			AnimatorClipInfo animatorClipInfo2 = currentAnimatorClipInfo[num2];
			if (animatorClipInfo2.clip.events.Length > 0)
			{
				for (int j = 0; j < animatorClipInfo2.clip.events.Length; j++)
				{
					AnimationEvent animationEvent = animatorClipInfo2.clip.events[j];
					if (animationEvent.intParameter == 9)
					{
						num3 = animationEvent.time / animatorClipInfo2.clip.length;
					}
					else if (animationEvent.intParameter == 10)
					{
						num4 = animationEvent.time / animatorClipInfo2.clip.length;
					}
				}
				if (num3 < 0f && num4 >= 0f)
				{
					DebugUtils.Assert("Missing DamageStart event in anim " + animatorClipInfo2.clip.name, true, DebugUtils.AssertType.Info);
				}
				else if (num3 >= 0f && num4 < 0f)
				{
					DebugUtils.Assert("Missing DamageEnd event in anim " + animatorClipInfo2.clip.name, true, DebugUtils.AssertType.Info);
				}
				damage_window = (num3 >= 0f && num4 >= 0f && (num3 <= currentAnimatorStateInfo.normalizedTime && num4 >= num3 && num4 >= currentAnimatorStateInfo.normalizedTime));
			}
		}
		CJObject cjobject = null;
		Collider collider = null;
		bool flag = false;
		Vector3 zero = Vector3.zero;
		if (this.UpdateCollisions(this.m_BladeT0, this.m_BladeT1, null, null, normalized, damage_window, out cjobject, out collider, out flag, num3, num4, out zero) && !this.m_HitObjects.Contains(collider.gameObject))
		{
			this.m_HitObjects.Add(collider.gameObject);
			if (flag)
			{
				this.EndAttack();
			}
			else
			{
				this.Hit(cjobject, collider, zero, normalized);
				if (this.StopAnimOnHit(cjobject, collider))
				{
					this.StopAnimation();
				}
				else
				{
					this.EndAttackNonStop();
				}
			}
			this.m_LastHitTime = Time.time;
		}
	}

	private void Hit(CJObject cj_object, Collider coll, Vector3 hit_pos, Vector3 hit_dir)
	{
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		if (cj_object)
		{
			HumanAI component = cj_object.GetComponent<HumanAI>();
			if (component)
			{
				this.HitHumanAI(component, hit_pos, hit_dir);
				this.MakeHitSound(coll.gameObject, currentItem.m_Info.m_ID);
				this.m_AlreadyHit = true;
				return;
			}
			this.HitObject(cj_object, hit_pos, hit_dir);
		}
		else
		{
			RagdollBone component2 = coll.gameObject.GetComponent<RagdollBone>();
			if (component2 && component2.m_Parent)
			{
				DeadBody component3 = component2.m_Parent.GetComponent<DeadBody>();
				if (component3)
				{
					component3.OnTakeDamage(new DamageInfo
					{
						m_DamageItem = currentItem,
						m_Damager = base.gameObject,
						m_Position = hit_pos,
						m_HitDir = hit_dir,
						m_Normal = -hit_dir
					});
					return;
				}
			}
		}
		DamageInfo damageInfo = new DamageInfo();
		ObjectMaterial component4;
		if (cj_object != null)
		{
			component4 = cj_object.gameObject.GetComponent<ObjectMaterial>();
		}
		else
		{
			component4 = coll.gameObject.GetComponent<ObjectMaterial>();
		}
		EObjectMaterial mat = (!(component4 == null)) ? component4.m_ObjectMaterial : EObjectMaterial.Unknown;
		damageInfo.m_Damage = currentItem.m_Info.m_DamageSelf * ObjectMaterial.GetDamageSelfMul(mat);
		damageInfo.m_DamageItem = currentItem;
		this.MakeHitSound(coll.gameObject, currentItem.m_Info.m_ID);
		currentItem.TakeDamage(damageInfo);
		this.OnHit();
	}

	protected virtual void OnHit()
	{
	}

	protected void GetBoxParameters(Triangle t0, Triangle t1, out Vector3 box_center, out Vector3 box_half_sizes, out Quaternion q)
	{
		Vector3 a = t0.p0 - t0.p2;
		Vector3 a2 = -Vector3.Cross((t0.p1 - t0.p0).normalized, a.normalized) * 0.1f;
		Vector3 b = CJTools.Math.ProjectPointOnSegment(t0.p0, t0.p2, t0.p1);
		Vector3 a3 = t0.p1 - b;
		Matrix4x4 identity = Matrix4x4.identity;
		identity.SetColumn(0, a3.normalized);
		identity.SetColumn(1, a2.normalized);
		identity.SetColumn(2, a.normalized);
		box_center = t0.p2 + a * 0.5f + a2 * 0.5f + a3 * 0.5f;
		box_half_sizes = new Vector3(a3.magnitude, a2.magnitude, a.magnitude) * 0.5f;
		q = CJTools.Math.QuaternionFromMatrix(identity);
	}

	protected virtual bool UpdateCollisions(Triangle blade_t0, Triangle blade_t1, Triangle handle_t0, Triangle handle_t1, Vector3 hit_dir, bool damage_window, out CJObject cj_obj, out Collider collider, out bool collision_with_handle, float damage_window_start, float damage_window_end, out Vector3 hit_pos)
	{
		collision_with_handle = false;
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Quaternion identity = Quaternion.identity;
		hit_pos = blade_t0.p0;
		this.GetBoxParameters(blade_t0, blade_t1, out zero2, out zero, out identity);
		if (!damage_window)
		{
			this.m_LastBoxHalfSizes = zero;
			this.m_LastBoxPos = zero2;
			this.m_LastBoxQuaternion = identity;
			this.m_LastAnimFrame = this.m_Animator.GetCurrentAnimatorStateInfo(1).length * this.m_Animator.GetCurrentAnimatorStateInfo(1).normalizedTime;
			cj_obj = null;
			collider = null;
			hit_pos = Vector3.zero;
			return false;
		}
		Collider[] array = Physics.OverlapBox(zero2, zero, identity);
		for (int i = 0; i < array.Length; i++)
		{
			bool flag = false;
			Collider collider2 = array[i];
			if (collider2.gameObject.IsWater())
			{
				this.OnHitWater(collider2);
			}
			if (!collider2.isTrigger || collider2.gameObject.IsAI())
			{
				if (!(collider2.gameObject == this.m_Player.gameObject) && !(collider2.gameObject == this.m_Player.GetCurrentItem(Hand.Right).gameObject))
				{
					if (!(collider2.gameObject.GetComponent<Player>() != null))
					{
						CJObject component = collider2.gameObject.GetComponent<CJObject>();
						if (component == null && collider2.gameObject.transform.parent != null)
						{
							component = collider2.gameObject.transform.parent.GetComponent<CJObject>();
						}
						if (component != null && component.GetHitCollisionType() == HitCollisionType.Bones)
						{
							if (this.CheckBonesIntersection(blade_t0, blade_t1, component))
							{
								flag = true;
							}
						}
						else
						{
							flag = true;
						}
						if (flag)
						{
							float anim_frame = this.m_Animator.GetCurrentAnimatorStateInfo(1).length * this.m_Animator.GetCurrentAnimatorStateInfo(1).normalizedTime;
							this.SetupAnimFrameForHit(zero2, zero, identity, anim_frame, collider2);
							this.m_LastBoxHalfSizes = zero;
							this.m_LastBoxPos = zero2;
							this.m_LastBoxQuaternion = identity;
							this.m_LastAnimFrame = this.m_Animator.GetCurrentAnimatorStateInfo(1).length * this.m_Animator.GetCurrentAnimatorStateInfo(1).normalizedTime;
							cj_obj = component;
							collider = collider2;
							hit_pos = blade_t0.p0;
							return true;
						}
					}
				}
			}
		}
		this.m_LastBoxHalfSizes = zero;
		this.m_LastBoxPos = zero2;
		this.m_LastBoxQuaternion = identity;
		this.m_LastAnimFrame = this.m_Animator.GetCurrentAnimatorStateInfo(1).length * this.m_Animator.GetCurrentAnimatorStateInfo(1).normalizedTime;
		cj_obj = null;
		collider = null;
		return false;
	}

	protected virtual void OnHitWater(Collider water_coll)
	{
	}

	protected virtual void SetupAnimFrameForHit(Vector3 box_center, Vector3 box_half_sizes, Quaternion q, float anim_frame, Collider coll)
	{
		int num = 10;
		float animationStopFrame = this.m_LastAnimFrame;
		Vector3 center = this.m_LastBoxPos;
		Vector3 halfExtents = this.m_LastBoxHalfSizes;
		Quaternion orientation = this.m_LastBoxQuaternion;
		for (int i = 0; i < num; i++)
		{
			animationStopFrame = this.m_LastAnimFrame + (anim_frame - this.m_LastAnimFrame) / (float)num * (float)i;
			float t = (float)i / (float)num;
			center = Vector3.Lerp(this.m_LastBoxPos, box_center, t);
			halfExtents = Vector3.Lerp(this.m_LastBoxHalfSizes, box_half_sizes, t);
			orientation = Quaternion.Slerp(this.m_LastBoxQuaternion, q, t);
			Collider[] array = Physics.OverlapBox(center, halfExtents, orientation);
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] == coll)
				{
					this.m_AnimationStopFrame = animationStopFrame;
					return;
				}
			}
		}
		this.m_AnimationStopFrame = animationStopFrame;
	}

	protected bool CheckBonesIntersection(Triangle t0, Triangle t1, CJObject obj)
	{
		List<OBB> colliderBoxes = obj.GetColliderBoxes();
		for (int i = 0; i < colliderBoxes.Count; i++)
		{
			OBB box = colliderBoxes[i];
			if (Triangle.TriOBBIntersect(t0, box))
			{
				return true;
			}
			if (Triangle.TriOBBIntersect(t1, box))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckBoxIntersection(Triangle t0, Triangle t1, BoxCollider collider)
	{
		AABB aabb = new AABB();
		aabb.start = collider.center;
		aabb.half_sizes = collider.size * 0.5f;
		return Triangle.TriAABBIntersect(new Triangle
		{
			p0 = collider.gameObject.transform.InverseTransformPoint(t0.p0),
			p1 = collider.gameObject.transform.InverseTransformPoint(t0.p1),
			p2 = collider.gameObject.transform.InverseTransformPoint(t0.p2)
		}, aabb) || Triangle.TriAABBIntersect(new Triangle
		{
			p0 = collider.gameObject.transform.InverseTransformPoint(t1.p0),
			p1 = collider.gameObject.transform.InverseTransformPoint(t1.p1),
			p2 = collider.gameObject.transform.InverseTransformPoint(t1.p2)
		}, aabb);
	}

	private bool CheckMeshRaycast(Triangle t0, Triangle t1, MeshCollider collider, out Vector3 hit_pos)
	{
		Ray ray = default(Ray);
		ray.origin = t0.p0;
		ray.direction = t0.p1 - t0.p0;
		float magnitude = (t0.p1 - t0.p0).magnitude;
		RaycastHit raycastHit;
		if (magnitude > 0f && collider.Raycast(ray, out raycastHit, magnitude))
		{
			hit_pos = raycastHit.point;
			return true;
		}
		ray.origin = t0.p1;
		ray.direction = t0.p2 - t0.p1;
		magnitude = (t0.p2 - t0.p1).magnitude;
		if (magnitude > 0f && collider.Raycast(ray, out raycastHit, magnitude))
		{
			hit_pos = raycastHit.point;
			return true;
		}
		ray.origin = t0.p2;
		ray.direction = t0.p0 - t0.p2;
		magnitude = (t0.p0 - t0.p2).magnitude;
		if (magnitude > 0f && collider.Raycast(ray, out raycastHit, magnitude))
		{
			hit_pos = raycastHit.point;
			return true;
		}
		hit_pos = Vector3.zero;
		return false;
	}

	private bool CheckMeshIntersection(Triangle t0, Triangle t1, MeshCollider coll)
	{
		Mesh sharedMesh = coll.sharedMesh;
		Triangle triangle = new Triangle();
		Triangle triangle2 = new Triangle();
		triangle2.p0 = coll.gameObject.transform.InverseTransformPoint(t0.p0);
		triangle2.p1 = coll.gameObject.transform.InverseTransformPoint(t0.p1);
		triangle2.p2 = coll.gameObject.transform.InverseTransformPoint(t0.p2);
		Triangle triangle3 = new Triangle();
		triangle3.p0 = coll.gameObject.transform.InverseTransformPoint(t1.p0);
		triangle3.p1 = coll.gameObject.transform.InverseTransformPoint(t1.p1);
		triangle3.p2 = coll.gameObject.transform.InverseTransformPoint(t1.p2);
		int i = 0;
		int num = 0;
		while (i < sharedMesh.GetTriangles(1).Length)
		{
			int num2 = sharedMesh.GetTriangles(1)[i];
			if (num == 0)
			{
				triangle.p0 = sharedMesh.vertices[num2];
			}
			else if (num == 1)
			{
				triangle.p1 = sharedMesh.vertices[num2];
			}
			else if (num == 2)
			{
				triangle.p2 = sharedMesh.vertices[num2];
				if (Triangle.TriTriIntersect(triangle, triangle2))
				{
					return true;
				}
				if (Triangle.TriTriIntersect(triangle, triangle3))
				{
					return true;
				}
				num = -1;
			}
			i++;
			num++;
		}
		return false;
	}

	private void HitHumanAI(HumanAI human, Vector3 hit_pos, Vector3 hit_dir)
	{
		if (!human.CanReceiveHit())
		{
			return;
		}
		this.m_AudioModule.PlayHitSound(1f, false);
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		Weapon weapon = null;
		if (currentItem.m_Info.IsWeapon())
		{
			weapon = (Weapon)currentItem;
		}
		if (!human.IsDead())
		{
			this.ExecuteSkill(currentItem);
		}
		DamageInfo damageInfo = new DamageInfo();
		damageInfo.m_Damage = ((!(weapon != null)) ? 1000f : ((WeaponInfo)weapon.m_Info).m_HumanDamage);
		damageInfo.m_Damage *= this.GetDamageMultiplier(currentItem);
		damageInfo.m_DamageItem = currentItem;
		damageInfo.m_Damager = this.m_Player.gameObject;
		damageInfo.m_Position = hit_pos;
		damageInfo.m_HitDir = hit_dir;
		damageInfo.m_Normal = -hit_dir;
		human.TakeDamage(damageInfo);
	}

	protected virtual void HitObject(CJObject obj, Vector3 hit_pos, Vector3 hit_dir)
	{
		this.m_AudioModule.PlayHitSound(1f, false);
		if (obj.GetParticleOnHit().Length > 3)
		{
			this.SpawnFX(obj.GetParticleOnHit(), hit_pos);
		}
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		Weapon weapon = null;
		if (currentItem.m_Info.IsWeapon())
		{
			weapon = (Weapon)currentItem;
		}
		bool flag = true;
		this.m_AlreadyHit = true;
		if (obj.CanReceiveDamageOfType(currentItem.m_Info.m_DamageType))
		{
			DamageInfo damageInfo = new DamageInfo();
			if (weapon != null)
			{
				WeaponInfo weaponInfo = (WeaponInfo)weapon.m_Info;
				if (obj.IsAI())
				{
					damageInfo.m_Damage = weaponInfo.m_AnimalDamage;
					if (((AI)obj).IsDead())
					{
						flag = false;
					}
				}
				else
				{
					Item component = obj.GetComponent<Item>();
					if (component)
					{
						if (component.m_IsPlant)
						{
							damageInfo.m_Damage = weaponInfo.m_PlantDamage;
							this.m_AlreadyHit = false;
						}
						else if (component.m_IsTree)
						{
							damageInfo.m_Damage = weaponInfo.m_TreeDamage;
						}
						else
						{
							damageInfo.m_Damage = weaponInfo.m_DefaultDamage;
						}
					}
					else
					{
						damageInfo.m_Damage = weaponInfo.m_DefaultDamage;
					}
				}
			}
			else
			{
				Item component2 = obj.GetComponent<Item>();
				if (component2 && component2.m_IsTree && weapon.m_Info.IsTorch())
				{
					damageInfo.m_Damage = 0f;
				}
				else
				{
					damageInfo.m_Damage = 1000f;
				}
			}
			damageInfo.m_Damage *= this.GetDamageMultiplier(currentItem);
			damageInfo.m_Damager = this.m_Player.gameObject;
			damageInfo.m_Position = hit_pos;
			damageInfo.m_HitDir = hit_dir;
			damageInfo.m_Normal = Vector3.up;
			damageInfo.m_DamageItem = currentItem;
			obj.TakeDamage(damageInfo);
			this.HitDestroyableChunkSource(obj.GetComponent<Collider>(), hit_pos, hit_dir, currentItem);
			if (flag)
			{
				this.ExecuteSkill(currentItem);
			}
		}
	}

	protected virtual void SpawnFX(string fx_name, Vector3 hit_pos)
	{
		ParticlesManager.Get().Spawn(fx_name, hit_pos, Quaternion.identity, null);
	}

	private void ExecuteSkill(Item current_item)
	{
		if (current_item.IsKnife())
		{
			Skill.Get<BladeSkill>().OnSkillAction();
		}
		else if (current_item.IsSpear())
		{
			Skill.Get<SpearSkill>().OnSkillAction();
		}
		else if (current_item.IsAxe())
		{
			Skill.Get<AxeSkill>().OnSkillAction();
		}
		else if (current_item.IsMachete())
		{
			Skill.Get<MacheteSkill>().OnSkillAction();
		}
	}

	private float GetDamageMultiplier(Item current_item)
	{
		if (current_item.IsKnife())
		{
			return Skill.Get<BladeSkill>().GetDamageMul();
		}
		if (current_item.IsSpear())
		{
			return Skill.Get<SpearSkill>().GetDamageMul();
		}
		if (current_item.IsAxe())
		{
			return Skill.Get<AxeSkill>().GetDamageMul();
		}
		if (current_item.IsMachete())
		{
			return Skill.Get<MacheteSkill>().GetDamageMul();
		}
		return 1f;
	}

	private void MakeHitSound(GameObject obj, ItemID item_id)
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		ObjectMaterial component = obj.GetComponent<ObjectMaterial>();
		if (component)
		{
			AudioClip specificSound = this.GetSpecificSound(item_id, component.m_ObjectMaterial);
			if (specificSound)
			{
				this.m_AudioSource.PlayOneShot(specificSound);
				return;
			}
		}
		if (component)
		{
			int index = UnityEngine.Random.Range(0, this.m_AudioClipsHit[(int)component.m_ObjectMaterial].Count);
			AudioClip clip = this.m_AudioClipsHit[(int)component.m_ObjectMaterial][index];
			this.m_AudioSource.PlayOneShot(clip);
		}
		else
		{
			int index2 = UnityEngine.Random.Range(0, this.m_AudioClipsHit[0].Count);
			AudioClip clip2 = this.m_AudioClipsHit[0][index2];
			this.m_AudioSource.PlayOneShot(clip2);
		}
	}

	private bool StopAnimation()
	{
		this.m_AnimationStopped = true;
		this.m_AnimationStopTime = Time.time;
		this.SetAnimationFrame(this.m_AnimationStopFrame);
		this.m_Animator.SetBool(this.m_BRemoveWeaponFromObstacle, true);
		this.m_Player.BlockMoves();
		this.m_Player.BlockRotation();
		this.m_MovesBlocked = true;
		return true;
	}

	private void StartAnimation()
	{
		this.m_AnimationStopped = false;
		this.m_AnimationStopTime = -1f;
		this.m_Animator.SetBool(this.m_BRemoveWeaponFromObstacle, false);
		this.m_Player.UnblockMoves();
		this.m_Player.UnblockRotation();
		this.m_MovesBlocked = false;
		this.AttackRelease();
		this.EndAttack();
		this.m_Animator.SetFloat(this.m_NSwingSpeedMultiplier, 0f);
	}

	protected virtual void AttackRelease()
	{
	}

	protected void UpdateStoppedAnimation()
	{
		if (Time.time > this.m_AnimationStopDuration + this.m_AnimationStopTime)
		{
			this.StartAnimation();
			return;
		}
		AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(1);
		this.m_AnimationStopLength = currentAnimatorStateInfo.length;
		this.m_Animator.PlayInFixedTime(currentAnimatorStateInfo.fullPathHash, 1, this.m_AnimationStopFrame);
	}

	private void UpdateStoppedAnimationPose()
	{
		this.SetAnimationFrame(this.m_AnimationStopFrame);
	}

	protected void SetState(WeaponControllerState state)
	{
		this.m_State = state;
	}

	protected override void Attack()
	{
		base.Attack();
		this.m_AlreadyHit = false;
		this.m_HitObjects.Clear();
		this.SetState(WeaponControllerState.Swing);
		this.m_LastAnimFrame = 0f;
	}

	private void SetAnimationFrame(float frame)
	{
		AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(1);
		AnimatorClipInfo[] currentAnimatorClipInfo = this.m_Animator.GetCurrentAnimatorClipInfo(1);
		if (currentAnimatorClipInfo.Length == 0)
		{
			return;
		}
		this.m_Animator.PlayInFixedTime(currentAnimatorStateInfo.fullPathHash, 1, this.m_AnimationStopFrame);
		currentAnimatorClipInfo[0].clip.SampleAnimation(this.m_Animator.gameObject, this.m_AnimationStopFrame);
		this.m_Player.UpdateBonesRotation();
	}

	public virtual bool StopAnimOnHit(CJObject hit_obj, Collider coll = null)
	{
		return false;
	}

	private void HitDestroyableChunkSource(Collider coll, Vector3 hit_pos, Vector3 hit_dir, Item item)
	{
		if (coll == null)
		{
			return;
		}
		if (coll.gameObject.GetComponent<DestroyableChunkSource>() != null)
		{
			coll.gameObject.GetComponent<DestroyableChunkSource>().Hit(hit_pos, hit_dir, item);
		}
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.PlaySwingSound && this.CanPlaySwingSound())
		{
			this.PlaySwingSound();
		}
	}

	public virtual AttackDirection GetAttackDirection()
	{
		return AttackDirection.None;
	}

	public virtual bool DuckDuringAttack()
	{
		return false;
	}

	public virtual bool CanPlaySwingSound()
	{
		return true;
	}

	private void PlaySwingSound()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		if (currentItem == null)
		{
			return;
		}
		ItemInfo info = currentItem.m_Info;
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

	protected int m_IWeaponType = Animator.StringToHash("WeaponType");

	private int m_MeleeIdleHash = Animator.StringToHash("MeleeIdle");

	private int m_TakeOutItemHash = Animator.StringToHash("TakeOutItem");

	protected int m_SpearIdleHash = Animator.StringToHash("Spear_Idle");

	private int m_BRemoveWeaponFromObstacle = Animator.StringToHash("RemoveWeaponFromObstacle");

	protected PlayerAudioModule m_AudioModule;

	private Vector3 m_DamagerLastPosStart = Vector3.zero;

	private Vector3 m_DamagerLastPosEnd = Vector3.zero;

	private Vector3 m_HandleLastPosEnd = Vector3.zero;

	private Transform m_HandleEndTransform;

	private AudioSource m_AudioSource;

	private Dictionary<int, List<AudioClip>> m_AudioClipsHit = new Dictionary<int, List<AudioClip>>();

	private float m_LastHitTime;

	protected bool m_AnimationStopped;

	private float m_AnimationStopDuration = 0.2333f;

	private float m_AnimationStopTime = -1f;

	protected float m_AnimationStopFrame = -1f;

	protected float m_AnimationStopLength = -1f;

	private WeaponControllerState m_State;

	protected Vector3 m_LastBoxPos = Vector3.zero;

	protected Vector3 m_LastBoxHalfSizes = Vector3.zero;

	protected Quaternion m_LastBoxQuaternion = Quaternion.identity;

	protected float m_LastAnimFrame = -1f;

	protected bool m_MovesBlocked;

	private Dictionary<int, List<AudioClip>> m_SwingSoundClipsDict = new Dictionary<int, List<AudioClip>>();

	protected bool m_AlreadyHit;

	protected List<GameObject> m_HitObjects = new List<GameObject>(10);

	private Triangle m_BladeT0 = new Triangle();

	private Triangle m_BladeT1 = new Triangle();

	protected bool m_ComboScheduled;

	protected bool m_ReleaseComboScheduled;

	protected bool m_ReleaseCombo;

	protected bool m_ComboBlocked;

	protected int m_NSwingSpeedMultiplier = Animator.StringToHash("SwingSpeedMultiplier");

	private bool m_WasActivated;

	private Dictionary<int, Dictionary<int, List<AudioClip>>> m_HitSpecificSounds = new Dictionary<int, Dictionary<int, List<AudioClip>>>();
}
