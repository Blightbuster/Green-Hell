using System;

public interface IYesNoDialogOwner
{
	void OnYesFromDialog();

	void OnNoFromDialog();

	void OnOkFromDialog();
}
