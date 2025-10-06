using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lucrarea1PSSC.clase
{
    public class SalvareProduse
    {
        public static void SalvareProduseInFisier(string filePath, List<Produs> produse)
        {
            try
            {
                var lines = new List<string>();
                foreach (var produs in produse)
                {
                    // Use .Value to get the numeric value for each property
                    string line = $"{produs.Nume} {produs.Quantity.Cantitate} {produs.Kilogram.CantitateKilogram} {produs.Pret.pret}";
                    lines.Add(line);
                }
                File.WriteAllLines(filePath, lines, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la salvarea fisierului: {ex.Message}");
            }
        }
    }
}
