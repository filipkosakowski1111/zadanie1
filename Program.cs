using System;
using System.Collections.Generic;

public enum typProduktu
{
    banany,  
    mleko,     
    hel
}

public interface IHazardNotifier
{
    void NotifyHazard(string containerId);
}

public interface IKontener
{
    string KID { get; }
    double maxwagi { get; }
    double zaladowanaWaga { get; }
    typProduktu Produkt { get; set; }
    double Load(double weight);
    void Unload();
    void Wypisz();
}

public class OverfillException : Exception
{
    public OverfillException(string message) : base(message) { }
}

public class Kontener : IKontener
{
    public string KID { get; private set; }
    public double maxwagi { get; private set; }
    public double zaladowanaWaga { get; private set; }
    public typProduktu Produkt { get; set; }

    public Kontener(string KID, double maxwagi, typProduktu produkt)
    {
        this.KID = KID;
        this.maxwagi = maxwagi;
        zaladowanaWaga = 0;
        Produkt = produkt;
    }

    public virtual double Load(double weight)
    {
        if (zaladowanaWaga + weight > maxwagi)
            throw new OverfillException("za duzo kontenerow zaladowanych");

        zaladowanaWaga += weight;
        return zaladowanaWaga;
    }

    public void Unload()
    {
        zaladowanaWaga = 0;
    }

    public virtual void Wypisz()
    {
        Console.WriteLine($"nr kontenera: {KID}, max waga: {maxwagi} kg, waga ladunku: {zaladowanaWaga} kg");
        Console.WriteLine($"Produkt w kontenerze: {Produkt}");
    }
}

public class LqKontener : Kontener, IHazardNotifier
{
    public LqKontener(string kid, double maxwagi, typProduktu produkt) : base(kid, maxwagi, produkt) { }

    public override double Load(double weight)
    {
        if (Produkt == typProduktu.mleko && weight > maxwagi * 0.5)
            Console.WriteLine($"za duzo cieczy");
        return base.Load(weight);
    }

    public void NotifyHazard(string containerId)
    {
        Console.WriteLine($"zgloszenie niebezpiecznego ladunku  {containerId}");
    }

    public override void Wypisz()
    {
        base.Wypisz();
        Console.WriteLine("Ciecz");
    }
}

public class GasKontener : Kontener, IHazardNotifier
{
    public double Pressure { get; private set; }

    public GasKontener(string kid, double maxwagi, double pressure, typProduktu produkt) : base(kid, maxwagi, produkt)
    {
        Pressure = pressure;
    }

    public void NotifyHazard(string containerId)
    {
        Console.WriteLine($"zgloszenie niebezpiecznego ladunku  {containerId}");
    }

    public override void Wypisz()
    {
        base.Wypisz();
        Console.WriteLine($"cisnienie  {Pressure} atm");
    }
}

public class Chlodnia : Kontener, IHazardNotifier
{
    public double temperatura { get; private set; }

    public Chlodnia(string kid, double maxwagi, double temperatura, typProduktu produkt) : base(kid, maxwagi, produkt)
    {
        temperatura = temperatura;
    }

    public void NotifyHazard(string containerId)
    {
        Console.WriteLine($"zgloszenie niebezpiecznego ladunku {containerId}");
    }

    public override double Load(double weight)
    {
        if (Produkt == typProduktu.banany && temperatura > 5)
            Console.WriteLine("temperatura zbyt wysoka dla bananow");
        return base.Load(weight);
    }

    public override void Wypisz()
    {
        base.Wypisz();
        Console.WriteLine($"Temperatura: {temperatura}°C");
    }
}

public class Stataek
{
    public string nazwa { get; private set; }
    public double maxPredkosc { get; private set; }
    public int maxKontenerow { get; private set; }
    public double maxWagi { get; private set; }

    private List<IKontener> kontenery { get; set; }

    public Stataek(string nazwa, double maxPredkosc, int maxKontenerow, double maxWagi)
    {
        nazwa = nazwa;
        maxPredkosc = maxPredkosc;
        maxKontenerow = maxKontenerow;
        maxWagi = maxWagi;
        kontenery = new List<IKontener>();
    }

    public void AddContainer(IKontener kontener)
    {
        if (kontenery.Count >= maxKontenerow)
        {
            Console.WriteLine("statek pelny");
            return;
        }
        if (GetCurrentWeight() + kontener.maxwagi > maxWagi)
        {
            Console.WriteLine("przekroczono max wage statku");
            return;
        }

        kontenery.Add(kontener);
        Console.WriteLine($"kontener {kontener.KID} zostal dodany do statku {nazwa}.");
    }

    public void usunKontener(IKontener kontener)
    {
        if (kontenery.Remove(kontener))
        {
            Console.WriteLine($"kontener {kontener.KID} zostal usuniety z statku {nazwa}.");
        }
        else
        {
            Console.WriteLine("nie znaleziono kontenera");
        }
    }

    public double GetCurrentWeight()
    {
        double currentWeight = 0;
        foreach (var container in kontenery)
        {
            currentWeight += container.zaladowanaWaga;
        }
        return currentWeight;
    }

    public void PrintDetails()
    {
        Console.WriteLine($"statek {nazwa}: max predkosc: {maxPredkosc} wezlow, max kontenerow: {maxKontenerow}, max waga: {maxWagi} ton");
        Console.WriteLine("kontenery na statku ");
        foreach (var container in kontenery)
        {
            container.Wypisz();
        }
    }
}

public class Program
{
    static void Main()
    {
        Stataek ship = new Stataek("Titanic", 30, 100, 40000);
        
        var kontener1 = new LqKontener("KON-L-1", 5000, typProduktu.mleko);
        kontener1.Load(2000);
        ship.AddContainer(kontener1);
        
        var kontener2 = new GasKontener("KON-G-1", 8000, 10, typProduktu.hel);
        kontener2.Load(3000);
        ship.AddContainer(kontener2);

        var kontener3 = new Chlodnia("KON-C-1", 7000, -18, typProduktu.banany);
        kontener3.Load(1500);
        ship.AddContainer(kontener3);
        ship.PrintDetails();
    }
}
