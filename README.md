# TaskManager

System zarządzania zadaniami zbudowany w ASP.NET Core MVC z Identity i Entity Framework Core.

## Wymagania

- .NET 8.0 SDK
- Visual Studio 2022 lub VS Code
- SQLite (wbudowany w pakiet Entity Framework)

## Instalacja pakietów

Zainstaluj następujące pakiety NuGet za pomocą Package Manager Console:

```powershell
Install-Package Microsoft.EntityFrameworkCore.Sqlite -Version 8.0.8
Install-Package Microsoft.EntityFrameworkCore.Tools -Version 8.0.8
Install-Package Microsoft.AspNetCore.Identity.EntityFrameworkCore -Version 8.0.0
Install-Package Microsoft.AspNetCore.Identity.UI -Version 8.0.0
```

## Migracje bazy danych

### Tworzenie pierwszej migracji

W Package Manager Console:

```powershell
Add-Migration InitialCreate
Update-Database
```

## Użytkownicy testowi

Aplikacja automatycznie tworzy przykładowych użytkowników przy pierwszym uruchomieniu (dane w `Program.cs`):

### 1. Administrator
- **Email:** admin@taskmanager.com
- **Hasło:** Admin@12345!
- **Rola:** Administrator
- **Uprawnienia:** Pełny dostęp do wszystkich funkcji

### 2. Manager
- **Email:** manager@taskmanager.com
- **Hasło:** Manager@123!
- **Rola:** Manager
- **Uprawnienia:** Zarządzanie grupami i zadaniami, do których jest przypisany

### 3. User (Anna Nowak)
- **Email:** user@taskmanager.com
- **Hasło:** User@123456!
- **Rola:** User
- **Uprawnienia:** Przeglądanie i praca nad przypisanymi zadaniami


## Przykładowe dane

Przy pierwszym uruchomieniu aplikacja automatycznie tworzy:

### Grupy:
- **Marketing** (Manager: Jan Kowalski)
- **Development** (Manager: Admin)

### Zadania:
- "Create marketing campaign" (Status: InProgress, Priority: High)
- "Fix login bug" (Status: ToDo, Priority: Critical)
- "Update documentation" (Status: Done, Priority: Low)

### Dziennik aktywności:
- Przykładowe logi dla utworzonych zadań

## Funkcje

### Role użytkowników:
- **Administrator:** Pełny dostęp, zarządzanie użytkownikami
- **Manager:** Tworzenie i zarządzanie zadaniami oraz grupami
- **User:** Przeglądanie i praca nad przypisanymi zadaniami

### Moduły:
- ✅ Zarządzanie zadaniami (CRUD)
- ✅ Zarządzanie grupami (CRUD)
- ✅ Zarządzanie użytkownikami (tylko Administrator)
- ✅ Dziennik aktywności
- ✅ Przypisywanie zadań do użytkowników
- ✅ Przypisywanie użytkowników do grup
- ✅ Zarządzanie rolami




### Format telefonu:
- **Wymagany format:** +48-XXX-XXX-XXX





## Technologie

- ASP.NET Core 8.0 MVC
- Entity Framework Core 8.0
- ASP.NET Core Identity 8.0
- SQLite
- Bootstrap 5
- Razor Pages
