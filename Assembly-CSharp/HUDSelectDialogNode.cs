using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDSelectDialogNode : HUDBase, IInputsReceiver
{
	public static HUDSelectDialogNode Get()
	{
		return HUDSelectDialogNode.s_Instance;
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
		HUDSelectDialogNode.s_Instance = this;
		this.m_TextGen = new TextGenerator();
		this.m_Dummy = base.transform.Find("Dummy").gameObject;
		this.m_Reply = base.transform.Find("Reply").gameObject;
		this.m_Reply.SetActive(false);
		this.m_ReplyPad = base.transform.Find("ReplyPad").gameObject;
		this.m_ReplyPad.SetActive(false);
		this.m_Icon = this.m_Reply.transform.Find("Icon").GetComponent<RawImage>();
		this.m_ReplyCanvasGroup = this.m_Reply.gameObject.AddComponent<CanvasGroup>();
		this.m_ReplyCanvasGroup.alpha = 0f;
		this.m_ReplyPadCanvasGroup = this.m_ReplyPad.gameObject.AddComponent<CanvasGroup>();
		this.m_ReplyPadCanvasGroup.alpha = 0f;
		this.m_KeyText = base.transform.FindDeepChild("Key").gameObject.GetComponent<Text>();
		this.m_KeyFrame = base.transform.FindDeepChild("KeyFrame").gameObject;
		this.m_PadIcon = base.transform.FindDeepChild("PadIcon").gameObject;
	}

	public void ShowNodeSelection(List<DialogNode> nodes, float duration)
	{
		while (this.m_UINodeDatas.Count > 0)
		{
			UnityEngine.Object.Destroy(this.m_UINodeDatas[0].m_Object.gameObject);
			this.m_UINodeDatas.RemoveAt(0);
		}
		this.m_Nodes = new List<DialogNode>(nodes);
		this.m_ShowTime = Time.time;
		this.m_Duration = duration;
	}

	public void HideNodeSelection()
	{
		if (this.m_Nodes != null)
		{
			this.m_Nodes = null;
		}
	}

	protected override bool ShouldShow()
	{
		return this.m_Nodes != null && !BodyInspectionController.Get().IsActive();
	}

	protected override void OnShow()
	{
		base.OnShow();
		Vector3 position = this.m_Dummy.transform.position;
		position.y = (float)Screen.height * 0.23f;
		this.m_Dummy.transform.position = position;
		this.m_Reply.SetActive(GreenHellGame.IsPCControllerActive());
		this.m_ReplyCanvasGroup.alpha = 0f;
		this.m_ReplyPad.SetActive(GreenHellGame.IsPadControllerActive());
		this.m_ReplyPadCanvasGroup.alpha = 0f;
		this.m_NodesVisible = false;
		this.m_SelectedNode = 0;
		this.Setup();
		InputsManager.Get().RegisterReceiver(this);
		this.Update();
		foreach (UISelectDialogNodeData uiselectDialogNodeData in this.m_UINodeDatas)
		{
			uiselectDialogNodeData.m_Object.SetActive(this.m_NodesVisible);
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (this.m_PlayerBlocked)
		{
			Player.Get().UnblockMoves();
			Player.Get().UnblockRotation();
			if (this.m_CursorVisible)
			{
				CursorManager.Get().ShowCursor(false, false);
				CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
				this.m_CursorVisible = false;
			}
			this.m_PlayerBlocked = false;
		}
		InputsManager.Get().UnregisterReceiver(this);
	}

	private void Setup()
	{
		this.m_PosY = this.m_Dummy.transform.localPosition.y;
		this.m_MaxBGWidth = 0f;
		int num = 0;
		foreach (DialogNode dialogNode in this.m_Nodes)
		{
			if (!dialogNode.m_Additional)
			{
				this.AddNode(dialogNode, num++);
			}
		}
		float x = ((RectTransform)HUDManager.Get().m_CanvasGameObject.transform).localScale.x;
		foreach (UISelectDialogNodeData uiselectDialogNodeData in this.m_UINodeDatas)
		{
			uiselectDialogNodeData.m_BG.rectTransform.sizeDelta = new Vector2(this.m_MaxBGWidth, uiselectDialogNodeData.m_BG.rectTransform.sizeDelta.y);
			Vector3 position = uiselectDialogNodeData.m_BG.transform.position;
			position.x += this.m_MaxBGWidth * 0.29f * x + uiselectDialogNodeData.m_PadIcon.rectTransform.rect.width * 0.3f * x;
			uiselectDialogNodeData.m_PadIcon.transform.position = position;
			uiselectDialogNodeData.m_PadIcon.gameObject.SetActive(false);
		}
		string str = (this.m_Nodes[0].m_Prevs.Count > 0 && this.m_Nodes[0].m_Prevs[0].m_IconName != string.Empty) ? this.m_Nodes[0].m_Prevs[0].m_IconName : "Walkie_talkie_icon";
		this.m_Icon.texture = Resources.Load<Texture>("HUD/HUD_Walkie_Talkie/" + str);
		this.m_TimerBelt[GreenHellGame.IsPCControllerActive() ? 0 : 1].gameObject.SetActive(this.m_Duration > 0f);
		InputActionData actionDataByInputAction = InputsManager.Get().GetActionDataByInputAction(InputsManager.InputAction.ShowSelectDialogNode, ControllerType._Count);
		this.m_KeyText.text = KeyCodeToString.GetString((actionDataByInputAction != null) ? actionDataByInputAction.m_KeyCode : KeyCode.None);
		GameObject gameObject = GreenHellGame.IsPCControllerActive() ? this.m_Reply : this.m_ReplyPad;
		Vector3 position2 = this.m_KeyFrame.transform.position;
		position2.x = gameObject.transform.position.x;
		Text component = gameObject.GetComponent<Text>();
		TextGenerationSettings generationSettings = component.GetGenerationSettings(component.rectTransform.rect.size);
		generationSettings.scaleFactor = 1f;
		float num2 = this.m_TextGen.GetPreferredWidth(component.text, generationSettings) * x;
		position2.x -= num2 * 0.5f;
		position2.x -= this.m_KeyFrame.GetComponent<RectTransform>().sizeDelta.x * 1.1f * x;
		this.m_KeyFrame.transform.position = position2;
		this.m_PadIcon.transform.position = position2;
	}

	private void AddNode(DialogNode node, int index)
	{
		UISelectDialogNodeData uiselectDialogNodeData = new UISelectDialogNodeData();
		uiselectDialogNodeData.m_Object = UnityEngine.Object.Instantiate<GameObject>(this.m_NodePrefab, base.transform);
		Text component = uiselectDialogNodeData.m_Object.transform.Find("Text").gameObject.GetComponent<Text>();
		string text = node.m_Name + "_Short";
		if (GreenHellGame.Instance.GetLocalization().Contains(text))
		{
			component.text = GreenHellGame.Instance.GetLocalization().Get(text, true);
		}
		else
		{
			component.text = GreenHellGame.Instance.GetLocalization().Get(node.m_Name, true);
		}
		component.fontSize = ((GreenHellGame.Instance.m_Settings.m_Subtitles == SubtitlesSetting.Large) ? HUDDialog.FONT_SIZE_LARGE : HUDDialog.FONT_SIZE);
		if (node.m_Locked)
		{
			Color grey = Color.grey;
			grey.r = 0.65f;
			grey.g = 0.65f;
			grey.b = 0.65f;
			component.color = grey;
		}
		TextGenerationSettings generationSettings = component.GetGenerationSettings(component.rectTransform.rect.size);
		generationSettings.scaleFactor = 1f;
		float preferredWidth = this.m_TextGen.GetPreferredWidth(component.text, generationSettings);
		float preferredHeight = this.m_TextGen.GetPreferredHeight(component.text, generationSettings);
		uiselectDialogNodeData.m_BG = uiselectDialogNodeData.m_Object.FindChild("BG").GetComponent<RawImage>();
		uiselectDialogNodeData.m_BG.enabled = false;
		this.m_MaxBGWidth = Mathf.Max(preferredWidth, this.m_MaxBGWidth);
		uiselectDialogNodeData.m_Object.GetComponent<Button>().onClick.AddListener(delegate
		{
			this.OnSelect(index);
		});
		Vector3 position = uiselectDialogNodeData.m_Object.transform.position;
		position.x = 0f;
		position.y = this.m_PosY;
		position.z = 0f;
		uiselectDialogNodeData.m_Object.transform.localPosition = position;
		this.m_PosY -= preferredHeight + this.m_Margin;
		uiselectDialogNodeData.m_Object.transform.localScale = Vector3.one;
		uiselectDialogNodeData.m_PadIcon = uiselectDialogNodeData.m_Object.FindChild("PadIcon").GetComponent<Image>();
		uiselectDialogNodeData.m_Node = node;
		uiselectDialogNodeData.m_Object.SetActive(this.m_NodesVisible);
		this.m_UINodeDatas.Add(uiselectDialogNodeData);
	}

	private void ShowNodes(bool show)
	{
		if (this.m_NodesVisible == show)
		{
			return;
		}
		this.m_PadNodeIndex = 0;
		foreach (UISelectDialogNodeData uiselectDialogNodeData in this.m_UINodeDatas)
		{
			uiselectDialogNodeData.m_Object.SetActive(show);
		}
		this.m_NodesVisible = show;
		this.m_SelectedNode = 0;
		if (this.m_NodesVisible)
		{
			Player.Get().BlockMoves();
			Player.Get().BlockRotation();
			this.m_PlayerBlocked = true;
			if (GreenHellGame.IsPCControllerActive())
			{
				this.m_Reply.SetActive(this.m_Duration != 0f);
			}
			else
			{
				this.m_ReplyPad.SetActive(this.m_Duration != 0f);
			}
			if (GreenHellGame.IsPCControllerActive())
			{
				CursorManager.Get().ShowCursor(CursorManager.TYPE.Normal);
				this.m_CursorVisible = true;
			}
			Vector3 zero = Vector3.zero;
			zero.x = (float)Screen.width * this.m_CursorOffset.x;
			zero.y = (float)Screen.height * this.m_CursorOffset.y;
			CursorManager.Get().SetCursorPos(this.m_Dummy.transform.position + zero);
			HintsManager.Get().ShowHint("Scroll_Dialog", 10f);
			return;
		}
		if (this.m_PlayerBlocked)
		{
			Player.Get().UnblockMoves();
			Player.Get().UnblockRotation();
			if (this.m_CursorVisible)
			{
				CursorManager.Get().ShowCursor(false, false);
				CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
			}
			this.m_PlayerBlocked = false;
		}
		if (GreenHellGame.IsPCControllerActive())
		{
			this.m_Reply.SetActive(true);
		}
		else
		{
			this.m_ReplyPad.SetActive(true);
		}
		this.m_ReplyCanvasGroup.alpha = 0f;
		this.m_ReplyPadCanvasGroup.alpha = 0f;
		HintsManager.Get().HideHint("Scroll_Dialog");
	}

	public void OnSelect(int index)
	{
		if (index < 0)
		{
			this.ShowNodes(false);
			return;
		}
		if (this.m_Nodes[index].m_Locked)
		{
			return;
		}
		DialogsManager.Get().OnSelectNode(this.m_Nodes[index]);
		this.m_Nodes = null;
		this.m_LastSelectNodeTime = Time.time;
		while (this.m_UINodeDatas.Count > 0)
		{
			UnityEngine.Object.Destroy(this.m_UINodeDatas[0].m_Object.gameObject);
			this.m_UINodeDatas.RemoveAt(0);
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

	public void OnInputAction(InputActionData action_data)
	{
		if (action_data.m_Action == InputsManager.InputAction.ShowSelectDialogNode)
		{
			this.ShowNodes(true);
		}
		else if ((action_data.m_Action == InputsManager.InputAction.LSBackward || action_data.m_Action == InputsManager.InputAction.DPadDown) && this.m_PadNodeIndex < this.m_UINodeDatas.Count - 1)
		{
			this.m_PadNodeIndex++;
		}
		if ((action_data.m_Action == InputsManager.InputAction.LSForward || action_data.m_Action == InputsManager.InputAction.DPadUp) && this.m_PadNodeIndex > 0)
		{
			this.m_PadNodeIndex--;
		}
	}

	protected override void Update()
	{
		base.Update();
		this.m_SelectedNode = -1;
		if (this.m_NodesVisible)
		{
			for (int i = 0; i < this.m_UINodeDatas.Count; i++)
			{
				UISelectDialogNodeData uiselectDialogNodeData = this.m_UINodeDatas[i];
				if (GreenHellGame.IsPCControllerActive())
				{
					if (!uiselectDialogNodeData.m_Node.m_Locked && RectTransformUtility.RectangleContainsScreenPoint(uiselectDialogNodeData.m_BG.rectTransform, Input.mousePosition))
					{
						this.m_SelectedNode = i;
						break;
					}
				}
				else
				{
					this.m_SelectedNode = this.m_PadNodeIndex;
				}
			}
			for (int j = 0; j < this.m_UINodeDatas.Count; j++)
			{
				this.m_UINodeDatas[j].m_BG.enabled = (j == this.m_SelectedNode);
				this.m_UINodeDatas[j].m_PadIcon.gameObject.SetActive(GreenHellGame.IsPadControllerActive() && j == this.m_SelectedNode);
			}
			if (GreenHellGame.IsPCControllerActive())
			{
				if (!InputsManager.Get().IsActionActive(InputsManager.InputAction.ShowSelectDialogNode))
				{
					this.OnSelect(this.m_SelectedNode);
				}
			}
			else if (Input.GetKeyDown(InputHelpers.PadButton.Button_X.KeyFromPad()))
			{
				this.OnSelect(this.m_SelectedNode);
			}
			else if (Input.GetKeyDown(InputHelpers.PadButton.Button_B.KeyFromPad()))
			{
				this.OnSelect(-1);
			}
		}
		CursorManager.Get().SetCursor((this.m_SelectedNode >= 0) ? CursorManager.TYPE.MouseOver : CursorManager.TYPE.Normal);
		if (this.m_TimerBelt[GreenHellGame.IsPCControllerActive() ? 0 : 1] && this.m_TimerBelt[GreenHellGame.IsPCControllerActive() ? 0 : 1].gameObject.activeSelf)
		{
			float b = Time.time - this.m_ShowTime;
			Vector3 localScale = this.m_TimerBelt[GreenHellGame.IsPCControllerActive() ? 0 : 1].rectTransform.localScale;
			localScale.x = CJTools.Math.GetProportionalClamp(1f, 0f, b, 0f, this.m_Duration);
			this.m_TimerBelt[GreenHellGame.IsPCControllerActive() ? 0 : 1].rectTransform.localScale = localScale;
		}
		GameObject gameObject = GreenHellGame.IsPCControllerActive() ? this.m_Reply : this.m_ReplyPad;
		CanvasGroup canvasGroup = GreenHellGame.IsPCControllerActive() ? this.m_ReplyCanvasGroup : this.m_ReplyPadCanvasGroup;
		if (gameObject.gameObject.activeSelf)
		{
			if (canvasGroup.alpha < 1f)
			{
				canvasGroup.alpha += Time.deltaTime * 3f;
			}
			canvasGroup.alpha = Mathf.Min(canvasGroup.alpha, 1f);
			float num = Mathf.Sin(Time.time);
			if (num > 0f)
			{
				if (!this.m_WasSound)
				{
					Player.Get().m_AudioModule.PlayDialogSound(1f, false);
					this.m_WasSound = true;
				}
				Quaternion identity = Quaternion.identity;
				identity.z = Mathf.Sin(Time.time * 100f) * num * 0.1f;
				this.m_Icon.rectTransform.localRotation = identity;
				return;
			}
			this.m_Icon.rectTransform.localRotation = Quaternion.identity;
			this.m_WasSound = false;
		}
	}

	private TextGenerator m_TextGen;

	private List<DialogNode> m_Nodes;

	private List<UISelectDialogNodeData> m_UINodeDatas = new List<UISelectDialogNodeData>();

	public GameObject m_NodePrefab;

	private GameObject m_Dummy;

	private float m_PosY;

	public float m_Margin;

	private bool m_NodesVisible;

	private GameObject m_Reply;

	private GameObject m_ReplyPad;

	private RawImage m_Icon;

	private float m_MaxBGWidth;

	private bool m_PlayerBlocked;

	private float m_ShowTime;

	private float m_Duration;

	public RawImage[] m_TimerBelt;

	private static HUDSelectDialogNode s_Instance;

	private CanvasGroup m_ReplyCanvasGroup;

	private CanvasGroup m_ReplyPadCanvasGroup;

	private int m_SelectedNode;

	private Text m_KeyText;

	private GameObject m_KeyFrame;

	private GameObject m_PadIcon;

	[HideInInspector]
	public float m_LastSelectNodeTime;

	private bool m_CursorVisible;

	private int m_PadNodeIndex;

	public Vector3 m_CursorOffset = Vector3.zero;

	private bool m_WasSound;
}
