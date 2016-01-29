#new makefile.

# alias for compiler
CC = cc

# alias for compiling commands
CCOMP = $(CC) -c -g

# alias for linking commands
CLINK = -g -o

# libraries to link to
LIBS =  -I /usr/local/include -lvogl -L/usr/X11R6/lib -lX11 -lm 

# name of object file(s)
OBJS =  cam.o


# the first argument is an object you wish to create, i.e. cam.
# Then follow the dependencies.  To make cam you need cam.o .
# Next follows information on how to create cam with cam.o.

cam: cam.o 
	  $(CC) $(CLINK) cam $(OBJS) $(LIBS)


# the creation of cam.o is dependent upon the existence of 
# cam.c .  $(CCOMP) cam.c creates cam.o from cam.c 

cam.o: cam.c
	    $(CCOMP) cam.c
