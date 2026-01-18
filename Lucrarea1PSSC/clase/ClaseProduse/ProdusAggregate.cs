using System;

namespace Lucrarea1PSSC.clase.ClaseProduse
{
    /// <summary>
    /// Product Aggregate Root
    /// Maintains product invariants including stock levels
    /// INVARIANTS:
    /// - Stock cannot be negative
    /// - Price must be positive
    /// - Product code is unique and immutable
    /// - Stock operations are atomic
    /// </summary>
    public class ProdusAggregate
    {
        private readonly CodProdus _codProdus;
        private readonly string _nume;
        private UnitQuantity _quantity;
        private readonly KilogramQuantity _kilogram;
        private readonly Price _pret;

        private ProdusAggregate(
            CodProdus codProdus,
            string nume,
            UnitQuantity quantity,
            KilogramQuantity kilogram,
            Price pret)
        {
            _codProdus = codProdus ?? throw new ArgumentNullException(nameof(codProdus));
            _nume = nume ?? throw new ArgumentNullException(nameof(nume));
            _quantity = quantity ?? throw new ArgumentNullException(nameof(quantity));
            _kilogram = kilogram ?? throw new ArgumentNullException(nameof(kilogram));
            _pret = pret ?? throw new ArgumentNullException(nameof(pret));

            // INVARIANT: Stock cannot be negative
            if (_quantity.Cantitate < 0)
                throw new InvalidOperationException("Stocul nu poate fi negativ");
        }

        /// <summary>
        /// Factory method to create product aggregate
        /// </summary>
        public static ProdusAggregate Create(
            CodProdus codProdus,
            string nume,
            UnitQuantity quantity,
            KilogramQuantity kilogram,
            Price pret)
        {
            return new ProdusAggregate(codProdus, nume, quantity, kilogram, pret);
        }

        /// <summary>
        /// Decrease stock with invariant checks
        /// Returns domain event StocProdusScazutEvent
        /// </summary>
        public InventoryEvents.StocProdusScazutEvent DecreaseStock(double cantitate)
        {
            if (cantitate <= 0)
                throw new ArgumentException("Cantitatea trebuie s? fie pozitiv?");

            // INVARIANT: Cannot decrease more than available
            if (_quantity.Cantitate < cantitate)
                throw new InvalidOperationException(
                    $"Stoc insuficient pentru {_nume}. Disponibil: {_quantity.Cantitate}, Cerut: {cantitate}");

            var stocVechi = _quantity.Cantitate;
            _quantity = new UnitQuantity(_quantity.Cantitate - cantitate);

            return new InventoryEvents.StocProdusScazutEvent(
                _codProdus.Cod,
                _nume,
                cantitate,
                _quantity.Cantitate,
                DateTime.UtcNow
            );
        }

        /// <summary>
        /// Increase stock with invariant checks
        /// Returns domain event StocProdusMaritEvent
        /// </summary>
        public InventoryEvents.StocProdusMaritEvent IncreaseStock(double cantitate)
        {
            if (cantitate <= 0)
                throw new ArgumentException("Cantitatea trebuie s? fie pozitiv?");

            var stocVechi = _quantity.Cantitate;
            _quantity = new UnitQuantity(_quantity.Cantitate + cantitate);

            return new InventoryEvents.StocProdusMaritEvent(
                _codProdus.Cod,
                _nume,
                cantitate,
                _quantity.Cantitate,
                DateTime.UtcNow
            );
        }

        /// <summary>
        /// Check if product is out of stock
        /// </summary>
        public bool IsOutOfStock() => _quantity.Cantitate == 0;

        /// <summary>
        /// Check if product is available (has stock)
        /// </summary>
        public bool IsAvailable() => _quantity.Cantitate > 0;

        /// <summary>
        /// Convert to record for compatibility with existing code
        /// </summary>
        public Produs ToProdus()
        {
            return new Produs(_codProdus, _nume, _quantity, _kilogram, _pret);
        }

        // Read-only properties
        public CodProdus CodProdus => _codProdus;
        public string Nume => _nume;
        public UnitQuantity Quantity => _quantity;
        public KilogramQuantity Kilogram => _kilogram;
        public Price Pret => _pret;
    }
}
