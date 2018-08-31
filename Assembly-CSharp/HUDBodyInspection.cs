using System;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class HUDBodyInspection : HUDBase
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
			HUDBodyInspection.Limb limb = (HUDBodyInspection.Limb)i;
			limbSelections[num] = transform.FindDeepChild(limb.ToString()).gameObject.GetComponent<RawImage>();
			RawImage[] limbCurrentSelections = this.m_LimbCurrentSelections;
			int num2 = i;
			Transform transform2 = base.transform;
			HUDBodyInspection.Limb limb2 = (HUDBodyInspection.Limb)i;
			limbCurrentSelections[num2] = transform2.FindDeepChild(limb2.ToString() + "_Selected").gameObject.GetComponent<RawImage>();
			this.m_LimbCurrentSelections[i].enabled = false;
			this.m_SelectionColliders[i] = this.m_LimbSelections[i].GetComponent<PolygonCollider2D>();
		}
		this.m_TextGen = new TextGenerator();
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.OnClickLimb(HUDBodyInspection.Limb.LArm);
		this.m_LimbCurrentSelections[0].enabled = true;
		this.m_HintBG.gameObject.SetActive(true);
		this.m_HintText.gameObject.SetActive(true);
	}

	protected override void OnHide()
	{
		base.OnHide();
		this.m_HintBG.gameObject.SetActive(false);
		this.m_HintText.gameObject.SetActive(false);
	}

	protected override bool ShouldShow()
	{
		return !(BodyInspectionController.Get() == null) && BodyInspectionController.Get().enabled && (BodyInspectionController.Get().m_State == BIState.ChooseLimb || BodyInspectionController.Get().m_State == BIState.RotateLeftArm || BodyInspectionController.Get().m_State == BIState.RotateRightArm || BodyInspectionController.Get().m_State == BIState.RotateLeftLeg || BodyInspectionController.Get().m_State == BIState.RotateRightLeg);
	}

	private void SelectLimb(HUDBodyInspection.Limb limb)
	{
		for (int i = 0; i < 4; i++)
		{
			this.m_LimbSelections[i].enabled = (limb == (HUDBodyInspection.Limb)i);
		}
	}

	private void OnClickLimb(HUDBodyInspection.Limb limb)
	{
		switch (limb)
		{
		case HUDBodyInspection.Limb.LArm:
			PlayerAudioModule.Get().PlayBILeftArmStart();
			foreach (RawImage rawImage in this.m_LimbCurrentSelections)
			{
				rawImage.enabled = false;
			}
			this.m_LimbCurrentSelections[0].enabled = true;
			break;
		case HUDBodyInspection.Limb.RArm:
			PlayerAudioModule.Get().PlayBIRightArmStart();
			foreach (RawImage rawImage2 in this.m_LimbCurrentSelections)
			{
				rawImage2.enabled = false;
			}
			this.m_LimbCurrentSelections[1].enabled = true;
			break;
		case HUDBodyInspection.Limb.LLeg:
			PlayerAudioModule.Get().PlayBILeftLegStart();
			foreach (RawImage rawImage3 in this.m_LimbCurrentSelections)
			{
				rawImage3.enabled = false;
			}
			this.m_LimbCurrentSelections[2].enabled = true;
			break;
		case HUDBodyInspection.Limb.RLeg:
			PlayerAudioModule.Get().PlayBIRightLegStart();
			foreach (RawImage rawImage4 in this.m_LimbCurrentSelections)
			{
				rawImage4.enabled = false;
			}
			this.m_LimbCurrentSelections[3].enabled = true;
			break;
		}
	}

	protected override void Update()
	{
		base.Update();
		HUDBodyInspection.Limb limb = HUDBodyInspection.Limb.None;
		for (int i = 0; i < 4; i++)
		{
			if (this.m_SelectionColliders[i].OverlapPoint(Input.mousePosition))
			{
				limb = (HUDBodyInspection.Limb)i;
				break;
			}
		}
		this.SelectLimb(limb);
		if (Input.GetMouseButtonDown(0))
		{
			this.OnClickLimb(limb);
			switch (limb)
			{
			case HUDBodyInspection.Limb.LArm:
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbX = -1f;
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbY = 1f;
				break;
			case HUDBodyInspection.Limb.RArm:
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbX = 1f;
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbY = 1f;
				break;
			case HUDBodyInspection.Limb.LLeg:
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbX = -1f;
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbY = -1f;
				break;
			case HUDBodyInspection.Limb.RLeg:
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbX = 1f;
				BodyInspectionController.Get().m_Inputs.m_ChooseLimbY = -1f;
				break;
			}
		}
	}

	private RawImage[] m_LimbSelections = new RawImage[4];

	private RawImage[] m_LimbCurrentSelections = new RawImage[4];

	private PolygonCollider2D[] m_SelectionColliders = new PolygonCollider2D[4];

	private static HUDBodyInspection s_Instance;

	public Text m_HintText;

	public RawImage m_HintBG;

	private TextGenerator m_TextGen;

	private enum Limb
	{
		None = -1,
		LArm,
		RArm,
		LLeg,
		RLeg
	}
}
