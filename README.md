<div style="display: flex; align-items: center; gap: 10px; margin-bottom: 20px;">
  <img src="wwwroot/assets/icons/vibelang_transparent_1024.png" alt="VibeLang" width="50" height="50">
  <img src="wwwroot/assets/title.png" alt="VibeLang Title" height="46">
</div>

## 📝 Description

VibeLang is a modern web application for language learning, built on micro-learning and gamification principles. Originally a static prototype, it has been evolved into a dynamic **ASP.NET Core MVC** application with a robust backend and integrated user management.

## 📂 Page Structure (Razor Views)

- **Dashboard** (`/`): Personalized progress tracking, XP, and "Continue last lesson" functionality.
- **Courses** (`/home/courses`): Grid of language courses with chapter and lesson navigation.
- **Lesson** (`/home/lesson`): Interactive vocabulary learning with word translations and examples.
- **Quiz** (`/home/quiz`): Dynamic assessment system with multiple question types (translation, matching, context).
- **Leaderboard** (`/home/leaderboard`): Global XP rankings and user statistics.
- **Vocabulary** (`/home/vocabulary`): Personal dictionary tracking word mastery ("New" vs "Learned").
- **Profile** (`/home/profile`): User account management and personalization.

## 🛠️ Technologies

- **Backend:** .NET 9, ASP.NET Core MVC
- **Database:** PostgreSQL with Entity Framework Core
- **Identity:** ASP.NET Core Identity for secure authentication
- **Frontend:** Razor Views, Bootstrap 5, CSS3, JavaScript
- **CI/CD:** GitHub Actions for automated build and testing

## 🏗️ Architecture

The application follows the MVC (Model-View-Controller) pattern:
- **Models:** Entity Framework Core entities representing the language learning domain and user progress.
- **Controllers:** Logic for handling requests, interacting with the database, and returning views.
- **Views:** Dynamic Razor templates for a responsive and interactive user interface.
- **Data:** `VibeLangDbContext` handles database interactions, with `DbInitializer` for seeding initial content.

---
Copyright (c) 2026 Sugubete Andrei. All rights reserved.
