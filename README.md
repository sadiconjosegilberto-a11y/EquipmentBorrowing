A. Actors

	Student - expects to check equipment availability, request available equipment, and return borrowed equipment.
	Labaratory Staff/Administrator - expects the system to enforce borrowing rules, track equipment status, and manage records.

B. Use Cases

	| 	    Item       |											    	Description                                                                           |
	|------------------|------------------------------------------------------------------------------------------------------------------------------------------|
	| Use Case         | Borrow Equipment																														  |
	| Primary Actor	   | Student																																  |
	| Preconditions	   | Student exists and ism allowed to borrow; Equipment exists and is available; Student has not reached the maximum active borrowing limit. |
	| Main Action	   | The Student submits a request to borrow an available piece of equipment. The system validates all rules and creates a borrowing record.  |
	| Expected Result  | Borrowing record is created with active status; equipment status changes to unavailable.												  |
	| Possible Failure | Equipment is missing or unavailable; student is not allowed to borrow; or student exceeded mas active borrowings.						  |

	|       Item   	   |                                 Description                                    |
	|------------------|--------------------------------------------------------------------------------|
	| Use Case		   | Return Equipment															    |
	| Primary Actor    | Student																	    |
	| Preconditions    | Active borrowing record exists for the equipment and student.				    |
	| Main Action      | Student returns equipment. The system marks the borrowing rescord as returned. |
	| Expected Result  | Borrowing status updates to "Returned"; equipment becomes available again.		|
	| Possible Failure | Borrowing record does not exist or equipment is already marked as returned.    |

	|       Item       |                           Description                                  |
	|------------------|------------------------------------------------------------------------|
	| Use Case         | Find Available Equipmrnt												|
	| Primary Actor    | Student																|
	| Preconditions    | Equipment items are cataloged in the system.						    |
	| Main Action	   | Students searches or requests a list of currently available equipment. |
	| Expected Result  | System returns a list of equipment with an available status.		    |
	| Possible Failure | System contains no equipment or no  items currently available.			|

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
		Infomartion: Borrowing ID, Student ID, Equipment ID, Borrowed Date, Expected Return Date, Status(Active/Returned).
		Rules/State: Represents the transaction state and return conditions.
		Not Responsible For: Directly storing full student profiles or hardware catalog management.

D. Part I – Architecture Explanation

	1. Solution Structure
		* Domain: Contains the important concepts and rules belonging to the problem itself.
		* Application: Contains operations or use cases performed by the application, coordinating domain objects.
		* Infrastructure: Contains implementations concerned with external technical mechanisms, such as in-memory storage.
		* Tests: Contains automated tests for application or domain behavior.

	2. Dependency Direction

		```text
		EquipmentBorrowing.ConsoleApp
            │               │
            ▼               ▼
        Application ◄── Infrastructure
            │               │
            └──────┬───────┘
                   ▼
                Domain
		```
			
	3. Use Case Mapping

	```text
	Actor: Student.
	Use Case: Borrow Equipment.
	Application Service: BorrowEquipmentService.
	Domain Objects Used: Student, Equipment, Borrowing, Borrowing Status.
	Repository Interfaces Used: IsStudentRepository, IEquipmentRepository, IBorrowingRepository.
	Infrastructure Implementations Used: InMemoryStudentRepository, InMemoryEquipmentRepository, InMemoryBorrowingRepository.
	```

	4. Reflection

	1. Why should the application service depend on a repository interface instead of directly depending on a database implementation?
		So that it prevents the application layer from needing to know the data source, making it so that by allowing data mechanism it can change without alltering business logic.

	2. Which parts of your current solution could remain unchanged if SQLite were added later?
		Its Domain and Application Layer.

	3. Which project would eventually contain Avalonia Views?
		It would be a new executable project for UI at the top of the dependency graph.

	4. Should an Avalonia button directly execute database queries? Why or why not?
		No. UI should only trigger the application services. Its violating the separation concerns.

	5. What part of your implementation represents the actual business operation requested by the actor?
		The BorrowAsync method in the BorrowEquipmentService.
