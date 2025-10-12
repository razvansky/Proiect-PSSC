using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Lucrarea1PSSC.clase.ClaseGestionareFisiere
{
    public class SalvareFisiere
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

        public static void SalvarePersoaneInJson(string filePath, List<Persoana> persoane)
        {
            try
            {
                var jsonList = new List<JsonPersoana>();
                int id = 1;
                foreach (var persoana in persoane)
                {
                    jsonList.Add(new JsonPersoana
                    {
                        id = id++,
                        nume = persoana.Nume.Name,
                        email = persoana.Email.email,
                        adresa = persoana.Adress.adress
                    });
                }
                var json = JsonSerializer.Serialize(jsonList, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la salvarea fisierului JSON persoane: {ex.Message}");
            }
        }

        private class JsonProdus
        {
            public string Nume { get; set; }
            public double Cantitate { get; set; }
            public double CantitateKilogram { get; set; }
            public double Pret { get; set; }
        }

        private class JsonPersoana
        {
            public int id { get; set; }
            public string nume { get; set; }
            public string email { get; set; }
            public string adresa { get; set; }
        }
    }
}
