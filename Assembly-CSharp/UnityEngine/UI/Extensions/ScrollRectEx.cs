using System;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/ScrollRectEx")]
	public class ScrollRectEx : ScrollRect
	{
		protected override void Start()
		{
			if (base.horizontalScrollbar)
			{
				base.horizontalScrollbar.gameObject.SetActive(false);
			}
			if (base.verticalScrollbar)
			{
				base.verticalScrollbar.gameObject.SetActive(false);
			}
			base.Start();
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			if (base.horizontalScrollbar)
			{
				base.horizontalScrollbar.size = 0f;
			}
			if (base.verticalScrollbar)
			{
				base.verticalScrollbar.size = 0f;
			}
		}

		public override void Rebuild(CanvasUpdate executing)
		{
			base.Rebuild(executing);
			if (base.horizontalScrollbar)
			{
				base.horizontalScrollbar.size = 0f;
			}
			if (base.verticalScrollbar)
			{
				base.verticalScrollbar.size = 0f;
			}
		}
	}
}
