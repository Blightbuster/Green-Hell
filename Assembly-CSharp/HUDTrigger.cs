using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDTrigger : HUDBase
{
	public static HUDTrigger GetNormal()
	{
		return HUDTrigger.s_NormalInstance;
	}

	public static HUDTrigger GetAdditional()
	{
		return HUDTrigger.s_AdditionalInstance;
	}

	protected override void Awake()
	{
		base.Awake();
		if (this.m_TriggerType == HUDTrigger.TriggerType.Normal)
		{
			HUDTrigger.s_NormalInstance = this;
		}
		else
		{
			HUDTrigger.s_AdditionalInstance = this;
		}
		this.m_Parent = base.transform.Find("Group").gameObject;
		this.m_CanvasGroup = this.m_Parent.AddComponent<CanvasGroup>();
		this.m_DefaultPosition = this.m_Parent.transform.localPosition;
		for (int i = 0; i < 2; i++)
		{
			this.m_KeyFrames[i] = this.m_Parent.transform.FindDeepChild("KeyFrame" + i.ToString()).gameObject.GetComponent<RawImage>();
			this.m_Keys[i] = this.m_Parent.transform.FindDeepChild("Key" + i.ToString()).gameObject.GetComponent<Text>();
			this.m_Actions[i] = this.m_Parent.transform.FindDeepChild("Action" + i.ToString()).gameObject.GetComponent<Text>();
			this.m_PadIcons[i] = this.m_Parent.transform.FindDeepChild("PadIcon" + i.ToString()).gameObject.GetComponent<Image>();
			if (this.m_TriggerType == HUDTrigger.TriggerType.Normal)
			{
				this.m_MouseRMBIcon[i] = this.m_Parent.transform.FindDeepChild("MouseRMBIcon" + i.ToString()).GetComponent<RawImage>();
				if (i > 0)
				{
					this.m_KeyFrameParents[i] = this.m_Parent.transform.FindDeepChild("KeyFrameParent" + i.ToString()).GetComponent<RectTransform>();
				}
			}
		}
		this.m_Name = this.m_Parent.transform.Find("Name").gameObject.GetComponent<Text>();
		this.m_Icon = this.m_Parent.transform.Find("Icon").gameObject.GetComponent<Image>();
		this.m_HoldProgress = this.m_Icon.transform.Find("HoldProgress").gameObject.GetComponent<Image>();
		this.m_AdditionalIcon = this.m_Icon.transform.Find("AdditionalIcon").gameObject.GetComponent<Image>();
		Transform transform = this.m_Parent.transform.Find("Info/AdditionalInfo");
		if (transform != null)
		{
			this.m_AdditionalInfo = transform.gameObject.GetComponent<Text>();
		}
		this.m_Parent.SetActive(false);
		this.m_TextGen = new TextGenerator();
		this.m_ConsumableEffectsDummy = this.m_Parent.transform.Find("Info/ConsumableEffectsDummy");
		Transform transform2 = this.m_Parent.transform.Find("Info/ConsumableEffects");
		if (transform2)
		{
			this.m_ConsumableEffects = transform2.gameObject.GetComponent<Text>();
			this.m_ConsumableEffects.GetComponent<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUDTrigger_ConsumableEffects", true).ToUpper();
			this.m_UnknownEffect = this.m_ConsumableEffects.gameObject.FindChild("UnknownEffect");
			this.m_UnknownEffect.GetComponent<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUDTrigger_Unknown", true).ToUpper();
			for (int j = 0; j < 999; j++)
			{
				Transform transform3 = transform2.Find("Effect_" + j);
				if (!transform3)
				{
					break;
				}
				TriggerCEData triggerCEData = new TriggerCEData();
				triggerCEData.m_Parent = transform3.gameObject;
				triggerCEData.m_Icon = transform3.Find("Icon").gameObject.GetComponent<Image>();
				triggerCEData.m_Text = transform3.Find("Text").gameObject.GetComponent<Text>();
				this.m_EffectsData.Add(triggerCEData);
			}
		}
		Transform transform4 = this.m_Parent.transform.Find("RequiredItems");
		if (transform4)
		{
			this.m_RequiredItems = transform4.gameObject.GetComponent<Text>();
			this.m_RequiredItems.GetComponent<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUDTrigger_RequiredItems", true).ToUpper();
			for (int k = 0; k < 999; k++)
			{
				Transform transform5 = transform4.Find("Item_" + k);
				if (!transform5)
				{
					break;
				}
				TriggerRIData triggerRIData = new TriggerRIData();
				triggerRIData.m_Parent = transform5.gameObject;
				triggerRIData.m_Icon = transform5.Find("Icon").gameObject.GetComponent<Image>();
				triggerRIData.m_Text = transform5.Find("Text").gameObject.GetComponent<Text>();
				this.m_RequiredItemsData.Add(triggerRIData);
			}
		}
		for (int l = 0; l < this.m_Sprites.Count; l++)
		{
			if (this.m_Sprites[l] != null)
			{
				this.m_SpritesMap.Add(this.m_Sprites[l].name, this.m_Sprites[l]);
			}
		}
		if (this.m_TriggerType == HUDTrigger.TriggerType.Normal)
		{
			this.m_AcrePlowIcon = UnityEngine.Object.Instantiate<GameObject>(this.m_AcreSeedicon, this.m_AcreSeedicon.transform.position, this.m_AcreSeedicon.transform.rotation, this.m_AcreSeedicon.transform.parent);
			Sprite sprite = null;
			ItemsManager.Get().m_ItemIconsSprites.TryGetValue("plow_icon", out sprite);
			this.m_AcrePlowIcon.GetComponent<Image>().sprite = sprite;
			this.m_AcrePlowIcon.gameObject.SetActive(false);
			this.m_AcrePlowIcon.name = "AcrePlow";
		}
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	private Trigger GetTrigger()
	{
		if (this.m_TriggerType == HUDTrigger.TriggerType.Normal)
		{
			return TriggerController.Get().GetBestTrigger();
		}
		return TriggerController.Get().GetAdditionalTrigger();
	}

	protected override bool ShouldShow()
	{
		return !TriggerController.Get().IsGrabInProgress() && (!HUDSelectDialog.Get().enabled || HUDManager.Get().m_ActiveGroup != HUDManager.HUDGroup.Game) && ((this.IsExpanded() && this.m_TriggerType == HUDTrigger.TriggerType.Normal) || (this.GetTrigger() && this.GetTrigger().CanTrigger() && !this.GetTrigger().m_HideHUD && (!Inventory3DManager.Get().IsActive() || !Inventory3DManager.Get().m_CarriedItem) && (this.m_TriggerType == HUDTrigger.TriggerType.Normal || !Inventory3DManager.Get().gameObject.activeSelf)));
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.m_Parent.SetActive(true);
		this.m_CanvasGroup.alpha = 0f;
		this.Update();
	}

	protected override void OnHide()
	{
		base.OnHide();
		this.m_CurrentTrigger = null;
		this.m_Parent.SetActive(false);
		this.m_HoldProgress.fillAmount = 0f;
	}

	private void SetupIcon()
	{
		if (this.m_TriggerType == HUDTrigger.TriggerType.Additional)
		{
			return;
		}
		if (this.GetTrigger() == null)
		{
			this.m_Icon.gameObject.SetActive(false);
			return;
		}
		string text = this.GetTrigger().GetIconName();
		if (text.Empty())
		{
			text = "Default_Pickup";
		}
		Sprite sprite = null;
		ItemsManager.Get().m_ItemIconsSprites.TryGetValue(text, out sprite);
		if (sprite != null)
		{
			this.m_Icon.sprite = sprite;
			this.m_Icon.gameObject.SetActive(true);
		}
		else
		{
			this.m_Icon.gameObject.SetActive(false);
		}
		ItemAdditionalIcon additionalIcon = this.GetTrigger().GetAdditionalIcon();
		Sprite sprite2 = (additionalIcon == ItemAdditionalIcon.None) ? null : ItemsManager.Get().m_ItemAdditionalIconSprites[additionalIcon];
		if (sprite2 != null)
		{
			this.m_AdditionalIcon.sprite = sprite2;
			this.m_AdditionalIcon.gameObject.SetActive(true);
			return;
		}
		this.m_AdditionalIcon.gameObject.SetActive(false);
	}

	private void SetupName()
	{
		Trigger trigger = this.GetTrigger();
		if (trigger)
		{
			this.m_Name.text = trigger.GetTriggerInfoLocalized();
			this.m_Name.gameObject.SetActive(true);
			if (trigger.IsItem() && Inventory3DManager.Get().gameObject.activeSelf)
			{
				Item item = (Item)trigger;
				if (item.m_InInventory || item.m_InStorage)
				{
					int itemsCount = InventoryBackpack.Get().GetItemsCount(item.GetInfoID());
					Text name = this.m_Name;
					name.text = string.Concat(new object[]
					{
						name.text,
						" (",
						itemsCount,
						")"
					});
					return;
				}
			}
		}
		else
		{
			this.m_Name.gameObject.SetActive(false);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_CurrentTrigger != this.GetTrigger())
		{
			this.m_CanvasGroup.alpha = 0f;
			this.SetupAcre();
			this.SetupIcon();
			this.m_CurrentTrigger = this.GetTrigger();
		}
		this.SetupName();
		this.SetupDurabilityInfo();
		this.SetupConsumableEffects();
		this.SetupRequiredItems();
		this.SetupDurability();
		this.UpdateActions();
		this.UpdateHoldProgress();
		this.UpdateAcre();
		if (this.m_ConsumableEffects)
		{
			Vector3 position = this.m_ConsumableEffects.rectTransform.position;
			position.y = this.m_Durability.rectTransform.position.y;
			if (this.m_DurabilityParent && this.m_DurabilityParent.activeSelf)
			{
				position.y = this.m_ConsumableEffectsDummy.position.y;
			}
			this.m_ConsumableEffects.rectTransform.position = position;
		}
		this.SetupAdditionalInfo();
		if (this.m_CanvasGroup.alpha < 1f)
		{
			this.m_CanvasGroup.alpha += Time.deltaTime * 4f;
		}
		this.m_CanvasGroup.alpha = Mathf.Min(this.m_CanvasGroup.alpha, 1f);
	}

	private void SetupAcre()
	{
		if (!this.m_AcreGroup)
		{
			return;
		}
		Trigger trigger = this.GetTrigger();
		if (trigger == null)
		{
			this.m_AcreGroup.SetActive(false);
			return;
		}
		if (trigger.IsAcre())
		{
			this.m_AcreGroup.SetActive(true);
			return;
		}
		this.m_AcreGroup.SetActive(false);
	}

	private void UpdateAcre()
	{
		if (!this.m_AcreGroup)
		{
			return;
		}
		Trigger trigger = this.GetTrigger();
		if (!trigger.IsAcre())
		{
			this.m_AcreGroup.SetActive(false);
			Color color = this.m_Icon.color;
			color.a = 1f;
			this.m_Icon.color = color;
			return;
		}
		this.m_AcreGroup.SetActive(true);
		Acre acre = (Acre)trigger;
		this.m_AcreHydrationText.text = GreenHellGame.Instance.GetLocalization().Get("HUD_Hydration", true) + ": " + (acre.m_WaterAmount / acre.m_MaxWaterAmount * 100f).ToString("F0") + "%";
		this.m_AcreFertilizerText.text = GreenHellGame.Instance.GetLocalization().Get("HUD_Fertilizer", true) + ": " + (acre.m_FertilizerAmount / acre.m_MaxFertilizerAmount * 100f).ToString("F0") + "%";
		if (acre.m_WaterAmount <= 0f)
		{
			this.m_AcreHydrationIcon.gameObject.SetActive(false);
			this.m_AcreHydrationEmptyIcon.gameObject.SetActive(true);
			this.m_AcreHydrationText.color = Color.red;
		}
		else
		{
			this.m_AcreHydrationIcon.gameObject.SetActive(true);
			this.m_AcreHydrationEmptyIcon.gameObject.SetActive(false);
			this.m_AcreHydrationText.color = Color.white;
		}
		this.m_AcrePlowIcon.SetActive(acre.GetState() == AcreState.NotReady);
		if (acre.GetState() == AcreState.NotReady)
		{
			this.m_AcreSeedicon.SetActive(false);
			this.m_AcreGrowIcon0.SetActive(false);
			this.m_AcreGrowIcon1.SetActive(false);
			this.m_AcreGrowIcon2.SetActive(false);
			this.m_AcreGrowIcon3.SetActive(false);
			this.m_AcreHydrationEmptyIcon.gameObject.SetActive(false);
			this.m_AcreHydrationIcon.gameObject.SetActive(false);
			this.m_AcreHydrationText.gameObject.SetActive(false);
			this.m_AcreFertilizerIcon.gameObject.SetActive(false);
			this.m_AcreFertilizerText.gameObject.SetActive(false);
		}
		else if (acre.GetState() == AcreState.Ready)
		{
			this.m_AcreSeedicon.SetActive(true);
			this.m_AcreGrowIcon0.SetActive(false);
			this.m_AcreGrowIcon1.SetActive(false);
			this.m_AcreGrowIcon2.SetActive(false);
			this.m_AcreGrowIcon3.SetActive(false);
			this.m_AcreHydrationIcon.gameObject.SetActive(true);
			this.m_AcreHydrationText.gameObject.SetActive(true);
			this.m_AcreFertilizerIcon.gameObject.SetActive(true);
			this.m_AcreFertilizerText.gameObject.SetActive(true);
		}
		else if (acre.GetState() == AcreState.Growing || acre.GetState() == AcreState.Grown || acre.GetState() == AcreState.GrownNoFruits)
		{
			if (acre.m_WaterAmount > 0f)
			{
				this.m_AcreHydrationIcon.gameObject.SetActive(true);
			}
			this.m_AcreHydrationText.gameObject.SetActive(true);
			this.m_AcreFertilizerIcon.gameObject.SetActive(true);
			this.m_AcreFertilizerText.gameObject.SetActive(true);
			if (acre.m_NumBuffs == 0)
			{
				this.m_AcreSeedicon.SetActive(false);
				this.m_AcreGrowIcon0.SetActive(true);
				this.m_AcreGrowIcon1.SetActive(false);
				this.m_AcreGrowIcon2.SetActive(false);
				this.m_AcreGrowIcon3.SetActive(false);
			}
			else if (acre.m_NumBuffs == 1)
			{
				this.m_AcreSeedicon.SetActive(false);
				this.m_AcreGrowIcon0.SetActive(false);
				this.m_AcreGrowIcon1.SetActive(true);
				this.m_AcreGrowIcon2.SetActive(false);
				this.m_AcreGrowIcon3.SetActive(false);
			}
			else if (acre.m_NumBuffs == 2)
			{
				this.m_AcreSeedicon.SetActive(false);
				this.m_AcreGrowIcon0.SetActive(false);
				this.m_AcreGrowIcon1.SetActive(false);
				this.m_AcreGrowIcon2.SetActive(true);
				this.m_AcreGrowIcon3.SetActive(false);
			}
			else if (acre.m_NumBuffs == 3)
			{
				this.m_AcreSeedicon.SetActive(false);
				this.m_AcreGrowIcon0.SetActive(false);
				this.m_AcreGrowIcon1.SetActive(false);
				this.m_AcreGrowIcon2.SetActive(false);
				this.m_AcreGrowIcon3.SetActive(true);
			}
		}
		Color color2 = this.m_Icon.color;
		color2.a = 0f;
		this.m_Icon.color = color2;
	}

	public override void UpdateAfterCamera()
	{
		base.UpdateAfterCamera();
		if (this.m_TriggerType == HUDTrigger.TriggerType.Additional)
		{
			return;
		}
		Trigger trigger = this.GetTrigger();
		if (!trigger || !trigger.gameObject.activeSelf)
		{
			return;
		}
		if (!Inventory3DManager.Get().gameObject.activeSelf)
		{
			if (trigger.IsLiquidSource() || trigger.IsLadder())
			{
				this.m_Parent.transform.localPosition = this.m_DefaultPosition;
				return;
			}
			Vector3 a = CameraManager.Get().m_MainCamera.WorldToScreenPoint(trigger.m_Collider ? trigger.m_Collider.bounds.center : trigger.transform.position);
			Vector3 zero = Vector3.zero;
			zero.x = (float)Screen.width * this.m_OffsetMul.x;
			zero.y = (float)Screen.height * this.m_OffsetMul.y;
			this.m_Parent.transform.position = a + zero + trigger.GetHudInfoDisplayOffset();
			return;
		}
		else
		{
			Item item = trigger.IsItem() ? ((Item)trigger) : null;
			if (item)
			{
				Vector3 position = trigger.m_Collider ? trigger.m_Collider.bounds.center : trigger.transform.position;
				Vector3 a2 = Vector3.zero;
				if (item.m_InInventory || item.m_OnCraftingTable || item.m_InStorage)
				{
					a2 = CameraManager.Get().m_MainCamera.ViewportToScreenPoint(Inventory3DManager.Get().m_Camera.WorldToViewportPoint(position));
				}
				else
				{
					a2 = CameraManager.Get().m_MainCamera.WorldToScreenPoint(position);
				}
				Vector3 zero2 = Vector3.zero;
				zero2.x = (float)Screen.width * this.m_OffsetMul.x;
				if (item.m_Info.IsSpear() && item.m_InInventory)
				{
					zero2.y = (float)Screen.height * 0.4f;
				}
				else if (item.m_Info.m_ID == ItemID.Fishing_Rod && item.m_InInventory)
				{
					zero2.y = (float)Screen.height * -0.4f;
				}
				else if (item.m_Info.m_ID == ItemID.Bamboo_Fishing_Rod && item.m_InInventory)
				{
					zero2.y = (float)Screen.height * -0.7f;
				}
				else
				{
					zero2.y = (float)Screen.height * this.m_OffsetMul.y;
				}
				this.m_Parent.transform.position = a2 + zero2 + trigger.GetHudInfoDisplayOffset();
				return;
			}
			if (!this.IsExpanded())
			{
				this.m_Parent.transform.position = Input.mousePosition;
			}
			return;
		}
	}

	private void UpdateActions()
	{
		Trigger trigger = this.GetTrigger();
		if (!trigger || !trigger.CanExecuteActions() || this.IsExpanded())
		{
			for (int i = 0; i < this.m_Actions.Length; i++)
			{
				this.m_KeyFrames[i].gameObject.SetActive(false);
				this.m_Keys[i].gameObject.SetActive(false);
				this.m_Actions[i].gameObject.SetActive(false);
				this.m_PadIcons[i].gameObject.SetActive(false);
				if (this.m_MouseRMBIcon[i])
				{
					this.m_MouseRMBIcon[i].gameObject.SetActive(false);
				}
			}
			return;
		}
		this.m_TriggerActions.Clear();
		if (!this.IsExpanded())
		{
			if (Inventory3DManager.Get().gameObject.activeSelf)
			{
				Item item = trigger.IsItem() ? ((Item)trigger) : null;
				if (item)
				{
					if (item.m_OnCraftingTable)
					{
						this.m_TriggerActions.Add(TriggerAction.TYPE.Remove);
					}
					else if (item.CanShowExpandMenu())
					{
						if (GreenHellGame.IsPadControllerActive())
						{
							this.m_TriggerActions.Add(TriggerAction.TYPE.Pick);
						}
						this.m_TriggerActions.Add(TriggerAction.TYPE.InventoryExpand);
					}
				}
			}
			else
			{
				trigger.GetActions(this.m_TriggerActions);
			}
		}
		Vector3 position = Vector3.zero;
		bool flag = GreenHellGame.IsPadControllerActive();
		int num = 0;
		while (num < this.m_TriggerActions.Count && num < 2)
		{
			this.m_KeyFrames[num].gameObject.SetActive(!flag);
			this.m_Keys[num].gameObject.SetActive(!flag);
			this.m_Actions[num].gameObject.SetActive(true);
			this.m_PadIcons[num].gameObject.SetActive(flag);
			if (this.m_MouseRMBIcon[num])
			{
				this.m_MouseRMBIcon[num].gameObject.SetActive(false);
			}
			this.m_Keys[num].text = string.Empty;
			this.m_Actions[num].text = string.Empty;
			TriggerAction.TYPE type = this.m_TriggerActions[num];
			InputActionData inputActionData = InputsManager.Get().GetInputActionData(type);
			if (inputActionData != null)
			{
				if (flag)
				{
					this.m_PadIcons[num].sprite = inputActionData.m_PadIcon;
				}
				else if (inputActionData.m_KeyCode == KeyCode.Mouse1)
				{
					if (this.m_MouseRMBIcon[num])
					{
						this.m_MouseRMBIcon[num].gameObject.SetActive(true);
						this.m_KeyFrames[num].gameObject.SetActive(false);
					}
					this.m_Keys[num].gameObject.SetActive(false);
				}
				else
				{
					Text text = this.m_Keys[num];
					text.text += KeyCodeToString.GetString(inputActionData.m_KeyCode);
				}
				if (inputActionData.m_Hold > 0f)
				{
					Text text2 = this.m_Actions[num];
					text2.text = text2.text + GreenHellGame.Instance.GetLocalization().Get("HUD_Trigger_Hold", true) + " ";
				}
				Text text3 = this.m_Actions[num];
				text3.text += GreenHellGame.Instance.GetLocalization().Get(TriggerAction.GetText(type), true);
			}
			else
			{
				this.m_Actions[num].text = GreenHellGame.Instance.GetLocalization().Get(TriggerAction.GetText(type), true);
			}
			if (num == 0)
			{
				TextGenerationSettings generationSettings = this.m_Actions[num].GetGenerationSettings(this.m_Actions[num].rectTransform.rect.size);
				float width = ((RectTransform)HUDManager.Get().m_CanvasGameObject.transform).rect.width;
				float x = ((RectTransform)HUDManager.Get().m_CanvasGameObject.transform).localScale.x;
				generationSettings.scaleFactor = x;
				float preferredWidth = this.m_TextGen.GetPreferredWidth(this.m_Actions[num].text, generationSettings);
				position = this.m_KeyFrames[num].rectTransform.position;
				position.x += this.m_KeyFrames[num].rectTransform.rect.width * 0.5f * x;
				position.x += preferredWidth + width * x * 0.01f;
				position.x += this.m_KeyFrames[num].rectTransform.rect.width * x;
			}
			else if (num == 1 && this.m_KeyFrameParents[num] != null)
			{
				this.m_KeyFrameParents[num].position = position;
			}
			num++;
		}
		for (int j = this.m_TriggerActions.Count; j < this.m_Actions.Length; j++)
		{
			this.m_KeyFrames[j].gameObject.SetActive(false);
			this.m_Keys[j].gameObject.SetActive(false);
			this.m_Actions[j].gameObject.SetActive(false);
			this.m_PadIcons[j].gameObject.SetActive(false);
			if (this.m_MouseRMBIcon[j] != null)
			{
				this.m_MouseRMBIcon[j].gameObject.SetActive(false);
			}
		}
	}

	private void UpdateHoldProgress()
	{
		Trigger trigger = this.GetTrigger();
		if (trigger == null || !trigger.CanExecuteActions() || this.IsExpanded())
		{
			this.m_HoldProgress.fillAmount = 0f;
			if (this.m_LastHoldProgress != 0f)
			{
				TriggerController.Get().OnTriggerHoldEnd(TriggerAction.TYPE.None);
			}
			this.m_LastHoldProgress = 0f;
			return;
		}
		List<TriggerAction.TYPE> list = new List<TriggerAction.TYPE>();
		trigger.GetActions(list);
		float fillAmount = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			float actionHoldProgress = InputsManager.Get().GetActionHoldProgress(list[i]);
			if (actionHoldProgress > 0f)
			{
				if (this.m_LastHoldProgress == 0f)
				{
					TriggerController.Get().OnTriggerHoldStart(list[i]);
				}
				this.m_LastHoldProgress = actionHoldProgress;
				fillAmount = actionHoldProgress;
				break;
			}
			if (this.m_LastHoldProgress != 0f)
			{
				TriggerController.Get().OnTriggerHoldEnd(list[i]);
				this.m_LastHoldProgress = actionHoldProgress;
			}
		}
		this.m_HoldProgress.fillAmount = fillAmount;
	}

	private void SetupDurabilityInfo()
	{
		if (!this.m_DurabilityParent)
		{
			return;
		}
		this.m_DurabilityParent.SetActive(false);
		if (!Inventory3DManager.Get().gameObject.activeSelf || this.IsExpanded())
		{
			return;
		}
		Trigger trigger = this.GetTrigger();
		if (trigger == null || !trigger.IsItem())
		{
			return;
		}
		Item item = (Item)trigger;
		if (item.m_Info.IsFood())
		{
			Food food = (Food)item;
			if (food.CanSpoil())
			{
				this.m_DurabilityParent.SetActive(true);
				this.m_DurabilityName.text = GreenHellGame.Instance.GetLocalization().Get("HUDTrigger_Decay", true);
				float num = food.m_FInfo.m_SpoilOnlyIfTriggered ? food.m_FirstTriggerTime : food.m_FInfo.m_CreationTime;
				float num2 = food.m_FInfo.m_SpoilTime - (MainLevel.Instance.m_TODSky.Cycle.GameTime - num);
				float num3 = num2 % 1f;
				float num4 = num2 - num3;
				float num5 = Mathf.Floor(num4 / 24f);
				num4 -= num5 * 24f;
				num3 *= 60f;
				this.m_Durability.text = string.Concat(new string[]
				{
					num5.ToString("F0"),
					"d ",
					num4.ToString("F0"),
					"h ",
					num3.ToString("F0"),
					"m"
				});
			}
			return;
		}
		if (item.m_Info.IsWeapon() || item.m_Info.IsTool() || (item.m_Info.IsArmor() && item.m_Info.m_ID != ItemID.broken_armor))
		{
			this.m_DurabilityParent.SetActive(true);
			this.m_DurabilityName.text = GreenHellGame.Instance.GetLocalization().Get("HUDTrigger_Durability", true);
			this.m_Durability.text = (item.m_Info.m_Health / item.m_Info.m_MaxHealth * 100f).ToString("F0") + "%";
			return;
		}
	}

	private void SetupConsumableEffects()
	{
		if (!this.m_ConsumableEffects)
		{
			return;
		}
		if (!Inventory3DManager.Get().gameObject.activeSelf || this.IsExpanded())
		{
			this.m_ConsumableEffects.gameObject.SetActive(false);
			return;
		}
		Trigger trigger = this.GetTrigger();
		if (trigger == null || !trigger.IsItem())
		{
			this.m_ConsumableEffects.gameObject.SetActive(false);
			return;
		}
		Item item = (Item)trigger;
		if (!item.m_Info.IsConsumable() && !item.m_Info.IsLiquidContainer())
		{
			this.m_ConsumableEffects.gameObject.SetActive(false);
			return;
		}
		int num = 0;
		if (item.m_Info.IsConsumable())
		{
			if (!ItemsManager.Get().WasConsumed(item.m_Info.m_ID))
			{
				this.m_UnknownEffect.SetActive(true);
			}
			else
			{
				this.m_UnknownEffect.SetActive(false);
				ConsumableInfo consumableInfo = (ConsumableInfo)item.m_Info;
				if (consumableInfo.m_Proteins > 0f)
				{
					this.SetupEffect("Watch_protein_icon", IconColors.GetColor(IconColors.Icon.Proteins), consumableInfo.m_Proteins, "HUD_Nutrition_Protein", ref num, -1f);
				}
				if (consumableInfo.m_Fat > 0f)
				{
					this.SetupEffect("Watch_fat_icon", IconColors.GetColor(IconColors.Icon.Fat), consumableInfo.m_Fat, "HUD_Nutrition_Fat", ref num, -1f);
				}
				if (consumableInfo.m_Carbohydrates > 0f)
				{
					this.SetupEffect("Watch_carbo_icon", IconColors.GetColor(IconColors.Icon.Carbo), consumableInfo.m_Carbohydrates, "HUD_Nutrition_Carbo", ref num, -1f);
				}
				if (consumableInfo.m_Water > 0f)
				{
					this.SetupEffect("Watch_water_icon", IconColors.GetColor(IconColors.Icon.Hydration), consumableInfo.m_Water, "HUD_Hydration", ref num, -1f);
				}
				if (consumableInfo.m_AddEnergy > 0f)
				{
					this.SetupEffect("Energy_icon", Color.white, consumableInfo.m_AddEnergy, "HUD_Energy", ref num, -1f);
				}
				if ((float)consumableInfo.m_SanityChange != 0f)
				{
					this.SetupEffect("", Color.white, (float)consumableInfo.m_SanityChange, "HUD_Sanity", ref num, -1f);
				}
				if (consumableInfo.m_ConsumeEffect == ConsumeEffect.FoodPoisoning)
				{
					this.SetupEffect("Vomit_icon_H", Color.white, (float)consumableInfo.m_ConsumeEffectLevel, "HUD_FoodPoisoning", ref num, -1f);
				}
				else if (consumableInfo.m_ConsumeEffect == ConsumeEffect.ParasiteSickness)
				{
					this.SetupEffect("ParasiteSichness_icon_H", Color.white, (float)consumableInfo.m_ConsumeEffectLevel, "HUD_ParasiteSickness", ref num, -1f);
				}
			}
		}
		else if (item.m_Info.IsLiquidContainer())
		{
			LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)item.m_Info;
			if (liquidContainerInfo.m_Amount > 0f)
			{
				LiquidData liquidData = LiquidManager.Get().GetLiquidData(liquidContainerInfo.m_LiquidType);
				if (liquidContainerInfo.m_Amount >= 1f)
				{
					this.SetupEffect("Watch_water_icon", IconColors.GetColor(IconColors.Icon.Hydration), liquidContainerInfo.m_Amount, "HUD_Hydration", ref num, liquidContainerInfo.m_Capacity);
				}
				if (liquidData.m_Energy > 0f)
				{
					this.SetupEffect("Energy_icon", Color.white, liquidData.m_Energy, "HUD_Energy", ref num, -1f);
				}
				for (int i = 0; i < liquidData.m_ConsumeEffects.Count; i++)
				{
					if (liquidData.m_ConsumeEffects[i].m_ConsumeEffect == ConsumeEffect.FoodPoisoning)
					{
						this.SetupEffect("Vomit_icon_H", Color.white, (float)liquidData.m_ConsumeEffects[i].m_ConsumeEffectLevel, "HUD_FoodPoisoning", ref num, -1f);
					}
				}
				if (liquidContainerInfo.IsBowl())
				{
					if (liquidData.m_Proteins > 0f)
					{
						this.SetupEffect("Watch_protein_icon", IconColors.GetColor(IconColors.Icon.Proteins), liquidData.m_Proteins, "HUD_Nutrition_Protein", ref num, -1f);
					}
					if (liquidData.m_Fat > 0f)
					{
						this.SetupEffect("Watch_fat_icon", IconColors.GetColor(IconColors.Icon.Fat), liquidData.m_Fat, "HUD_Nutrition_Fat", ref num, -1f);
					}
					if (liquidData.m_Carbohydrates > 0f)
					{
						this.SetupEffect("Watch_carbo_icon", IconColors.GetColor(IconColors.Icon.Carbo), liquidData.m_Carbohydrates, "HUD_Nutrition_Carbo", ref num, -1f);
					}
				}
			}
			this.m_UnknownEffect.SetActive(num == 0);
		}
		for (int j = num; j < this.m_EffectsData.Count; j++)
		{
			this.m_EffectsData[j].m_Parent.SetActive(false);
		}
		this.m_ConsumableEffects.gameObject.SetActive(true);
	}

	private void SetupEffect(string icon_name, Color icon_color, float value, string name, ref int index, float max_value = -1f)
	{
		TriggerCEData triggerCEData = this.m_EffectsData[index];
		Sprite sprite = null;
		ItemsManager.Get().m_ItemIconsSprites.TryGetValue(icon_name, out sprite);
		if (!sprite)
		{
			this.m_SpritesMap.TryGetValue(icon_name, out sprite);
		}
		if (sprite)
		{
			triggerCEData.m_Icon.sprite = sprite;
			triggerCEData.m_Icon.color = icon_color;
			triggerCEData.m_Icon.gameObject.SetActive(true);
		}
		else
		{
			triggerCEData.m_Icon.gameObject.SetActive(false);
		}
		triggerCEData.m_Text.text = Mathf.FloorToInt(value).ToString();
		if (max_value >= 0f)
		{
			Text text = triggerCEData.m_Text;
			text.text = text.text + "/" + Mathf.FloorToInt(max_value).ToString();
		}
		Text text2 = triggerCEData.m_Text;
		text2.text = text2.text + " " + GreenHellGame.Instance.GetLocalization().Get(name, true);
		triggerCEData.m_Parent.SetActive(true);
		Vector3 localPosition = triggerCEData.m_Parent.transform.localPosition;
		localPosition.y = -8f * (float)index;
		triggerCEData.m_Parent.transform.localPosition = localPosition;
		index++;
	}

	private void SetupRequiredItems()
	{
		if (!this.m_RequiredItems)
		{
			return;
		}
		if (this.IsExpanded())
		{
			this.m_RequiredItems.gameObject.SetActive(false);
			return;
		}
		Trigger trigger = this.GetTrigger();
		if (trigger == null || trigger.m_RequiredItems.Count == 0)
		{
			this.m_RequiredItems.gameObject.SetActive(false);
			return;
		}
		if (trigger.CanExecuteActions())
		{
			this.m_RequiredItems.gameObject.SetActive(false);
			return;
		}
		if (trigger.m_CantExecuteActionsDuringDialog && DialogsManager.Get().IsAnyDialogPlaying())
		{
			this.m_RequiredItems.gameObject.SetActive(false);
			return;
		}
		int num = 0;
		foreach (ItemID id in trigger.m_RequiredItems)
		{
			TriggerRIData triggerRIData = this.m_RequiredItemsData[num];
			ItemInfo info = ItemsManager.Get().GetInfo(id);
			if (info != null)
			{
				string iconName = info.m_IconName;
				Sprite sprite = null;
				ItemsManager.Get().m_ItemIconsSprites.TryGetValue(iconName, out sprite);
				if (!sprite)
				{
					this.m_SpritesMap.TryGetValue(iconName, out sprite);
				}
				if (sprite)
				{
					triggerRIData.m_Icon.sprite = sprite;
					triggerRIData.m_Icon.gameObject.SetActive(true);
				}
				else
				{
					triggerRIData.m_Icon.gameObject.SetActive(false);
				}
				triggerRIData.m_Text.text = GreenHellGame.Instance.GetLocalization().Get(id.ToString(), true);
				triggerRIData.m_Parent.SetActive(true);
				Vector3 localPosition = triggerRIData.m_Parent.transform.localPosition;
				localPosition.y = -8f * (float)num;
				triggerRIData.m_Parent.transform.localPosition = localPosition;
				if (Player.Get().HaveItem(id))
				{
					triggerRIData.m_Text.color = Color.white;
				}
				else
				{
					triggerRIData.m_Text.color = Color.red;
				}
				num++;
			}
		}
		for (int i = num; i < this.m_RequiredItemsData.Count; i++)
		{
			this.m_RequiredItemsData[i].m_Parent.SetActive(false);
		}
		this.m_RequiredItems.gameObject.SetActive(true);
	}

	private void SetupDurability()
	{
	}

	private void SetupAdditionalInfo()
	{
		if (this.m_AdditionalInfo == null)
		{
			return;
		}
		Trigger trigger = this.GetTrigger();
		if (trigger == null || !trigger.ShowAdditionalInfo() || this.IsExpanded())
		{
			this.m_AdditionalInfo.gameObject.SetActive(false);
			return;
		}
		this.m_AdditionalInfo.gameObject.SetActive(true);
		this.m_AdditionalInfo.text = trigger.GetAdditionalInfoLocalized();
	}

	private bool IsExpanded()
	{
		return HUDItem.Get().enabled && this.GetTrigger() && ((HUDItem.Get().m_Item && HUDItem.Get().m_Item.gameObject == this.GetTrigger().gameObject) || (HUDItem.Get().m_LiquidSource && HUDItem.Get().m_LiquidSource.gameObject == this.GetTrigger().gameObject) || (HUDItem.Get().m_PlantFruit && HUDItem.Get().m_PlantFruit.gameObject == this.GetTrigger().gameObject) || (HUDItem.Get().m_ItemReplacer && HUDItem.Get().m_ItemReplacer.gameObject == this.GetTrigger().gameObject));
	}

	public HUDTrigger.TriggerType m_TriggerType;

	[HideInInspector]
	public GameObject m_Parent;

	private Image m_Icon;

	private Image m_AdditionalIcon;

	private Text m_Name;

	private Image m_HoldProgress;

	private Vector3 m_DefaultPosition = Vector3.zero;

	private Trigger m_CurrentTrigger;

	private const int m_MaxActionsNum = 2;

	private RawImage[] m_KeyFrames = new RawImage[2];

	private Text[] m_Keys = new Text[2];

	private RawImage[] m_MouseRMBIcon = new RawImage[2];

	private Text[] m_Actions = new Text[2];

	private Image[] m_PadIcons = new Image[2];

	private RectTransform[] m_KeyFrameParents = new RectTransform[2];

	private TextGenerator m_TextGen;

	private static HUDTrigger s_NormalInstance;

	private static HUDTrigger s_AdditionalInstance;

	private Text m_ConsumableEffects;

	private Transform m_ConsumableEffectsDummy;

	private List<TriggerCEData> m_EffectsData = new List<TriggerCEData>();

	private GameObject m_UnknownEffect;

	private Text m_RequiredItems;

	private List<TriggerRIData> m_RequiredItemsData = new List<TriggerRIData>();

	public List<Sprite> m_Sprites;

	private Dictionary<string, Sprite> m_SpritesMap = new Dictionary<string, Sprite>();

	public GameObject m_DurabilityParent;

	public Text m_Durability;

	public Text m_DurabilityName;

	private CanvasGroup m_CanvasGroup;

	private Text m_AdditionalInfo;

	public GameObject m_AcreGroup;

	public Image m_AcreHydrationIcon;

	public Image m_AcreHydrationEmptyIcon;

	public Text m_AcreHydrationText;

	public Image m_AcreFertilizerIcon;

	public Text m_AcreFertilizerText;

	public GameObject m_AcreSeedicon;

	public GameObject m_AcreGrowIcon0;

	public GameObject m_AcreGrowIcon1;

	public GameObject m_AcreGrowIcon2;

	public GameObject m_AcreGrowIcon3;

	private GameObject m_AcrePlowIcon;

	public Vector3 m_OffsetMul = Vector3.zero;

	private List<TriggerAction.TYPE> m_TriggerActions = new List<TriggerAction.TYPE>(10);

	private float m_LastHoldProgress;

	public enum TriggerType
	{
		Normal,
		Additional
	}
}
