using System;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldCJ : MonoBehaviour
{
	public string text
	{
		get
		{
			return this.m_InputText.text;
		}
		set
		{
			this.m_InputField.text = value;
			this.m_InputText.text = value;
		}
	}

	private void Awake()
	{
		if (this.m_InputField == null)
		{
			this.m_InputField = base.GetComponent<InputField>();
		}
		if (this.m_InputField == null)
		{
			this.m_InputField = base.gameObject.AddComponent<InputField>();
			this.m_InputField.textComponent = this.m_InputTextInvisible;
			this.m_InputField.placeholder = this.m_Placeholder;
		}
	}

	protected void LateUpdate()
	{
		if (this.m_InputField.text != null && this.m_InputField.characterLimit > 0 && this.m_InputField.text.Length > this.m_InputField.characterLimit)
		{
			this.m_InputText.text = this.m_InputField.text.Substring(0, this.m_InputField.characterLimit);
		}
		else
		{
			this.m_InputText.text = this.m_InputField.text;
		}
		this.m_InputText.enabled = !string.IsNullOrEmpty(this.m_InputText.text);
	}

	public void ActivateInputField()
	{
		this.m_InputField.ActivateInputField();
	}

	public void DeactivateInputField()
	{
		this.m_InputField.DeactivateInputField();
	}

	public Text m_InputText;

	public Text m_InputTextInvisible;

	public Text m_Placeholder;

	public InputField m_InputField;
}
