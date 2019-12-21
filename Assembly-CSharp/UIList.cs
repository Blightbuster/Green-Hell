using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIList : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler
{
	private void Awake()
	{
		this.m_Text.text = string.Empty;
		for (int i = 0; i < 100; i++)
		{
			Text text = this.m_Text;
			text.text = text.text + i + "\n";
		}
	}

	private void Start()
	{
		this.UpdateText();
		this.UpdateButtons();
	}

	public void AddElement(string element, int data = -1)
	{
		UIListElement<int> uilistElement = new UIListElement<int>(data);
		uilistElement.text = element;
		uilistElement.data = data;
		this.m_Elements.Add(uilistElement);
		this.OnElementAdded();
	}

	public void AddElement<T>(string element, T data)
	{
		UIListElement<T> uilistElement = new UIListElement<T>(data);
		uilistElement.text = element;
		uilistElement.data = data;
		this.m_Elements.Add(uilistElement);
		this.OnElementAdded();
	}

	private void OnElementAdded()
	{
		if (this.m_Sorted)
		{
			this.SortAlphabetically();
		}
		this.UpdateText();
		this.UpdateSlider();
		this.UpdateButtons();
	}

	public void RemoveElementAt(int idx)
	{
		if (idx >= 0 && idx < this.m_Elements.Count)
		{
			this.m_Elements.RemoveAt(idx);
			this.UpdateText();
			return;
		}
		DebugUtils.Assert(DebugUtils.AssertType.Info);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			this.SelectNext();
		}
		else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			this.SelectPrev();
		}
		this.UpdateSlider();
	}

	private void UpdateText()
	{
		string text = string.Empty;
		int num = this.m_ListStartIndex;
		while (num < this.m_ListStartIndex + this.m_Capacity && num < this.m_Elements.Count)
		{
			if (this.m_Focus)
			{
				if (num == this.m_SelectionIndex)
				{
					text = text + "<color=" + this.m_SelectedColor + ">";
				}
				else
				{
					text = text + "<color=" + this.m_NormalColor + ">";
				}
			}
			else
			{
				text = text + "<color=" + this.m_UnfocusedColor + ">";
			}
			text += this.m_Elements[num].GetText();
			text += "</color>";
			text += "\n";
			num++;
		}
		this.m_Text.text = text;
	}

	private void UpdateSlider()
	{
		if (!this.m_Slider)
		{
			return;
		}
		this.m_Slider.gameObject.SetActive(this.m_Elements.Count > this.m_Capacity);
		if (!this.m_Slider.gameObject.activeSelf)
		{
			return;
		}
		float normalizedValue = this.m_Slider.normalizedValue;
		if (normalizedValue == this.m_LastSliderVal)
		{
			return;
		}
		this.m_ListStartIndex = (int)((float)(this.m_Elements.Count - this.m_Capacity) * normalizedValue);
		this.UpdateText();
		this.m_LastSliderVal = normalizedValue;
	}

	private void UpdateSliderVal()
	{
		if (!this.m_Slider)
		{
			return;
		}
		if (this.m_Elements.Count - this.m_Capacity > 0)
		{
			this.m_Slider.normalizedValue = (float)this.m_ListStartIndex / (float)(this.m_Elements.Count - this.m_Capacity);
		}
	}

	private void UpdateButtons()
	{
		if (this.m_ButtonUp)
		{
			if (this.m_ListStartIndex > 0)
			{
				this.m_ButtonUp.enabled = true;
			}
			else
			{
				this.m_ButtonUp.enabled = false;
			}
		}
		if (this.m_ButtonDown)
		{
			if (this.m_Elements.Count - this.m_ListStartIndex > this.m_Capacity)
			{
				this.m_ButtonDown.enabled = true;
				return;
			}
			this.m_ButtonDown.enabled = false;
		}
	}

	public void Clear()
	{
		this.m_Elements.Clear();
		this.UpdateText();
	}

	public void SelectNext()
	{
		this.m_SelectionIndex++;
		if (this.m_SelectionIndex > this.m_Elements.Count - 1)
		{
			this.m_SelectionIndex = 0;
		}
		if (this.m_SelectionIndex >= this.m_ListStartIndex + this.m_Capacity)
		{
			this.m_ListStartIndex = this.m_SelectionIndex - this.m_Capacity + 1;
		}
		else if (this.m_SelectionIndex < this.m_ListStartIndex)
		{
			this.m_ListStartIndex = this.m_SelectionIndex;
		}
		this.UpdateSliderVal();
		this.UpdateText();
		this.UpdateButtons();
		if (this.m_ChangeSelectionReceiver != null)
		{
			this.m_ChangeSelectionReceiver.OnUIListChangeSelection(this);
		}
	}

	public void SelectPrev()
	{
		this.m_SelectionIndex--;
		if (this.m_SelectionIndex < 0)
		{
			this.m_SelectionIndex = ((this.m_Elements.Count > 0) ? (this.m_Elements.Count - 1) : 0);
		}
		if (this.m_SelectionIndex >= this.m_ListStartIndex + this.m_Capacity)
		{
			this.m_ListStartIndex = this.m_SelectionIndex - this.m_Capacity + 1;
		}
		else if (this.m_SelectionIndex < this.m_ListStartIndex)
		{
			this.m_ListStartIndex = this.m_SelectionIndex;
		}
		this.UpdateSliderVal();
		this.UpdateText();
		this.UpdateButtons();
		if (this.m_ChangeSelectionReceiver != null)
		{
			this.m_ChangeSelectionReceiver.OnUIListChangeSelection(this);
		}
	}

	public void SetSelectionIndex(int index)
	{
		this.m_SelectionIndex = index;
		if (this.m_SelectionIndex < 0)
		{
			this.m_SelectionIndex = ((this.m_Elements.Count > 0) ? (this.m_Elements.Count - 1) : 0);
		}
		if (this.m_SelectionIndex > this.m_Elements.Count - 1)
		{
			this.m_SelectionIndex = 0;
		}
		if (this.m_SelectionIndex >= this.m_ListStartIndex + this.m_Capacity)
		{
			this.m_ListStartIndex = this.m_SelectionIndex - this.m_Capacity + 1;
		}
		else if (this.m_SelectionIndex < this.m_ListStartIndex)
		{
			this.m_ListStartIndex = this.m_SelectionIndex;
		}
		this.UpdateSliderVal();
		this.UpdateText();
		this.UpdateButtons();
		if (this.m_ChangeSelectionReceiver != null)
		{
			this.m_ChangeSelectionReceiver.OnUIListChangeSelection(this);
		}
	}

	public int GetSelectionIndex()
	{
		return this.m_SelectionIndex;
	}

	public string GetSelectedElementText()
	{
		return this.GetElementText(this.m_SelectionIndex);
	}

	public string GetElementText(int index)
	{
		if (index < 0 || index >= this.m_Elements.Count)
		{
			return string.Empty;
		}
		return this.m_Elements[index].GetText();
	}

	public T GetSelectedElementData<T>()
	{
		return this.GetElementData<T>(this.m_SelectionIndex);
	}

	public T GetElementData<T>(int index)
	{
		if (index >= 0 && index < this.m_Elements.Count)
		{
			UIListElement<T> uilistElement = this.m_Elements[index] as UIListElement<T>;
			if (uilistElement != null)
			{
				return uilistElement.data;
			}
		}
		return default(T);
	}

	public int GetCount()
	{
		return this.m_Elements.Count;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		List<GameObject> hovered = eventData.hovered;
		for (int i = 0; i < hovered.Count; i++)
		{
			GameObject x = hovered[i];
			if (this.m_ButtonDown && x == this.m_ButtonDown.gameObject)
			{
				this.SelectNext();
				break;
			}
			if (this.m_ButtonUp && x == this.m_ButtonUp.gameObject)
			{
				this.SelectPrev();
				break;
			}
		}
		if (this.m_ClickReceiver != null)
		{
			this.m_ClickReceiver.OnUIListClicked(this);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		List<GameObject> hovered = eventData.hovered;
		for (int i = 0; i < hovered.Count; i++)
		{
			if (hovered[i] == this.m_Text.gameObject)
			{
				this.OnPointerDownText(eventData);
				break;
			}
		}
		if (this.m_MouseDownReceiver != null)
		{
			this.m_MouseDownReceiver.OnUIListMouseDown(this);
		}
	}

	private void OnPointerDownText(PointerEventData eventData)
	{
		RectTransform rectTransform = this.m_Text.rectTransform;
		Vector2 zero = Vector2.zero;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out zero))
		{
			return;
		}
		float num = rectTransform.sizeDelta.y - zero.y;
		float num2 = rectTransform.sizeDelta.y / (float)this.m_Capacity;
		int num3 = Mathf.FloorToInt(num / num2);
		this.m_SelectionIndex = this.m_ListStartIndex + num3;
		this.UpdateText();
		this.UpdateButtons();
	}

	public void SetFocus(bool focus)
	{
		if (focus != this.m_Focus)
		{
			this.m_Focus = focus;
			this.UpdateText();
			this.UpdateButtons();
		}
	}

	private static int CompareListByName(IUIListElement i1, IUIListElement i2)
	{
		return i1.GetText().CompareTo(i2.GetText());
	}

	public void SortAlphabetically()
	{
		this.m_Elements.Sort(new Comparison<IUIListElement>(UIList.CompareListByName));
	}

	public Text m_Text;

	private List<IUIListElement> m_Elements = new List<IUIListElement>();

	private int m_SelectionIndex;

	public int m_Capacity;

	private int m_ListStartIndex;

	public RawImage m_ButtonUp;

	public RawImage m_ButtonDown;

	public Slider m_Slider;

	public string m_NormalColor = "black";

	public string m_SelectedColor = "orange";

	public string m_UnfocusedColor = "#505050ff";

	private bool m_Focus;

	public IUIListClickReceiver m_ClickReceiver;

	public IUIListMouseDownReceiver m_MouseDownReceiver;

	public IUIListChangeSelection m_ChangeSelectionReceiver;

	public bool m_Sorted;

	private float m_LastSliderVal;
}
