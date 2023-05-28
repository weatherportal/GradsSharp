using System.Text;

namespace GradsSharp.Drawing.Grads;

internal class GScript
{
    private DrawingContext _drawingContext;

    public GScript(DrawingContext drawingContext)
    {
        _drawingContext = drawingContext;
    }

    static string rcdef = "rc              ";
    static string redef = "result          ";


    /*  Execute a script, from one or more files. 
    Beware: various levels of recursion are used.     */

    char* gsfile(string cmd, out int retc, int impp)
    {
        gsvar parg;
        gscmn pcmn;
        char* ch, *fname,*res;
        int len, rc, i;

        /* Point to (and delimit) the file name from the cmd line */

        while (*cmd == ' ') cmd++;
        fname = cmd;
        while (*cmd != ' ' && *cmd != '\0' && *cmd != '\n') cmd++;
        if (*cmd == '\n') *cmd = '\0';
        else if (*cmd == ' ')
        {
            *cmd = '\0';
            cmd++;
            while (*cmd == ' ') cmd++;
        }

        /* Allocate the common structure; this anchors most allocated
           memory for executing this script */

        pcmn = new gscmn();
        
        pcmn.ffdef = null;
        pcmn.lfdef = null;
        pcmn.frecd = null;
        pcmn.lrecd = null;
        pcmn.fvar = null;
        pcmn.ffnc = null;
        pcmn.iob = null;
        pcmn.gvar = null;
        pcmn.farg = null;
        pcmn.fname = fname; /* Don't free this later */
        pcmn.fprefix = null;
        pcmn.ppath = null;
        pcmn.rres = null;
        pcmn.gsfflg = 0; /* No dynamic functions by default.
                         The gsfallow function controls this. */
        res = null;

        /* Open, read, and scan the script file. */

        rc = gsgsfrd(pcmn, 0, fname);

        if (rc > 0)
        {
            gsfree(pcmn);
            if (rc == 9)
            {
                if (impp)
                {
                    /* This should be handled by caller -- fix later */
                    gaprnt(0, "Unknown command: ");
                    gaprnt(0, fname);
                    gaprnt(0, "\n");
                }
                else
                {
                    Console.WriteLine("Error opening script file: %s\n", fname);
                }
            }

            *retc = 1;
            return (null);
        }

        /* Get ready to start executing the script.
           Allocate a var block and provide arg string */

        parg = (gsvar *)malloc(sizeof(gsvar));
        if (parg == null)
        {
            Console.WriteLine("Memory allocation error:  Script variable buffering\n");
            goto retrn;
        }

        parg.forw = null;
        ch = cmd;
        len = 0;
        while (*(ch + len) != '\0' && *(ch + len) != '\n') len++;
        parg.strng = (char*)malloc(len + 1);
        if (parg.strng == null)
        {
            Console.WriteLine("Memory allocation error:  Script variable buffering\n");
            free(parg);
            goto retrn;
        }

        for (i = 0; i < len; i++) *(parg.strng + i) = *(ch + i);
        *(parg.strng + len) = '\0';
        pcmn.farg = parg;

        /* Execute the main function. */

        rc = gsrunf(pcmn.frecd, pcmn);
        res = pcmn.rres;
        if (rc == 999) rc = -1;

        /*  We are done.  Return.  */

        retrn:
        gsfree(pcmn);
        *retc = rc;
        return (res);
    }

/* Free gscmn and associated storage */

    void gsfree(gscmn *pcmn) {
        gsfdef* pfdf,  *tfdf;
        gsrecd* precd,  *trecd;
        gsfnc* pfnc,  *tfnc;
        gsiob* piob,  *tiob;

        pfdf = pcmn.ffdef;
        while (pfdf)
        {
            tfdf = pfdf.forw;
            if (pfdf.name) free(pfdf.name);
            if (pfdf.file) free(pfdf.file);
            free(pfdf);
            pfdf = tfdf;
        }

        gsfrev(pcmn.gvar);
        gsfrev(pcmn.fvar);
        precd = pcmn.frecd;
        while (precd)
        {
            trecd = precd.forw;
            free(precd);
            precd = trecd;
        }

        pfnc = pcmn.ffnc;
        while (pfnc)
        {
            tfnc = pfnc.forw;
            free(pfnc);
            pfnc = tfnc;
        }

        piob = pcmn.iob;
        while (piob)
        {
            fclose(piob.file);
            free(piob.name);
            tiob = piob.forw;
            free(piob);
            piob = tiob;
        }

        if (pcmn.fprefix) free(pcmn.fprefix);
        if (pcmn.ppath) free(pcmn.ppath);
        free(pcmn);
    }

/* Read in the main script or a script function (.gsf)
   and scan the contents, adding to the chain of 
   recd descriptors if appropriate.  
   When lflag is zero we are reading the main script; 
   when 1 we are handling a .gsf file (and the name of
   the function is provided as pfnc.

   return codes:  0:  normal
                  1:  error; message already printed
                  9:  couldn't open file; message not yet printed */

    int gsgsfrd(gscmn pcmn, int lflag, char* pfnc) {
        gsfdef pfdf,  tfdf;
        gsrecd rectmp,  *reccur = null;
        int fpos;
        Stream ifile;
        string sfile;
        int rc, flen, len, reccnt, first;

        /* First allocate a gsfdef file, and chain it off of
           gscmn.  Gets freed at end of script execution; we are
           careful to set nullS so things are freed properly
           if an error occurs and execution falls thru. */

        pfdf = new gsfdef();

        if (pcmn.ffdef == null) pcmn.ffdef = new List<gsfdef>() { pfdf };
        else
        {
            pcmn.ffdef.Add(pfdf);
            
        }

        pfdf.name = null;
        pfdf.file = null;

        pcmn.lfdef = pfdf;

        /* Open the file */

        if (lflag == 0)
        {
            ifile = gsonam(pcmn, pfdf);
        }
        else
        {
            ifile = gsogsf(pcmn, pfdf, pfnc);
        }

        if (ifile == null) return (9);

        /* Read in the file */

        flen = (int)ifile.Length;

        sfile = new StreamReader(ifile).ReadToEnd();
        ifile.Close();
        
        /* Remove cr for PC version */

        fpos = sfile.LastIndexOf("\n");
        if (fpos > 0)
        {
            sfile = sfile.Substring(0, fpos);
        }

        sfile = sfile.Replace('\r', ' ');
        flen = sfile.Length;
        /* Above for pc version */

        pfdf.file = sfile;

        /* Build link list of record descriptor blocks.
           Append to existing list if handling a .gsf */

        first = 1;
        fpos = 0;
        reccnt = 1;
        while (fpos < flen)
        {
            rectmp = gsrtyp(&fpos, &reccnt, &rc);
            if (rc > 0) return (1);
            if (rectmp != null)
            {
                if (pcmn.frecd == null)
                {
                    pcmn.frecd = rectmp;
                    pfdf.precd = rectmp;
                    reccur = rectmp;
                    first = 0;
                }
                else
                {
                    if (first)
                    {
                        reccur = pcmn.lrecd;
                        pfdf.precd = rectmp;
                        first = 0;
                    }

                    reccur.forw = rectmp;
                    reccur = rectmp;
                }

                reccur.forw = null;
                reccur.pfdf = pfdf;
            }
        }

        pcmn.lrecd = reccur;

        /* Resolve flow-control blocks */

        rc = gsblck(pfdf.precd, pcmn);
        if (rc > 0) return (1);

        return (0);
    }

/* Determine what kind of record in the script file we have,
   and fill in a record descriptor block.                  */

    gsrecd gsrtyp(char** ppos, int* reccnt, int* rc)
    {
        char* fpos, *pos;
        gsrecd* recd;
        char ch[20];
        int i, eflg, cflg;

        /* Ignore comments */

        fpos = *ppos;
        if (*fpos == '*' || *fpos == '#')
        {
            while (*fpos != '\n') fpos++;
            fpos++;
            *ppos = fpos;
            *rc = 0;
            *reccnt = *reccnt + 1;
            return (null);
        }

        /* Ignore blank lines */

        while (*fpos == ' ') fpos++;
        if (*fpos == '\n' || *fpos == ';')
        {
            if (*fpos == '\n') *reccnt = *reccnt + 1;
            fpos++;
            *ppos = fpos;
            *rc = 0;
            return (null);
        }

        /* We found something, so allocate a descriptor block */

        recd = (gsrecd *)malloc(sizeof(gsrecd));
        if (recd == null)
        {
            Console.WriteLine("Memory allocation error: script scan\n");
            *rc = 1;
            return (null);
        }

        recd.forw = null;
        recd.pos = fpos;
        recd.num = *reccnt;
        recd.refer = null;

        /* Check for assignment statement first */

        eflg = 0;
        recd.epos = null;
        pos = fpos;
        recd.type = -9;
        if ((*pos >= 'a' && *pos <= 'z') || (*pos >= 'A' && *pos <= 'Z') || *pos == '_')
        {
            while ((*pos >= 'a' && *pos <= 'z') ||
                   (*pos >= 'A' && *pos <= 'Z') ||
                   (*pos == '.') || (*pos == '_') ||
                   (*pos >= '0' && *pos <= '9')) pos++;
            while (*pos == ' ') pos++;
            if (*pos == '=')
            {
                recd.type = 2;
                fpos = pos + 1;
                eflg = 1;
            }
        }

        /* Check for other keywords:  if, while, etc.  */

        if (recd.type != 2)
        {
            i = 0;
            while (*(fpos + i) != '\n' && *(fpos + i) != ';' && i < 9)
            {
                ch[i] = *(fpos + i);
                i++;
            }

            ch[i] = '\0';
            lowcas(ch);

            if (cmpwrd(ch, "if") || !cmpch(ch, "if(", 3))
            {
                fpos += 2;
                eflg = 1;
                recd.type = 7;
            }
            else if (cmpwrd(ch, "else"))
            {
                fpos += 4;
                recd.type = 8;
            }
            else if (cmpwrd(ch, "endif"))
            {
                fpos += 5;
                recd.type = 9;
            }
            else if (cmpwrd(ch, "while") || !cmpch(ch, "while(", 6))
            {
                fpos += 5;
                eflg = 1;
                recd.type = 3;
            }
            else if (cmpwrd(ch, "endwhile"))
            {
                fpos += 8;
                recd.type = 4;
            }
            else if (cmpwrd(ch, "continue"))
            {
                fpos += 8;
                recd.type = 5;
            }
            else if (cmpwrd(ch, "break"))
            {
                fpos += 5;
                recd.type = 6;
            }
            else if (cmpwrd(ch, "return") || !cmpch(ch, "return(", 7))
            {
                fpos += 6;
                recd.type = 10;
                eflg = 1;
            }
            else if (cmpwrd(ch, "function"))
            {
                fpos += 8;
                recd.type = 11;
                eflg = 1;
            }
            else if (cmpwrd(ch, "say"))
            {
                fpos += 3;
                recd.type = 12;
                eflg = 1;
            }
            else if (cmpwrd(ch, "print"))
            {
                fpos += 5;
                recd.type = 12;
                eflg = 1;
            }
            else if (cmpwrd(ch, "prompt"))
            {
                fpos += 6;
                recd.type = 15;
                eflg = 1;
            }
            else if (cmpwrd(ch, "pull"))
            {
                fpos += 4;
                recd.type = 13;
                eflg = 1;
            }
            else if (cmpwrd(ch, "exit"))
            {
                fpos += 4;
                recd.type = 14;
                eflg = 1;
            }
            else
            {
                recd.type = 1;
                recd.epos = fpos;
            }
        }

        /* Locate expression */

        if (eflg)
        {
            while (*fpos == ' ') fpos++;
            if (*fpos == '\n' || *fpos == ';')
            {
                recd.epos = null;
            }
            else recd.epos = fpos;
        }

        /* Advance to end of record */

        cflg = 0;
        while (1)
        {
            if (!cflg && *fpos == ';') break;
            if (*fpos == '\n') break;
            if (*fpos == '\'')
            {
                if (cflg == 1) cflg = 0;
                else if (cflg == 0) cflg = 1;
            }
            else if (*fpos == '\"')
            {
                if (cflg == 2) cflg = 0;
                else if (cflg == 0) cflg = 2;
            }

            fpos++;
        }

        /* Remove trailing blanks */

        pos = fpos - 1;
        while (*pos == ' ')
        {
            *pos = '\0';
            pos--;
        }

        /* Finish building rec block and return */

        if (*fpos == '\n') *reccnt = *reccnt + 1;
        *fpos = '\0';
        fpos++;
        *ppos = fpos;
        *rc = 0;
        return (recd);
    }

/*  Resolve flow-control blocks.  Scan each function
    seperately.                                      */

    int gsblck(gsrecd recd, gscmn pcmn) {
        gsfnc* pfnc, *prev,*cfnc;
        int rc, i;
        char* fch;

        /* Loop looking at statements.  If a function definition, allocate a
           function block and chain it.   */

        while (recd)
        {
            recd = gsbkst(recd, null, null, &rc);
            if (rc > 0) return (rc);

            /* If a function, allocate a function block */

            if (recd != null && recd.type == 11)
            {
                pfnc = (gsfnc *)malloc(sizeof(gsfnc));
                if (pfnc == null)
                {
                    Console.WriteLine("Error allocating memory: script scan\n");
                    return (1);
                }

                /* Chain it */

                if (pcmn.ffnc == null)
                {
                    pcmn.ffnc = pfnc;
                }
                else
                {
                    cfnc = pcmn.ffnc;
                    while (cfnc)
                    {
                        prev = cfnc;
                        cfnc = cfnc.forw;
                    }

                    prev.forw = pfnc;
                }

                pfnc.forw = null;

                /* Fill it in */

                pfnc.recd = recd;
                for (i = 0; i < 16; i++) pfnc.name[i] = ' ';
                fch = recd.epos;
                if (fch == null) goto err;
                if ((*fch >= 'a' && *fch <= 'z') || (*fch >= 'A' && *fch <= 'Z'))
                {
                    i = 0;
                    while ((*fch >= 'a' && *fch <= 'z') ||
                           (*fch >= 'A' && *fch <= 'Z') ||
                           (*fch >= '0' && *fch <= '9') ||
                           *fch == '_')
                    {
                        if (i > 15)
                        {
                            Console.WriteLine("Function name too long\n");
                            goto err;
                        }

                        pfnc.name[i] = *fch;
                        fch++;
                        i++;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid function name\n");
                    goto err;
                }

                while (*fch == ' ') fch++;
                if (*fch == ';' || *fch == '\0') recd.epos = null;
                else recd.epos = fch;
                recd = recd.forw;
            }
        }

        return (0);

        err:
        Console.WriteLine("Error in %s: Invalid function statement", recd.pfdf.name);
        Console.WriteLine(" at line %i\n", recd.num);
        Console.WriteLine("  In file %s\n", recd.pfdf.name);
        return (1);
    }

/*  Figure out status of a statement.  Recursively resolve
    if/then/else and while/endwhile blocks.  Return pointer
    to next statement (unless a function statement). */

    gsrecd gsbkst(gsrecd recd, gsrecd ifblk, gsrecd doblk, out int rc) {
        int ret;

        if (recd.type == 3)
        {
            recd = gsbkdo(recd.forw, null, recd, &ret);
            if (ret)
            {
                *rc = ret;
                return (null);
            }
        }
        else if (recd.type == 4)
        {
            Console.WriteLine("Unexpected endwhile.  Incorrect loop nesting.\n");
            if (ifblk)
            {
                Console.WriteLine("  Expecting endif before endwhile for ");
                Console.WriteLine("if statement at line %i\n", ifblk.num);
            }

            Console.WriteLine("  Error occurred scanning line %i\n", recd.num);
            Console.WriteLine("  In file %s\n", recd.pfdf.name);
            *rc = 1;
            return (null);
        }
        else if (recd.type == 5)
        {
            if (doblk == null)
            {
                Console.WriteLine("Unexpected continue.  No associated while\n");
                Console.WriteLine("  Error occurred scanning line %i\n", recd.num);
                Console.WriteLine("  In file %s\n", recd.pfdf.name);
                Console.WriteLine("  Statement is ignored\n");
            }

            recd.refer = doblk;
        }
        else if (recd.type == 6)
        {
            if (doblk == null)
            {
                Console.WriteLine("Unexpected break.  No associated while\n");
                Console.WriteLine("  Error occurred scanning line %i\n", recd.num);
                Console.WriteLine("  In file %s\n", recd.pfdf.name);
                Console.WriteLine("  Statement is ignored\n");
            }

            recd.refer = doblk;
        }
        else if (recd.type == 7)
        {
            recd = gsbkif(recd.forw, recd, doblk, &ret);
            if (ret)
            {
                *rc = ret;
                return (null);
            }
        }
        else if (recd.type == 8)
        {
            Console.WriteLine("Unexpected else.  Incorrect if block nesting.\n");
            Console.WriteLine("  Error occurred scanning line %i\n", recd.num);
            Console.WriteLine("  In file %s\n", recd.pfdf.name);
            *rc = 1;
            return (null);
        }
        else if (recd.type == 9)
        {
            Console.WriteLine("Unexpected endif.  Incorrect if block nesting.\n");
            Console.WriteLine("  Error occurred scanning line %i\n", recd.num);
            Console.WriteLine("  In file %s\n", recd.pfdf.name);
            *rc = 1;
            return (null);
        }
        else if (recd.type == 11)
        {
            *rc = 0;
            return (recd);
        }

        *rc = 0;
        return (recd.forw);
    }

/* Resolve an while/endwhile block.  Recursively resolve any
   nested elements. */

    gsrecd gsbkdo(gsrecd recd, gsrecd ifblk, gsrecd doblk, out int rc) {
        int ret;

        ret = 0;
        while (recd != null && recd.type != 4 && recd.type != 11 && ret == 0)
        {
            recd = gsbkst(recd, ifblk, doblk, &ret);
        }

        if (ret == 0 && (recd == null || recd.type == 11))
        {
            Console.WriteLine("Unable to locate ENDWHILE statement");
            Console.WriteLine(" for the WHILE statement at line %i\n", doblk.num);
            Console.WriteLine("  In file %s\n", doblk.pfdf.name);
            *rc = 1;
            return (null);
        }

        *rc = ret;
        if (ret == 0)
        {
            recd.refer = doblk;
            doblk.refer = recd;
            return (recd);
        }
        else return (null);
    }

/*  Resolve if/else/endif block */

    gsrecd gsbkif(gsrecd recd, gsrecd ifblk, gsrecd doblk, out int rc) {
        int ret, eflg;
        gsrecd? elsblk = null;

        eflg = 0;
        ret = 0;
        while (recd != null && recd.type != 11 && recd.type != 9 && ret == 0)
        {
            if (recd.type == 8 && eflg == 0)
            {
                elsblk = recd;
                eflg = 1;
                recd = recd.forw;
            }
            else recd = gsbkst(recd, ifblk, doblk, &ret);
        }

        if (ret == 0 && (recd == null || recd.type == 11))
        {
            Console.WriteLine("Unable to locate ENDIF statement");
            Console.WriteLine(" for the IF statement at line %i\n", ifblk.num);
            Console.WriteLine("  In file %s\n", ifblk.pfdf.name);
            *rc = 1;
            return (null);
        }

        *rc = ret;
        if (ret == 0)
        {
            recd.refer = ifblk;
            if (eflg)
            {
                ifblk.refer = elsblk;
                elsblk.refer = recd;
            }
            else
            {
                ifblk.refer = recd;
            }

            return (recd);
        }
        else return (null);
    }

/* Execute the function pointed to by recd and with the
   arguments pointed to by farg in pcmn */

    int gsrunf(gsrecd recd, gscmn pcmn) {
        gsvar* fvar,  *tvar, *avar, *nvar, *svar;
        int i, ret, len;
        char fnm[20],*ch;

        svar = pcmn.fvar; /* Save caller's args  */
        avar = null; /* Create new arg list */

        /* First two variables in var list are rc and result */

        fvar = null;
        fvar = (gsvar *)malloc(sizeof(gsvar));
        if (fvar == null) goto merr;
        fvar.forw = null;
        for (i = 0; i < 16; i++) fvar.name[i] = *(rcdef + i);
        fvar.strng = (char*)malloc(1);
        if (fvar.strng == null) goto merr;
        *(fvar.strng) = '\0';

        tvar = (gsvar *)malloc(sizeof(gsvar));
        if (tvar == null) goto merr;
        tvar.forw = null;
        fvar.forw = tvar;
        for (i = 0; i < 16; i++) tvar.name[i] = *(redef + i);
        tvar.strng = (char*)malloc(1);
        if (tvar.strng == null) goto merr;
        *(tvar.strng) = '\0';

        /* If the recd is a function record, check the prototype
           list to assign variables.  Add these variables to
           the variable list */

        avar = pcmn.farg;
        if (recd.type == 11 && recd.epos)
        {
            ch = recd.epos;
            if (*ch != '(') goto argerr;
            ch++;
            while (1)
            {
                while (*ch == ' ') ch++;
                if (*ch == ')') break;
                if ((*ch >= 'a' && *ch <= 'z') || (*ch >= 'A' && *ch <= 'Z'))
                {
                    len = 0;
                    for (i = 0; i < 16; i++) fnm[i] = ' ';
                    while ((*ch >= 'a' && *ch <= 'z') ||
                           (*ch >= 'A' && *ch <= 'Z') ||
                           (*ch == '.') || (*ch == '_') ||
                           (*ch >= '0' && *ch <= '9'))
                    {
                        fnm[len] = *ch;
                        len++;
                        ch++;
                        if (len > 15) goto argerr;
                    }
                }
                else goto argerr;

                if (avar)
                {
                    nvar = avar;
                    avar = avar.forw;
                }
                else
                {
                    nvar = (gsvar *)malloc(sizeof(gsvar));
                    if (nvar == null) goto merr;
                    nvar.strng = (char*)malloc(len + 1);
                    if (nvar.strng == null)
                    {
                        free(nvar);
                        goto merr;
                    }

                    for (i = 0; i < len; i++) *(nvar.strng + i) = fnm[i];
                    *(nvar.strng + len) = '\0';
                }

                for (i = 0; i < 16; i++) nvar.name[i] = fnm[i];
                tvar.forw = nvar;
                nvar.forw = null;
                tvar = nvar;
                while (*ch == ' ') ch++;
                if (*ch == ')') break;
                if (*ch == ',') ch++;
            }
        }

        /* If the calling arg list was too long, discard the
           unused var blocks */

        gsfrev(avar);

        /* Execute commands until we are done.  Flow control is
           handled recursively. */

        pcmn.fvar = fvar;
        pcmn.rc = 0;
        ret = 0;
        if (recd.type == 11) recd = recd.forw;
        while (recd && ret == 0)
        {
            recd = gsruns(recd, pcmn, &ret);
        }

        if (ret == 1 || ret == 2)
        {
            Console.WriteLine("Error in gsrunf:  Internal Logic Check 8\n");
            ret = 1;
        }
        else if (ret == 3) ret = 0;

        gsfrev(fvar);
        pcmn.fvar = svar; /* Restore caller's arg list */
        return (ret);

        merr:

        Console.WriteLine("Error allocating variable memory\n");
        gsfrev(fvar);
        gsfrev(avar);
        pcmn.fvar = svar;
        return (99);

        argerr:

        Console.WriteLine("Error:  Invalid function list\n");
        Console.WriteLine("  Error occurred on line %i\n", recd.num);
        Console.WriteLine("  In file %s\n", recd.pfdf.name);
        gsfrev(fvar);
        gsfrev(avar);
        pcmn.fvar = svar;
        return (99);
    }

/* Free a link list of variable blocks */

    // void gsfrev(gsvar var) {
    //     gsvar nvar;
    //
    //     while (var!=null)
    //     {
    //         nvar = var.forw;
    //         if (var.strng!=null) var.strng);
    //         free(var);
    //         var = nvar;
    //     }
    // }

/* Execute a statement in the scripting language */

    gsrecd gsruns(gsrecd recd, gscmn pcmn, out int rc) {
        int ret, ntyp;
        int lv;
        double vv;
        char* res;

        if (gaqsig())
        {
            *rc = 99;
            return (null);
        }

        /* Statement */

        if (recd.type == 1)
        {
            *rc = gsstmt(recd, pcmn);
            return (recd.forw);
        }
        /* Assignment */

        else if (recd.type == 2)
        {
            *rc = gsassn(recd, pcmn);
            return (recd.forw);
        }
        /* While */

        else if (recd.type == 3)
        {
            recd = gsrund(recd, pcmn, &ret);
            *rc = ret;
            return (recd);
        }
        /* Endwhile */

        else if (recd.type == 4)
        {
            Console.WriteLine("Error in gsruns:  Internal Logic Check 8\n");
            *rc = 99;
            return (null);
        }
        /* Continue */

        else if (recd.type == 5)
        {
            if (recd.refer)
            {
                *rc = 1;
                return (null);
            }
        }
        /* Break */

        else if (recd.type == 6)
        {
            if (recd.refer)
            {
                *rc = 2;
                return (null);
            }
        }
        /* If */

        else if (recd.type == 7)
        {
            recd = gsruni(recd, pcmn, &ret);
            *rc = ret;
            return (recd);
        }
        /* Else */

        else if (recd.type == 8)
        {
            Console.WriteLine("Error in gsruns:  Internal Logic Check 12\n");
            *rc = 99;
            return (null);
        }
        /* Endif */

        else if (recd.type == 9)
        {
            Console.WriteLine("Error in gsruns:  Internal Logic Check 16\n");
            *rc = 99;
            return (null);
        }
        /* Return */

        else if (recd.type == 10)
        {
            if (recd.epos)
            {
                pcmn.rres = gsexpr(recd.epos, pcmn);
                if (pcmn.rres == null)
                {
                    Console.WriteLine("  Error occurred on line %i\n", recd.num);
                    Console.WriteLine("  In file %s\n", recd.pfdf.name);
                    *rc = 99;
                    return (null);
                }
            }
            else pcmn.rres = null;

            *rc = 3;
            return (null);
        }
        /* Function statement (ie, implied return) */

        else if (recd.type == 11)
        {
            pcmn.rres = null;
            *rc = 3;
            return (null);
        }
        /* 'say' command */

        else if (recd.type == 12 || recd.type == 15)
        {
            if (recd.epos) res = gsexpr(recd.epos, pcmn);
            else
            {
                Console.WriteLine("\n");
                *rc = 0;
                return (recd.forw);
            }

            if (res == null)
            {
                Console.WriteLine("Error occurred on line %i\n", recd.num);
                Console.WriteLine("  In file %s\n", recd.pfdf.name);
                *rc = 99;
                return (null);
            }

            if (recd.type == 12) Console.WriteLine("%s\n", res);
            else Console.WriteLine("%s", res);
            free(res);
            return (recd.forw);
        }
        /* Pull command */

        else if (recd.type == 13)
        {
            *rc = gsassn(recd, pcmn);
            return (recd.forw);
        }
        /* Exit command */

        else if (recd.type == 14)
        {
            if (recd.epos)
            {
                res = gsexpr(recd.epos, pcmn);
                if (res == null)
                {
                    Console.WriteLine("  Error occurred on line %i\n", recd.num);
                    Console.WriteLine("  In file %s\n", recd.pfdf.name);
                    *rc = 99;
                    return (null);
                }

                gsnum(res, &ntyp, &lv, &vv);
                if (ntyp != 1)
                {
                    Console.WriteLine("Error on Exit Command:  Non Integer Argument\n");
                    Console.WriteLine("  Error occurred on line %i\n", recd.num);
                    Console.WriteLine("  In file %s\n", recd.pfdf.name);
                    *rc = 99;
                    return (null);
                }

                pcmn.rc = lv;
                free(res);
            }
            else
            {
                pcmn.rc = 0;
            }

            *rc = 4;
            return (null);
        }
        /* Anything else? */

        else
        {
            Console.WriteLine("Error in gsruns:  Internal Logic Check 16\n");
            *rc = 99;
            return (null);
        }

        return (null);
    }

/*  Execute a while loop */

    gsrecd gsrund(gsrecd recd, gscmn pcmn, out int rc) {
        gsrecd* dorec;
        int ret;
        char* rslt;


        rslt = gsexpr(recd.epos, pcmn);
        if (rslt == null)
        {
            Console.WriteLine("  Error occurred on line %i\n", recd.num);
            Console.WriteLine("  In file %s\n", recd.pfdf.name);
            *rc = 99;
            return (null);
        }

        dorec = recd;

        ret = 0;
        while (*rslt != '0' || *(rslt + 1) != '\0')
        {
            recd = dorec.forw;
            ret = 0;
            while (ret == 0 && recd.type != 4)
            {
                recd = gsruns(recd, pcmn, &ret);
            }

            if (ret > 1) break;
            free(rslt);
            rslt = gsexpr(dorec.epos, pcmn);
            if (rslt == null)
            {
                Console.WriteLine("  Error occurred on line %i\n", recd.num);
                Console.WriteLine("  In file %s\n", recd.pfdf.name);
                *rc = 99;
                return (null);
            }
        }

        free(rslt);
        if (ret < 3) ret = 0;
        *rc = ret;
        recd = dorec.refer;
        return (recd.forw);
    }

/*  Execute an if block */

    gsrecd gsruni(gsrecd recd, gscmn pcmn, out int rc) {
        int ret;
        char* rslt;

        rslt = gsexpr(recd.epos, pcmn);
        if (rslt == null)
        {
            Console.WriteLine("  Error occurred on line %i\n", recd.num);
            Console.WriteLine("  In file %s\n", recd.pfdf.name);
            *rc = 99;
            return (null);
        }

        if (*rslt == '0' && *(rslt + 1) == '\0') recd = recd.refer;
        free(rslt);

        if (recd.type != 9)
        {
            recd = recd.forw;
            ret = 0;
            while (ret == 0 && recd.type != 8 && recd.type != 9)
            {
                recd = gsruns(recd, pcmn, &ret);
            }

            if (ret)
            {
                *rc = ret;
                return (null);
            }

            if (recd.type == 8) recd = recd.refer;
        }

        *rc = 0;
        return (recd.forw);
    }

/* Execute a statement that is to be passed to the program
   environment, and get a response back. */

    int gsstmt(gsrecd recd, gscmn pcmn) {
        gsvar? pvar;
        int rc;
        char* res,  *buf, *tmp;

        res = gsexpr(recd.epos, pcmn);
        if (res == null)
        {
            Console.WriteLine("  Error occurred on line %i\n", recd.num);
            Console.WriteLine("  In file %s\n", recd.pfdf.name);
            return (99);
        }

        /* Execute the command */

        buf = gagsdo(res, &rc);
        free(res);

        /* We want to reflect the quit command back to the scripting
           language so we really do quit.  */

        if (rc == -1)
        {
            if (buf) free(buf);
            return (999);
        }

        /* Put the return code and command response into the appropriate
           variables.  We ASSUME that rc and result are variables that
           are at the start of the link list. */

        pvar = pcmn.fvar;
        tmp = (char*)malloc(6);
        if (tmp == null)
        {
            Console.WriteLine("Memory allocation error\n");
            if (buf) free(buf);
            return (99);
        }

        snConsole.WriteLine(tmp, 5, "%i", rc);
        free(pvar.strng);
        pvar.strng = tmp;

        pvar = pvar.forw;
        if (buf == null)
        {
            tmp = (char*)malloc(1);
            if (tmp == null)
            {
                Console.WriteLine("Memory allocation error\n");
                return (99);
            }

            *tmp = '\0';
        }
        else tmp = buf;

        free(pvar.strng);
        pvar.strng = tmp;
        return (0);
    }

/* Execute an assignment or pull command*/

    int gsassn(gsrecd recd, gscmn pcmn) {
        gsvar? var,  pvar = null;
        int rc, i, flg;
        char* res,  *pos;
        char varnm[16];

        /* Evaluate expression or read user input */

        if (recd.type == 13)
        {
            res = (char*)malloc(RSIZ);
            if (res == null)
            {
                Console.WriteLine("Memory allocation Error\n");
                return (99);
            }

            for (i = 0; i < 10; i++) *(res + i) = '\0';
            fgets(res, 512, stdin);
            /* Replace newline character or return character at end of user input string with null */
            for (i = 0; i < 512; i++)
            {
/*       if (*(res+i) == '\n') *(res+i)='\0';  */
                if ((*(res + i) == '\n') || (*(res + i) == '\r')) *(res + i) = '\0';
            }
        }
        else
        {
            res = gsexpr(recd.epos, pcmn);
            if (res == null)
            {
                Console.WriteLine("  Error occurred on line %i\n", recd.num);
                Console.WriteLine("  In file %s\n", recd.pfdf.name);
                return (99);
            }
        }

        /* Get variable name */

        for (i = 0; i < 16; i++) varnm[i] = ' ';
        if (recd.type == 13) pos = recd.epos;
        else pos = recd.pos;
        i = 0;
        while (*pos != ' ' && *pos != '=' && i < 16 && *pos != '\0')
        {
            varnm[i] = *pos;
            pos++;
            i++;
        }

        /* Resolve possible compound name. */

        rc = gsrvar(pcmn, varnm, varnm);
        if (rc > 0)
        {
            Console.WriteLine("  Error occurred on line %i\n", recd.num);
            Console.WriteLine("  In file %s\n", recd.pfdf.name);
            return (99);
        }

        /* See if this variable name already exists */

        if (varnm[0] == '_') var = pcmn.gvar;
        else var = pcmn.fvar;
        if (var == null) flg = 1;
        else flg = 0;
        while (var)
        {
            for (i = 0; i < 16; i++)
            {
                if (varnm[i] != var.name[i]) break;
            }

            if (i == 16) break;
            pvar = var;
            var = var.forw;
        }

        /* If it didn't, create it.  If it did, release old value */

        if (var == null)
        {
            var = (gsvar *)malloc(sizeof(gsvar));
            if (var == null)
            {
                Console.WriteLine("Error allocating memory for variable\n");
                return (99);
            }

            if (flg)
            {
                if (varnm[0] == '_') pcmn.gvar = var;
                else pcmn.fvar = var;
            }
            else pvar.forw = var;

            var.forw = null;
            for (i = 0; i < 16; i++) var.name[i] = varnm[i];
        }
        else
        {
            free(var.strng);
        }

        /* Assign new value */

        var.strng = res;

        return (0);
    }

/* Dump stack.  Any member of the list may be passed */

    void stkdmp(stck? stack) {
        while (stack.pback!=null) stack = stack.pback;
        while (stack!=null)
        {
            if (stack.type == 0)
            {
                Console.WriteLine("Operand: %s\n", stack.obj.strng);
            }
            else if (stack.type == 1)
            {
                Console.WriteLine("Operator: %i \n", stack.obj.op);
            }
            else if (stack.type == 2) Console.WriteLine("Left paren '('\n");
            else if (stack.type == 3) Console.WriteLine("Right paren ')'\n");
            else Console.WriteLine("Type = %i \n", stack.type);

            stack = stack.pforw;
        }
    }

/* Evaluate an expression in the GrADS scripting language.
   The expression must be null terminated.  The result string
   is returned, or if an error occurs, null is returned.    */

    string gsexpr(string expr,  gscmn pcmn) {
        stck? curr,  snew,  sold;
        char* pos;
        int state, uflag, i, flag;

        /* First element on stack is artificial left paren.  We
           will match with artificial right paren at end of expr
           to force final expression evaluation.  */

        curr = (stck *)malloc(sizeof(stck));
        if (curr == null) goto err2;
        curr.pback = null;
        curr.pforw = null;
        curr.type = 2;

        /* Initial state */

        state = 1;
        uflag = 0;
        pos = expr;

        /* Loop while parsing expression.  Each loop iteration deals with
           the next element of the expression.  Each expression element
           is pushed onto the expression stack.  When a right paren is
           encountered, the stack is evaluated back to the matching left
           paren, with the intermediate result restacked.                 */

        while (1)
        {
            /* Allocate next link list item so its ready when we need it  */

            snew = (stck *)malloc(sizeof(stck));
            if (snew == null) goto err2;
            curr.pforw = snew;
            sold = curr;
            curr = snew;
            curr.pforw = null;
            curr.pback = sold;
            curr.type = -1;

            /* Advance past any imbedded blanks */

            while (*pos == ' ') pos++;

            /* End of expr?  If so, leave loop.  */

            if (*pos == '\0') break;

            /*  The state flag determines what is expected next in the
                expression.  After an operand, we would expect an operator,
                for example -- or a ')'.  And after an operator, we would
                expect an operand, among other things.                      */

            if (state)
            {
                /* Expect oprnd, unary op, '(' */

                /*  Handle a left paren. */

                if (*pos == '(')
                {
                    curr.type = 2;
                    pos++;
                    uflag = 0;
                }
                /* Unary minus */

                else if (*pos == '-')
                {
                    if (uflag) goto err1;
                    curr.type = 1;
                    curr.obj.op = 15;
                    pos++;
                    uflag = 1;
                }
                /* Unary not */

                else if (*pos == '!')
                {
                    if (uflag) goto err1;
                    curr.type = 1;
                    curr.obj.op = 14;
                    pos++;
                    uflag = 1;
                }
                /*  Handle a constant   */

                else if (*pos == '\"' || *pos == '\'' ||
                         (*pos >= '0' && *pos <= '9'))
                {
                    curr.type = 0;
                    curr.obj.strng = gscnst(&pos);
                    if (curr.obj.strng == null) goto err3;
                    state = 0;
                    uflag = 0;
                }
                /*  Handle a variable or function call */

                else if ((*pos >= 'a' && *pos <= 'z') ||
                         (*pos >= 'A' && *pos <= 'Z') ||
                         (*pos == '_'))
                {
                    curr.type = 0;
                    curr.obj.strng = gsgopd(&pos, pcmn);
                    if (curr.obj.strng == null) goto err3;
                    state = 0;
                    uflag = 0;
                }
                /*  Anything else is an error.  */

                else
                {
                    goto err1;
                }
            }
            else
            {
                /* Expect operator or ')'      */

                uflag = 0;

                /*  Handle right paren.   */

                if (*pos == ')')
                {
                    curr.type = 3;
                    pos++;
                    snew = gseval(curr);
                    if (snew == null) goto err3;
                    curr = snew;
                }
                /*  Handle implied concatenation - check for operand */

                else if (*pos == '\"' || *pos == '\'' || *pos == '_' ||
                         (*pos >= '0' && *pos <= '9') ||
                         (*pos >= 'a' && *pos <= 'z') ||
                         (*pos >= 'A' && *pos <= 'Z'))
                {
                    curr.type = 1;
                    curr.obj.op = 9;
                    state = 1;
                }
                /*  Handle operator   */

                else
                {
                    flag = -1;
                    for (i = 0; i < 13; i++)
                    {
                        if (*pos != *(opchars[i])) continue;
                        if (*(opchars[i] + 1) && (*(pos + 1) != *(opchars[i] + 1))) continue;
                        flag = opvals[i];
                        break;
                    }

                    if (flag < 0) goto err1;
                    curr.type = 1;
                    curr.obj.op = flag;
                    state = 1;
                    if (i < 3) pos += 2;
                    else pos++;
                }
            }
        }

        /*  We get here when the end of the expression is reached.
            If the last thing stacked wasn't an operand or a closing
            paren, then an error.    */

        if (sold.type != 0 && sold.type != 3) goto err1;

        /*  Put an artificial right paren at the end of the stack
            (to match the artificial opening paren), then do a
            final evaluation of the stack.  If the result doesn't
            resolve to one operand, then unmatched parens or something */

        curr.type = 3;
/*
  stkdmp(curr);
*/
        snew = gseval(curr);
        if (snew == null) goto err3;
        curr = snew;
        if (curr.pback != null) goto err4;
        if (curr.pforw != null) goto err4;
/*
  stkdmp (curr);
*/

        /*  The expression has been evaluated without error.
            Free the last stack entry and return the result.     */

        pos = curr.obj.strng;
        free(curr);
        return (pos);

        /* Handle errors.  Issue error messages, free stack and
           associated memory.  */

        err1:

        Console.WriteLine("Syntax Error\n");
        goto err3;

        err2:

        Console.WriteLine("Memory Allocation Error\n");
        goto err3;

        err4:

        Console.WriteLine("Unmatched parens\n");
        goto err3;

        err3:

        while (curr.pback) curr = curr.pback;
        while (curr != null)
        {
            if (curr.type == 0) free(curr.obj.strng);
            sold = curr;
            curr = curr.pforw;
            free(sold);
        }

        return (null);
    }

/*  Evaluate the stack between opening and closing parentheses.
    This is done by making multiple passes at decreasing
    precedence levels, and evaluating all the operators at that
    precedence level.  When the final result is obtained, it is
    placed on the end of the stack without the parens.           */

    stck? gseval(stck? curr) {
        stck* sbeg,  *srch, *stmp;
        int i;

        /* Locate matching left paren. */

        sbeg = curr;
        while (sbeg)
        {
            if (sbeg.type == 2) break;
            sbeg = sbeg.pback;
        }

        if (sbeg == null)
        {
            Console.WriteLine("Unmatched parens\n");
            return (null);
        }

        /* Make a pass between the parens at each precedence level.  */

        for (i = 0; i < 7; i++)
        {
/*
    stkdmp(sbeg);
*/
            srch = sbeg;
            while (srch != curr)
            {
                if (srch.type == 1 &&
                    srch.obj.op >= opmins[i] && srch.obj.op <= opmaxs[i])
                {
                    srch = gsoper(srch);
                    if (srch == null) return (null);
                }

                srch = srch.pforw;
            }
        }

        /* Make sure we are down to one result.  If not, we are in
           deep doodoo */

        srch = sbeg.pforw;
        srch = srch.pforw;
        if (srch != curr)
        {
            Console.WriteLine("Logic error 8 in gseval \n");
            return (null);
        }

        /* Remove the parens from the linklist */

        srch = sbeg.pforw;
        srch.pforw = curr.pforw;
        srch.pback = sbeg.pback;
        stmp = sbeg.pback;
        if (stmp) stmp.pforw = srch;
        stmp = curr.pforw;
        if (stmp) stmp.pback = srch;
        free(sbeg);
        free(curr);

        return (srch);
    }

/* Perform an operation.  Unstack the operator and operands,
   and stack the result in their place.  Return a pointer to
   the link list element representing the result.           */

    stck? gsoper(stck soper) {
        stck* sop1,  *sop2, *stmp;
        int op, ntyp1, ntyp2, ntype = 0, comp = 0, len;
        double v1, v2, v;
        int iv1, iv2, iv;
        char* s1,  *s2, *ch, *res, buf[25];

        /* Get pointers to the operands.  If a potentially numeric
           operation, do string to numeric conversion.             */

        op = soper.obj.op;
        sop1 = soper.pback;
        sop2 = soper.pforw;
        if (optyps[op - 1])
        {
            gsnum(sop2.obj.strng, &ntyp2, &iv2, out v2);
            if (op < 14) gsnum(sop1.obj.strng, &ntyp1, &iv1, &v1);
            else ntyp1 = ntyp2;
            if (ntyp1 == 1 && ntyp2 == 1) ntype = 1;
            else if (ntyp1 == 0 || ntyp2 == 0) ntype = 0;
            else ntype = 2;
        }

        /* If an op that requires numbers, check to make sure we
           can do it.  */

        if (optyps[op - 1] == 2 && ntype == 0)
        {
            Console.WriteLine("Non-numeric args to numeric operation\n");
            return (null);
        }

        /* Perform actual operations. */

        /* Logical or, and */

        if (op == 1 || op == 2)
        {
            s1 = sop1.obj.strng;
            s2 = sop2.obj.strng;
            res = malloc(2);
            if (res == null)
            {
                Console.WriteLine("Memory allocation error\n");
                return (null);
            }

            *(res + 1) = '\0';
            if (op == 1)
            {
                if ((*s1 == '0' && *(s1 + 1) == '\0') &&
                    (*s2 == '0' && *(s2 + 1) == '\0')) *res = '0';
                else *res = '1';
            }
            else
            {
                if ((*s1 == '0' && *(s1 + 1) == '\0') ||
                    (*s2 == '0' && *(s2 + 1) == '\0')) *res = '0';
                else *res = '1';
            }
        }
        /* Logical comparitive */

        else if (op > 2 && op < 9)
        {
            res = malloc(2);
            if (res == null)
            {
                Console.WriteLine("Memory allocation error\n");
                return (null);
            }

            *(res + 1) = '\0';

            /* Determine relationship between the ops */

            if (ntype == 2)
            {
                if (v1 < v2) comp = 1;
                else if (v1 == v2) comp = 3;
                else comp = 2;
            }
            else if (ntype == 1)
            {
                if (iv1 < iv2) comp = 1;
                else if (iv1 == iv2) comp = 3;
                else comp = 2;
            }
            else
            {
                s1 = sop1.obj.strng;
                s2 = sop2.obj.strng;
                while (*s1 && *s2)
                {
                    if (*s1 < *s2)
                    {
                        comp = 1;
                        break;
                    }

                    if (*s1 > *s2)
                    {
                        comp = 2;
                        break;
                    }

                    s1++;
                    s2++;
                }

                if (*s1 == '\0' && *s2 == '\0') comp = 3;
                else if (*s1 == '\0') comp = 1;
                else if (*s2 == '\0') comp = 2;
            }

            /* Apply relationship to specific op */

            if (op == 3)
            {
                if (comp == 3) *res = '1';
                else *res = '0';
            }
            else if (op == 4)
            {
                if (comp != 3) *res = '1';
                else *res = '0';
            }
            else if (op == 5)
            {
                if (comp == 2) *res = '1';
                else *res = '0';
            }
            else if (op == 6)
            {
                if (comp == 2 || comp == 3) *res = '1';
                else *res = '0';
            }
            else if (op == 7)
            {
                if (comp == 1) *res = '1';
                else *res = '0';
            }
            else
            {
                if (comp == 1 || comp == 3) *res = '1';
                else *res = '0';
            }
        }
        /* String concatenation */

        else if (op == 9)
        {
            s1 = sop1.obj.strng;
            s2 = sop2.obj.strng;
            len = strlen(s1) + strlen(s2);
            res = malloc(len + 1);
            if (res == null)
            {
                Console.WriteLine("Memory allocation error\n");
                return (null);
            }

            ch = res;
            while (*s1)
            {
                *ch = *s1;
                s1++;
                ch++;
            }

            while (*s2)
            {
                *ch = *s2;
                s2++;
                ch++;
            }

            *ch = '\0';
        }
        /*  Handle arithmetic operator */

        else if (op < 14 && op > 9)
        {
/*
    if (ntype==1) {
      if (op==10) iv = iv1+iv2;
      else if (op==11) iv = iv1-iv2;
      else if (op==12) iv = iv1*iv2;
      else {
        if (iv2==0) {
          Console.WriteLine ("Divide by zero\n");
          return (null);
        }
        iv = iv1 / iv2;
      }
      snConsole.WriteLine(buf,24,"%i",iv);
    } else {
      if (op==10) v = v1+v2;
      else if (op==11) v = v1-v2;
      else if (op==12) v = v1*v2;
      else {
        if (v2==0.0) {
          Console.WriteLine ("Divide by zero\n");
          return (null);
        }
        v = v1 / v2;
      }
      snConsole.WriteLine(buf,24,"%.15g",v);
    }
*/
            if (op == 10) v = v1 + v2;
            else if (op == 11) v = v1 - v2;
            else if (op == 12) v = v1 * v2;
            else
            {
                if (v2 == 0.0)
                {
                    Console.WriteLine("Divide by zero\n");
                    return (null);
                }

                v = v1 / v2;
            }

            snConsole.WriteLine(buf, 24, "%.15g", v);
/**/
            len = strlen(buf) + 1;
            res = malloc(len);
            if (res == null)
            {
                Console.WriteLine("Memory allocation error\n");
                return (null);
            }

            strcpy(res, buf);
        }
        /*  Do unary not operation */

        else if (op == 14)
        {
            res = malloc(2);
            if (res == null)
            {
                Console.WriteLine("Memory allocation error\n");
                return (null);
            }

            *(res + 1) = '\0';
            s2 = sop2.obj.strng;
            if (*s2 == '\0' || (*s2 == '0' && *(s2 + 1) == '\0')) *res = '1';
            else *res = '0';
        }
        /* Do unary minus operation */

        else if (op == 15)
        {
            if (ntype == 1)
            {
                iv = -1 * iv2;
                snConsole.WriteLine(buf, 24, "%i", iv);
            }
            else
            {
                v = -1.0 * v2;
                snConsole.WriteLine(buf, 24, "%.15g", v);
            }

            len = strlen(buf) + 1;
            res = malloc(len);
            if (res == null)
            {
                Console.WriteLine("Memory allocation error\n");
                return (null);
            }

            strcpy(res, buf);
        }

        else
        {
            Console.WriteLine("Logic error 12 in gsoper\n");
            return (null);
        }

        /* Rechain, Free stuff and return */

        free(sop2.obj.strng);
        if (op < 14) free(sop1.obj.strng);
        sop2.obj.strng = res;
        if (op < 14)
        {
            sop2.pback = sop1.pback;
            stmp = sop1.pback;
            stmp.pforw = sop2;
            free(sop1);
        }
        else
        {
            sop2.pback = soper.pback;
            stmp = soper.pback;
            stmp.pforw = sop2;
        }

        free(soper);
        return (sop2);
    }

/* Obtain the value of an operand.  This may be either a
   variable or a function.                                */

    char* gsgopd(char** ppos,  gscmn pcmn) {
        char* pos,  *res;
        char name[16];
        int i, pflag;

        pos = *ppos;
        for (i = 0; i < 16; i++) name[i] = ' ';
        i = 0;
        pflag = 0;
        while ((*pos >= 'a' && *pos <= 'z') ||
               (*pos >= 'A' && *pos <= 'Z') ||
               (*pos == '.') || (*pos == '_') ||
               (*pos >= '0' && *pos <= '9'))
        {
            if (*pos == '.') pflag = 1;
            if (i > 15)
            {
                Console.WriteLine("Variable name too long - 1st 16 chars are: ");
                for (i = 0; i < 16; i++) Console.WriteLine("%c", name[i]);
                Console.WriteLine("\n");
                return (null);
            }

            name[i] = *pos;
            pos++;
            i++;
        }

        while (*pos == ' ') pos++;

        /* Handle a function call -- this is a recursive call all the
           way back to gsrunf.   */

        if (*pos == '(')
        {
            if (pflag)
            {
                Console.WriteLine("Invalid function name: ");
                for (i = 0; i < 16; i++) Console.WriteLine("%c", name[i]);
                Console.WriteLine("\n");
                return (null);
            }

            pos = gsfunc(pos, name, pcmn);
            if (pos == null) return (null);
            *ppos = pos;
            res = pcmn.rres;
            if (res == null)
            {
                res = (char*)malloc(1);
                if (res == null)
                {
                    Console.WriteLine("Memory allocation error\n");
                    return (null);
                }

                *res = '\0';
            }

            pcmn.rres = null;
            return (res);
        }

        *ppos = pos;
        res = gsfvar(name, pcmn);
        return (res);
    }

/* Call a function.  */

    char* gsfunc(char* pos, string name,  gscmn pcmn) {
        gsfnc* pfnc;
        gsvar* avar,  *nvar, *cvar = null;
        char* astr,  *res;
        int len, rc, i, cflg, pcnt;

        avar = null;

        /*  Get storage for holding argument expressions */

        len = 0;
        while (*(pos + len)) len++;
        astr = (char*)malloc(len);
        if (astr == null)
        {
            Console.WriteLine("Memory allocation error \n");
            return (null);
        }

        /*  Evaluate each argument found.  Allocate a gsvar block
            for each one, and chain them together */

        pos++;
        pcnt = 0;
        while (!(*pos == ')' && pcnt == 0))
        {
            cflg = 0;
            len = 0;
            while (*pos)
            {
                if (!cflg && (*pos == ',' || (*pos == ')' && pcnt == 0))) break;
                if (!cflg)
                {
                    if (*pos == '(') pcnt++;
                    if (*pos == ')') pcnt--;
                    if (pcnt < 0) break;
                }

                if (*pos == '\'')
                {
                    if (cflg == 1) cflg = 0;
                    else if (cflg == 0) cflg = 1;
                }
                else if (*pos == '\"')
                {
                    if (cflg == 2) cflg = 0;
                    else if (cflg == 0) cflg = 2;
                }

                *(astr + len) = *pos;
                pos++;
                len++;
            }

            if (*pos == '\0')
            {
                Console.WriteLine("Unmatched parens on function call\n");
                pos = null;
                goto retrn;
            }

            *(astr + len) = '\0';
            res = gsexpr(astr, pcmn);
            if (res == null)
            {
                Console.WriteLine("Error occurred processing function arguments\n");
                pos = null;
                goto retrn;
            }

            nvar = (gsvar *)malloc(sizeof(gsvar));
            if (nvar == null)
            {
                Console.WriteLine("Memory allocation error\n");
                pos = null;
                goto retrn;
            }

            nvar.strng = res;
            if (avar == null) avar = nvar;
            else cvar.forw = nvar;
            cvar = nvar;
            cvar.forw = null;
            if (*pos == ',') pos++;
        }

        pos++;

        /*  We are all set up to invoke the function.  So now we need
            to find the function.  Look for internal functions first */

        pcmn.farg = avar;
        if (cmpwrd(name, "substr")) rc = gsfsub(pcmn);
        else if (cmpwrd(name, "subwrd")) rc = gsfwrd(pcmn);
        else if (cmpwrd(name, "sublin")) rc = gsflin(pcmn);
        else if (cmpwrd(name, "wrdpos")) rc = gsfpwd(pcmn);
        else if (cmpwrd(name, "strlen")) rc = gsfsln(pcmn);
        else if (cmpwrd(name, "valnum")) rc = gsfval(pcmn);
        else if (cmpwrd(name, "read")) rc = gsfrd(pcmn);
        else if (cmpwrd(name, "write")) rc = gsfwt(pcmn);
        else if (cmpwrd(name, "close")) rc = gsfcl(pcmn);
        else if (cmpwrd(name, "sys")) rc = gsfsys(pcmn);
        else if (cmpwrd(name, "gsfallow")) rc = gsfallw(pcmn);
        else if (cmpwrd(name, "gsfpath")) rc = gsfpath(pcmn);
        else if (cmpwrd(name, "math_log")) rc = gsfmath(pcmn, 1);
        else if (cmpwrd(name, "math_log10")) rc = gsfmath(pcmn, 2);
        else if (cmpwrd(name, "math_cos")) rc = gsfmath(pcmn, 3);
        else if (cmpwrd(name, "math_sin")) rc = gsfmath(pcmn, 4);
        else if (cmpwrd(name, "math_tan")) rc = gsfmath(pcmn, 5);
        else if (cmpwrd(name, "math_atan")) rc = gsfmath(pcmn, 6);
        else if (cmpwrd(name, "math_atan2")) rc = gsfmath(pcmn, 7);
        else if (cmpwrd(name, "math_sqrt")) rc = gsfmath(pcmn, 8);
        else if (cmpwrd(name, "math_abs")) rc = gsfmath(pcmn, 9);
        else if (cmpwrd(name, "math_acosh")) rc = gsfmath(pcmn, 10);
        else if (cmpwrd(name, "math_asinh")) rc = gsfmath(pcmn, 11);
        else if (cmpwrd(name, "math_atanh")) rc = gsfmath(pcmn, 12);
        else if (cmpwrd(name, "math_cosh")) rc = gsfmath(pcmn, 13);
        else if (cmpwrd(name, "math_sinh")) rc = gsfmath(pcmn, 14);
        else if (cmpwrd(name, "math_exp")) rc = gsfmath(pcmn, 15);
        else if (cmpwrd(name, "math_fmod")) rc = gsfmath(pcmn, 16);
        else if (cmpwrd(name, "math_pow")) rc = gsfmath(pcmn, 17);
        else if (cmpwrd(name, "math_sinh")) rc = gsfmath(pcmn, 18);
        else if (cmpwrd(name, "math_tanh")) rc = gsfmath(pcmn, 19);
        else if (cmpwrd(name, "math_acos")) rc = gsfmath(pcmn, 20);
        else if (cmpwrd(name, "math_asin")) rc = gsfmath(pcmn, 21);
        else if (cmpwrd(name, "math_format")) rc = gsfmath(pcmn, 22);
        else if (cmpwrd(name, "math_nint")) rc = gsfmath(pcmn, 23);
        else if (cmpwrd(name, "math_int")) rc = gsfmath(pcmn, 24);
        else if (cmpwrd(name, "math_mod")) rc = gsfmath(pcmn, 25);
        else if (cmpwrd(name, "math_strlen")) rc = gsfmath(pcmn, 26);
        /*  Not an intrinsic function.  See if it is a function
            within the file we are currently working on.  */

        else
        {
            pfnc = pcmn.ffnc;
            while (pfnc)
            {
                if (!cmpch(pfnc.name, name, 16)) break;
                pfnc = pfnc.forw;
            }

            /* If not found, try to load it, assuming this is 
               currently allowed */

            if (pfnc == null && pcmn.gsfflg != 0)
            {
                rc = gsgsfrd(pcmn, 1, name); /* Load function file */
                if (rc == 0)
                {
                    /* Now look again */
                    pfnc = pcmn.ffnc;
                    while (pfnc)
                    {
                        if (!cmpch(pfnc.name, name, 16)) break;
                        pfnc = pfnc.forw;
                    }

                    if (pfnc == null)
                    {
                        Console.WriteLine("Loaded function file %s\n", pcmn.lfdef.name);
                        Console.WriteLine("  But... ");
                    }
                }
                else if (rc != 9)
                {
                    /* An error occurred */
                    Console.WriteLine("Error while loading function: ");
                    for (i = 0; i < 16; i++) Console.WriteLine("%c", name[i]);
                    Console.WriteLine("\n");
                    pos = null;
                    goto retrn;
                } /* File not found (rc==9) just */
            } /*   fall thru and give msg below */

            if (pfnc)
            {
                rc = gsrunf(pfnc.recd, pcmn);
            }
            else
            {
                Console.WriteLine("Function not found: ");
                for (i = 0; i < 16; i++) Console.WriteLine("%c", name[i]);
                Console.WriteLine("\n");
                pos = null;
                goto retrn;
            }
        }

        if (rc > 0) pos = null;
        avar = null;

        retrn:
        gsfrev(avar);
        free(astr);
        return (pos);
    }

/* Find the value of a variable */

    char* gsfvar(string iname,  gscmn pcmn) {
        gsvar* var;
        char* ch,  *src, name[16];
        int len, i;

        /* Resolve possible compound name. */

        i = gsrvar(pcmn, iname, name);
        if (i) return (null);

        /* See if this variable name already exists */

        if (name[0] == '_') var = pcmn.gvar;
        else var = pcmn.fvar;

        while (var)
        {
            for (i = 0; i < 16; i++)
            {
                if (name[i] != var.name[i]) break;
            }

            if (i == 16) break;
            var = var.forw;
        }

        /* If it didn't, use var name.  If it did, use current value */

        if (var == null)
        {
            len = 0;
            while (name[len] != ' ' && len < 16) len++;
            src = name;
        }
        else
        {
            len = 0;
            while (*(var.strng + len)) len++;
            src = var.strng;
        }

        ch = malloc(len + 1);
        if (ch == null)
        {
            Console.WriteLine("Error allocating memory for variable \n");
            return (null);
        }

        for (i = 0; i < len; i++) *(ch + i) = *(src + i);
        *(ch + len) = '\0';
        return (ch);
    }

/*  Resolve compound variable name */

    int gsrvar(gscmn pcmn, string name, string oname) {
        gsvar? var;
        char rname[16],sname[16];
        int len, pos, tpos, cnt, i;

        for (i = 0; i < 16; i++) rname[i] = ' ';
        pos = 0;
        len = 0;
        while (pos < 16)
        {
            if (len > 15)
            {
                if (*(name + pos) != ' ')
                {
                    Console.WriteLine("Compound variable name too long: ");
                    for (i = 0; i < 16; i++) Console.WriteLine("%c", *(name + i));
                    return (1);
                }

                break;
            }

            rname[len] = *(name + pos);
            len++;
            if (*(name + pos) == '.')
            {
                /* Split off sub name */
                pos++;
                cnt = 0;
                tpos = pos;
                for (i = 0; i < 16; i++) sname[i] = ' ';
                while (*(name + tpos) != '.' && *(name + tpos) != ' ' && tpos < 16)
                {
                    sname[cnt] = *(name + tpos);
                    tpos++;
                    cnt++;
                }

                if (cnt > 0)
                {
                    /* See if it's a var  */
                    if (*sname == '_') var = pcmn.gvar;
                    else var = pcmn.fvar;
                    while (var)
                    {
                        for (i = 0; i < 16; i++)
                        {
                            if (*(sname + i) != var.name[i]) break;
                        }

                        if (i == 16) break;
                        var = var.forw;
                    }

                    if (var != null)
                    {
                        /* If so, use value   */
                        cnt = 0;
                        while (len < 16 && *(var.strng + cnt) != '\0')
                        {
                            rname[len] = *(var.strng + cnt);
                            cnt++;
                            len++;
                        }

                        if (len == 16 && *(var.strng + cnt) == '\0')
                        {
                            Console.WriteLine("Compound variable name too long: ");
                            for (i = 0; i < 16; i++) Console.WriteLine("%c", *(name + i));
                            return (1);
                        }

                        pos = tpos; /* Advance pointer    */
                    }
                }
            }
            else pos++;
        }

        for (i = 0; i < 16; i++) *(oname + i) = rname[i];
/*
  for (i=0; i<16; i++) Console.WriteLine ("%c",*(oname+i));
  Console.WriteLine ("\n");
*/
        return (0);
    }


/*  Retreive a constant */

    char* gscnst(char** ppos)
    {
        char* pos,  *ch, *cpos, delim;
        int len, i, dflg, eflg;

        pos = *ppos;

        /* Handle integer constant */

        if (*pos >= '0' && *pos <= '9')
        {
            len = 0;
            dflg = 1;
            while ((*pos >= '0' && *pos <= '9') ||
                   (dflg && *pos == '.'))
            {
                if (*pos == '.') dflg = 0;
                pos++;
                len++;
            }

            eflg = 0;
            if ((*pos == 'e' || *pos == 'E'))
            {
                if (*(pos + 1) >= '0' && *(pos + 1) <= '9') eflg = 1;
                else if (*(pos + 1) == '-' || *(pos + 1) == '+')
                {
                    if (*(pos + 2) >= '0' && *(pos + 2) <= '9') eflg = 2;
                }
            }

            if (eflg)
            {
                pos += eflg; /* Skip past 'e' and exponent sign */
                len += eflg;
                while (*pos >= '0' && *pos <= '9')
                {
                    pos++;
                    len++;
                }
            }

            ch = malloc(len + 1);
            if (ch == null)
            {
                Console.WriteLine("Memory allocation error \n");
                return (null);
            }

            pos = *ppos;
            for (i = 0; i < len; i++) *(ch + i) = *(pos + i);
            *(ch + len) = '\0';
            *ppos = pos + len;
            return (ch);
        }

        /* Handle string constant */

        delim = *pos;
        len = 0;
        pos++;
        while (*pos)
        {
            if (*pos == delim && *(pos + 1) == delim)
            {
                len++;
                pos += 2;
                continue;
            }

            if (*pos == delim) break;
            len++;
            pos++;
        }

        if (*pos == '\0')
        {
            Console.WriteLine("Non-terminated constant\n");
            return (null);
        }

        ch = malloc(len + 1);
        if (ch == null)
        {
            Console.WriteLine("Memory allocation error \n");
            return (null);
        }

        pos = *ppos;
        cpos = ch;
        pos++;
        while (1)
        {
            if (*pos == delim && *(pos + 1) == delim)
            {
                *cpos = *pos;
                cpos++;
                pos += 2;
                continue;
            }

            if (*pos == delim) break;
            *cpos = *pos;
            pos++;
            cpos++;
        }

        *cpos = '\0';
        *ppos = pos + 1;
        return (ch);
    }

/* Determine if an operand is a numeric.  Numerics must not
   have any leading or trailing blanks, and must be
   either integer or floating values.  */

    void gsnum(string strng, out int type, out int ival, out double val)
    {
        char* ch;
        int dflg, eflg, len;

        ch = strng;
        len = 0;

        dflg = 0; /* we found a decimal point */
        eflg = 0; /* we found an exponent */

        if (*ch == '\0')
        {
            *type = 0;
            return;
        }

        if (*ch < '0' || *ch > '9')
        {
            if (*ch == '+' || *ch == '-')
            {
                ch++;
                len++;
                if (*ch == '.')
                {
                    dflg = 1;
                    ch++;
                    len++;
                }
            }
            else if (*ch == '.')
            {
                dflg = 1;
                ch++;
                len++;
            }
            else
            {
                *type = 0;
                return;
            }
        }

        if (*ch < '0' || *ch > '9')
        {
            /* should be a number at this point */
            *type = 0;
            return;
        }

        while (*ch)
        {
            if (*ch < '0' || *ch > '9')
            {
                if (*ch == '.')
                {
                    if (dflg) break;
                    dflg = 1;
                }
                else break;
            }

            ch++;
            len++;
        }

        if (*ch == 'E' || *ch == 'e')
        {
            eflg = 1;
            ch++;
            len++;
            if (*ch == '+' || *ch == '-')
            {
                ch++;
                len++;
            }

            if (*ch < '0' || *ch > '9')
            {
                *type = 0;
                return;
            }

            while (*ch >= '0' && *ch <= '9')
            {
                ch++;
                len++;
            }
        }

        if (*ch)
        {
            *type = 0;
            return;
        }

        if (!dflg && !eflg && len < 10)
        {
            *ival = atol(strng);
            *val = (double)(*ival);
            *type = 1;
        }
        else
        {
            *val = atof(strng);
            *type = 2;
        }

        return;
    }

/*  Intrinsic functions.  */

/* Sys function. Expects one arg, the command to execute.
   Uses popen to execute the command and read the stdout from it.  
   Note that popen has limitations. */

    int gsfsys(gscmn pcmn) {
        FILE* pipe;
        gsvar* pvar;
        char* cmd, *res,*buf;
        double v;
        int ret, len, siz, incr, pos, ntype;

        pcmn.rres = null;

        pvar = pcmn.farg;
        if (pvar == null)
        {
            Console.WriteLine("Error from script function: sys:  1st argument missing\n");
            ret = 1;
            goto retrn;
        }

        cmd = pvar.strng;

        incr = 1000;
        pvar = pvar.forw;
        if (pvar != null)
        {
            gsnum(pvar.strng, out ntype, &ret, &v);
            if (ntype != 1 || ret < 5000)
            {
                Console.WriteLine("Warning from script function: sys: 2nd arg invalid, ignored\n");
            }
            else incr = ret;
        }

        /* Call popen to execute the command. */

        pipe = popen(cmd, "r");
        if (pipe == null)
        {
            Console.WriteLine("Error from script function: sys:  popen error\n");
            ret = 1;
            goto retrn;
        }

        /* Allocate storage for the result and read the result */

        siz = incr;
        res = null;
        pos = 0;
        while (1)
        {
            buf = (char*)realloc(res, siz + 10);
            if (buf == null)
            {
                Console.WriteLine("Error from script function: sys:  Memory allocation error\n");
                Console.WriteLine("Error from script function: sys:  Attempted size %i\n", siz);
                free(res);
                pclose(pipe);
                ret = 1;
                goto retrn;
            }

            res = buf;
            len = fread(res + pos, sizeof(char), incr, pipe);
            pos += len;
            if (len < incr) break;
            if (siz > 10000 && incr < 5000) incr = 5000;
            if (siz > 100000 && incr < 10000) incr = 10000;
            if (siz > 1000000 && incr < 100000) incr = 100000;
            siz += incr;
        }

        *(res + pos) = '\0';
        pclose(pipe);

        ret = 0;
        pcmn.rres = res;

        /* Release arg storage and return */

        retrn:

        gsfrev(pcmn.farg);
        pcmn.farg = null;
        return (ret);
    }

/* Substring function.  Expects three args:  string, start, length */

    int gsfsub(gscmn pcmn) {
        gsvar* pvar;
        char* ch,  *res;
        int ret, ntype, strt, len, i;
        int lstrt, llen;
        double v;

        pcmn.rres = null;

        /* Attempt to convert 2nd and thrd args to integer */

        pvar = pcmn.farg;
        if (pvar == null)
        {
            Console.WriteLine("Error in substr:  1st argument missing\n");
            ret = 1;
            goto retrn;
        }

        pvar = pvar.forw;
        if (pvar == null)
        {
            Console.WriteLine("Error in substr:  2nd argument missing\n");
            ret = 1;
            goto retrn;
        }

        gsnum(pvar.strng, out ntype, &lstrt, &v);
        strt = lstrt;
        if (ntype != 1 || strt < 1)
        {
            Console.WriteLine("Error in substr:  2nd argument invalid.\n");
            ret = 1;
            goto retrn;
        }

        pvar = pvar.forw;
        if (pvar == null)
        {
            Console.WriteLine("Error in substr:  3rd argument missing\n");
            ret = 1;
            goto retrn;
        }

        gsnum(pvar.strng, out ntype, &llen, &v);
        len = llen;
        if (ntype != 1 || len < 1)
        {
            Console.WriteLine("Error in substr:  3rd argument invalid.\n");
            ret = 1;
            goto retrn;
        }

        /* Allocate storage for the result */

        res = (char*)malloc(len + 1);
        if (res == null)
        {
            Console.WriteLine("Error:  Storage allocation error\n");
            ret = 1;
            goto retrn;
        }

        /* Move the desired substring.  null return is possible. */

        pvar = pcmn.farg;
        i = 1;
        ch = pvar.strng;
        while (*ch && i < strt)
        {
            ch++;
            i++;
        } /* Don't start past end of string */

        i = 0;
        while (*ch && i < len)
        {
            *(res + i) = *ch;
            ch++;
            i++;
        }

        *(res + i) = '\0';

        ret = 0;
        pcmn.rres = res;

        /* Release arg storage and return */

        retrn:

        gsfrev(pcmn.farg);
        pcmn.farg = null;
        return (ret);
    }

/*  Routine to get specified word in a string */

    int gsfwrd(gscmn pcmn) {
        gsvar* pvar;
        char* ch,  *res;
        int ret, ntype, wnum, i, len;
        int lwnum;
        double v;

        pcmn.rres = null;

        /* Attempt to convert 2nd arg to integer. */

        pvar = pcmn.farg;
        if (pvar == null)
        {
            Console.WriteLine("Error in subwrd:  1st argument missing\n");
            ret = 1;
            goto retrn;
        }

        pvar = pvar.forw;
        if (pvar == null)
        {
            Console.WriteLine("Error in subwrd:  2nd argument missing\n");
            ret = 1;
            goto retrn;
        }

        gsnum(pvar.strng, out ntype, out lwnum, &v);
        wnum = lwnum;
        if (ntype != 1 || wnum < 1)
        {
            Console.WriteLine("Error in subwrd:  2nd argument invalid.\n");
            ret = 1;
            goto retrn;
        }

        /* Find the desired word in the string */

        pvar = pcmn.farg;
        ch = pvar.strng;
        i = 0;
        while (*ch)
        {
            if (*ch == ' ' || *ch == '\n' || *ch == '\t' || i == 0)
            {
                while (*ch == ' ' || *ch == '\n' || *ch == '\t') ch++;
                if (*ch) i++;
                if (i == wnum) break;
            }
            else ch++;
        }


        /* Get length of returned word. */

        len = 0;
        while (*(ch + len) != '\0' && *(ch + len) != ' '
                                   && *(ch + len) != '\t' && *(ch + len) != '\n') len++;

        /* Allocate storage for the result */

        res = (char*)malloc(len + 1);
        if (res == null)
        {
            Console.WriteLine("Error:  Storage allocation error\n");
            ret = 1;
            goto retrn;
        }

        /* Deliver the result and return */

        for (i = 0; i < len; i++) *(res + i) = *(ch + i);
        *(res + len) = '\0';

        ret = 0;
        pcmn.rres = res;

        /* Release arg storage and return */

        retrn:

        gsfrev(pcmn.farg);
        pcmn.farg = null;
        return (ret);
    }

/*  Routine to get specified line in a string */

    int gsflin(gscmn pcmn) {
        gsvar? pvar;
        string ch,  res;
        int ret, ntype, lnum, i, len;
        int llnum;
        double v = -1;

        pcmn.rres = null;

        /* Attempt to convert 2nd arg to integer. */

        pvar = pcmn.farg;
        if (pvar == null)
        {
            Console.WriteLine("Error in sublin:  1st argument missing\n");
            ret = 1;
            goto retrn;
        }

        pvar = pvar.forw;
        if (pvar == null)
        {
            Console.WriteLine("Error in sublin:  2nd argument missing\n");
            ret = 1;
            goto retrn;
        }

        gsnum(pvar.strng, out ntype, out llnum, out v);
        lnum = llnum;
        if (ntype != 1 || lnum < 1)
        {
            Console.WriteLine("Error in sublin:  2nd argument invalid.\n");
            ret = 1;
            goto retrn;
        }

        /* Find the desired line in the string */

        pvar = pcmn.farg;
        ch = pvar.strng;

        string[] lines = ch.Split('\n');
        res = lines[lnum];
        
        

        /* Deliver the result and return */
        ret = 0;
        pcmn.rres = res;

        /* Release arg storage and return */

        retrn:

        pcmn.farg = null;
        return (ret);
    }

/* Read function.  Expects one arg: the file name.
   Returnes a two line result -- an rc, and the record read */

    int gsfrd(gscmn pcmn) {
        Stream? ifile;
        gsvar? pvar;
        gsiob? iob, iobo;
        string? res, name,ch;
        int ret, n;
        char rc;

        pcmn.rres = null;
        
        /* Get file name */

        pvar = pcmn.farg;
        if (pvar == null)
        {
            Console.WriteLine("Error in read:  File name missing\n");
            ret = 1;
            goto retrn;
        }

        name = pvar.strng;
        if (String.IsNullOrEmpty(name))
        {
            Console.WriteLine("Error in read:  null File Name\n");
            ret = 1;
            goto retrn;
        }

        /* Check to see if the file is already open */

        iob = pcmn.iob;
        iobo = iob;
        while (iob!=null)
        {
            if (name == iob.name) break;
            iobo = iob;
            iob = iob.forw;
        }

        /* If it was not open, open it and chain a new iob */

        if (iob == null)
        {
            if(File.Exists(name))
            {
                ifile = File.Open(name, FileMode.Open);
            }
            else
            {
                rc = '1';
                goto rslt;
            }

            iob = new gsiob();
           
            if (pcmn.iob == null) pcmn.iob = iob;
            else iobo.forw = iob;
            iob.forw = null;
            iob.file = ifile;
            iob.name = name;
            iob.flag = 1;
            pvar.strng = null;
        }
        else
        {
            if (iob.flag != 1)
            {
                rc = '8';
                Console.WriteLine("Error in read:  attempt to read a file open for write");
                Console.WriteLine("  File name = {0}", iob.name);
                goto rslt;
            }

            ifile = iob.file;
        }

        /* Read the next record into the buffer area */

        ch = fgets(res + 2, RSIZ - 3, ifile);
        if (ch == null)
        {
            if (feof(ifile)) rc = '2';
            else rc = '9';
            goto rslt;
        }

        rc = '0';
        /* Remove cr for PC/cygwin version */
        ch = res + 2;
        n = strlen(ch);
        if (n > 1)
        {
            if ((int)ch[n - 2] == 13)
            {
                ch[n - 2] = ch[n - 1];
                ch[n - 1] = '\0';
            }
        }
        /* Complete return arg list */

        rslt:

        ret = 0;
        pcmn.rres = rc.ToString();
        res = null;

        /* Release arg storage and return */

        retrn:

        pcmn.farg = null;
        return (ret);
    }

/* Write function.  Expects two or three args:  file name,
   output record, and optional append flag.  Returns a return code. */

    int gsfwt(gscmn pcmn) {
        Stream ofile;
        gsvar? pvar,  pvars;
        gsiob? iob,  iobo;
        string? res,  name, orec;
        char rc;
        int ret, appflg, len;

        pcmn.rres = null;
        
        /* Get file name */

        pvar = pcmn.farg;
        if (pvar == null)
        {
            Console.WriteLine("Error in write:  File name missing\n");
            ret = 1;
            goto retrn;
        }

        name = pvar.strng;
        if (String.IsNullOrEmpty(name))
        {
            Console.WriteLine("Error in write:  null File Name\n");
            ret = 1;
            goto retrn;
        }

        pvars = pvar;

        /* Get output record */

        pvar = pvar.forw;
        if (pvar == null)
        {
            Console.WriteLine("Error in write:  Output Record arg is missing\n");
            ret = 1;
            goto retrn;
        }

        orec = pvar.strng;

        /* Check for append flag */

        pvar = pvar.forw;
        if (pvar == null) appflg = 0;
        else appflg = 1;

        /* Check to see if the file is already open */

        iob = pcmn.iob;
        iobo = iob;
        while (iob!=null)
        {
            if (name == iob.name) break;
            iobo = iob;
            iob = iob.forw;
        }

        /* If it was not open, open it and chain a new iob */

        if (iob == null)
        {
            if (appflg==1) ofile = File.Open(name, FileMode.Append);
            else ofile = File.Open(name, FileMode.Create);
            if (ofile == null)
            {
                rc = '1';
                goto rslt;
            }

            iob = new gsiob();
            

            if (pcmn.iob == null) pcmn.iob = iob;
            else iobo.forw = iob;
            iob.forw = null;
            iob.file = ofile;
            iob.name = name;
            iob.flag = 2;
            pvars.strng = null;
        }
        else
        {
            if (iob.flag != 2)
            {
                rc = '8';
                Console.WriteLine("Error in write: attempt to write a file open for read");
                Console.WriteLine("  File name = {0}", iob.name);
                goto rslt;
            }

            ofile = iob.file;
        }

        /* Write the next record */

        new StreamWriter(ofile).Write(orec);
        rc = '0';

        /* Complete return arg list */

        rslt:

        ret = 0;
        pcmn.rres = rc.ToString();
        res = null;

        /* Release arg storage and return */

        retrn:

        pcmn.farg = null;
        return (ret);
    }

/* Close function.  Expects one arg:  file name.
   Returns a return code:  0, normal, 1, file not open */

    int gsfcl(gscmn pcmn) {
        gsvar? pvar;
        gsiob? iob,  iobo;
        string? name, res;
            char rc;
        int ret;

        pcmn.rres = null;
        

        /* Get file name */

        pvar = pcmn.farg;
        if (pvar == null)
        {
            Console.WriteLine("Error in close:  File name missing\n");
            ret = 1;
            goto retrn;
        }

        name = pvar.strng;
        if (String.IsNullOrEmpty(name))
        {
            Console.WriteLine("Error in close:  null File Name\n");
            ret = 1;
            goto retrn;
        }

        /* Check to see if the file is already open */

        iob = pcmn.iob;
        iobo = iob;
        while (iob!=null)
        {
            if (name == iob.name) break;
            iobo = iob;
            iob = iob.forw;
        }

        /* If it was not open, print message and return */

        if (iob == null)
        {
            rc = '1';
            Console.WriteLine("Error in close:  file not open\n");
            Console.WriteLine("  File name = %s\n", name);
        }
        else
        {
            iob.file.Close();
            if (iob == pcmn.iob) pcmn.iob = iob.forw;
            else iobo.forw = iob.forw;
            rc = '0';
        }

        /* Complete return arg list */
        ret = 0;
        pcmn.rres = rc.ToString();
        res = null;

        /* Release arg storage and return */

        retrn:

        pcmn.farg = null;
        return (ret);
    }

/*  Routine to return position of specified word in a string */

    int gsfpwd(gscmn pcmn) {
        gsvar? pvar;
        string ch,  res;
        int ret, ntype, wnum, i, pos;
        int lwnum;
        double v = -1;

        pcmn.rres = null;

        /* Attempt to convert 2nd arg to integer. */

        pvar = pcmn.farg;
        if (pvar == null)
        {
            Console.WriteLine("Error in wrdpos:  1st argument missing\n");
            ret = 1;
            goto retrn;
        }

        pvar = pvar.forw;
        if (pvar == null)
        {
            Console.WriteLine("Error in wrdpos:  2nd argument missing\n");
            ret = 1;
            goto retrn;
        }

        gsnum(pvar.strng, out ntype, out lwnum, out v);
        wnum = lwnum;
        if (ntype != 1 || wnum < 1)
        {
            Console.WriteLine("Error in wrdpos:  2nd argument invalid.\n");
            ret = 1;
            goto retrn;
        }

        /* Find the desired word in the string */

        pvar = pcmn.farg;
        ch = pvar.strng;
        i = 0;
        while (i < ch.Length)
        {
            if (ch[i] == ' ' || ch[i] == '\n' || ch[i] == '\t' || i == 0)
            {
                while (ch[i] == ' ' || ch[i] == '\n' || ch[i] == '\t') i++;
                if (i == wnum) break;
            }
            else i++;
        }

        /* Calculcate position of the desired word */

        if (i == ch.Length) pos = 0;
        else pos = i;

        

        /* Deliver the result and return */

        ret = 0;
        pcmn.rres = pos.ToString();

        /* Release arg storage and return */

        retrn:

        pcmn.farg = null;
        return (ret);
    }

/*  Routine to return the length of a string */

    int gsfsln(gscmn pcmn) {
        gsvar? pvar;
        int ret;

        pcmn.rres = null;

        pvar = pcmn.farg;
        if (pvar == null)
        {
            Console.WriteLine("Error in strlen:  Argument missing\n");
            ret = 1;
            goto retrn;
        }

        

        /* Deliver the result and return */

        ret = 0;
        pcmn.rres = pvar.strng.Length.ToString();

        /* Release arg storage and return */

        retrn:

        pcmn.farg = null;
        return (ret);
    }

/*  Routine to check if a string is a valid numeric */

    int gsfval(gscmn pcmn) {
        gsvar? pvar;
        string res;
        int ret, ntype;
        int lwnum;
        double v;

        pcmn.rres = null;

        pvar = pcmn.farg;
        if (pvar == null)
        {
            Console.WriteLine("Error in valnum:  Argument missing\n");
            ret = 1;
            goto retrn;
        }

        gsnum(pvar.strng, out ntype, out lwnum, out v);
        
        /* Deliver the result and return */

        ret = 0;
        pcmn.rres = ntype.ToString();

        /* Release arg storage and return */

        retrn:

        pcmn.farg = null;
        return (ret);
    }

/*  Routine to control gsf loading.  */

    int gsfallw(gscmn pcmn) {
        gsvar? pvar;
        int ret, i;

        pcmn.rres = null;

        pvar = pcmn.farg;
        if (pvar == null)
        {
            Console.WriteLine("Error in gsfallow:  Argument missing\n");
            ret = 1;
            goto retrn;
        }

        i = 999;
        if (pvar.strng.ToLower() == "on") i = 1;
        if (pvar.strng.ToLower() == "off") i = 0;
        if (i < 900) pcmn.gsfflg = i;

        /* Deliver the result and return */

        ret = 0;
        pcmn.rres = i.ToString();

        /* Release arg storage and return */

        retrn:
        pcmn.farg = null;
        return (ret);
    }

/*  Routine to set gsf private path  */

    int gsfpath(gscmn pcmn) {
        gsvar pvar;
        string res;
        int ret, i, j;

        pcmn.rres = null;

        pvar = pcmn.farg;
        if (pvar == null)
        {
            Console.WriteLine("Error in gsfpath:  Argument missing\n");
            ret = 1;
            goto retrn;
        }

        /* Copy the path to the gscmn area */

        pcmn.ppath = pvar.strng;

        /* Deliver the result and return */

        res = "1";
        ret = 0;
        pcmn.rres = res;

        /* Release arg storage and return */

        retrn:

        pcmn.farg = null;
        return (ret);
    }

/*  Routine to do libmf math */

    int gsfmath(gscmn pcmn, int mathflg) {
        gsvar? pvar;
        string res;
        string buf;
        string vformat = "{0}";
        string mathmsg1 = "log";
        string mathmsg2 = "log10";
        string mathmsg3 = "cos";
        string mathmsg4 = "sin";
        string mathmsg5 = "tan";
        string mathmsg6 = "atan";
        string mathmsg7 = "atan2";
        string mathmsg8 = "sqrt";
        string mathmsg9 = "abs";
        string mathmsg10 = "acosh";
        string mathmsg11 = "asinh";
        string mathmsg12 = "atanh";
        string mathmsg13 = "cosh";
        string mathmsg14 = "sinh";
        string mathmsg15 = "exp";
        string mathmsg16 = "fmod";
        string mathmsg17 = "pow";
        string mathmsg18 = "sinh";
        string mathmsg19 = "tanh";
        string mathmsg20 = "acos";
        string mathmsg21 = "asin";
        string mathmsg22 = "format";
        string mathmsg23 = "nint";
        string mathmsg24 = "int";
        string mathmsg25 = "mod";
        string mathmsg26 = "strlen";
        string? mathmsg = null;
        int ret, ntype, i, len;
        int lwnum;
        double v=-1, v2=-1;

        pcmn.rres = null;

        if (mathflg == 1) mathmsg = mathmsg1;
        if (mathflg == 2) mathmsg = mathmsg2;
        if (mathflg == 3) mathmsg = mathmsg3;
        if (mathflg == 4) mathmsg = mathmsg4;
        if (mathflg == 5) mathmsg = mathmsg5;
        if (mathflg == 6) mathmsg = mathmsg6;
        if (mathflg == 7) mathmsg = mathmsg7;
        if (mathflg == 8) mathmsg = mathmsg8;
        if (mathflg == 9) mathmsg = mathmsg9;
        if (mathflg == 10) mathmsg = mathmsg10;
        if (mathflg == 11) mathmsg = mathmsg11;
        if (mathflg == 12) mathmsg = mathmsg12;
        if (mathflg == 13) mathmsg = mathmsg13;
        if (mathflg == 14) mathmsg = mathmsg14;
        if (mathflg == 15) mathmsg = mathmsg15;
        if (mathflg == 16) mathmsg = mathmsg16;
        if (mathflg == 17) mathmsg = mathmsg17;
        if (mathflg == 18) mathmsg = mathmsg18;
        if (mathflg == 19) mathmsg = mathmsg19;
        if (mathflg == 20) mathmsg = mathmsg20;
        if (mathflg == 21) mathmsg = mathmsg21;
        if (mathflg == 22) mathmsg = mathmsg22;
        if (mathflg == 23) mathmsg = mathmsg23;
        if (mathflg == 24) mathmsg = mathmsg24;
        if (mathflg == 25) mathmsg = mathmsg25;
        if (mathflg == 26) mathmsg = mathmsg26;

        pvar = pcmn.farg;
        if (pvar == null)
        {
            Console.WriteLine("Error in math_{0}:  Argument missing\n", mathmsg);
            ret = 1;
            goto retrn;
        }

        if (!(mathflg == 22 || mathflg == 26))
        {
            gsnum(pvar.strng, out ntype, out lwnum, out v);

            if (ntype == 0)
            {
                Console.WriteLine("Error in math_{0}:  Argument not a valid numeric\n", mathmsg);
                ret = 1;
                goto retrn;
            }
        }
        else
        {
            if (mathflg == 22)
            {
                if (pvar.strng.Length < 15)
                {
                    vformat = pvar.strng;
                }
                else
                {
                    Console.WriteLine("Error in math_{0}:  argument: {0}  too long < 15\n", mathmsg, pvar.strng);
                    ret = 1;
                    goto retrn;
                }
            }
        }

        if (v <= 0.0 && (mathflg == 1 || mathflg == 2))
        {
            Console.WriteLine("Error in math_{0}:  Argument less than or equal to zero\n", mathmsg);
            ret = 1;
            goto retrn;
        }

        if (mathflg == 16)
        {
            pvar = pvar.forw;
            if (pvar == null)
            {
                Console.WriteLine("Error in fmod:  2rd argument missing\n");
                ret = 1;
                goto retrn;
            }

            gsnum(pvar.strng, out ntype, out lwnum, out v2);

            if (ntype == 0)
            {
                Console.WriteLine("Error in fmod:  2rd argument invalid.\n");
                ret = 1;
                goto retrn;
            }
        }

        if (mathflg == 17)
        {
            pvar = pvar.forw;
            if (pvar == null)
            {
                Console.WriteLine("Error in pow:  2rd argument missing\n");
                ret = 1;
                goto retrn;
            }

            gsnum(pvar.strng, out ntype, out lwnum, out v2);

            if (ntype == 0)
            {
                Console.WriteLine("Error in pow:  2rd argument invalid.\n");
                ret = 1;
                goto retrn;
            }
        }

        if (mathflg == 7)
        {
            pvar = pvar.forw;
            if (pvar == null)
            {
                Console.WriteLine("Error in atan2:  2rd argument missing\n");
                ret = 1;
                goto retrn;
            }

            gsnum(pvar.strng, out ntype, out lwnum, out v2);

            if (ntype == 0)
            {
                Console.WriteLine("Error in atan2:  2rd argument invalid.\n");
                ret = 1;
                goto retrn;
            }
        }

        if (mathflg == 25)
        {
            pvar = pvar.forw;
            if (pvar == null)
            {
                Console.WriteLine("Error in mod:  2rd argument missing\n");
                ret = 1;
                goto retrn;
            }

            gsnum(pvar.strng, out ntype, out lwnum, out v2);

            if (ntype == 0)
            {
                Console.WriteLine("Error in mod:  2rd argument invalid.\n");
                ret = 1;
                goto retrn;
            }
        }

        if (mathflg == 22)
        {
            pvar = pvar.forw;
            if (pvar == null)
            {
                Console.WriteLine("Error in format:  2rd argument missing\n");
                ret = 1;
                goto retrn;
            }

            gsnum(pvar.strng, out ntype, out lwnum, out v);

            if (ntype == 0)
            {
                Console.WriteLine("Error in format:  2rd argument invalid.\n");
                ret = 1;
                goto retrn;
            }
        }


        /* Get result */

        if (mathflg == 1) v = Math.Log(v);
        if (mathflg == 2) v = Math.Log10(v);
        if (mathflg == 3) v = Math.Cos(v);
        if (mathflg == 4) v = Math.Sin(v);
        if (mathflg == 5) v = Math.Tan(v);
        if (mathflg == 6) v = Math.Atan(v);
        if (mathflg == 7) v = Math.Atan2(v, v2);
        if (mathflg == 8) v = Math.Sqrt(v);
        if (mathflg == 9) v = Math.Abs(v);
        if (mathflg == 10) v = Math.Abs(v);
        if (mathflg == 11) v = Math.Asinh(v);
        if (mathflg == 12) v = Math.Atanh(v);
        if (mathflg == 13) v = Math.Cosh(v);
        if (mathflg == 14) v = Math.Sinh(v);
        if (mathflg == 15) v = Math.Exp(v);
        if (mathflg == 16) v = v % v2;
        if (mathflg == 17) v = Math.Pow(v, v2);
        if (mathflg == 18) v = Math.Sinh(v);
        if (mathflg == 19) v = Math.Tanh(v);
        if (mathflg == 20) v = Math.Acos(v);
        if (mathflg == 21) v = Math.Asin(v);


        if (mathflg == 23)
        {
            v = Math.Floor(v + 0.5);
        }
        else if (mathflg == 24)
        {
            v = Math.Floor(v);
        }
        else if (mathflg == 25)
        {
            v = Math.Floor((v % v2));
        }
        else if (mathflg == 26)
        {
            v = pvar.strng.Length;
        }

        if (mathflg == 22)
        {
            buf = String.Format(vformat, v);
            
        }
        else
        {
            buf = String.Format("{0:.15g}", v);
            
        }


        

        /* Allocate storage for the result */

        res = buf;
        /* Deliver the result and return */

        ret = 0;
        pcmn.rres = res;

        /* Release arg storage and return */

        retrn:

        pcmn.farg = null;
        return (ret);
    }

/* Following functions are related to reading the script file
   into memory based on the file name and path specification */

/* Open the main script; search the path if needed 

     Rules:  When working with the name of the primary 
             script, 1st try to open the name provided, as is.
             If this fails, append .gs (if not there already)
             and try again.  If this fails, and the file 
             name provided does not start with a /, then 
             we try the directories in the GASCRP envvar, 
             both with the primary name and the .gs extension.

     Code originally by M.Fiorino   */

    Stream? gsonam(gscmn pcmn, gsfdef pfdf) {
        Stream? ifile = null;
        string? uname, xname, dname, lname, oname;
        string? sdir;
        int len;

        uname = null; /* user provided name */
        xname = null; /* user name plus extension */
        dname = null; /* path dir name */
        lname = null; /* path plus uname or xname */
        oname = null; /* name of file that gets opened */

        /* First try to open by using the name provided. */

        uname = pcmn.fname;
        if (uname == null) return (null);
        if (File.Exists(uname))
        {
            ifile = File.Open(uname, FileMode.Open, FileAccess.Read);
        }

        
        /* If that failed, then try adding a .gs extension,
           but only if one is not already there */
        
        if (ifile == null)
        {
            if (!uname.EndsWith(".gs"))
            {
                xname = uname + ".gs";
                if (File.Exists(xname))
                {
                    ifile = File.Open(xname, FileMode.Open, FileAccess.Read);
                    oname = xname;
                    xname = null;
                }
            }
            
            /* If that didn't work, search in the GASCRP path --
               the path contains blank-delimited directory names */

            if (ifile == null && !uname.StartsWith("/"))
            {
                sdir = System.Environment.GetEnvironmentVariable("GASCRP");
                
                int pos = 0;
                while (pos < sdir.Length)
                {
                    
                    while (gsdelim(sdir[pos])) pos++;
                    if (pos == sdir.Length) break;
                    dname = gsstcp(sdir.Substring(pos));
                    if (!dname.EndsWith("/"))
                    {
                        dname += "/";
                    }

                    lname = gsstad(dname, uname); /* try uname plus dirname */
                    if (File.Exists(lname))
                    {
                        ifile = File.Open(lname, FileMode.Open, FileAccess.Read);
                        oname = lname;
                        lname = null;
                        break;
                    }
                    else
                    {
                        /* try xname plus dirname */
                        lname = null;
                        if (xname!=null)
                        {
                            if (File.Exists(lname))
                            {
                                ifile = File.Open(lname, FileMode.Open, FileAccess.Read);
                           
                                oname = lname;
                                lname = null;
                                break;
                            }
                            else
                            {
                                lname = null;
                            }
                        }
                    }

                    while (sdir[pos] != ' ' && pos!=sdir.Length) pos++; /* Advance */
                    dname = null;
                }
            }
        }
        else
        {
            oname = uname;
            uname = null;
        }
        /* If we opened a file, figure out the prefix */

        if (ifile!=null)
        {
            pfdf.name = oname;
            xname = oname;
            len = 0;
            pcmn.fprefix = Path.GetDirectoryName(xname);
            pcmn.fprefix = xname;
        }

        return (ifile);
    }

/*  When working with a .gsf, the function name
    is appended with .gsf.  Then we first try the
    same directory that the main script was found
    in.  If that fails, then we try the search path
    in GASCRP. */

    Stream gsogsf(gscmn pcmn, gsfdef pfdf, string pfnc) {
        Stream? ifile;
        string fname, tname,dname,sdir;
        int len, i;
        string nname;

        /* Function name is not null terminated -- make a copy that is */

        nname = pfnc.Trim();

        fname = gsstad(nname, ".gsf");
        
        /* First try the prefix directory */

        tname = gsstad(pcmn.fprefix??"", fname);
        if (File.Exists(tname))
        {
            ifile = File.Open(tname, FileMode.Open, FileAccess.Read);
            pfdf.name = tname;
            return (ifile);
        }
        
        /* Next try the private path.  The file names are constructed
           as the prefix plus the private path plus the function name
           plus the .gsf */

        sdir = pcmn.ppath;

        int pos = 0;
        while (pos < sdir.Length)
        {
            while (gsdelim(sdir[pos])) pos++;
            if (pos == sdir.Length) break;
            dname = gsstcp(sdir);
            if (!dname.EndsWith("/"))
            {
                dname += "/";
            }
            
            tname = gsstad(dname, fname);
            dname = gsstad(pcmn.fprefix??"", tname);
            if (File.Exists(dname))
            {
                ifile = File.Open(dname, FileMode.Open, FileAccess.Read);
                pfdf.name = dname;
                return (ifile);
            }
            while (sdir[pos] != ' ' && pos!=sdir.Length) pos++; /* Advance */
        }

        /* If we fall thru, next try the GASCRP path */

        sdir = System.Environment.GetEnvironmentVariable("GASCRP")??"";
        pos = 0;
        while (pos < sdir.Length)
        {
            while (gsdelim(sdir[pos])) pos++;
            if (pos==sdir.Length) break;
            dname = gsstcp(sdir);
            
            if (!dname.EndsWith("/"))
            {
                dname += "/";
            }
            
            tname = gsstad(dname, fname);
            
            if (File.Exists(tname))
            {
                ifile = File.Open(tname, FileMode.Open, FileAccess.Read);
                pfdf.name = tname;
                return (ifile);
            }
            
            while (sdir[pos] != ' ' && pos!=sdir.Length) pos++; /* Advance */
        }

        /* If we fall thru, we didn't find anything.  */
        return (null);
    }

/* Copy a string to a new dynamically allocated area.
   Copy until a delimiter or null is encountered.
   Caller is responsible for freeing the storage.  */

    string gsstcp(string ch)
    {
        StringBuilder bld = new StringBuilder();

        int i = 0;
        while (!gsdelim(ch[i]) && i < ch.Length)
        {
            bld.Append(ch[i]);
            i++;
        }

        return bld.ToString();
    }

/* Determine if a character is a delimiter for seperating the
   directory names in the GASCRP path.    To add new 
   delimiters, put them here. */

    bool gsdelim(char ch)
    {
        if (ch == ' ') return (true);
        if (ch == ';') return true;
        if (ch == ',') return true;
        if (ch == ':') return true;
        return false;
    }

/* Concatentate two strings and make a new string */

    string gsstad(string ch1, string ch2)
    {
        return ch1 + ch2;
    }
}