using System;

namespace EquipmentBorrowing.Application.Services;

internal static class PersistenceErrorHelper
{
    public static (bool IsUnknownOutcome, string Message) Describe(Exception ex)
    {
        var exceptionTypeName = ex.GetType().Name;

        if (exceptionTypeName == "DbUpdateConcurrencyException")
            return (false, "The record was modified or removed by another operation. Refresh and try again.");

        if (exceptionTypeName == "DbUpdateException")
        {
            var inner = ex.InnerException;
            var innerMessage = inner?.Message ?? string.Empty;
            var sqlState = inner?.GetType().GetProperty("SqlState")?.GetValue(inner)?.ToString();

            if (sqlState == "23505" ||
                innerMessage.Contains("23505", StringComparison.Ordinal) ||
                innerMessage.Contains("unique", StringComparison.OrdinalIgnoreCase))
                return (false, "Operation rejected by database uniqueness or single-active-borrowing constraint.");

            if (sqlState == "23503" ||
                innerMessage.Contains("23503", StringComparison.Ordinal) ||
                innerMessage.Contains("foreign key", StringComparison.OrdinalIgnoreCase))
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