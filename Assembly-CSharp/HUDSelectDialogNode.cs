using System;
using System.Collections.Generic;
using CJTools;
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
	}

	protected override void Awake()
	{
		base.Awake();
		HUDSelectDialogNode.s_Instance = this;
		this.m_TextGen = new TextGenerator();
		this.m_Dummy = base.transform.Find("Dummy").gameObject;
		this.m_Reply = base.transform.Find("Reply").gameObject;
		this.m_Reply.SetActive(false);
		this.m_Icon = this.m_Reply.transform.Find("Icon").GetComponent<RawImage>();
		this.m_ReplyCanvasGroup = this.m_Reply.gameObject.AddComponent<CanvasGroup>();
		this.m_ReplyCanvasGroup.alpha = 0f;
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
		this.m_Reply.SetActive(true);
		this.m_ReplyCanvasGroup.alpha = 0f;
		this.m_NodesVisible = false;
		this.m_SelectedNode = 0;
		this.Setup();
		InputsManager.Get().RegisterReceiver(this);
		this.Update();
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (this.m_PlayerBlocked)
		{
			Player.Get().UnblockMoves();
			Player.Get().UnblockRotation();
			CursorManager.Get().ShowCursor(false);
			CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
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
				this.AddNode(dialogNode.m_Name, num++);
			}
		}
		foreach (UISelectDialogNodeData uiselectDialogNodeData in this.m_UINodeDatas)
		{
			uiselectDialogNodeData.m_BG.rectTransform.sizeDelta = new Vector2(this.m_MaxBGWidth, uiselectDialogNodeData.m_BG.rectTransform.sizeDelta.y);
		}
		string str = (this.m_Nodes[0].m_Prevs.Count <= 0 || !(this.m_Nodes[0].m_Prevs[0].m_IconName != string.Empty)) ? "Walkie_talkie_icon" : this.m_Nodes[0].m_Prevs[0].m_IconName;
		this.m_Icon.texture = Resources.Load<Texture>("HUD/HUD_Walkie_Talkie/" + str);
		this.m_TimerBelt.gameObject.SetActive(this.m_Duration > 0f);
	}

	private void AddNode(string text, int index)
	{
		UISelectDialogNodeData uiselectDialogNodeData = new UISelectDialogNodeData();
		uiselectDialogNodeData.m_Object = UnityEngine.Object.Instantiate<GameObject>(this.m_NodePrefab, base.transform);
		Text component = uiselectDialogNodeData.m_Object.transform.Find("Text").gameObject.GetComponent<Text>();
		string text2 = text + "_Short";
		if (GreenHellGame.Instance.GetLocalization().Contains(text2))
		{
			component.text = GreenHellGame.Instance.GetLocalization().Get(text2);
		}
		else
		{
			component.text = GreenHellGame.Instance.GetLocalization().Get(text);
		}
		TextGenerationSettings generationSettings = component.GetGenerationSettings(component.rectTransform.rect.size);
		generationSettings.scaleFactor = 1f;
		float preferredWidth = this.m_TextGen.GetPreferredWidth(component.text, generationSettings);
		float preferredHeight = this.m_TextGen.GetPreferredHeight(component.text, generationSettings);
		uiselectDialogNodeData.m_BG = uiselectDialogNodeData.m_Object.FindChild("BG").GetComponent<RawImage>();
		this.m_MaxBGWidth = Mathf.Max(preferredWidth, this.m_MaxBGWidth);
		Button component2 = uiselectDialogNodeData.m_Object.GetComponent<Button>();
		component2.onClick.AddListener(delegate
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
		uiselectDialogNodeData.m_Object.SetActive(this.m_NodesVisible);
		this.m_UINodeDatas.Add(uiselectDialogNodeData);
	}

	private void ShowNodes(bool show)
	{
		if (this.m_NodesVisible == show)
		{
			return;
		}
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
			this.m_Reply.SetActive(false);
			CursorManager.Get().ShowCursor(true);
			CursorManager.Get().UpdateCursorVisibility();
			CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
			Vector3 zero = Vector3.zero;
			zero.x = (float)Screen.width * this.m_CursorOffset.x;
			zero.y = (float)Screen.height * this.m_CursorOffset.y;
			CursorManager.Get().SetCursorPos(this.m_Dummy.transform.position + zero);
			HintsManager.Get().ShowHint("Scroll_Dialog", 10f);
		}
		else
		{
			if (this.m_PlayerBlocked)
			{
				Player.Get().UnblockMoves();
				Player.Get().UnblockRotation();
				CursorManager.Get().ShowCursor(false);
				CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
				this.m_PlayerBlocked = false;
			}
			this.m_Reply.SetActive(true);
			this.m_ReplyCanvasGroup.alpha = 0f;
			HintsManager.Get().HideHint("Scroll_Dialog");
		}
	}

	public void OnSelect(int index)
	{
		if (index < 0)
		{
			this.ShowNodes(false);
			return;
		}
		DialogsManager.Get().OnSelectNode(this.m_Nodes[index]);
		this.m_Nodes = null;
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

	public void OnInputAction(InputsManager.InputAction action)
	{
		if (action == InputsManager.InputAction.ShowSelectDialogNode)
		{
			this.ShowNodes(true);
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
				if (RectTransformUtility.RectangleContainsScreenPoint(uiselectDialogNodeData.m_BG.rectTransform, Input.mousePosition))
				{
					this.m_SelectedNode = i;
					break;
				}
			}
			for (int j = 0; j < this.m_UINodeDatas.Count; j++)
			{
				this.m_UINodeDatas[j].m_BG.enabled = (j == this.m_SelectedNode);
			}
			if (!InputsManager.Get().IsActionActive(InputsManager.InputAction.ShowSelectDialogNode))
			{
				this.OnSelect(this.m_SelectedNode);
			}
		}
		CursorManager.Get().SetCursor((this.m_SelectedNode < 0) ? CursorManager.TYPE.Normal : CursorManager.TYPE.MouseOver);
		if (this.m_TimerBelt && this.m_TimerBelt.gameObject.activeSelf)
		{
			float b = Time.time - this.m_ShowTime;
			Vector3 localScale = this.m_TimerBelt.rectTransform.localScale;
			localScale.x = CJTools.Math.GetProportionalClamp(1f, 0f, b, 0f, this.m_Duration);
			this.m_TimerBelt.rectTransform.localScale = localScale;
		}
		if (this.m_Reply.gameObject.activeSelf)
		{
			if (this.m_ReplyCanvasGroup.alpha < 1f)
			{
				this.m_ReplyCanvasGroup.alpha += Time.deltaTime * 3f;
			}
			this.m_ReplyCanvasGroup.alpha = Mathf.Min(this.m_ReplyCanvasGroup.alpha, 1f);
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
			}
			else
			{
				this.m_Icon.rectTransform.localRotation = Quaternion.identity;
				this.m_WasSound = false;
			}
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

	private RawImage m_Icon;

	private float m_MaxBGWidth;

	private bool m_PlayerBlocked;

	private float m_ShowTime;

	private float m_Duration;

	public RawImage m_TimerBelt;

	private static HUDSelectDialogNode s_Instance;

	private CanvasGroup m_ReplyCanvasGroup;

	private int m_SelectedNode;

	public Vector3 m_CursorOffset = Vector3.zero;

	private bool m_WasSound;
}
