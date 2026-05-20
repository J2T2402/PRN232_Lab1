using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Data;

public static class LmsDbSeeder
{
    public static async Task SeedAsync(LmsDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (!await context.Semesters.AnyAsync(cancellationToken))
        {
            var semesters = new[]
            {
                new Semester { SemesterName = "Spring 2024", StartDate = new DateTime(2024, 1, 8), EndDate = new DateTime(2024, 5, 15) },
                new Semester { SemesterName = "Summer 2024", StartDate = new DateTime(2024, 5, 20), EndDate = new DateTime(2024, 8, 25) },
                new Semester { SemesterName = "Fall 2024", StartDate = new DateTime(2024, 9, 2), EndDate = new DateTime(2024, 12, 20) },
                new Semester { SemesterName = "Spring 2025", StartDate = new DateTime(2025, 1, 6), EndDate = new DateTime(2025, 5, 12) },
                new Semester { SemesterName = "Summer 2025", StartDate = new DateTime(2025, 5, 19), EndDate = new DateTime(2025, 8, 22) }
            };

            await context.Semesters.AddRangeAsync(semesters, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.Subjects.AnyAsync(cancellationToken))
        {
            var subjects = Enumerable.Range(1, 10)
                .Select(index => new Subject
                {
                    SubjectCode = $"SUB{index:000}",
                    SubjectName = $"Subject {index}",
                    Credit = (index % 3) + 2
                })
                .ToList();

            await context.Subjects.AddRangeAsync(subjects, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.Courses.AnyAsync(cancellationToken))
        {
            var semesterIds = await context.Semesters
                .OrderBy(x => x.SemesterId)
                .Select(x => x.SemesterId)
                .ToListAsync(cancellationToken);

            var subjectIds = await context.Subjects
                .OrderBy(x => x.SubjectId)
                .Select(x => x.SubjectId)
                .ToListAsync(cancellationToken);

            var courses = Enumerable.Range(1, 20)
                .Select(index => new Course
                {
                    CourseName = $"Course {index}",
                    SemesterId = semesterIds[(index - 1) % semesterIds.Count],
                    SubjectId = subjectIds[(index - 1) % subjectIds.Count]
                })
                .ToList();

            await context.Courses.AddRangeAsync(courses, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.Students.AnyAsync(cancellationToken))
        {
            var firstNames = new[]
            {
                "Nguyen", "Tran", "Le", "Pham", "Hoang",
                "Vo", "Dang", "Bui", "Do", "Huynh"
            };

            var lastNames = new[]
            {
                "An", "Binh", "Chi", "Dung", "Giang",
                "Hanh", "Khanh", "Linh", "Minh", "Phuong"
            };

            var students = Enumerable.Range(1, 50)
                .Select(index => new Student
                {
                    FullName = $"{firstNames[(index - 1) % firstNames.Length]} {lastNames[(index - 1) % lastNames.Length]} {index}",
                    Email = $"student{index:000}@prn232.local",
                    DateOfBirth = new DateTime(2000, 1, 1).AddDays(index * 37)
                })
                .ToList();

            await context.Students.AddRangeAsync(students, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        var currentEnrollmentCount = await context.Enrollments.CountAsync(cancellationToken);
        if (currentEnrollmentCount < 500)
        {
            var studentIds = await context.Students
                .OrderBy(x => x.StudentId)
                .Select(x => x.StudentId)
                .ToListAsync(cancellationToken);

            var courseIds = await context.Courses
                .OrderBy(x => x.CourseId)
                .Select(x => x.CourseId)
                .ToListAsync(cancellationToken);

            var existingPairKeys = await context.Enrollments
                .Select(x => $"{x.StudentId}-{x.CourseId}")
                .ToListAsync(cancellationToken);

            var usedPairs = existingPairKeys.ToHashSet(StringComparer.Ordinal);

            var statuses = new[] { "Active", "Completed", "Dropped" };
            var random = new Random(232);
            var enrollmentsToAdd = new List<Enrollment>();

            for (var studentIndex = 0; studentIndex < studentIds.Count && currentEnrollmentCount + enrollmentsToAdd.Count < 500; studentIndex++)
            {
                for (var courseIndex = 0; courseIndex < courseIds.Count && currentEnrollmentCount + enrollmentsToAdd.Count < 500; courseIndex++)
                {
                    var studentId = studentIds[studentIndex];
                    var courseId = courseIds[(studentIndex + courseIndex) % courseIds.Count];
                    var key = $"{studentId}-{courseId}";

                    if (!usedPairs.Add(key))
                    {
                        continue;
                    }

                    enrollmentsToAdd.Add(new Enrollment
                    {
                        StudentId = studentId,
                        CourseId = courseId,
                        EnrollDate = new DateTime(2024, 1, 1).AddDays(random.Next(0, 500)),
                        Status = statuses[random.Next(statuses.Length)]
                    });
                }
            }

            if (enrollmentsToAdd.Count > 0)
            {
                await context.Enrollments.AddRangeAsync(enrollmentsToAdd, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        logger.LogInformation("Database seeding completed successfully.");
    }
}
