using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.ToTable("Borrowings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedOnAdd();

        builder.Property(b => b.StudentId)
            .IsRequired();

        builder.Property(b => b.EquipmentId)
            .IsRequired();

        builder.Property(b => b.DateBorrowed)
            .IsRequired();

        builder.Property(b => b.ExpectedReturnDate)
            .IsRequired();

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Foreign Key Relationships
        builder.HasOne(b => b.Student)
            .WithMany()
            .HasForeignKey(b => b.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Equipment)
            .WithMany()
            .HasForeignKey(b => b.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance on queries and foreign keys
        builder.HasIndex(b => b.StudentId);
        builder.HasIndex(b => b.EquipmentId);
        builder.HasIndex(b => b.Status);
    }
}
