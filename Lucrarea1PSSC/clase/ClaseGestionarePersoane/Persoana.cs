using Lucrarea1PSSC.clase.ClaseCos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Lucrarea1PSSC.clase.ClaseGestionarePersoane
{
    public record Persoana( Nume Nume, EmailP Email,Adress Adress,List<CosDeCumparaturi> Cosuri ) : IPersoana
    {
   
        public CosDeCumparaturi? CosCurent => Cosuri?.LastOrDefault();

       
        public IEnumerable<CosDeCumparaturi> CosuriVechi => Cosuri?.Take(Cosuri.Count - 1) ?? Enumerable.Empty<CosDeCumparaturi>();

       
        public Persoana AdaugaCos(CosDeCumparaturi cosNou)
        {
            var cosuriNoi = new List<CosDeCumparaturi>(Cosuri ?? new List<CosDeCumparaturi>());
            cosuriNoi.Add(cosNou);
            return this with { Cosuri = cosuriNoi };
        }
    }
}
