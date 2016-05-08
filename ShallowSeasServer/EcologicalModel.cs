using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/****************************************************************************************************************
* A dynamical spatial fish-comunity model.  Developed for implementation in the ShallowSeas game.               *
*                                                                                                               *
*                                                                                                               *
* PREAMBLE                                                                                                      *
*                                                                                                               *
* The code takes a set of species: nspp is the number of species;  nspp=2 for now.                              *
*                                                                                                               *
* Each species can have a number of life stages, to allow different life histories; nstage is maximum number    *
* of life stages.  Have set nstage=3 for now, and put in parameters to make:                                    *
* Species 0 a small fish that reaches maturity quickly: a low-value forage fish.                                *
* Species 1 a fish that grows to a larger size and takes longer to mature: a high-value fish when mature.       *
*                                                                                                               *
* Coexistence of the species becomes a potential issue because species interact. Have put in an interaction     *
* matrix for weighting  the effects of the intra- and inter-specific predation. This has the form (for          * 
* two species):                                                                                                 *
*   alpha  =    1   a                   a = 0: species are independent                                          *
*               a   1                   a = 1: species do not discriminate in their feeding.                    *
* You tune the strength of interaction between species by altering the value of 'a' from 0 to 1; 'a'=0.5 for    *
* now.                                                                                                          *
*                                                                                                               *
* Time t goes in discrete steps.  Have been working towards a time step of 1 day, trying to get ecological      *
* parameters acting slowly enough on this time scale (fishers' dynamics operate much faster than the ecology).  *
* The grid is updated synchronously, in two steps each day:                                                     *
*   1. birth, death and growth events within cells                                                              *
*   2. movements between cells                                                                                  *
*                                                                                                               *
* I've done a bit of checking and haven't found serious errors.  But it's a good idea to treat code I write     *
* with plenty of scepticism, until you have checked it yourself.                                                *
*                                                                                                               *
*                                                                                                               *
* BIRTHS DEATHS AND GROWTH                                                                                      *
*                                                                                                               *
* Dynamics of birth, death and growth are given through transition matrices (based on Leslie and Lefkovich      *
* matrices).  These dynamics operate within cells.  Each row and column in the matrix corresponds to a life     *
* stage, and non-zero elements give the transitions between and within stages.  The basic updating is           *
* N(t+1) = A * N(t), where A is the matrix and N is a vector containing the densities of indviduals in          *
* each life stage.  The transition matrices have to be fully specified.                                         *
*                                                                                                               *
* I put in place different procedures while building ideas about the transition matrices.  These are labelled   *
* leslie0(), leslie1(), leslie2().  The dynamics get more intricate and 'interesting' as you go through         *
* the sequence of models below, and I'm now using leslie2().                                                    *
*                                                                                                               *
* LESLIE0 has fixed values for matrix elements -- no feedbacks -- species either increase without limit,        *
* or they die out                                                                                               *
*    A0    =    0    10                 Species 0 has two life stages: juvenile, adult; egg production is 2;    *
*               0.1  0.1                survival from juvenile to adult is 0.1; survival as adult is 0.1.       *
*                                                                                                               *
*    A1    =    0    0    50            Species 1 has three life stages: juvenile, intermediate, adult.  Same   *
*               0.1  0    0             interpretation of the parameters.  Has a leading eigenvalue = 1 by      *
*               0    0.1  0.5           coincidence.                                                            *
*                                                                                                               *
* LESLIE1 has a density-dependent feedback to controls species                                          *
*    A0    =    0       20              p00(N) = 0.05*exp(-0.05*N[1]), where N[1] = N0[1]+N1[1]; i.e. p00 goes  *
*               p00(N)  0.1             down as the summed density of the species in life stage 1 goes up.      *
*                                                                                                               *
*    A1    =    0       0      100      p10(N) = 0.05*exp(-0.05*N[1])                                           *
*               p10(N)  0      0        p11(N) = 0.20*exp(-0.05*N[2])                                           *
*               0       p11(N) 0.5      p10 is like p10; p11 goes down as density of life stage 2 goes up       *
* These feedbacks are a minimal concession to reality, that bigger fish eat smaller fish so the proportion      *
* surviving to the next life stage is reduced the more fish there are in that stage.  Each species now comes    *
* to equilibrium when the other species is absent.  When both species are present species 1 eliminates 2;       *
* coexistence is unfinished business.                                                                           *
*                                                                                                               *
* LESLIE2 has density-dependent feedback, and tries to set parameters nominally as 1 iteration = 1 day.  There  *
* is now no correspondence between time and life stage, so there are non-zero terms all along the diagonal to   *
* deal with updating of densities within life stages.  This is rough and ready at the moment.                   *
*    A0    =    q00(N)  0.1             q00(N) = 0.999 - (1.0 - exp(-0.01*N0[1, x, y]))                         *
*               0.001   q10(N)          q01(N) = 0.999 - (1.0 - exp(-0.01*N0[1, x, y]))                         *
																												*
*    A1    =    q10(N)  0      0.1      q10(N) = 0.999 - (1.0 - exp(-0.01*N1[2, x, y]))                         *
*               0.001   q11(N) 0        q11(N) = 0.999 - (1.0 - exp(-0.01*N1[2, x, y]))                         *
*               0       0.05   0.9988                                                                           *
* Here N0[1, x, y] = alpha * N[1, [x, y], where N[j, x, y] is the species vector of densities at life stage j,  *
* and Ni[j, x, y] is the species vector of densities at stage j on stage i, after weighting by alpha, the       *
* interaction matrix.  Again, feedbacks here are a minimal concession to reality.  In this case, bigger fish    *
* eat smaller fish so the proportion surviving in the current stage is reduced the more fish there are in the   *
* next stage.                                                                                                   *
*                                                                                                               *
* To do: Another feedback in which fish grow faster if there are more smaller fish.  That's the counterpart     *
* to death from predation, and could go in on the subdiagonals.                                                 *
*                                                                                                               *
*                                                                                                               *
* MOVEMENTS                                                                                                     *
*                                                                                                               *
* Cells are linked by movement of fish.  This calls for an assumption about the boundary.  For simplicity, and  *
* to check things, I assumed periodic boundary conditions.  This will need to be changed for the game.          *
*                                                                                                               *
* For now, a fixed proportion 0.001 move between cells in each iteration.  Have written the code to allow more  *
* interesting behaviour in the future, such as small fish moving away from big fish (to reduce predation),      *
* and big fish to moving towards smaller fish (to get more food).                                               *
*                                                                                                               *
*                                                                                                               *
* FISHING                                                                                                       *
*                                                                                                               *
* It's not in code yet.  But will be easy enough to deal with.                                                  *
*                                                                                                               *
*                                                                                                               *
* FILE STRUCTURES                                                                                               *
*                                                                                                               *
* cam.c:        core C code                                                                                     *
* cam.leslie.c: procedures for birth-death-growth                                                               *
* cam.move.c:   procedures for movement                                                                         *
* cam.vogl.c:   a trip into the deep archaeology of graphics; I use this so I can see what's going on.          *
*                                                                                                               *
* If you want to use the code unaltered, vogl does need to be installed.  Simon Hickinbotham has kindly         *
* supplied a webpage at https://github.com/franticspider/voglc21                                                *
* But I'm assuming you will be suitably horrified by this, and will just take the core bits of ecology into     *
* something much better.                                                                                        *
*                                                                                                               *
*                                                                                                               *
* (Note for RL: comes from spatiotemporal/2003.discrete/stochastic)                                             *
*                                                                                                               *
* Last update: 26.01.16.                                                                                        *
****************************************************************************************************************/

namespace ShallowSeasServer
{
	partial class EcologicalModel
	{
		/********************************************************************************
		* Global constants                                                              *
		********************************************************************************/

		private const int nspp = ShallowNet.GameConstants.c_numFishSpecies;         /*number of species: indexed 0,1                */
		private const int nstage = ShallowNet.GameConstants.c_numFishStages;        /*maximum number of life stages: indexed 0,1,2  */
		private const double dt = 1.0;      /*time step                                     */
		private const int xmax = 32;        /*number of cells along x coordinate            */
		private const int ymax = 32;        /*number of cells along y coordinate            */
		private const int nbrmax = 4;       /*number of neighbour cells                     */


		/********************************************************************************
		* Switches for turning on and off parts of the program                          *
		********************************************************************************/
		private const int method_leslie = 2;       /* Method for birth-death matrices
													   0: transition matrices fixed
													   1: transition matrices with feedbacks
													   2: transition matrices with time - scaling */

		private const int method_move = 0;         /* Method for movement between cells
										               0: constant proportion move */

		/********************************************************************************
		* Structures                                                                    *
		********************************************************************************/
		class species_properties
		{
			internal double[,,] N = new double[nstage, xmax, ymax];          /*    species densities by life stage and grid cell     */
			internal double[,,] newN = new double[nstage, xmax, ymax];       /*new species densities by life stage and grid cell     */
			internal double[] sumN = new double[nstage];                   /*    species densities by life stage summed over grid  */
			internal double[,] leslie = new double[nstage, nstage];         /*matrix for dynamics -- Leslie-matrix-like             */
			internal double[] move = new double[2];                        /*vector for movement between 2 cells                   */
		};

		species_properties[] species = new species_properties[nspp];

		class community_properties
		{
			internal double[,] alpha = new double[nspp, nspp];              /*  interaction matrix of species                       */
			internal double[,,] N = new double[nstage, xmax, ymax];          /*  community densities by life stage and grid cell     */
		};

		community_properties community = new community_properties();


		public double getDensity(int spec, int stage, int x, int y)
		{
			return species[spec].N[stage, x, y];
		}

		/********************************************************************************
		* Variables for random number generator                                         *
		********************************************************************************/
		private Random rng = new Random();
		double drand48()
		{
			return rng.NextDouble();
		}

		/****************************************************************************************************************
		* This procedure sets the strength of interactions between species relative to those within species.            *
		* Unless the feeding on heterospecifics is less than on conspecifics, it may be hard to get coexistence.        *
		****************************************************************************************************************/
		void initialize_interactions()
		{
			community.alpha[0, 0] = 1.0f;
			community.alpha[0, 1] = 0.5f;
			community.alpha[1, 0] = 0.5f;
			community.alpha[1, 1] = 1.0f;
		}

		/****************************************************************************************************************
		* This procedure sets the spatial densities at the start.                                                       *
		****************************************************************************************************************/
		void initialize_spatial_densities()
		{
			int x, y, sp, i;
			int layout, n_start;
			double p;


			/***work across the grid***/
			for (x = 0; x < xmax; ++x)
				for (y = 0; y < ymax; ++y)
				{
					/***species densities by cell and life stage***/
					for (sp = 0; sp < nspp; ++sp)
						switch (sp)
						{
							case 0: /*species 0*/
								species[sp].N[0, x, y] = 50.0 * drand48();
								species[sp].N[1, x, y] = 10.0 * drand48();
								species[sp].N[2, x, y] = 0.0 * drand48();
								break;

							case 1:/*species 1*/
								species[sp].N[0, x, y] = 20.0 * drand48();
								species[sp].N[1, x, y] = 5.0 * drand48();
								species[sp].N[2, x, y] = 1.0 * drand48();
								break;
						}

					/***community densities by cell and life stage (summing over species)***/
					for (i = 0; i < nstage; ++i)
					{
						community.N[i, x, y] = 0.0;
						for (sp = 0; sp < nspp; ++sp) community.N[i, x, y] += species[sp].N[i, x, y];
					}
				}
		}


		/****************************************************************************************************************
		* This procedure updates the densities in each cell through births and deaths                                   *
		****************************************************************************************************************/
		void newgrid_leslie(double t)
		{
			int sp, x, y, i, j;

			/***work across each cell of the grid***/
			for (x = 0; x < xmax; ++x)
				for (y = 0; y < ymax; ++y)
				{
					/***put new species densities to zero***/
					for (sp = 0; sp < nspp; ++sp)
						for (i = 0; i < nstage; ++i) species[sp].newN[i, x, y] = 0.0;

					/***construct transition matrices for cell x,y***/
					switch (method_leslie)
					{
						case 0: /*Leslie matrices: no density-dependent (dd) feedbacks within cells; just calculate for first cell*/
							if (x == 0 && y == 0)

								leslie0(t, x, y);
							break;

						case 1: /*Leslie matrices: dd feedbacks within cells; recalculated for each cell*/

							leslie1(t, x, y);
							break;

						case 2: /*Leslie matrices: dd feedbacks + time-scaling + species interactions; recalculated for each cell*/

							leslie2(t, x, y);
							break;
					}

					/***calculate new species densities: A*N***/
					for (sp = 0; sp < nspp; ++sp)
						for (i = 0; i < nstage; ++i)
							for (j = 0; j < nstage; ++j) species[sp].newN[i, x, y] += species[sp].leslie[i, j] * species[sp].N[j, x, y];

					/***update species densities***/
					for (sp = 0; sp < nspp; ++sp)
						for (i = 0; i < nstage; ++i) species[sp].N[i, x, y] = species[sp].newN[i, x, y];
				}

			/***sum of species densities to get community densities***/
			for (x = 0; x < xmax; ++x)
				for (y = 0; y < ymax; ++y)
					for (i = 0; i < nstage; ++i)
					{
						community.N[i, x, y] = 0.0;
						for (sp = 0; sp < nspp; ++sp) community.N[i, x, y] += species[sp].N[i, x, y];
					}

			/***sum of species densities over grid***/
			for (sp = 0; sp < nspp; ++sp)
				for (i = 0; i < nstage; ++i)
				{
					species[sp].sumN[i] = 0.0;
					for (x = 0; x < xmax; ++x)
						for (y = 0; y < ymax; ++y) species[sp].sumN[i] += species[sp].newN[i, x, y];
				}
		}


		/****************************************************************************************************************
		* This procedure updates the densities in each cell through movement between cells                              *
		****************************************************************************************************************/
		void newgrid_move(double t)
		{
			int sp, i, x, y, nbr, xnbr, ynbr;

			/***put new species densities to the current densites***/
			for (x = 0; x < xmax; ++x)
				for (y = 0; y < ymax; ++y)
					for (sp = 0; sp < nspp; ++sp)
						for (i = 0; i < nstage; ++i) species[sp].newN[i, x, y] = species[sp].N[i, x, y];

			for (x = 0; x < xmax; ++x)
				for (y = 0; y < ymax; ++y)
					for (i = 0; i < nstage; ++i)
						for (nbr = 0; nbr < nbrmax; ++nbr)
						{
							/***coordinates of neighbour cell***/
							neighbour_coords(x, y, nbr, out xnbr, out ynbr);

							/***matrices for movement between cell x,y and cell xnbr, ynbr***/
							switch (method_move)
							{
								case 0: /*movement matrices: simplest stochastic matrix*/

									move0(t, i, x, y, xnbr, ynbr);
									break;
							}

							/***loss to target cell from movement OUT to nbr cell***/
							for (sp = 0; sp < nspp; ++sp)
								species[sp].newN[i, x, y] -= species[sp].move[1] * species[sp].N[i, x, y];

							/***gain to target cell from movement IN from nbr cell***/
							for (sp = 0; sp < nspp; ++sp)
								species[sp].newN[i, x, y] += species[sp].move[1] * species[sp].N[i, xnbr, ynbr];
						}

			/***update grid***/
			for (x = 0; x < xmax; ++x)
				for (y = 0; y < ymax; ++y)
					for (i = 0; i < nstage; ++i)
					{
						/***update species densities***/
						for (sp = 0; sp < nspp; ++sp)
							species[sp].N[i, x, y] = species[sp].newN[i, x, y];

						/***sum of species densities to get community densities***/
						community.N[i, x, y] = 0.0;
						for (sp = 0; sp < nspp; ++sp) community.N[i, x, y] += species[sp].N[i, x, y];
					}


			/***sum of species densities to get community densities***/
			for (x = 0; x < xmax; ++x)
				for (y = 0; y < ymax; ++y)
					for (i = 0; i < nstage; ++i)
					{
						community.N[i, x, y] = 0.0;
						for (sp = 0; sp < nspp; ++sp) community.N[i, x, y] += species[sp].N[i, x, y];
					}

			/***sum of species densities over grid***/
			for (sp = 0; sp < nspp; ++sp)
				for (i = 0; i < nstage; ++i)
				{
					species[sp].sumN[i] = 0.0;
					for (x = 0; x < xmax; ++x)
						for (y = 0; y < ymax; ++y) species[sp].sumN[i] += species[sp].newN[i, x, y];
				}
		}


		/****************************************************************************************************************
		* This procedure carries out the iteration                                                                      *
		* Updating is synchronous across the grid, and is done in two steps:                                            *
		*   1. calculate new densities arising from births and deaths                                                   *
		*   2. calculate new densities arising from movement beween cells                                               *
		****************************************************************************************************************/
		double t = 0;

		public void iterate()
		{
			/***Output current status of system***/
			//output(t);
			//if (method_vogl == 1) display_vogl(t);

			/***Update state of system through births and deaths***/
			newgrid_leslie(t);

			/***Update state of system through movement***/
			newgrid_move(t);

			/***Update time***/
			t += dt;

			/*              system("sleep 0.3");
			*/
		}

		public EcologicalModel()
		{
			for (int i = 0; i < nspp; i++)
				species[i] = new species_properties();

			initialize_interactions();
			initialize_spatial_densities();
		}

		/****************************************************************************************************************
		* Main body of program.                                                                                         *
		****************************************************************************************************************/
		//int main()
		//{

		//	/***Initialize and empty output file***/
		//	fclose(fopen("output.details.dat", "w"));
		//	fclose(fopen("output.grid.dat", "w"));

		//	/***Initialize random number generator***/
		//	printf("Seed for random number generator <= 4 digits:");
		//	scanf("%4ld", &seed);
		//	srand48(seed);

		//	/***Initialize interaction matrix between species***/
		//	initialize_interactions();

		//	/***Enter the initial densities across space***/
		//	initialize_spatial_densities();

		//	/***Initialize a vogl graphics window***/
		//	if (method_vogl == 1)
		//	{
		//		initialize_vogl();
		//		display_vogl(0.0);
		//	}

		//	/***Run the simulation***/
		//	iterate();

		//	system("sleep 5");
		//}

	}
}
