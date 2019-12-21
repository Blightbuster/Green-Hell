using System;
using System.Collections.Generic;

public class PlayerParams
{
	public float m_FistFightNormalDamage { get; protected set; }

	public float m_FistFightHardDamage { get; protected set; }

	public PlayerParams()
	{
		this.ParseScript();
	}

	public void ParseScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Player/Player_params.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Param")
			{
				switch (key.GetVariable(1).GetVariableType())
				{
				case CJVariable.TYPE.String:
					this.m_Params.Add(key.GetVariable(0).SValue, new PlayerParams.PlayerParamTyped<string>(key.GetVariable(1).SValue));
					break;
				case CJVariable.TYPE.Int:
					this.m_Params.Add(key.GetVariable(0).SValue, new PlayerParams.PlayerParamTyped<int>(key.GetVariable(1).IValue));
					break;
				case CJVariable.TYPE.Float:
					this.m_Params.Add(key.GetVariable(0).SValue, new PlayerParams.PlayerParamTyped<float>(key.GetVariable(1).FValue));
					break;
				case CJVariable.TYPE.Bool:
					this.m_Params.Add(key.GetVariable(0).SValue, new PlayerParams.PlayerParamTyped<bool>(key.GetVariable(1).BValue));
					break;
				}
			}
			else if (key.GetName() == "TriggerCheckRange")
			{
				this.m_TriggerCheckRange = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "TriggerUseRange")
			{
				this.m_TriggerUseRange = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "FistFightNormalDamage")
			{
				this.m_FistFightNormalDamage = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "FistFightHardDamage")
			{
				this.m_FistFightHardDamage = key.GetVariable(0).FValue;
			}
		}
	}

	public T GetParam<T>(string param_name)
	{
		PlayerParams.PlayerParam playerParam;
		if (!this.m_Params.TryGetValue(param_name, out playerParam))
		{
			DebugUtils.Assert("Missing player param " + param_name, true, DebugUtils.AssertType.Info);
			return default(T);
		}
		DebugUtils.Assert(typeof(T) == playerParam.m_Type, true);
		return ((PlayerParams.PlayerParamTyped<T>)playerParam).Get();
	}

	public float GetTriggerCheckRange()
	{
		return this.m_TriggerCheckRange;
	}

	public float GetTriggerUseRange()
	{
		return this.m_TriggerUseRange;
	}

	private float m_TriggerCheckRange = 2f;

	private float m_TriggerUseRange = 2f;

	private Dictionary<string, PlayerParams.PlayerParam> m_Params = new Dictionary<string, PlayerParams.PlayerParam>();

	private class PlayerParam
	{
		public Type m_Type;
	}

	private class PlayerParamTyped<T> : PlayerParams.PlayerParam
	{
		public PlayerParamTyped(T val)
		{
			this.m_Value = val;
			this.m_Type = typeof(T);
		}

		public T Get()
		{
			return this.m_Value;
		}

		private T m_Value;
	}
}
