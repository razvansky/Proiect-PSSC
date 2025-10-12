using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucrarea1PSSC.clase.ClaseGestionarePersoane
{
    public record Persoana(Nume Nume, EmailP Email, Adress Adress) : IPersoana
    {
    }
}
