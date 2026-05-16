<div style="display: flex; align-items: center; gap: 10px; margin-bottom: 20px;">
  <img src="wwwroot/assets/icons/vibelang_transparent_1024.png" alt="VibeLang" width="50" height="50">
  <img src="wwwroot/assets/title.png" alt="VibeLang Title" height="46">
</div>

> **A gamified language learning web application built with ASP.NET Core MVC, Entity Framework Core, and ASP.NET Core Identity.**

---

## 📝 Description

VibeLang is a modern web application for language learning, built on micro-learning and gamification principles. Originally a static prototype, it has been evolved into a full-featured **ASP.NET Core MVC** application with a robust backend, secure authentication, role-based access control, and a service-layer architecture.

---

## 🚀 Features

### 🎓 Learning
- **Interactive Lessons** — vocabulary cards, translation exercises, and contextual examples driven by JSON lesson content
- **Quiz System** — multiple question types (multiple choice, matching, fill-in-the-blank) with automatic scoring
- **Course Hierarchy** — Courses → Chapters → Lessons with ordered navigation and progress tracking

### 🎮 Gamification
- **XP System** — earn experience points for completing lessons and quizzes
- **Daily Streaks** — maintain streaks with active tracking, break detection, and cooldown warnings
- **Achievements** — 6 unlockable badges (Beginner's Start, Perfect Score, Expert Learner, Speed Learner, Marathon Runner, Social Butterfly)
- **Leaderboard** — global XP rankings across all users

### 👤 User Management
- **Registration & Login** — styled forms with validation, show/hide password toggle, and Forgot Password link
- **Profile Page** — update first/last name, upload profile picture, remove picture
- **Profile Picture** — displayed on the profile page, in the navigation bar, and wherever user identity is shown
- **Vocabulary Tracker** — personal dictionary of words encountered in lessons (New → Learned progression)

### 🔐 Security
- **ASP.NET Core Identity** — full membership system with hashed passwords and cookie-based authentication
- **Role-Based Authorization** — two roles: `Admin` and `User`, enforced via `[Authorize(Roles = "...")]` attributes
- **Service Layer** — all Identity operations (`RegisterAsync`, `LoginAsync`, `LogoutAsync`) are decoupled from Razor Pages into `IAuthService`
- **Access Denied Page** — themed denial page with role-aware redirect

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 9 / ASP.NET Core MVC |
| Database | PostgreSQL via Entity Framework Core 9 |
| ORM | EF Core (Code-First, Migrations) |
| Identity | ASP.NET Core Identity with `IdentityRole` |
| Frontend | Razor Views, Bootstrap 5, Vanilla CSS, JavaScript |
| CI/CD | GitHub Actions |

---

## 🏗️ Architecture

The application follows a layered architecture:

```
┌──────────────────────────────────────────────┐
│              Presentation Layer              │
│  Razor Pages (Identity) + MVC Controllers   │
│  Views: .cshtml with auth-* design system   │
└───────────────┬──────────────────────────────┘
                │ Dependency Injection
┌───────────────▼──────────────────────────────┐
│               Service Layer                  │
│  IAuthService    → AuthService               │
│  ICourseService  → CourseService             │
│  IStatsService   → StatsService              │
│  IAchievementService, IVocabularyService...  │
└───────────────┬──────────────────────────────┘
                │
┌───────────────▼──────────────────────────────┐
│             Repository Layer                 │
│  IRepository<T> → Repository<T> (generic)   │
│  ICourseRepository, ILessonRepository...    │
└───────────────┬──────────────────────────────┘
                │
┌───────────────▼──────────────────────────────┐
│          Data Layer (EF Core)                │
│  VibeLangDbContext : IdentityDbContext       │
│  PostgreSQL (Neon / local)                  │
└──────────────────────────────────────────────┘
```

---

## 🔑 Role-Based Authorization

| Role | Access |
|---|---|
| `Admin` | Admin panel, Course/Chapter/Lesson CRUD, all User pages |
| `User` | Courses (read), Lessons, Achievements, Leaderboard, Vocabulary, Profile |
| Anonymous | Home (`/`), Privacy — everything else redirects to Login |

**Seeded Admin account** (created automatically at first startup):

| Field | Value |
|---|---|
| Email | `admin@vibelang.com` |
| Password | `Admin123` |

---

## 🔐 Authentication Service Layer

All Identity logic is extracted from Razor Page code-behind into `IAuthService`:

```csharp
// LoginModel.cshtml.cs — NO direct Identity API calls
var result = await _authService.LoginAsync(Input.Email, Input.Password, Input.RememberMe);
if (result.Succeeded)
{
    var role = await _authService.GetUserRoleAsync(Input.Email);
    return role == "Admin"
        ? RedirectToAction("Index", "Admin")    // Admin dashboard
        : RedirectToAction("Courses", "Home");  // Learner dashboard
}
```

```csharp
// RegisterModel.cshtml.cs — delegates everything to the service
var result = await _authService.RegisterAsync(
    Input.Email, Input.Password, Input.FirstName, Input.LastName);
```

---

## 📂 Project Structure

```
VibeLang/
├── Areas/Identity/Pages/Account/   # Login, Register, Logout, AccessDenied, Manage/*
├── Controllers/                    # MVC Controllers with [Authorize(Roles=...)]
│   ├── AdminController.cs          # [Authorize(Roles = "Admin")]
│   ├── HomeController.cs           # [Authorize(Roles = "Admin,User")] + [AllowAnonymous] on Index
│   ├── ProfileController.cs        # Upload/Remove profile picture
│   └── ...
├── Services/
│   ├── IAuthService.cs             # Auth contract
│   ├── AuthService.cs              # Identity implementation
│   └── ...                         # Course, Stats, Achievement, Vocabulary services
├── Repositories/                   # Generic + specialized EF Core repos
├── Models/                         # EF Core entities (ApplicationUser, Course, Lesson...)
├── Views/                          # Razor views with auth-* CSS design system
├── Migrations/                     # EF Core migration history
├── wwwroot/
│   ├── css/auth.css                # Custom auth form design system
│   └── uploads/profile-pictures/   # Runtime user uploads (gitignored)
└── Program.cs                      # DI registration, Identity config, role seeding
```

---

## ⚙️ Running Locally

### Prerequisites
- .NET 9 SDK
- PostgreSQL (local or remote)

### Setup

```bash
# 1. Clone the repo
git clone <repo-url>
cd VibeLang

# 2. Set your connection string in appsettings.json
# "DefaultConnection": "Host=...;Database=vibelang;Username=...;Password=..."

# 3. Apply migrations
dotnet ef database update

# 4. Run
dotnet run
```

The app seeds roles (`Admin`, `User`) and a default admin account on first startup.  
Navigate to `https://localhost:5001`.

---

## 🗂️ Relational Diagram

<div align="center">
  <img src="docs/mermaidjs/erd_diag_relationala.svg" alt="Relational Diagram" style="max-width:100%; height:auto;" />
</div>

---

Copyright (c) 2026 Sugubete Andrei. All rights reserved.
