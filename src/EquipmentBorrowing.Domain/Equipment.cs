namespace EquipmentBorrowing.Domain;

public class Equipment
{
    public int Id { get; private set; }
    public string Identifier { get; private set; } = string.Empty;
    public string NormalizedIdentifier { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsAvailable { get; private set; } = true;

    private Equipment() { }

    public Equipment(string identifier, string name, string? description = null)
        : this(0, identifier, name, description)
    {
    }

    public Equipment(int id, string identifier, string name, string? description = null)
    {
        Id = id;
        UpdateDetails(identifier, name, description);
        IsAvailable = true;
    }

    public Equipment(int id, string name)
        : this(id, $"EQ-{id}", name, null)
    {
    }

    public void UpdateDetails(string identifier, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Equipment identifier is required.", nameof(identifier));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Equipment name is required.", nameof(name));

        Identifier = identifier.Trim();
        NormalizedIdentifier = Identifier.ToUpperInvariant();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void MarkBorrowed() => IsAvailable = false;
    public void MarkReturned() => IsAvailable = true;
}