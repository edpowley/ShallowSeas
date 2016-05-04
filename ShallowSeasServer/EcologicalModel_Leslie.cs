using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/****************************************************************************************************************
* Some procedures to try out different assumptions about dynamics.                                              *
*                                                                                                               *
* Last update: 22.01.16.                                                                                        *
****************************************************************************************************************/

namespace ShallowSeasServer
{
	partial class EcologicalModel
	{
		/****************************************************************************************************************
		* Procedure to output transition matrices.  In general they will be different for every cell. So am just        *
		* giving output on a singe cell x,y.                                                                            *
		****************************************************************************************************************/
		/*		void output_leslie(t, x, y)
		int x, y;
				double t;
		{
				FILE*out;
				int sp, i, j;

				out = fopen("output.details.dat", "a");


				fprintf(out, "\nt:%6.2f: transition matrices and densities for cell:%2d,%2d\n", t, x, y);
				for (i=0; i<nstage; ++i)
				{       for (sp=0; sp<nspp; ++sp)
						{       for (j=0; j<nstage; ++j)

								fprintf(out, "%7.4f ", species[sp].leslie[i, j]);

								fprintf(out, "\t");

								fprintf(out, "%7.4f ", species[sp].N[i, x, y]);

								fprintf(out, "\t");
			}

						fprintf(out, "%7.4f ", community.N[i, x, y]);

						fprintf(out, "\n");
		}


				fflush(out);

				fclose(out);
		}*/


		/****************************************************************************************************************
		* leslie2() keeps the feedbacks as in leslie1(), but tries a rescaling so that the updating is done every       *
		* day.  Trying to sort out the different time scales for fishing and ecology here.                              *
		* Needs a new kind of density dependence, since there is no matching of matrices with age.                      *
		****************************************************************************************************************/
		void leslie2(double t, int x, int y)
		{
			int i, j, k, sp;
			double[,] N = new double[nspp, nstage];        /*weighted density for each species*/

			/***Sets up density-dependent mortality with life stages allowing for interactions across species***/
			for (i = 0; i < nspp; ++i)
				for (k = 1; k < nstage; ++k) N[i, k] = 0.0;
			for (i = 0; i < nspp; ++i)
				for (j = 0; j < nspp; ++j)
					for (k = 1; k < nstage; ++k) N[i, k] += community.alpha[i, j] * species[j].N[k, x, y];

			/***Define transition matrices using density-dependent mortality***/
			for (sp = 0; sp < nspp; ++sp)
				switch (sp)
				{
					case 0: /*species 0*/
						species[sp].leslie[0, 0] = 0.999 - (1.0 - Math.Exp(-0.01 * N[0, 1]));
						species[sp].leslie[0, 1] = 0.1;
						species[sp].leslie[0, 2] = 0.0;
						species[sp].leslie[1, 0] = 0.001;
						species[sp].leslie[1, 1] = 0.999 - (1.0 - Math.Exp(-0.01 * N[0, 2]));
						species[sp].leslie[1, 2] = 0.0;
						species[sp].leslie[2, 0] = 0.0;
						species[sp].leslie[2, 1] = 0.0;
						species[sp].leslie[2, 2] = 0.0;
						break;

					case 1:/*species 1*/
						species[sp].leslie[0, 0] = 0.999 - (1.0 - Math.Exp(-0.01 * N[1, 1]));
						species[sp].leslie[0, 1] = 0.0;
						species[sp].leslie[0, 2] = 0.1;
						species[sp].leslie[1, 0] = 0.001;
						species[sp].leslie[1, 1] = 0.999 - (1.0 - Math.Exp(-0.01 * N[1, 2]));
						species[sp].leslie[1, 2] = 0.0;
						species[sp].leslie[2, 0] = 0.0;
						species[sp].leslie[2, 1] = 0.001/*0.005*/;
						species[sp].leslie[2, 2] = 0.9988;
						break;
				}

		}


		/****************************************************************************************************************
		* leslie1() contains some simple feedbacks:                                                                     *
		* (1)   survival to the next stage goes down as the density in the next stage goes up (eaten by bigger fish)    *
		* (2)   survival within a  stage goes up as the density in the previous stage goes up (food from smaller fish)  *
		*       (actually have commented this out for now)                                                              *
		* Feeding assumed to be independent of species -- they just feed by the life stage; this couples the species    *
		* together.                                                                                                     *
		****************************************************************************************************************/
		void leslie1(double t, int x, int y)
		{
			int sp, i, j;

			/***Define transition matrices***/
			for (sp = 0; sp < nspp; ++sp)
				switch (sp)
				{
					case 0: /*species 0*/
						species[sp].leslie[0, 0] = 0.0;
						species[sp].leslie[0, 1] = 20.0;
						species[sp].leslie[0, 2] = 0.0;
						species[sp].leslie[1, 0] = 0.05 * Math.Exp(-0.05 * community.N[1, x, y]);
						species[sp].leslie[1, 1] = 0.5;
						species[sp].leslie[1, 2] = 0.0;
						species[sp].leslie[2, 0] = 0.0;
						species[sp].leslie[2, 1] = 0.0;
						species[sp].leslie[2, 2] = 0.0;
						break;

					case 1:/*species 1*/
						species[sp].leslie[0, 0] = 0.0;
						species[sp].leslie[0, 1] = 0.0;
						species[sp].leslie[0, 2] = 100.0;
						species[sp].leslie[1, 0] = 0.05 * Math.Exp(-0.05 * community.N[1, x, y]);
						species[sp].leslie[1, 1] = 0.0;
						species[sp].leslie[1, 2] = 0.0;
						species[sp].leslie[2, 0] = 0.0;
						species[sp].leslie[2, 1] = 0.2 * Math.Exp(-0.05 * community.N[2, x, y]);
						species[sp].leslie[2, 2] = 0.5;
						break;
				}

		}


		/****************************************************************************************************************
		* Simplest transition matrices -- just to get things up and running.  Will not work in the long run as there    *
		* are no feedbacks in the ecosystem                                                                             *
		****************************************************************************************************************/
		void leslie0(double t, int x, int y)
		{
			int sp, i, j;

			/***Define transition matrices***/
			for (sp = 0; sp < nspp; ++sp)
				switch (sp)
				{
					case 0: /*species 0 -- leading eigenvalues > 1*/
						species[sp].leslie[0, 0] = 0.0;
						species[sp].leslie[0, 1] = 10.0;
						species[sp].leslie[0, 2] = 0.0;
						species[sp].leslie[1, 0] = 0.1;
						species[sp].leslie[1, 1] = 0.1;
						species[sp].leslie[1, 2] = 0.0;
						species[sp].leslie[2, 0] = 0.0;
						species[sp].leslie[2, 1] = 0.0;
						species[sp].leslie[2, 2] = 0.0;
						break;

					case 1:/*species 1 -- by coincidence has leading eigenvalue = 1*/
						species[sp].leslie[0, 0] = 0.0;
						species[sp].leslie[0, 1] = 0.0;
						species[sp].leslie[0, 2] = 50.0;
						species[sp].leslie[1, 0] = 0.1;
						species[sp].leslie[1, 1] = 0.0;
						species[sp].leslie[1, 2] = 0.0;
						species[sp].leslie[2, 0] = 0.0;
						species[sp].leslie[2, 1] = 0.1;
						species[sp].leslie[2, 2] = 0.5;
						break;
				}
		}



	}
}
