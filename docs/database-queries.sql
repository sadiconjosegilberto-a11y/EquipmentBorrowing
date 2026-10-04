
SELECT * 
FROM EQUIPMENT;


SELECT * 
FROM EQUIPMENT 
WHERE IsAvailable = 1;


SELECT 
    S.Name AS Student, 
    E.Name AS Equipment, 
    B.BorrowedDate, 
    B.ExpectedReturnDate 
FROM BORROWINGS B
JOIN STUDENTS S ON B.StudentId = S.Id
JOIN EQUIPMENT E ON B.EquipmentId = E.Id
WHERE B.Status = 0;


SELECT 
    StudentId, 
    COUNT(*) AS ActiveBorrowings 
FROM BORROWINGS 
WHERE Status = 0 
GROUP BY StudentId;


UPDATE EQUIPMENT 
SET IsAvailable = 0 
WHERE Id = 1;