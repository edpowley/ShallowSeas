using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShallowNet
{
	public class FishType : IEquatable<FishType>
	{
		public FishType() { }

		public FishType(int sp, int st)
		{
			species = sp;
			stage = st;
		}

		public int species, stage;

		private static List<FishType> s_all = null;

		public static IEnumerable<FishType> All
		{
			get
			{
				if (s_all == null)
				{
					s_all = new List<FishType>();
					for (int sp = 0; sp < GameConstants.c_numFishSpecies; sp++)
						for (int st = 0; st < GameConstants.c_numFishStages; st++)
							s_all.Add(new FishType(sp, st));
				}

				return s_all;
			}
		}

		public bool Equals(FishType other)
		{
			return this.species == other.species && this.stage == other.stage;
		}

		public override bool Equals(object obj)
		{
			return obj is FishType && Equals((FishType)obj);
		}

		public override int GetHashCode()
		{
			return species * 10 + stage;
		}

		public override string ToString()
		{
			return string.Format("{0} {1}", GameConstants.c_stageNames[stage], GameConstants.c_speciesNames[species]);
		}
	}
}
