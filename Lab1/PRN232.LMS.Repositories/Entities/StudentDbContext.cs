using Microsoft.EntityFrameworkCore;

namespace PRN232.LMS.Repositories.Entities;

public sealed class StudentDbContext : LmsDbContext
{
    public StudentDbContext(DbContextOptions<StudentDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Course>();
        modelBuilder.Ignore<Enrollment>();
        modelBuilder.Ignore<RefreshToken>();
        modelBuilder.Ignore<Semester>();
        modelBuilder.Ignore<Subject>();
        modelBuilder.Ignore<User>();

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52B997AC15F8E");
            entity.HasIndex(e => e.Email, "UQ__Students__A9D1053408BA210E").IsUnique();

            entity.Ignore(e => e.Enrollments);

            entity.Property(e => e.DateOfBirth).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
        });
    }
}
