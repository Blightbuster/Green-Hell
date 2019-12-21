using System;
using Enums;

namespace CJTools
{
	public class EnumTools
	{
		public static bool Equal(InjuryPlace place, Limb limb)
		{
			return (place == InjuryPlace.LHand && limb == Limb.LArm) || (place == InjuryPlace.RHand && limb == Limb.RArm) || (place == InjuryPlace.LLeg && limb == Limb.LLeg) || (place == InjuryPlace.RLeg && limb == Limb.RLeg);
		}

		public static Limb ConvertInjuryPlaceToLimb(InjuryPlace place)
		{
			if (place == InjuryPlace.LHand)
			{
				return Limb.LArm;
			}
			if (place == InjuryPlace.RHand)
			{
				return Limb.RArm;
			}
			if (place == InjuryPlace.LLeg)
			{
				return Limb.LLeg;
			}
			if (place == InjuryPlace.RLeg)
			{
				return Limb.RLeg;
			}
			return Limb.None;
		}

		public static bool IsItemSpoiled(ItemID item_id)
		{
			if (item_id <= ItemID.Poison_Dart_Frog_Meat_Spoiled)
			{
				if (item_id <= ItemID.Lean_Meat_Spoiled)
				{
					if (item_id <= ItemID.Raffia_nut_Spoiled)
					{
						if (item_id <= ItemID.Coconut_Shell_Flesh_Spoiled)
						{
							if (item_id != ItemID.Egg_Spoiled && item_id != ItemID.Coconut_Shell_Flesh_Spoiled)
							{
								return false;
							}
						}
						else if (item_id != ItemID.Coconut_flesh_Spoiled && item_id != ItemID.Brazil_nut_Spoiled)
						{
							switch (item_id)
							{
							case ItemID.Cassava_bulb_Spoiled:
							case ItemID.Banana_Spoiled:
							case ItemID.Palm_heart_Spoiled:
							case ItemID.Raffia_nut_Spoiled:
								break;
							case ItemID.Banana:
							case ItemID.Palm_heart:
							case ItemID.Raffia_nut:
								return false;
							default:
								return false;
							}
						}
					}
					else if (item_id <= ItemID.copa_hongo_Spoiled)
					{
						if (item_id != ItemID.Malanga_bulb_Spoiled && item_id != ItemID.Cocona_fruit_Spoiled)
						{
							switch (item_id)
							{
							case ItemID.Phallus_indusiatus_Spoiled:
							case ItemID.Gerronema_viridilucens_Spoiled:
							case ItemID.Gerronema_retiarium_Spoiled:
							case ItemID.indigo_blue_leptonia_Spoiled:
							case ItemID.copa_hongo_Spoiled:
								break;
							case ItemID.Gerronema_viridilucens:
							case ItemID.Gerronema_retiarium:
							case ItemID.indigo_blue_leptonia:
							case ItemID.copa_hongo:
								return false;
							default:
								return false;
							}
						}
					}
					else if (item_id - ItemID.Meat_Cooked_Spoiled > 2 && item_id - ItemID.Fat_Meat_Cooked_Spoiled > 2 && item_id - ItemID.Lean_Meat_Cooked_Spoiled > 2)
					{
						return false;
					}
				}
				else if (item_id <= ItemID.Stingray_Meat_Spoiled)
				{
					if (item_id <= ItemID.Pirahnia_Meat_Spoiled)
					{
						if (item_id - ItemID.Fish_Meat_Cooked_Spoiled > 2 && item_id != ItemID.Prawn_Meat_Spoiled && item_id != ItemID.Pirahnia_Meat_Spoiled)
						{
							return false;
						}
					}
					else if (item_id != ItemID.Peacock_Bass_Meat_Spoiled && item_id != ItemID.Arowana_Meat_Spoiled && item_id != ItemID.Stingray_Meat_Spoiled)
					{
						return false;
					}
				}
				else if (item_id <= ItemID.Cane_Toad_Meat_Spoiled)
				{
					if (item_id != ItemID.Rattlesnake_Meat_Spoiled && item_id != ItemID.Macaw_Meat_Spoiled && item_id != ItemID.Cane_Toad_Meat_Spoiled)
					{
						return false;
					}
				}
				else if (item_id != ItemID.Mouse_Meat_Spoiled && item_id != ItemID.Toucan_Meat_Spoiled && item_id != ItemID.Poison_Dart_Frog_Meat_Spoiled)
				{
					return false;
				}
			}
			else if (item_id <= ItemID.hura_crepitans_Spoiled)
			{
				if (item_id <= ItemID.Red_Footed_Tortoise_Meat_Spoiled)
				{
					if (item_id <= ItemID.Capybara_Meat_Spoiled)
					{
						if (item_id != ItemID.Green_Iguana_Meat_Spoiled && item_id != ItemID.Peccary_Meat_Spoiled && item_id != ItemID.Capybara_Meat_Spoiled)
						{
							return false;
						}
					}
					else if (item_id != ItemID.Tapir_Meat_Spoiled && item_id != ItemID.Mud_Turtle_Meat_Spoiled && item_id != ItemID.Red_Footed_Tortoise_Meat_Spoiled)
					{
						return false;
					}
				}
				else if (item_id <= ItemID.Puma_Meat_Spoiled)
				{
					if (item_id != ItemID.Armadilo_Meat_Spoiled && item_id != ItemID.Human_Meat_Spoiled && item_id != ItemID.Puma_Meat_Spoiled)
					{
						return false;
					}
				}
				else if (item_id != ItemID.Jaguar_Meat_Spoiled && item_id != ItemID.Guanabana_Fruit_Spoiled && item_id != ItemID.hura_crepitans_Spoiled)
				{
					return false;
				}
			}
			else if (item_id <= ItemID.AngelFish_Spoiled)
			{
				if (item_id <= ItemID.GoliathBirdEater_Spoiled)
				{
					if (item_id != ItemID.Caiman_Lizard_Meat_Spoiled && item_id != ItemID.Armadillo_Three_Banded_Meat_Spoiled && item_id != ItemID.GoliathBirdEater_Spoiled)
					{
						return false;
					}
				}
				else if (item_id != ItemID.BrasilianWanderingSpider_Spoiled && item_id != ItemID.Scorpion_Spoiled && item_id != ItemID.AngelFish_Spoiled)
				{
					return false;
				}
			}
			else if (item_id <= ItemID.Snail_Spoiled)
			{
				if (item_id != ItemID.DiscusFish_Spoiled && item_id != ItemID.Crab_Spoiled && item_id != ItemID.Snail_Spoiled)
				{
					return false;
				}
			}
			else if (item_id != ItemID.Caiman_Meat_Spoiled && item_id != ItemID.Shrimp_Spoiled && item_id != ItemID.geoglossum_viride_Spoiled)
			{
				return false;
			}
			return true;
		}
	}
}
