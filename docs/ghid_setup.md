# Ghid Setup și Implementare: .NET 9 MVC cu PostgreSQL și EF Core

Acest document conține pașii tehnici și comenzile CLI (Command Line Interface) necesare pentru a construi un proiect ASP.NET Core MVC de la zero pe Linux/VS Code, îndeplinind strict cerințele de evaluare pentru arhitectura bazei de date.

## Obiective Acoperite (Barem)

1. **Design-ul bazei de date** (minim 5-6 tabele) - 4p

2. **Implementarea bazei de date** (abordarea Code First) - 2p

3. **Crearea conexiunii** (Entity Framework Core) - 2p

4. **Testarea conexiunii** (Controller cu operații CRUD) - 1p

## Etapa 1: Inițializarea Proiectului și Instalarea Pachetelor

Deschide terminalul și rulează următoarele comenzi pentru a crea scheletul aplicației și a descărca pachetele necesare pentru PostgreSQL, Entity Framework și sistemul de utilizatori (Identity).

```bash
# 1. Creează un proiect nou de tip MVC folosind .NET 9.0
dotnet new mvc -n NumeProiect -f net9.0

# 2. Navighează în folderul proiectului
cd NumeProiect

# 3. Instalează driver-ul pentru PostgreSQL
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# 4. Instalează instrumentele Entity Framework Core (pentru migrații)
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design

# 5. Instalează pachetele pentru ASP.NET Core Identity (Utilizatori)
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Identity.UI

# 6. Instalează utilitarele pentru Scaffolding (Generare automată de controllere)
dotnet tool install -g dotnet-aspnet-codegenerator
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
```

## Etapa 2: Implementarea Code-First și Design-ul (4p + 2p)

Pentru a îndeplini cerința de 5-6 tabele folosind abordarea Code-First, trebuie create modelele de date direct în C#.

1. Creează clasele în folderul `Models` (ex: `Course.cs`, `Lesson.cs`, etc.) adăugând adnotările necesare (`[Key]`, `[Required]`, `[ForeignKey]`).

2. Creează clasa de context pentru baza de date care moștenește `IdentityDbContext` pentru a include automat tabelele de utilizatori:

```csharp
// Fișier: Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NumeProiect.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Definirea tabelelor personalizate (Minim 5-6 pentru barem)
    public DbSet<Course> Courses { get; set; }
    public DbSet<Chapter> Chapters { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    // ... restul tabelelor
}
```

## Etapa 3: Crearea Conexiunii cu Baza de Date (2p)

Se configurează conexiunea către serverul PostgreSQL.

**1. Setarea Connection String-ului în `appsettings.json`:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=NumeBazaDate;Username=postgres;Password=ParolaTa"
  }
}
```

**2. Înregistrarea serviciului în `Program.cs`:**

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Înregistrarea DbContext cu PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
```

## Etapa 4: Aplicarea Migrațiilor în PostgreSQL

Odată ce modelele și conexiunea sunt gata, transformăm clasele C# în tabele SQL reale folosind comenzile CLI pentru migrații:

```bash
# Generează codul pentru crearea bazei de date (snapshot-ul claselor)
dotnet ef migrations add InitialCreate

# Aplică migrația pe serverul PostgreSQL (creează/actualizează tabelele fizice)
dotnet ef database update
```

## Etapa 5: Testarea Conexiunii prin Operații CRUD (1p)

Pentru a testa conexiunea, generăm automat un Controller complet funcțional (Create, Read, Update, Delete) legat la unul dintre tabele (ex: `Course`), folosind utilitarul de Scaffolding instalat la Etapa 1.

```bash
# Generează Controller-ul și View-urile CRUD asociate modelului Course
dotnet aspnet-codegenerator controller -name CoursesController -m Course -dc ApplicationDbContext --relativeFolderPath Controllers --useDefaultLayout --referenceScriptLibraries
```

*Notă: Controller-ul rezultat va folosi automat `ApplicationDbContext` injectat pentru a executa comenzile EF Core (ex: `_context.Add()`, `_context.SaveChangesAsync()`).*

## Etapa 6: Rularea Aplicației

După finalizarea tuturor pașilor de mai sus, aplicația este gata de testare.

```bash
# Pornește serverul web local
dotnet run
```

Accesează în browser ruta `/Courses` pentru a testa vizual operațiile CRUD și a confirma scrierea datelor în PostgreSQL.
