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

		public List<GearInfo> gear;
	}
}
