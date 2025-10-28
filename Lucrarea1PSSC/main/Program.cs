using Lucrarea1PSSC.clase.ClaseCos;
using Lucrarea1PSSC.clase.ClaseGestionareFisiere;
using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using Lucrarea1PSSC.clase.Infrastructure;
using Lucrarea1PSSC.clase.Workflow;

internal class Program
{
    private static void Main(string[] args)
    {
        // Setup event bus subscriptions
        SetupEventBusSubscriptions();

        List<Produs> produse = CitireFisiere.CitireProduseDinJson(@"..\..\..\resources\produsemagazin.json");
        List<Persoana> persoane = CitireFisiere.CitirePersoaneDinJson(@"..\..\..\resources\persoane.json");
        CosDeCumparaturi cos = new CosDeCumparaturi(1);
        string numeProdus;
        
        Console.WriteLine("=== E-Commerce Shopping System ===");
        Console.WriteLine("Event-Driven Architecture with DDD\n");
        
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
            Console.WriteLine("\n=== MENU ===");
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
            Console.Write("Alegeti optiunea: ");
            
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
                    // CREATE CART WITH COMMANDS AND EVENTS
                    Console.WriteLine("Introduceti numele persoanei:");
                    string numePersoana = Console.ReadLine();
                    
                    // Use command with built-in validation
                    var (success, command, error) = 
                        Lucrarea1PSSC.clase.ClaseCos.Commands.CreateCartCommand.TryCreate(numePersoana);
                    
                    if (!success)
                    {
                        Console.WriteLine($"Eroare: {error}");
                        break;
                    }

                    var persoana = persoane.FirstOrDefault(p => p.Nume.Name == command.NumePersoana);
                    if (persoana == null)
                    {
                        Console.WriteLine("Persoana nu exista");
                        break;
                    }

                    // Create cart
                    cos = new CosDeCumparaturi();
                    Console.WriteLine("Cos creat cu succes");

                    // Update person
                    var updatedPersoana = persoana.AdaugaCos(cos);
                    persoane[persoane.IndexOf(persoana)] = updatedPersoana;

                    // Publish domain event
                    EventBus.Publish(new CartEvents.CosCreatEvent(
                        command.NumePersoana, 
                        DateTime.UtcNow
                    ));

                    Console.WriteLine($"Cos adaugat {command.NumePersoana} cu succes");
                    break;

                case 2:
                    // ADD PRODUCT WITH COMMANDS AND EVENTS
                    Console.WriteLine("Introduceti numele produsului:");
                    numeProdus = Console.ReadLine();

                    if (cos == null)
                    {
                        Console.WriteLine("Cosul nu a fost creat");
                        break;
                    }

                    // Use command with validation
                    var (addSuccess, addCommand, addError) = 
                        Lucrarea1PSSC.clase.ClaseCos.Commands.AddProductToCartCommand.TryCreate(numeProdus);

                    if (!addSuccess)
                    {
                        Console.WriteLine($"Eroare: {addError}");
                        break;
                    }

                    // Execute domain logic
                    var produsToAdd = produse.FirstOrDefault(p => p.Nume == addCommand.NumeProdus);
                    if (produsToAdd != null)
                    {
                        cos.AdaugaProdus(addCommand.NumeProdus, produse);
                        
                        // Publish event
                        EventBus.Publish(new CartEvents.ProdusAdaugatInCosEvent(
                            produsToAdd.Nume,
                            produsToAdd.CodProdus.Cod,
                            1,
                            produsToAdd.Pret.pret,
                            DateTime.UtcNow
                        ));
                    }
                    break;

                case 3:
                    // REMOVE PRODUCT WITH COMMANDS AND EVENTS
                    Console.WriteLine("Introduceti numele produsului:");
                    numeProdus = Console.ReadLine();
                    
                    if (cos == null)
                    {
                        Console.WriteLine("Cosul nu a fost creat");
                        break;
                    }

                    var (removeSuccess, removeCommand, removeError) = 
                        Lucrarea1PSSC.clase.ClaseCos.Commands.RemoveProductFromCartCommand.TryCreate(numeProdus);

                    if (!removeSuccess)
                    {
                        Console.WriteLine($"Eroare: {removeError}");
                        break;
                    }

                    var produsInCos = cos.GetProduseCos()?.FirstOrDefault(p => p.Nume == removeCommand.NumeProdus);
                    if (produsInCos != null)
                    {
                        cos.StergeProdus(removeCommand.NumeProdus, produse);
                        
                        // Publish event
                        EventBus.Publish(new CartEvents.ProdusStergeDinCosEvent(
                            produsInCos.Nume,
                            produsInCos.CodProd.Cod,
                            produsInCos.Cantitate.Cantitate,
                            DateTime.UtcNow
                        ));
                    }
                    break;

                case 4:
                    // EMPTY CART WITH COMMANDS AND EVENTS
                    if (cos == null)
                    {
                        Console.WriteLine("Cosul nu a fost creat");
                        break;
                    }

                    var emptyCommand = Lucrarea1PSSC.clase.ClaseCos.Commands.EmptyCartCommand.Create();
                    
                    var produseInCos = cos.GetProduseCos()?.Select(p => 
                        (p.Nume, p.CodProd.Cod, p.Cantitate.Cantitate)
                    ).ToList();

                    cos.GolesteCos(produse);
                    
                    if (produseInCos != null && produseInCos.Count > 0)
                    {
                        // Publish event
                        EventBus.Publish(new CartEvents.CosGolitEvent(
                            produseInCos,
                            DateTime.UtcNow
                        ));
                    }
                    break;

                case 5:
                    if (cos.GetStareCos() is UnvalidatedCos)
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
                    Console.WriteLine("\n=== PRODUSE MAGAZIN ===");
                    foreach (var produs in produse)
                    {
                        Console.WriteLine($"Produs: {produs.Nume}, Cantitate unitati: {produs.Quantity}, Cantitate kg: {produs.Kilogram}, Pret: {produs.Pret}");
                    }
                    break;

                case 8:
                    Console.WriteLine("\n=== PERSOANE ===");
                    foreach (var pers in persoane)
                    {
                        Console.WriteLine($"Persoana: {pers.Nume}, Email: {pers.Email}, Adresa: {pers.Adress}");
                        Console.WriteLine("Cosuri:");
                        foreach (var xcos in pers.Cosuri)
                        {
                            Console.WriteLine($"Cos nr: {pers.Cosuri.IndexOf(xcos) + 1}:");
                            xcos.AfiseazaProduse();
                            Console.WriteLine($"Total cos: {xcos.TotalCos()}");
                        }
                        Console.WriteLine();
                    }
                    break;

                case 9:
                    // PAY CART WITH COMMANDS AND EVENTS
                    if (cos == null)
                    {
                        Console.WriteLine("Cosul nu a fost creat");
                        break;
                    }

                    var payCommand = Lucrarea1PSSC.clase.ClaseCos.Commands.PayCartCommand.Create();

                    // Execute payment
                    bool paymentSuccess = cos.platesteCos();

                    if (paymentSuccess)
                    {
                        // Find current person
                        var currentPerson = persoane.FirstOrDefault(p => p.CosCurent == cos);
                        if (currentPerson != null)
                        {
                            // Publish payment event
                            EventBus.Publish(new CartEvents.CosPlatitEvent(
                                currentPerson.Nume.Name,
                                cos.TotalCos(),
                                cos.GetProduseCos().Count,
                                DateTime.UtcNow
                            ));
                        }
                    }
                    break;

                case 10:
                    // PLACE ORDER WITH AGGREGATE AND COMMANDS
                    Console.WriteLine("Introduceti numele persoanei:");
                    string numePersoanaComanda = Console.ReadLine();
                    
                    var persoanaComanda = persoane.FirstOrDefault(p => p.Nume.Name == numePersoanaComanda);
                    if (persoanaComanda == null)
                    {
                        Console.WriteLine("Persoana nu exista");
                        break;
                    }

                    if (persoanaComanda.CosCurent == null)
                    {
                        Console.WriteLine("Persoana nu are cos curent");
                        break;
                    }

                    // Use PlaceOrderCommand for validation
                    var (cmdSuccess, placeOrderCmd, cmdError) = 
                        Lucrarea1PSSC.clase.Workflow.Commands.PlaceOrderCommand.TryCreate(
                            persoanaComanda, 
                            persoanaComanda.CosCurent
                        );

                    if (!cmdSuccess)
                    {
                        Console.WriteLine($"Eroare validare: {cmdError}");
                        break;
                    }

                    // Use ComandaAggregate to create order with invariants
                    var (orderSuccess, order, orderError) = 
                        ComandaAggregate.CreateFromPaidCart(
                            persoanaComanda, 
                            persoanaComanda.CosCurent
                        );

                    if (orderSuccess)
                    {
                        // Order created successfully
                        var successEvent = order.ToSuccessEvent();
                        
                        Console.WriteLine(successEvent.Message);
                        Console.WriteLine($"Order ID: {order.ComandaId}");
                        Console.WriteLine($"Total: {order.Total} lei");
                        Console.WriteLine($"Produse: {order.Produse.Count}");
                        Console.WriteLine($"Stare: {order.Stare}");
                        Console.WriteLine($"Data plasare: {order.DataPlasare:dd/MM/yyyy HH:mm:ss}");

                        // Publish success event
                        EventBus.Publish(successEvent);
                    }
                    else
                    {
                        // Order creation failed
                        var failedEvent = new ComandaEvent.ComandaPlasataFailedEvent(orderError);
                        Console.WriteLine(failedEvent.Message);
                        
                        // Publish failure event
                        EventBus.Publish(failedEvent);
                    }
                    break;

                case 0:
                    SalvareFisiere.SalvareProduseInJson(@"..\..\..\resources\produsemagazin.json", produse);
                    SalvareFisiere.SalvarePersoaneInJson(@"..\..\..\resources\persoane.json", persoane);
                    Console.WriteLine("Date salvate cu succes!");
                    Environment.Exit(0);
                    break;

                default:
                    Console.WriteLine("Optiune invalida");
                    break;
            }

        } while (true);
    }

    /// <summary>
    /// Setup EventBus subscriptions for cross-context communication
    /// </summary>
    private static void SetupEventBusSubscriptions()
    {
        Console.WriteLine("[EVENTBUS] Setting up event subscriptions...\n");

        // Inventory subscribes to cart events
        EventBus.Subscribe<CartEvents.ProdusAdaugatInCosEvent>(evt =>
        {
            Console.WriteLine($"[EVENT] Product added to cart: {evt.NumeProdus}, Stock decreased");
        });

        EventBus.Subscribe<CartEvents.ProdusStergeDinCosEvent>(evt =>
        {
            Console.WriteLine($"[EVENT] Product removed from cart: {evt.NumeProdus}, Stock increased");
        });

        EventBus.Subscribe<CartEvents.CosGolitEvent>(evt =>
        {
            Console.WriteLine($"[EVENT] Cart emptied, {evt.ProduseReturnate.Count} products returned to stock");
        });

        EventBus.Subscribe<CartEvents.CosPlatitEvent>(evt =>
        {
            Console.WriteLine($"[EVENT] Cart paid by {evt.NumePersoana}, Total: {evt.TotalPlatit} lei");
        });

        // Order management subscribes to cart payment
        EventBus.Subscribe<CartEvents.CosCreatEvent>(evt =>
        {
            Console.WriteLine($"[EVENT] New cart created for {evt.NumePersoana}");
        });

        // Inventory subscribes to stock events
        EventBus.Subscribe<InventoryEvents.ProdusEpuizatEvent>(evt =>
        {
            Console.WriteLine($"[WARNING] Product {evt.NumeProdus} is OUT OF STOCK!");
        });

        EventBus.Subscribe<InventoryEvents.ProdusDisponibilEvent>(evt =>
        {
            Console.WriteLine($"[INFO] Product {evt.NumeProdus} is now AVAILABLE (Stock: {evt.StocDisponibil})");
        });

        // Order events
        EventBus.Subscribe<ComandaEvent.ComandaPlasataSuccessEvent>(evt =>
        {
            Console.WriteLine("[EVENT] Order placed successfully: ");
            Console.WriteLine($"  - Order ID: {evt.ComandaId}");
            Console.WriteLine($"  - Total: {evt.TotalComanda} lei");
            Console.WriteLine($"  - Numar produse: {evt.NumarProduse}");
        });

        EventBus.Subscribe<ComandaEvent.ComandaPlasataFailedEvent>(evt =>
        {
            Console.WriteLine($"[EVENT] Order placement failed: {evt.Reason}");
        });

        Console.WriteLine("[EVENTBUS] Event subscriptions configured!\n");
    }
}

