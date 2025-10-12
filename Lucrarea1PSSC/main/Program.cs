using Lucrarea1PSSC.clase.ClaseCos;
using Lucrarea1PSSC.clase.ClaseGestionareFisiere;
using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;

internal class Program
{
    private static void Main(string[] args)
    {
        List<Produs> produse = CitireFisiere.CitireProduseDinJson(@"..\..\..\resources\produsemagazin.json");
        List<Persoana> persoane = CitireFisiere.CitirePersoaneDinJson(@"..\..\..\resources\persoane.json");
        CosDeCumparaturi cos = null;
        string numeProdus;
        foreach (var produs in produse)
        {
            Console.WriteLine($"Produs: {produs.Nume}, Cantitate unitati: {produs.Quantity}, Cantitate kg: {produs.Kilogram}, Pret: {produs.Pret}");
        }

        do
        {
            Console.WriteLine("1.Creare cos cumparaturi");
            Console.WriteLine("2.Adauga in cos");
            Console.WriteLine("3.Sterge din cos");
            Console.WriteLine("4.Goleste cos");
            Console.WriteLine("5.Afiseaza cos");
            Console.WriteLine("6.Total de plata");
            Console.WriteLine("7.Afiseaza produse magazin");
            Console.WriteLine("8.Afiseaza persoane");
            Console.WriteLine("0.Iesire");
            string input = Console.ReadLine();
            if (!int.TryParse(input, out int optiune))
            {
                Console.WriteLine("Optiune invalida. Introduceti un numar.");
                Console.ReadKey();
                continue;
            }

            switch (optiune)
            {
                case 1:
                    try {
                       
                       
                    }
                    catch(InvalidOperationException ex)
                    {
                        Console.WriteLine(ex.Message);
                        break;
                    }
                   
                    break;
                case 2:
                    numeProdus=Console.ReadLine();
                    try
                    {
                        if(cos==null)
                        {
                            throw new NullReferenceException("Cosul nu a fost creat");
                        }
                    }
                    catch(NullReferenceException ex)
                    {
                        Console.WriteLine(ex.Message);
                        break;
                    }
                    cos.AdaugaProdus(numeProdus,produse);
                    break;
                case 3:
                    numeProdus = Console.ReadLine();
                    cos.StergeProdus(numeProdus,produse);
                    break;

                case 4: 
                    cos.GolesteCos(produse);
                    break;

                case 5: 
                    cos.AfiseazaProduse();
                    break;

                case 6: 
                    Console.WriteLine($"Total de plata: {cos.TotalCos()}");
                    break;

                case 7:
                        foreach (var produs in produse)
                        {
                            Console.WriteLine($"Produs: {produs.Nume}, Cantitate unitati: {produs.Quantity}, Cantitate kg: {produs.Kilogram}, Pret: {produs.Pret}");
                        }
                    break;
                case 8:
                     foreach (var persoana in persoane)
                        {
                            Console.WriteLine($"Persoana: {persoana.Nume}, Email: {persoana.Email}, Adresa: {persoana.Adress}");
                            Console.WriteLine("Cosuri:");
                            foreach (var xcos in persoana.Cosuri)
                            {
                                xcos.AfiseazaProduse(); // This will print the products in the cart
                                Console.WriteLine($"Total cos: {xcos.TotalCos()}");
                            }
                            Console.WriteLine();
                        }
                    break;
                case 0:
                    SalvareFisiere.SalvareProduseInJson(@"..\..\..\resources\produsemagazin.json", produse);
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Optiune invalida");
                    break;
            }

        } while (true);
    }
}

