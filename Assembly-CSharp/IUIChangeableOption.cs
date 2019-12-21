using System;

public interface IUIChangeableOption
{
	bool DidValueChange();

	void StoreValue();

	void RevertValue();
}
