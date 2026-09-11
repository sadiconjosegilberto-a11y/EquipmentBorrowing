# Campus Equipment Borrowing System

## Laboratory Activity 1 — Domain and Application Layer

### Overview
A layered .NET solution implementing a Campus Equipment Borrowing System. The architecture separates concerns across distinct projects:

| Project | Responsibility |
|---|---|
| `EquipmentBorrowing.Domain` | Core entities: `Student`, `Equipment`, `Borrowing`, `BorrowingStatus` |
| `EquipmentBorrowing.Application` | Application services, repository interfaces |
| `EquipmentBorrowing.Infrastructure` | In-memory repository implementations |
| `EquipmentBorrowing.Console` | Simple console entry point (Lab 1) |

### Domain Models
- **Student** — has an Id, Name, and `IsAllowedToBorrow` flag
- **Equipment** — has an Id, Name, and `IsAvailable` flag with `MarkBorrowed()`/`MarkReturned()` methods
- **Borrowing** — records a borrow event with student, equipment, dates, and status

### Borrowing Rules (enforced in `BorrowEquipmentService`)
1. Student must exist
2. Student must be allowed to borrow
3. Equipment must exist and be available
4. Student must not have reached the maximum of 3 active borrowings

---

## Laboratory Activity 2 — Avalonia Desktop UI

### Desktop Project

`EquipmentBorrowing.Desktop` is an Avalonia application that provides a graphical interface for the borrowing system. It is responsible for:

- Displaying equipment and their availability
- Collecting user input (student, equipment, return date)
- Displaying active borrowings
- Initiating borrow and return operations via ViewModels
- Providing user-facing validation feedback

The Desktop project references `Application` and `Infrastructure`. It does **not** contain any business rules — those remain in the Application and Domain layers.

### Project Structure

```
EquipmentBorrowing/
├── src/
│   ├── EquipmentBorrowing.Domain/          # Entities, value objects
│   ├── EquipmentBorrowing.Application/     # Services, repository interfaces
│   │   ├── Interfaces/
│   │   └── Services/
│   │       ├── BorrowEquipmentService.cs
│   │       └── ReturnEquipmentService.cs
│   ├── EquipmentBorrowing.Infrastructure/  # In-memory repositories
│   │   └── Repositories/
│   └── EquipmentBorrowing.Desktop/         # Avalonia UI
│       ├── Views/
│       │   ├── MainWindow.axaml
│       │   ├── EquipmentView.axaml
│       │   └── BorrowingsView.axaml
│       ├── ViewModels/
│       │   ├── MainWindowViewModel.cs
│       │   ├── EquipmentViewModel.cs
│       │   └── BorrowingsViewModel.cs
│       ├── Assets/
│       │   └── Styles.axaml
│       ├── App.axaml
│       └── App.axaml.cs
└── tests/
    └── EquipmentBorrowing.Tests/
```

### Updated Architecture

```
Avalonia View  (EquipmentView, BorrowingsView)
     │
     │  Binding / Command
     ▼
ViewModel  (EquipmentViewModel, BorrowingsViewModel)
     │
     │  Application Operation
     ▼
Application Service  (BorrowEquipmentService, ReturnEquipmentService)
     │
     ├──────────► Domain  (Student, Equipment, Borrowing)
     │
     ▼
Repository Interface  (IEquipmentRepository, IStudentRepository, IBorrowingRepository)
     ▲
     │
Infrastructure Implementation  (InMemory*Repository)
```

### Dependency Injection

Dependencies are configured in `App.axaml.cs` (the composition root):

```csharp
// Singletons — shared state across views
services.AddSingleton<IStudentRepository>(studentRepo);
services.AddSingleton<IEquipmentRepository>(equipmentRepo);
services.AddSingleton<IBorrowingRepository, InMemoryBorrowingRepository>();

// Services
services.AddTransient<BorrowEquipmentService>();
services.AddTransient<ReturnEquipmentService>();

// ViewModels
services.AddTransient<EquipmentViewModel>();
services.AddTransient<BorrowingsViewModel>();
services.AddSingleton<MainWindowViewModel>();
```

Repositories are singletons so state (borrowed equipment) is preserved when navigating between views.

### Borrow Equipment Flow

1. User selects a **Student** from the ComboBox in the Equipment view.
2. User selects an **Equipment** item from the ListBox.
3. User picks an **Expected Return Date** from the CalendarDatePicker.
4. User clicks **Borrow Equipment**.
5. `EquipmentViewModel.BorrowAsync()` runs:
   - Checks that all fields are filled (presentation validation).
   - Calls `BorrowEquipmentService.ExecuteAsync(studentId, equipmentId, returnDate)`.
6. `BorrowEquipmentService` enforces all business rules (existence, availability, borrowing limit).
7. On success, the borrowing is saved and the equipment list is refreshed — the item now shows "In Use".
8. A status message (green or red) is displayed to the user.

### Return Equipment Flow

1. User navigates to **Active Borrowings**.
2. `BorrowingsViewModel.LoadAsync()` fetches all active borrowings and resolves student/equipment names.
3. User selects a borrowing from the list.
4. User clicks **Return Equipment**.
5. `BorrowingsViewModel.ReturnAsync()` calls `ReturnEquipmentService.ExecuteAsync(borrowingId)`.
6. `ReturnEquipmentService` marks the borrowing as Returned and marks the equipment as Available.
7. The active borrowings list is refreshed — the returned item disappears.
8. A status message confirms the result.

### Architectural Reflection

**Why should the View not call a repository directly?**
The View is responsible only for presentation. If it called repositories directly, it would mix data access concerns with display concerns, making both harder to maintain and test. It also bypasses business rules entirely.

**Why should business rules not be implemented in the ViewModel?**
Business rules belong to the domain. Duplicating them in the ViewModel means they can drift out of sync, be missed in other entry points, and can not be easily unit-tested in isolation.

**What is the responsibility of the ViewModel?**
The ViewModel bridges the View and the application logic. It holds presentation state (selected items, status messages, observable collections), exposes commands, performs presentation-level validation (empty fields), and delegates business operations to application services.

**Why can the existing Application layer work without knowing that Avalonia is being used?**
The Application layer depends only on abstractions (repository interfaces) and the Domain layer. It has no reference to any UI framework, so it can be driven by a console app, a desktop app, an API, or tests — all without modification.

**What advantage is gained from registering dependencies in one composition point?**
The entire dependency graph is assembled in one place (`App.axaml.cs`). Changing an implementation (e.g., swapping the repository) requires a change in exactly one location, not scattered across the codebase.

**If the in-memory repository were replaced by SQLite, which parts would remain unchanged?**
The Domain, Application (services and interfaces), ViewModels, and Views would all remain unchanged. Only the Infrastructure implementations and the DI registrations in `App.axaml.cs` would need to change.

---

## How to Run

```bash
dotnet run --project src/EquipmentBorrowing.Desktop
```

## How to Build

```bash
dotnet build
```

## How to Test

```bash
dotnet test
```
