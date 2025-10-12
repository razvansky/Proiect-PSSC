using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucrarea1PSSC.clase.ClaseProduse
{
   public record Produs(string Nume,UnitQuantity Quantity,KilogramQuantity Kilogram, Price Pret) : IProdus
    {
    }
}
