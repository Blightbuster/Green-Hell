using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDCursorControllerHint : HUDBase
{
	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override bool ShouldShow()
	{
		return GreenHellGame.IsPadControllerActive() && BodyInspectionController.Get().IsActive() && BodyInspectionController.Get().IsCursorOverLeech();
	}

	public override void UpdateAfterCamera()
	{
		base.UpdateAfterCamera();
		Vector3 localScale = ((RectTransform)HUDManager.Get().m_CanvasGameObject.transform).localScale;
		Vector3 offset = this.m_Offset;
		offset.x *= localScale.x;
		offset.y *= localScale.y;
		offset.z = 0f;
		this.m_Icon.transform.position = Input.mousePosition + offset;
	}

	public Image m_Icon;

	public Vector3 m_Offset = Vector3.zero;
}
