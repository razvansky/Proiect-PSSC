using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucrarea1PSSC
{
    internal class CosDeCumparaturi
    {
       private List<ProdusCos> produse_cos;
        public CosDeCumparaturi()
        {
            produse_cos = new List<ProdusCos>();
        }
        public void AdaugaProdus(ProdusCos produs)
        {
            produse_cos.Add(produs);
        }
        public void AfiseazaProduse()
        {
            foreach (var produs in produse_cos)
            {
                Console.WriteLine($"Produs: {produs.Nume}, Pret: {produs.Pret}, Cantitate: {produs.}");
            }
        }
        public double CalculeazaTotal()
        {
            return produse.Sum(p => p.Pret * p.Cantitate);
        }
    }
}
