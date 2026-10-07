CreateInvoiceSystem
Aplikacja webowa do wystawiania i zarządzania fakturami. System pozwala prowadzić podstawową ewidencję klientów, produktów i faktur z poziomu interfejsu Blazor WebAssembly oraz udostępnia API ASP.NET Core.

Dokumentacja opisuje aktualny stan projektu. Przed wdrożeniem produkcyjnym uzupełnij wartości konfiguracyjne i wykonaj testy funkcjonalne oraz bezpieczeństwa.

Najważniejsze funkcje
rejestracja, logowanie i wylogowanie użytkownika,

obsługa access tokenów i refresh tokenów,

opcja „Zapamiętaj mnie” z przechowywaniem tokenów w localStorage lub sessionStorage,

automatyczne dołączanie tokenu do chronionych requestów przez AuthenticatedAndRefreshedHandler,

automatyczne odświeżanie sesji po odpowiedzi 401,

dashboard z podsumowaniem danych,

tworzenie, edycja, usuwanie i wyszukiwanie klientów,

tworzenie, edycja, usuwanie i wyszukiwanie produktów,

tworzenie, edycja, przeglądanie i usuwanie faktur,

generowanie PDF faktury,

eksport faktur i produktów do CSV,

obsługa cen i wartości dziesiętnych z walidacją,

walidacja formularzy po stronie klienta i serwera,

obsługa resetowania hasła,

pobieranie kursów NBP,

testy end-to-end z NUnit, Reqnroll i FluentAssertions.

Architektura
Projekt jest podzielony na frontend, API oraz warstwy domenowe i infrastrukturalne.

CreateInvoiceSystem/
├── src/
│   ├── CreateInvoiceSystem.Frontend/          # Blazor WebAssembly
│   ├── CreateInvoiceSystem.API/               # ASP.NET Core Web API
│   ├── CreateInvoiceSystem.Abstractions/      # Wspólne kontrakty
│   ├── CreateInvoiceSystem.Persistence/       # Dostęp do danych
│   ├── CreateInvoiceSystem.Identity/          # Uwierzytelnianie i użytkownicy
│   ├── CreateInvoiceSystem.Csv/               # Eksport CSV
│   ├── CreateInvoiceSystem.Pdf/               # Generowanie PDF
│   ├── CreateInvoiceSystem.Mail/              # Obsługa poczty
│   └── CreateInvoiceSystem.Modules.*          # Moduły domenowe
└── tests/
    └── CreateInvoiceSystem.E2E/               # Testy end-to-end


Frontend
Frontend korzysta z typed HttpClient. Serwisy takie jak InvoiceService, ClientService, ProductService i UserService używają klientów z podpiętym handlerem:

builder.Services.AddHttpClient<ProductService>(client =>
{
    client.BaseAddress = apiUri;
})
.AddHttpMessageHandler<AuthenticatedAndRefreshedHandler>();

NbpService oraz AuthClient są rejestrowane bez tego handlera, ponieważ obsługują requesty, które nie powinny automatycznie otrzymywać tokenu.

Handler uwierzytelniania
AuthenticatedAndRefreshedHandler:

odczytuje access token z localStorage, a następnie z sessionStorage,

dodaje nagłówek Authorization: Bearer ...,

reaguje na 401,

próbuje odświeżyć token,

ponawia request po udanym odświeżeniu,

czyści dane sesji i przekierowuje do logowania, gdy sesji nie można odnowić.

Endpointy logowania i odświeżania tokenu korzystają z klienta bez handlera, aby uniknąć rekurencyjnego wywoływania mechanizmu odświeżania.

Wymagania
.NET SDK zgodny z wersją ustawioną w plikach projektu,

SQL Server lub zgodna konfiguracja bazy danych,

Visual Studio 2022/18 albo aktualny Rider/VS Code,

przeglądarka obsługująca WebAssembly,

opcjonalnie: narzędzia do uruchamiania testów E2E.

Log uruchomieniowy projektu wskazuje na .NET 9 oraz frontend Blazor WebAssembly. Dostosuj wersję SDK do TargetFramework w .csproj, jeśli konfiguracja projektu została zmieniona.

Konfiguracja
Przed uruchomieniem sprawdź konfigurację frontendu, w szczególności:

{
  "BaseApiUrl": "https://localhost:7022/"
}

Wartość BaseApiUrl musi być poprawnym absolutnym adresem URL API. Nie umieszczaj sekretów, haseł ani kluczy w repozytorium. Dane wrażliwe przechowuj w User Secrets, zmiennych środowiskowych albo bezpiecznym systemie konfiguracji.

Konfiguracja API powinna zawierać co najmniej:

connection string do bazy danych,

ustawienia JWT,

konfigurację refresh tokenów,

ustawienia CORS dla frontendu,

konfigurację poczty, jeśli funkcje mailowe są włączone,

ustawienia eksportu PDF/CSV, jeśli są wymagane przez wdrożenie.

Uruchomienie lokalne
1. Sklonuj repozytorium.

git clone <adres-repozytorium>
cd CreateInvoiceSystem

2. Uzupełnij konfigurację API i frontendu.

3. Przywróć zależności i zbuduj rozwiązanie.

dotnet restore
dotnet build

4. Uruchom API.

dotnet run --project src/CreateInvoiceSystem.API

5. Uruchom frontend w osobnym terminalu, jeśli rozwiązanie nie uruchamia obu projektów razem.

dotnet run --project src/CreateInvoiceSystem.Frontend

Adresy aplikacji zależą od profilu uruchomieniowego. W środowisku deweloperskim sprawdź adresy wypisane w konsoli, np. https://localhost:7022 dla API lub adres frontendu przypisany przez profil uruchomieniowy.

Uwierzytelnianie
Po zalogowaniu aplikacja zapisuje access token i — jeśli API go zwróci — refresh token:

localStorage, gdy użytkownik wybierze „Remember me”,

sessionStorage, gdy sesja ma obowiązywać tylko dla bieżącej sesji przeglądarki.

Chronione serwisy nie powinny ręcznie pobierać tokenu ani ustawiać nagłówka Authorization. Powinny korzystać z HttpClient skonfigurowanego z AuthenticatedAndRefreshedHandler.

AuthClient jest klientem bez handlera i służy do operacji takich jak logowanie, rejestracja, reset hasła oraz odświeżanie tokenu.

Testy
Uruchomienie wszystkich testów:

dotnet test

Testy E2E wymagają uruchomionej aplikacji, dostępnej bazy danych oraz poprawnej konfiguracji środowiska testowego. Przed uruchomieniem testów sprawdź:

adres frontendu i API,

dane użytkownika testowego,

stan bazy danych,

dostępność przeglądarki używanej przez narzędzie E2E.

Testy formularzy powinny sprawdzać zarówno zachowanie użytkownika, jak i dokładny tekst komunikatów walidacyjnych. Przy zmianie treści komunikatu trzeba zaktualizować odpowiedni locator lub asercję.

Główne endpointy
Nazwy endpointów wynikają z używanych serwisów frontendowych. Przykładowe ścieżki:

| Obszar     | Przykładowe endpointy                                                                |
| ---------- | ------------------------------------------------------------------------------------ |
| Auth       | api/Auth/login, api/Auth/register, api/Auth/forgot-password, api/Auth/reset-password |
| Token      | api/User/refresh                                                                     |
| Użytkownik | api/User/me, api/User/update/{id}, api/User/{id}                                     |
| Klienci    | api/Client, api/Client/create, api/Client/update/{id}                                |
| Produkty   | api/Product, api/Product/create, api/Product/update/{id}                             |
| Faktury    | api/Invoice, api/Invoice/create, api/Invoice/update/{id}                             |
| Eksport    | api/export/products, api/export/invoices                                             |


Rzeczywiste metody HTTP, wymagania autoryzacji i formaty odpowiedzi należy traktować jako źródło prawdy w kontrolerach API i kontraktach DTO.

Bezpieczeństwo
nie commituj tokenów, haseł, connection stringów ani kluczy,

zawsze waliduj dane również po stronie API,

traktuj tokeny przechowywane w Web Storage jako dostępne dla kodu JavaScript uruchomionego w aplikacji,

używaj HTTPS poza lokalnym środowiskiem,

ogranicz CORS do znanych originów,

sprawdzaj autoryzację na API, a nie tylko w interfejsie,

po wylogowaniu i wygaśnięciu sesji usuwaj access oraz refresh tokeny,

nie loguj tokenów ani pełnych danych uwierzytelniających.

Typowy przepływ
Użytkownik rejestruje konto lub loguje się.

API zwraca access token i opcjonalnie refresh token.

Frontend zapisuje token w wybranym magazynie przeglądarki.

Chronione serwisy wysyłają request przez AuthenticatedAndRefreshedHandler.

Handler dodaje nagłówek Bearer.

Przy 401 handler próbuje odświeżyć sesję.

Po sukcesie request jest ponawiany; po niepowodzeniu użytkownik trafia na stronę logowania.

Użytkownik zarządza klientami, produktami i fakturami oraz może pobrać dokumenty PDF/CSV.

Rozwiązywanie problemów
Brak tokenu w requestach
Sprawdź, czy dany serwis jest zarejestrowany przez AddHttpClient<T>() z:

.AddHttpMessageHandler<AuthenticatedAndRefreshedHandler>();


Następnie sprawdź w narzędziach przeglądarki zakładkę Network i nagłówki requestu.

Request logowania dostaje token
Logowanie powinno korzystać z AuthClient, który nie ma AuthenticatedAndRefreshedHandler.

„Remember me” nie działa
Sprawdź, czy handler sprawdza zarówno localStorage, jak i sessionStorage, oraz czy login zapisuje token w tym samym magazynie.

Walidacja nie pojawia się w teście E2E
Sprawdź, czy locator szuka dokładnie tego tekstu, który renderuje komponent. Sama walidacja może działać poprawnie, mimo że asercja oczekuje innego komunikatu.

Błąd po zmianie modelu formularza
Upewnij się, że Model formularza jest tym samym obiektem, do którego prowadzą bindingi pól, np. formularz dodawania używa _newProduct, a formularz edycji _editModel.

Status projektu
Projekt zawiera działające moduły do obsługi użytkowników, klientów, produktów i faktur oraz testy E2E. Przed oznaczeniem wydania produkcyjnego należy jeszcze utrzymywać tę dokumentację razem ze zmianami API, konfiguracji, bezpieczeństwa i procesów wdrożeniowych.










