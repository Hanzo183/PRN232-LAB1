using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.CourseService.Infrastructure;

public static class CourseDbSeeder
{
    public static async Task SeedAsync(LmsDbContext db)
    {
        if (await db.Semesters.AnyAsync() || await db.Courses.AnyAsync())
        {
            return;
        }

        var rng = new Random(123);

        var semesters = new List<Semester>();
        var startYear = DateTime.UtcNow.Year - 1;
        for (var i = 1; i <= 5; i++)
        {
            var start = new DateTime(startYear, 1, 1).AddMonths((i - 1) * 4);
            semesters.Add(new Semester
            {
                SemesterName = $"Semester {i} ({start:yyyy-MM})",
                StartDate = start,
                EndDate = start.AddMonths(4).AddDays(-1),
            });
        }

        db.Semesters.AddRange(semesters);
        await db.SaveChangesAsync();

        var courses = Enumerable.Range(1, 20)
            .Select(i => new Course
            {
                CourseName = $"Course {i}",
                SemesterId = semesters[rng.Next(semesters.Count)].SemesterId,
            })
            .ToList();

        db.Courses.AddRange(courses);
        await db.SaveChangesAsync();

        var courseIds = await db.Courses.Select(c => c.CourseId).ToListAsync();
        var statuses = new[] { "active", "completed", "dropped" };
        var enrollments = new List<Enrollment>();

        for (var i = 0; i < 50; i++)
        {
            enrollments.Add(new Enrollment
            {
                StudentId = rng.Next(1, 51),
                CourseId = courseIds[rng.Next(courseIds.Count)],
                EnrollDate = DateTime.UtcNow.AddDays(-rng.Next(0, 365)),
                Status = statuses[rng.Next(statuses.Length)],
            });
        }

        db.Enrollments.AddRange(enrollments);
        await db.SaveChangesAsync();
    }
}
