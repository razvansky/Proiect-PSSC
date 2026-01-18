using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.exceptii;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Lucrarea1PSSC.clase.ClaseCos
{
    public class CosDeCumparaturi
    {
        private List<ProdusCos> produse_cos;
        private ProdusCos produs;
        private IStareCos stare_cos;
        private Thread backgroundThread;

        private void ControlStareCos()
        {
            while (true)
            {
                if (stare_cos is PayedCos payed && payed.payed)
                {

                    break; // Stop checking if paid
                }

                RefreshStateFromLines();

                Thread.Sleep(1000); // Check every second
            }
        }

        private void RefreshStateFromLines()
        {
            if (stare_cos is PayedCos) return;

            if (produse_cos == null || produse_cos.Count == 0)
            {
                stare_cos = new EmptyCos(true);
            }
            else
            {
                stare_cos = new ValidatedCos(true);
            }
        }

        public CosDeCumparaturi(int abc)
        {
            produse_cos = new List<ProdusCos>();
            stare_cos = new UnvalidatedCos(true);
        }

        public CosDeCumparaturi()
        {
            produse_cos = new List<ProdusCos>();
            stare_cos = new EmptyCos(true);
            backgroundThread = new Thread(ControlStareCos)
            {
                IsBackground = true
            };
            backgroundThread.Start();
        }

        public CosDeCumparaturi(List<ProdusCos> produse_cos)
        {
            this.produse_cos = produse_cos ?? new List<ProdusCos>();
            stare_cos = this.produse_cos.Count == 0 ? new EmptyCos(true) : new ValidatedCos(true);
        }

        public void AdaugaProdus(string Nume, List<Produs> produse_mag)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Nume))
                    throw new ArgumentException("Numele produsului nu poate fi gol");

                if (stare_cos is PayedCos)
                    throw new InvalidCosException("Cosul a fost platit, nu se mai pot adauga produse");

                if (stare_cos is UnvalidatedCos)
                    throw new InvalidCosException("Cosul nu este validat, nu se pot adauga produse");
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidCosException)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            var produss = produse_mag.FirstOrDefault(p => p.Nume == Nume);

            try
            {
                if (produss == null)
                    throw new ArgumentException("Produsul nu exista in magazin");

                if (produss.Quantity.Cantitate <= 0)
                    throw new ArgumentException("Produsul a fost epuizat");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            produs = new ProdusCos(produss.CodProdus, produss.Nume, new UnitQuantity(1), produss.Kilogram, produss.Pret);
            produse_cos.Add(produs);

            for (int i = 0; i < produse_mag.Count; i++)
            {
                var p = produse_mag[i];
                if (p.Nume == produs.Nume)
                {
                    var updatedProdus = p with
                    {
                        Quantity = new UnitQuantity(p.Quantity.Cantitate - 1)
                    };
                    produse_mag[i] = updatedProdus;
                    break;
                }
            }

            // ensure 'TotalCos' reflects the new state immediately
            RefreshStateFromLines();
        }

        public void StergeProdus(string Nume, List<Produs> produse_mag)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Nume))
                    throw new ArgumentException("Numele produsului nu poate fi gol");

                if (stare_cos is PayedCos)
                    throw new InvalidCosException("Cosul a fost platit, nu se mai pot sterge produse");

                if (stare_cos is UnvalidatedCos)
                    throw new InvalidCosException("Cosul nu este validat, nu se pot sterge produse");
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidCosException)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            var produs = produse_cos.FirstOrDefault(p => p.Nume == Nume);
            try
            {
                if (produs == null)
                    throw new ArgumentException("Produsul nu exista in cos");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            produse_cos.Remove(produs);
            for (int i = 0; i < produse_mag.Count; i++)
            {
                var p = produse_mag[i];
                if (p.Nume == produs.Nume)
                {
                    var updatedProdus = p with
                    {
                        Quantity = new UnitQuantity(p.Quantity.Cantitate + 1)
                    };
                    produse_mag[i] = updatedProdus;
                    break;
                }
            }

            RefreshStateFromLines();
        }

        public void GolesteCos(List<Produs> produse_mag)
        {
            try
            {
                if (stare_cos is PayedCos)
                    throw new InvalidOperationException("Cosul a fost platit, nu se mai poate goli");

                if (stare_cos is UnvalidatedCos || stare_cos is EmptyCos)
                    throw new InvalidOperationException("Cosul nu este validat, nu se poate goli");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            foreach (var produsCos in produse_cos)
            {
                for (int i = 0; i < produse_mag.Count; i++)
                {
                    var p = produse_mag[i];
                    if (p.Nume == produsCos.Nume)
                    {
                        var updatedProdus = p with
                        {
                            Quantity = new UnitQuantity(p.Quantity.Cantitate + produsCos.Cantitate.Cantitate)
                        };
                        produse_mag[i] = updatedProdus;
                        break;
                    }
                }
            }

            produse_cos.Clear();
            RefreshStateFromLines();
        }

        public void AfiseazaProduse()
        {
            foreach (var produs in produse_cos)
            {
                Console.WriteLine($"Produs: {produs.Nume}, Cantitate: {produs.Cantitate.Cantitate}, Kilograme: {produs.Kilogram.CantitateKilogram},Cod Produs: {produs.CodProd.Cod}, Pret unitar: {produs.Price.pret}, Pret total produs: {CalculeazaTotalProdus(produs)}");
            }
        }

        public double CalculeazaTotalProdus(ProdusCos produs)
        {
            return (double)produs.Price.pret * produs.Cantitate.Cantitate * produs.Kilogram.CantitateKilogram;
        }

        public double TotalCos()
        {
            try
            {
                if (stare_cos is UnvalidatedCos)
                    throw new InvalidCosException("Cosul nu este validat, nu se poate calcula totalul");

                // IMPORTANT: do not rely on state to determine emptiness; rely on data.
                if (produse_cos == null || produse_cos.Count == 0)
                    throw new InvalidCosException("Cosul este gol, nu se poate calcula totalul");
            }
            catch (InvalidCosException ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }

            double totalCos = 0;
            foreach (var produs in produse_cos)
            {
                totalCos += (double)produs.Price.pret * produs.Cantitate.Cantitate * produs.Kilogram.CantitateKilogram;
            }
            return totalCos;
        }

        public List<ProdusCos> GetProduseCos()
        {
            return produse_cos;
        }

        public bool platesteCos()
        {
            try
            {
                stare_cos = stare_cos switch
                {
                    PayedCos payed when payed.payed => throw new InvalidCosException("Cosul a fost deja platit!"),
                    UnvalidatedCos unvalidated => throw new InvalidCosException("Cosul nu este validat!"),
                    EmptyCos(true) => throw new InvalidCosException("Cosul este gol!"),
                    _ => new PayedCos(true)
                };
            }
            catch (InvalidCosException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            Console.WriteLine("Cosul a fost platit cu succes! Pentru alte cumparaturi va fi necesar sa creati un cos nou");
            return true;
        }

        public IStareCos GetStareCos()
        {
            return stare_cos;
        }
    }
}
