namespace GradsSharp.Data.Grads;

public interface IGradsDatabase
{
    int gxdbqhersh ();
    (string, bool, int) gxdbqfont (int fn);
    (int, int, int, int, int) gxdbqcol(int colr);
    int gxdbacol (int clr, int red, int green, int blue, int alpha);
    void gxdbsettransclr (int clr);
    int gxdbqtransclr();
    double gxdbqwid(int idx);

    void gxdboutbck(int clr);

    int gxdbkq();
}