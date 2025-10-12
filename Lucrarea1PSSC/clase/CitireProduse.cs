using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Lucrarea1PSSC.clase
{
    public class CitireProduse
    {
        public static List<Produs> CitireProduseDinJson(string filePath)
        {
            var produse = new List<Produs>();
            try
            {
                var json = File.ReadAllText(filePath);
                var items = JsonSerializer.Deserialize<List<JsonProdus>>(json);

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        var quantity = new UnitQuantity(item.Cantitate); // double
                        var kilogram = new KilogramQuantity(item.CantitateKilogram); // double
                        var price = new Price(item.Pret);
                        produse.Add(new Produs(item.Nume, quantity, kilogram, price));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la citirea fisierului JSON: {ex.Message}");
            }
            return produse;
        }

        // Helper DTO for deserialization
        private class JsonProdus
        {
            public string Nume { get; set; }
            public double Cantitate { get; set; }
            public double CantitateKilogram { get; set; }
            public double Pret { get; set; }
        }
    }
}
