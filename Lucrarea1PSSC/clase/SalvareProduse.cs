using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Lucrarea1PSSC.clase
{
    public class SalvareProduse
    {
   
        public static void SalvareProduseInJson(string filePath, List<Produs> produse)
        {
            try
            {
                var jsonList = new List<JsonProdus>();
                foreach (var produs in produse)
                {
                    jsonList.Add(new JsonProdus
                    {
                        Nume = produs.Nume,
                        Cantitate = produs.Quantity.Cantitate,
                        CantitateKilogram = produs.Kilogram.CantitateKilogram,
                        Pret = produs.Pret.pret
                    });
                }
                var json = JsonSerializer.Serialize(jsonList, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la salvarea fisierului JSON: {ex.Message}");
            }
        }

        private class JsonProdus
        {
            public string Nume { get; set; }
            public double Cantitate { get; set; }
            public double CantitateKilogram { get; set; }
            public double Pret { get; set; }
        }
    }
}
