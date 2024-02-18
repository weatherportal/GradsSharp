using GradsSharp.Models.Internal;
using Microsoft.Extensions.Logging;

namespace GradsSharp.Drawing.Grads;

internal class GaExpr
{
    private int pass = 0;
    private const int BLKNUM = 50000;
    private DrawingContext _drawingContext;

    public GaExpr(DrawingContext context)
    {
        _drawingContext = context;
    }

    /* Evaluate a GrADS expression.  The expression must have
   blanks removed.  The expression is evaluated with respect
   to the environment defined in the status block (pst).
   This routine is invoked recursively from functions in order
   to evaluate sub-expressions.                                  */

    public int gaexpr(string expression, gastat pst)
    {
        GradsGrid pgr;
        gastn stn;
        smem[] stack;
        int? ptr;
        double val;
        int cmdlen, i, j, curr;
        int size;
        int pos = 0;
        bool cont = true, state = true, err = false, rc;

        if (_drawingContext.CommonData.sig > 0) return (1);
        pass++;

        cmdlen = expression.Length;
        size = cmdlen + 10;
        stack = new smem[size];

        for (int x = 0; x < size; x++) stack[x] = new smem();

        state = true;
        curr = -1;
        pos = 0;

        cont = true;
        err = false;
        while (cont)
        {
            /* Loop while parsing exprssn  */

            if (state)
            {
                /* Expect operand or '('       */

                if (expression[pos] == '(')
                {
                    /* Handle left paren           */
                    curr++;
                    stack[curr].type = 2;
                    pos++;
                }
                else if (expression[pos] == '-')
                {
                    /* Handle unary '-' operator   */
                    curr++;
                    stack[curr].type = -1;
                    stack[curr].obj.pgr = gagrvl(-1.0);
                    curr++;
                    stack[curr].type = 1;
                    stack[curr].obj.op = 0;
                    pos++;
                }
                else if (expression[pos] >= '0' && expression[pos] <= '9')
                {
                    /* Handle numeric value   */
                    if ((ptr = GaUtil.getdbl(expression, pos, out val)) == null)
                    {
                        cont = false;
                        err = true;
                        i = 1 + pos;
                        _drawingContext.Logger?.LogInformation("Syntax Error:  Invalid numeric value");
                    }
                    else
                    {
                        curr++;
                        stack[curr].type = -1;
                        stack[curr].obj.pgr = gagrvl(val);
                        /*     stkdmp(stack,curr);  */
                        rc = eval(0, stack, ref curr);
                        if (rc)
                        {
                            err = true;
                            cont = false;
                        }

                        state = false;
                        pos = ptr ?? int.MaxValue;
                    }
                }
                else if ((expression[pos] >= 'a' && expression[pos] <= 'z') || (expression[pos] >= 'A' && expression[pos] <= 'Z'))
                {
                    /* Handle variable        */
                    if ((ptr = varprs(expression, pos, pst)) == null)
                    {
                        cont = false;
                        err = true;
                    }
                    else
                    {
                        curr++;
                        if (pst.type > 0)
                        {
                            stack[curr].type = -1;
                            stack[curr].obj.pgr = pst.result.pgr;
                        }
                        else
                        {
                            stack[curr].type = -2;
                            stack[curr].obj.stn = pst.result.stn;
                        }

                        /*      stkdmp(stack,curr);  */
                        rc = eval(0, stack, ref curr);
                        if (rc)
                        {
                            err = true;
                            cont = false;
                        }

                        state = false;
                        pos = ptr ?? int.MaxValue;
                    }
                }
                else
                {
                    _drawingContext.Logger?.LogInformation("Syntax Error:  Expected operand or '('");
                    cont = false;
                    err = true;
                }
            }
            else
            {
                /* Expect operator or ')'      */

                if (expression[pos] == ')')
                {
                    /* Handle right paren          */
                    curr++;
                    stack[curr].type = 3;
                    pos++;
                    rc = eval(0, stack, ref curr); /* Process stack         */
                    if (rc)
                    {
                        err = true;
                        cont = false;
                        pos--;
                    }
                }
                /* Handle operator             */

                else if ((expression[pos] == '*') || (expression[pos] == '/') || (expression[pos] == '+') ||
                         (expression[pos] == '-'))
                {
                    curr++;
                    stack[curr].type = 1;
                    if (expression[pos] == '*') stack[curr].obj.op = 0;
                    if (expression[pos] == '/') stack[curr].obj.op = 1;
                    if (expression[pos] == '+') stack[curr].obj.op = 2;
                    if (expression[pos] == '-')
                    {
                        stack[curr].obj.op = 2;
                        curr++;
                        stack[curr].type = -1;
                        stack[curr].obj.pgr = gagrvl(-1.0);
                        curr++;
                        stack[curr].type = 1;
                        stack[curr].obj.op = 0;
                    }

                    /*   stkdmp(stack,curr);  */
                    pos++;
                    state = true;
                }
                /* logical operator */

                else if ((expression[pos] == '=') || (expression[pos] == '!') || (expression[pos] == '<') ||
                         (expression[pos] == '>')
                         || (expression[pos] == '|') || (expression[pos] == '&'))
                {
                    if (expression[pos + 1] == '=')
                    {
                        curr++;
                        stack[curr].type = 1;
                        if ((expression[pos] == '=') && (expression[pos + 1] == '=')) stack[curr].obj.op = 20;
                        if ((expression[pos] == '<') && (expression[pos + 1] == '=')) stack[curr].obj.op = 23;
                        if ((expression[pos] == '>') && (expression[pos + 1] == '=')) stack[curr].obj.op = 24;
                        if ((expression[pos] == '!') && (expression[pos + 1] == '=')) stack[curr].obj.op = 25;
                        pos += 2;
                        state = true;
                    }
                    else if ((expression[pos] == '|') && (expression[pos + 1] == '|'))
                    {
                        curr++;
                        stack[curr].type = 1;
                        stack[curr].obj.op = 26;
                        pos += 2;
                        state = true;
                    }
                    else if ((expression[pos] == '&') && (expression[pos + 1] == '&'))
                    {
                        curr++;
                        stack[curr].type = 1;
                        stack[curr].obj.op = 27;
                        pos += 2;
                        state = true;
                    }
                    else
                    {
                        curr++;
                        stack[curr].type = 1;
                        if (expression[pos] == '=') stack[curr].obj.op = 20;
                        if (expression[pos] == '<') stack[curr].obj.op = 21;
                        if (expression[pos] == '>') stack[curr].obj.op = 22;
                        if (expression[pos] == '|') stack[curr].obj.op = 26;
                        if (expression[pos] == '&') stack[curr].obj.op = 27;
                        pos++;
                        state = true;
                    }
                }
                else
                {
                    _drawingContext.Logger?.LogInformation("Syntax Error:  Expected operator or ')'\n");
                    cont = false;
                    err = true;
                }
            }

            if (pos >= expression.Length) cont = false;
        }

        if (!err)
        {
            rc = eval(1, stack, ref curr);
/*  stkdmp(stack,curr);  */
            if (rc)
            {
                err = true;
            }
            else
            {
                if (curr == 0)
                {
                    if (stack[0].type == -1)
                    {
                        pst.type = 1;
                        pst.result.pgr = stack[0].obj.pgr;
                    }
                    else if (stack[0].type == -2)
                    {
                        pst.type = 0;
                        pst.result.stn = stack[0].obj.stn;
                    }
                    else
                    {
                        _drawingContext.Logger?.LogInformation("GAEXPR Logic Error Number 29\n");
                        err = true;
                    }
                }
                else
                {
                    _drawingContext.Logger?.LogInformation("Syntax Error:  Unmatched Parens\n");
                    err = true;
                }
            }
        }

        if (err)
        {
            if (pass == 1)
            {
                i = 1 + pos;
                _drawingContext.Logger?.LogInformation($"  Error ocurred at column {i}");
            }

/*  release any memory still hung off the stack  */
            for (i = 0; i <= curr; i++)
            {
                if (stack[i].type == -1)
                {
                    pgr = stack[i].obj.pgr;
                    pst.result.pgr = null;
                }
                else if (stack[i].type == -2)
                {
                    // stn = stack[i].obj.stn;
                    // for (j = 0; j < BLKNUM; j++) {
                    //     if (stn.blks[j] != null) gree(stn.blks[j], "f172");
                    // }
                    // gree(stn);
                    pst.result.stn = null;
                }
            }
        }

        stack = null;
        pass--;
        return (err ? 1 : 0);
    }


/* Evaluate the stack.  If the state is zero, then don't go
   past an addition operation unless enclosed in parens.
   If state is one, then do all operations to get the result.
   If we hit an error, set up the stack pointer to insure
   everything gets released properly.                                   */

    bool eval(int state, smem[] stack, ref int cpos)
    {
        int op, rc;
        int curr, curr1, curr2;
        bool err, cont, pflag;

        curr = cpos;
        err = false;
        cont = true;
        pflag = false;
        while (curr > 0 && cont)
        {
            /* Handle an operand in the stack.  An operand is preceded by
               either a left paren, or an operator.  We will do an operation
               if it is * or /, or if it is enclosed in parens, or if we
               have hit the end of the expression.                           */

            if (stack[curr].type < 0)
            {
                /* Operand?                   */
                curr2 = curr; /* Remember operand           */
                curr--; /* Look at prior item         */
                if (stack[curr].type == 2)
                {
                    /* Left paren?                */
                    if (pflag)
                    {
                        /* Was there a matching right?*/
                        stack[curr].type = stack[curr2].type; /* Yes, restack oprnd*/
                        stack[curr].obj = stack[curr2].obj;
                        pflag = false; /* Have evaluated parens      */
                    }
                    else
                    {
                        /* If not,                    */
                        cont = false; /*  stop here,                */
                        curr++; /*  leaving operand on stack  */
                    }
                }
                else if (stack[curr].type == 1)
                {
                    /* Prior item an operator?  */
                    op = stack[curr].obj.op; /* Remember operator          */
                    curr--; /* Get other operand          */
                    if (stack[curr].type > 0)
                    {
                        /* Better be an operand       */
                        cont = false; /* If not then an error       */
                        err = true;
                        _drawingContext.Logger?.LogInformation("Internal logic check 12");
                    }
                    else
                    {
                        /* Is an operand...           */
                        curr1 = curr; /* Get the operand            */
                        if (op < 2 || pflag || state > 0)
                        {
                            /* Operate?    */
                            rc = gaoper(stack, curr1, curr2, curr, op); /* Yes...      */
                            if (rc > 0)
                            {
                                /* Did we get an error?       */
                                cont = false;
                                err = true; /* Yes...  don't continue     */
                                curr += 2; /* Leave ptr so can free ops  */
                            }
                        }
                        else
                        {
                            /* Don't operate...           */
                            curr += 2; /* Leave stuff stacked        */
                            cont = false; /* Won't continue             */
                        }
                    }
                }
                else
                {
                    /* Prior item invalid         */
                    _drawingContext.Logger?.LogInformation("Internal logic check 14 \n");
                    cont = false;
                    err = true;
                }
            }
            else if (stack[curr].type == 3)
            {
                /* Current item right paren?  */
                pflag = true; /* Indicate we found paren    */
                curr--; /* Unstack it                 */
            }
            else
            {
                cont = false;
                err = true;
            } /* Invalid if not op or paren */
        }

        cpos = curr;
        return (err);
    }

/* Perform an operation on two data objects.  Determine what class
   of data we are working with and call the appropriate routine     */

    int gaoper(smem[] stack, int c1, int c2, int c, int op)
    {
        GradsGrid pgr = null;
        gastn stn;

        /* If both grids, do grid-grid operation */
        if (stack[c1].type == -1 && stack[c2].type == -1)
        {
            pgr = null;
            pgr = gagrop(stack[c1].obj.pgr, stack[c2].obj.pgr, op, 1);
            if (pgr == null) return (1);
            stack[c].type = -1;
            stack[c].obj.pgr = pgr;
            return (0);
        }

        /* If both stns, do stn-stn operation */

        if (stack[c1].type == -2 && stack[c2].type == -2)
        {
            stn = null;
            stn = gastop(stack[c1].obj.stn, stack[c2].obj.stn, op, 1);
            if (stn == null) return (1);
            stack[c].type = -2;
            stack[c].obj.stn = stn;
            return (0);
        }

        /* Operation between grid and stn is invalid -- unless the grid
           is really a constant.  Check for this.  */

        if (stack[c1].type == -1) pgr = stack[c1].obj.pgr;
        if (stack[c2].type == -1) pgr = stack[c2].obj.pgr;
        if (pgr.IDimension == -1 && pgr.JDimension == -1)
        {
            if (stack[c1].type == -2)
            {
                stn = gascop(stack[c1].obj.stn, pgr.MinimumGridValue, op, 0);
            }
            else
            {
                stn = gascop(stack[c2].obj.stn, pgr.MinimumGridValue, op, 1);
            }

            if (stn == null) return (1);
            stack[c].type = -2;
            stack[c].obj.stn = stn;
        }
        else
        {
            _drawingContext.Logger?.LogInformation("Operation Error: Incompatable Data Types\n");
            _drawingContext.Logger?.LogInformation("  One operand was stn data, other was grid\n");
            return (1);
        }

        return (0);
    }

/* Perform the operation between two grid data objects.
   Varying dimensions must match.  Grids with fewer varying dimensions
   are 'expanded' to match the larger grid as needed.                 */

    public static GradsGrid gagrop(GradsGrid pgr1, GradsGrid pgr2,
        int op, int rel)
    {
        int val1, val2;
        int dnum1, dnum2;
        GradsGrid pgr;
        int incr, imax, omax;
        int i, i1, i2;
        bool swap;
        int uval1, uval2;

        /* Figure out how many varying dimensions for each grid.            */

        val1 = 0;
        uval1 = 0;
        dnum1 = 0;
        if (pgr1.IDimension > -1) dnum1++;
        if (pgr1.JDimension > -1) dnum1++;

        val2 = 0;
        uval2 = 0;
        dnum2 = 0;
        if (pgr2.IDimension > -1) dnum2++;
        if (pgr2.JDimension > -1) dnum2++;

        /* Force operand 1 (pgr1, dnum1, etc.) to have fewer varying dims.  */
        swap = false;
        if (dnum2 < dnum1)
        {
            pgr = pgr1;
            pgr1 = pgr2;
            pgr2 = pgr;
            swap = true;
            i = dnum1;
            dnum1 = dnum2;
            dnum2 = i;
        }

        /* Check the validity of the operation (same dimensions varying;
           same absolute dimension ranges.    First do the case where there
           are the same number of dimensions varying (dnum1=dnum2=0,1,2).   */

        if (dnum1 == dnum2)
        {
            if (pgr1.IDimension != pgr2.IDimension || pgr1.JDimension != pgr2.JDimension) goto err1;
            i = pgr1.IDimension;
            if (dnum1 > 0 && gagchk(pgr1, pgr2, pgr1.IDimension) > 0) goto err2;
            i = pgr1.JDimension;
            if (dnum1 > 1 && gagchk(pgr1, pgr2, pgr1.JDimension) > 0) goto err2;
            incr = 0;
            imax = pgr1.ISize * pgr1.JSize;

            /* Case where dnum1=0, dnum2=1 or 2.  */
        }
        else if (dnum1 == 0)
        {
            incr = pgr2.ISize * pgr2.JSize;
            imax = 1;

            /* Case where dnum1=1, dnum2=2.  */
        }
        else
        {
            i = pgr1.IDimension;
            if (gagchk(pgr1, pgr2, pgr1.IDimension) > 0) goto err2;
            if (pgr1.IDimension == pgr2.IDimension)
            {
                incr = 0;
                imax = pgr1.ISize;
            }
            else if (pgr1.IDimension == pgr2.JDimension)
            {
                incr = pgr2.ISize;
                imax = pgr1.ISize;
            }
            else goto err1;
        }

        omax = pgr2.ISize * pgr2.JSize;

        /* Perform the operation.  Put the result in operand 2 (which is
           always the operand with the greater number of varying
           dimensions).  The smaller grid is 'expanded' by using incrementing
           variables which will cause the values in the smaller grid to be
           used multiple times as needed.                                   */

        i1 = 0;
        i2 = 0;
        for (i = 0; i < omax; i++)
        {
            if (pgr1.UndefinedMask[uval1] == 0 || pgr2.UndefinedMask[uval2] == 0)
            {
                pgr2.UndefinedMask[uval2] = 0;
            }
            else
            {
                if (op == 2) pgr2.GridData[val2] = pgr1.GridData[val1] + pgr2.GridData[val2];
                else if (op == 0) pgr2.GridData[val2] = pgr1.GridData[val1] * pgr2.GridData[val2];
                else if (op == 1)
                {
                    if (swap)
                    {
                        if (GaUtil.dequal(pgr1.GridData[val1], 0.0, 1e-08) == 0)
                        {
                            pgr2.UndefinedMask[uval2] = 0;
                        }
                        else
                        {
                            pgr2.GridData[val2] = pgr2.GridData[val2] / pgr1.GridData[val1];
                        }
                    }
                    else
                    {
                        if (GaUtil.dequal(pgr2.GridData[val2], 0.0, 1e-08) == 0) pgr2.UndefinedMask[uval2] = 0;
                        else pgr2.GridData[val2] = pgr1.GridData[val1] / pgr2.GridData[val2];
                    }
                }
                else if (op == 10)
                {
                    if (swap)
                    {
                        if (Double.IsNaN(Math.Pow(pgr2.GridData[val2], pgr1.GridData[val1]))) pgr2.UndefinedMask[uval2] = 0;
                        else pgr2.GridData[val2] = Math.Pow(pgr2.GridData[val2], pgr1.GridData[val1]);
                    }
                    else
                    {
                        if (Double.IsNaN(Math.Pow(pgr1.GridData[val1], pgr2.GridData[val2]))) pgr2.UndefinedMask[uval2] = 0;
                        else pgr2.GridData[val2] = Math.Pow(pgr1.GridData[val1], pgr2.GridData[val2]);
                    }
                }
                else if (op == 11) pgr2.GridData[val2] = GaUtil.hypot(pgr1.GridData[val1], pgr2.GridData[val2]);
                else if (op == 12)
                {
                    if (pgr1.GridData[val1] == 0.0 && pgr2.GridData[val2] == 0.0) pgr2.GridData[val2] = 0.0;
                    else
                    {
                        if (swap) pgr2.GridData[val2] = Math.Atan2(pgr2.GridData[val2], pgr1.GridData[val1]);
                        else pgr2.GridData[val2] = Math.Atan2(pgr1.GridData[val1], pgr2.GridData[val2]);
                    }
                }
                else if (op == 13)
                {
                    if (swap)
                    {
                        if (pgr1.GridData[val1] < 0.0) pgr2.UndefinedMask[uval2] = 0;
                    }
                    else
                    {
                        if (pgr2.GridData[val2] < 0.0) pgr2.UndefinedMask[uval2] = 0;
                        else pgr2.GridData[val2] = pgr1.GridData[val1];
                    }
                }
                else if (op == 14)
                {
                    /* for if function.  pairs with op 15 */
                    if (swap)
                    {
                        if (pgr2.GridData[val2] < 0.0) pgr2.GridData[val2] = 0.0;
                        else pgr2.GridData[val2] = pgr1.GridData[val1];
                    }
                    else
                    {
                        if (pgr1.GridData[val1] < 0.0) pgr2.GridData[val2] = 0.0;
                    }
                }
                else if (op == 15)
                {
                    if (swap)
                    {
                        if (pgr2.GridData[val2] < 0.0) pgr2.GridData[val2] = pgr1.GridData[val1];
                        else pgr2.GridData[val2] = 0.0;
                    }
                    else
                    {
                        if (!(pgr1.GridData[val1] < 0.0)) pgr2.GridData[val2] = 0.0;
                    }
                }
                else if (op >= 21 && op <= 24)
                {
                    if (swap)
                    {
                        if (op == 21)
                        {
                            if (pgr2.GridData[val2] < pgr1.GridData[val1]) pgr2.GridData[val2] = 1.0;
                            else pgr2.GridData[val2] = -1.0;
                        }

                        if (op == 22)
                        {
                            if (pgr2.GridData[val2] > pgr1.GridData[val1]) pgr2.GridData[val2] = 1.0;
                            else pgr2.GridData[val2] = -1.0;
                        }

                        if (op == 23)
                        {
                            if (pgr2.GridData[val2] <= pgr1.GridData[val1]) pgr2.GridData[val2] = 1.0;
                            else pgr2.GridData[val2] = -1.0;
                        }

                        if (op == 24)
                        {
                            if (pgr2.GridData[val2] >= pgr1.GridData[val1]) pgr2.GridData[val2] = 1.0;
                            else pgr2.GridData[val2] = -1.0;
                        }
                    }
                    else
                    {
                        if (op == 21)
                        {
                            if (pgr1.GridData[val1] < pgr2.GridData[val2]) pgr2.GridData[val2] = 1.0;
                            else pgr2.GridData[val2] = -1.0;
                        }

                        if (op == 22)
                        {
                            if (pgr1.GridData[val1] > pgr2.GridData[val2]) pgr2.GridData[val2] = 1.0;
                            else pgr2.GridData[val2] = -1.0;
                        }

                        if (op == 23)
                        {
                            if (pgr1.GridData[val1] <= pgr2.GridData[val2]) pgr2.GridData[val2] = 1.0;
                            else pgr2.GridData[val2] = -1.0;
                        }

                        if (op == 24)
                        {
                            if (pgr1.GridData[val1] >= pgr2.GridData[val2]) pgr2.GridData[val2] = 1.0;
                            else pgr2.GridData[val2] = -1.0;
                        }
                    }
                }
                else if (op == 20)
                {
                    if (pgr1.GridData[val1] == pgr2.GridData[val2]) pgr2.GridData[val2] = 1.0;
                    else pgr2.GridData[val2] = -1.0;
                }
                else if (op == 25)
                {
                    if (pgr1.GridData[val1] != pgr2.GridData[val2]) pgr2.GridData[val2] = 1.0;
                    else pgr2.GridData[val2] = -1.0;
                }
                else if (op == 26)
                {
                    if ((pgr1.GridData[val1] < 0.0) && (pgr2.GridData[val2] < 0.0)) pgr2.GridData[val2] = -1.0;
                    else pgr2.GridData[val2] = 1.0;
                }
                else if (op == 27)
                {
                    if ((pgr1.GridData[val1] >= 0.0) && (pgr2.GridData[val2] >= 0.0)) pgr2.GridData[val2] = 1.0;
                    else pgr2.GridData[val2] = -1.0;
                }
                else
                {
                    GradsEngine.Logger?.LogInformation("Internal logic check 17: invalid oper value\n");
                    return (null);
                }
            }

            val2++;
            uval2++;
            i2++;
            if (i2 >= incr)
            {
                i2 = 0;
                val1++;
                uval1++;
                i1++;
            } /* Special increment for*/

            if (i1 >= imax)
            {
                i1 = 0;
                val1 = 0;
                uval1 = 0;
            } /*   the smaller grid   */
        }

        /* If requested, release the storage for operand 1 (which does not
           contain the result).  Note that this refers to operand 1 AFTER
           the possible grid swap earlier in the routine.                   */


        return (pgr2);

        err1:
        GradsEngine.Logger?.LogInformation("Operation error:  Incompatable grids \n");
        GradsEngine.Logger?.LogInformation("   Varying dimensions are different\n");
        GradsEngine.Logger?.LogInformation($"  1st grid dims = {pgr1.IDimension} {pgr2.IDimension}   2nd = {pgr1.JDimension} {pgr2.JDimension}");
        return (null);

        err2:
        GradsEngine.Logger?.LogInformation("Operation error:  Incompatable grids \n");
        GradsEngine.Logger?.LogInformation($"  Dimension = {i}");
        GradsEngine.Logger?.LogInformation(
            $"  1st grid range = {pgr1.DimensionMinimum[i]} {pgr1.DimensionMaximum[i]}   2nd = {pgr2.DimensionMinimum[i]} {pgr2.DimensionMaximum[i]}");
        return (null);
    }


/* Perform operation on two stn data items.  The operation is done
   only when the varying dimensions are equal.  Currently, only
   three station data dimension environments are supported:
   X,Y varying (X,Y plot), T varying (time series), and Z
   varying (vertical profile).  This routine will probably need to
   be rewritten at some point.                                     */

    gastn gastop(gastn stn1, gastn stn2,
        int op, int rel)
    {
        // gastn *stn;
        // garpt *rpt1, *rpt2;
        // int swap, i, j, flag, dimtyp;
        //
        // /* Verify dimension environment */
        //
        // if (stn1.idim == 0 && stn1.jdim == 1 &&
        //     stn2.idim == 0 && stn2.jdim == 1)
        //     dimtyp = 1;                                 /* X and Y are varying */
        // else if (stn1.idim == 2 && stn1.jdim == -1 &&
        //          stn2.idim == 2 && stn2.jdim == -1)
        //     dimtyp = 2;                                 /* Z is varying */
        // else if (stn1.idim == 3 && stn1.jdim == -1 &&
        //          stn2.idim == 3 && stn2.jdim == -1)
        //     dimtyp = 3;                                 /* T is varying */
        // else {
        //     _drawingContext.Logger?.LogInformation("Invalid dimension environment for station data");
        //     _drawingContext.Logger?.LogInformation(" operation\n");
        //     return (null);
        // }
        //
        // /* Set it up so first stn set has fewer stations */
        //
        // swap = 0;
        // if (stn1.rnum > stn2.rnum) {
        //     stn = stn1;
        //     stn1 = stn2;
        //     stn2 = stn;
        //     swap = 1;
        // }
        //
        // /* Loop through stations of 1st station set.  Find matching
        //    stations in 2nd station set.  If a match, perform operation.
        //    Any duplicates in the 2nd station set get ignored.      */
        //
        // rpt1 = stn1.rpt;
        // for (i = 0; i < stn1.rnum; i++, rpt1 = rpt1.rpt) {
        //     if (rpt1.umask == 0) continue;
        //     flag = 0;
        //     rpt2 = stn2.rpt;
        //     for (j = 0; j < stn2.rnum; j++, rpt2 = rpt2.rpt) {
        //         if (rpt2.umask == 0) continue;
        //         if (dimtyp == 1 && dequal(rpt1.lat, rpt2.lat, 1e-08) != 0) continue;
        //         if (dimtyp == 1 && dequal(rpt1.lon, rpt2.lon, 1e-08) != 0) continue;
        //         if (dimtyp == 2 && dequal(rpt1.lev, rpt2.lev, 1e-08) != 0) continue;
        //         if (dimtyp == 3 && dequal(rpt1.tim, rpt2.tim, 1e-08) != 0) continue;
        //         if (op == 2)
        //             rpt1.val = rpt1.val + rpt2.val;
        //         else if (op == 0)
        //             rpt1.val = rpt1.val * rpt2.val;
        //         else if (op == 1) {
        //             if (swap) {
        //                 if (dequal(rpt1.val, 0.0, 1e-08) == 0) rpt1.umask = 0;
        //                 else rpt1.val = rpt2.val / rpt1.val;
        //             } else {
        //                 if (dequal(rpt2.val, 0.0, 1e-08) == 0) rpt1.umask = 0;
        //                 else rpt1.val = rpt1.val / rpt2.val;
        //             }
        //         } else if (op == 10) {
        //             if (swap) {
        //                 if (Double.IsNaN(Math.Pow(rpt2.val, rpt1.val))) rpt1.umask = 0;
        //                 else rpt1.val = Math.Pow(rpt2.val, rpt1.val);
        //             } else {
        //                 if (Double.IsNaN(Math.Pow(rpt1.val, rpt2.val))) rpt1.umask = 0;
        //                 else rpt1.val = Math.Pow(rpt1.val, rpt2.val);
        //             }
        //         } else if (op == 11)
        //             rpt1.val = hypot(rpt1.val, rpt2.val);
        //         else if (op == 12) {
        //             if ((dequal(rpt1.val, 0.0, 1e-08) == 0) && (dequal(rpt2.val, 0.0, 1e-08) == 0))
        //                 rpt1.val = 0.0;
        //             else rpt1.val = Math.Atan2(rpt1.val, rpt2.val);
        //         } else if (op == 13) {
        //             if (swap) {
        //                 if (rpt1.val < 0.0) rpt1.umask = 0;
        //                 else rpt1.val = rpt2.val;
        //             } else {
        //                 if (rpt2.val < 0.0) rpt1.umask = 0;
        //             }
        //         } else {
        //             _drawingContext.Logger?.LogInformation("Internal logic check 57: invalid oper value\n");
        //             return (null);
        //         }
        //         flag = 1;
        //         break;
        //     }
        //     if (!flag) rpt1.umask = 0;
        // }
        //
        // /* Release storage if requested then return */
        //
        // if (rel) {
        //     for (i = 0; i < BLKNUM; i++) {
        //         if (stn2.blks[i] != null) gree(stn2.blks[i], "f168");
        //     }
        //     gree(stn2, "f169");
        // }
        return (stn1);
    }

/* Perform operation between a stn set and a constant   */

    gastn gascop(gastn stn, double val, int op, int swap)
    {
        // garpt *rpt;
        // int i;
        //
        // /* Loop through stations.  Perform operation.              */
        //
        // rpt = stn.rpt;
        // for (i = 0; i < stn.rnum; i++, rpt = rpt.rpt) {
        //     if (rpt.umask == 0) continue;
        //     if (op == 2)
        //         rpt.val = rpt.val + val;
        //     else if (op == 0)
        //         rpt.val = rpt.val * val;
        //     else if (op == 1) {
        //         if (swap) {
        //             if (dequal(rpt.val, 0.0, 1e-08) == 0) rpt.umask = 0;
        //             else rpt.val = val / rpt.val;
        //         } else {
        //             if (dequal(val, 0.0, 1e-08) == 0) rpt.umask = 0;
        //             else rpt.val = rpt.val / val;
        //         }
        //     } else if (op == 10) {
        //         if (swap) {
        //             if (Double.IsNaN(Math.Pow(val, rpt.val))) rpt.umask = 0;
        //             else rpt.val = Math.Pow(val, rpt.val);
        //         } else {
        //             if (Double.IsNaN(Math.Pow(rpt.val, val))) rpt.umask = 0;
        //             else rpt.val = Math.Pow(rpt.val, val);
        //         }
        //     } else if (op == 11)
        //         rpt.val = hypot(rpt.val, val);
        //     else if (op == 12) {
        //         if (dequal(rpt.val, 0.0, 1e-08) == 0 && dequal(val, 0.0, 1e-08) == 0)
        //             rpt.val = 0.0;
        //         else {
        //             if (swap) rpt.val = Math.Atan2(val, rpt.val);
        //             else rpt.val = Math.Atan2(rpt.val, val);
        //         }
        //     } else if (op == 13) {
        //         if (rpt.val < 0.0) rpt.umask = 0;
        //     } else {
        //         _drawingContext.Logger?.LogInformation("Internal logic check 57: invalid oper value\n");
        //         return (null);
        //     }
        // }
        return (stn);
    }

/* Put a constant value into a grid.  We will change this at
   some point to have three data types (grid, stn, constant) but
   for now it is easier to keep the constant grid concept.     */

    GradsGrid gagrvl(double val)
    {
        GradsGrid pgr;
        int i;
        long sz;

        /* Allocate memory */
        pgr = new GradsGrid();
        /* Fill in gagrid variables */
        pgr.pfile = null;
        pgr.Undef = -9.99e8;
        pgr.pvar = null;
        pgr.IDimension = -1;
        pgr.JDimension = -1;
        pgr.alocf = 0;
        for (i = 0; i < 5; i++)
        {
            pgr.DimensionMinimum[i] = 0;
            pgr.DimensionMaximum[i] = 0;
        }

        pgr.MinimumGridValue = val;
        pgr.MaximumGridValue = val;
        pgr.GridData = new double[] { pgr.MinimumGridValue };
        pgr.umin = 1;
        pgr.UndefinedMask = new byte[] { pgr.umin };
        pgr.ISize = 1;
        pgr.JSize = 1;
        pgr.exprsn = null;
        return (pgr);
    }

/* Handle a variable or function call.  If successful, we return
   a data object (pointed to by the pst) and a ptr to the first
   character after the variable or function name.  If an error
   happens, we return a null pointer.                                */

    int? varprs(string ch, int ipos, gastat pst)
    {
        GradsGrid? pgr, pgr2 = null;
        GradsFile? pfi;
        gavar? pvar, pvar2, vfake = new gavar();

        Func<double[], double, double>? conv;

        double[] dmin = new double[5], dmax = new double[5];
        double d1, d2;
        int r, r2;
        float wrot;
        int i, fnum, ii, jj, rc, dotflg, idim, jdim, dim, sbu;
        int[] id = new int [5];
        double[] cvals;
        int toff = 0;
        int size, j, dotest, defined;
        char[] name = new char[20], vnam = new char[20];
        string sName = "", sVName = "";
        int ru, r2u;
        int? pos;
        long sz;

        /* Get the variable or function name.  It must start with a
           letter, and consist of letters or numbers or underscore.  */
        i = 0;
        while (i < ch.Length && ((ch[ipos] >= 'a' && ch[ipos] <= 'z') || (ch[ipos] >= '0' && ch[ipos] <= '9') || (ch[ipos] == '_') || (ch[ipos] >= 'A' && ch[ipos] <= 'Z')) )
        {
            name[i] = ch[ipos];
            vnam[i] = ch[ipos];
            ipos++;
            i++;
            if (i > 16) break;
        }

        name = name.Take(i).ToArray();
        vnam = vnam.Take(i).ToArray(); /* Save 'i' for next loop */

        /* Check for the data set number in the variable name.  If there,
           then this has to be a variable name.                            */

        fnum = pst.fnum;
        dotflg = 0;
        if (ipos < ch.Length && ch[ipos] == '.')
        {
            dotflg = 1;
            ipos++;
            pos = GaUtil.intprs(ch, ipos, out fnum);
            if (pos == null || fnum < 1)
            {
                _drawingContext.Logger?.LogInformation("Syntax error: Bad file number for variable {name} \n");
                return (null);
            }

            vnam[i] = '.';
            i++;
            while (ipos < pos)
            {
                vnam[i] = ch[ipos];
                ipos++;
                i++;
            }

            vnam[i] = '\0';
        }

        sName = new string(name);
        sVName = new string(vnam);

        /* Check for a predefined data object. */
        pfi = null;
        pvar = null;
        defined = 0;
        if (dotflg == 0)
        {
            pfi = getdfn(sName, pst);
            if (pfi != null) defined = 1;
        }

        /* If not a defined grid, get a pointer to a file structure    */
        if (pfi == null)
        {
            if (dotflg == 0)
            {
                pfi = pst.pfid;
            }
            else
            {
                try
                {
                    pfi = pst.pfi1[fnum];
                }
                catch (Exception ex)
                {
                    _drawingContext.Logger?.LogInformation("Data Request Error:  File number out of range");
                    _drawingContext.Logger?.LogInformation($"  Variable = {sVName}");
                }
            }

            /* Check here for predefined variable name: lat,lon,lev */
            if (sName == "lat" ||
                sName == "lon" ||
                sName == "lev")
            {
                pvar = vfake;
                vfake.levels = -999;
                vfake.vecpair = -999;
                if (sName == "lon")
                {
                    vfake.offset = 0;
                    vfake.abbrv = "lon";
                }

                if (sName == "lat")
                {
                    vfake.offset = 1;
                    vfake.abbrv = "lat";
                }

                if (sName == "lev")
                {
                    vfake.offset = 2;
                    vfake.abbrv = "lev";
                }

                if (pfi.type == 2 || pfi.type == 3)
                {
                    // snprintf(pout, 1255, "Data Request Error:  Predefined variable %s\n", sVName);
                    // _drawingContext.Logger?.LogInformation(pout);
                    // _drawingContext.Logger?.LogInformation("   is only defined for grid type files\n");
                    // snprintf(pout, 1255, "   File %i is a station file\n", fnum);
                    // _drawingContext.Logger?.LogInformation(pout);
                    return (null);
                }
            }
            else
            {
                /* See if this is a variable name.
               If not, give an error message (if a file number was specified)
               or check for a function call via rtnprs.   */
                pvar = pfi.pvar1.FirstOrDefault(x => x.abbrv == sName);
                if (pvar == null)
                {
                    if (dotflg > 0)
                    {
                        _drawingContext.Logger?.LogInformation("Data Request Error:  Invalid variable name ");
                        _drawingContext.Logger?.LogInformation($"  Variable {sVName} not found in file {fnum}\n");
                        return (null);
                    }
                    else
                    {
                        //pos = rtnprs(ch, sName, pst); /* Handle function call */
                        throw new Exception("Function calls not implemented yet");
                        return (pos);
                    }
                }
            }
        }

        /* It wasn't a function call (or we would have returned).
           If the variable is to a stn type file, call the parser
           routine that handles stn requests.                         */
        if (pfi.type == 2 || pfi.type == 3)
        {
            // ch = stnvar(ch, sVName, pfi, pvar, pst);
            // return (ch);
            throw new NotImplementedException();
        }

        /* We are dealing with a grid data request.  We handle this inline.
           Our default dimension limits are defined in gastat.  These
           may be modified by the user (by specifying the new settings
           in parens).  First get grid coordinates of the limits, then
           figure out if user modifies these.        */

        /* Convert world coordinates in the status block to grid
           dimensions using the file scaling for this variable.  */

        for (i = 0; i < 5; i++)
        {
            if (i == 3)
            {
                dmin[i] = GaUtil.t2gr(pfi.abvals[i], pst.tmin);
                dmax[i] = GaUtil.t2gr(pfi.abvals[i], pst.tmax);
            }
            else
            {
                conv = pfi.ab2gr[i];
                cvals = pfi.abvals[i];
                dmin[i] = conv(cvals, pst.dmin[i]);
                dmax[i] = conv(cvals, pst.dmax[i]);
            }
        }

        /* Round varying dimensions 'outwards' to integral grid units. */
        for (i = 0; i < 5; i++)
        {
            if (i == pst.idim || i == pst.jdim)
            {
                dmin[i] = Math.Floor(dmin[i] + 0.0001);
                dmax[i] = Math.Ceiling(dmax[i] - 0.0001);
                if (dmax[i] <= dmin[i])
                {
                    _drawingContext.Logger?.LogInformation("Data Request Error: Invalid grid coordinates");
                    _drawingContext.Logger?.LogInformation($"  Varying dimension {i} decreases: {dmin[i]} to {dmax[i]}");
                    _drawingContext.Logger?.LogInformation($"  Error ocurred getting variable '{sVName}'");
                    return (null);
                }
            }
        }

        /* Check for user provided dimension expressions */
        if (ipos < ch.Length && ch[ipos] == '(')
        {
            ipos++;
            for (i = 0; i < 5; i++) id[i] = 0;
            while (ch[ipos] != ')')
            {
                pos = GaUtil.dimprs(ch, ipos, pst, pfi, out dim, out d1, 1, out rc);
                if (pos == null)
                {
                    _drawingContext.Logger?.LogInformation($"  Variable name = {sVName}");
                    return (null);
                }

                if (id[dim]>0)
                {
                    _drawingContext.Logger?.LogInformation("Syntax Error: Invalid dimension expression\n");
                    _drawingContext.Logger?.LogInformation("  Same dimension specified multiple times ");
                    _drawingContext.Logger?.LogInformation($"for variable = {sVName}\n");
                    return (null);
                }

                id[dim] = 1;
                if (dim == pst.idim || dim == pst.jdim)
                {
                    _drawingContext.Logger?.LogInformation("Data Request Error: Invalid dimension expression\n");
                    _drawingContext.Logger?.LogInformation("  Attempt to set or modify varying dimension\n");
                    _drawingContext.Logger?.LogInformation($"  Variable = {sVName}, Dimension = {dim} \n");
                    return (null);
                }

                dmin[dim] = d1;
                dmax[dim] = d1;
                /* check if we need to set flag for time offset */
                if (rc > 1)
                {
                    if (defined == 1)
                    {
                        _drawingContext.Logger?.LogInformation("Error: The \"offt\" dimension expression is ");
                        _drawingContext.Logger?.LogInformation("       not supported for defined variables. ");
                        return (null);
                    }
                    else toff = 1;
                }

                ipos = pos ?? int.MaxValue;
                if (ch[ipos] == ',') ipos++;
            }

            ipos++;
        }

        /* If request from a defined grid, ignore fixed dimensions
           in the defined grid */

        if (pfi.type == 4)
        {
            for (i = 0; i < 5; i++)
            {
                if (pfi.dnum[i] == 1)
                {
                    dmin[i] = 0.0;
                    dmax[i] = 0.0;
                }
            }
        }

        /* All the grid level coordinates are set.  Insure they
           are integral values, otherwise we can't do it.   The varying
           dimensions will be integral (since we forced them to be
           earlier) so this is only relevent for fixed dimensions. */

        for (i = 0; i < 5; i++)
        {
            if (dmin[i] < 0.0)
                ii = (int)(dmin[i] - 0.1);
            else
                ii = (int)(dmin[i] + 0.1);
            d1 = ii;
            if (dmax[i] < 0.0)
                ii = (int)(dmax[i] - 0.1);
            else
                ii = (int)(dmax[i] + 0.1);
            d2 = ii;
            /* ignore z test if variable has no levels */
            dotest = 1;
            if (pvar != null)
            {
                if (pvar.levels == 0 && i == 2) dotest = 0;
            }

            if ((GaUtil.dequal(dmin[i], d1, 1e-8) != 0 || GaUtil.dequal(dmax[i], d2, 1e-8) != 0) && dotest == 1)
            {
                _drawingContext.Logger?.LogInformation("Data Request Error: Invalid grid coordinates");
                _drawingContext.Logger?.LogInformation("  World coordinates convert to non-integer");
                _drawingContext.Logger?.LogInformation("  grid coordinates");
                _drawingContext.Logger?.LogInformation($"    Variable = {sVName}  Dimension = {i} ");
                return (null);
            }
        }
        /* Variable has been parsed and is valid, and the ch pointer is
           set to the first character past it.  We now need to set up
           the grid requestor block and get the grid.  */

        pgr = new GradsGrid();

        pgr.DataReader = pfi.DataReader;
        /* Fill in gagrid variables */

        idim = pst.idim;
        jdim = pst.jdim;
        pgr.alocf = 0;
        pgr.pfile = pfi;
        pgr.Undef = pfi.undef;
        pgr.pvar = pvar;
        pgr.IDimension = idim;
        pgr.JDimension = jdim;
        pgr.iwrld = 0;
        pgr.jwrld = 0;
        pgr.toff = toff;
        for (i = 0; i < 5; i++)
        {
            if (dmin[i] < 0.0)
            {
                pgr.DimensionMinimum[i] = (int)(dmin[i] - 0.1);
            }
            else
            {
                pgr.DimensionMinimum[i] = (int)(dmin[i] + 0.1);
            }

            if (dmax[i] < 0.0)
            {
                pgr.DimensionMaximum[i] = (int)(dmax[i] - 0.1);
            }
            else
            {
                pgr.DimensionMaximum[i] = (int)(dmax[i] + 0.1);
            }
        }

        pgr.WorldDimensionMaximum = pst.dmax;
        pgr.WorldDimensionMinimum = pst.dmin;

        pgr.exprsn = null;
        pgr.ilinr = 1;
        pgr.jlinr = 1;
        if (idim > -1 && idim != 3)
        {
            pgr.igrab = pfi.gr2ab[idim];
            pgr.iabgr = pfi.ab2gr[idim];
        }

        if (jdim > -1 && jdim != 3)
        {
            pgr.jgrab = pfi.gr2ab[jdim];
            pgr.jabgr = pfi.ab2gr[jdim];
        }

        if (idim > -1 && jdim <= 4)
        {
            /* qqqqq xxxxx fix this later ? */
            pgr.ivals = pfi.grvals[idim];
            pgr.iavals = pfi.abvals[idim];
            pgr.ilinr = pfi.linear[idim];
        }

        if (jdim > -1 && jdim <= 4)
        {
            /* qqqqq xxxxx fix this later ? */
            pgr.jvals = pfi.grvals[jdim];
            pgr.javals = pfi.abvals[jdim];
            pgr.jlinr = pfi.linear[jdim];
        }

        pgr.GridData = null;

        if (pfi != null && pvar != null && pfi.ppflag > 0 && pfi.ppwrot > 0 && pvar.vecpair > 0)
        {
            pgr2 = (GradsGrid)pgr.Clone();
        }

        /* Get grid */
        rc = _drawingContext.GaIO.gaggrd(pgr);
        if (rc > 0)
        {
            _drawingContext.Logger?.LogInformation($"Data Request Error:  Error for variable '{sVName}'\n");
            return (null);
        }

        if (rc < 0)
        {
            _drawingContext.Logger?.LogInformation($"  Warning issued for variable = {sVName}");
        }

        /* Special test for auto-interpolated data, when the
           data requested is U or V.  User MUST indicate variable unit
           number in the descriptor file for auto-rotation to take place */

        if (pfi != null && pvar != null && pfi.ppflag > 0 && pfi.ppwrot > 0 && pvar.vecpair > 0)
        {
            /* Find the matching vector component */
            if (pvar.isu > 0) sbu = 0; /* if pvar is u, then matching component should not be u */
            else sbu = 1; /* pvar is v, so matching component should be u */
            pvar2 = pfi.pvar1.FirstOrDefault(x => x.vecpair == pvar.vecpair && x.isu == sbu);
            if (pvar2 == null)
            {
                /* didn't find a match */
                ru = 0;
                size = pgr.ISize * pgr.JSize;
                for (i = 0; i < size; i++)
                {
                    pgr.UndefinedMask[ru] = 0;
                    ru++;
                }
            }
            else
            {
                /* get the 2nd grid */
                pgr2.pvar = pvar2;
                rc = _drawingContext.GaIO.gaggrd(pgr2);
                if (rc > 0)
                {
                    _drawingContext.Logger?.LogInformation($"Data Request Error:  Error for variable '{sVName}'\n");
                    return (null);
                }

                /* r is u component, r2 is v component */
                ii = pgr.DimensionMinimum[0];
                jj = pgr.DimensionMinimum[1];
                if (pvar2.isu > 0)
                {
                    r = 0;
                    r2 = 0;
                    ru = 0;
                    r2u = 0;

                    for (j = 0; j < pgr.JSize; j++)
                    {
                        if (pgr.IDimension == 0) ii = pgr.DimensionMinimum[0];
                        if (pgr.IDimension == 1) jj = pgr.DimensionMinimum[1];
                        for (i = 0; i < pgr.ISize; i++)
                        {
                            if (pgr2.UndefinedMask[ru] == 0 || pgr.UndefinedMask[r2u] == 0)
                            {
                                /* u or v is undefined */
                                pgr2.UndefinedMask[ru] = 0;
                                pgr.UndefinedMask[r2u] = 0;
                            }
                            else
                            {
                                if (ii < 1 || ii > pfi.dnum[0] ||
                                    jj < 1 || jj > pfi.dnum[1])
                                {
                                    /* outside file's grid dimensions */
                                    pgr2.UndefinedMask[ru] = 0;
                                    pgr.UndefinedMask[r2u] = 0;
                                }
                                else
                                {
                                    /* get wrot value for grid element */
                                    wrot = (float)pfi.ppw[(jj - 1) * pfi.dnum[0] + ii - 1];
                                    if (wrot < -900.0)
                                    {
                                        pgr.UndefinedMask[ru] = 0;
                                        pgr2.UndefinedMask[r2u] = 0;
                                    }
                                    else if (wrot != 0.0)
                                    {
                                        if (pvar2.isu > 0)
                                        {
                                            pgr.GridData[r2] = (pgr2.GridData[r]) * Math.Sin(wrot) +
                                                           (pgr.GridData[r2]) * Math.Cos(wrot); /* display variable is v */
                                            pgr2.UndefinedMask[r2u] = 1;
                                        }
                                        else
                                        {
                                            pgr2.GridData[r] = (pgr2.GridData[r]) * Math.Cos(wrot) -
                                                           (pgr.GridData[r2]) * Math.Sin(wrot); /* display variable is u */
                                            pgr.UndefinedMask[ru] = 1;
                                        }
                                    }
                                }
                            }

                            r++;
                            r2++;
                            ru++;
                            r2u++;
                            if (pgr.IDimension == 0) ii++;
                            if (pgr.IDimension == 1) jj++;
                        }

                        if (pgr.JDimension == 1) jj++;
                    }
                }
                else
                {
                    r = 0;
                    r2 = 0;
                    ru = 0;
                    r2u = 0;
                    for (j = 0; j < pgr.JSize; j++)
                    {
                        if (pgr.IDimension == 0) ii = pgr.DimensionMinimum[0];
                        if (pgr.IDimension == 1) jj = pgr.DimensionMinimum[1];
                        for (i = 0; i < pgr.ISize; i++)
                        {
                            if (pgr.UndefinedMask[ru] == 0 || pgr2.UndefinedMask[r2u] == 0)
                            {
                                /* u or v is undefined */
                                pgr.UndefinedMask[ru] = 0;
                                pgr2.UndefinedMask[r2u] = 0;
                            }
                            else
                            {
                                if (ii < 1 || ii > pfi.dnum[0] ||
                                    jj < 1 || jj > pfi.dnum[1])
                                {
                                    /* outside file's grid dimensions */
                                    pgr.UndefinedMask[ru] = 0;
                                    pgr2.UndefinedMask[r2u] = 0;
                                }
                                else
                                {
                                    /* get wrot value for grid element */
                                    wrot = (float)pfi.ppw[(jj - 1) * pfi.dnum[0] + ii - 1];
                                    if (wrot < -900.0)
                                    {
                                        pgr.UndefinedMask[ru] = 0;
                                        pgr2.UndefinedMask[r2u] = 0;
                                    }
                                    else if (wrot != 0.0)
                                    {
                                        if (pvar2.isu > 0)
                                        {
                                            pgr2.GridData[r2] = (pgr.GridData[r]) * Math.Sin(wrot) +
                                                            (pgr2.GridData[r2]) *
                                                            Math.Cos(wrot); /* display variable is v */
                                            pgr2.UndefinedMask[r2u] = 1;
                                        }
                                        else
                                        {
                                            pgr.GridData[r] = (pgr.GridData[r]) * Math.Cos(wrot) -
                                                          (pgr2.GridData[r2]) * Math.Sin(wrot); /* display variable is u */
                                            pgr.UndefinedMask[ru] = 1;
                                        }
                                    }
                                }
                            }

                            r++;
                            r2++;
                            ru++;
                            r2u++;
                            if (pgr.IDimension == 0) ii++;
                            if (pgr.IDimension == 1) jj++;
                        }

                        if (pgr.JDimension == 1) jj++;
                    }
                }
            }
        }

        pst.result.pgr = pgr;
        pst.type = 1;
        return (ipos);
    }

    public static int gagchk(GradsGrid pgr1, GradsGrid pgr2, int dim)
    {
        double gmin1, gmax1, gmin2, gmax2, fuz1, fuz2, fuzz;
        Func<double[], double, double> conv1, conv2;
        double[] vals1, vals2;
        int i1, i2, i, siz1, siz2, rc;
        dt dtim1 = new(), dtim2 = new();

        if (dim < 0) return (0);

        if (dim == pgr1.IDimension)
        {
            conv1 = pgr1.igrab;
            vals1 = pgr1.ivals;
            i1 = pgr1.ilinr;
            siz1 = pgr1.ISize;
        }
        else if (dim == pgr1.JDimension)
        {
            conv1 = pgr1.jgrab;
            vals1 = pgr1.jvals;
            i1 = pgr1.jlinr;
            siz1 = pgr1.JSize;
        }
        else return (1);

        if (dim == pgr2.IDimension)
        {
            conv2 = pgr2.igrab;
            vals2 = pgr2.ivals;
            i2 = pgr2.ilinr;
            siz2 = pgr2.ISize;
        }
        else if (dim == pgr2.JDimension)
        {
            conv2 = pgr2.jgrab;
            vals2 = pgr2.jvals;
            i2 = pgr2.jlinr;
            siz2 = pgr2.JSize;
        }
        else return (1);

        if (siz1 != siz2)
        {
            //TODO logging
            //_drawingContext.Logger?.LogInformation("Error in gagchk: axis sizes are not the same");
            return (1);
        }

        gmin1 = pgr1.DimensionMinimum[dim];
        gmax1 = pgr1.DimensionMaximum[dim];
        gmin2 = pgr2.DimensionMinimum[dim];
        gmax2 = pgr2.DimensionMaximum[dim];

        if (dim == 3)
        {
            /* Dimension is time.      */
            rc = 0;
            GaUtil.gr2t(vals1, gmin1, out dtim1);
            GaUtil.gr2t(vals2, gmin2, out dtim2);
            if (dtim1.yr != dtim2.yr) rc = 1;
            if (dtim1.mo != dtim2.mo) rc = 1;
            if (dtim1.dy != dtim2.dy) rc = 1;
            if (dtim1.hr != dtim2.hr) rc = 1;
            if (dtim1.mn != dtim2.mn) rc = 1;
            GaUtil.gr2t(vals1, gmax1, out dtim1);
            GaUtil.gr2t(vals2, gmax2, out dtim2);
            if (dtim1.yr != dtim2.yr) rc = 1;
            if (dtim1.mo != dtim2.mo) rc = 1;
            if (dtim1.dy != dtim2.dy) rc = 1;
            if (dtim1.hr != dtim2.hr) rc = 1;
            if (dtim1.mn != dtim2.mn) rc = 1;
            if (rc > 0)
            {
                GradsEngine.Logger?.LogInformation("Error in gagchk: time axis endpoint values are not equivalent\n");
                return (1);
            }

            return (0);
        }

        /* Check endpoints.  If unequal, then automatic no match.        */

        fuz1 = Math.Abs(conv1(vals1, gmax1) - conv1(vals1, gmin1)) * Gx.FUZZ_SCALE;
        fuz2 = Math.Abs(conv2(vals2, gmax2) - conv2(vals2, gmin2)) * Gx.FUZZ_SCALE;
        fuzz = (fuz1 + fuz2) * 0.5;

        rc = 0;
        if (Math.Abs((conv1(vals1, gmin1)) - (conv2(vals2, gmin2))) > fuzz) rc = 1;
        if (Math.Abs((conv1(vals1, gmax1)) - (conv2(vals2, gmax2))) > fuzz) rc = 1;
        if (rc > 0)
        {
            GradsEngine.Logger?.LogInformation("Error in gagchk: axis endpoint values are not equivalent\n");
            return (1);
        }

        if (i1 != i2)
        {
            GradsEngine.Logger?.LogInformation("Error in gagchk: one axis is linear and the other is non-linear\n");
            return (1);
        }

        if (i1 > 0) return (0); /* If linear then matches  */

        /* Nonlinear, but endpoints match.  Check every grid point for a
           match.  If any non-matches, then not a match.     */

        for (i = 0; i < siz1; i++)
        {
            if (Math.Abs(
                    (conv1(vals1, gmin1 + (double)i)) - (conv2(vals2, gmin2 + (double)i))) >
                fuzz)
            {
                GradsEngine.Logger?.LogInformation("Error in gagchk: axis values are not all the same\n");
                return (1);
            }
        }

        return (0);
    }

    GradsFile? getdfn(string name, gastat pst)
    {
        gadefn pdf;

        if (pst.pdf1 == null) return null;
        
        /* See if the name is a defined grid */
        pdf = pst.pdf1.FirstOrDefault(x => x.abbrv == name);
        if (pdf == null) return (null);
        return (pdf.pfi);
    }

/* Handle a station data request variable.                      */
    // char* stnvar(char* ch, char* vnam, gafile* pfi,
    //     gavar* pvar, gastat* pst)
    // {
    //     gastn* stn;
    //     double dmin[5], dmax[5], d, radius;
    //     int id[6], dim, i, rc, rflag, sflag;
    //     char expression[pos];
    //     char stid[10];
    //     size_t sz;
    //
    //     rflag = 0;
    //     sflag = 0;
    //     radius = 0;
    //
    //     /* We want to finish parsing the variable name by looking at
    //        any dimension settings by the user.  First initialize the
    //        request environment to that found in the pst.             */
    //
    //     for (i = 0; i < 3; i++)
    //     {
    //         dmin[i] = pst.dmin[i];
    //         dmax[i] = pst.dmax[i];
    //     }
    //
    //     dmin[3] = t2gr(pfi.abvals[3], &(pst.tmin));
    //     dmax[3] = t2gr(pfi.abvals[3], &(pst.tmax));
    //
    //     /* Check for user provided dimension expressions */
    //     if (*ch == '(')
    //     {
    //         ch++;
    //         for (i = 0; i < 6; i++) id[i] = 0;
    //         while (*ch != ')')
    //         {
    //             if (!cmpch(ch, "stid=", 5))
    //             {
    //                 /* special stid= arg */
    //                 for (i = 0; i < 8; i++) stid[i] = ' ';
    //                 stid[8] = '\0';
    //                 pos = ch + 5;
    //                 i = 0;
    //                 while (expression[pos] != ',' && expression[pos] != ')' && i < 8)
    //                 {
    //                     stid[i] = expression[pos];
    //                     pos++;
    //                     i++;
    //                 }
    //
    //                 if (i == 0)
    //                 {
    //                     _drawingContext.Logger?.LogInformation("Dimension Expression Error: No stid provided\n");
    //                     pos = null;
    //                 }
    //
    //                 if (i > 8)
    //                 {
    //                     _drawingContext.Logger?.LogInformation("Dimension Expression Error: stid too long\n");
    //                     pos = null;
    //                 }
    //
    //                 dim = 11;
    //             }
    //             else
    //             {
    //                 pos = dimprs(ch, pst, pfi, &dim, &d, 0, &rc);
    //             }
    //
    //             if (pos == null)
    //             {
    //                 snprintf(pout, 1255, "  Variable name = %s\n", vnam);
    //                 _drawingContext.Logger?.LogInformation(pout);
    //                 return (null);
    //             }
    //
    //             if (dim < 6 && id[dim] > 1)
    //             {
    //                 _drawingContext.Logger?.LogInformation("Syntax Error: Invalid dimension expression\n");
    //                 _drawingContext.Logger?.LogInformation("  Same dimension specified more than twice ");
    //                 snprintf(pout, 1255, "for variable = %s\n", vnam);
    //                 _drawingContext.Logger?.LogInformation(pout);
    //                 return (null);
    //             }
    //
    //             if (dim == pst.idim || dim == pst.jdim ||
    //                 (dim > 3 && (pst.idim == 0 || pst.idim == 1 || pst.jdim == 1)))
    //             {
    //                 _drawingContext.Logger?.LogInformation("Data Request Error: Invalid dimension expression\n");
    //                 _drawingContext.Logger?.LogInformation("  Attempt to set or modify varying dimension\n");
    //                 snprintf(pout, 1255, "  Variable = %s, Dimension = %i \n", vnam, dim);
    //                 _drawingContext.Logger?.LogInformation(pout);
    //                 return (null);
    //             }
    //
    //             if (dim == 10)
    //             {
    //                 rflag = 1;
    //                 radius = d;
    //             }
    //             else if (dim == 11)
    //             {
    //                 sflag = 1;
    //             }
    //             else
    //             {
    //                 if (id[dim] == 0) dmin[dim] = d;
    //                 dmax[dim] = d;
    //             }
    //
    //             ch = pos;
    //             if (*ch == ',') ch++;
    //             id[dim]++;
    //         }
    //
    //         ch++;
    //     }
    //
    //     /* Verify that dmin is less than or equal to dmax for all our dims */
    //     for (i = 0; i < 4; i++)
    //     {
    //         if ((i != 2 && dmin[i] > dmax[i]) || (i == 2 && dmax[i] > dmin[i]))
    //         {
    //             _drawingContext.Logger?.LogInformation("Data Request Error: Invalid grid coordinates\n");
    //             snprintf(pout, 1255, "  Varying dimension %i decreases: %g to %g \n", i, dmin[i], dmax[i]);
    //             _drawingContext.Logger?.LogInformation(pout);
    //             snprintf(pout, 1255, "  Error ocurred getting variable '%s'\n", vnam);
    //             _drawingContext.Logger?.LogInformation(pout);
    //             return (null);
    //         }
    //     }
    //
    //     /* Looks like the user specified good stuff, and we are ready to
    //        try to get some data.  Allocate and fill in a gastn block.     */
    //
    //     sz = sizeof(gastn);
    //     stn = (gastn*)galloc(sz, "stn");
    //     if (stn == null)
    //     {
    //         _drawingContext.Logger?.LogInformation("Memory Allocation Error:  Station Request Block \n");
    //         return (null);
    //     }
    //
    //     stn.rnum = 0;
    //     stn.rpt = null;
    //     stn.pfi = pfi;
    //     stn.idim = pst.idim;
    //     stn.jdim = pst.jdim;
    //     stn.undef = pfi.undef;
    //     stn.tmin = dmin[3];
    //     stn.tmax = dmax[3];
    //     stn.ftmin = dmin[3];
    //     stn.ftmax = dmax[3];
    //     stn.pvar = pvar;
    //     for (i = 0; i < 3; i++)
    //     {
    //         stn.dmin[i] = dmin[i];
    //         stn.dmax[i] = dmax[i];
    //     }
    //
    //     stn.rflag = rflag;
    //     stn.radius = radius;
    //     stn.sflag = sflag;
    //     if (sflag)
    //     {
    //         for (i = 0; i < 8; i++) stn.stid[i] = stid[i];
    //     }
    //
    //     sz = sizeof(double) * 8;
    //     stn.tvals = (double*)galloc(sz, "stntvals");
    //     if (stn.tvals == null)
    //     {
    //         gree(stn, "f170");
    //         _drawingContext.Logger?.LogInformation("Memory Allocation Error:  Station Request Block \n");
    //         return (null);
    //     }
    //
    //     for (i = 0; i < 8; i++) *(stn.tvals + i) = *(pfi.grvals[3] + i);
    //
    //     rc = gagstn(stn);
    //
    //     if (rc > 0)
    //     {
    //         snprintf(pout, 1255, "Data Request Error:  Variable is '%s'\n", vnam);
    //         _drawingContext.Logger?.LogInformation(pout);
    //         gree(stn, "f171");
    //         return (null);
    //     }
    //
    //     pst.result.stn = stn;
    //     pst.type = 0;
    //     return (ch);
    // }
}