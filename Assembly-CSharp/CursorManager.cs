using System;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager
{
	public static CursorManager Get()
	{
		return CursorManager.s_Instance;
	}

	public static void Initialize()
	{
		CursorManager.s_Instance = new CursorManager();
		CursorManager.s_Instance.LoadTextures();
		CursorManager.s_Instance.SetCursor(CursorManager.TYPE.Normal);
	}

	private void LoadTextures()
	{
		TextAsset textAsset = Resources.Load("Scripts/Cursors") as TextAsset;
		DebugUtils.Assert(textAsset, "ERROR - Missing Cursors script.", true, DebugUtils.AssertType.Info);
		TextAssetParser textAssetParser = new TextAssetParser(textAsset);
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
			if (key.GetName() == "Cursor")
			{
				CursorManager.TYPE key2 = (CursorManager.TYPE)Enum.Parse(typeof(CursorManager.TYPE), key.GetVariable(0).SValue);
				Texture2D texture2D = Resources.Load<Texture2D>("Cursors/" + key.GetVariable(1).SValue);
				DebugUtils.Assert(texture2D != null, "ERROR - Missing cursor texture. TYPE = " + key2.ToString(), true, DebugUtils.AssertType.Info);
				this.m_TexturesMap.Add(key2, texture2D);
			}
		}
		Resources.UnloadAsset(textAsset);
	}

	public void SetCursor(CursorManager.TYPE type)
	{
		if (this.m_Type == type)
		{
			return;
		}
		this.m_Type = type;
		Cursor.SetCursor(this.m_TexturesMap[this.m_Type], Vector2.zero, this.m_Mode);
	}

	public CursorManager.TYPE GetCursor()
	{
		return this.m_Type;
	}

	public void Update()
	{
		bool visible = Cursor.visible;
		CursorMode cursorMode = (!Inventory3DManager.Get() || !Inventory3DManager.Get().gameObject.activeSelf || !Inventory3DManager.Get().m_CarriedItem) ? CursorMode.Auto : CursorMode.ForceSoftware;
		if (cursorMode != this.m_Mode)
		{
			this.m_Mode = cursorMode;
			if (this.m_Mode == CursorMode.ForceSoftware)
			{
				CustomCursor.Get().m_Texture = this.m_TexturesMap[this.m_Type];
				CustomCursor.Get().Show(true);
				this.m_SystemCursorActive = false;
			}
			else
			{
				CustomCursor.Get().Show(false);
				this.m_SystemCursorActive = true;
				Cursor.SetCursor(this.m_TexturesMap[this.m_Type], Vector2.zero, CursorMode.Auto);
			}
		}
	}

	public void LateUpdate()
	{
		this.UpdateCursorVisibility();
	}

	public void UpdateCursorVisibility()
	{
		if (this.IsCursorVisible())
		{
			if (this.m_currentVisibilityMode == CursorManager.VisibilityMode.Visible)
			{
				Cursor.visible = true;
			}
			Cursor.lockState = CursorLockMode.None;
		}
		else
		{
			Cursor.visible = false;
			Cursor.lockState = ((!CustomCursor.Get() || !CustomCursor.Get().m_Visible) ? CursorLockMode.Locked : CursorLockMode.None);
		}
	}

	public void ResetCursorRequests()
	{
		this.m_ShowCursorRequests = 0;
	}

	public void ShowCursor(bool show)
	{
		if (show)
		{
			this.m_ShowCursorRequests++;
			this.m_currentVisibilityMode = CursorManager.VisibilityMode.Visible;
		}
		else
		{
			this.m_ShowCursorRequests--;
		}
		if (this.m_ShowCursorRequests < 0)
		{
			this.m_ShowCursorRequests = 0;
		}
		if (this.m_ShowCursorRequests == 0)
		{
			this.m_currentVisibilityMode = CursorManager.VisibilityMode.Hidden;
		}
	}

	public void ShowCursor(CursorManager.VisibilityMode show)
	{
		if (show != CursorManager.VisibilityMode.Hidden)
		{
			this.m_ShowCursorRequests++;
			this.m_currentVisibilityMode = show;
		}
		else
		{
			this.m_ShowCursorRequests--;
		}
		if (this.m_ShowCursorRequests < 0)
		{
			this.m_ShowCursorRequests = 0;
		}
		if (this.m_ShowCursorRequests == 0)
		{
			this.m_currentVisibilityMode = CursorManager.VisibilityMode.Hidden;
		}
	}

	public Vector2 GetCursorPos()
	{
		return CursorControl.GetGlobalCursorPos();
	}

	public void SetCursorPos(Vector2 pos)
	{
		CursorControl.SetLocalCursorPos(pos);
	}

	public bool IsCursorVisible()
	{
		return this.m_ShowCursorRequests > 0 && CursorManager.Get().m_SystemCursorActive;
	}

	private CursorManager.TYPE m_Type = CursorManager.TYPE.None;

	private CursorManager.VisibilityMode m_currentVisibilityMode;

	private Dictionary<CursorManager.TYPE, Texture2D> m_TexturesMap = new Dictionary<CursorManager.TYPE, Texture2D>();

	private CursorMode m_Mode;

	public bool m_SystemCursorActive = true;

	private int m_ShowCursorRequests;

	private static CursorManager s_Instance;

	public enum TYPE
	{
		None = -1,
		Normal,
		Hand_0,
		Hand_1,
		InspectionArrow,
		MouseOver,
		Hammer
	}

	public enum VisibilityMode
	{
		Visible,
		HiddenUnlocked,
		Hidden
	}

	public struct Point
	{
		public static implicit operator Vector2(CursorManager.Point p)
		{
			return new Vector2((float)p.X, (float)p.Y);
		}

		public int X;

		public int Y;
	}
}
