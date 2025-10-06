using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucrarea1PSSC.clase
{
    public class CitireProduse
    {
        public static List<Produs> CitireProduseDinFisier(string filePath)
        {
            var produse = new List<Produs>();
            try
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(' ');
                    if (parts.Length == 4 &&
                        !string.IsNullOrWhiteSpace(parts[0]) &&
                        int.TryParse(parts[1], out int unitQty) &&
                        double.TryParse(parts[2], out double kgQty) &&
                        double.TryParse(parts[3], out double pret))
                    {
                        var nume = parts[0].Trim();
                        var quantity = new UnitQuantity(unitQty);
                        var kilogram = new KilogramQuantity(kgQty);
                        var price = new Price(pret); 
                        produse.Add(new Produs(nume, quantity, kilogram, price));
                    }
                    else
                    {
                        Console.WriteLine($"Linie invalida in fisier: {line}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la citirea fisierului: {ex.Message}");
            }
            return produse;
        }

    }
}
