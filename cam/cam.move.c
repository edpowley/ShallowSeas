/****************************************************************************************************************
* Some procedures to try out assumptions about movement.							*
*														*
* Last update: 25.01.16.											*
****************************************************************************************************************/


/****************************************************************************************************************
* Procedure to get coordinates of a neighbour cell xnbr,ynbr of target cell x,y.				*
* Assumes periodic boundary conditions.										*
****************************************************************************************************************/
void    neighbour_coords(x, y, nbr, xnbr, ynbr)
int	x, y;		/*coordinates of target cell		*/
int	nbr;		/*index for neighbour cell		*/
int	*xnbr, *ynbr;	/*coordinates of chosen neighbour cell	*/
{
	switch (nbr)
	{
		case(0):	*xnbr = x - 1;
				*ynbr = y;
				break;
		case(1):	*xnbr = x;
				*ynbr = y + 1;
				break;
		case(2):	*xnbr = x + 1;
				*ynbr = y;
				break;
		case(3):	*xnbr = x;
				*ynbr = y - 1;
				break;
	}

    /***Make boundary periodic***/
    /***Note grid cells numbered 0, ..., max-1***/
	if (*xnbr < 0)		*xnbr = xmax-1;
	if (*xnbr > xmax-1)	*xnbr = 0;
	if (*ynbr < 0)		*ynbr = ymax-1;
	if (*ynbr > ymax-1)	*ynbr = 0;
}


/****************************************************************************************************************
* Simple assumption of stochastic matrix for movement between a pair of cells					*
* In due course this is going to depend on spatial structure and will need to be recomputed at every step	*
*   move[0]: proportion moving out from target cell								*
*   move[1]: proportion moving in  from nbr    cell								*
****************************************************************************************************************/
void    move0(t, stage, x, y, xnbr, ynbr)
int	stage, x, y, xnbr, ynbr;
float	t;
{
	int	sp;

    /***Define movements -- independent of life stage and density for now***/
	for (sp=0; sp<nspp; ++sp)
	switch(sp)
	{	case 0:	/*species 0*/
			species[sp].move[0] = 0.001;
			species[sp].move[1] = 0.001;
			break;

		case 1:/*species 1*/
			species[sp].move[0] = 0.001;
			species[sp].move[1] = 0.001;
			break;
	}
}

