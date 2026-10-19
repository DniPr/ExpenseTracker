# ExpenseTracker

ExpenseTracker is a full-stack personal finance management application built with ASP.NET Core and .NET 8.

The project uses a separate Web frontend and Web API backend. The frontend communicates with the API through HTTP/JSON requests using JavaScript `fetch`, while the API contains the application business logic, authentication, data access, and background processing.

The application allows users to manage income and expenses, budgets, recurring transactions, financial goals, and view financial statistics through an interactive dashboard.

## Features

### Authentication

- User registration
- User login
- JWT-based authentication
- Protected API endpoints
- User-specific financial data

### Transactions

- Create, edit, delete, and view transactions
- Income and expense transactions
- Search transactions
- Filter by date range
- Filter by transaction type
- Filter by category
- User ownership validation

### Categories

- Default system categories
- User-created personal categories
- Income and expense categories
- Edit and delete personal categories
- Default categories are read-only
- Prevents deleting categories that are already in use

### Budgets

- Monthly budgets
- Yearly budgets
- Overall budgets
- Category-specific budgets
- Duplicate budget prevention
- Validation for current and future periods
- Budget progress tracking

### Recurring Transactions

- Weekly recurring transactions
- Monthly recurring transactions
- Yearly recurring transactions
- Active and inactive recurring transactions
- Optional end dates
- Automatic next execution date calculation
- Automatic transaction generation through a background service
- Catch-up processing for missed recurring periods
- Duplicate transaction protection

### Financial Goals

- Create and manage savings goals
- Track current and target amounts
- Optional target dates
- Active, completed, and cancelled statuses
- Automatic completion when the target amount is reached

### Dashboard

The dashboard provides an overview of the user's finances, including:

- Current balance
- Monthly income
- Monthly expenses
- Monthly savings
- Expenses by category
- Last six months summary
- Budget progress
- Recent transactions
- Active financial goals
- Upcoming recurring transactions
- Interactive charts using Chart.js

## Architecture

The application follows a layered architecture:

```text
ExpenseTracker.Web
        |
        | HTTP / JSON
        v
ExpenseTracker.Api
        |
        v
ExpenseTracker.Services
        |
        v
ExpenseTracker.Data
        |
        v
ExpenseTracker.Data.Models
        |
        v
SQL Server
```

The Web project does not reference the backend projects directly.

Communication between the frontend and backend happens through REST API requests.

## Solution Structure

```text
ExpenseTracker
├── ExpenseTracker.Api
├── ExpenseTracker
│   └── ExpenseTracker.Web
├── ExpenseTracker.Services
├── ExpenseTracker.Data
├── ExpenseTracker.Data.Models
├── ExpenseTracker.DTOs
└── ExpenseTracker.Tests
```

### Project Responsibilities

- **ExpenseTracker.Api** — REST API, controllers, JWT authentication, CORS, Swagger, and hosted background processing
- **ExpenseTracker.Web** — ASP.NET Core MVC frontend, Razor Views, JavaScript, Bootstrap, and Chart.js
- **ExpenseTracker.Services** — business logic and service interfaces
- **ExpenseTracker.Data** — Entity Framework Core DbContext and migrations
- **ExpenseTracker.Data.Models** — entities and enums
- **ExpenseTracker.DTOs** — request and response models used by the API
- **ExpenseTracker.Tests** — NUnit unit tests using EF Core InMemory

## Technologies

### Backend

- C#
- .NET 8
- ASP.NET Core Web API
- ASP.NET Core Identity
- JWT Authentication
- Entity Framework Core
- SQL Server
- LINQ
- Dependency Injection
- Hosted Background Services
- Swagger / OpenAPI

### Frontend

- ASP.NET Core MVC
- Razor Views
- JavaScript
- Fetch API
- HTML
- CSS
- Bootstrap
- Chart.js

### Testing

- NUnit
- Entity Framework Core InMemory
- Moq
- Coverlet
- Microsoft.NET.Test.Sdk

## Main Entities

### ApplicationUser

Represents an authenticated user and owns the user's personal finance data.

### Transaction

Represents an income or expense operation.

### Category

Represents a transaction category.

Categories can be:

- Default system categories
- Personal user categories

### Budget

Represents a monthly or yearly spending limit.

Budgets can be:

- Overall
- Category-specific

### RecurringTransaction

Represents a financial operation that repeats at a defined interval.

Supported intervals:

- Weekly
- Monthly
- Yearly

### FinancialGoal

Represents a savings target with progress and status tracking.

## Background Processing

ExpenseTracker includes a hosted background service responsible for processing recurring transactions.

The worker periodically checks for active recurring transactions whose next execution date has been reached.

When a recurring transaction is due, the application:

1. Checks whether the transaction has already been generated.
2. Creates the corresponding transaction when necessary.
3. Advances the next execution date.
4. Processes multiple missed periods when necessary.
5. Deactivates the recurring transaction when its end date has passed.

This allows recurring financial operations to be processed automatically without requiring manual user interaction.

## API Modules

The API contains endpoints for:

- Authentication
- Transactions
- Categories
- Budgets
- Recurring Transactions
- Financial Goals
- Dashboard

Swagger is available while running the API in development mode.

## Testing

The project includes unit tests for the core business logic:

- `TransactionService`
- `BudgetService`
- `RecurringTransactionService`
- `FinancialGoalService`
- `RecurringTransactionProcessorService`

The tests cover:

- Successful operations
- User ownership rules
- Filtering
- Category validation
- Date validation
- Duplicate prevention
- Budget rules
- Financial goal status rules
- Recurring transaction scheduling
- Background processing
- Catch-up processing for missed recurring periods

## Getting Started

### Requirements

- .NET 8 SDK
- SQL Server or SQL Server Express
- Visual Studio 2022 or another .NET-compatible IDE

### 1. Clone the repository

```bash
git clone https://github.com/DniPr/ExpenseTracker.git
cd ExpenseTracker
```

### 2. Configure the database

Update the connection string in the API configuration if your SQL Server instance is different.

Example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=ExpenseTrackerDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### 3. Configure JWT

The API expects the JWT signing key through .NET User Secrets.

Example:

```bash
dotnet user-secrets set "Jwt:Key" "your-secure-development-key" --project ExpenseTracker.Api
```

### 4. Apply migrations

```bash
dotnet ef database update --project ExpenseTracker.Data --startup-project ExpenseTracker.Api
```

### 5. Run the application

Start both projects:

```text
ExpenseTracker.Api
ExpenseTracker.Web
```

When using Visual Studio, both projects can be configured as multiple startup projects.

The Web frontend communicates with the API through HTTP requests.

## Default Development URLs

By default, the application uses:

```text
Web:
https://localhost:7055

API:
https://localhost:7063
```

The frontend API base URL is configured in:

```text
ExpenseTracker/wwwroot/js/api.js
```

## Project Purpose

This project was built as a portfolio application to practice and demonstrate:

- ASP.NET Core application architecture
- REST API development
- JWT authentication
- Entity Framework Core
- Relational data modeling
- Service-layer business logic
- JavaScript API integration
- Background processing
- Validation and authorization rules
- Unit testing
- Clean project organization

## Author

**DniPr**

GitHub:

https://github.com/DniPr

## License

This project is intended for educational and portfolio purposes.
