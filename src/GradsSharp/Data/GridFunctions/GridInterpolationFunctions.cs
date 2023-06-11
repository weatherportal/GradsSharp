using GradsSharp.Drawing.Grads;
using GradsSharp.Models;
using GradsSharp.Models.Internal;

namespace GradsSharp.Data.GridFunctions;

public static class GridInterpolationFunctions
{
    /// <summary>
    /// function to interpolate within a 3-D grid to a specified
    /// pressure level.  Can also be used on non-pressure level data, such
    /// as sigma or eta-coordinate output where pressure is a function
    /// of time and grid level.
    /// </summary>
    /// <param name="field">Grid to interpolate</param>
    /// <param name="pgrid">
    /// name of 3-D grid holding pressure values at each gridpoint
    /// If you are using regular pressure-level data, this should be
    /// set to the builtin GrADS variable 'lev'
    /// </param>
    /// <param name="plev">Pressure level at which to interpolate</param>
    /// <returns></returns>
    public static IGradsGrid Interpolate(VariableDefinition fieldvar, VariableDefinition pgridvar, VariableDefinition plevvar, IGradsCommandInterface cmd, InterpolationMode mode)
    {
        // function pinterp(field,pgrid,plev)
        var gcmd = cmd as GradsCommandInterface;
        var pcm = gcmd.CommonData;
        var ctx = gcmd.DrawingContext;
        

        GradsGrid[] pgr = new GradsGrid[4];
        int[] size = new int[4];
        GradsFile[] pfi = new GradsFile[2];
        List<double[]> lvvals = new List<double[]>();
        Func<double[], double, double>[] lvconv = new Func<double[], double, double>[2];
        Func<double[], double, double>[] conv = new Func<double[], double, double>[2];
        double clev = 0;
        int[] lvt = new int[3];
        int method = (int)mode;
        if (method < 0) method = 0;
        int returnGrid, isGridCompatible;
        
        VariableDefinition[] fncs = new VariableDefinition[3] { fieldvar, pgridvar, plevvar };

        pgr[3] = cmd.GetVariable(plevvar, plevvar.File) as GradsGrid;
        size[3] = pgr[3].ISize * pgr[3].JSize;

        for (int i = 0; i < 2; i++)
        {
            pfi[i] = pcm.pfi1[fncs[i].File - 1];
            size[i+1] = pfi[i].dnum[0] * pfi[0].dnum[1];
            lvvals.Add(pfi[i].grvals[2]);
            lvconv[i] = pfi[i].gr2ab[2];
            conv[i] = pfi[i].ab2gr[2];
            clev = lvconv[i](lvvals[i], 1);

            pgr[i+1] = cmd.GetVariable(new VariableDefinition
            {
                HeightType = fncs[i].HeightType,
                VariableType = fncs[i].VariableType,
                HeightValue = clev,
                VariableName = fncs[i].VariableName
            }) as GradsGrid;

            size[i + 1] = pgr[i + 1].ISize * pgr[i + 1].JSize;

            if (pfi[i].type == 4)
            {
                lvt[i + 1] = pfi[i].dnum[2];
            }
            else
            {
                lvt[i + 1] = pgr[i + 1].pvar.levels;
            }
            
            if(lvt[i+1]<=0) lvt[i+1]=pfi[i].dnum[2];
            
            if (lvt[i+1]<2)
            {
                throw new Exception("Too few levels");
            }
            
        }

        size[0] = size[1];


returnGrid=1;
	for (int i=2; i<=3; i++) {
		if (size[i]>size[0]) {
			size[0]=size[i];
			returnGrid=i;
		}
	}
	pgr[0]=pgr[returnGrid].CloneGrid() as GradsGrid;
	// 1.2 chk whether grid is compatible	
	isGridCompatible=1;
	for (int i=1; i<=3; i++) {
		if (size[0]!= size[i] && 1!= size[i]) {
			isGridCompatible=0;
			// sprintf (pout,"grid[%i]'s size = %i\n",i,size[i]);gaprnt (0, pout);
			// sprintf (pout,"max grid[%i]'s size = %i\n",returnGrid,size[0]);gaprnt (0, pout);
		}
	}

	if (isGridCompatible == 0)
	{
		throw new Exception("Incompatible grids");
	}

	// 1.3 choosing the minimum level between field and pgrid	
	lvt[0]=lvt[1];
	for (int i=1; i<=2; i++) {
		if (lvt[i]<lvt[0]) {
			lvt[0]=lvt[i];
		}
	}
//	2.0 Allocate memory and data	
	double[] field= new double[size[0]*lvt[0]]; 
	for (int i=0; i<size[0]*lvt[0]; i++) field[i] = pgr[0].Undef;
	
	double[] pgrid= new double[size[0]*lvt[0]];
	for (int i=0; i<size[0]*lvt[0]; i++) pgrid[i] = pgr[0].Undef;
	
	double[] plev= new double[size[0]]; for (int i=0; i<size[0]; i++) plev[i] = pgr[0].Undef;
	double[] xa= new double[lvt[0]]; for (int i=0; i<lvt[0]; i++) xa[i] =pgr[0].Undef;
	double[] ya=new double[lvt[0]]; for (int i=0; i<lvt[0]; i++) ya[i] =pgr[0].Undef;
	double[] y2=new double[lvt[0]]; for (int i=0; i<lvt[0]; i++) y2[i] =pgr[0].Undef;
	
	
	// 2.1 Set surface (pleb) data	
	for (int j=0; j<size[0]; j++) {
		if (size[3]!=1) {
			if (pgr[3].GridData[j]!=pgr[3].Undef) plev[j]=pgr[3].GridData[j];
		} else {
			if (pgr[3].GridData[0]!=pgr[3].Undef) plev[j]=pgr[3].GridData[0];
		}
	}
	// 2.2 release unneed grid except for return grid	
	//for (int j=1; j<=3; j++) if(j!=returnGrid) gagfre (pgr[j]);
//	2.3 Get level data (field, pgrid) from bottom to top */	
	for (int j=0; j<lvt[0]; j++) {
		for (int i=0; i<2; i++) {
			clev = lvconv[i](lvvals[i], (float)(j+1));
			pst->dmin[2] = clev;
			pst->dmax[2] = clev;
			rc = gaexpr(pfc->argpnt[i],pst);
			if (rc) {
				sprintf (pout,"Error from %s:  read %s error. \n",interpnam[sel],pfc->argpnt[i]);gaprnt (0,pout);
				sprintf (pout,"1) pgr[1]=%d;pgr[2]=%d. \n",pgr[1],pgr[2]);gaprnt (0,pout);
				for (k=0; k<i; k++) gagfre(pgr[k]); 
				goto erret;
			}
			if(0==i) pgr[1]= pst->result.pgr;		
			else if(1==i) pgr[2]= pst->result.pgr;		
		}
		ptr[1] = pgr[1].GridData;
		ptr[2] = pgr[2].GridData;
		for (int i=0; i<size[0]; i++) {
#ifdef lDiag
//	sprintf (pout,"2.3.2: j=%i, i=%i\n",j,i);gaprnt (0,pout);
#endif 	
			if(*ptr[1]!=pgr[1].Undef) field[j*size[0]+i]= *ptr[1];		
			if(*ptr[2]!=pgr[2].Undef) pgrid[j*size[0]+i]= *ptr[2];
			if (size[1]!=1) ptr[1]++;
			if (size[2]!=1) ptr[2]++;
		}
		gagfre (pgr[1]);
		gagfre (pgr[2]);
#ifdef lDiag
	sprintf (pout,"3) pgr[1]=%d;pgr[2]=%d. \n",pgr[1],pgr[2]);gaprnt (0,pout);
	sprintf (pout,"2.3.3\n");gaprnt (0,pout);
#endif 	
	}              	
//	3.0 Start to work	
#ifdef lDiag
	sprintf (pout,"3.0: Start to work\n");gaprnt (0,pout);
#endif
	ptr[0] = pgr[0].GridData;
	for (int i=0; i<size[0]; i++) {
#ifdef lDiag
	sprintf (pout,"3.0.1: i=%i\n",i);gaprnt (0,pout);
#endif
		if(plev[i]!=pgr[0].Undef) {
			// 3.1 preparing the working vector
			n=0;
			for (j=0; j<lvt[0]; j++) {
#ifdef lDiag
//	sprintf (pout,"3.1.1: i=%i, j=%i\n",i,j);gaprnt (0,pout);
#endif
				if((field[j*size[0]+i]!=pgr[0].Undef)&&(pgrid[j*size[0]+i]!=pgr[0].Undef)) {
					if (sel==0) {
					// log scale interpolation
						xa[n]=log(pgrid[j*size[0]+i]);
						x=log(plev[i]);
					} else if (sel==1) {
					// linear scale interpolation		
						xa[n]=pgrid[j*size[0]+i];
						x=plev[i];
					}
					ya[n]=field[j*size[0]+i];
					n++;
				}
			}
			// 3.2 call interpolation
			if (n>0) {
				if (n>2 && 1==method) {
				// 3.2.1 spine interpolation
#ifdef lDiag
	sprintf (pout,"3.2.1.1 i=%i,n=%i,vinterp=%f",i,n,*ptr[0]);gaprnt (0,pout);
	gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
	gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
#endif
					rc=sort2b(n,xa,ya);
#ifdef lDiag
	sprintf (pout,"3.2.1.2 i=%i,n=%i,vinterp=%f",i,n,*ptr[0]);gaprnt (0,pout);
	gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
	gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
#endif
					if (!rc) {
						spline(xa,ya,n,9.99e33,9.99e33,y2);	// natural spline
#ifdef lDiag
	sprintf (pout,"3.2.1.3 i=%i,n=%i,vinterp=%f",i,n,*ptr[0]);gaprnt (0,pout);
	gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
	gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
	gaprnt(0,"y2=");for (j=0;j<n;j++) {sprintf (pout,"%f ",y2[j]);gaprnt (0,pout);};gaprnt(0,"\n");
#endif
						rc=splintb(xa,ya,y2,n,x,ptr[0]);
						if (rc) {
							sprintf (pout,"3.2.1.4 i=%i,n=%i,vinterp=%f",i,n,*ptr[0]);gaprnt (0,pout);
							gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
							gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
							gaprnt(0,"y2=");for (j=0;j<n;j++) {sprintf (pout,"%f ",y2[j]);gaprnt (0,pout);};gaprnt(0,"\n");
						}
					}
					if (rc) *ptr[0]=pgr[0].Undef;
				}
				else if (2==method) {
				// 3.2.2 polynominal interpolation
					rc=polintb(xa,ya,n,x,ptr[0],&dy);
					if (rc) { sprintf(pout,"Error from %s. \n",interpnam[sel]);gaprnt (1,pout);}
				}
				else {
				// 3.2.3 piecewise linear interpolation
#ifdef lDiag
	sprintf (pout,"3.2.3.1 i=%i,n=%i,vinterp=%f",i,n,*ptr[0]);gaprnt (0,pout);
	gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
	gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
#endif
					rc=sort2b(n,xa,ya);
#ifdef lDiag
	sprintf (pout,"3.2.3.2 i=%i,n=%i,vinterp=%f",i,n,*ptr[0]);gaprnt (0,pout);
	gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
	gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
#endif
					if (rc) *ptr[0]=pgr[0].Undef;
					else {
						locate(xa,n,x,&jj);
						if(-1==jj) {
							// left of leftmost point	
							if(fabs(xa[1]-xa[0])>0) *ptr[0]=ya[0]+(ya[1]-ya[0])/(xa[1]-xa[0])*(x-xa[0]);
							else *ptr[0]=ya[0];
						}
						else if (n-1==jj) {
							// right of the rightmost point
							if(fabs(xa[n-2]-xa[n-1])>0)*ptr[0]=ya[n-1]+(ya[n-2]-ya[n-1])/(xa[n-2]-xa[n-1])*(x-xa[n-1]);
							else *ptr[0]=ya[n-1];
						}
						else {
							// mid-point
							if(fabs(xa[jj+1]-xa[jj])>0) *ptr[0]=ya[jj]+(ya[jj+1]-ya[jj])/(xa[jj+1]-xa[jj])*(x-xa[jj]);
							else *ptr[0]=(ya[jj+1]+ya[jj])/2.;
						}
					}
				}
			} else {
				*ptr[0]=pgr[0].Undef;
			}
#ifdef lDiag
	sprintf (pout,"3.2 i=%i,n=%i,vinterp=%f",i,n,*ptr[0]);gaprnt (0,pout);
	gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
	gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
#endif
		} else {
			*ptr[0]=pgr[0].Undef;
		}
#ifdef lDiag
		if (*ptr[0]!=pgr[0].Undef&& abs(*ptr[0])>10000.) {
			sprintf (pout,"3.3 i=%i,n=%i,x=%f,y==%f",i,n,x,*ptr[0]);gaprnt (0,pout);
			gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
			gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
		}
#endif
		ptr[0]++;
	}
//	4.0 Finished	
#ifdef lDiag
	sprintf (pout,"4.0\n");gaprnt (0, pout);
#endif	
 	/* Release storage and return */
	if (NULL!=field) free(field);
	if (NULL!=pgrid) free(pgrid);
	if (NULL!=plev) free(plev);
	if (NULL!=xa) free(xa);
	if (NULL!=ya) free(ya);
	if (NULL!=y2) free(y2);
	pst->result.pgr = pgr[0];
	return (0);
	
erret:
/* Error return */	
	if (NULL!=field) free(field);
	if (NULL!=pgrid) free(pgrid);
	if (NULL!=plev) free(plev);
	if (NULL!=xa) free(xa);
	if (NULL!=ya) free(ya);
	if (NULL!=y2) free(y2);
	return (rc);	

    }
}