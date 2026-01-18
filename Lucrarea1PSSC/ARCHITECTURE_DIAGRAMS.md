# Visual Architecture Diagrams

## Complete DDD Architecture

```mermaid
graph TB
    subgraph "Presentation Layer"
        UI[Program.cs - Console UI]
    end

    subgraph "Application Layer - Commands"
        CMD1[CreateCartCommand]
        CMD2[AddProductToCartCommand]
        CMD3[RemoveProductFromCartCommand]
        CMD4[EmptyCartCommand]
        CMD5[PayCartCommand]
        CMD6[DecreaseStockCommand]
        CMD7[IncreaseStockCommand]
        CMD8[PlaceOrderCommand]
        CMD9[AssociateCartCommand]
    end

    subgraph "Domain Layer - Aggregates"
        AGG1[CosDeCumparaturi<br/>Shopping Cart Aggregate]
        AGG2[ProdusAggregate<br/>Product Aggregate]
        AGG3[ComandaAggregate<br/>Order Aggregate]
        AGG4[Persoana<br/>Customer Aggregate]
    end

    subgraph "Domain Layer - Events"
        EVT1[CartEvents]
        EVT2[InventoryEvents]
        EVT3[OrderEvents]
        EVT4[CustomerEvents]
    end

    subgraph "Infrastructure Layer"
        BUS[EventBus]
        PERSIST[File Persistence]
    end

    UI --> CMD1
    UI --> CMD2
    UI --> CMD3
    UI --> CMD4
    UI --> CMD5
    UI --> CMD8

    CMD1 --> AGG4
    CMD2 --> AGG1
    CMD3 --> AGG1
    CMD4 --> AGG1
    CMD5 --> AGG1
    CMD6 --> AGG2
    CMD7 --> AGG2
    CMD8 --> AGG3
    CMD9 --> AGG4

    AGG1 --> EVT1
    AGG2 --> EVT2
    AGG3 --> EVT3
    AGG4 --> EVT4

    EVT1 --> BUS
    EVT2 --> BUS
    EVT3 --> BUS
    EVT4 --> BUS

    BUS --> PERSIST

    style CMD1 fill:#e1f5ff
    style CMD2 fill:#e1f5ff
    style CMD3 fill:#e1f5ff
    style CMD4 fill:#e1f5ff
    style CMD5 fill:#e1f5ff
    style CMD6 fill:#e1f5ff
    style CMD7 fill:#e1f5ff
    style CMD8 fill:#e1f5ff
    style CMD9 fill:#e1f5ff

    style AGG1 fill:#fff3e0
    style AGG2 fill:#fff3e0
    style AGG3 fill:#fff3e0
    style AGG4 fill:#fff3e0

    style EVT1 fill:#e8f5e9
    style EVT2 fill:#e8f5e9
    style EVT3 fill:#e8f5e9
    style EVT4 fill:#e8f5e9

    style BUS fill:#f3e5f5
    style PERSIST fill:#f3e5f5
```

---

## Command Flow Example: Add Product to Cart

```mermaid
sequenceDiagram
    participant User
    participant UI as Program.cs
    participant Cmd as AddProductToCartCommand
    participant Cart as CosDeCumparaturi Aggregate
    participant Prod as ProdusAggregate
    participant Bus as EventBus
    participant Inv as Inventory Subscriber

    User->>UI: Enter product name
    UI->>Cmd: TryCreate(productName)
    Cmd-->>UI: (success, command, error)
    
    alt Command Valid
        UI->>Cart: AdaugaProdus(name, catalog)
        Cart->>Cart: Validate invariants<br/>(not paid, not invalid)
        Cart->>Prod: Check stock availability
        Prod-->>Cart: Stock available
        Cart->>Cart: Add product to cart
        Cart->>Cart: Transition to ValidatedCos
        Cart->>Bus: Publish ProdusAdaugatInCosEvent
        Bus->>Inv: Notify subscribers
        Inv->>Prod: DecreaseStock(1)
        Prod->>Prod: Check invariant<br/>(stock >= 0)
        Prod->>Bus: Publish StocProdusScazutEvent
        
        alt Stock = 0
            Prod->>Bus: Publish ProdusEpuizatEvent
            Bus->>UI: Display warning
        end
        
        UI->>User: Success message
    else Command Invalid
        UI->>User: Error message
    end
```

---

## Order Placement Flow

```mermaid
sequenceDiagram
    participant User
    participant UI as Program.cs
    participant Cmd as PlaceOrderCommand
    participant Ord as ComandaAggregate
    participant Cart as CosDeCumparaturi
    participant Bus as EventBus

    User->>UI: Place order
    UI->>Cmd: TryCreate(person, cart)
    Cmd->>Cmd: Validate inputs
    Cmd-->>UI: (success, command, error)

    alt Command Valid
        UI->>Ord: CreateFromPaidCart(person, cart)
        Ord->>Cart: GetStareCos()
        Cart-->>Ord: PayedCos
        
        Ord->>Ord: Validate address<br/>(min 5 chars)
        Ord->>Cart: GetProduseCos()
        Cart-->>Ord: List of products
        Ord->>Cart: TotalCos()
        Cart-->>Ord: Total amount
        
        Ord->>Ord: Check invariants:<br/>- Cart is paid<br/>- Address valid<br/>- Has products<br/>- Total > 0
        
        Ord->>Ord: Create order with<br/>unique ID
        Ord->>Ord: Set state to Plasata
        Ord-->>UI: (success, order, null)
        
        UI->>Ord: ToSuccessEvent()
        Ord-->>UI: ComandaPlasataSuccessEvent
        UI->>Bus: Publish event
        UI->>User: Order confirmation<br/>Order ID, Total, Status
    else Command Invalid
        UI->>Bus: Publish ComandaPlasataFailedEvent
        UI->>User: Error message
    end
```

---

## State Transitions: Shopping Cart

```mermaid
stateDiagram-v2
    [*] --> UnvalidatedCos: new CosDeCumparaturi(1)
    [*] --> EmptyCos: new CosDeCumparaturi()
    
    EmptyCos --> ValidatedCos: AddProduct
    ValidatedCos --> EmptyCos: RemoveLastProduct
    ValidatedCos --> ValidatedCos: AddProduct / RemoveProduct
    ValidatedCos --> PayedCos: PayCart
    
    PayedCos --> [*]: OrderPlaced
    
    UnvalidatedCos --> [*]: Discarded
    
    note right of EmptyCos
        Cart is empty
        Cannot be paid
        Can add products
    end note
    
    note right of ValidatedCos
        Cart has products
        Can be modified
        Can be paid
    end note
    
    note right of PayedCos
        Cart is paid
        IMMUTABLE
        Ready for order
    end note
    
    note right of UnvalidatedCos
        Cart is invalid
        Cannot be used
        Must create new
    end note
```

---

## State Transitions: Order

```mermaid
stateDiagram-v2
    [*] --> Plasata: CreateFromPaidCart
    
    Plasata --> InPregatire: StartPreparation()
    Plasata --> Anulata: Cancel()
    
    InPregatire --> Expediata: Ship()
    InPregatire --> Anulata: Cancel()
    
    Expediata --> Livrata: Deliver()
    
    Livrata --> [*]
    Anulata --> [*]
    
    note right of Plasata
        Order placed
        Can be prepared
        Can be cancelled
    end note
    
    note right of InPregatire
        Being prepared
        Can be shipped
        Can be cancelled
    end note
    
    note right of Expediata
        Shipped
        Cannot be cancelled
        Awaiting delivery
    end note
    
    note right of Livrata
        Delivered
        FINAL STATE
    end note
    
    note right of Anulata
        Cancelled
        FINAL STATE
    end note
```

---

## Bounded Context Map

```mermaid
graph LR
    subgraph "Shopping Cart Context"
        SC[Shopping Cart<br/>Aggregate]
        SC_CMD[Cart Commands]
        SC_EVT[Cart Events]
    end
    
    subgraph "Inventory Context"
        INV[Product<br/>Aggregate]
        INV_CMD[Inventory Commands]
        INV_EVT[Inventory Events]
    end
    
    subgraph "Order Management Context"
        ORD[Order<br/>Aggregate]
        ORD_CMD[Order Commands]
        ORD_EVT[Order Events]
    end
    
    subgraph "Customer Context"
        CUST[Customer<br/>Aggregate]
        CUST_CMD[Customer Commands]
        CUST_EVT[Customer Events]
    end
    
    SC_EVT -->|ProdusAdaugatInCosEvent| INV
    SC_EVT -->|ProdusStergeDinCosEvent| INV
    SC_EVT -->|CosGolitEvent| INV
    SC_EVT -->|CosPlatitEvent| ORD
    
    INV_EVT -->|ProdusEpuizatEvent| SC
    ORD_EVT -->|ComandaPlasataSuccessEvent| CUST
    
    CUST_CMD -->|CreateCart| SC
    
    style SC fill:#ffebee
    style INV fill:#e8f5e9
    style ORD fill:#e3f2fd
    style CUST fill:#fff3e0
```

---

## Event Flow Timeline

```mermaid
gantt
    title Order Placement Timeline
    dateFormat HH:mm:ss
    
    section User Actions
    Create Cart           :a1, 00:00:00, 1s
    Add Product 1         :a2, 00:00:01, 1s
    Add Product 2         :a3, 00:00:02, 1s
    Pay Cart              :a4, 00:00:05, 1s
    Place Order           :a5, 00:00:07, 1s
    
    section Cart Events
    CosCreatEvent         :e1, 00:00:00, 1s
    ProdusAdaugat 1       :e2, 00:00:01, 1s
    ProdusAdaugat 2       :e3, 00:00:02, 1s
    CosValidatEvent       :e4, 00:00:02, 1s
    CosPlatitEvent        :e5, 00:00:05, 1s
    
    section Inventory Events
    StocScazut Product 1  :i1, 00:00:01, 1s
    StocScazut Product 2  :i2, 00:00:02, 1s
    
    section Order Events
    OrderReady            :o1, 00:00:05, 1s
    OrderPlaced           :o2, 00:00:07, 1s
```

---

## Class Diagram: Key Aggregates

```mermaid
classDiagram
    class CosDeCumparaturi {
        -List~ProdusCos~ produse_cos
        -IStareCos stare_cos
        -Thread backgroundThread
        +AdaugaProdus(string, List~Produs~)
        +StergeProdus(string, List~Produs~)
        +GolesteCos(List~Produs~)
        +platesteCos() bool
        +GetStareCos() IStareCos
        +TotalCos() double
        -ControlStareCos()
    }

    class IStareCos {
        <<interface>>
    }

    class EmptyCos {
        +bool gol
    }

    class ValidatedCos {
        +bool valid
    }

    class PayedCos {
        +bool payed
    }

    class UnvalidatedCos {
        +bool invalid
    }

    class ProdusAggregate {
        -CodProdus _codProdus
        -string _nume
        -UnitQuantity _quantity
        -KilogramQuantity _kilogram
        -Price _pret
        +DecreaseStock(double) StocProdusScazutEvent
        +IncreaseStock(double) StocProdusMaritEvent
        +IsOutOfStock() bool
        +IsAvailable() bool
    }

    class ComandaAggregate {
        -Guid _comandaId
        -string _numeClient
        -Adress _adresaLivrare
        -List~ProdusCos~ _produse
        -double _total
        -DateTime _dataPlasare
        -StaraComanda _stare
        +CreateFromPaidCart(Persoana, CosDeCumparaturi)$ tuple
        +ToSuccessEvent() ComandaPlasataSuccessEvent
        +StartPreparation()
        +Ship()
        +Deliver()
        +Cancel()
    }

    class Persoana {
        +Nume Nume
        +EmailP Email
        +Adress Adress
        +List~CosDeCumparaturi~ Cosuri
        +CosCurent CosDeCumparaturi
        +AdaugaCos(CosDeCumparaturi) Persoana
    }

    CosDeCumparaturi --> IStareCos
    IStareCos <|.. EmptyCos
    IStareCos <|.. ValidatedCos
    IStareCos <|.. PayedCos
    IStareCos <|.. UnvalidatedCos

    Persoana "1" --> "*" CosDeCumparaturi
    ComandaAggregate --> Persoana
    ComandaAggregate --> CosDeCumparaturi
```

---

## Deployment View

```mermaid
graph TB
    subgraph "Client Tier"
        UI[Console Application<br/>Program.cs]
    end

    subgraph "Application Tier"
        CMD[Commands Layer]
        WF[Workflows Layer]
    end

    subgraph "Domain Tier"
        AGG[Aggregates]
        EVT[Domain Events]
        VO[Value Objects]
    end

    subgraph "Infrastructure Tier"
        BUS[Event Bus]
        REPO[Repositories]
        FS[File System]
    end

    UI --> CMD
    UI --> WF
    CMD --> AGG
    WF --> AGG
    AGG --> EVT
    AGG --> VO
    EVT --> BUS
    BUS --> REPO
    REPO --> FS

    style UI fill:#e1f5ff
    style CMD fill:#fff3e0
    style WF fill:#fff3e0
    style AGG fill:#ffebee
    style EVT fill:#e8f5e9
    style VO fill:#e8f5e9
    style BUS fill:#f3e5f5
    style REPO fill:#f3e5f5
    style FS fill:#f3e5f5
```

---

## Package Dependencies

```mermaid
graph TD
    Main[main/Program.cs]
    
    ClaseCos[clase.ClaseCos]
    ClaseProduse[clase.ClaseProduse]
    Workflow[clase.Workflow]
    Persoane[clase.ClaseGestionarePersoane]
    Files[clase.ClaseGestionareFisiere]
    Infra[clase.Infrastructure]
    
    Main --> ClaseCos
    Main --> ClaseProduse
    Main --> Workflow
    Main --> Persoane
    Main --> Files
    Main --> Infra
    
    ClaseCos --> ClaseProduse
    ClaseCos --> Infra
    
    Workflow --> ClaseCos
    Workflow --> Persoane
    Workflow --> Infra
    
    Persoane --> ClaseCos
    Persoane --> Infra
    
    Files --> ClaseCos
    Files --> ClaseProduse
    Files --> Persoane

    style Main fill:#e1f5ff
    style ClaseCos fill:#ffebee
    style ClaseProduse fill:#e8f5e9
    style Workflow fill:#e3f2fd
    style Persoane fill:#fff3e0
    style Files fill:#f3e5f5
    style Infra fill:#f3e5f5
```

---

These diagrams provide a complete visual representation of your DDD architecture! ??
