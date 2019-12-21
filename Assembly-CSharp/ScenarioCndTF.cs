using System;
using System.Reflection;
using UnityEngine;

public class ScenarioCndTF : ScenarioElement
{
	public override void Setup()
	{
		base.Setup();
		string[] array = this.m_EncodedContent.Split(new char[]
		{
			':'
		});
		if (array.Length < 4 || array.Length > 7)
		{
			DebugUtils.Assert(string.Concat(new string[]
			{
				"[ScenarioAction:Setup] Error in element - ",
				this.m_Content,
				", node - ",
				this.m_Node.m_Name,
				". Check spelling!"
			}), true, DebugUtils.AssertType.Info);
		}
		string text = array[1];
		Type type = Type.GetType(text);
		UnityEngine.Object objectOfType = ScenarioManager.Get().GetObjectOfType(type);
		DebugUtils.Assert(objectOfType != null, "[ScenarioCndTF:Setup] ERROR - Can't find object " + text, true, DebugUtils.AssertType.Info);
		MethodInfo method = type.GetMethod(array[2]);
		if (method == null)
		{
			DebugUtils.Assert("[ScenarioCndTF:Setup] - missing method - " + array[2], true, DebugUtils.AssertType.Info);
			return;
		}
		string a = array[3].ToLower();
		if (a != "true" && a != "false")
		{
			DebugUtils.Assert(string.Concat(new string[]
			{
				"[ScenarioCndTF:Setup] Missing 'true' or 'false' in element - ",
				this.m_Content,
				", node - ",
				this.m_Node.m_Name,
				". Check spelling!"
			}), true, DebugUtils.AssertType.Info);
			return;
		}
		this.m_Result = bool.Parse(array[3]);
		ParameterInfo[] parameters = method.GetParameters();
		switch (parameters.Length)
		{
		case 0:
			this.m_Method = (BDelegate)Delegate.CreateDelegate(typeof(BDelegate), objectOfType, method, false);
			break;
		case 1:
		{
			Type parameterType = parameters[0].ParameterType;
			if (parameterType == typeof(string))
			{
				this.m_ParamS1 = array[4];
				this.m_MethodS = (BDelegateS)Delegate.CreateDelegate(typeof(BDelegateS), objectOfType, method, false);
			}
			else if (parameterType.IsAssignableFrom(typeof(GameObject)))
			{
				this.m_IsGO1 = true;
				this.m_ParamO1Name = array[4];
				this.m_ParamO1 = MainLevel.Instance.FindObject(array[4], (array.Length > 5) ? array[5] : string.Empty);
				if (this.m_ParamO1 == null)
				{
					this.m_HasNullObject = true;
				}
				this.m_MethodO = (BDelegateO)Delegate.CreateDelegate(typeof(BDelegateO), objectOfType, method, false);
			}
			else if (parameterType == typeof(int))
			{
				this.m_ParamI1 = int.Parse(array[4]);
				this.m_MethodI = (BDelegateI)Delegate.CreateDelegate(typeof(BDelegateI), objectOfType, method, false);
			}
			else if (parameterType == typeof(float))
			{
				this.m_ParamF1 = float.Parse(array[4]);
				this.m_MethodF = (BDelegateF)Delegate.CreateDelegate(typeof(BDelegateF), objectOfType, method, false);
			}
			break;
		}
		case 2:
		{
			Type parameterType = parameters[0].ParameterType;
			if (parameterType == typeof(string))
			{
				this.m_ParamS1 = array[4];
				Type parameterType2 = parameters[1].ParameterType;
				if (parameterType2 == typeof(string))
				{
					if (array.Length < 6)
					{
						DebugUtils.Assert("[ScenarioAction:Setup] Error in element - " + this.m_Content + ". Missing last param!", true, DebugUtils.AssertType.Info);
						return;
					}
					this.m_ParamS2 = array[5];
					this.m_MethodSS = (BDelegateSS)Delegate.CreateDelegate(typeof(BDelegateSS), objectOfType, method, false);
				}
				else if (parameterType2 == typeof(float))
				{
					this.m_ParamF2 = float.Parse(array[5]);
					this.m_MethodSF = (BDelegateSF)Delegate.CreateDelegate(typeof(BDelegateSF), objectOfType, method, false);
				}
				else if (parameterType2 == typeof(int))
				{
					this.m_ParamI2 = int.Parse(array[5]);
					this.m_MethodSI = (BDelegateSI)Delegate.CreateDelegate(typeof(BDelegateSI), objectOfType, method, false);
				}
			}
			else if (parameterType.IsAssignableFrom(typeof(GameObject)))
			{
				this.m_IsGO1 = true;
				this.m_ParamO1Name = array[4];
				this.m_ParamO1 = MainLevel.Instance.GetUniqueObject(array[4]);
				if (this.m_ParamO1 == null)
				{
					this.m_HasNullObject = true;
					Debug.Log("ScenarioCndTF + null object added" + array[4]);
				}
				if (array.Length < 6)
				{
					DebugUtils.Assert("Too little parameters " + this.m_Content + "CheckSyntax.", true, DebugUtils.AssertType.Info);
				}
				this.m_ParamF2 = float.Parse(array[5]);
				this.m_MethodOF = (BDelegateOF)Delegate.CreateDelegate(typeof(BDelegateOF), objectOfType, method, false);
			}
			break;
		}
		case 3:
			this.m_IsGO1 = true;
			this.m_IsGO2 = true;
			this.m_ParamO1Name = array[4];
			this.m_ParamO1 = MainLevel.Instance.GetUniqueObject(array[4]);
			this.m_ParamO2Name = array[5];
			this.m_ParamO2 = MainLevel.Instance.GetUniqueObject(array[5]);
			this.m_ParamF3 = float.Parse(array[6]);
			this.m_MethodOOF = (BDelegateOOF)Delegate.CreateDelegate(typeof(BDelegateOOF), objectOfType, method, false);
			break;
		default:
			DebugUtils.Assert(DebugUtils.AssertType.Info);
			break;
		}
		DebugUtils.Assert(this.m_Method != null || this.m_MethodO != null || this.m_MethodS != null || this.m_MethodSS != null || this.m_MethodSF != null || this.m_MethodOF != null || this.m_MethodOOF != null || this.m_MethodSI != null || this.m_MethodF != null || this.m_MethodI != null, this.m_EncodedContent, true, DebugUtils.AssertType.Info);
	}

	protected override bool ShouldComplete()
	{
		bool flag;
		if (this.m_Method != null)
		{
			flag = this.m_Method();
		}
		else if (this.m_MethodO != null)
		{
			flag = (this.m_ParamO1 != null && this.m_MethodO(this.m_ParamO1));
		}
		else if (this.m_MethodS != null)
		{
			flag = this.m_MethodS(this.m_ParamS1);
		}
		else if (this.m_MethodSS != null)
		{
			flag = this.m_MethodSS(this.m_ParamS1, this.m_ParamS2);
		}
		else if (this.m_MethodSF != null)
		{
			flag = this.m_MethodSF(this.m_ParamS1, this.m_ParamF2);
		}
		else if (this.m_MethodOF != null)
		{
			flag = (this.m_ParamO1 != null && this.m_MethodOF(this.m_ParamO1, this.m_ParamF2));
		}
		else if (this.m_MethodOOF != null)
		{
			flag = (this.m_ParamO1 != null && this.m_ParamO2 != null && this.m_MethodOOF(this.m_ParamO1, this.m_ParamO2, this.m_ParamF3));
		}
		else if (this.m_MethodI != null)
		{
			flag = this.m_MethodI(this.m_ParamI1);
		}
		else if (this.m_MethodSI != null)
		{
			flag = this.m_MethodSI(this.m_ParamS1, this.m_ParamI2);
		}
		else
		{
			if (this.m_MethodF == null)
			{
				DebugUtils.Assert(DebugUtils.AssertType.Info);
				return true;
			}
			flag = this.m_MethodF(this.m_ParamF1);
		}
		return flag == this.m_Result;
	}

	public override void Load(ScenarioNode node, int index)
	{
		base.Load(node, index);
		if (this.m_HasNullObject)
		{
			return;
		}
		if ((this.m_IsGO1 && this.m_ParamO1 == null) || (this.m_IsGO2 && this.m_ParamO2 == null))
		{
			this.m_HasNullObject = true;
		}
	}

	private BDelegate m_Method;

	private BDelegateO m_MethodO;

	private BDelegateS m_MethodS;

	private BDelegateSS m_MethodSS;

	private BDelegateSF m_MethodSF;

	private BDelegateOF m_MethodOF;

	private BDelegateOOF m_MethodOOF;

	private BDelegateI m_MethodI;

	private BDelegateF m_MethodF;

	private BDelegateSI m_MethodSI;

	private bool m_Result = true;

	private int m_ParamI2;

	private float m_ParamF2;

	private float m_ParamF3;

	private string m_ParamS1 = string.Empty;

	private string m_ParamS2 = string.Empty;

	private int m_ParamI1;

	private float m_ParamF1;
}
