using System;

namespace UltimateWater
{
	public class Gerstner4
	{
		public Gerstner4(GerstnerWave wave0, GerstnerWave wave1, GerstnerWave wave2, GerstnerWave wave3)
		{
			this.Wave0 = wave0;
			this.Wave1 = wave1;
			this.Wave2 = wave2;
			this.Wave3 = wave3;
		}

		public GerstnerWave Wave0;

		public GerstnerWave Wave1;

		public GerstnerWave Wave2;

		public GerstnerWave Wave3;
	}
}
