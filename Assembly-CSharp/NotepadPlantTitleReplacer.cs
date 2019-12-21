using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class NotepadPlantTitleReplacer : MonoBehaviour
{
	private void OnEnable()
	{
		ItemID item = (ItemID)Enum.Parse(typeof(ItemID), this.m_ItemID);
		ItemInfo info = ItemsManager.Get().GetInfo(this.m_ItemID);
		if (ItemsManager.Get().m_UnlockedItemInfos.Contains(item) || info.m_LockedInfoID.Length == 0)
		{
			base.GetComponent<Text>().text = GreenHellGame.Instance.GetLocalization().Get(base.gameObject.name, true);
			return;
		}
		base.GetComponent<Text>().text = GreenHellGame.Instance.GetLocalization().Get(info.m_LockedInfoID, true);
	}

	public string m_ItemID = string.Empty;
}
