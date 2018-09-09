using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class TriggerController : PlayerController
{
	public bool m_TriggerInAction { get; protected set; }

	public static TriggerController Get()
	{
		return TriggerController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		TriggerController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.Trigger;
		this.m_BestTrigger = default(TriggerController.TriggerData);
		this.m_BestTrigger.actions = new List<TriggerAction.TYPE>();
		this.m_BestTrigger.Reset();
		this.m_AdditionalTrigger = default(TriggerController.TriggerData);
		this.m_AdditionalTrigger.actions = new List<TriggerAction.TYPE>();
		this.m_AdditionalTrigger.Reset();
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		this.SetupAudio();
		this.m_BalanceSpawnerLayer = LayerMask.NameToLayer("BalanceSpawner");
	}

	private void SetupAudio()
	{
		this.m_DeconstructHoldClips.Add((AudioClip)Resources.Load("Sounds/Constructions/construction_deconstruct_hold_01"));
		this.m_DeconstructHoldClips.Add((AudioClip)Resources.Load("Sounds/Constructions/construction_deconstruct_hold_03"));
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_BestTrigger.Reset();
		this.m_AdditionalTrigger.Reset();
		if (this.m_Animator.isInitialized)
		{
			this.m_Animator.SetBool(TriggerController.s_BGrabItem, false);
			this.m_Animator.SetBool(this.m_BDrinkWater, false);
			this.m_Animator.SetBool(TriggerController.s_BGrabItemBow, false);
			this.m_Animator.SetBool(TriggerController.s_BGrabItemBambooBow, false);
		}
	}

	public Trigger GetBestTrigger()
	{
		return this.m_BestTrigger.trigger;
	}

	public Vector3 GetBestTriggerHitPos()
	{
		return this.m_BestTrigger.hit_pos;
	}

	public Trigger GetAdditionalTrigger()
	{
		return this.m_AdditionalTrigger.trigger;
	}

	public Vector3 GetAdditionalTriggerHitPos()
	{
		return this.m_AdditionalTrigger.hit_pos;
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateBestTrigger();
		if (this.m_BestTrigger.trigger)
		{
			this.m_BestTrigger.trigger.UpdateBestTrigger();
		}
	}

	public void OnTriggerAction(TriggerAction.TYPE action)
	{
		if (this.m_Player.GetRotationBlocked())
		{
			return;
		}
		this.OnTriggerAction(ref this.m_BestTrigger, ref this.m_HoldDatasBestTrigger, action);
		if (this.m_AdditionalTrigger.trigger && this.m_AdditionalTrigger.actions.Contains(action) && this.m_AdditionalTrigger.trigger.CanExecuteActions())
		{
			this.ExecuteTrigger(this.m_AdditionalTrigger.trigger, action);
		}
	}

	private void OnTriggerAction(ref TriggerController.TriggerData data, ref List<TriggerHoldData> hold_data, TriggerAction.TYPE action)
	{
		if (data.trigger && data.actions.Contains(action) && data.trigger.CanExecuteActions())
		{
			InputActionData inputActionData = InputsManager.Get().GetInputActionData(action);
			if (this.m_TriggerInAction)
			{
				return;
			}
			if (inputActionData.m_Hold == 0f)
			{
				TriggerAction.TYPE type = TriggerAction.TYPE.None;
				for (int i = 0; i < data.actions.Count; i++)
				{
					InputActionData inputActionData2 = InputsManager.Get().GetInputActionData(data.actions[i]);
					if (inputActionData2.m_TriggerAction != action)
					{
						if (inputActionData2.m_KeyCodes[0] == inputActionData.m_KeyCodes[0])
						{
							if (inputActionData2.m_Hold > 0f)
							{
								type = inputActionData2.m_TriggerAction;
								break;
							}
						}
					}
				}
				if (type != TriggerAction.TYPE.None)
				{
					hold_data.Add(new TriggerHoldData(action, type));
				}
				else
				{
					this.ExecuteTrigger(data.trigger, action);
				}
			}
			else
			{
				int j = 0;
				while (j < hold_data.Count)
				{
					if (hold_data[j].m_HoldAction == action)
					{
						hold_data.RemoveAt(j);
					}
					else
					{
						j++;
					}
				}
				this.ExecuteTrigger(data.trigger, action);
			}
		}
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		this.UpdateHoldDatas();
	}

	private void UpdateHoldDatas()
	{
		int i = 0;
		while (i < this.m_HoldDatasBestTrigger.Count)
		{
			if (!InputsManager.Get().IsActionActive(this.m_HoldDatasBestTrigger[i].m_OrigAction))
			{
				this.ExecuteTrigger(this.m_BestTrigger.trigger, this.m_HoldDatasBestTrigger[i].m_OrigAction);
				this.m_HoldDatasBestTrigger.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
	}

	private void UpdateBestTrigger()
	{
		if (HUDReadableItem.Get().enabled)
		{
			this.m_BestTrigger.Reset();
			this.m_AdditionalTrigger.Reset();
			return;
		}
		if (Camera.main == null)
		{
			this.m_BestTrigger.Reset();
			this.m_AdditionalTrigger.Reset();
			return;
		}
		if (CutscenesManager.Get().IsCutscenePlaying())
		{
			this.m_BestTrigger.Reset();
			this.m_AdditionalTrigger.Reset();
			return;
		}
		if (MakeFireController.Get().IsActive() && (MakeFireController.Get().ShouldBlockTriggers() || !Inventory3DManager.Get().gameObject.activeSelf))
		{
			this.m_BestTrigger.Reset();
			this.m_AdditionalTrigger.Reset();
			return;
		}
		if (CraftingController.Get().IsActive() && CraftingController.Get().m_InProgress)
		{
			this.m_BestTrigger.Reset();
			this.m_AdditionalTrigger.Reset();
			return;
		}
		if (this.m_Player.m_ActiveFightController && this.m_Player.m_ActiveFightController.IsBlock())
		{
			this.m_BestTrigger.Reset();
			this.m_AdditionalTrigger.Reset();
			return;
		}
		if (HUDItem.Get().m_Active && HUDItem.Get().m_Item)
		{
			this.SetBestTrigger(HUDItem.Get().m_Item, HUDItem.Get().m_Item.transform.position);
			return;
		}
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			if (Inventory3DManager.Get().m_FocusedItem)
			{
				this.SetBestTrigger(Inventory3DManager.Get().m_FocusedItem, Inventory3DManager.Get().m_FocusedItem.transform.position);
			}
			else
			{
				this.m_BestTrigger.Reset();
				this.m_AdditionalTrigger.Reset();
			}
			return;
		}
		if (BodyInspectionController.Get().IsActive())
		{
			this.m_BestTrigger.Reset();
			this.m_AdditionalTrigger.Reset();
			return;
		}
		if (Player.Get().m_Aim)
		{
			this.m_BestTrigger.Reset();
			this.m_AdditionalTrigger.Reset();
			return;
		}
		if (WeaponSpearController.Get().IsActive() && WeaponSpearController.Get().m_ItemBody)
		{
			this.SetBestTrigger(WeaponSpearController.Get().m_ItemBody, WeaponSpearController.Get().m_ItemBody.transform.position);
			return;
		}
		TriggerController.s_AllPotentialTriggers.Clear();
		TriggerController.s_OffCrosshairTriggers.Clear();
		TriggerController.s_ColldersEnabledMap.Clear();
		Vector3 crossHairOrigin = this.GetCrossHairOrigin();
		Vector3 position = Player.Get().transform.position;
		float num = 0.8f;
		float num2 = -1f;
		float num3 = -1f;
		float num4 = float.MinValue;
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		TriggersManager triggersManager = TriggersManager.Get();
		bool flag = false;
		Trigger trigger = null;
		Vector3 hit_pos = Vector3.zero;
		Trigger trigger2 = null;
		Vector3 hit_pos2 = Vector3.zero;
		float num5 = float.MinValue;
		for (int i = 0; i < triggersManager.GetActiveTriggersCount(); i++)
		{
			Trigger trigger3 = triggersManager.GetTrigger(i, false);
			if (trigger3 && trigger3.enabled)
			{
				if (!trigger3.m_IsCut)
				{
					if (trigger3.CanTrigger())
					{
						Collider collider = null;
						trigger3.gameObject.GetComponents<Collider>(TriggerController.s_ColliderCache);
						if (TriggerController.s_ColliderCache.Count > 0)
						{
							collider = TriggerController.s_ColliderCache[0];
						}
						if (collider)
						{
							if (trigger3.CheckInsideCollider() && collider.bounds.Contains(Camera.main.transform.position) && trigger3.IsAdditionalTrigger() && (!trigger3.CheckDot() || num4 > num3))
							{
								hit_pos2 = collider.bounds.center;
								trigger2 = trigger3;
								num3 = num4;
							}
							float num6 = (trigger3.m_TriggerCheckRange <= 0f) ? this.m_Player.GetParams().GetTriggerCheckRange() : trigger3.m_TriggerCheckRange;
							if (trigger3.CheckRange())
							{
								vector2 = ((!collider) ? trigger3.gameObject.transform.position : collider.ClosestPointOnBounds(position));
								float num7 = Vector3.Distance(position, vector2);
								if (num7 > num6)
								{
									goto IL_539;
								}
							}
							vector2 = ((!collider) ? trigger3.gameObject.transform.position : collider.bounds.center);
							vector = vector2 - crossHairOrigin;
							vector.Normalize();
							if (trigger3.CheckDot())
							{
								num4 = Vector3.Dot(Camera.main.transform.forward, vector);
								if (num4 < num)
								{
									goto IL_539;
								}
							}
							TriggerController.s_AllPotentialTriggers.Add(trigger3);
							if (!trigger3.OnlyInCrosshair())
							{
								TriggerController.s_OffCrosshairTriggers.Add(trigger3);
							}
							TriggerController.s_ColldersEnabledMap[collider] = collider.enabled;
							collider.enabled = true;
							if (num6 > num5)
							{
								num5 = num6;
							}
						}
					}
				}
			}
			IL_539:;
		}
		int num8 = Physics.RaycastNonAlloc(crossHairOrigin, this.GetCrossHairDir(), TriggerController.s_RaycastHitCache, num5);
		if ((float)num8 > 0f)
		{
			TriggerController.s_CrosshairOrigin = this.GetCrossHairOrigin();
			Array.Sort<RaycastHit>(TriggerController.s_RaycastHitCache, 0, num8, TriggerController.s_DistComparer);
			for (int j = 0; j < TriggerController.s_AllPotentialTriggers.Count; j++)
			{
				Trigger trigger4 = TriggerController.s_AllPotentialTriggers[j];
				Collider collider2 = null;
				trigger4.gameObject.GetComponents<Collider>(TriggerController.s_ColliderCache);
				if (TriggerController.s_ColliderCache.Count > 0)
				{
					collider2 = TriggerController.s_ColliderCache[0];
				}
				if (collider2)
				{
					for (int k = 0; k < num8; k++)
					{
						RaycastHit raycastHit = TriggerController.s_RaycastHitCache[k];
						if (raycastHit.collider)
						{
							if (!(raycastHit.collider.gameObject == base.gameObject))
							{
								if (!(raycastHit.collider == FistFightController.Get().m_RightHandCollider))
								{
									if (!(raycastHit.collider == FistFightController.Get().m_LeftHandCollider))
									{
										GhostSlot x = null;
										raycastHit.collider.gameObject.GetComponents<GhostSlot>(TriggerController.s_GhostSlotCache);
										if (TriggerController.s_GhostSlotCache.Count > 0)
										{
											x = TriggerController.s_GhostSlotCache[0];
										}
										if (!flag || !(x == null) || trigger4.IsAdditionalTrigger())
										{
											if (collider2 == raycastHit.collider || trigger4.IsAdditionalCollider(raycastHit.collider))
											{
												if (trigger4.IsAdditionalTrigger())
												{
													if (!trigger4.CheckDot() || num4 > num3)
													{
														hit_pos2 = raycastHit.point;
														trigger2 = trigger4;
														num3 = num4;
													}
												}
												else if (!trigger4.CheckDot() || num4 > num2)
												{
													hit_pos = raycastHit.point;
													trigger = trigger4;
													if (!trigger || !trigger.IsLiquidSource())
													{
														num2 = num4;
													}
												}
												break;
											}
											ITriggerThrough triggerThrough = null;
											raycastHit.collider.gameObject.GetComponents<ITriggerThrough>(TriggerController.s_TriggerThroughCache);
											if (TriggerController.s_TriggerThroughCache.Count > 0)
											{
												triggerThrough = TriggerController.s_TriggerThroughCache[0];
											}
											if (triggerThrough == null)
											{
												if (!trigger || !trigger.IsLiquidSource() || !(trigger.gameObject == raycastHit.collider.gameObject))
												{
													if (x != null)
													{
														flag = true;
													}
													else
													{
														Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
														if (!currentItem || !(currentItem.gameObject == raycastHit.collider.gameObject))
														{
															currentItem = this.m_Player.GetCurrentItem(Hand.Left);
															if (!currentItem || !(currentItem.gameObject == raycastHit.collider.gameObject))
															{
																Trigger trigger5 = null;
																raycastHit.collider.gameObject.GetComponents<Trigger>(TriggerController.s_OtherTriggerCache);
																if (TriggerController.s_OtherTriggerCache.Count > 0)
																{
																	trigger5 = TriggerController.s_OtherTriggerCache[0];
																}
																if (!trigger5 || !trigger5.TriggerThrough())
																{
																	break;
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
		if (trigger == null || trigger.IsLiquidSource())
		{
			TriggerController.s_CrosshairDir = this.GetCrossHairDir();
			TriggerController.s_CrosshairOrigin = this.GetCrossHairOrigin();
			TriggerController.s_OffCrosshairTriggers.Sort(TriggerController.s_DotComparer);
			bool flag2 = false;
			int num9 = 0;
			while (num9 < TriggerController.s_OffCrosshairTriggers.Count && !flag2)
			{
				Trigger trigger6 = TriggerController.s_OffCrosshairTriggers[num9];
				Collider collider3 = null;
				trigger6.gameObject.GetComponents<Collider>(TriggerController.s_ColliderCache);
				if (TriggerController.s_ColliderCache.Count > 0)
				{
					collider3 = TriggerController.s_ColliderCache[0];
				}
				if (collider3)
				{
					float maxDistance = (trigger6.m_TriggerCheckRange <= 0f) ? this.m_Player.GetParams().GetTriggerCheckRange() : trigger6.m_TriggerCheckRange;
					vector2 = ((!collider3) ? trigger6.gameObject.transform.position : collider3.bounds.center);
					vector = vector2 - crossHairOrigin;
					vector.Normalize();
					num8 = Physics.RaycastNonAlloc(crossHairOrigin, vector, TriggerController.s_RaycastHitCache, maxDistance);
					if ((float)num8 > 0f)
					{
						TriggerController.s_CrosshairOrigin = this.GetCrossHairOrigin();
						Array.Sort<RaycastHit>(TriggerController.s_RaycastHitCache, 0, num8, TriggerController.s_DistComparer);
						for (int l = 0; l < num8; l++)
						{
							RaycastHit raycastHit2 = TriggerController.s_RaycastHitCache[l];
							if (!(raycastHit2.collider.gameObject == base.gameObject))
							{
								GhostSlot x2 = null;
								raycastHit2.collider.gameObject.GetComponents<GhostSlot>(TriggerController.s_GhostSlotCache);
								if (TriggerController.s_GhostSlotCache.Count > 0)
								{
									x2 = TriggerController.s_GhostSlotCache[0];
								}
								if (!flag || !(x2 == null) || trigger6.IsAdditionalTrigger())
								{
									if (collider3 == raycastHit2.collider)
									{
										if (!trigger6.CheckDot() || num4 > num2)
										{
											hit_pos = raycastHit2.point;
											trigger = trigger6;
											num2 = num4;
											flag2 = true;
										}
										break;
									}
									ITriggerThrough triggerThrough2 = null;
									raycastHit2.collider.gameObject.GetComponents<ITriggerThrough>(TriggerController.s_TriggerThroughCache);
									if (TriggerController.s_TriggerThroughCache.Count > 0)
									{
										triggerThrough2 = TriggerController.s_TriggerThroughCache[0];
									}
									if (triggerThrough2 == null)
									{
										if (raycastHit2.collider.gameObject.layer != this.m_BalanceSpawnerLayer)
										{
											if (!trigger || !trigger.IsLiquidSource() || !(trigger.gameObject == raycastHit2.collider.gameObject))
											{
												if (x2 != null)
												{
													flag = true;
												}
												else
												{
													Item currentItem2 = this.m_Player.GetCurrentItem(Hand.Right);
													if (!currentItem2 || !(currentItem2.gameObject == raycastHit2.collider.gameObject))
													{
														currentItem2 = this.m_Player.GetCurrentItem(Hand.Left);
														if (!currentItem2 || !(currentItem2.gameObject == raycastHit2.collider.gameObject))
														{
															Trigger trigger7 = null;
															raycastHit2.collider.gameObject.GetComponents<Trigger>(TriggerController.s_OtherTriggerCache);
															if (TriggerController.s_OtherTriggerCache.Count > 0)
															{
																trigger7 = TriggerController.s_OtherTriggerCache[0];
															}
															if (!trigger7 || !trigger7.TriggerThrough())
															{
																if (trigger7 && TriggerController.s_OffCrosshairTriggers.Contains(trigger7))
																{
																	trigger = trigger7;
																	hit_pos = raycastHit2.point;
																	flag2 = true;
																}
																break;
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
				num9++;
			}
		}
		this.SetBestTrigger(trigger, hit_pos);
		if (this.m_AdditionalTrigger.trigger != trigger2)
		{
			this.m_AdditionalTrigger.trigger = trigger2;
			this.m_AdditionalTrigger.actions.Clear();
			if (this.m_AdditionalTrigger.trigger)
			{
				this.m_AdditionalTrigger.trigger.GetActions(this.m_AdditionalTrigger.actions);
			}
			this.m_AdditionalTrigger.hit_pos = hit_pos2;
		}
		foreach (KeyValuePair<Collider, bool> keyValuePair in TriggerController.s_ColldersEnabledMap)
		{
			Collider key = keyValuePair.Key;
			Collider collider4 = key;
			Dictionary<Collider, bool>.Enumerator enumerator;
			KeyValuePair<Collider, bool> keyValuePair2 = enumerator.Current;
			collider4.enabled = keyValuePair2.Value;
		}
	}

	private void SetBestTrigger(Trigger trigger, Vector3 hit_pos)
	{
		this.m_BestTrigger.trigger = trigger;
		this.m_BestTrigger.actions.Clear();
		if (this.m_BestTrigger.trigger)
		{
			this.m_BestTrigger.trigger.GetActions(this.m_BestTrigger.actions);
		}
		this.m_BestTrigger.hit_pos = hit_pos;
	}

	public void ExecuteTrigger(Trigger trigger, TriggerAction.TYPE action)
	{
		if (this.IsGrabInProgress())
		{
			return;
		}
		if (Time.time - this.m_LastTrigerExecutionTime < 0.1f)
		{
			return;
		}
		if (!trigger)
		{
			return;
		}
		if (this.m_TriggerToExecute)
		{
			if (Time.time - this.m_TriggerToExecuteTime > 2f)
			{
				this.m_TriggerToExecute = null;
				this.m_TriggerActionToExecute = TriggerAction.TYPE.None;
			}
			return;
		}
		if (trigger.PlayGrabAnimOnExecute(action))
		{
			this.m_TriggerToExecute = trigger;
			this.m_TriggerActionToExecute = action;
			this.m_TriggerToExecuteTime = Time.time;
		}
		else
		{
			trigger.OnExecute(action);
			this.m_LastTrigerExecutionTime = Time.time;
		}
		if (action == TriggerAction.TYPE.Use)
		{
			HUDMessages hudmessages = (HUDMessages)HUDManager.Get().GetHUD(typeof(HUDMessages));
			string text = GreenHellGame.Instance.GetLocalization().Get(TriggerAction.GetTextPerfect(action)) + ": " + GreenHellGame.Instance.GetLocalization().Get(trigger.GetName());
			if (action == TriggerAction.TYPE.Drink && trigger.IsItem() && ((Item)trigger).m_Info.IsLiquidContainer())
			{
				Localization localization = GreenHellGame.Instance.GetLocalization();
				LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)((Item)trigger).m_Info;
				text += liquidContainerInfo.m_Amount.ToString("F1");
				text += localization.Get("HUD_Hydration");
			}
			hudmessages.AddMessage(text, null, (action != TriggerAction.TYPE.Drink) ? HUDMessageIcon.None : HUDMessageIcon.Hydration, string.Empty);
		}
		else if (action != TriggerAction.TYPE.Eat)
		{
			if (action == TriggerAction.TYPE.Take || action == TriggerAction.TYPE.TakeHold || action == TriggerAction.TYPE.PickUp)
			{
				Item currentItem = this.m_Player.GetCurrentItem(Hand.Left);
				if (currentItem != null && currentItem.m_Info.m_ID == ItemID.Bow)
				{
					this.m_Animator.SetBool(TriggerController.s_BGrabItemBow, true);
				}
				else if (currentItem != null && currentItem.m_Info.m_ID == ItemID.Bamboo_Bow)
				{
					this.m_Animator.SetBool(TriggerController.s_BGrabItemBambooBow, true);
				}
				else if (currentItem && currentItem.m_Info.IsBow())
				{
					this.m_Animator.SetBool(TriggerController.s_BGrabItemBow, true);
				}
				else if (trigger.PlayGrabAnimOnExecute(action))
				{
					this.m_Animator.SetBool(TriggerController.s_BGrabItem, true);
				}
			}
			else if (action == TriggerAction.TYPE.DrinkHold)
			{
				this.m_Animator.SetBool(this.m_BDrinkWater, true);
				this.m_TriggerInAction = true;
			}
		}
	}

	public bool IsGrabInProgress()
	{
		return this.m_Animator.GetBool(TriggerController.s_BGrabItemBow) || this.m_Animator.GetBool(TriggerController.s_BGrabItem) || this.m_Animator.GetBool(TriggerController.s_BGrabItemBambooBow);
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.GrabItem || id == AnimEventID.DrinkLiquid)
		{
			if (this.m_TriggerToExecute)
			{
				this.m_TriggerToExecute.OnExecute(this.m_TriggerActionToExecute);
				this.m_TriggerActionToExecute = TriggerAction.TYPE.None;
				this.m_LastTrigerExecutionTime = Time.time;
				this.m_TriggerToExecute = null;
			}
		}
		else if (id == AnimEventID.GrabItemEnd)
		{
			this.m_Animator.SetBool(TriggerController.s_BGrabItem, false);
			this.m_Animator.SetBool(TriggerController.s_BGrabItemBow, false);
			this.m_Animator.SetBool(TriggerController.s_BGrabItemBambooBow, false);
			this.m_TriggerToExecute = null;
		}
		else if (id == AnimEventID.DrinkLiquidEnd)
		{
			this.m_Animator.SetBool(this.m_BDrinkWater, false);
			this.m_TriggerInAction = false;
		}
	}

	public Vector3 GetCrossHairOrigin()
	{
		return Camera.main.transform.position;
	}

	public Vector3 GetCrossHairDir()
	{
		return Camera.main.transform.forward;
	}

	public void OnTriggerHoldStart(TriggerAction.TYPE action)
	{
		if (action == TriggerAction.TYPE.Deconstruct && this.m_AudioSource != null && !this.m_AudioSource.isPlaying)
		{
			this.m_AudioSource.PlayOneShot(this.m_DeconstructHoldClips[UnityEngine.Random.Range(0, this.m_DeconstructHoldClips.Count)]);
		}
	}

	public void OnTriggerHoldEnd(TriggerAction.TYPE action)
	{
		if (this.m_AudioSource != null && this.m_AudioSource.isPlaying)
		{
			this.m_AudioSource.Stop();
		}
	}

	private static TriggerController s_Instance = null;

	private float m_LastTrigerExecutionTime;

	public static int s_BGrabItem = Animator.StringToHash("GrabItem");

	[HideInInspector]
	public int m_BDrinkWater = Animator.StringToHash("DrinkWater");

	public static int s_BGrabItemBow = Animator.StringToHash("GrabItemBow");

	public static int s_BGrabItemBambooBow = Animator.StringToHash("GrabItemBambooBow");

	private TriggerController.TriggerData m_BestTrigger;

	private TriggerController.TriggerData m_AdditionalTrigger;

	[HideInInspector]
	public Trigger m_TriggerToExecute;

	private TriggerAction.TYPE m_TriggerActionToExecute = TriggerAction.TYPE.None;

	private List<TriggerHoldData> m_HoldDatasBestTrigger = new List<TriggerHoldData>();

	private List<TriggerHoldData> m_HoldDatasAdditionalTrigger = new List<TriggerHoldData>();

	private List<AudioClip> m_DeconstructHoldClips = new List<AudioClip>();

	private AudioSource m_AudioSource;

	public static Vector3 s_CrosshairDir = Vector3.one;

	public static Vector3 s_CrosshairOrigin = Vector3.zero;

	private static List<Trigger> s_AllPotentialTriggers = new List<Trigger>(50);

	private static List<Trigger> s_OffCrosshairTriggers = new List<Trigger>(50);

	private static Dictionary<Collider, bool> s_ColldersEnabledMap = new Dictionary<Collider, bool>(50);

	private int m_BalanceSpawnerLayer;

	private static RaycastHit[] s_RaycastHitCache = new RaycastHit[30];

	private static List<Collider> s_ColliderCache = new List<Collider>(10);

	private static List<GhostSlot> s_GhostSlotCache = new List<GhostSlot>(10);

	private static List<ITriggerThrough> s_TriggerThroughCache = new List<ITriggerThrough>(10);

	private static List<Trigger> s_OtherTriggerCache = new List<Trigger>(10);

	private static CompareArrayByDist s_DistComparer = new CompareArrayByDist();

	private static CompareListByDot s_DotComparer = new CompareListByDot();

	private float m_TriggerToExecuteTime;

	private struct TriggerData
	{
		public void Reset()
		{
			this.trigger = null;
			this.hit_pos = Vector3.zero;
			this.actions.Clear();
		}

		public Trigger trigger;

		public Vector3 hit_pos;

		public List<TriggerAction.TYPE> actions;
	}
}
