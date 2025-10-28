using Lucrarea1PSSC.clase.ClaseCos;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lucrarea1PSSC.clase.Workflow
{
    /// <summary>
    /// Order state enumeration
    /// </summary>
    public enum StaraComanda
    {
        Plasata,        // Order placed
        InPregatire,    // Order being prepared
        Expediata,      // Order shipped
        Livrata,        // Order delivered
        Anulata         // Order cancelled
    }

    /// <summary>
    /// Order Aggregate Root
    /// Represents a placed order with delivery information
    /// INVARIANTS:
    /// - Order can only be created from paid carts
    /// - Delivery address must be valid
    /// - Order total is calculated at creation and immutable
    /// - Products in order cannot be modified after placement
    /// - Order state transitions follow business rules
    /// </summary>
    public class ComandaAggregate
    {
        private readonly Guid _comandaId;
        private readonly string _numeClient;
        private readonly Adress _adresaLivrare;
        private readonly List<ProdusCos> _produse;
        private readonly double _total;
        private readonly DateTime _dataPlasare;
        private StaraComanda _stare;

        private ComandaAggregate(
            Guid comandaId,
            string numeClient,
            Adress adresaLivrare,
            List<ProdusCos> produse,
            double total,
            DateTime dataPlasare)
        {
            _comandaId = comandaId;
            _numeClient = numeClient ?? throw new ArgumentNullException(nameof(numeClient));
            _adresaLivrare = adresaLivrare ?? throw new ArgumentNullException(nameof(adresaLivrare));
            _produse = new List<ProdusCos>(produse ?? throw new ArgumentNullException(nameof(produse)));
            _total = total;
            _dataPlasare = dataPlasare;
            _stare = StaraComanda.Plasata;

            // INVARIANT: Order must have at least one product
            if (_produse.Count == 0)
                throw new InvalidOperationException("Comanda trebuie s? con?in? cel pu?in un produs");

            // INVARIANT: Total must be positive
            if (_total <= 0)
                throw new InvalidOperationException("Totalul comenzii trebuie s? fie pozitiv");
        }

        /// <summary>
        /// Factory method to create order from paid cart
        /// Validates all business rules and invariants
        /// </summary>
        public static (bool Success, ComandaAggregate? Order, string? Error) CreateFromPaidCart(
            Persoana persoana,
            CosDeCumparaturi cos)
        {
            // Validate person
            if (persoana == null)
                return (false, null, "Persoana nu poate fi null");

            // Validate cart
            if (cos == null)
                return (false, null, "Co?ul nu poate fi null");

            // INVARIANT: Order can only be created from paid cart
            if (cos.GetStareCos() is not PayedCos)
                return (false, null, "Co?ul trebuie s? fie pl?tit pentru a plasa comanda");

            // Validate delivery address
            if (persoana.Adress == null || persoana.Adress.adress.Length < 5)
                return (false, null, "Adresa de livrare este invalid? (minim 5 caractere)");

            // Get products from cart
            var produse = cos.GetProduseCos();
            if (produse == null || produse.Count == 0)
                return (false, null, "Co?ul este gol");

            // Calculate total
            var total = cos.TotalCos();
            if (total <= 0)
                return (false, null, "Totalul comenzii trebuie s? fie pozitiv");

            // Create order
            try
            {
                var comanda = new ComandaAggregate(
                    Guid.NewGuid(),
                    persoana.Nume.Name,
                    persoana.Adress,
                    produse,
                    total,
                    DateTime.UtcNow
                );

                return (true, comanda, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Eroare la crearea comenzii: {ex.Message}");
            }
        }

        /// <summary>
        /// Convert order to success event
        /// </summary>
        public ComandaEvent.ComandaPlasataSuccessEvent ToSuccessEvent()
        {
            return new ComandaEvent.ComandaPlasataSuccessEvent(
                _numeClient,
                _total,
                _produse.Count,
                _comandaId
            );
        }

        /// <summary>
        /// Transition order to InPregatire state
        /// </summary>
        public void StartPreparation()
        {
            if (_stare != StaraComanda.Plasata)
                throw new InvalidOperationException($"Nu se poate începe preg?tirea din starea {_stare}");

            _stare = StaraComanda.InPregatire;
        }

        /// <summary>
        /// Transition order to Expediata state
        /// </summary>
        public void Ship()
        {
            if (_stare != StaraComanda.InPregatire)
                throw new InvalidOperationException($"Nu se poate expedia comanda din starea {_stare}");

            _stare = StaraComanda.Expediata;
        }

        /// <summary>
        /// Transition order to Livrata state
        /// </summary>
        public void Deliver()
        {
            if (_stare != StaraComanda.Expediata)
                throw new InvalidOperationException($"Nu se poate livra comanda din starea {_stare}");

            _stare = StaraComanda.Livrata;
        }

        /// <summary>
        /// Cancel order (only if not shipped or delivered)
        /// </summary>
        public void Cancel()
        {
            if (_stare == StaraComanda.Expediata || _stare == StaraComanda.Livrata)
                throw new InvalidOperationException($"Nu se poate anula comanda din starea {_stare}");

            _stare = StaraComanda.Anulata;
        }

        // Read-only properties
        public Guid ComandaId => _comandaId;
        public string NumeClient => _numeClient;
        public Adress AdresaLivrare => _adresaLivrare;
        public IReadOnlyList<ProdusCos> Produse => _produse.AsReadOnly();
        public double Total => _total;
        public DateTime DataPlasare => _dataPlasare;
        public StaraComanda Stare => _stare;
    }
}
