using System;

public interface IUIListExMouseDownReceiver
{
	void OnUIListExMouseDown(UIListEx list);

	void OnUIListExMouseDownElement(UIListExElement go);

	void OnUIListExMouseDownDeleteButton(UIListExElement go);
}
