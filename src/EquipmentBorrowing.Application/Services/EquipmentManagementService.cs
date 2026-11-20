using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public record EquipmentOperationResult(bool Success, string Message, Equipment? Equipment = null, bool IsUnknownOutcome = false);

public class EquipmentManagementService
{
    private readonly IEquipmentRepository _equipmentRepository;

    public EquipmentManagementService(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }

    public async Task<EquipmentOperationResult> RegisterAsync(
        string identifier,
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return new EquipmentOperationResult(false, "Equipment identifier is required.");

        if (string.IsNullOrWhiteSpace(name))
            return new EquipmentOperationResult(false, "Equipment name is required.");

        var normalized = identifier.Trim().ToUpperInvariant();

        try
        {
            if (await _equipmentRepository.ExistsByNormalizedIdentifierAsync(normalized, null, cancellationToken))
                return new EquipmentOperationResult(false, $"Equipment identifier '{identifier.Trim()}' already exists.");

            var equipment = new Equipment(identifier, name, description);
            await _equipmentRepository.AddAsync(equipment, cancellationToken);
            return new EquipmentOperationResult(true, "Equipment registered as available.", equipment);
        }
        catch (ArgumentException ex)
        {
            return new EquipmentOperationResult(false, ex.Message);
        }
        catch (Exception ex)
        {
            var (unknown, msg) = PersistenceErrorHelper.Describe(ex);
            return new EquipmentOperationResult(false, msg, null, unknown);
        }
    }

    public async Task<EquipmentOperationResult> UpdateAsync(
        int equipmentId,
        string identifier,
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        if (equipmentId <= 0)
            return new EquipmentOperationResult(false, "Select an existing equipment item to update.");

        if (string.IsNullOrWhiteSpace(identifier))
            return new EquipmentOperationResult(false, "Equipment identifier is required.");

        if (string.IsNullOrWhiteSpace(name))
            return new EquipmentOperationResult(false, "Equipment name is required.");

        var normalized = identifier.Trim().ToUpperInvariant();

        try
        {
            var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
            if (equipment is null)
                return new EquipmentOperationResult(false, "Selected equipment record no longer exists.");

            if (await _equipmentRepository.ExistsByNormalizedIdentifierAsync(normalized, equipmentId, cancellationToken))
                return new EquipmentOperationResult(false, $"Equipment identifier '{identifier.Trim()}' is already assigned to another item.");

            equipment.UpdateDetails(identifier, name, description);
            await _equipmentRepository.UpdateAsync(equipment, cancellationToken);
            return new EquipmentOperationResult(true, "Equipment descriptive details updated.", equipment);
        }
        catch (ArgumentException ex)
        {
            return new EquipmentOperationResult(false, ex.Message);
        }
        catch (Exception ex)
        {
            var (unknown, msg) = PersistenceErrorHelper.Describe(ex);
            return new EquipmentOperationResult(false, msg, null, unknown);
        }
    }
}