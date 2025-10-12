using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using Lucrarea1PSSC.clase.ClaseCos;
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

                        // Map carts
                        var cosuri = new List<CosDeCumparaturi>();
                        if (item.cosuri != null)
                        {
                            foreach (var cosJson in item.cosuri)
                            {
                                var produseCos = new List<ProdusCos>();
                                if (cosJson.produse_cos != null)
                                {
                                    foreach (var prodJson in cosJson.produse_cos)
                                    {
                                        var produsCos = new ProdusCos(
                                            prodJson.nume,
                                            new UnitQuantity(prodJson.cantitate),
                                            new KilogramQuantity(prodJson.kilogram),
                                            new Price(prodJson.pret)
                                        );
                                        produseCos.Add(produsCos);
                                    }
                                }
                                var cos = new CosDeCumparaturi(produseCos);
                                cosuri.Add(cos);
                            }
                        }

                        persoane.Add(new Persoana(nume, email, adresa, cosuri));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la citirea fisierului JSON persoane: {ex.Message}");
            }
            return persoane;
        }

        // Helper DTOs for deserialization
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
            public List<JsonCos> cosuri { get; set; }
        }

        private class JsonCos
        {
            public List<JsonProdusCos> produse_cos { get; set; }
        }

        private class JsonProdusCos
        {
            public string nume { get; set; }
            public double cantitate { get; set; }
            public double kilogram { get; set; }
            public double pret { get; set; }
        }
    }
}
