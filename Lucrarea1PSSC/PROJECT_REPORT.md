# Lucrarea1PSSC - DDD Lab
Echipa
[Nume Student 1]
[Nume Student 2]
[Nume Student 3]

Domeniul Ales
E-Commerce Order Management System

Descriere
Sistemul implementeaz? un flux complet de gestionare a comenzilor pentru un magazin online, incluzând func?ionalit??i pentru co?ul de cump?r?turi (ad?ugare produse, validare stoc, plat?) ?i procesarea comenzilor (preluare, preg?tire, livrare) folosind principii DDD ?i arhitectur? bazat? pe evenimente.

Bounded Contexts Identificate
[Shopping Cart Context]: Gestionarea stadiilor co?ului de cump?r?turi (Empty -> Paid) ?i validarea con?inutului.
[Order Fulfillment Context]: Gestionarea ciclului de via?? al comenzii (Plasat? -> Livrat?) ?i a fluxurilor de lucru pentru operatori.
[Catalog Context]: Definirea produselor, pre?urilor ?i unit??ilor de m?sur?.

Event Storming Results
[Link la diagram sau imagine]

Implementare
Value Objects
[CodProdus]: Identificator unic pentru produse, validat la creare.
[Price]: Încapsuleaz? valoarea numeric? a pre?ului.
[Quantity (Unit/Kilogram)]: Gestioneaz? cantit??ile produselor în func?ie de unitatea de m?sur? (buc??i sau greutate).
[Adress]: Value object pentru detaliile de livrare, asigurând imuabilitatea.
[Money]: Gestionarea sumelor monetare ?i a valutei.

Entity States
Unvalidated[Cos]: Co?ul con?ine produse dar nu a fost verificat (stoc, pre?uri).
Validated[Cos]: Co?ul este validat ?i preg?tit pentru plat?.
Payed[Cos]: Plata a fost confirmat?, co?ul devine o comand? plasat?.
StaraComanda[Plasata]: Comanda a fost ini?ializat? de client.
StaraComanda[InPregatire]: Comanda a fost preluat? de un operator.

Operations
Validate[Order]Operation: Verific? regulile de business (vechime comand?, total pozitiv, adres? valid?) înainte de preluare.
StartPreparation[Order]: Tranzi?ioneaz? starea comenzii în "InPregatire".
CalculateEstimatedPreparationTime: Estimeaz? timpul necesar proces?rii comenzii.

Workflow
[Preluare][Comanda]Workflow: Fluxul de preluare a comenzii de c?tre operator: Validare -> Actualizare Stare -> Publicare Evenimente -> Generare Bon.

Rulare
# Compile
dotnet build

# Run console app
dotnet run --project Lucrarea1PSSC

# Run tests
dotnet test

Lec?ii Înv??ate
Ce a func?ionat bine cu AI
Generarea rapid? a claselor boilerplate pentru Value Objects.
Implementarea pattern-ului de st?ri pentru Co?ul de Cump?r?turi.

Limit?ri ale AI identificate
Dificult??i ini?iale în în?elegerea contextului specific al "KilogramQuantity" vs "UnitQuantity" f?r? explica?ii detaliate.
Necesitatea ajust?rii manuale a conexiunilor cu baza de date.

Prompturi Utile
"Genereaz? o clas? Workflow pentru preluarea comenzii care s? includ? valid?ri ?i evenimente."
"Refactorizeaz? co?ul de cump?r?turi folosind State Pattern pentru a evita st?rile invalide."

Design Decisions
*   **Utilizarea Value Objects**: Am decis s? folosim obiecte de tip valoare (`Money`, `Quantity`, `Adress`) pentru a preveni "Primitive Obsession" ?i a centraliza logica de validare, asigurând c? un obiect nu poate exista într-o stare invalid?.
*   **State Pattern pentru Co?ul de Cump?r?turi**: Tranzi?iile co?ului (`Empty` -> `Unvalidated` -> `Validated` -> `Payed`) sunt modelate prin clase distincte care implementeaz? `IStareCos`. Aceasta garanteaz? c? opera?iunile specifice (ex. Plat?) pot fi apelate doar când co?ul este în starea corect?.
*   **Result Pattern (Railway Oriented Programming)**: Gestionarea erorilor se face prin tipul `Result<T>` în loc de excep?ii pentru logica de business previzibil?. Acest lucru permite un flux de execu?ie liniar ?i explicit.
*   **Workflow Pattern**: Procesele complexe de business (ex. preluarea comenzii, plasarea comenzii) sunt izolate în clase de tip Workflow. Acestea orchestreaz? interac?iunea dintre agregate, repository-uri ?i event bus, p?strând agregatele concentrate pe invarian?i.
*   **Event-Driven Communication**: Decuplarea componentelor se realizeaz? prin publicarea de evenimente de domeniu (ex. `ComandaPreluataSuccessEvent`). Acest lucru permite altor p?r?i ale sistemului (ex. facturare, notific?ri) s? reac?ioneze f?r? a fi cuplate direct de logica de preluare.
*   **Repository Pattern (In-Memory cu Fallback DB)**: Pentru simplitate în dezvoltare dar robuste?e în produc?ie, folosim un dic?ionar in-memory ca cache rapid, cu fallback c?tre baza de date SQL pentru persisten??.