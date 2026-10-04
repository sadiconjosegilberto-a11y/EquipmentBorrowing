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
        builder.Property(b => b.BorrowedDate).IsRequired();
        builder.Property(b => b.ExpectedReturnDate).IsRequired();
        builder.Property(b => b.Status).IsRequired().HasConversion<int>();

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(b => b.StudentId)
            .IsRequired();

        builder.HasOne<Equipment>()
            .WithMany()
            .HasForeignKey(b => b.EquipmentId)
            .IsRequired();
    }
}