using System;
using System.Collections.Generic;

public class TriggerAction
{
	public static string GetText(TriggerAction.TYPE action)
	{
		string text;
		if (!TriggerAction.s_CachedText.TryGetValue((int)action, out text))
		{
			text = "HUD_Trigger_" + action.ToString();
			TriggerAction.s_CachedText[(int)action] = text;
		}
		return text;
	}

	public static string GetTextPerfect(TriggerAction.TYPE action)
	{
		switch (action)
		{
		case TriggerAction.TYPE.Take:
			return "HUD_Trigger_Taken";
		case TriggerAction.TYPE.Expand:
		case TriggerAction.TYPE.CantSleep:
		case TriggerAction.TYPE.Climb:
		case TriggerAction.TYPE.Deconstruct:
		case TriggerAction.TYPE.Exit:
		case TriggerAction.TYPE.Look:
		case TriggerAction.TYPE.Open:
		case TriggerAction.TYPE.TakeHold:
		case TriggerAction.TYPE.Close:
			goto IL_EB;
		case TriggerAction.TYPE.Harvest:
			return "HUD_Trigger_Harvested";
		case TriggerAction.TYPE.Eat:
			return "HUD_Trigger_Eaten";
		case TriggerAction.TYPE.Drink:
		case TriggerAction.TYPE.DrinkHold:
			return "HUD_Trigger_Drank";
		case TriggerAction.TYPE.Use:
			return "HUD_Trigger_Used";
		case TriggerAction.TYPE.Sleep:
			return "HUD_Trigger_Slept";
		case TriggerAction.TYPE.Insert:
			break;
		case TriggerAction.TYPE.PickUp:
			return "HUD_Trigger_PickedUp";
		case TriggerAction.TYPE.Arm:
			return "HUD_Trigger_Armed";
		case TriggerAction.TYPE.Pour:
			return "HUD_Trigger_Pour";
		case TriggerAction.TYPE.Read:
			return "HUD_Trigger_ReadPast";
		default:
			switch (action)
			{
			case TriggerAction.TYPE.CloseContextMenu:
				return "HUD_Trigger_CloseContextMenu";
			case TriggerAction.TYPE.InsertToStand:
				break;
			case TriggerAction.TYPE.TakeHoldLong:
				goto IL_EB;
			case TriggerAction.TYPE.TurnOn:
				return "HUD_Trigger_TurnedOn";
			case TriggerAction.TYPE.TurnOff:
				return "HUD_Trigger_TurnedOff";
			case TriggerAction.TYPE.Touch:
				return "HUD_Trigger_Touched";
			case TriggerAction.TYPE.Talk:
				return "HUD_Trigger_Talked";
			case TriggerAction.TYPE.Play:
				return "HUD_Trigger_Played";
			default:
				goto IL_EB;
			}
			break;
		}
		return "HUD_Trigger_Inserted";
		IL_EB:
		return string.Empty;
	}

	private static Dictionary<int, string> s_CachedText = new Dictionary<int, string>();

	public enum TYPE
	{
		None = -1,
		Take,
		Expand,
		Harvest,
		Eat,
		Drink,
		Use,
		Sleep,
		CantSleep,
		Insert,
		Climb,
		PickUp,
		Arm,
		Deconstruct,
		Exit,
		Pour,
		Read,
		Look,
		Open,
		TakeHold,
		Close,
		DrinkHold,
		RemoveFromSnareTrap,
		Ignite,
		Fill,
		InventoryExpand,
		UseHold,
		ClimbHold,
		SwapHold,
		SaveGame,
		Remove,
		CloseContextMenu,
		InsertToStand,
		TakeHoldLong,
		TurnOn,
		TurnOff,
		Touch,
		Talk,
		Play,
		Plow,
		Pick,
		_Count
	}
}
