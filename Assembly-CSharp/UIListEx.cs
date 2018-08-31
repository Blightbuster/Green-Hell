using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIListEx : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IEventSystemHandler
{
	private void Awake()
	{
	}

	private void Start()
	{
		this.UpdateElements();
		this.UpdateButtons();
	}

	public void AddElement(string element, int data = -1, bool add_delete_button = false)
	{
		UIListExElement uilistExElement = new UIListExElement(this.m_ElementPrefab, base.gameObject);
		uilistExElement.text = element;
		uilistExElement.ui_element.GetComponentInChildren<Text>().text = element;
		uilistExElement.data = data;
		uilistExElement.idx = this.m_Elements.Count;
		uilistExElement.show_delete_button = add_delete_button;
		this.m_Elements.Add(uilistExElement);
		UIListExElement uilistExElement2 = new UIListExElement(this.m_DeleteButtonPrefabToInstantiate, base.gameObject);
		uilistExElement2.data = data;
		uilistExElement2.idx = this.m_DeleteButtons.Count;
		this.m_DeleteButtons.Add(uilistExElement2);
		if (add_delete_button)
		{
			uilistExElement2.ui_element.SetActive(true);
		}
		else
		{
			uilistExElement2.ui_element.SetActive(false);
		}
		if (this.m_Sorted)
		{
			this.SortAlphabetically();
		}
		this.UpdateElements();
		this.UpdateSlider();
		this.UpdateButtons();
	}

	public void AddElementAt(string element, int idx, int data = -1, bool add_delete_button = false)
	{
		UIListExElement uilistExElement = new UIListExElement(this.m_ElementPrefab, base.gameObject);
		uilistExElement.text = element;
		uilistExElement.ui_element.GetComponentInChildren<Text>().text = element;
		uilistExElement.data = data;
		uilistExElement.idx = idx;
		uilistExElement.show_delete_button = add_delete_button;
		this.m_Elements.Insert(idx, uilistExElement);
		UIListExElement uilistExElement2 = new UIListExElement(this.m_DeleteButtonPrefabToInstantiate, base.gameObject);
		uilistExElement2.data = data;
		uilistExElement2.idx = idx;
		this.m_DeleteButtons.Insert(idx, uilistExElement2);
		if (add_delete_button)
		{
			uilistExElement2.ui_element.SetActive(true);
		}
		else
		{
			uilistExElement2.ui_element.SetActive(false);
		}
		if (this.m_Sorted)
		{
			this.SortAlphabetically();
		}
		this.UpdateElements();
		this.UpdateSlider();
		this.UpdateButtons();
	}

	public void RemoveElementAt(int idx)
	{
		if (idx >= 0 && idx < this.m_Elements.Count)
		{
			UnityEngine.Object.Destroy(this.m_Elements[idx].ui_element);
			this.m_Elements.RemoveAt(idx);
			UnityEngine.Object.Destroy(this.m_DeleteButtons[idx].ui_element);
			this.m_DeleteButtons.RemoveAt(idx);
			this.UpdateElements();
		}
		else
		{
			DebugUtils.Assert(DebugUtils.AssertType.Info);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.S) || Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			this.SelectNext();
		}
		else if (Input.GetKeyDown(KeyCode.W) || Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			this.SelectPrev();
		}
		this.UpdateSlider();
	}

	public void UpdateElements()
	{
		RectTransform component = base.gameObject.GetComponent<RectTransform>();
		Vector3 zero = Vector3.zero;
		zero.y += component.rect.height * 0.5f;
		Vector3 localPosition = zero;
		localPosition.x -= component.rect.width * 0.5f * this.m_DeleteButtonOffsetX;
		for (int i = 0; i < this.m_Elements.Count; i++)
		{
			if (i < this.m_ListStartIndex || i > this.m_ListStartIndex + this.m_Capacity)
			{
				this.m_Elements[i].ui_element.gameObject.SetActive(false);
				this.m_DeleteButtons[i].ui_element.gameObject.SetActive(false);
			}
			else
			{
				this.m_Elements[i].ui_element.gameObject.SetActive(true);
				this.m_DeleteButtons[i].ui_element.gameObject.SetActive(this.m_Elements[i].show_delete_button);
				this.m_Elements[i].ui_element.transform.localPosition = zero;
				this.m_DeleteButtons[i].ui_element.transform.localPosition = localPosition;
				zero.y -= this.m_Elements[i].ui_element.GetComponent<RectTransform>().rect.height;
				localPosition.y -= this.m_Elements[i].ui_element.GetComponent<RectTransform>().rect.height;
				if (i == this.m_SelectionIndex)
				{
					this.m_Elements[i].ui_element.GetComponentInChildren<Text>().color = this.m_SelectedColor;
				}
				else
				{
					this.m_Elements[i].ui_element.GetComponentInChildren<Text>().color = this.m_NormalColor;
				}
			}
			this.m_Elements[i].idx = i;
			this.m_DeleteButtons[i].idx = i;
		}
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
		this.UpdateElements();
		this.m_LastSliderVal = normalizedValue;
	}

	private void UpdateSliderVal()
	{
		if (!this.m_Slider)
		{
			return;
		}
		int num = this.m_Elements.Count - this.m_Capacity;
		if (num > 0)
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
			}
			else
			{
				this.m_ButtonDown.enabled = false;
			}
		}
	}

	public void Clear()
	{
		this.m_Elements.Clear();
		this.UpdateElements();
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
		this.UpdateElements();
		this.UpdateButtons();
		if (this.m_ChangeSelectionReceiver != null)
		{
			this.m_ChangeSelectionReceiver.OnUIListExChangeSelection(this);
		}
	}

	public void SelectPrev()
	{
		this.m_SelectionIndex--;
		if (this.m_SelectionIndex < 0)
		{
			this.m_SelectionIndex = ((this.m_Elements.Count <= 0) ? 0 : (this.m_Elements.Count - 1));
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
		this.UpdateElements();
		this.UpdateButtons();
		if (this.m_ChangeSelectionReceiver != null)
		{
			this.m_ChangeSelectionReceiver.OnUIListExChangeSelection(this);
		}
	}

	public void SetSelectionIndex(int index)
	{
		this.m_SelectionIndex = index;
		if (this.m_SelectionIndex < 0)
		{
			this.m_SelectionIndex = ((this.m_Elements.Count <= 0) ? 0 : (this.m_Elements.Count - 1));
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
		this.UpdateElements();
		this.UpdateButtons();
		if (this.m_ChangeSelectionReceiver != null)
		{
			this.m_ChangeSelectionReceiver.OnUIListExChangeSelection(this);
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
		return (index < 0 || index >= this.m_Elements.Count) ? string.Empty : this.m_Elements[index].text;
	}

	public int GetSelectedElementData()
	{
		return this.GetElementData(this.m_SelectionIndex);
	}

	public int GetElementData(int index)
	{
		return (index < 0 || index >= this.m_Elements.Count) ? -1 : this.m_Elements[index].data;
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
			this.m_ClickReceiver.OnUIListExClicked(this);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		List<GameObject> hovered = eventData.hovered;
		for (int i = 0; i < hovered.Count; i++)
		{
			GameObject x = hovered[i];
			for (int j = 0; j < this.m_Elements.Count; j++)
			{
				if (x == this.m_Elements[j].ui_element)
				{
					this.OnPointerDownElement(this.m_Elements[j]);
					return;
				}
			}
		}
		for (int k = 0; k < hovered.Count; k++)
		{
			GameObject x2 = hovered[k];
			for (int l = 0; l < this.m_DeleteButtons.Count; l++)
			{
				if (x2 == this.m_DeleteButtons[l].ui_element)
				{
					this.OnPointerDownDeleteButton(this.m_DeleteButtons[l]);
					return;
				}
			}
		}
		if (this.m_MouseDownReceiver != null)
		{
			this.m_MouseDownReceiver.OnUIListExMouseDown(this);
		}
	}

	private void OnPointerDownElement(UIListExElement elem)
	{
		if (this.m_MouseDownReceiver != null)
		{
			this.m_MouseDownReceiver.OnUIListExMouseDownElement(elem);
		}
	}

	private void OnPointerDownDeleteButton(UIListExElement elem)
	{
		this.m_Elements[elem.idx].show_delete_button = false;
		this.UpdateElements();
		if (this.m_MouseDownReceiver != null)
		{
			this.m_MouseDownReceiver.OnUIListExMouseDownDeleteButton(elem);
		}
	}

	private void OnPointerDownText(PointerEventData eventData)
	{
	}

	public void SetFocus(bool focus)
	{
		if (focus != this.m_Focus)
		{
			this.m_Focus = focus;
			this.UpdateElements();
			this.UpdateButtons();
		}
	}

	private static int CompareListByName(UIListExElement i1, UIListExElement i2)
	{
		return i1.text.CompareTo(i2.text);
	}

	public void SortAlphabetically()
	{
		List<UIListExElement> elements = this.m_Elements;
		if (UIListEx.<>f__mg$cache0 == null)
		{
			UIListEx.<>f__mg$cache0 = new Comparison<UIListExElement>(UIListEx.CompareListByName);
		}
		elements.Sort(UIListEx.<>f__mg$cache0);
	}

	[HideInInspector]
	public List<UIListExElement> m_Elements = new List<UIListExElement>();

	private List<UIListExElement> m_DeleteButtons = new List<UIListExElement>();

	public GameObject m_ElementPrefab;

	public GameObject m_DeleteButtonPrefabToInstantiate;

	public float m_DeleteButtonOffsetX;

	private int m_SelectionIndex;

	public int m_Capacity;

	private int m_ListStartIndex;

	public RawImage m_ButtonUp;

	public RawImage m_ButtonDown;

	public Slider m_Slider;

	public Color m_NormalColor = Color.white;

	public Color m_SelectedColor = Color.green;

	public Color m_UnfocusedColor = Color.grey;

	private bool m_Focus;

	public IUIListExClickReceiver m_ClickReceiver;

	public IUIListExMouseDownReceiver m_MouseDownReceiver;

	public IUIListExChangeSelection m_ChangeSelectionReceiver;

	public bool m_Sorted;

	private float m_LastSliderVal;

	[CompilerGenerated]
	private static Comparison<UIListExElement> <>f__mg$cache0;
}
