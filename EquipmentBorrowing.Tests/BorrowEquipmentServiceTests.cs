using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Infrastructure.Repositories;
using System.Threading.Tasks;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class BorrowEquipmentServiceTests
{
    [Fact]
    public async Task BorrowAsync_ValidStudentAndAvailableEquipment_ReturnsTrue()
    {
        var studentRepo = new InMemoryStudentRepository();
        var equipmentRepo = new InMemoryEquipmentRepository();
        var borrowingRepo = new InMemoryBorrowingRepository();
        var service = new BorrowEquipmentService(studentRepo, equipmentRepo, borrowingRepo);

        bool result = await service.BorrowAsync(1, 1, TestContext.Current.CancellationToken);

        Assert.True(result);
    }

    [Fact]
    public async Task BorrowAsync_UnavailableEquipment_ReturnsFalse()
    {
        var studentRepo = new InMemoryStudentRepository();
        var equipmentRepo = new InMemoryEquipmentRepository();
        var borrowingRepo = new InMemoryBorrowingRepository();
        var service = new BorrowEquipmentService(studentRepo, equipmentRepo, borrowingRepo);

        bool result = await service.BorrowAsync(1, 2, TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    [Fact]
    public async Task BorrowAsync_UnauthorizedStudent_ReturnsFalse()
    {
        var studentRepo = new InMemoryStudentRepository();
        var equipmentRepo = new InMemoryEquipmentRepository();
        var borrowingRepo = new InMemoryBorrowingRepository();
        var service = new BorrowEquipmentService(studentRepo, equipmentRepo, borrowingRepo);

        bool result = await service.BorrowAsync(2, 1, TestContext.Current.CancellationToken);

        Assert.False(result);
    }
}