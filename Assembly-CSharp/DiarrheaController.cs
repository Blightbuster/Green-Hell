using System;
using UnityEngine;

public class DiarrheaController : PlayerController
{
	public static DiarrheaController Get()
	{
		return DiarrheaController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		DiarrheaController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.Diarrhea;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Player.BlockMoves();
		if (this.m_ChatterName != string.Empty)
		{
			ChatterManager.Get().Play(this.m_ChatterName, 0f);
		}
		this.m_Animator.SetTrigger(this.m_TDiarrhea);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Player.UnblockMoves();
		this.m_Animator.SetTrigger(this.m_TDiarrhea);
	}

	public void SetDiarrhea(Diarrhea diarrhea)
	{
		this.m_Diarrhea = diarrhea;
	}

	public Diarrhea GetDiarrhea()
	{
		return this.m_Diarrhea;
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
	}

	private int m_TDiarrhea = Animator.StringToHash("Diarrhea");

	private Diarrhea m_Diarrhea;

	public string m_ChatterName = string.Empty;

	private static DiarrheaController s_Instance;
}
