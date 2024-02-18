using GradsSharp.Drawing.Grads;
using GradsSharp.Enums;
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
    public static IGradsGrid Interpolate(VariableDefinition fieldvar, VariableDefinition pgridvar, IGradsGrid plevvar, IGradsCommandInterface cmd, InterpolationMode mode)
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
        double clev = 0, x = 0, dy=0;
        int[] lvt = new int[3];
        int method = (int)mode;
        if (method < 0) method = 0;
        int returnGrid, isGridCompatible;
        int cnt0, cnt1, cnt2, rc, jj=0;
        
        VariableDefinition[] fncs = new VariableDefinition[2] { fieldvar, pgridvar };

        pgr[3] = plevvar as GradsGrid;
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

			var grid = cmd.GetVariable(new VariableDefinition
			{
				HeightType = fncs[i].HeightType,
				HeightValue = clev,
				VariableName = fncs[i].VariableName
			}) as GradsGrid;
			
			if(0==i) pgr[1] = grid;		
			else if(1==i) pgr[2]= grid;		
		}

		cnt1 = 0;
		cnt2 = 0;
		for (int i=0; i<size[0]; i++) {
			if (pgr[1].GridData[cnt1]!=pgr[1].Undef) field[j*size[0]+i]= pgr[1].GridData[cnt1];
			if (pgr[2].GridData[cnt2]!=pgr[2].Undef) pgrid[j*size[0]+i]= pgr[2].GridData[cnt2];
			if (size[1]!=1) cnt1++;
			if (size[2]!=1) cnt2++;
		}
		
	}              	
//	3.0 Start to work	
	//ptr[0] = pgr[0].GridData;
	cnt0 = 0;
	for (int i=0; i<size[0]; i++) {
		if(plev[i]!=pgr[0].Undef) {
			// 3.1 preparing the working vector
			int n=0;
			for (int j=0; j<lvt[0]; j++) {
				if((field[j*size[0]+i]!=pgr[0].Undef)&&(pgrid[j*size[0]+i]!=pgr[0].Undef)) {
					// log scale interpolation
					xa[n]=Math.Log(pgrid[j*size[0]+i]);
					x=Math.Log(plev[i]);
					// } else if (sel==1) {
					// // linear scale interpolation		
					// 	xa[n]=pgrid[j*size[0]+i];
					// 	x=plev[i];
					// }
					ya[n]=field[j*size[0]+i];
					n++;
				}
			}
			// 3.2 call interpolation
			if (n>0) {
				if (n>2 && 1==method) {
				// 3.2.1 spine interpolation
// #ifdef lDiag
// 	sprintf (pout,"3.2.1.1 i=%i,n=%i,vinterp=%f",i,n,*ptr[0]);gaprnt (0,pout);
// 	gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
// 	gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
// #endif
					rc=sort2b(n,ref xa,ref ya);
// #ifdef lDiag
// 	sprintf (pout,"3.2.1.2 i=%i,n=%i,vinterp=%f",i,n,*ptr[0]);gaprnt (0,pout);
// 	gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
// 	gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
// #endif
					if (rc>0) {
						spline(xa,ya,n,9.99e33,9.99e33, ref y2);	// natural spline
// #ifdef lDiag
// 	sprintf (pout,"3.2.1.3 i=%i,n=%i,vinterp=%f",i,n,*ptr[0]);gaprnt (0,pout);
// 	gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
// 	gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
// 	gaprnt(0,"y2=");for (j=0;j<n;j++) {sprintf (pout,"%f ",y2[j]);gaprnt (0,pout);};gaprnt(0,"\n");
// #endif
						rc=splintb(xa,ya,y2,n,x,ref pgr[0].GridData[cnt0]);
						if (rc==1) {
							// sprintf (pout,"3.2.1.4 i=%i,n=%i,vinterp=%f",i,n,*ptr[0]);gaprnt (0,pout);
							// gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
							// gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
							// gaprnt(0,"y2=");for (j=0;j<n;j++) {sprintf (pout,"%f ",y2[j]);gaprnt (0,pout);};gaprnt(0,"\n");
							
						}
					}
					if (rc>0) pgr[0].GridData[cnt0]=pgr[0].Undef;
				}
				else if (2==method) {
				// 3.2.2 polynominal interpolation
					rc=polintb(xa,ya,n,x,ref pgr[0].GridData[cnt0],ref dy);
					if (rc>0) { //sprintf(pout,"Error from %s. \n",interpnam[sel]);gaprnt (1,pout);
					}
				}
				else {
				// 3.2.3 piecewise linear interpolation
// #ifdef lDiag
// 	sprintf (pout,"3.2.3.1 i=%i,n=%i,vinterp=%f",i,n,*ptr[0]);gaprnt (0,pout);
// 	gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
// 	gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
// #endif
					rc=sort2b(n,ref xa,ref ya);
// #ifdef lDiag
// 	sprintf (pout,"3.2.3.2 i=%i,n=%i,vinterp=%f",i,n,*ptr[0]);gaprnt (0,pout);
// 	gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
// 	gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
// #endif
					if (rc>0) pgr[0].GridData[cnt0]=pgr[0].Undef;
					else {
						locate(ref xa,n,x,ref jj);
						if(-1==jj) {
							// left of leftmost point	
							if(Math.Abs(xa[1]-xa[0])>0) pgr[0].GridData[cnt0]=ya[0]+(ya[1]-ya[0])/(xa[1]-xa[0])*(x-xa[0]);
							else pgr[0].GridData[cnt0]=ya[0];
						}
						else if (n-1==jj) {
							// right of the rightmost point
							if(Math.Abs(xa[n-2]-xa[n-1])>0) pgr[0].GridData[cnt0] =ya[n-1]+(ya[n-2]-ya[n-1])/(xa[n-2]-xa[n-1])*(x-xa[n-1]);
							else pgr[0].GridData[cnt0]=ya[n-1];
						}
						else {
							// mid-point
							if(Math.Abs(xa[jj+1]-xa[jj])>0) pgr[0].GridData[cnt0]=ya[jj]+(ya[jj+1]-ya[jj])/(xa[jj+1]-xa[jj])*(x-xa[jj]);
							else pgr[0].GridData[cnt0]=(ya[jj+1]+ya[jj])/2.0;
						}
					}
				}
			} else {
				pgr[0].GridData[cnt0]=pgr[0].Undef;
			}
// #ifdef lDiag
// 	sprintf (pout,"3.2 i=%i,n=%i,vinterp=%f",i,n,*ptr[0]);gaprnt (0,pout);
// 	gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
// 	gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
// #endif
		} else {
			pgr[0].GridData[cnt0]=pgr[0].Undef;
		}
// #ifdef lDiag
// 		if (*ptr[0]!=pgr[0].Undef&& abs(*ptr[0])>10000.) {
// 			sprintf (pout,"3.3 i=%i,n=%i,x=%f,y==%f",i,n,x,*ptr[0]);gaprnt (0,pout);
// 			gaprnt(0,"x=");for (j=0;j<n;j++) {sprintf (pout,"%f ",xa[j]);gaprnt (0,pout);};gaprnt(0,"\t");
// 			gaprnt(0,"y=");for (j=0;j<n;j++) {sprintf (pout,"%f ",ya[j]);gaprnt (0,pout);};gaprnt(0,"\n");
// 		}
// #endif
		cnt0++;
	}
//	4.0 Finished	
// #ifdef lDiag
// 	sprintf (pout,"4.0\n");gaprnt (0, pout);
// #endif	
 	/* Release storage and return */
	// if (NULL!=field) free(field);
	// if (NULL!=pgrid) free(pgrid);
	// if (NULL!=plev) free(plev);
	// if (NULL!=xa) free(xa);
	// if (NULL!=ya) free(ya);
	// if (NULL!=y2) free(y2);
	return pgr[0];

	//erret:
/* Error return */	
	// if (NULL!=field) free(field);
	// if (NULL!=pgrid) free(pgrid);
	// if (NULL!=plev) free(plev);
	// if (NULL!=xa) free(xa);
	// if (NULL!=ya) free(ya);
	// if (NULL!=y2) free(y2);
	//return (rc);	
	

    }

    private static void swap(ref double a, ref double b)
    {
	    double t=a;
	    a=b;
	    b=t;
    }
    
    private static int sort2b(int n, ref double[] arr, ref double[] brr)
    {
	    const int M=7;
	    int i,ir,j,k,jstack=-1,l=0,nstack=50;
	    double a,b,temp;
//	nstack=(int)max(50,(int)2.*log((double)n)/log(2.));
	    int[] istack= new int[nstack];
	    
	    ir=n-1;
	    for (;;) {
		    if (ir-l < M) {
			    for (j=l+1;j<=ir;j++) {
				    a=arr[j];
				    b=brr[j];
				    for (i=j-1;i>=l;i--) {
					    if (arr[i] <= a) break;
					    arr[i+1]=arr[i];
					    brr[i+1]=brr[i];
				    }
				    arr[i+1]=a;
				    brr[i+1]=b;
			    }
			    if (jstack < 0) break;
			    ir=istack[jstack--];
			    l=istack[jstack--];
		    } else {
			    k=(l+ir) >> 1;
			    swap(ref arr[k],ref arr[l+1]);
			    swap(ref brr[k],ref brr[l+1]);
			    if (arr[l] > arr[ir]) {
				    swap(ref arr[l],ref arr[ir]);
				    swap(ref brr[l],ref brr[ir]);
			    }
			    if (arr[l+1] > arr[ir]) {
				    swap(ref arr[l+1],ref arr[ir]);
				    swap(ref brr[l+1],ref brr[ir]);
			    }
			    if (arr[l] > arr[l+1]) {
				    swap(ref arr[l],ref arr[l+1]);
				    swap(ref brr[l],ref brr[l+1]);
			    }
			    i=l+1;
			    j=ir;
			    a=arr[l+1];
			    b=brr[l+1];
			    for (;;) {
				    do i++; while (arr[i] < a);
				    do j--; while (arr[j] > a);
				    if (j < i) break;
				    swap(ref arr[i],ref arr[j]);
				    swap(ref brr[i],ref brr[j]);
			    }
			    arr[l+1]=arr[j];
			    arr[j]=a;
			    brr[l+1]=brr[j];
			    brr[j]=b;
			    jstack += 2;
			    if (jstack >= nstack) {
				    // sprintf(pout,"ERROR: sort2b:nstack too small in sort2.\n");
				    // gaprnt (0,pout);
//				nrerror("nstack too small in sort2.");
				    return (1);
			    }
			    if (ir-i+1 >= j-l) {
				    istack[jstack]=ir;
				    istack[jstack-1]=i;
				    ir=j-1;
			    } else {
				    istack[jstack]=j-1;
				    istack[jstack-1]=l;
				    l=i;
			    }
		    }
	    }
	    //free_lvector(istack);
	    return (0);
    }
    private static void spline(double[] x, double[] y, int n, double yp1, double ypn, ref double[] y2)
    {
	    int i,k;
	    double p,qn,sig,un;

	    double[] u = new double[n-1];

	    if (yp1 > 0.99e30)
		    y2[0]=u[0]=0.0;
	    else {
		    y2[0] = -0.5;
		    u[0]=(3.0/(x[1]-x[0]))*((y[1]-y[0])/(x[1]-x[0])-yp1);
	    }
	    for (i=1;i<n-1;i++) {
		    sig=(x[i]-x[i-1])/(x[i+1]-x[i-1]);
		    p=sig*y2[i-1]+2.0;
		    y2[i]=(sig-1.0)/p;
		    u[i]=(y[i+1]-y[i])/(x[i+1]-x[i]) - (y[i]-y[i-1])/(x[i]-x[i-1]);
		    u[i]=(6.0*u[i]/(x[i+1]-x[i-1])-sig*u[i-1])/p;
	    }
	    if (ypn > 0.99e30)
		    qn=un=0.0;
	    else {
		    qn=0.5;
		    un=(3.0/(x[n-1]-x[n-2]))*(ypn-(y[n-1]-y[n-2])/(x[n-1]-x[n-2]));
	    }
	    y2[n-1]=(un-qn*u[n-2])/(qn*y2[n-2]+1.0);
	    for (k=n-2;k>=0;k--) y2[k]=y2[k]*y2[k+1]+u[k];
    }
    private static int splintb(double[] xa, double[] ya, double[] y2a, int n, double x, ref double y)
    {
	    int k,rc;
	    double h,b,a;

	    int klo=0;
	    int khi=n-1;

	    while (khi-klo > 1) {
		    k=(khi+klo) >> 1;
		    if (xa[k] > x) khi=k;
		    else klo=k;
	    }
	    h=xa[khi]-xa[klo];
	    if (h == 0.0) {
		    // sprintf(pout,"ERROR: splintb: xa's must be distinct!\n");
		    // gaprnt (0,pout);
		    return(1);
	    }
	    a=(xa[khi]-x)/h;
	    b=(x-xa[klo])/h;
	    y=a*ya[klo]+b*ya[khi]+((a*a*a-a)*y2a[klo]
	                            +(b*b*b-b)*y2a[khi])*(h*h)/6.0;
	    return(0);
    }

    private static void locate(ref double[] xx, int n, double x, ref int j)
    {
	    int ju,jm,jl;
	    bool ascnd;

	    jl=-1;
	    ju=n;
	    ascnd=(xx[n-1] >= xx[0]);
	    while (ju-jl > 1) {
		    jm=(ju+jl) >> 1;
		    if (x >= xx[jm] == ascnd)
			    jl=jm;
		    else
			    ju=jm;
	    }
	    if (x == xx[0]) j=0;
	    else if (x == xx[n-1]) j=n-2;
	    else j=jl;
    }
    private static int polintb(double[] xa, double[] ya, int n, double x, ref double y, ref double dy) {
//  Polynomial interpolaton and extrapolation 
//	Ben-Jei Tsuang
//	2005/02/02: modify from numerical recepies
//
	    int i,m,ns=0;
	    double den,dif,dift,ho,hp,w;
	    double[] c, d;
	    c=new double[n];
	    d=new double[n];
	    
	    dif=Math.Abs(x-xa[0]);
	    for (i=0;i<n;i++) {
		    if ((dift=Math.Abs(x-xa[i])) < dif) {
			    ns=i;
			    dif=dift;
		    }
		    c[i]=ya[i];
		    d[i]=ya[i];
	    }
	    y=ya[ns--];
	    for (m=1;m<n;m++) {
		    for (i=0;i<n-m;i++) {
			    ho=xa[i]-x;
			    hp=xa[i+m]-x;
			    w=c[i+1]-d[i];
			    if ((den=ho-hp) == 0.0) { return (1);}
			    den=w/den;
			    d[i]=hp*den;
			    c[i]=ho*den;
		    }
		    y += (dy=(2*(ns+1) < (n-m) ? c[ns+1] : d[ns--]));
	    }
	    
	    return (0);	
    }

}