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

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.IsAvailable)
            .IsRequired();

        // Seed initial equipment
        builder.HasData(
            new Equipment(101, "Digital Multimeter"),
            new Equipment(102, "Oscilloscope"),
            new Equipment(103, "Function Generator"),
            new Equipment(104, "Soldering Station"),
            new Equipment(105, "Logic Analyzer"),
            new Equipment(106, "DC Power Supply"),
            new Equipment(107, "Breadboard Kit"),
            new Equipment(108, "Component Tester")
        );
    }
}
