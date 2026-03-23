# VibeLang - Project Documentation & Guidelines

VibeLang is an ASP.NET Core MVC application designed for language learning, focusing on micro-learning and gamification.

## 🚀 Tech Stack

- **Framework:** ASP.NET Core MVC (.NET 9)
- **Database:** PostgreSQL with Entity Framework Core
- **Identity:** ASP.NET Core Identity (Customized with `ApplicationUser`)
- **Frontend:** Razor Views, Bootstrap 5, CSS3, Minimal JavaScript
- **CI/CD:** GitHub Actions (Current workflow is configured for GitHub Pages, but should be updated for .NET deployment)

## 📂 Core Structure

- **Controllers/**: Handles application logic (Home, Languages, Chapters).
- **Models/**: Contains database entities (`Language`, `Course`, `Chapter`, `Lesson`, `VocabularyWord`, `Quiz`, `QuizQuestion`, `QuizOption`) and progress tracking models.
- **Views/**: Razor templates for the UI.
- **Data/**: DbContext and Seed data (`DbInitializer`).
- **Migrations/**: Entity Framework Core database migrations.
- **wwwroot/**: Static assets (CSS, JS, images, icons).

## 🛠️ Key Components & Models

- **Content Hierarchy:** Course -> Chapter -> Lesson.
- **Dynamic Lesson Parser:** Lessons use a `ContentJson` field to store interactive quizzes. The frontend (`lesson-parser.js`) handles:
    - Tip 1: RO -> EN with word bank hints.
    - Tip 2: EN -> RO with free text input.
    - Tip 3: Matching pairs grid.
    - Tip 4: Context/Information display.
- **Normalization:** Custom logic for Romanian character normalization (ă, î, ș, ț, â) during answer comparison.
- **User Progress:** Tracks XP and streaks. `UserId` in progress tables is a `string` linked to `AspNetUsers.Id`.

## 📝 Development Guidelines

### 1. Database & Migrations
- Always use Entity Framework Core migrations for schema changes.
- Ensure `DbInitializer.cs` is updated if new seed data is required.
- **Note:** Progress tables use `string UserId` to match ASP.NET Identity.

### 2. Identity & User Context
- Use `ApplicationUser` for user-related data (includes `FirstName`, `LastName`).
- Check for user authentication using `UserManager<ApplicationUser>` and `User.Identity.IsAuthenticated`.

### 3. UI/UX Standards
- Adhere to the existing Bootstrap 5 styling in `wwwroot/css/site.css` and `wwwroot/css/styles.css`.
- Maintain the gamified look and feel (cards, badges, progress bars).
- Keep URLs lowercase and query strings lowercase (configured in `Program.cs`).

### 4. Code Quality
- Follow standard C# naming conventions (PascalCase for classes/methods, camelCase for local variables).
- Use `async/await` for all database and I/O operations.
- Ensure proper validation in both Models (Data Annotations) and Controllers.

## 🎯 Current Phase
The project has moved from a static HTML/CSS prototype to a dynamic .NET application. The backend structure is in place, and core views are integrated with the database.

## 🔐 Security
- Never hardcode connection strings (use `appsettings.json` and environment variables).
- Protect Identity secrets and sensitive user data.
- Use `[ValidateAntiForgeryToken]` on all POST actions.
