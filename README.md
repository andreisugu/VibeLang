

# VibeLang

## Description
VibeLang is a web application for language learning, built on micro-learning and gamification principles. In the current phase, the project is 100% static (HTML5, CSS3, Bootstrap, minimal JS), with a shared layout and unified navigation, ready for future .NET/ASP.NET Core expansion.

## Page Structure
- **Dashboard** (index.html): progress, XP, "Continue last lesson" button
- **Courses** (courses.html): grid of language cards, "Start" and "Vocabulary" buttons
- **Lesson** (lesson.html): word, translation, "Repeat" and "Learned" buttons, quiz link
- **Quiz** (quiz.html): question, answer options, check button
- **Leaderboard** (leaderboard.html): XP top table, highlighted user row
- **Vocabulary** (vocabulary.html): word list, "Learned/New" badge, filter
- **Profile** (profile.html): avatar, user data, edit form

## Technologies
- HTML5, Bootstrap 5, CSS3
- GitHub Actions for automatic deploy to GitHub Pages

## Automatic Deploy
On every push to `main`, the site is automatically published to GitHub Pages (see `.github/workflows/gh-pages.yml`).

## .NET-Ready Architecture
The structure and layout are designed for easy migration to ASP.NET Core MVC and SQL Server integration.

---
Copyright (c) 2026 Sugubete Andrei. All rights reserved.
