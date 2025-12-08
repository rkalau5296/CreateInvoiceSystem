# CreateInvoiceSystem

## 1. Uruchomienie bazy danych w kontenerze Docker

Aby uruchomić bazę MS SQL w kontenerze, użyj poniższego polecenia w katalogu głównym projektu:

```
docker-compose up -d
```

Domyślne dane dostępowe do bazy:
- Serwer: `localhost,1433`
- Użytkownik: `sa`
- Hasło: `SuperSecretPass!234`

## 2. Konfiguracja aplikacji lokalnie

Do pracy lokalnej wymagany jest plik `appsettings.local.json` w katalogu `src/CreateInvoiceSystem.API`. Przykładowa zawartość pliku:

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=CreateInvoiceSystemDb;User Id=sa;Password=SuperSecretPass!234;TrustServerCertificate=True;"
  }
}
```

Plik ten nie jest wersjonowany i należy go utworzyć samodzielnie.

## 3. Migracje bazy danych

Aby wykonać migracje na bazie danych uruchomionej w kontenerze, przejdź do katalogu `src` i użyj polecenia:

```
dotnet ef database update --project ./CreateInvoiceSystem.Persistence --startup-project ./CreateInvoiceSystem.API
```

Upewnij się, że masz zainstalowane narzędzie `dotnet-ef`:

```
dotnet tool install --global dotnet-ef
```

Po wykonaniu tych kroków aplikacja powinna być gotowa do uruchomienia lokalnie.

