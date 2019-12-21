using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuDebugLog : MenuDebugScreen, IInputsReceiver, IUIListClickReceiver
{
	public void Start()
	{
		if (this.m_OptionList != null)
		{
			this.m_OptionList.m_ClickReceiver = this;
			this.m_OptionList.AddElement("MenuDebugLog GetLoadedScenes ?", -1);
		}
	}

	public override void OnShow()
	{
		base.OnShow();
		this.m_List.SetFocus(true);
		this.m_List.Clear();
		foreach (string element in CJDebug.m_Log.Split(new char[]
		{
			'\n'
		}))
		{
			this.m_List.AddElement(element, -1);
		}
		this.m_List.SetSelectionIndex(0);
		this.m_ConsoleInput = base.GetComponentInChildren<InputField>();
	}

	protected override void Update()
	{
		base.Update();
		if (Input.GetKeyDown(KeyCode.Return))
		{
			if (this.m_ConsoleInput != null)
			{
				string text;
				ImmediateWindow.Result result = ImmediateWindow.RunCommand(this.m_ConsoleInput.text, out text);
				this.m_List.AddElement<MenuDebugLog.CommandData>(EnumUtils<ImmediateWindow.Result>.GetName(result) + " " + text, new MenuDebugLog.CommandData
				{
					result = result,
					command = this.m_ConsoleInput.text,
					output = text
				});
				this.m_FastTypeHelper = 0;
				return;
			}
		}
		else if (Input.GetKeyDown(KeyCode.Tab))
		{
			if (this.m_List.GetCount() > 0)
			{
				int index = Mathf.Min(this.m_List.GetCount() - 1, this.m_List.GetCount() + this.m_UpDownNavigation);
				MenuDebugLog.CommandData elementData = this.m_List.GetElementData<MenuDebugLog.CommandData>(index);
				ImmediateWindow.Result result2 = elementData.result;
				if (result2 == ImmediateWindow.Result.IncompleteType)
				{
					string[] array = elementData.output.Split(new char[]
					{
						' '
					});
					this.m_FastTypeHelper %= array.Length;
					this.m_ConsoleInput.text = (array[this.m_FastTypeHelper] ?? "");
					this.m_ConsoleInput.ActivateInputField();
					this.m_ConsoleInput.Select();
					this.m_FastTypeHelper++;
					return;
				}
				if (result2 != ImmediateWindow.Result.IncompleteMember)
				{
					return;
				}
				string[] array2 = elementData.output.Split(new char[]
				{
					' '
				});
				this.m_FastTypeHelper %= array2.Length;
				this.m_ConsoleInput.text = elementData.command.Split(new char[]
				{
					' '
				})[0] + " " + array2[this.m_FastTypeHelper];
				this.m_ConsoleInput.ActivateInputField();
				this.m_ConsoleInput.Select();
				this.m_FastTypeHelper++;
				return;
			}
		}
		else if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow)) && this.m_List.GetCount() > 0)
		{
			this.m_UpDownNavigation += (Input.GetKeyDown(KeyCode.DownArrow) ? 1 : -1);
			this.m_UpDownNavigation = Mathf.Min(0, Mathf.Max(this.m_UpDownNavigation, -this.m_List.GetCount() + 1));
			MenuDebugLog.CommandData elementData2 = this.m_List.GetElementData<MenuDebugLog.CommandData>(this.m_List.GetCount() + this.m_UpDownNavigation);
			this.m_ConsoleInput.text = elementData2.command;
		}
	}

	private void OnClear()
	{
		this.m_List.Clear();
		this.m_UpDownNavigation = 0;
	}

	public void OnUIListClicked(UIList list)
	{
		string selectedElementText = list.GetSelectedElementText();
		if (!selectedElementText.Empty())
		{
			string text;
			ImmediateWindow.Result result = ImmediateWindow.RunCommand(selectedElementText, out text);
			this.m_List.AddElement<MenuDebugLog.CommandData>(text, new MenuDebugLog.CommandData
			{
				result = result,
				command = selectedElementText,
				output = text
			});
		}
	}

	private void GetLoadedScenes()
	{
		int sceneCount = SceneManager.sceneCount;
		this.m_List.AddElement("Scenes count: " + sceneCount.ToString(), -1);
		for (int i = 0; i < sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.IsValid() && sceneAt.isLoaded)
			{
				this.m_List.AddElement("Scene '" + sceneAt.name + "'", -1);
			}
		}
	}

	public UIList m_List;

	public UIList m_OptionList;

	private InputField m_ConsoleInput;

	private int m_UpDownNavigation;

	private int m_FastTypeHelper;

	private struct CommandData
	{
		public ImmediateWindow.Result result;

		public string command;

		public string output;
	}
}
