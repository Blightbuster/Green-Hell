using System;

namespace Enums
{
	public enum DamageType
	{
		None,
		Cut,
		Thrust,
		Melee = 4,
		VenomPoison = 8,
		Claws = 16,
		Insects = 32,
		Fall = 64,
		Critical = 128,
		SnakePoison = 256
	}
}
