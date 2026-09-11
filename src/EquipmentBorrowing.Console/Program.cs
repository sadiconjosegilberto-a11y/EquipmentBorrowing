using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

var studentRepo = new InMemoryStudentRepository();
var equipmentRepo = new InMemoryEquipmentRepository();
var borrowingRepo = new InMemoryBorrowingRepository();

studentRepo.Seed(new Student(id: 1, name: "Marc"));
equipmentRepo.Seed(new Equipment(id: 100, name: "Digital Multimeter"));

var service = new BorrowEquipmentService(studentRepo, equipmentRepo, borrowingRepo);

var success = await service.ExecuteAsync(studentId: 1, equipmentId: 100, expectedReturnDate: DateTime.UtcNow.AddDays(7));
Console.WriteLine(success.Success
    ? $"Borrow succeeded. Borrowing #{success.Borrowing!.Id}, status: {success.Borrowing.Status}"
    : $"Borrow failed: {success.FailureReason}");

var failure = await service.ExecuteAsync(studentId: 1, equipmentId: 100, expectedReturnDate: DateTime.UtcNow.AddDays(7));
Console.WriteLine(failure.Success
    ? "Unexpectedly succeeded."
    : $"Borrow failed as expected: {failure.FailureReason}");