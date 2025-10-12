using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Lucrarea1PSSC.clase.ClaseGestionareFisiere
{
    public class CitireFisiere
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

        public static List<Persoana> CitirePersoaneDinJson(string filePath)
        {
            var persoane = new List<Persoana>();
            try
            {
                var json = File.ReadAllText(filePath);
                var items = JsonSerializer.Deserialize<List<JsonPersoana>>(json);

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        var nume = new Nume(item.nume);
                        var email = new EmailP(item.email);
                        var adresa = new Adress(item.adresa);
                        persoane.Add(new Persoana(nume, email, adresa));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la citirea fisierului JSON persoane: {ex.Message}");
            }
            return persoane;
        }

        // Helper DTO for deserialization
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
