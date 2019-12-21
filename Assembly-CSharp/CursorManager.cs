using System;
using System.Collections.Generic;
using CJTools;
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
		CursorManager.s_Instance.SetCursorPos(new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f));
		CursorManager.s_Instance.LoadSettings();
	}

	public void LoadSettings()
	{
		this.m_ControllerCursorSpeedCurve = new AnimationCurve();
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("ControllerSettings.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "SpeedCurveKey")
			{
				Keyframe key2 = default(Keyframe);
				key2.time = key.GetVariable(0).FValue;
				key2.value = key.GetVariable(1).FValue;
				key2.inTangent = key.GetVariable(2).FValue;
				key2.outTangent = key.GetVariable(3).FValue;
				key2.inWeight = key.GetVariable(4).FValue;
				key2.outWeight = key.GetVariable(5).FValue;
				key2.weightedMode = (WeightedMode)key.GetVariable(6).IValue;
				this.m_ControllerCursorSpeedCurve.AddKey(key2);
			}
			else if (key.GetName() == "Speed")
			{
				this.m_ControllerCursorSpeed = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "DeadZone")
			{
				this.m_ControllerCursorDeadZone = key.GetVariable(0).FValue;
			}
		}
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
		if (this.m_WantedType != type)
		{
			this.m_WantedType = type;
		}
	}

	public CursorManager.TYPE GetCursor()
	{
		return this.m_Type;
	}

	public void Update()
	{
		if (!CustomCursor.Get())
		{
			return;
		}
		bool visible = Cursor.visible;
		CursorMode cursorMode = (Inventory3DManager.Get() && Inventory3DManager.Get().gameObject.activeSelf && Inventory3DManager.Get().m_CarriedItem) ? CursorMode.ForceSoftware : CursorMode.Auto;
		if (cursorMode != this.m_Mode)
		{
			this.m_Mode = cursorMode;
			if (this.m_Mode == CursorMode.ForceSoftware)
			{
				CustomCursor.Get().Show(true);
				this.m_SystemCursorActive = false;
			}
			else
			{
				CustomCursor.Get().Show(false);
				this.m_SystemCursorActive = true;
			}
			if (CustomCursor.Get())
			{
				CustomCursor.Get().m_Texture = this.m_TexturesMap[this.m_Type];
			}
			Cursor.SetCursor(this.m_TexturesMap[this.m_Type], Vector2.zero, this.m_Mode);
		}
	}

	public void LateUpdate()
	{
		this.UpdateCursorVisibility();
		this.UpdateCursorType();
	}

	public void UpdateCursorVisibility()
	{
		if (this.IsCursorVisible())
		{
			if (this.m_currentVisibilityMode == CursorManager.VisibilityMode.Visible && !Cursor.visible)
			{
				Cursor.visible = true;
			}
			CursorLockMode cursorLockMode = CursorLockMode.None;
			if (cursorLockMode != Cursor.lockState)
			{
				Cursor.lockState = cursorLockMode;
				return;
			}
		}
		else
		{
			if (Cursor.visible)
			{
				Cursor.visible = false;
			}
			if (GreenHellGame.IsPCControllerActive())
			{
				CursorLockMode cursorLockMode2 = (CustomCursor.Get() && CustomCursor.Get().m_Visible) ? CursorLockMode.None : CursorLockMode.Locked;
				if (cursorLockMode2 != Cursor.lockState)
				{
					Cursor.lockState = cursorLockMode2;
				}
			}
		}
	}

	private void UpdateCursorType()
	{
		if (this.m_WantedType != this.m_Type)
		{
			this.m_Type = this.m_WantedType;
			if (CustomCursor.Get())
			{
				CustomCursor.Get().m_Texture = this.m_TexturesMap[this.m_Type];
			}
			Cursor.SetCursor(this.m_TexturesMap[this.m_Type], Vector2.zero, CursorMode.Auto);
		}
	}

	public void ResetCursorRequests()
	{
		this.m_ShowCursorRequests = 0;
	}

	public void ShowCursor(Vector2 pos)
	{
		this.ShowCursor(true, false);
		this.UpdateCursorVisibility();
		this.SetCursorPos(pos);
	}

	public void ShowCursor(CursorManager.TYPE type)
	{
		this.ShowCursor(true, false);
		this.UpdateCursorVisibility();
		this.SetCursor(type);
	}

	public void ShowCursor(Vector2 pos, CursorManager.TYPE type)
	{
		this.ShowCursor(true, false);
		this.UpdateCursorVisibility();
		this.SetCursor(type);
		this.SetCursorPos(pos);
	}

	public void ShowCursor(bool show, bool center = false)
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
		this.m_Center.Set((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
		if (center)
		{
			this.m_LocalCursorPos = this.m_Center;
			this.SetCursorPos(this.m_LocalCursorPos);
		}
	}

	public Vector2 GetGlobalCursorPos()
	{
		return CursorControl.GetGlobalCursorPos();
	}

	public void SetGlobalCursorPos(Vector2 pos)
	{
		CursorControl.SetGlobalCursorPos(pos);
	}

	public void SetCursorPos(Vector2 pos)
	{
		CursorControl.SetLocalCursorPos(pos);
		this.m_LocalCursorPos = pos;
	}

	public bool IsCursorVisible()
	{
		return this.m_ShowCursorRequests > 0 && CursorManager.Get().m_SystemCursorActive;
	}

	public void UpdatePadCursor(float speed_mul = 1f)
	{
		if (!GreenHellGame.IsPadControllerActive())
		{
			return;
		}
		Vector2 vector = new Vector2(InputsManager.Get().GetActionValue(InputsManager.InputAction.LookRight) - InputsManager.Get().GetActionValue(InputsManager.InputAction.LookLeft), InputsManager.Get().GetActionValue(InputsManager.InputAction.LookUp) - InputsManager.Get().GetActionValue(InputsManager.InputAction.LookDown));
		vector.x = CJTools.Math.GetProportionalClamp(this.m_ControllerCursorDeadZone, 1f, System.Math.Abs(vector.x), 0f, 1f) * (float)System.Math.Sign(vector.x);
		vector.y = CJTools.Math.GetProportionalClamp(this.m_ControllerCursorDeadZone, 1f, System.Math.Abs(vector.y), 0f, 1f) * (float)System.Math.Sign(vector.y);
		if (vector == Vector2.zero)
		{
			return;
		}
		float magnitude = vector.magnitude;
		if (magnitude > 1f)
		{
			vector /= magnitude;
		}
		float num = (float)Screen.height / 1080f;
		float num2 = this.m_ControllerCursorSpeedCurve.Evaluate(vector.magnitude);
		vector *= num2 * this.m_ControllerCursorSpeed * num * Time.deltaTime * speed_mul;
		this.m_LocalCursorPos += vector;
		this.m_LocalCursorPos.x = Mathf.Clamp(this.m_LocalCursorPos.x, 0f, (float)Screen.width);
		this.m_LocalCursorPos.y = Mathf.Clamp(this.m_LocalCursorPos.y, 0f, (float)Screen.height);
		this.SetCursorPos(this.m_LocalCursorPos);
	}

	private CursorManager.TYPE m_Type = CursorManager.TYPE.None;

	private CursorManager.TYPE m_WantedType = CursorManager.TYPE.None;

	private CursorManager.VisibilityMode m_currentVisibilityMode;

	private Dictionary<CursorManager.TYPE, Texture2D> m_TexturesMap = new Dictionary<CursorManager.TYPE, Texture2D>();

	private CursorMode m_Mode;

	public bool m_SystemCursorActive = true;

	private int m_ShowCursorRequests;

	private Vector2 m_LocalCursorPos = Vector2.zero;

	private Vector2 m_Center = Vector2.zero;

	private AnimationCurve m_ControllerCursorSpeedCurve = new AnimationCurve();

	private float m_ControllerCursorSpeed;

	private float m_ControllerCursorDeadZone;

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
}
