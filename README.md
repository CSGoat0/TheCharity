# The Charity Platform

> A modern, multi-tenant fundraising platform built with ASP.NET Core 8.0, featuring event-driven architecture, background job processing, and a robust role-based authorization system.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
  - [Installation](#installation)
  - [Database Setup](#database-setup)
  - [Configuration](#configuration)
- [API Documentation](#api-documentation)
- [Contact](#contact)

---

<h2 id="overview">📖 Overview</h2>

**The Charity Platform** is a complete fundraising solution that enables organizations to create and manage charitable campaigns, accept donations, and collaborate through shared campaigns. Built with a clean, event-driven architecture, the platform ensures scalability, maintainability, and a seamless experience for all stakeholders.

### Who Uses the Platform?

| User Role | Capabilities |
|-----------|--------------|
| **Super Admin** | Full system oversight, manages all organizations, campaigns, and users |
| **Organization Admin** | Manages their organization, campaigns, sub-admins, and payment info |
| **Sub-Admin** | Creates and manages campaigns (cannot update payment info) |
| **Regular User** | Views campaigns, makes donations, views donor lists |

---

<h2 id="key-features">✨ Key Features</h2>

### Campaign Management
- **Solo Campaigns** – Organizations create and manage independent campaigns.
- **Shared Campaigns** – Multiple organizations collaborate on a single campaign.
- **Invitation System** – Organizations can invite others to join shared campaigns.
- **Campaign Progress Tracking** – Real-time updates on donation progress and milestones.
- **Status Management** – Active, Completed, Expired, Dismissed, Postponed.
- **Deadline Extensions** – Campaign deadlines can be extended with automatic notifications.

### Donations & Payments
- **Secure Payment Processing** – Integration with Paymob payment gateway.
- **Donation History** – Users can view their donation history.
- **Campaign Progress Updates** – Donations automatically update campaign achievements.
- **Milestone Notifications** – Automated emails at 25%, 50%, 75%, and 100% targets.

### Organizations & Users
- **Multi-Tenant Architecture** – Each organization operates independently.
- **Complete Authentication** – Login, Signup, Email Verification, Password Reset.
- **Role-Based Authorization** – Fine-grained permissions with resource-based checks.
- **Soft Delete** – All entities support soft delete for data recovery and audit trails.

### Background Jobs (Hangfire)
- **Campaign Deadline Reminders** – Daily reminders for campaigns ending soon.
- **Auto-Expire Campaigns** – Hourly job to expire campaigns past their deadline.
- **Weekly Campaign Digest** – Weekly summary for SuperAdmins.
- **Expire Old Invites** – Daily cleanup of expired shared campaign invites.

### Notifications & Communication
- **Email Notifications** – Automated emails for campaign creation, completion, milestones, deadlines.
- **Event-Driven Architecture** – Decoupled notifications via events and handlers.
- **Organization Contact Methods** – Multiple contact methods per organization.

---

<h2 id="technology-stack">🛠️ Technology Stack</h2>

| Layer | Technology |
|-------|------------|
| **Backend Framework** | ASP.NET Core 8.0 |
| **ORM** | Entity Framework Core 8.0 |
| **Database** | SQL Server (Azure SQL) |
| **Authentication** | ASP.NET Core Identity + JWT |
| **Authorization** | Custom policies + resource-based handlers |
| **Background Jobs** | Hangfire |
| **Event System** | Custom EventDispatcher + EventHandlers |
| **Payment** | Paymob |
| **Mapping** | Mapperly (Source Generator) |
| **API Documentation** | Swagger / OpenAPI |
| **Deployment** | Azure App Service + Azure SQL + GitHub Actions |

---

<h2 id="architecture">🏗️ Architecture</h2>

```
┌────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                     │
│  • API Controllers (JWT Auth)                           │
│  • Custom Authorization Attributes                      │
│  • Middleware (Global Error Handling)                   │
├────────────────────────────────────────────────────────────┤
│                    BUSINESS LOGIC LAYER                  │
│  • Services (Campaign, Organization, Donation, User)    │
│  • Event Dispatcher + Handlers                         │
│  • Background Jobs (Hangfire)                          │
│  • DTOs + Mappers                                      │
├────────────────────────────────────────────────────────────┤
│                    DATA ACCESS LAYER                     │
│  • Entities + DbContext                                │
│  • Repository Pattern                                  │
│  • Migrations                                          │
└────────────────────────────────────────────────────────────┘
```

### Key Architectural Decisions

| Decision | Rationale |
|----------|-----------|
| **3-Tier Architecture** | Clear separation of concerns, maintainable, testable |
| **Repository Pattern** | Consistent data access, easy to mock for testing |
| **Event-Driven Design** | Loose coupling, easy to add new features |
| **Background Jobs** | Offload heavy operations (notifications, digests) |
| **Custom Authorization** | Fine-grained permissions, secure by design |
| **Soft Delete** | Data recovery and audit trails |

---

<h2 id="getting-started">🚀 Getting Started</h2>

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/the-charity-platform.git
   cd the-charity-platform
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

### Database Setup

1. **Update the connection string** in `appsettings.Development.template.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost,1433;Database=TheCharityDB;User Id=sa;Password=YourStrong@Passw0rd;Trusted_Connection=False;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```

2. **Apply migrations**
   ```bash
   dotnet ef database update
   ```

### Configuration

1. **Rename the template file:**
   ```bash
   cp appsettings.Development.template.json appsettings.Development.json
   ```

2. **Update configuration values:**
   - `Jwt:Key` – Generate a secure key (minimum 32 characters)
   - `EmailSettings` – Configure your SMTP settings
   - `Authentication` – Configure Google/Facebook OAuth (optional)

---

<h2 id="api-documentation">📚 API Documentation</h2>

Once running, Swagger is available at:
```
https://localhost:7204/swagger
```


<h2 id="contact">📧 Contact</h2>

- **Author:** Abdelrahman Nabil
- **Email:** abdelrahmannabil3950@gmail.com
- **LinkedIn:** www.linkedin.com/in/csgabdelrahman
