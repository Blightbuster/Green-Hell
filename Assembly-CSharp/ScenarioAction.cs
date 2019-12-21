using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ScenarioAction : ScenarioElement
{
	public override void Setup()
	{
		base.Setup();
		this.m_PerformOnLoad = this.m_ScenarioSyntaxData.m_PerformOnLoad;
		string[] array = this.m_EncodedContent.Split(new char[]
		{
			':'
		});
		if (array.Length < 3 || array.Length > 8)
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
		DebugUtils.Assert(objectOfType != null, "[ScenarioAction:Setup] ERROR - Can't find object " + text, true, DebugUtils.AssertType.Info);
		MethodInfo method = type.GetMethod(array[2]);
		if (method == null)
		{
			DebugUtils.Assert("[ScenarioAction:Setup] Can't find method - " + array[2] + " - content - " + this.m_Content, true, DebugUtils.AssertType.Info);
		}
		ParameterInfo[] parameters = method.GetParameters();
		switch (parameters.Length)
		{
		case 0:
			this.m_Method = (VDelegate)Delegate.CreateDelegate(typeof(VDelegate), objectOfType, method, false);
			break;
		case 1:
		{
			Type parameterType = parameters[0].ParameterType;
			string text2 = array[3];
			if (parameterType == typeof(int))
			{
				this.m_ParamI1 = int.Parse(text2);
				this.m_MethodI = (VDelegateI)Delegate.CreateDelegate(typeof(VDelegateI), objectOfType, method, false);
			}
			else if (parameterType == typeof(bool))
			{
				this.m_ParamB1 = bool.Parse(text2);
				this.m_MethodB = (VDelegateB)Delegate.CreateDelegate(typeof(VDelegateB), objectOfType, method, false);
			}
			else if (parameterType == typeof(float))
			{
				this.m_ParamF1 = float.Parse(text2);
				this.m_MethodF = (VDelegateF)Delegate.CreateDelegate(typeof(VDelegateF), objectOfType, method, false);
			}
			else if (parameterType == typeof(string))
			{
				this.m_ParamS1 = text2;
				this.m_MethodS = (VDelegateS)Delegate.CreateDelegate(typeof(VDelegateS), objectOfType, method, false);
			}
			else if (parameterType.IsAssignableFrom(typeof(GameObject)))
			{
				this.m_IsGO1 = true;
				this.m_ParamO1Name = text2;
				this.m_ParamO1 = MainLevel.Instance.FindObject(text2, (array.Length > 4) ? array[4] : string.Empty);
				if (this.m_ParamO1 == null)
				{
					this.m_HasNullObject = true;
				}
				this.m_MethodO = (VDelegateO)Delegate.CreateDelegate(typeof(VDelegateO), objectOfType, method, false);
			}
			break;
		}
		case 2:
		{
			Type parameterType2 = parameters[0].ParameterType;
			Type parameterType3 = parameters[1].ParameterType;
			string text3 = array[3];
			string text4 = array[4];
			if (parameterType2 == typeof(string) && parameterType3 == typeof(bool))
			{
				this.m_ParamS1 = text3;
				this.m_ParamB2 = bool.Parse(text4);
				this.m_MethodSB = (VDelegateSB)Delegate.CreateDelegate(typeof(VDelegateSB), objectOfType, method, false);
			}
			else if (parameterType2 == typeof(string) && parameterType3 == typeof(string))
			{
				this.m_ParamS1 = text3;
				this.m_ParamS2 = text4;
				this.m_MethodSS = (VDelegateSS)Delegate.CreateDelegate(typeof(VDelegateSS), objectOfType, method, false);
			}
			else if (parameterType2 == typeof(string) && parameterType3 == typeof(float))
			{
				this.m_ParamS1 = text3;
				this.m_ParamF2 = float.Parse(text4);
				this.m_MethodSF = (VDelegateSF)Delegate.CreateDelegate(typeof(VDelegateSF), objectOfType, method, false);
			}
			else if (parameterType2 == typeof(int) && parameterType3 == typeof(int))
			{
				this.m_ParamI1 = int.Parse(text3);
				this.m_ParamI2 = int.Parse(text4);
				this.m_MethodII = (VDelegateII)Delegate.CreateDelegate(typeof(VDelegateII), objectOfType, method, false);
			}
			else if (parameterType2 == typeof(int) && parameterType3 == typeof(float))
			{
				this.m_ParamI1 = int.Parse(text3);
				this.m_ParamF2 = float.Parse(text4);
				this.m_MethodIF = (VDelegateIF)Delegate.CreateDelegate(typeof(VDelegateIF), objectOfType, method, false);
			}
			else if (parameterType2.IsAssignableFrom(typeof(GameObject)) && parameterType3.IsAssignableFrom(typeof(GameObject)))
			{
				this.m_ParamO1Name = text3;
				this.m_ParamO2Name = text4;
				this.m_ParamO1 = MainLevel.Instance.GetUniqueObject(text3);
				this.m_ParamO2 = MainLevel.Instance.GetUniqueObject(text4);
				this.m_IsGO1 = true;
				this.m_IsGO2 = true;
				if (this.m_ParamO1 == null || this.m_ParamO2 == null)
				{
					this.m_HasNullObject = true;
				}
				this.m_MethodOO = (VDelegateOO)Delegate.CreateDelegate(typeof(VDelegateOO), objectOfType, method, false);
			}
			else if (parameterType2.IsAssignableFrom(typeof(GameObject)) && parameterType3 == typeof(float))
			{
				this.m_IsGO1 = true;
				this.m_ParamO1Name = text3;
				this.m_ParamO1 = MainLevel.Instance.GetUniqueObject(text3);
				this.m_ParamF2 = float.Parse(text4);
				if (this.m_ParamO1 == null)
				{
					this.m_HasNullObject = true;
				}
				this.m_MethodOF = (VDelegateOF)Delegate.CreateDelegate(typeof(VDelegateOF), objectOfType, method, false);
			}
			else if (parameterType2.IsAssignableFrom(typeof(GameObject)) && parameterType3 == typeof(string))
			{
				this.m_IsGO1 = true;
				this.m_ParamO1Name = text3;
				this.m_ParamO1 = MainLevel.Instance.GetUniqueObject(text3);
				this.m_ParamS2 = text4;
				if (this.m_ParamO1 == null)
				{
					this.m_HasNullObject = true;
				}
				this.m_MethodOS = (VDelegateOS)Delegate.CreateDelegate(typeof(VDelegateOS), objectOfType, method, false);
				if (method.Name == "EnableComponent" || method.Name == "DisableComponent")
				{
					this.m_ComponentName = text4;
				}
			}
			else if (parameterType2.IsAssignableFrom(typeof(GameObject)) && parameterType3 == typeof(bool))
			{
				this.m_IsGO1 = true;
				this.m_ParamO1Name = text3;
				this.m_ParamO1 = MainLevel.Instance.GetUniqueObject(text3);
				this.m_ParamB2 = bool.Parse(text4);
				if (this.m_ParamO1 == null)
				{
					this.m_HasNullObject = true;
				}
				this.m_MethodOB = (VDelegateOB)Delegate.CreateDelegate(typeof(VDelegateOB), objectOfType, method, false);
			}
			else if (parameterType2.IsAssignableFrom(typeof(List<GameObject>)) && parameterType3 == typeof(bool))
			{
				this.m_ParamListO1 = new List<GameObject>();
				this.m_ParamListO1Names = text3.Split(new char[]
				{
					';'
				});
				for (int i = 0; i < this.m_ParamListO1Names.Length; i++)
				{
					GameObject uniqueObject = MainLevel.Instance.GetUniqueObject(this.m_ParamListO1Names[i]);
					if (uniqueObject == null)
					{
						this.m_HasNullObject = true;
					}
					else
					{
						this.m_ParamListO1.Add(uniqueObject);
					}
				}
				this.m_ParamB2 = bool.Parse(text4);
				this.m_MethodListOB = (VDelegateListOB)Delegate.CreateDelegate(typeof(VDelegateListOB), objectOfType, method, false);
			}
			else if (parameterType2 == typeof(float) && parameterType3 == typeof(float))
			{
				this.m_ParamF1 = float.Parse(text3);
				this.m_ParamF2 = float.Parse(text4);
				this.m_MethodFF = (VDelegateFF)Delegate.CreateDelegate(typeof(VDelegateFF), objectOfType, method, false);
			}
			else if (parameterType2 == typeof(float) && parameterType3 == typeof(int))
			{
				this.m_ParamF1 = float.Parse(text3);
				this.m_ParamI2 = int.Parse(text4);
				this.m_MethodFI = (VDelegateFI)Delegate.CreateDelegate(typeof(VDelegateFI), objectOfType, method, false);
			}
			else if (parameterType2 == typeof(string) && parameterType3 == typeof(int))
			{
				this.m_ParamS1 = text3;
				this.m_ParamI2 = int.Parse(text4);
				this.m_MethodSI = (VDelegateSI)Delegate.CreateDelegate(typeof(VDelegateSI), objectOfType, method, false);
			}
			break;
		}
		case 3:
		{
			Type parameterType4 = parameters[0].ParameterType;
			Type parameterType5 = parameters[1].ParameterType;
			Type parameterType6 = parameters[2].ParameterType;
			string text5 = array[3];
			string text6 = array[4];
			string text7 = array[5];
			if (parameterType4.IsAssignableFrom(typeof(GameObject)) && parameterType5 == typeof(string) && parameterType6 == typeof(string))
			{
				this.m_IsGO1 = true;
				this.m_ParamO1Name = text5;
				this.m_ParamO1 = MainLevel.Instance.GetUniqueObject(text5);
				this.m_ParamS1 = text6;
				this.m_ParamS2 = text7;
				if (this.m_ParamO1 == null)
				{
					this.m_HasNullObject = true;
				}
				this.m_MethodOSS = (VDelegateOSS)Delegate.CreateDelegate(typeof(VDelegateOSS), objectOfType, method, false);
			}
			else if (parameterType4 == typeof(string) && parameterType5 == typeof(string) && parameterType6 == typeof(string))
			{
				this.m_ParamS1 = text5;
				this.m_ParamS2 = text6;
				this.m_ParamS3 = text7;
				this.m_MethodSSS = (VDelegateSSS)Delegate.CreateDelegate(typeof(VDelegateSSS), objectOfType, method, false);
			}
			else if (parameterType4 == typeof(string) && parameterType5 == typeof(bool) && parameterType6 == typeof(float))
			{
				this.m_ParamS1 = text5;
				this.m_ParamB2 = bool.Parse(text6);
				this.m_ParamF3 = float.Parse(text7);
				this.m_MethodSBF = (VDelegateSBF)Delegate.CreateDelegate(typeof(VDelegateSBF), objectOfType, method, false);
			}
			else if (parameterType4.IsAssignableFrom(typeof(GameObject)) && parameterType5 == typeof(string) && parameterType6 == typeof(bool))
			{
				this.m_IsGO1 = true;
				this.m_ParamO1Name = text5;
				this.m_ParamO1 = MainLevel.Instance.GetUniqueObject(text5);
				if (this.m_ParamO1 == null)
				{
					this.m_HasNullObject = true;
				}
				this.m_ParamS2 = text6;
				this.m_ParamB3 = bool.Parse(text7);
				this.m_MethodOSB = (VDelegateOSB)Delegate.CreateDelegate(typeof(VDelegateOSB), objectOfType, method, false);
			}
			else if (parameterType4 == typeof(string) && parameterType5 == typeof(int) && parameterType6 == typeof(bool))
			{
				this.m_ParamS1 = text5;
				this.m_ParamI2 = int.Parse(text6);
				this.m_ParamB3 = bool.Parse(text7);
				this.m_MethodSIB = (VDelegateSIB)Delegate.CreateDelegate(typeof(VDelegateSIB), objectOfType, method, false);
			}
			break;
		}
		case 4:
		{
			Type parameterType7 = parameters[0].ParameterType;
			Type parameterType8 = parameters[1].ParameterType;
			Type parameterType9 = parameters[2].ParameterType;
			Type parameterType10 = parameters[3].ParameterType;
			string paramS = array[3];
			string text8 = array[4];
			string text9 = array[5];
			string text10 = array[6];
			if (parameterType8.IsAssignableFrom(typeof(GameObject)) && parameterType7 == typeof(string) && parameterType9 == typeof(string) && parameterType10 == typeof(string))
			{
				this.m_IsGO1 = true;
				this.m_ParamO1Name = text8;
				this.m_ParamO1 = MainLevel.Instance.GetUniqueObject(text8);
				this.m_ParamS1 = paramS;
				this.m_ParamS2 = text9;
				this.m_ParamS3 = text10;
				if (this.m_ParamO1 == null)
				{
					this.m_HasNullObject = true;
				}
				this.m_MethodSOSS = (VDelegateSOSS)Delegate.CreateDelegate(typeof(VDelegateSOSS), objectOfType, method, false);
			}
			else if (parameterType7 == typeof(string) && parameterType8 == typeof(bool) && parameterType9 == typeof(float) && parameterType10 == typeof(int))
			{
				this.m_ParamS1 = paramS;
				this.m_ParamB2 = bool.Parse(text8);
				this.m_ParamF3 = float.Parse(text9);
				this.m_ParamI4 = int.Parse(text10);
				this.m_MethodSBFI = (VDelegateSBFI)Delegate.CreateDelegate(typeof(VDelegateSBFI), objectOfType, method, false);
			}
			break;
		}
		default:
			DebugUtils.Assert(DebugUtils.AssertType.Info);
			break;
		}
		DebugUtils.Assert(this.m_Method != null || this.m_MethodI != null || this.m_MethodB != null || this.m_MethodF != null || this.m_MethodS != null || this.m_MethodO != null || this.m_MethodII != null || this.m_MethodSB != null || this.m_MethodSS != null || this.m_MethodSF != null || this.m_MethodOF != null || this.m_MethodOS != null || this.m_MethodOO != null || this.m_MethodOB != null || this.m_MethodFF != null || this.m_MethodOSS != null || this.m_MethodOSB != null || this.m_MethodSOSS != null || this.m_MethodListOB != null || this.m_MethodSSS != null || this.m_MethodSBF != null || this.m_MethodSBFI != null || this.m_MethodFI != null || this.m_MethodSI != null || this.m_MethodSIB != null || this.m_MethodIF != null, this.m_EncodedContent, true, DebugUtils.AssertType.Info);
	}

	public override bool IsAction()
	{
		return true;
	}

	public override void Activate()
	{
		base.Activate();
		if (this.m_Method != null)
		{
			this.m_Method();
		}
		else if (this.m_MethodI != null)
		{
			this.m_MethodI(this.m_ParamI1);
		}
		else if (this.m_MethodB != null)
		{
			this.m_MethodB(this.m_ParamB1);
		}
		else if (this.m_MethodF != null)
		{
			this.m_MethodF(this.m_ParamF1);
		}
		else if (this.m_MethodS != null)
		{
			this.m_MethodS(this.m_ParamS1);
		}
		else if (this.m_MethodO != null)
		{
			this.m_MethodO(this.m_ParamO1);
		}
		else if (this.m_MethodII != null)
		{
			this.m_MethodII(this.m_ParamI1, this.m_ParamI2);
		}
		else if (this.m_MethodSB != null)
		{
			this.m_MethodSB(this.m_ParamS1, this.m_ParamB2);
		}
		else if (this.m_MethodSS != null)
		{
			this.m_MethodSS(this.m_ParamS1, this.m_ParamS2);
		}
		else if (this.m_MethodSF != null)
		{
			this.m_MethodSF(this.m_ParamS1, this.m_ParamF2);
		}
		else if (this.m_MethodOF != null)
		{
			this.m_MethodOF(this.m_ParamO1, this.m_ParamF2);
		}
		else if (this.m_MethodOS != null)
		{
			this.m_MethodOS(this.m_ParamO1, this.m_ParamS2);
		}
		else if (this.m_MethodOO != null)
		{
			this.m_MethodOO(this.m_ParamO1, this.m_ParamO2);
		}
		else if (this.m_MethodOB != null)
		{
			this.m_MethodOB(this.m_ParamO1, this.m_ParamB2);
		}
		else if (this.m_MethodFF != null)
		{
			this.m_MethodFF(this.m_ParamF1, this.m_ParamF2);
		}
		else if (this.m_MethodOSS != null)
		{
			this.m_MethodOSS(this.m_ParamO1, this.m_ParamS1, this.m_ParamS2);
		}
		else if (this.m_MethodOSB != null)
		{
			this.m_MethodOSB(this.m_ParamO1, this.m_ParamS2, this.m_ParamB3);
		}
		else if (this.m_MethodSOSS != null)
		{
			this.m_MethodSOSS(this.m_ParamS1, this.m_ParamO1, this.m_ParamS2, this.m_ParamS3);
		}
		else if (this.m_MethodSSS != null)
		{
			this.m_MethodSSS(this.m_ParamS1, this.m_ParamS2, this.m_ParamS3);
		}
		else if (this.m_MethodListOB != null)
		{
			this.m_MethodListOB(this.m_ParamListO1, this.m_ParamB2);
		}
		else if (this.m_MethodSBF != null)
		{
			this.m_MethodSBF(this.m_ParamS1, this.m_ParamB2, this.m_ParamF3);
		}
		else if (this.m_MethodSBFI != null)
		{
			this.m_MethodSBFI(this.m_ParamS1, this.m_ParamB2, this.m_ParamF3, this.m_ParamI4);
		}
		else if (this.m_MethodFI != null)
		{
			this.m_MethodFI(this.m_ParamF1, this.m_ParamI2);
		}
		else if (this.m_MethodSI != null)
		{
			this.m_MethodSI(this.m_ParamS1, this.m_ParamI2);
		}
		else if (this.m_MethodSIB != null)
		{
			this.m_MethodSIB(this.m_ParamS1, this.m_ParamI2, this.m_ParamB3);
		}
		else if (this.m_MethodIF != null)
		{
			this.m_MethodIF(this.m_ParamI1, this.m_ParamF2);
		}
		else
		{
			DebugUtils.Assert(DebugUtils.AssertType.Info);
		}
		base.Complete();
	}

	protected override bool ShouldComplete()
	{
		return true;
	}

	public override void Save(ScenarioNode node)
	{
		base.Save(node);
		if (this.m_ParamO1 && !Scenario.Get().m_ActionObjectsToSave.Contains(this.m_ParamO1))
		{
			Scenario.Get().m_ActionObjectsToSave.Add(this.m_ParamO1);
		}
		if (this.m_ParamO2 && !Scenario.Get().m_ActionObjectsToSave.Contains(this.m_ParamO2))
		{
			Scenario.Get().m_ActionObjectsToSave.Add(this.m_ParamO2);
		}
		string empty = string.Empty;
		if (this.m_ParamO1 && this.m_ComponentName != string.Empty && !Scenario.Get().m_ActionComponentToSave.TryGetValue(this.m_ParamO1, out empty))
		{
			Scenario.Get().m_ActionComponentToSave.Add(this.m_ParamO1, this.m_ComponentName);
		}
	}

	public override void Load(ScenarioNode node, int index)
	{
		base.Load(node, index);
		if (this.m_ParamO1 && !Scenario.Get().m_ActionObjectsToLoad.Contains(this.m_ParamO1))
		{
			Scenario.Get().m_ActionObjectsToLoad.Add(this.m_ParamO1);
		}
		if (this.m_ParamO2 && !Scenario.Get().m_ActionObjectsToLoad.Contains(this.m_ParamO2))
		{
			Scenario.Get().m_ActionObjectsToLoad.Add(this.m_ParamO2);
		}
		string empty = string.Empty;
		if (this.m_ParamO1 && this.m_ComponentName != string.Empty && !Scenario.Get().m_ActionComponentToLoad.TryGetValue(this.m_ParamO1, out empty))
		{
			Scenario.Get().m_ActionComponentToLoad.Add(this.m_ParamO1, this.m_ComponentName);
		}
		if (this.m_HasNullObject)
		{
			return;
		}
		if ((this.m_IsGO1 && this.m_ParamO1 == null) || (this.m_IsGO2 && this.m_ParamO2 == null))
		{
			this.m_HasNullObject = true;
		}
	}

	public override void PostLoad()
	{
		base.PostLoad();
		if (this.m_State != ScenarioElement.State.None && this.m_PerformOnLoad)
		{
			this.Activate();
		}
	}

	public override bool CheckObjects(GameObject go = null)
	{
		if (!this.m_HasNullObject)
		{
			return true;
		}
		base.CheckObjects(go);
		if (this.m_ParamListO1Names != null)
		{
			this.m_ParamListO1.Clear();
			for (int i = 0; i < this.m_ParamListO1Names.Length; i++)
			{
				GameObject uniqueObject = MainLevel.Instance.GetUniqueObject(this.m_ParamListO1Names[i]);
				if (uniqueObject == null)
				{
					this.m_HasNullObject = true;
				}
				else
				{
					this.m_ParamListO1.Add(uniqueObject);
				}
			}
		}
		return this.m_HasNullObject;
	}

	private VDelegate m_Method;

	private VDelegateI m_MethodI;

	private VDelegateB m_MethodB;

	private VDelegateF m_MethodF;

	private VDelegateS m_MethodS;

	private VDelegateO m_MethodO;

	private VDelegateII m_MethodII;

	private VDelegateSB m_MethodSB;

	private VDelegateSS m_MethodSS;

	private VDelegateSF m_MethodSF;

	private VDelegateOF m_MethodOF;

	private VDelegateOS m_MethodOS;

	private VDelegateOO m_MethodOO;

	private VDelegateOB m_MethodOB;

	private VDelegateFF m_MethodFF;

	private VDelegateOSS m_MethodOSS;

	private VDelegateOSB m_MethodOSB;

	private VDelegateSOSS m_MethodSOSS;

	private VDelegateSSS m_MethodSSS;

	private VDelegateListOB m_MethodListOB;

	private VDelegateSBF m_MethodSBF;

	private VDelegateSBFI m_MethodSBFI;

	private VDelegateFI m_MethodFI;

	private VDelegateSI m_MethodSI;

	private VDelegateSIB m_MethodSIB;

	private VDelegateIF m_MethodIF;

	private int m_ParamI1;

	private int m_ParamI2;

	private int m_ParamI4;

	private bool m_ParamB1;

	private bool m_ParamB2;

	private bool m_ParamB3;

	private float m_ParamF1;

	private float m_ParamF2;

	private float m_ParamF3;

	private string m_ParamS1 = string.Empty;

	private string m_ParamS2 = string.Empty;

	private string m_ParamS3 = string.Empty;

	private string m_ComponentName = string.Empty;

	private List<GameObject> m_ParamListO1;

	private string[] m_ParamListO1Names;

	private bool m_PerformOnLoad = true;
}
