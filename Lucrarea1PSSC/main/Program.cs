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
        CosDeCumparaturi cos = new CosDeCumparaturi(1);
        string numeProdus;
        foreach (var produs in produse)
        {
            Console.WriteLine($"Cod Produs: {produs.CodProdus.Cod} Produs: {produs.Nume}, Cantitate unitati: {produs.Quantity.Cantitate}, Cantitate kg: {produs.Kilogram.CantitateKilogram}, Pret: {produs.Pret.pret}");
        }
        foreach (var persoana in persoane)
        {
            Console.WriteLine($"Persoana: {persoana.Nume.Name}");
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
            Console.WriteLine("9.Plateste cos");
            Console.WriteLine("10.Plaseaza comanda");
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
                        Console.WriteLine("Introduceti numele persoanei:");
                        string numePersoana = Console.ReadLine();
                        if(string.IsNullOrWhiteSpace(numePersoana))
                        {
                            throw new ArgumentException("Numele persoanei nu poate fi gol");
                        }
                          var persoana = persoane.FirstOrDefault(p => p.Nume.Name == numePersoana);
                        if(persoana==null)
                        {
                            throw new InvalidOperationException("Persoana nu exista");
                        }
                       
                            cos = new CosDeCumparaturi();
                       Console.WriteLine("Cos creat cu succes");
                        persoane[persoane.IndexOf(persoana)] = persoana.AdaugaCos(cos);
                        Console.WriteLine($"Cos adaugat {numePersoana} cu succes");
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
                    if(cos.GetStareCos() is UnvalidatedCos)
                    {
                        Console.WriteLine("Cosul este invalid, generati un cos nou");
                        break;
                    }
                    Console.WriteLine($"Stare cos: {cos.GetStareCos()}");
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
                                Console.WriteLine($"Cos nr: {persoana.Cosuri.IndexOf(xcos) + 1}:");
                            xcos.AfiseazaProduse(); // This will print the products in the cart
                                Console.WriteLine($"Total cos: {xcos.TotalCos()}");
                            }
                            Console.WriteLine();
                        }
                    break;
                case 9:
                    cos.platesteCos();
                    break;
                case 10:
                    Console.WriteLine("Introduceti numele persoanei:");
                    string numePersoanaComanda = Console.ReadLine();
                    var persoanaComanda = persoane.FirstOrDefault(p => p.Nume.Name == numePersoanaComanda);
                    if(persoanaComanda==null)
                    {
                        Console.WriteLine("Persoana nu exista");
                        break;
                    }
                    if (persoanaComanda.CosCurent == null)
                    {
                        Console.WriteLine("Persoana nu are cos curent");
                        break;
                    }
                    var eventres = PlasareComandaWorkflow.PlaseazaComanda(persoanaComanda, persoanaComanda.CosCurent);
                    Console.WriteLine(eventres.Message);
                  
                        break;
                case 0:
                    SalvareFisiere.SalvareProduseInJson(@"..\..\..\resources\produsemagazin.json", produse);
                    SalvareFisiere.SalvarePersoaneInJson(@"..\..\..\resources\persoane.json", persoane);
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Optiune invalida");
                    break;
            }

        } while (true);
    }
}

