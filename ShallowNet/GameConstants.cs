using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShallowNet
{
	public static class GameConstants
	{
		public const int c_numFishSpecies = 2;
		public static readonly string[] c_speciesNames = { "herring", "cod" };

		public const int c_numFishStages = 3;
		public static readonly string[] c_stageNames = { "juvenile", "intermediate", "adult" };
	}
}
