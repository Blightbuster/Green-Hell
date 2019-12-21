using System;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class HUDBodyInspection : HUDBase, IInputsReceiver
{
	public static HUDBodyInspection Get()
	{
		return HUDBodyInspection.s_Instance;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override void Awake()
	{
		base.Awake();
		HUDBodyInspection.s_Instance = this;
		for (int i = 0; i < 4; i++)
		{
			RawImage[] limbSelections = this.m_LimbSelections;
			int num = i;
			Transform transform = base.transform;
			Limb limb = (Limb)i;
			limbSelections[num] = transform.FindDeepChild(limb.ToString()).gameObject.GetComponent<RawImage>();
			RawImage[] limbCurrentSelections = this.m_LimbCurrentSelections;
			int num2 = i;
			Transform transform2 = base.transform;
			limb = (Limb)i;
			limbCurrentSelections[num2] = transform2.FindDeepChild(limb.ToString() + "_Selected").gameObject.GetComponent<RawImage>();
			this.m_LimbCurrentSelections[i].enabled = false;
			this.m_SelectionColliders[i] = this.m_LimbSelections[i].GetComponent<PolygonCollider2D>();
			this.m_ArmorSmallIcons[i] = new ArmorSmallIcon();
			this.m_ArmorSmallIcons[i].m_Limb = (Limb)i;
			ArmorSmallIcon armorSmallIcon = this.m_ArmorSmallIcons[i];
			Transform transform3 = base.transform;
			string str = "Armor_";
			limb = (Limb)i;
			armorSmallIcon.m_On = transform3.FindDeepChild(str + limb.ToString() + "_On").gameObject.GetComponent<RawImage>();
			ArmorSmallIcon armorSmallIcon2 = this.m_ArmorSmallIcons[i];
			Transform transform4 = base.transform;
			string str2 = "Armor_";
			limb = (Limb)i;
			armorSmallIcon2.m_Off = transform4.FindDeepChild(str2 + limb.ToString() + "_Off").gameObject.GetComponent<RawImage>();
		}
		this.m_TextGen = new TextGenerator();
		this.m_ArmorBG.gameObject.SetActive(false);
		this.m_ArmorTooltip.SetActive(false);
		this.m_ArmorTooltipText = General.GetComponentDeepChild<Text>(this.m_ArmorTooltip);
		this.m_ArmorTooltipShow = GreenHellGame.Instance.GetLocalization().Get("ArmorShow", true);
		this.m_ArmorTooltipHide = GreenHellGame.Instance.GetLocalization().Get("ArmorHide", true);
		this.m_ArmorEnabled = true;
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.OnClickLimb(Limb.LArm);
		this.m_LimbCurrentSelections[0].enabled = true;
		this.m_HintBG.gameObject.SetActive(true);
		this.m_HintText.gameObject.SetActive(true);
		PlayerArmorModule.Get().SetMeshesVisible(this.m_ArmorEnabled);
		if (this.m_ArmorEnabled)
		{
			BodyInspectionController.Get().OnArmorMeshesEnabled();
		}
		else
		{
			BodyInspectionController.Get().OnArmorMeshesDisabled();
		}
		this.m_ArmorBG.gameObject.SetActive(true);
		this.m_BackpackHint.SetActive(GreenHellGame.IsPadControllerActive() && !Inventory3DManager.Get().IsActive());
		this.m_SortBackpackHint.SetActive(GreenHellGame.IsPadControllerActive() && Inventory3DManager.Get().IsActive());
		TextGenerationSettings generationSettings = this.m_SelectLimbText.GetGenerationSettings(this.m_SelectLimbText.rectTransform.rect.size);
		generationSettings.scaleFactor = 1f;
		float preferredWidth = this.m_TextGen.GetPreferredWidth(this.m_SelectLimbText.text, generationSettings);
		this.m_SelectLimgBGSize.Set(Mathf.Max(99.75f, preferredWidth + this.m_SelectLimbW), this.m_SelectLimbBG.rectTransform.rect.size.y);
		this.m_SelectLimbBG.rectTransform.sizeDelta = this.m_SelectLimgBGSize;
		preferredWidth = this.m_TextGen.GetPreferredWidth(this.m_RotateLimbText.text, generationSettings);
		this.m_RotateLimgBGSize.Set(Mathf.Max(80f, preferredWidth + this.m_SelectLimbW), this.m_RotateLimbBG.rectTransform.rect.size.y);
		this.m_RotateLimbBG.rectTransform.sizeDelta = this.m_RotateLimgBGSize;
	}

	protected override void OnHide()
	{
		base.OnHide();
		this.m_HintBG.gameObject.SetActive(false);
		this.m_HintText.gameObject.SetActive(false);
		PlayerArmorModule.Get().SetMeshesVisible(true);
		BodyInspectionController.Get().OnArmorMeshesEnabled();
		this.m_ArmorBG.gameObject.SetActive(false);
	}

	protected override bool ShouldShow()
	{
		return !(BodyInspectionController.Get() == null) && BodyInspectionController.Get().enabled && (BodyInspectionController.Get().m_State == BIState.ChooseLimb || BodyInspectionController.Get().m_State == BIState.RotateLeftArm || BodyInspectionController.Get().m_State == BIState.RotateRightArm || BodyInspectionController.Get().m_State == BIState.RotateLeftLeg || BodyInspectionController.Get().m_State == BIState.RotateRightLeg);
	}

	private void SelectLimb(Limb limb)
	{
		for (int i = 0; i < 4; i++)
		{
			this.m_LimbSelections[i].enabled = (limb == (Limb)i);
		}
	}

	public Limb GetSelectedLimb()
	{
		int num = 0;
		RawImage[] limbCurrentSelections = this.m_LimbCurrentSelections;
		for (int i = 0; i < limbCurrentSelections.Length; i++)
		{
			if (limbCurrentSelections[i].enabled)
			{
				return (Limb)num;
			}
			num++;
		}
		return Limb.None;
	}

	private void OnClickLimb(Limb limb)
	{
		switch (limb)
		{
		case Limb.LArm:
		{
			PlayerAudioModule.Get().PlayBILeftArmStart();
			RawImage[] limbCurrentSelections = this.m_LimbCurrentSelections;
			for (int i = 0; i < limbCurrentSelections.Length; i++)
			{
				limbCurrentSelections[i].enabled = false;
			}
			this.m_LimbCurrentSelections[0].enabled = true;
			return;
		}
		case Limb.RArm:
		{
			PlayerAudioModule.Get().PlayBIRightArmStart();
			RawImage[] limbCurrentSelections = this.m_LimbCurrentSelections;
			for (int i = 0; i < limbCurrentSelections.Length; i++)
			{
				limbCurrentSelections[i].enabled = false;
			}
			this.m_LimbCurrentSelections[1].enabled = true;
			return;
		}
		case Limb.LLeg:
		{
			PlayerAudioModule.Get().PlayBILeftLegStart();
			RawImage[] limbCurrentSelections = this.m_LimbCurrentSelections;
			for (int i = 0; i < limbCurrentSelections.Length; i++)
			{
				limbCurrentSelections[i].enabled = false;
			}
			this.m_LimbCurrentSelections[2].enabled = true;
			return;
		}
		case Limb.RLeg:
		{
			PlayerAudioModule.Get().PlayBIRightLegStart();
			RawImage[] limbCurrentSelections = this.m_LimbCurrentSelections;
			for (int i = 0; i < limbCurrentSelections.Length; i++)
			{
				limbCurrentSelections[i].enabled = false;
			}
			this.m_LimbCurrentSelections[3].enabled = true;
			return;
		}
		default:
			return;
		}
	}

	protected override void Update()
	{
		base.Update();
		this.m_BackpackHint.SetActive(GreenHellGame.IsPadControllerActive() && !Inventory3DManager.Get().IsActive());
		this.m_SortBackpackHint.SetActive(GreenHellGame.IsPadControllerActive() && Inventory3DManager.Get().IsActive());
		Limb limb = Limb.None;
		if (GreenHellGame.IsPCControllerActive())
		{
			for (int i = 0; i < 4; i++)
			{
				if (this.m_SelectionColliders[i].OverlapPoint(Input.mousePosition))
				{
					limb = (Limb)i;
					break;
				}
			}
		}
		else if (!HUDItem.Get().enabled)
		{
			float axis = CrossPlatformInputManager.GetAxis("LeftStickX");
			float axis2 = CrossPlatformInputManager.GetAxis("LeftStickY");
			Vector2 zero = Vector2.zero;
			zero.x = axis;
			zero.y = axis2;
			if (zero.magnitude > 0.08f)
			{
				float num = Vector3.Angle(zero, Vector3.up);
				if (axis > 0f)
				{
					num = 360f - num;
				}
				if (num <= 90f)
				{
					limb = Limb.RArm;
				}
				else if (num > 90f && num <= 180f)
				{
					limb = Limb.RLeg;
				}
				else if (num > 180f && num <= 270f)
				{
					limb = Limb.LLeg;
				}
				else if (num > 270f)
				{
					limb = Limb.LArm;
				}
			}
		}
		this.SelectLimb(limb);
		if ((GreenHellGame.IsPCControllerActive() && Input.GetMouseButtonDown(0)) || (GreenHellGame.IsPadControllerActive() && Input.GetKeyDown(InputHelpers.PadButton.L3.KeyFromPad())))
		{
			this.OnClickLimb(limb);
			switch (limb)
			{
			case Limb.LArm:
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbX = -1f;
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbY = 1f;
				break;
			case Limb.RArm:
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbX = 1f;
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbY = 1f;
				break;
			case Limb.LLeg:
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbX = -1f;
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbY = -1f;
				break;
			case Limb.RLeg:
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbX = 1f;
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbY = -1f;
				break;
			}
		}
		this.UpdateArmor();
		this.UpdateSmallIcons();
		this.UpdateArmorTooltip();
	}

	private void UpdateArmor()
	{
		if (this.m_ArmorCollider.OverlapPoint(Input.mousePosition))
		{
			this.m_ArmorHL.gameObject.SetActive(true);
			if (Input.GetMouseButtonDown(0))
			{
				this.m_ArmorEnabled = !this.m_ArmorEnabled;
				PlayerArmorModule.Get().SetMeshesVisible(this.m_ArmorEnabled);
				if (this.m_ArmorEnabled)
				{
					BodyInspectionController.Get().OnArmorMeshesEnabled();
				}
				else
				{
					BodyInspectionController.Get().OnArmorMeshesDisabled();
				}
			}
		}
		else
		{
			this.m_ArmorHL.gameObject.SetActive(false);
		}
		if (this.m_ArmorEnabled)
		{
			this.m_ArmorOff.gameObject.SetActive(false);
			this.m_ArmorOn.gameObject.SetActive(true);
			return;
		}
		this.m_ArmorOff.gameObject.SetActive(true);
		this.m_ArmorOn.gameObject.SetActive(false);
	}

	private void UpdateSmallIcons()
	{
		for (int i = 0; i < 4; i++)
		{
			if (PlayerArmorModule.Get().GetArmorData((Limb)i).m_ArmorType != ArmorType.None)
			{
				if (this.m_ArmorEnabled)
				{
					this.m_ArmorSmallIcons[i].On();
				}
				else
				{
					this.m_ArmorSmallIcons[i].Off();
				}
			}
			else
			{
				this.m_ArmorSmallIcons[i].Disable();
			}
		}
	}

	private void UpdateArmorTooltip()
	{
		if (!this.m_ArmorCollider.OverlapPoint(Input.mousePosition))
		{
			this.m_ArmorTooltip.SetActive(false);
			return;
		}
		this.m_ArmorTooltip.SetActive(true);
		this.m_ArmorTooltip.transform.position = Input.mousePosition;
		if (this.m_ArmorTooltipLastEnabled < 0 || (this.m_ArmorTooltipLastEnabled == 0 && this.m_ArmorEnabled))
		{
			this.m_ArmorTooltipText.text = this.m_ArmorTooltipHide;
		}
		else if (this.m_ArmorTooltipLastEnabled < 0 || (this.m_ArmorTooltipLastEnabled == 1 && !this.m_ArmorEnabled))
		{
			this.m_ArmorTooltipText.text = this.m_ArmorTooltipShow;
		}
		if (this.m_ArmorEnabled)
		{
			this.m_ArmorTooltipLastEnabled = 1;
			return;
		}
		this.m_ArmorTooltipLastEnabled = 0;
	}

	public void OnInputAction(InputActionData action_data)
	{
		if (action_data.m_Action == InputsManager.InputAction.BIShowHideArmor)
		{
			this.m_ArmorEnabled = !this.m_ArmorEnabled;
			PlayerArmorModule.Get().SetMeshesVisible(this.m_ArmorEnabled);
			if (this.m_ArmorEnabled)
			{
				BodyInspectionController.Get().OnArmorMeshesEnabled();
			}
			else
			{
				BodyInspectionController.Get().OnArmorMeshesDisabled();
			}
			if (this.m_ArmorEnabled)
			{
				this.m_ArmorOff.gameObject.SetActive(false);
				this.m_ArmorOn.gameObject.SetActive(true);
				return;
			}
			this.m_ArmorOff.gameObject.SetActive(true);
			this.m_ArmorOn.gameObject.SetActive(false);
		}
	}

	public bool CanReceiveAction()
	{
		return base.enabled;
	}

	public bool CanReceiveActionPaused()
	{
		return false;
	}

	private RawImage[] m_LimbSelections = new RawImage[4];

	private RawImage[] m_LimbCurrentSelections = new RawImage[4];

	private PolygonCollider2D[] m_SelectionColliders = new PolygonCollider2D[4];

	private ArmorSmallIcon[] m_ArmorSmallIcons = new ArmorSmallIcon[4];

	private static HUDBodyInspection s_Instance;

	public Text m_HintText;

	public RawImage m_HintBG;

	private TextGenerator m_TextGen;

	public RawImage m_ArmorBG;

	public RawImage m_ArmorOff;

	public RawImage m_ArmorOn;

	public RawImage m_ArmorHL;

	public Collider2D m_ArmorCollider;

	[HideInInspector]
	public bool m_ArmorEnabled;

	public GameObject m_ArmorTooltip;

	private Text m_ArmorTooltipText;

	private string m_ArmorTooltipShow = string.Empty;

	private string m_ArmorTooltipHide = string.Empty;

	public GameObject m_BackpackHint;

	public GameObject m_SortBackpackHint;

	public RawImage m_SelectLimbBG;

	public Text m_SelectLimbText;

	public RawImage m_RotateLimbBG;

	public Text m_RotateLimbText;

	private Vector2 m_SelectLimgBGSize;

	private Vector2 m_RotateLimgBGSize;

	public float m_SelectLimbW = 50f;

	public float m_RotateLimbW = 50f;

	private int m_ArmorTooltipLastEnabled = -1;
}
