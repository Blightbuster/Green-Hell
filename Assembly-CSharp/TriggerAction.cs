using System;

public class TriggerAction
{
	public static string GetText(TriggerAction.TYPE action)
	{
		return "HUD_Trigger_" + action.ToString();
	}

	public static string GetTextPerfect(TriggerAction.TYPE action)
	{
		switch (action)
		{
		case TriggerAction.TYPE.Take:
			return "HUD_Trigger_Taken";
		default:
			if (action == TriggerAction.TYPE.CloseContextMenu)
			{
				return "HUD_Trigger_CloseContextMenu";
			}
			if (action != TriggerAction.TYPE.InsertToStand)
			{
				return string.Empty;
			}
			break;
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
		}
		return "HUD_Trigger_Inserted";
	}

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
		InsertToStand
	}
}
