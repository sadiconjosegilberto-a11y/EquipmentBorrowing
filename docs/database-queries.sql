-- ====================================================================
-- Laboratory Activity 3: Campus Equipment Borrowing System
-- Database Demonstration Queries (SQLite / Relational Schema)
-- ====================================================================

-- 1. Basic Retrieval
-- Retrieve all equipment records from the database.
SELECT 
    Id,
    Name,
    IsAvailable
FROM Equipment;

-- 2. Filtering
-- Retrieve only the equipment items that are currently available for borrowing.
SELECT 
    Id,
    Name,
    IsAvailable
FROM Equipment
WHERE IsAvailable = 1;

-- 3. Join
-- Retrieve active borrowings together with their associated student and equipment information.
SELECT 
    s.Name AS Student,
    e.Name AS Equipment,
    b.DateBorrowed AS Borrowed,
    b.ExpectedReturnDate AS Due
FROM Borrowings AS b
INNER JOIN Students AS s ON b.StudentId = s.Id
INNER JOIN Equipment AS e ON b.EquipmentId = e.Id
WHERE b.Status = 'Active' OR b.Status = 0;

-- 4. Aggregate Queries
-- A. Count the number of currently active borrowings in the campus system.
SELECT 
    COUNT(*) AS ActiveBorrowingCount
FROM Borrowings
WHERE Status = 'Active' OR Status = 0;

-- B. Count active borrowings grouped by student to monitor borrowing limits (max 3).
SELECT 
    s.Id AS StudentId,
    s.Name AS StudentName,
    COUNT(b.Id) AS ActiveBorrowings
FROM Students AS s
LEFT JOIN Borrowings AS b ON s.Id = b.StudentId AND (b.Status = 'Active' OR b.Status = 0)
GROUP BY s.Id, s.Name;

-- 5. Update
-- A. Update equipment availability status when an item is borrowed or returned.
UPDATE Equipment
SET IsAvailable = 0
WHERE Id = 101;

-- B. Mark a borrowing record as returned when equipment is turned in.
UPDATE Borrowings
SET Status = 'Returned'
WHERE Id = 1;
