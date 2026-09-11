# Campus Equipment Borrowing System

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
