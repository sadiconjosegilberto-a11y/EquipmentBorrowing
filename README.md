A. Actors

	Student - expects to check equipment availability, request available equipment, and return borrowed equipment.
	Labaratory Staff/Administrator - expects the system to enforce borrowing rules, track equipment status, and manage records.

B. Use Cases

	| 	    Item       |											    	Description                                                                           |
	|------------------|------------------------------------------------------------------------------------------------------------------------------------------|
	| Use Case         | Borrow Equipment																														  |
	| Primary Actor	   | Student																																  |
	| Preconditions	   | Student exists and is allowed to borrow; Equipment exists and is available; Student has not reached the maximum active borrowing limit.  |
	| Main Action	   | The Student submits a request to borrow an available piece of equipment. The system validates all rules and creates a borrowing record.  |
	| Expected Result  | Borrowing record is created with active status; equipment status changes to unavailable.												  |
	| Possible Failure | Equipment is missing or unavailable; student is not allowed to borrow; or student exceeded max active borrowings.						  |

	|       Item   	   |                                 Description                                    |
	|------------------|--------------------------------------------------------------------------------|
	| Use Case		   | Return Equipment															    |
	| Primary Actor    | Student																	    |
	| Preconditions    | Active borrowing record exists for the equipment and student.				    |
	| Main Action      | Student returns equipment. The system marks the borrowing record as returned.  |
	| Expected Result  | Borrowing status updates to "Returned"; equipment becomes available again.		|
	| Possible Failure | Borrowing record does not exist or equipment is already marked as returned.    |

	|       Item       |                           Description                                  |
	|------------------|------------------------------------------------------------------------|
	| Use Case         | Find Available Equipment												|
	| Primary Actor    | Student																|
	| Preconditions    | Equipment items are cataloged in the system.						    |
	| Main Action	   | Student searches or requests a list of currently available equipment.  |
	| Expected Result  | System returns a list of equipment with an available status.		    |
	| Possible Failure | System contains no equipment or no items currently available.			|

C. Domain Concepts

	1. Student 
		Information: Student ID, Name, IsAllowedToBorrow flag, current active borrowing count.
		Rules/State: Tracks eligibility to borrow and enforces active borrowing limits.
		Not Responsible For: Checking equipment state or creating borrowing records.

	2. Equipment
		Information: Equipment ID, Name, IsAvailable flag.
		Rules/State: Tracks whether the item is available or currently borrowed.
		Not Responsible For: Validating student rules or keeping borrowing histories.

	3. Borrowing
		Information: Borrowing ID, Student ID, Equipment ID, Borrowed Date, Expected Return Date, Status (Active/Returned).
		Rules/State: Represents the transaction state and return conditions.
		Not Responsible For: Directly storing full student profiles or hardware catalog management.

D. Part I – Architecture Explanation

	1. Solution Structure
		* Domain: Contains the important concepts and rules belonging to the problem itself.
		* Application: Contains operations or use cases performed by the application, coordinating domain objects.
		* Infrastructure: Contains implementations concerned with external technical mechanisms, such as in-memory storage.
		* EquipmentBorrowing.Desktop: Handles the Avalonia UI presentation layer, displays information, collects user input, and invokes operations through ViewModels.
		* Tests: Contains automated tests for application or domain behavior.

	2. Updated Architecture and Dependency Direction
		```text
		Avalonia View
		      │ Binding / Command
		      ▼
		  ViewModel
		      │ Application Operation
		      ▼
		Application Service
		      │
		  ┌───┴───┐
		  ▼       ▼
		Domain  Repository Interface
		          ▲
		          │
		Infrastructure Implementation
		```
			
	3. Use Case Mapping
		Actor: Student.
		Use Case: Borrow Equipment.
		Application Service: BorrowEquipmentService.
		Domain Objects Used: Student, Equipment, Borrowing, Borrowing Status.
		Repository Interfaces Used: IStudentRepository, IEquipmentRepository, IBorrowingRepository.
		Infrastructure Implementations Used: InMemoryStudentRepository, InMemoryEquipmentRepository, InMemoryBorrowingRepository.

	4. Borrow Equipment Flow
		* The user selects equipment from the interface.
		* The ViewModel collects the input and invokes the application service via a command.
		* The Application Service validates the business rules and executes the operation.
		* The result is communicated back to the user on the screen.

	5. Return Equipment Flow
		* The user selects a borrowing from the Active Borrowings View and issues a return command.
		* The ViewModel invokes the ReturnEquipmentService.
		* The service locates the borrowing, updates its status, and updates equipment availability via the repositories.
		* The interface refreshes to display the updated system state.

	6. Architectural Reflection
		1. Why should the View not call a repository directly? 
		   Views should strictly handle presentation; directly calling repositories bypasses the Application layer where business logic and operations are coordinated.
		2. Why should business rules not be implemented in the ViewModel? 
		   ViewModels handle presentation state; business rules belong in the Domain and Application layers to ensure logic remains independent of the UI.
		3. What is the responsibility of the ViewModel? 
		   To maintain presentation state, handle user commands, expose observable properties, and call application services.
		4. Why can the existing Application layer work without knowing that Avalonia is being used? 
		   The Application layer relies strictly on domain concepts and repository abstractions, keeping it entirely decoupled from the desktop framework.
		5. What advantage is gained from registering dependencies in one composition point? 
		   It centralizes the dependency graph, ensuring ViewModels receive instances through constructors rather than manually instantiating services themselves.
		6. If the in-memory repository were replaced by SQLite later, which parts of the current interface should remain largely unchanged? 
		   The Views and ViewModels would remain unchanged, as the UI does not depend on database infrastructure details.