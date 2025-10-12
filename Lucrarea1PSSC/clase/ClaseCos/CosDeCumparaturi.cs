using Lucrarea1PSSC.clase.ClaseProduse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucrarea1PSSC.clase.ClaseCos
{
    public class CosDeCumparaturi
    {
        private List<ProdusCos> produse_cos;
        private ProdusCos produs;

        public CosDeCumparaturi()
        {
            produse_cos = new List<ProdusCos>();
        }
        public CosDeCumparaturi(List<ProdusCos> produse_cos)
        {
            this.produse_cos = produse_cos;
        }

        public void AdaugaProdus(string Nume, List<Produs> produse_mag)
        {
            try { 
                if (string.IsNullOrWhiteSpace(Nume))
                {
                    throw new ArgumentException("Numele produsului nu poate fi gol");
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            var produss = produse_mag.FirstOrDefault(p => p.Nume == Nume);
            
            try
            {
                if (produss == null)
                {
                    throw new ArgumentException("Produsul nu exista in magazin");
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            produs = new ProdusCos(produss.CodProdus, produss.Nume, new UnitQuantity(1), produss.Kilogram, produss.Pret);
            
            try
            {
                if (produs.Cantitate.Cantitate <= 0)
                {
                    throw new ArgumentException("Produsul a fost epuizat");

                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

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
                }
            }
        }

        public void StergeProdus(string Nume, List<Produs> produse_mag)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Nume))
                {
                    throw new ArgumentException("Numele produsului nu poate fi gol");
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            var produs = produse_cos.FirstOrDefault(p => p.Nume == Nume);
            try
            {
                if(produs == null)
                {
                    throw new ArgumentException("Produsul nu exista in cos");
                }

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
                }
            }
        }
        public void GolesteCos(List<Produs> produse_mag)
        {
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
            double pret = 0;
            pret = (double)produs.Price.pret * produs.Cantitate.Cantitate * produs.Kilogram.CantitateKilogram;
            
            return pret;
        }
        public double TotalCos() {         
            double totalCos = 0;
            foreach (var produs in produse_cos)
            {
                totalCos += (double)produs.Price.pret * produs.Cantitate.Cantitate * produs.Kilogram.CantitateKilogram;
            }
            return totalCos;
        }

        // Add this public getter for serialization
        public List<ProdusCos> GetProduseCos()
        {
            return produse_cos;
        }
    }
}
