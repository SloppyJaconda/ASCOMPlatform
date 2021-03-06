/*
   C version of JPL Ephemeris Manager

   Version 0.2
   Updated to handle DE405 and DE406 6/98
   Updated to handle split Julian dates 11/06
   Added Ephem_Close function 11/07
*/

/*
	Peter Simpson 27th February 2010
	EXPORT added to relevant routines to planet_ephemeris and state to enable them to be seen outside this DLL
*/

#ifndef __EPHMAN__
#define __EPHMAN__

#ifndef __ASCOM__   
	#include "ascom.h" //PWGS Added ascom.h include
#endif

#ifndef __MATH__
   #include <math.h>
#endif

#ifndef __STRING__
   #include <string.h>
#endif

#ifndef __STDLIB__
   #include <stdlib.h>
#endif

#ifndef __STDIO__
   #include <stdio.h>
#endif


/*
   Define constants.
*/

extern char ephem_name[51];

extern short DE_Number;
extern short km;

extern long ipt[3][12], lpt[3], nrl, np, nv;
extern long record_length;

extern double ss[3], jplau, ve[2], pc[18], vc[18], twot, em_ratio;
extern double *buffer;

extern FILE *EPHFILE;

/*
   Function prototypes.
*/

EXPORT short Ephem_Open (char *Ephem_Name,

                  double *JD_Begin, double *JD_End);

EXPORT short Ephem_Close (void);

EXPORT short Planet_Ephemeris (double tjd[2], short target, short center, 
  
                        double *position, double *velocity);

EXPORT short State (double *jed, short target,

             double *target_pos, double *target_vel);

void Interpolate (double *buf, double *t, long ncm, long na,

                  double *position, double *velocity);

void Split (double tt, double *fr);

#endif
