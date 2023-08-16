using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;

/// @authors: Onni Karjalainen & Kati Nietosvuori
/// @versio: 3.14....
/// <summary>
/// Pakene koronan infektoimasta sairaalasta.
/// </summary>


public class CovidKarkuun : PhysicsGame
{
    const double RUUDUN_LEVEYS = 15;
    const double RUUDUN_KORKEUS = 15;

    const double LIIKKUMISVOIMA = 150;
    const double HYPPYVOIMA = 1000;

    int kenttaNro = 1;
    Image taustakuva = LoadImage("taustanew.png");

    /// <summary>
    /// Pelin aloitus, joka lataa ensimmäisen tason.
    /// </summary>
    public override void Begin()
    {

        Level.Background.Image = taustakuva;
        Level.Background.FitToLevel();
        SetWindowSize(700, 600);
        Level.CreateBorders(1.0, false);
        Camera.ZoomToLevel();

        LuoKentta("Kartta4");

        Gravity = new Vector(0, -2500);

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    /// <summary>
    /// Ladataan seuraava kenttä.
    /// </summary>
    public void SeuraavaKentta()
    {
        ClearAll();
        if (kenttaNro == 1) LuoKentta("Kartta1");
        else if (kenttaNro == 2) LuoKentta("Kartta2");
        else if (kenttaNro == 3) LuoKentta("Kartta3");
        else if (kenttaNro == 4) LuoKentta("Kartta4");
        else if (kenttaNro > 4) Exit();
    }

    /// <summary>
    /// Luodaan kentät teksitiedostoista.
    /// </summary>
    /// <param name="kenttatiedosto">Kartan tiedostonnimi muuttujana</param>
    public void LuoKentta(string kenttatiedosto)
    {
        TileMap kentta = TileMap.FromLevelAsset(kenttatiedosto);
        kentta.SetTileMethod('x', TeeTaso, "taso1new.png");
        kentta.SetTileMethod('t', TeeTaso, "taso2new.png");
        kentta.SetTileMethod('P', LuoPelaaja);
        kentta.SetTileMethod('V', LuoVirus, 18);
        kentta.SetTileMethod('v', LuoVirus, -10);
        kentta.SetTileMethod('k', LuoVirus, 5);
        kentta.SetTileMethod('K', LuoVirus, -4);
        kentta.SetTileMethod('M', LuoMaski);
        kentta.SetTileMethod('R', TeeRappuset);
        kentta.SetTileMethod('L', VoititPelin);
        kentta.Optimize('x');
        kentta.Optimize('t');

        double[] koko = new double[] { 10, 15, 20, 25, 30 };
        for (int i = 0; i < koko.Length; i++)
        {
            
            GameObject pilleri = new GameObject(koko[i], koko[i]);
            pilleri.Shape = Shape.Rectangle;
            pilleri.Position = RandomGen.NextVector(Level.BoundingRect);
            pilleri.Color = Color.Black;
            pilleri.Image = LoadImage("pilleri.png");
            Add(pilleri);
        }
       
        

        kentta.Execute(RUUDUN_LEVEYS, RUUDUN_KORKEUS);
        Level.Background.Image = taustakuva;
        Level.Background.FitToLevel();
        SetWindowSize(700, 600);
        Level.CreateBorders(1.0, false);
        Camera.ZoomToLevel();

        
        


    }



    /// <summary>
    /// Luo peliin pinnat, joiden päällä liikutaan.
    /// </summary>
    /// <param name="paikka">Objektin sijainti kentällä.</param>
    /// <param name="leveys">Objektin leveys</param>
    /// <param name="korkeus">Objektin korkeus</param>
    /// <param name="kuva">Objektin grafiikka</param>
    public void TeeTaso(Vector paikka, double leveys, double korkeus, string kuva)
    {
        PhysicsObject taso = new PhysicsObject(leveys, korkeus);
        taso.Position = paikka;
        taso.MakeStatic();
        taso.Image = LoadImage(kuva);
        Add(taso);
    }

    /// <summary>
    /// Saavutetaan rappuset seuraavalle tasolle.
    /// </summary>
    /// <param name="pelaaja">Pelattava hahmo</param>
    /// <param name="rappuset"></param>
    public void LoysiRappuset(Pelaaja pelaaja, PhysicsObject rappuset)
    {
        MessageDisplay.Add("Saavuit Rappusille");
        MessageDisplay.MessageTime = new TimeSpan(0, 0, 2);
        kenttaNro++;
        SeuraavaKentta();
    }

    /// <summary>
    /// Pelattavan hahmon luominen
    /// </summary>
    /// <param name="paikka">Hahmon paikka kartalla</param>
    /// <param name="leveys">Hahmon leveys</param>
    /// <param name="korkeus">Hahmon korkeus</param>
    public void LuoPelaaja(Vector paikka, double leveys, double korkeus)
    {
        Pelaaja pelaaja = new Pelaaja(leveys * 1.5, korkeus * 1.5, 0);
        pelaaja.Position = paikka;
        pelaaja.Color = Color.Blue;
        pelaaja.Shape = Shape.Circle;
        pelaaja.Image = LoadImage("pelaajanew.png");
        pelaaja.CanRotate = false;
        Add(pelaaja);

        AddCollisionHandler<Pelaaja, PhysicsObject>(pelaaja, "maski", PelaajaLoysiMaskin);
        AddCollisionHandler<Pelaaja, PhysicsObject>(pelaaja, "virus", PelaajaOsuiVirukseen);
        AddCollisionHandler<Pelaaja, PhysicsObject>(pelaaja, "rappuset", LoysiRappuset);

        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikuta pelaajaa oikealle", pelaaja, LIIKKUMISVOIMA);
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikuta pelaajaa vasemmalle", pelaaja, -LIIKKUMISVOIMA);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Hyppää", pelaaja, HYPPYVOIMA);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="liikutettavaJuttu"></param>
    /// <param name="suunta"></param>
    public void Liikuta(PlatformCharacter liikutettavaJuttu, double suunta)
    {
        liikutettavaJuttu.Walk(suunta);
    }

    public void Hyppaa(PlatformCharacter juttu, double voima)
    {
        juttu.Jump(voima);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    /// <param name="liikeala"></param>
    public void LuoVirus(Vector paikka, double leveys, double korkeus, int liikeala)
    {
        PhysicsObject virus = new PhysicsObject(leveys, korkeus);
        virus.Position = paikka;
        virus.Color = Color.Red;
        virus.Shape = Shape.Circle;
        virus.IgnoresGravity = true;
        virus.CanRotate = false;
        virus.Tag = "virus";
        virus.Image = LoadImage("covid1new.png");
        Add(virus);


        PathFollowerBrain pfb = new PathFollowerBrain();
        List<Vector> reitti = new List<Vector>();
        reitti.Add(virus.Position);
        Vector seuraavaPiste = virus.Position + new Vector(liikeala * RUUDUN_LEVEYS, 0);
        reitti.Add(seuraavaPiste);
        pfb.Path = reitti;
        pfb.Loop = true;
        pfb.Speed = 100;
        virus.Brain = pfb;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    public void LuoMaski(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject maski = new PhysicsObject(leveys * 3, korkeus * 3);
        maski.Position = paikka;
        maski.MakeStatic();
        maski.Tag = "maski";
        maski.Shape = Shape.Star;
        maski.Color = Color.Magenta;
        maski.Image = LoadImage("maskinew.png");
        Add(maski);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    public void TeeRappuset(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject rappuset = new PhysicsObject(leveys * 3, korkeus * 3);
        rappuset.Position = paikka;
        rappuset.MakeStatic();
        rappuset.Tag = "rappuset";
        rappuset.Shape = Shape.Rectangle;
        rappuset.Color = Color.Aqua;
        rappuset.Image = LoadImage("rappusetnew.png");
        Add(rappuset);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="loytaja"></param>
    /// <param name="kohde"></param>
    public void PelaajaLoysiMaskin(Pelaaja loytaja, PhysicsObject kohde)
    {
        MessageDisplay.Add("Löysit maskin!");
        MessageDisplay.MessageTime = new TimeSpan(0, 0, 2);
        kohde.Destroy();
        loytaja.LoydaMaski();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pelaaja"></param>
    /// <param name="kohde"></param>
    public void PelaajaOsuiVirukseen(Pelaaja pelaaja, PhysicsObject kohde)
    {

        MessageDisplay.Add("Vaaransit henkesi, mutta maski pelasti sinut!");
        MessageDisplay.MessageTime = new TimeSpan(0, 0, 2);
        bool x = pelaaja.OtavastaanOsuma();
        if (x) AloitaAlusta();

    }

    /// <summary>
    /// 
    /// </summary>
    public void AloitaAlusta()
    {
        ClearAll();
        kenttaNro = 1;
        MessageDisplay.Add("Sait tartunnan!");
        MessageDisplay.MessageTime = new TimeSpan(0, 0, 2);
        Begin();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    public void VoititPelin(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject maali = new PhysicsObject(leveys, korkeus);
        maali.Position = paikka;
        maali.MakeStatic();
        maali.Tag = "maali";
        maali.Shape = Shape.Circle;
        maali.Image = LoadImage("maalinew.png");
    }



}

