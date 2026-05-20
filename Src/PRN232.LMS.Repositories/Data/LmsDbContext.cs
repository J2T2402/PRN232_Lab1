using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Data;

public class LmsDbContext(DbContextOptions<LmsDbContext> options) : DbContext(options)
{
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.HasKey(x => x.SemesterId);
            entity.Property(x => x.SemesterName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.StartDate).IsRequired();
            entity.Property(x => x.EndDate).IsRequired();
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(x => x.SubjectId);
            entity.Property(x => x.SubjectCode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.SubjectName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Credit).IsRequired();
            entity.HasIndex(x => x.SubjectCode).IsUnique();
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(x => x.CourseId);
            entity.Property(x => x.CourseName).HasMaxLength(100).IsRequired();

            entity.HasOne(x => x.Semester)
                .WithMany(x => x.Courses)
                .HasForeignKey(x => x.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Subject)
                .WithMany(x => x.Courses)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(x => x.StudentId);
            entity.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DateOfBirth).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(x => x.EnrollmentId);
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.EnrollDate).IsRequired();

            entity.HasOne(x => x.Student)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Course)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.StudentId, x.CourseId }).IsUnique();
        });
    }
}
