using System;

public class UIListElement<T> : IUIListElement
{
	public UIListElement(T data)
	{
		this.data = data;
	}

	public string GetText()
	{
		return this.text;
	}

	public string text;

	public T data;
}
