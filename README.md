# TaskManager

System zarz?dzania zadaniami zbudowany w ASP.NET Core MVC z Identity i Entity Framework Core.

## Wymagania

- .NET 8.0 SDK
- Visual Studio 2022 lub VS Code
- SQLite (wbudowany w pakiet Entity Framework)

## Instalacja pakiet?w

Zainstaluj nast?puj?ce pakiety NuGet za pomoc? Package Manager Console:

```powershell
Install-Package Microsoft.EntityFrameworkCore.Sqlite -Version 8.0.8
Install-Package Microsoft.EntityFrameworkCore.Tools -Version 8.0.8
Install-Package Microsoft.AspNetCore.Identity.EntityFrameworkCore -Version 8.0.0
Install-Package Microsoft.AspNetCore.Identity.UI -Version 8.0.0
```

Lub za pomoc? .NET CLI:

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.8
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.8
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.AspNetCore.Identity.UI --version 8.0.0
```

## Migracje bazy danych

### Tworzenie pierwszej migracji

W Package Manager Console:

```powershell
Add-Migration InitialCreate
Update-Database
```

Lub za pomoc? .NET CLI:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Aktualizacja bazy danych po zmianach w modelach

```powershell
Add-Migration NazwaMigracji
Update-Database
```

## Uruchamianie aplikacji

```bash
dotnet run
```

Lub naci?nij **F5** w Visual Studio.

## U?ytkownicy testowi

Aplikacja automatycznie tworzy przyk?adowych u?ytkownik?w przy pierwszym uruchomieniu (dane w `Program.cs`):

### 1. Administrator
- **Email:** admin@taskmanager.com
- **Has?o:** Admin@12345!
- **Rola:** Administrator
- **Uprawnienia:** Pe?ny dost?p do wszystkich funkcji

### 2. Manager
- **Email:** manager@taskmanager.com
- **Has?o:** Manager@123!
- **Rola:** Manager
- **Uprawnienia:** Zarz?dzanie grupami i zadaniami, do kt?rych jest przypisany

### 3. User (Anna Nowak)
- **Email:** user@taskmanager.com
- **Has?o:** User@123456!
- **Rola:** User
- **Uprawnienia:** Przegl?danie i praca nad przypisanymi zadaniami

### 4. User (Ivan Zamishchak)
- **Email:** ivan@taskmanager.com
- **Has?o:** User@123456!
- **Rola:** User
- **Uprawnienia:** Przegl?danie i praca nad przypisanymi zadaniami

## Przyk?adowe dane

Przy pierwszym uruchomieniu aplikacja automatycznie tworzy:

### Grupy:
- **Marketing** (Manager: Jan Kowalski)
- **Development** (Manager: Admin)

### Zadania:
- "Create marketing campaign" (Status: InProgress, Priority: High)
- "Fix login bug" (Status: ToDo, Priority: Critical)
- "Update documentation" (Status: Done, Priority: Low)

### Dziennik aktywno?ci:
- Przyk?adowe logi dla utworzonych zada?

## Struktura projektu

```
TaskManager/
??? Controllers/          # Kontrolery MVC
?   ??? HomeController.cs
?   ??? TaskItemsController.cs
?   ??? GroupsController.cs
?   ??? UsersController.cs
?   ??? TaskLogsController.cs
??? Models/              # Modele danych
?   ??? TaskItem.cs
?   ??? Group.cs
?   ??? ApplicationUser.cs
?   ??? TaskLog.cs
??? Views/               # Widoki Razor
?   ??? Home/
?   ??? TaskItems/
?   ??? Groups/
?   ??? Users/
?   ??? TaskLogs/
??? Data/                # Kontekst bazy danych
?   ??? ApplicationDbContext.cs
??? Migrations/          # Migracje EF Core
??? Program.cs           # Konfiguracja aplikacji i seed danych
```

## Funkcje

### Role u?ytkownik?w:
- **Administrator:** Pe?ny dost?p, zarz?dzanie u?ytkownikami
- **Manager:** Tworzenie i zarz?dzanie zadaniami oraz grupami
- **User:** Przegl?danie i praca nad przypisanymi zadaniami

### Modu?y:
- ? Zarz?dzanie zadaniami (CRUD)
- ? Zarz?dzanie grupami (CRUD)
- ? Zarz?dzanie u?ytkownikami (tylko Administrator)
- ? Dziennik aktywno?ci
- ? Przypisywanie zada? do u?ytkownik?w
- ? Przypisywanie u?ytkownik?w do grup
- ? Zarz?dzanie rolami

## Bezpiecze?stwo

### Wymagania has?a:
- Minimum 10 znak?w
- Wymagany znak specjalny
- Cyfra nie jest wymagana
- Wielka litera nie jest wymagana

### Format telefonu:
- **Wymagany format:** +48-XXX-XXX-XXX

## Troubleshooting

### B??d: "Connection string not found"
Upewnij si?, ?e plik `appsettings.json` zawiera:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=TaskManager.db"
  }
}
```

### B??d: "SQLite Error 19: FOREIGN KEY constraint failed"
Upewnij si?, ?e:
1. Usuwane zadania nie maj? aktywnych powi?za?
2. Grupy nie maj? aktywnych zada? przed usuni?ciem
3. Ostatni Administrator nie mo?e by? usuni?ty

### Reset bazy danych
Usu? plik `TaskManager.db` i uruchom ponownie:
```bash
dotnet ef database update
```

## Technologie

- ASP.NET Core 8.0 MVC
- Entity Framework Core 8.0
- ASP.NET Core Identity 8.0
- SQLite
- Bootstrap 5
- Razor Pages

## Licencja

Ten projekt jest przyk?adowym projektem edukacyjnym.