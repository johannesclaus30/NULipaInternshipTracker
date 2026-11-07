# 🎓 NULipaInternshipTracker

> **A Cross-Platform Mobile & Desktop Application for Student Internship Matching, Tracking, and Partner Host Management.**

[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/en-us/apps/maui)
[![C#](https://img.shields.io/badge/Language-C%23-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Backend](https://img.shields.io/badge/Backend-PHP%20REST%20API-777BB4?logo=php&logoColor=white)](https://www.php.net/)
[![Architecture](https://img.shields.io/badge/Architecture-MVVM-blue)](https://learn.microsoft.com/en-us/dotnet/architecture/maui/)
[![Platforms](https://img.shields.io/badge/Platforms-Android%20%7C%20iOS%20%7C%20Windows-lightgrey)](https://dotnet.microsoft.com/en-us/apps/maui)

---

## 📌 Project Overview

**NULipaInternshipTracker** is a cross-platform client application engineered to manage and streamline academic internship workflows for students and host training establishments (HTEs)[cite: 1]. Developed for **National University Lipa**, the system replaces disconnected communication channels by providing a centralized bridge where students can browse partner opportunities, submit applications, and monitor internship approval states, while partner companies can manage job postings and screen student applicants in real time[cite: 1].

Built with **.NET MAUI (Multi-platform App UI)** and **C#**, the application leverages the **Model-View-ViewModel (MVVM)** pattern to deliver responsive, native experiences across mobile (Android, iOS) and desktop (Windows) platforms backed by a centralized PHP REST API[cite: 1].

> **Note on Public Repository:** This repository contains a clean, showcase-ready release of the .NET MAUI frontend client[cite: 1]. API keys, server endpoint configs, and production student/company records have been omitted for security and institutional privacy compliance.

---

## ✨ Key Features

### 👨‍🎓 Student Portal
- **Opportunity Discovery:** Browse and search approved internship openings published by partner companies with detailed role requirements[cite: 1].
- **Application Submission:** Submit internship applications directly through structured forms linked to user profiles[cite: 1].
- **Real-Time Application Tracking (`MyApplications`):** Monitor live application statuses (Pending, Accepted, Under Review) via data-bound observable collections[cite: 1].
- **Student Profile Management:** Maintain academic credentials, contact details, and resume attachments[cite: 1].

### 🏢 Partner / Company Console
- **Company Dashboard:** Centralized view summarizing applicant pipelines and active internship listings[cite: 1].
- **Post & Manage Openings:** Create, edit, and deactivate internship job listings with role qualifications, department tags, and vacancy counts[cite: 1].
- **Applicant Review:** Inspect student submissions and update application outcomes[cite: 1].
- **Company Profile:** Update corporate information, address, and training supervisor contact details[cite: 1].

### 🔒 Core Architecture & Infrastructure
- **MVVM Pattern:** Strict separation of concerns between UI definitions (XAML) and business logic/data manipulation via ViewModels[cite: 1].
- **Service-Oriented Client (`ApiService`):** Centralized HTTP service wrapper handling asynchronous REST requests, token management, and error handling[cite: 1].
- **Session Persistence (`UserSession`):** Secure in-memory session handling preserving authenticated state across application navigation flows[cite: 1].
- **Custom Design System:** Branded typography utilizing Montserrat and Open Sans fonts[cite: 1] with custom SVG vector assets and centralized resource dictionaries[cite: 1].

---

## 🛠️ Tech Stack & Architecture

- **Client Framework:** [.NET MAUI (.NET 8)](https://dotnet.microsoft.com/en-us/apps/maui)[cite: 1]
- **Language:** C#[cite: 1], XAML (UI Definition)[cite: 1]
- **Pattern:** Model-View-ViewModel (MVVM)[cite: 1]
- **Networking:** `HttpClient` / `System.Text.Json`[cite: 1]
- **Styling & Assets:** Custom XAML Resource Dictionaries[cite: 1], Montserrat & Open Sans Typefaces[cite: 1], SVG Vector Icons[cite: 1]
- **Backend Service:** PHP RESTful Web Services[cite: 1]
- **Target Platforms:** Android, iOS, Windows, MacCatalyst[cite: 1]

---

## 📁 Repository Structure

```text
NULipaInternshipTracker/
├── Models/                     # Core data transfer entities[cite: 1]
│   ├── Company.cs              # Partner enterprise schema[cite: 1]
│   ├── JobApplication.cs       # Application submission and status tracking schema[cite: 1]
│   ├── JobPosting.cs           # Internship opening entity[cite: 1]
│   └── User.cs                 # User account and role definition[cite: 1]
├── Services/                   # Communication and utility services[cite: 1]
│   ├── ApiService.cs           # Central HTTP REST client handling backend requests[cite: 1]
│   └── UserSession.cs          # Active session and identity state tracker[cite: 1]
├── ViewModels/                 # MVVM ViewModel layers with observable bindings[cite: 1]
│   ├── InternshipViewModel.cs  # Internship listings and application workflow logic[cite: 1]
│   └── MyApplicationsViewModel.cs # Student application pipeline and status updates[cite: 1]
├── Views/                      # XAML views and code-behinds[cite: 1]
│   ├── ApplicationFormPage.xaml# Student application submission form[cite: 1]
│   ├── CompanyDashboardPage.xaml # Partner management console[cite: 1]
│   ├── CompanyProfilePage.xaml # Enterprise profile management[cite: 1]
│   ├── ForgotPasswordPage.xaml # Credential reset flow[cite: 1]
│   ├── LoginPage.xaml          # Role-based authentication[cite: 1]
│   ├── ManageOpeningPage.xaml  # Partner job posting management[cite: 1]
│   ├── MyApplicationsPage.xaml #
