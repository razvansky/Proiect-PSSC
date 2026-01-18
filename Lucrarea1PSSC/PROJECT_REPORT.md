# Proiect E-Commerce  - DDD Lab
Echipa
Peia Razvan	
Mak Mario
Mladin Alexandru

Domeniul Ales
E-Commerce Order Management System

Descriere
Sistemul implementează un flux complet de gestionare a comenzilor pentru un magazin online, incluzând funcționalități pentru coșul de cumpărături (adăugare produse, validare stoc, plată) și procesarea comenzilor (preluare, pregătire, livrare) folosind principii DDD și arhitectură bazată pe evenimente.

Bounded Contexts Identificate
[Shopping Cart Context]: Gestionarea stadiilor coșului de cumpărături (Empty -> Paid) și validarea conținutului.
[Order Fulfillment Context]: Gestionarea ciclului de viață al comenzii (Plasată -> Livrată) și a fluxurilor de lucru pentru operatori.
[Catalog Context]: Definirea produselor, prețurilor și unităților de măsură.

Event Storming Results
[Link la diagram sau imagine]

Implementare
Value Objects
[CodProdus]: Identificator unic pentru produse, validat la creare.
[Price]: Încapsulează valoarea numerică a prețului.
[Quantity (Unit/Kilogram)]: Gestionează cantitățile produselor în funcție de unitatea de măsură (bucăți sau greutate).
[Adress]: Value object pentru detaliile de livrare, asigurând imuabilitatea.
[Money]: Gestionarea sumelor monetare și a valutei.

Entity States
Unvalidated[Cos]: Coșul conține produse dar nu a fost verificat (stoc, prețuri).
Validated[Cos]: Coșul este validat și pregătit pentru plată.
Payed[Cos]: Plata a fost confirmată, coșul devine o comandă plasată.
StaraComanda[Plasata]: Comanda a fost inițializată de client.
StaraComanda[InPregatire]: Comanda a fost preluată de un operator.

Operations
Validate[Order]Operation: Verifică regulile de business (vechime comandă, total pozitiv, adresă validă) înainte de preluare.
StartPreparation[Order]: Tranziționează starea comenzii în "InPregatire".
CalculateEstimatedPreparationTime: Estimează timpul necesar procesării comenzii.

Workflow
[Preluare][Comanda]Workflow: Fluxul de preluare a comenzii de către operator: Validare -> Actualizare Stare -> Publicare Evenimente -> Generare Bon.

Rulare
# Compile
dotnet build

# Run console app
dotnet run --project Lucrarea1PSSC

# Run tests
dotnet test

Lecții Învățate
Ce a funcționat bine cu AI
Generarea rapidă a claselor boilerplate pentru Value Objects.
Implementarea pattern-ului de stări pentru Coșul de Cumpărături.

Limitări ale AI identificate
Dificultăți inițiale în înțelegerea contextului specific al "KilogramQuantity" vs "UnitQuantity" fără explicații detaliate.
Necesitatea ajustării manuale a conexiunilor cu baza de date.

Prompturi Utile
"Generează o clasă Workflow pentru preluarea comenzii care să includă validări și evenimente."
"Refactorizează coșul de cumpărături folosind State Pattern pentru a evita stările invalide."

Design Decisions
*   **Utilizarea Value Objects**: Am decis să folosim obiecte de tip valoare (`Money`, `Quantity`, `Adress`) pentru a preveni "Primitive Obsession" și a centraliza logica de validare, asigurând că un obiect nu poate exista într-o stare invalidă.
*   **State Pattern pentru Coșul de Cumpărături**: Tranzițiile coșului (`Empty` -> `Unvalidated` -> `Validated` -> `Payed`) sunt modelate prin clase distincte care implementează `IStareCos`. Aceasta garantează că operațiunile specifice (ex. Plată) pot fi apelate doar când coșul este în starea corectă.
*   **Result Pattern (Railway Oriented Programming)**: Gestionarea erorilor se face prin tipul `Result<T>` în loc de excepții pentru logica de business previzibilă. Acest lucru permite un flux de execuție liniar și explicit.
*   **Workflow Pattern**: Procesele complexe de business (ex. preluarea comenzii, plasarea comenzii) sunt izolate în clase de tip Workflow. Acestea orchestrează interacțiunea dintre agregate, repository-uri și event bus, păstrând agregatele concentrate pe invarianți.
*   **Event-Driven Communication**: Decuplarea componentelor se realizează prin publicarea de evenimente de domeniu (ex. `ComandaPreluataSuccessEvent`). Acest lucru permite altor părți ale sistemului (ex. facturare, notificări) să reacționeze fără a fi cuplate direct de logica de preluare.
*   **Repository Pattern (In-Memory cu Fallback DB)**: Pentru simplitate în dezvoltare dar robustețe în producție, folosim un dicționar in-memory ca cache rapid, cu fallback către baza de date SQL pentru persistență.