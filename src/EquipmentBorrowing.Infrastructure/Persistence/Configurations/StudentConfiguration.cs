using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.StudentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.NormalizedStudentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.NormalizedStudentNumber)
            .IsUnique();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.ContactInfo)
            .HasMaxLength(150);

        builder.Property(s => s.IsAllowedToBorrow)
            .IsRequired();

        builder.HasData(
            new Student(1, "2026-0001", "Alice Santos"),
            new Student(2, "2026-0002", "Ben Cruz"),
            new Student(3, "2026-0003", "Carla Reyes"),
            new Student(4, "2026-0004", "Diego Lim"),
            new Student(5, "2026-0005", "Eva Tan")
        );
    }
}