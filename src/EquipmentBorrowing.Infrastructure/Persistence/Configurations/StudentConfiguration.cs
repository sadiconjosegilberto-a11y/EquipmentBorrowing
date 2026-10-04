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

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.IsAllowedToBorrow)
            .IsRequired();

        // Seed initial students
        builder.HasData(
            new Student(1, "Alice Santos"),
            new Student(2, "Ben Cruz"),
            new Student(3, "Carla Reyes"),
            new Student(4, "Diego Lim"),
            new Student(5, "Eva Tan")
        );
    }
}
