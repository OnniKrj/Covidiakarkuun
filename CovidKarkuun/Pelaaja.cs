using Jypeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Pelaaja : PlatformCharacter
{
    private int elamia;
    //private IntMeter elamaLaskuri = new IntMeter(3, 0, 3);
    //public IntMeter ElamaLaskuri { get { return elamaLaskuri; } }

    //public Pelaaja(double leveys, double korkeus)
    //   : base(leveys, korkeus)
    //{
    //    elamaLaskuri.LowerLimit += delegate { this.Destroy(); };
    //}
    public Pelaaja(double width, double heigth, int elamat) : base(width, heigth)
    {
        this.elamia = elamat;
    }

    public bool OtavastaanOsuma()
    {
        elamia--;
        if (elamia < 0)
        {
            this.Destroy();
            return true;
        }
        return false;
    }

    public void LoydaMaski()
    {
        elamia++;
    }

}
