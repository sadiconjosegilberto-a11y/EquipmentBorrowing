using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Application.Services;

internal static class PersistenceErrorHelper
{
    public static (bool IsUnknownOutcome, string Message) Describe(Exception ex)
    {
        if (ex is DbUpdateConcurrencyException)
            return (false, "The record was modified or removed by another operation. Refresh and try again.");

        if (ex is DbUpdateException dbEx)
        {
            var inner = dbEx.InnerException?.Message ?? string.Empty;
            if (inner.Contains("23505") || inner.Contains("unique", StringComparison.OrdinalIgnoreCase))
                return (false, "Operation rejected by database uniqueness or single-active-borrowing constraint.");

            if (inner.Contains("23503") || inner.Contains("foreign key", StringComparison.OrdinalIgnoreCase))
                return (false, "Operation rejected because a referenced student or equipment record does not exist.");

            return (false, "Database rejected the update. No partial changes were saved.");
        }

        var message = ex.Message;
        if (message.Contains("reading from stream", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("broken pipe", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("forcibly closed", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        {
            return (true, "Connection was interrupted during commit; outcome is unknown. Reconnect and refresh before retrying.");
        }

        return (false, "Database connection or operation failed. Verify PostgreSQL availability and try again.");
    }
}