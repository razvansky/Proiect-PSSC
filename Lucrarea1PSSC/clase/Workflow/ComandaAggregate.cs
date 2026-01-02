using Lucrarea1PSSC.clase.ClaseCos;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using Lucrarea1PSSC.clase.Infrastructure;
using Lucrarea1PSSC.clase.Workflow.ValueObjects;
using System;
using System.Collections.Generic;

namespace Lucrarea1PSSC.clase.Workflow
{
    /// <summary>
    /// Order state enumeration
    /// </summary>
    public enum StaraComanda
    {
        Plasata,
        InPregatire,
        Expediata,
        Livrata,
        Anulata
    }

    public class ComandaAggregate
    {
        private readonly Guid _comandaId;
        private readonly string _numeClient;
        private readonly Adress _adresaLivrare;
        private readonly List<ProdusCos> _produse;
        private readonly Money _total;
        private readonly DateTime _dataPlasare;
        private StaraComanda _stare;

        private readonly List<StateTransition> _stateHistory = new();

        private ComandaAggregate(
            Guid comandaId,
            string numeClient,
            Adress adresaLivrare,
            List<ProdusCos> produse,
            Money total,
            DateTime dataPlasare)
        {
            _comandaId = comandaId;
            _numeClient = numeClient ?? throw new ArgumentNullException(nameof(numeClient));
            _adresaLivrare = adresaLivrare ?? throw new ArgumentNullException(nameof(adresaLivrare));
            _produse = new List<ProdusCos>(produse ?? throw new ArgumentNullException(nameof(produse)));
            _total = total ?? throw new ArgumentNullException(nameof(total));
            _dataPlasare = dataPlasare;
            _stare = StaraComanda.Plasata;

            if (_produse.Count == 0)
                throw new InvalidOperationException("Comanda trebuie sa contina cel putin un produs");

            if (_total.Amount <= 0)
                throw new InvalidOperationException("Totalul comenzii trebuie sa fie pozitiv");

            RecordStateTransition(StaraComanda.Plasata, "Order created");
        }

        public static Result<ComandaAggregate> CreateFromPaidCart(Persoana persoana, CosDeCumparaturi cos)
        {
            if (persoana == null)
                return DomainError.ValidationFailed("Persoana nu poate fi null");

            if (cos == null)
                return DomainError.ValidationFailed("Cosul nu poate fi null");

            if (cos.GetStareCos() is not PayedCos)
                return DomainError.InvalidState(
                    cos.GetStareCos().GetType().Name,
                    "create order - cart must be paid");

            if (persoana.Adress == null || persoana.Adress.adress.Length < 5)
                return DomainError.ValidationFailed("Adresa de livrare este invalida (minim 5 caractere)");

            var produse = cos.GetProduseCos();
            if (produse == null || produse.Count == 0)
                return DomainError.BusinessRuleViolation("Cosul este gol", "Order must contain at least one product");

            var totalDouble = cos.TotalCos();
            if (totalDouble <= 0)
                return DomainError.InvariantViolation(
                    "Totalul comenzii trebuie sa fie pozitiv",
                    $"Calculated total: {totalDouble}");

            Money total;
            try
            {
                total = Money.FromDouble(totalDouble, "RON");
            }
            catch (Exception ex)
            {
                return DomainError.InvariantViolation("Total invalid", ex.Message);
            }

            try
            {
                var comanda = new ComandaAggregate(
                    Guid.NewGuid(),
                    persoana.Nume.Name,
                    persoana.Adress,
                    produse,
                    total,
                    DateTime.UtcNow);

                return comanda;
            }
            catch (Exception ex)
            {
                return DomainError.Create("ORDER_CREATION_FAILED", "Eroare la crearea comenzii", ex.Message);
            }
        }

        [Obsolete("Use CreateFromPaidCart returning Result<T> instead")]
        public static (bool Success, ComandaAggregate? Order, string? Error) CreateFromPaidCartLegacy(
            Persoana persoana,
            CosDeCumparaturi cos)
        {
            var result = CreateFromPaidCart(persoana, cos);

            return result switch
            {
                Result<ComandaAggregate>.Success success => (true, success.Value, null),
                Result<ComandaAggregate>.Failure failure => (false, null, failure.Error.Message),
                _ => (false, null, "Unknown error")
            };
        }

        public ComandaEvent.ComandaPlasataSuccessEvent ToSuccessEvent()
        {
            return new ComandaEvent.ComandaPlasataSuccessEvent(
                _numeClient,
                _total.ToDouble(),
                _produse.Count,
                _comandaId
            );
        }

        public Result<Unit> StartPreparation(string operatorName)
        {
            if (_stare != StaraComanda.Plasata)
                return DomainError.InvalidState(_stare.ToString(), "start preparation - order must be in Plasata state");

            if (string.IsNullOrWhiteSpace(operatorName))
                return DomainError.ValidationFailed("Operator name is required");

            _stare = StaraComanda.InPregatire;
            RecordStateTransition(StaraComanda.InPregatire, $"Preparation started by {operatorName}");

            return Unit.Default;
        }

        [Obsolete("Use StartPreparation(operatorName) returning Result<Unit>")]
        public void StartPreparation()
        {
            var result = StartPreparation("unknown");
            if (result.IsFailure)
                throw new InvalidOperationException(result.GetErrorOrThrow().Message);
        }

        public Result<Unit> Ship(string trackingNumber, string carrierName)
        {
            if (_stare != StaraComanda.InPregatire)
                return DomainError.InvalidState(_stare.ToString(), "ship - order must be in InPregatire state");

            if (string.IsNullOrWhiteSpace(trackingNumber))
                return DomainError.ValidationFailed("Tracking number is required");

            if (string.IsNullOrWhiteSpace(carrierName))
                return DomainError.ValidationFailed("Carrier name is required");

            _stare = StaraComanda.Expediata;
            RecordStateTransition(StaraComanda.Expediata, $"Shipped via {carrierName}, tracking: {trackingNumber}");

            return Unit.Default;
        }

        [Obsolete("Use Ship(trackingNumber, carrierName) returning Result<Unit>")]
        public void Ship()
        {
            var result = Ship("UNKNOWN", "UNKNOWN");
            if (result.IsFailure)
                throw new InvalidOperationException(result.GetErrorOrThrow().Message);
        }

        public Result<Unit> Deliver(string receivedBy)
        {
            if (_stare != StaraComanda.Expediata)
                return DomainError.InvalidState(_stare.ToString(), "deliver - order must be in Expediata state");

            if (string.IsNullOrWhiteSpace(receivedBy))
                return DomainError.ValidationFailed("Receiver name is required");

            _stare = StaraComanda.Livrata;
            RecordStateTransition(StaraComanda.Livrata, $"Delivered to {receivedBy}");

            return Unit.Default;
        }

        [Obsolete("Use Deliver(receivedBy) returning Result<Unit>")]
        public void Deliver()
        {
            var result = Deliver("UNKNOWN");
            if (result.IsFailure)
                throw new InvalidOperationException(result.GetErrorOrThrow().Message);
        }

        public Result<Unit> Cancel(string reason, string? cancelledBy = null)
        {
            if (_stare == StaraComanda.Expediata || _stare == StaraComanda.Livrata)
                return DomainError.BusinessRuleViolation(
                    $"Nu se poate anula comanda din starea {_stare}",
                    "Orders can only be cancelled before shipping");

            if (string.IsNullOrWhiteSpace(reason))
                return DomainError.ValidationFailed("Cancellation reason is required");

            _stare = StaraComanda.Anulata;
            var details = cancelledBy != null ? $"Cancelled by {cancelledBy}: {reason}" : $"Cancelled: {reason}";
            RecordStateTransition(StaraComanda.Anulata, details);

            return Unit.Default;
        }

        [Obsolete("Use Cancel(reason, cancelledBy) returning Result<Unit>")]
        public void Cancel()
        {
            var result = Cancel("No reason provided");
            if (result.IsFailure)
                throw new InvalidOperationException(result.GetErrorOrThrow().Message);
        }

        private void RecordStateTransition(StaraComanda newState, string reason)
        {
            _stateHistory.Add(new StateTransition(DateTime.UtcNow, newState, reason));
        }

        public Guid ComandaId => _comandaId;
        public string NumeClient => _numeClient;
        public Adress AdresaLivrare => _adresaLivrare;
        public IReadOnlyList<ProdusCos> Produse => _produse.AsReadOnly();
        public Money Total => _total;
        public DateTime DataPlasare => _dataPlasare;
        public StaraComanda Stare => _stare;
        public IReadOnlyList<StateTransition> StateHistory => _stateHistory.AsReadOnly();

        public TimeSpan EstimatedPreparationTime
        {
            get
            {
                var baseMinutes = 5;
                var productMinutes = _produse.Count * 2;
                var highValueMinutes = _total.Amount > 1000 ? 5 : 0;
                var bulkMinutes = _produse.Count > 5 ? 10 : 0;

                return TimeSpan.FromMinutes(baseMinutes + productMinutes + highValueMinutes + bulkMinutes);
            }
        }
    }

    public sealed record StateTransition(DateTime Timestamp, StaraComanda NewState, string Reason);
}
