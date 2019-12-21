using System;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class HUDSanity : HUDBase
{
	public static HUDSanity Get()
	{
		return HUDSanity.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDSanity.s_Instance = this;
		this.m_Icon = base.transform.Find("Icon").GetComponent<Image>();
		this.m_OrigColor = this.m_Icon.color;
		this.m_AlertColor.a = this.m_OrigColor.a;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override bool ShouldShow()
	{
		return !Player.Get().m_DreamActive && !HUDReadableItem.Get().isActiveAndEnabled && !CutscenesManager.Get().IsCutscenePlaying() && DifficultySettings.ActivePreset.m_Sanity;
	}

	protected override void Update()
	{
		base.Update();
		this.m_FillImage.fillAmount = (float)PlayerSanityModule.Get().m_Sanity / 100f;
		this.UpdateChangeSanityAnim();
	}

	public void OnChangeSanity(float diff)
	{
		if (this.m_ChangeSanityAnim != 0)
		{
			return;
		}
		this.m_ChangeSanityAnim = ((diff > 0f) ? 1 : -1);
		this.m_ChangeSanityTime = Time.time;
	}

	private void UpdateChangeSanityAnim()
	{
		Color color = Color.Lerp(this.m_OrigColor, this.m_AlertColor, CJTools.Math.GetProportionalClamp(0f, 1f, (float)PlayerSanityModule.Get().m_Sanity, 50f, 0f));
		if (this.m_ChangeSanityAnim == -1)
		{
			float num = Mathf.Sin((Time.time - this.m_ChangeSanityTime) * 5f);
			if (num >= 0f)
			{
				color = Color.Lerp(color, this.m_AlertColor, num);
			}
			else
			{
				this.m_ChangeSanityAnim = 0;
			}
		}
		else
		{
			int changeSanityAnim = this.m_ChangeSanityAnim;
		}
		this.m_Icon.color = color;
	}

	private Image m_Icon;

	public Image m_FillImage;

	private Color m_OrigColor = Color.black;

	private int m_ChangeSanityAnim;

	private float m_ChangeSanityTime;

	private Color m_AlertColor = Color.red;

	private static HUDSanity s_Instance;
}
