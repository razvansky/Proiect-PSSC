do
{
    Console.WriteLine("1.Creare cos cumparaturi");
    Console.WriteLine("2.Iesire");
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
            //CosCumparaturi cos = new CosCumparaturi();
           // cos.MeniuCosCumparaturi();
            break;
        case 2:
            Environment.Exit(0);
            break;
        default:
            Console.WriteLine("Optiune invalida");
            break;
    }

} while (true);  