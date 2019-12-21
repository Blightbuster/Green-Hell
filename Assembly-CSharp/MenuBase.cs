using System;
using System.Collections.Generic;
using System.Linq;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class MenuBase : MonoBehaviour
{
	public virtual void OnShow()
	{
		if (GreenHellGame.IsPadControllerActive())
		{
			if (this.m_ActiveMenuOption != null && this.m_ActiveMenuOption.m_Button != null)
			{
				this.m_ActiveMenuOption.m_Button.Select();
				this.m_ActiveMenuOption.m_Button.OnSelect(null);
				return;
			}
			foreach (MenuBase.MenuOptionData menuOptionData in this.m_OptionsObjects.Values)
			{
				if (this.IsMenuButtonEnabled(menuOptionData.m_Button))
				{
					UIButtonEx component = menuOptionData.m_Button.GetComponent<UIButtonEx>();
					if (!component || component.m_MoveWhenFocused)
					{
						this.m_ActiveMenuOption = menuOptionData;
						break;
					}
				}
				if (this.IsMenuSliderEnabled(menuOptionData.m_Slider))
				{
					this.m_ActiveMenuOption = menuOptionData;
					break;
				}
				if (this.IsMenuSelectButtonEnabled(menuOptionData.m_SelectButton))
				{
					this.m_ActiveMenuOption = menuOptionData;
					break;
				}
			}
		}
	}

	public virtual void OnHide()
	{
	}

	public virtual void OnBack()
	{
	}

	public void OnPostShow()
	{
		this.RememberOptionValues();
	}

	protected virtual void Awake()
	{
		if (this.m_AutoFindButtons)
		{
			Transform transform = base.transform;
			if (transform)
			{
				List<GameObject> list = new List<GameObject>();
				foreach (RectTransform rectTransform in General.GetComponentsDeepChild<RectTransform>(transform.gameObject))
				{
					if (rectTransform.GetComponent<Button>() || rectTransform.GetComponent<UISliderEx>() || rectTransform.GetComponent<UISelectButton>())
					{
						list.Add(rectTransform.gameObject);
					}
				}
				foreach (GameObject gameObject in list)
				{
					Button component = gameObject.GetComponent<Button>();
					if (component)
					{
						this.AddMenuButton(component, null);
					}
					else
					{
						UISliderEx component2 = gameObject.GetComponent<UISliderEx>();
						if (component2)
						{
							this.AddMenuSlider(component2);
						}
						else
						{
							UISelectButton component3 = gameObject.GetComponent<UISelectButton>();
							if (component3)
							{
								this.AddSelectButton(component3);
							}
						}
					}
				}
			}
		}
		this.m_ChangeableOptions = base.GetComponentsInChildren<IUIChangeableOption>();
	}

	protected virtual void Update()
	{
		this.UpdateMenuOptions();
	}

	public virtual void OnSelectionChanged(UISelectButton button, string option)
	{
	}

	public virtual void OnScreenResolutionChange(int width, int height)
	{
	}

	public void RememberOptionValues()
	{
		if (this.m_ChangeableOptions == null)
		{
			return;
		}
		IUIChangeableOption[] changeableOptions = this.m_ChangeableOptions;
		for (int i = 0; i < changeableOptions.Length; i++)
		{
			changeableOptions[i].StoreValue();
		}
	}

	public void RevertOptionValues()
	{
		if (this.m_ChangeableOptions == null)
		{
			return;
		}
		IUIChangeableOption[] changeableOptions = this.m_ChangeableOptions;
		for (int i = 0; i < changeableOptions.Length; i++)
		{
			changeableOptions[i].RevertValue();
		}
	}

	public bool IsAnyOptionModified()
	{
		if (this.m_ChangeableOptions == null)
		{
			return false;
		}
		IUIChangeableOption[] changeableOptions = this.m_ChangeableOptions;
		for (int i = 0; i < changeableOptions.Length; i++)
		{
			if (changeableOptions[i].DidValueChange())
			{
				return true;
			}
		}
		return false;
	}

	public virtual void AddMenuButton(Button button, string text = null)
	{
		if (button == null)
		{
			return;
		}
		if (button is UISelectButtonArrow)
		{
			return;
		}
		Text componentInChildren = button.GetComponentInChildren<Text>();
		if (componentInChildren == null)
		{
			DebugUtils.Assert(componentInChildren, "Button without Text component!", true, DebugUtils.AssertType.Info);
			return;
		}
		if (!this.m_OptionsObjects.Values.Any((MenuBase.MenuOptionData d) => d.m_Button == button))
		{
			Dictionary<GameObject, MenuBase.MenuOptionData> optionsObjects = this.m_OptionsObjects;
			GameObject gameObject = button.gameObject;
			MenuBase.MenuOptionData menuOptionData = new MenuBase.MenuOptionData();
			menuOptionData.m_Button = button;
			menuOptionData.m_Object = button.gameObject;
			menuOptionData.m_Texts = new Text[0];
			MenuBase.MenuOptionData menuOptionData2 = menuOptionData;
			UIButtonEx uibuttonEx = button as UIButtonEx;
			menuOptionData2.m_AnimatedRectTransform = ((uibuttonEx == null || uibuttonEx.m_MoveWhenFocused) ? componentInChildren.GetComponent<RectTransform>() : null);
			menuOptionData.m_ExtentRectTransforms = new RectTransform[]
			{
				button.GetComponent<RectTransform>()
			};
			menuOptionData.m_InteractRectTransforms = new RectTransform[]
			{
				button.GetComponent<RectTransform>()
			};
			optionsObjects.Add(gameObject, menuOptionData);
		}
		if (!text.Empty())
		{
			componentInChildren.text = GreenHellGame.Instance.GetLocalization().Get(text, true);
		}
	}

	public virtual void AddMenuSlider(UISliderEx slider)
	{
		if (slider == null)
		{
			return;
		}
		Transform parent = slider.transform.parent;
		Transform transform = parent.FindDeepChild("Title");
		if (!transform)
		{
			DebugUtils.Assert(transform, "Slider object is missing 'Title'!", true, DebugUtils.AssertType.Info);
			return;
		}
		slider.enabled = true;
		if (!this.m_OptionsObjects.Values.Any((MenuBase.MenuOptionData d) => d.m_Slider == slider))
		{
			Text componentInChildren = transform.GetComponentInChildren<Text>();
			RectTransform rectTransform = slider.transform.parent.GetComponent<RectTransform>() ?? transform.GetComponent<RectTransform>();
			this.m_OptionsObjects.Add(parent.gameObject, new MenuBase.MenuOptionData
			{
				m_Slider = slider,
				m_Object = parent.gameObject,
				m_Texts = new Text[]
				{
					componentInChildren
				},
				m_ExtentRectTransforms = new RectTransform[]
				{
					rectTransform,
					slider.GetComponent<RectTransform>()
				},
				m_InteractRectTransforms = new RectTransform[]
				{
					slider.transform.FindDeepChild("Fill").GetComponent<RectTransform>(),
					slider.transform.FindDeepChild("Handle").GetComponent<RectTransform>()
				}
			});
		}
	}

	public virtual void AddSelectButton(UISelectButton select)
	{
		if (select == null)
		{
			return;
		}
		if (select.m_LeftArrow == null || select.m_RightArrow == null)
		{
			return;
		}
		Transform transform = select.transform.FindDeepChild("Title");
		if (!transform)
		{
			DebugUtils.Assert(transform, "Slider object is missing 'Title'!", true, DebugUtils.AssertType.Info);
			return;
		}
		Transform transform2 = select.transform.FindDeepChild("Text");
		if (!transform2)
		{
			DebugUtils.Assert(transform2, "Slider object is missing 'Text' option field!", true, DebugUtils.AssertType.Info);
			return;
		}
		if (!this.m_OptionsObjects.Values.Any((MenuBase.MenuOptionData d) => d.m_SelectButton == select))
		{
			Text componentInChildren = transform.GetComponentInChildren<Text>();
			Text componentInChildren2 = transform2.GetComponentInChildren<Text>();
			RectTransform rectTransform = select.GetComponent<RectTransform>() ?? transform.GetComponent<RectTransform>();
			this.m_OptionsObjects.Add(select.transform.gameObject, new MenuBase.MenuOptionData
			{
				m_SelectButton = select,
				m_Object = select.gameObject,
				m_Texts = new Text[]
				{
					componentInChildren,
					componentInChildren2
				},
				m_ExtentRectTransforms = new RectTransform[]
				{
					rectTransform,
					componentInChildren2.GetComponent<RectTransform>(),
					select.m_LeftArrow.GetComponent<RectTransform>(),
					select.m_RightArrow.GetComponent<RectTransform>()
				},
				m_InteractRectTransforms = new RectTransform[]
				{
					select.m_LeftArrow.GetComponent<RectTransform>(),
					select.m_RightArrow.GetComponent<RectTransform>()
				}
			});
		}
	}

	public virtual bool IsMenuButtonEnabled(Button b)
	{
		return b != null && b.enabled && b.gameObject.activeSelf;
	}

	public virtual bool IsMenuSliderEnabled(Slider s)
	{
		return s != null && s.enabled && s.gameObject.activeSelf;
	}

	public virtual bool IsMenuSelectButtonEnabled(UISelectButton s)
	{
		return s != null && s.enabled && s.gameObject.activeSelf;
	}

	public virtual void UpdateMenuOptions()
	{
		if (this.m_OptionsObjects.Count > 0)
		{
			if (GreenHellGame.IsPCControllerActive())
			{
				this.m_ActiveMenuOption = null;
				foreach (MenuBase.MenuOptionData data in this.m_OptionsObjects.Values)
				{
					this.UpdateActiveMenuOption(data);
				}
			}
			foreach (MenuBase.MenuOptionData data2 in this.m_OptionsObjects.Values)
			{
				this.UpdateMenuOptionAnimation(data2);
			}
			if (!GreenHellGame.IsYesNoDialogActive())
			{
				CursorManager cursorManager = CursorManager.Get();
				if (cursorManager == null)
				{
					return;
				}
				cursorManager.SetCursor((this.m_ActiveMenuOption != null && this.m_ActiveMenuOptionCanInteract && !GreenHellGame.GetFadeSystem().IsFadeVisible()) ? CursorManager.TYPE.MouseOver : CursorManager.TYPE.Normal);
			}
		}
	}

	public void UpdateActiveMenuOption(MenuBase.MenuOptionData data)
	{
		if (data.m_Object == null)
		{
			return;
		}
		bool flag = true;
		if (data.m_Button && !this.IsMenuButtonEnabled(data.m_Button))
		{
			flag = false;
		}
		else if (data.m_Slider && !this.IsMenuSliderEnabled(data.m_Slider))
		{
			flag = false;
		}
		else if (data.m_SelectButton && !this.IsMenuSelectButtonEnabled(data.m_SelectButton))
		{
			flag = false;
		}
		else if (!data.m_Object.activeSelf)
		{
			flag = false;
		}
		if (data.m_Button)
		{
			data.m_Button.interactable = flag;
		}
		else if (data.m_Slider)
		{
			data.m_Slider.interactable = flag;
		}
		else if (data.m_SelectButton)
		{
			data.m_SelectButton.enabled = flag;
		}
		if (!flag)
		{
			data.SetColorAlpha(MenuScreen.s_InactiveButtonsAlpha);
			return;
		}
		MenuBase.MenuOptionData activeMenuOption = this.m_ActiveMenuOption;
		bool? flag2;
		if (activeMenuOption == null)
		{
			flag2 = null;
		}
		else
		{
			UISliderEx slider = activeMenuOption.m_Slider;
			flag2 = ((slider != null) ? new bool?(slider.m_IsDragged) : null);
		}
		if (flag2 ?? false)
		{
			return;
		}
		if (data.m_Slider && data.m_Slider.m_IsDragged)
		{
			this.m_ActiveMenuOption = data;
			this.m_ActiveMenuOptionCanInteract = false;
			return;
		}
		Vector2 screenPoint = Input.mousePosition;
		RectTransform[] extentRectTransforms = data.m_ExtentRectTransforms;
		for (int i = 0; i < extentRectTransforms.Length; i++)
		{
			if (RectTransformUtility.RectangleContainsScreenPoint(extentRectTransforms[i], screenPoint))
			{
				this.m_ActiveMenuOption = data;
				this.m_ActiveMenuOptionCanInteract = false;
				RectTransform[] interactRectTransforms = data.m_InteractRectTransforms;
				for (int j = 0; j < interactRectTransforms.Length; j++)
				{
					if (RectTransformUtility.RectangleContainsScreenPoint(interactRectTransforms[j], screenPoint))
					{
						this.m_ActiveMenuOptionCanInteract = true;
						return;
					}
				}
				return;
			}
		}
	}

	public void UpdateMenuOptionAnimation(MenuBase.MenuOptionData data)
	{
		if (!data.IsEnabled())
		{
			return;
		}
		if (data.m_Button == null)
		{
			data.SetColorAlpha((this.m_ActiveMenuOption == data) ? MenuScreen.s_ButtonsHighlightedAlpha : MenuScreen.s_ButtonsAlpha);
		}
		if (data.m_AnimatedRectTransform != null)
		{
			Vector3 localPosition = data.m_AnimatedRectTransform.localPosition;
			float num = Mathf.Ceil(((this.m_ActiveMenuOption == data) ? this.SELECTED_OPTION_OFFSET.x : 0f) + data.GetInitialPosition().x - localPosition.x) * Mathf.Min(1f, Time.unscaledDeltaTime * 10f);
			localPosition.x += num;
			data.m_AnimatedRectTransform.localPosition = localPosition;
			if (this.m_ActiveMenuOption == data)
			{
				data.m_Button.Select();
				data.m_Button.OnSelect(null);
			}
		}
	}

	[HideInInspector]
	public Dictionary<GameObject, MenuBase.MenuOptionData> m_OptionsObjects = new Dictionary<GameObject, MenuBase.MenuOptionData>();

	public IUIChangeableOption[] m_ChangeableOptions;

	[HideInInspector]
	public MenuBase.MenuOptionData m_ActiveMenuOption;

	private bool m_ActiveMenuOptionCanInteract;

	public bool m_AutoFindButtons;

	public readonly Vector2 SELECTED_OPTION_OFFSET = new Vector2(10f, 0f);

	public const float ANIMATION_SPEED = 10f;

	public class MenuOptionData
	{
		public void SetBackgroundVisible(bool visible)
		{
			if (!this.m_Background.v2)
			{
				this.m_Background.v1 = this.m_Object.FindChild("BG");
				this.m_Background.v2 = true;
			}
			GameObject v = this.m_Background.v1;
			if (v == null)
			{
				return;
			}
			v.SetActive(visible);
		}

		public Vector3 GetInitialPosition()
		{
			if (this.m_InitialPosition == null)
			{
				RectTransform animatedRectTransform = this.m_AnimatedRectTransform;
				this.m_InitialPosition = ((animatedRectTransform != null) ? new Vector3?(animatedRectTransform.localPosition) : null);
			}
			return this.m_InitialPosition.Value;
		}

		public void SetColorAlpha(float alpha)
		{
			if (this.m_Texts != null)
			{
				for (int i = 0; i < this.m_Texts.Length; i++)
				{
					Color color = this.m_Texts[i].color;
					color.a = alpha;
					this.m_Texts[i].color = color;
				}
			}
			if (this.m_Slider != null)
			{
				RectTransform fillRect = this.m_Slider.fillRect;
				Image image = (fillRect != null) ? fillRect.GetComponent<Image>() : null;
				if (image)
				{
					Color color2 = image.color;
					color2.a = alpha;
					image.color = color2;
				}
			}
		}

		public bool IsEnabled()
		{
			if (this.m_Button)
			{
				return this.m_Button.interactable;
			}
			if (this.m_Slider)
			{
				return this.m_Slider.interactable;
			}
			return !this.m_SelectButton || this.m_SelectButton.enabled;
		}

		public GameObject m_Object;

		public Button m_Button;

		public Text[] m_Texts;

		public UISliderEx m_Slider;

		public UISelectButton m_SelectButton;

		public RectTransform[] m_ExtentRectTransforms;

		public RectTransform[] m_InteractRectTransforms;

		public RectTransform m_AnimatedRectTransform;

		private CJPair<GameObject, bool> m_Background;

		private Vector3? m_InitialPosition;
	}
}
