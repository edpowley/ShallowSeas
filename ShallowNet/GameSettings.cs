using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShallowNet
{
	public class GameSettings
	{
		public class GearInfo
		{
			public string name;
			public float castTime;
			public float maxCatch;
			public Dictionary<string, float> catchMultiplier;

			public double getCatchMultiplier(FishType ft)
			{
				return catchMultiplier[ft.ToString()];
			}
		}

		public enum BuyCategory { Self, Group };

		public class BuyInfo
		{
			public string name;
			public BuyCategory category;
			public int price;
		}

		public class FishSpeciesInfo
		{
			public string name;
			public List<int> prices;
		}

		public float roundLength;
		public float modelUpdateFreq;
		public float maxFuel;
		public List<GearInfo> gear;
		public List<BuyInfo> buyItems;
		public List<FishSpeciesInfo> fishSpecies;
	}
}
