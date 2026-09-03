using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;

namespace EquipmentBorrowing.ConsoleApp;

class Program
{
    static async Task Main(string[] args)
    {
        // Manual Dependency Injection
        var studentRepo = new InMemoryStudentRepository();
        var equipmentRepo = new InMemoryEquipmentRepository();
        var borrowingRepo = new InMemoryBorrowingRepository();

        var service = new BorrowEquipmentService(studentRepo, equipmentRepo, borrowingRepo);

        Console.WriteLine("--- Equipment Borrowing System Demonstration ---");

        // Successful Case: Student 1 (Alice) borrows Equipment 1 (Multimeter)
        Console.WriteLine("\nTest 1: Valid student requesting available equipment.");
        bool success1 = await service.BorrowAsync(studentId: 1, equipmentId: 1);
        Console.WriteLine($"Result: {(success1 ? "SUCCESS" : "FAILED")}");

        // Failure Case: Student 1 (Alice) requests Equipment 2 (Oscilloscope - unavailable)
        Console.WriteLine("\nTest 2: Valid student requesting unavailable equipment.");
        bool success2 = await service.BorrowAsync(studentId: 1, equipmentId: 2);
        Console.WriteLine($"Result: {(success2 ? "SUCCESS" : "FAILED")}");

        // Failure Case: Student 2 (Bob - not allowed) requests Equipment 1 (Multimeter - now unavailable)
        Console.WriteLine("\nTest 3: Unauthorized student requesting equipment.");
        bool success3 = await service.BorrowAsync(studentId: 2, equipmentId: 1);
        Console.WriteLine($"Result: {(success3 ? "SUCCESS" : "FAILED")}");

        Console.ReadLine();
    }
}