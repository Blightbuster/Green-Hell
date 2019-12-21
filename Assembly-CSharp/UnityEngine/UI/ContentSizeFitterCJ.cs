using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
	[AddComponentMenu("Layout/Content Size FitterCJ", 141)]
	[ExecuteAlways]
	[RequireComponent(typeof(RectTransform))]
	public class ContentSizeFitterCJ : UIBehaviour, ILayoutSelfController, ILayoutController
	{
		protected override void Awake()
		{
			base.Awake();
			this.InitLayoutElements();
		}

		protected ContentSizeFitterCJ()
		{
		}

		public ContentSizeFitterCJ.FitMode horizontalFit
		{
			get
			{
				return this.m_HorizontalFit;
			}
			set
			{
				if (!ContentSizeFitterCJ.SetPropertyUtility.SetStruct<ContentSizeFitterCJ.FitMode>(ref this.m_HorizontalFit, value))
				{
					return;
				}
				this.SetDirty();
			}
		}

		public ContentSizeFitterCJ.FitMode verticalFit
		{
			get
			{
				return this.m_VerticalFit;
			}
			set
			{
				if (!ContentSizeFitterCJ.SetPropertyUtility.SetStruct<ContentSizeFitterCJ.FitMode>(ref this.m_VerticalFit, value))
				{
					return;
				}
				this.SetDirty();
			}
		}

		private RectTransform rectTransform
		{
			get
			{
				if (this.m_Rect == null)
				{
					this.m_Rect = base.GetComponent<RectTransform>();
				}
				return this.m_Rect;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			this.SetDirty();
		}

		protected override void OnDisable()
		{
			this.m_Tracker.Clear();
			LayoutRebuilder.MarkLayoutForRebuild(this.rectTransform);
			base.OnDisable();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			this.SetDirty();
		}

		private void HandleSelfFittingAlongAxis(int axis)
		{
			ContentSizeFitterCJ.FitMode fitMode = (axis != 0) ? this.verticalFit : this.horizontalFit;
			if (fitMode == ContentSizeFitterCJ.FitMode.Unconstrained)
			{
				this.m_Tracker.Add(this, this.rectTransform, DrivenTransformProperties.None);
				return;
			}
			this.m_Tracker.Add(this, this.rectTransform, (axis != 0) ? DrivenTransformProperties.SizeDeltaY : DrivenTransformProperties.SizeDeltaX);
			if (fitMode == ContentSizeFitterCJ.FitMode.MinSize)
			{
				this.rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, this.GetMinSize(this.m_Rect, axis));
				return;
			}
			this.rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, this.GetPreferredSize(this.m_Rect, axis));
		}

		public float GetMinSize(RectTransform rect, int axis)
		{
			if (axis == 0)
			{
				return LayoutUtility.GetMinWidth(rect);
			}
			return LayoutUtility.GetMinHeight(rect);
		}

		public float GetPreferredSize(RectTransform rect, int axis)
		{
			if (axis == 0)
			{
				return this.GetPreferredWidth(rect);
			}
			return this.GetPreferredHeight(rect);
		}

		public float GetPreferredWidth(RectTransform rect)
		{
			return Mathf.Max(LayoutUtility.GetLayoutProperty(rect, (ILayoutElement e) => e.minWidth, 0f), this.GetLayoutProperty(rect, (ILayoutElement e) => e.preferredWidth, 0f));
		}

		public float GetPreferredHeight(RectTransform rect)
		{
			return Mathf.Max(LayoutUtility.GetLayoutProperty(rect, (ILayoutElement e) => e.minHeight, 0f), this.GetLayoutProperty(rect, (ILayoutElement e) => e.preferredHeight, 0f));
		}

		public float GetLayoutProperty(RectTransform rect, Func<ILayoutElement, float> property, float defaultValue)
		{
			if (rect == null)
			{
				return 0f;
			}
			float num = defaultValue;
			for (int i = 0; i < this.m_LayoutElements.Count; i++)
			{
				ILayoutElement layoutElement = this.m_LayoutElements[i] as ILayoutElement;
				if (!(layoutElement is Behaviour) || ((Behaviour)layoutElement).isActiveAndEnabled)
				{
					float num2 = property(layoutElement);
					if ((double)num2 >= 0.0)
					{
						num += num2;
					}
				}
			}
			return num;
		}

		private void InitLayoutElements()
		{
			this.m_LayoutElements.Clear();
			for (int i = 0; i < base.transform.childCount; i++)
			{
				foreach (ILayoutElement layoutElement in base.transform.GetChild(i).GetComponents<ILayoutElement>())
				{
					this.m_LayoutElements.Add(layoutElement as Component);
				}
			}
		}

		public virtual void SetLayoutHorizontal()
		{
			this.m_Tracker.Clear();
			this.HandleSelfFittingAlongAxis(0);
		}

		public virtual void SetLayoutVertical()
		{
			this.HandleSelfFittingAlongAxis(1);
		}

		protected void SetDirty()
		{
			if (!this.IsActive())
			{
				return;
			}
			LayoutRebuilder.MarkLayoutForRebuild(this.rectTransform);
		}

		[SerializeField]
		protected ContentSizeFitterCJ.FitMode m_HorizontalFit;

		[SerializeField]
		protected ContentSizeFitterCJ.FitMode m_VerticalFit;

		[NonSerialized]
		private RectTransform m_Rect;

		private DrivenRectTransformTracker m_Tracker;

		private List<Component> m_LayoutElements = new List<Component>();

		internal static class SetPropertyUtility
		{
			public static bool SetColor(ref Color currentValue, Color newValue)
			{
				if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a)
				{
					return false;
				}
				currentValue = newValue;
				return true;
			}

			public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
			{
				if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
				{
					return false;
				}
				currentValue = newValue;
				return true;
			}

			public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
			{
				if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
				{
					return false;
				}
				currentValue = newValue;
				return true;
			}
		}

		public enum FitMode
		{
			Unconstrained,
			MinSize,
			PreferredSize
		}
	}
}
