using Lucrarea1PSSC.clase.ClaseProduse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucrarea1PSSC.clase.ClaseCos
{
   public record ProdusCos(string Nume, UnitQuantity Cantitate, KilogramQuantity Kilogram,Price Price) : IProdus
    {
    }
}
