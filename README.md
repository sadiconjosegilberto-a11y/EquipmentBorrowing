# Campus Equipment Borrowing System

## Group Member
- **SADICON, JOSE GILBERTO F.** (`2024300607`)
- **ARPON, MARC RANDOLPH H.** (`024300008`)

## Laboratory Activity 1 — Domain and Application Layer

### Overview
This project is a simple Campus Equipment Borrowing System built with layered architecture:

- **EquipmentBorrowing.Domain**: Holds models (`Student`, `Equipment`, `Borrowing`, and `BorrowingStatus`).
- **EquipmentBorrowing.Application**: Contains repository interfaces and services like `BorrowEquipmentService`.
- **EquipmentBorrowing.Infrastructure**: In-memory repository storage.
- **EquipmentBorrowing.Console**: Console test app from Lab 1.

### Borrowing Rules
- The student must exist and have borrowing privileges.
- The equipment must exist and be available.
- A student cannot borrow more than 3 active items at a time.

---

## Laboratory Activity 2 — Avalonia Desktop UI

### 1. Desktop Project
`EquipmentBorrowing.Desktop` adds a graphical interface using Avalonia and the MVVM pattern.

Its main jobs are:
- Show available equipment and active borrowings.
- Take user input (choose student, equipment, return date).
- Send actions to application services through ViewModels.
- Show success or error messages to the user.

It references the Application and Infrastructure layers to run the system. No business logic is placed in the Desktop project.

### 2. Updated Architecture

```
Avalonia View (EquipmentView, BorrowingsView)
     │
     │ Data Binding / Commands
     ▼
ViewModel (EquipmentViewModel, BorrowingsViewModel)
     │
     │ Calls Service
     ▼
Application Service (BorrowEquipmentService, ReturnEquipmentService)
     │
     ├──────────► Domain Models (Student, Equipment, Borrowing)
     │
     ▼
Repository Interface (IEquipmentRepository, IStudentRepository, IBorrowingRepository)
     ▲
     │ Implements
Infrastructure (InMemory Repositories)
```

### 3. Borrow Equipment Flow
1. The user selects a student, an equipment item, and a return date.
2. The user clicks **Borrow Equipment**.
3. `EquipmentViewModel` verifies that required fields are selected (presentation check).
4. `EquipmentViewModel` calls `BorrowEquipmentService.ExecuteAsync(...)`.
5. The service checks the business rules (availability, student status, borrow limit).
6. If valid, the borrowing is created, equipment availability updates to "In Use", and a success message appears.
7. If invalid, the service returns the reason, and an error message is shown.

### 4. Return Equipment Flow
1. The user goes to the **Active Borrowings** section.
2. The user selects an active borrowing from the list.
3. The user clicks **Return Equipment**.
4. `BorrowingsViewModel` calls `ReturnEquipmentService.ExecuteAsync(...)`.
5. The service marks the borrowing as returned and makes the equipment available again.
6. The list refreshes, removing the item from active borrowings, and a success message appears.

### 5. Architectural Reflection

- **Why should the View not call a repository directly?**  
  The View should only handle UI display. Calling a repository directly skips business rules and mixes UI code with data handling.

- **Why should business rules not be implemented in the ViewModel?**  
  Business rules belong in the Application/Domain layer. Putting them in the ViewModel duplicates logic, makes testing harder, and causes inconsistencies across different UIs.

- **What is the responsibility of the ViewModel?**  
  The ViewModel connects the View to the Application layer. It stores UI state, runs commands, checks simple input rules (like empty fields), and calls application services.

- **Why can the existing Application layer work without knowing that Avalonia is being used?**  
  The Application layer only depends on domain models and repository interfaces. It has no dependencies on Avalonia, so it works with any UI.

- **What advantage is gained from registering dependencies in one composition point?**  
  All objects and services are set up in one spot (`App.axaml.cs`). This makes dependencies easy to manage, update, and change without editing multiple files.

- **If the in-memory repository were replaced by SQLite later, which parts of the current interface should remain largely unchanged?**  
  The Views and ViewModels would remain completely unchanged. Only the Infrastructure repository classes and the registrations in `App.axaml.cs` would need to be updated.

## Laboratory Activity 4 — PostgreSQL Integration and Advanced Workflows

### 1. Setup, Configuration, and Execution

#### Prerequisites
- .NET SDK (.NET 8.0 or later)
- PostgreSQL (15+ or 16+) running locally
- EF Core CLI tools (`dotnet tool install --global dotnet-ef`)
- Git

#### Installation & Secret-Free Configuration
1. Clone the repository and restore packages:
   ```bash
   git clone [https://github.com/sadiconjosegilberto-a11y/EquipmentBorrowing.git](https://github.com/sadiconjosegilberto-a11y/EquipmentBorrowing.git)
   cd EquipmentBorrowing
   dotnet restore

2.Configure the local PostgreSQL connection string without committing credentials (via .NET User Secrets or an environment variable):
   ```bash
   cd EquipmentBorrowing.Desktop
   dotnet user-secrets init --project src/EquipmentBorrowing.Desktop
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=campus_borrowing_db;Username=YOUR_PG_USER;Password=YOUR_PG_PASSWORD" --project src/EquipmentBorrowing.Desktop
   (Or set ConnectionStrings__DefaultConnection as an environment variable.)

Database Migrations & Initial Data
1. Apply EF Core migrations to create the PostgreSQL schema, keys, and constraints:
    dotnet ef database update --project src/EquipmentBorrowing.Infrastructure --startup-project src/EquipmentBorrowing.Desktop

2. Initial Seed Data: On startup, the initializer checks whether student or equipment records already exist. Safe initial records are added only when the database is empty so existing records and borrowing histories are never overwritten across restarts.
    Running the Application
    dotnet run --project src/EquipmentBorrowing.Desktop

### 2. Updated Architecture
```
Avalonia View (Equipment, Students, Borrowings, Histories, Search)
     │
     │ Data Binding / Async Commands
     ▼
ViewModel (CommunityToolkit.Mvvm — UI State, Loading & Error Feedback)
     │
     │ Calls Service
     ▼
Application Service (Borrow, Multi-Checkout, Return, Student, Equipment, History/Search)
     │
     ├──────────► Domain Models & Business Rules (BR-01 to BR-19)
     │
     ▼
Repository Interface (IEquipmentRepository, IStudentRepository, IBorrowingRepository)
     ▲
     │ Implements
Infrastructure (EF Core + Npgsql Repositories, Migrations, DbContext Factory)
     │
     ▼
PostgreSQL Database (Keys, Foreign Keys, Unique & Single-Active-Item Indexes)
```

### 3. Multi-Item Checkout Flow (UC-06)
        1. The custodian selects an eligible student, at least two distinct available items, and an expected return date.
        2. The custodian clicks Check Out Selected Items.
        3. The ViewModel performs basic input checks and invokes the application service asynchronously.
        4. Inside a single database transaction, the service revalidates current student eligibility, active borrowing count (at most 3 total), distinct item count (at least 2), return date validity, and item availability.
        5. If valid, all active borrowing records and equipment availability updates are committed together in one atomic transaction.
        6. If any validation or write fails, the transaction rolls back with zero partial changes and displays a clear rejection or error reason.

### 4. Student/Equipment Management & History/Search Flows (UC-07–UC-09, UC-12–UC-16)
        1. The custodian registers or updates student and equipment records with normalized identifier checks (trimmed, case-insensitive uniqueness).
        2. Student eligibility can be toggled between permitted and restricted without canceling or blocking the return of existing active loans.
        3. Selecting a student or equipment item asynchronously loads its complete active and returned borrowing history without freezing the UI.
        4. The borrowing search view asynchronously filters by partial student name, student number, or equipment name, filters by status (Active, Returned, All), and sorts deterministically by checkout or expected return date.

### 5. Architectural and Design Decisions
    - Why are Views and ViewModels kept free of SQL, DbContext, and migrations?
    -   **Keeping all persistence in EquipmentBorrowing.Infrastructure and business validation in EquipmentBorrowing.Application preserves testability, prevents bypassing business rules (BR-01–BR-19), and keeps UI code focused strictly on presentation and state feedback.

    - What DbContext and Dependency Injection lifetime is used, and why?
    - **Short-lived per-operation DbContext instances (created via IDbContextFactory / scoped units of work registered in App.axaml.cs) are used instead of a single application-wide DbContext. In a long-running desktop app, per-operation contexts prevent stale tracked entities across views, avoid concurrency conflicts during asynchronous history/search loading, and guarantee fresh database reads at transaction boundaries.

    - How are transaction boundaries and database integrity enforced?
    - **Multi-item checkout (UC-06) and single borrow/return operations (UC-03, UC-05) execute inside explicit EF Core transactions (BeginTransactionAsync) that revalidate current database state before committing all changes together. PostgreSQL primary keys, restricting foreign keys, normalized unique indexes, and a filtered unique constraint on active borrowings backstop application validation at the database level.

    - How are connection failures and unknown commit outcomes handled?
    - **Persistence exceptions are caught in the service layer and translated into safe, credential-free error messages. If a connection loss interrupts commit confirmation, the system reports an unknown commit outcome without falsely claiming rollback or auto-retrying, prompting the custodian to reconnect and refresh state before retrying.