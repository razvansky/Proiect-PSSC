using Lucrarea1PSSC.clase.ClaseCos;
using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;

public class ComandaEvent
{
    public bool Success { get; }
    public string Message { get; }

    public ComandaEvent(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}

public static class PlasareComandaWorkflow
{
    public static ComandaEvent PlaseazaComanda(Persoana persoana, CosDeCumparaturi cosx)
    {
        // 1. Validare date intrare
        if (persoana == null || cosx == null )
            return new ComandaEvent(false, "Datele de intrare nu sunt valide.");

        // 4. Verificare adresa livrare
        if (persoana.Adress.adress.Length < 5) // exemplu simplu
            return new ComandaEvent(false, "Adresa de livrare invalida.");

     switch(cosx.GetStareCos())
        {
            case UnvalidatedCos:
                return new ComandaEvent(false, "Cosul este invalid, generati un cos nou");
            case EmptyCos:
                return new ComandaEvent(false, "Cosul este gol, adaugati produse in cos");
            case ValidatedCos:
                return new ComandaEvent(false, "Cosul nu este platit, platiti cosul inainte de a plasa comanda");
                break;
            case PayedCos:
                // 5. Procesare comanda
                return new ComandaEvent(true, "Comanda a fost plasata cu succes.");
            default:
                return new ComandaEvent(false, "Stare cos necunoscuta.");
        }

    
          

        // 6. Finalizare workflow
        // (aici poți adăuga produsul în cos, scădea stocul etc.)

      
      
    }
}