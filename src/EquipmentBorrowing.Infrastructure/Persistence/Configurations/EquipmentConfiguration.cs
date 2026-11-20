using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Identifier)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.NormalizedIdentifier)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.NormalizedIdentifier)
            .IsUnique();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.IsAvailable)
            .IsRequired();

        builder.HasData(
            new Equipment(101, "EQ-101", "Digital Multimeter"),
            new Equipment(102, "EQ-102", "Oscilloscope"),
            new Equipment(103, "EQ-103", "Function Generator"),
            new Equipment(104, "EQ-104", "Soldering Station"),
            new Equipment(105, "EQ-105", "Logic Analyzer"),
            new Equipment(106, "EQ-106", "DC Power Supply"),
            new Equipment(107, "EQ-107", "Breadboard Kit"),
            new Equipment(108, "EQ-108", "Component Tester")
        );
    }
}