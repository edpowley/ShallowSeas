/********************************************************************************
* This is a collection of procedures using vogl graphics to provide a display	*
* of the optimization of parameters for a CAM.  It is inserted into a program	*
* 'cam.c' using #include.							*
*										*
* Last update 22.10.96.								*
********************************************************************************/


/***Global constants for vogl graphics***/

#define		color_code 1	/*0: white to black	1: black to red		*/
#define		window     110.0/*Size of window				*/
#define		txt_margin 0.3	/*Gives a margin around edge for text		*/
#define		sep_margin 0.1	/*Gives a margin between grid displays		*/
#define		col_margin 0.01	/*Gives a margin for drawing around the plot	*/
#define		nmax       25.0 /*Scaling of numbers to maximum colour number	*/



/********************************************************************************
* This procedure carries out colour mixing.  Colouring depends on color_code:	*
*	0:	white to black	(for postscript)				*
*	1:	black to red	(for vogl)					*
* Procedure supplied by Ulf.							*
********************************************************************************/

void fraction2rgb(fraction, rP, gP, bP)
double	fraction, *rP, *gP, *bP;
{
	int	huesector;
	double	hue, huetune, mix_up, mix_do, r, g, b;

	if(color_code==1)
	{
		hue=1.0-fraction;
		if(hue<0.0)	hue=0.0;
		if(hue>1.0)	hue=1.0;
		huesector=(int)floor(hue*5.0);
		huetune=hue*5.0-huesector;
		mix_up=huetune;
		mix_do=1.0-huetune;
		mix_up=pow(mix_up,1.0/2.5);
		mix_do=pow(mix_do,1.0/2.5);
		switch(huesector)
		{
			case 0 : r=1.0   ;g=mix_up;b=0.0   ;break; /* red    to yellow */
			case 1 : r=mix_do;g=1.0   ;b=0.0   ;break; /* yellow to green  */
			case 2 : r=0.0   ;g=1.0   ;b=mix_up;break; /* green  to cyan   */
			case 3 : r=0.0   ;g=mix_do;b=1.0   ;break; /* cyan   to blue   */
			case 4 : r=0.0   ;g=0.0   ;b=mix_do;break; /* blue   to black  */
			default: r=0.0   ;g=0.0   ;b=0.0   ;break;
		}
	}

	else
	{
		r=1.0-fraction;
		g=1.0-fraction;
		b=1.0-fraction;    
	}

	*rP=r;*gP=g;*bP=b;
}


/********************************************************************************
* This procedure generates a color scale from black to red for coding numbers.	*
* Procedure supplied by Ulf.							*
********************************************************************************/

void colors_init()
{
	int	index;
	double	r, g, b;

    /***Colours 0 to 7 are predefined***/

    /***colors 8 to 15 are less bright versions of predefined colours***/
	mapcolor( 8,  0,  0,  0);
	mapcolor( 9,100,  0,  0);
	mapcolor(10,  0,100,  0);
	mapcolor(11, 50, 50,  0);
	mapcolor(12,  0,  0,100);
	mapcolor(13, 50,  0, 50);
	mapcolor(14,  0, 50, 50);
	mapcolor(15,153,153,153);  

    /***colors 16 to 79 are mixed as below (64 colors)***/
	for(index=16; index<=79; index++)
	{	fraction2rgb((double)(index-16)/(double)(80-16),&r,&g,&b);
		mapcolor(index,(int)(r*255.0),(int)(g*255.0),(int)(b*255.0));
	}
}


/********************************************************************************
* This procedure initializes a window for vogl graphics				*
********************************************************************************/

void	initialize_vogl()
{
	double	width, height;

    /***Initialize a window***/
	width  = txt_margin + 6.0 * (1.0 + sep_margin);
	height = txt_margin + 4.0 * (1.0 + sep_margin) + txt_margin;
	prefposition(0, (int)(width*window), 0, (int)(height*window));
	vinit("X11");
	winopen("CAM");		/*Window must have a name before display appears*/

    /***Set background colour***/
	color(BLACK);
	clear();

    /***Initialize colour scheme***/
	colors_init();

    /***Set up a coordinate frame***/
	ortho2(0.0, width, 0.0, height);
}


/********************************************************************************
* This procedure displays the status of the simulation.				*
********************************************************************************/

void	display_status(t)
int	t;			/*Current iteration				*/
{
	double	xshift, yshift;
	double	x_lower, x_upper, y_lower, y_upper;
	double	x1, y1;
	char	labelling[50];

    /***Set the origin for output of status information***/
	xshift = txt_margin;
	yshift = txt_margin + 4.0*(1.0 + sep_margin) + txt_margin/2.0;

    /***Draw boundary of status information***/
/*	x_lower = xshift - col_margin;
	y_lower = yshift - col_margin;
	x_upper = xshift + col_margin + nspp/2.0;
	y_upper = yshift + col_margin + txt_margin;
	color(BLACK);
	polymode(PYM_LINE);
	rectf(x_lower, y_lower, x_upper, y_upper);
*/
    /***Insert the text***/
/*	color(3);
	hfont("times.rb");
	hcentertext(0);
	htextsize(txt_margin/3.0, txt_margin/2.2);
	htextang(0.0);
	sprintf(labelling, "Simulation");
	x1  = xshift;
	y1  = yshift;
	move2(x1, y1);
	hcharstr(labelling);

	sprintf(labelling, "Year:%4d", t);
	yshift = txt_margin + 4.0*(1.0 + sep_margin);
	x1  = xshift;
	y1  = yshift;
	move2(x1, y1);
	hcharstr(labelling);
*/
}


/********************************************************************************
* This procedure displays the simulated grids for each species.			*
********************************************************************************/

void	display_grid_sim()
{
	int	sp, stage, x, y, scale_color, choose;
	double	xshift, yshift, cell_color;
	double	x1, x2, y1, y2;
	char	labelling[50];

    /***Set the origin for labelling of row of grids***/
	xshift = txt_margin - sep_margin;
	yshift = txt_margin + 1.0 * (1.0 + sep_margin);

    /***Insert the labelling***/
/*	color(3);
	hfont("times.rb");
	hcentertext(0);
	htextsize(txt_margin/3.0, txt_margin/2.2);
	htextang(90.0);
	sprintf(labelling, "Simulation");
	x1  = xshift;
	y1  = yshift;
	move2(x1, y1);
	hcharstr(labelling);
*/

    /***Loop for each species***/
	for (   sp=0;    sp<nspp;      ++sp)
	for (stage=0; stage<nstage; ++stage)
	{
	    /***Set the origin for labelling current species***/
		xshift = txt_margin + (double)stage * (1.0 + sep_margin);
		yshift = sep_margin + (double)sp    * (1.0 + sep_margin);

	    /***Insert the labelling***/
/*		color(3);
		hfont("times.rb");
		htextsize(txt_margin/3.0, txt_margin/2.2);
		htextang(0.0);
		sprintf(labelling, "Species %d ", sp);
		x1 = xshift;
		y1 = yshift;
		move2(x1, y1);
		hcharstr(labelling);
*/
	    /***Set the origin for current species***/
		xshift = txt_margin + (double)stage * (2.0 + sep_margin);
		yshift = txt_margin + (double)sp    * (2.0 + sep_margin);

	    /***Draw the plot boundary***/
		color(7);
		polymode(PYM_LINE);
		rectf(xshift-col_margin, yshift-col_margin, xshift+2.0+col_margin, yshift+2.0+col_margin);

	    /***Colour cells by species abundances***/
		polymode(PYM_FILL);
		for (x=0; x<xmax; ++x)
		for (y=0; y<ymax; ++y)
		{
			scale_color = (int) (64.0 * species[sp].N[stage][x][y]/5.0);
			if (scale_color == 0)			   choose = 16;
			if (scale_color >  0 && scale_color < 64)  choose = 17 + scale_color;
			if (scale_color >= 64)			   choose = 79;
			color(choose);

			x1 = xshift + 2.0 * (double)(x)   / (double)xmax;
			y1 = yshift + 2.0 * (double)(y)   / (double)ymax;
			x2 = xshift + 2.0 * (double)(x+1) / (double)xmax;
			y2 = yshift + 2.0 * (double)(y+1) / (double)ymax;
			rectf(x1, y1, x2, y2);
		}
	}
}


/********************************************************************************
* This procedure displays the results of the stochastic simulation		*
********************************************************************************/

void	display_vogl(t)
double	t;
{
    /***Puts modifications to graphs into a backbuffer for synchronous updating***/
	backbuffer();

    /***Clear the vogl window***/
    /***(backbuffer only overwrites -- it doesn't start from blank screen)***/
	color(BLACK);
	clear();

    /***Display the following information in the vogl window***/
	display_grid_sim();

    /***Move backbuffer to become the front buffer***/
	swapbuffers();
}
