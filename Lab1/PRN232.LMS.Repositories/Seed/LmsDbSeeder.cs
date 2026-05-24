using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Seed;

public static class LmsDbSeeder
{
    public static async Task SeedAsync(LmsDbContext db)
    {
        // Seed once only.
        if (await db.Semesters.AnyAsync() || await db.Students.AnyAsync())
        {
            return;
        }

        var rng = new Random(123);

        // Semesters (5)
        var semesters = new List<Semester>();
        var startYear = DateTime.UtcNow.Year - 1;
        for (var i = 1; i <= 5; i++)
        {
            var start = new DateTime(startYear, 1, 1).AddMonths((i - 1) * 4);
            var end = start.AddMonths(4).AddDays(-1);
            semesters.Add(new Semester
            {
                SemesterName = $"Semester {i} ({start:yyyy-MM})",
                StartDate = start,
                EndDate = end,
            });
        }

        // Subjects (10)
        var subjects = Enumerable.Range(1, 10)
            .Select(i => new Subject
            {
                SubjectCode = $"SUBJ{i:00}",
                SubjectName = $"Subject {i}",
                Credit = 1 + (i % 4),
            })
            .ToList();

        db.Semesters.AddRange(semesters);
        db.Subjects.AddRange(subjects);
        await db.SaveChangesAsync();

        // Courses (20)
        var courses = Enumerable.Range(1, 20)
            .Select(i => new Course
            {
                CourseName = $"Course {i}",
                SemesterId = semesters[rng.Next(semesters.Count)].SemesterId,
            })
            .ToList();

        db.Courses.AddRange(courses);
        await db.SaveChangesAsync();

        // Students (50)
        var firstNames = new[] { "Anh", "Binh", "Chi", "Dung", "Hanh", "Khanh", "Linh", "Minh", "Nam", "Phuong", "Quang", "Trang" };
        var lastNames = new[] { "Nguyen", "Tran", "Le", "Pham", "Hoang", "Huynh", "Phan", "Vu", "Vo", "Dang" };

        var students = new List<Student>();
        for (var i = 1; i <= 50; i++)
        {
            var fullName = $"{lastNames[rng.Next(lastNames.Length)]} {firstNames[rng.Next(firstNames.Length)]} {i}";
            students.Add(new Student
            {
                FullName = fullName,
                Email = $"student{i:000}@lms.local",
                DateOfBirth = new DateTime(1998, 1, 1).AddDays(rng.Next(0, 3650)),
            });
        }

        db.Students.AddRange(students);
        await db.SaveChangesAsync();

        // Enrollments (500)
        var statuses = new[] { "active", "completed", "dropped" };
        var courseIds = await db.Courses.Select(c => c.CourseId).ToListAsync();
        var studentIds = await db.Students.Select(s => s.StudentId).ToListAsync();

        var enrollments = new List<Enrollment>(capacity: 500);
        for (var i = 0; i < 500; i++)
        {
            var studentId = studentIds[rng.Next(studentIds.Count)];
            var courseId = courseIds[rng.Next(courseIds.Count)];

            enrollments.Add(new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollDate = DateTime.UtcNow.AddDays(-rng.Next(0, 365)),
                Status = statuses[rng.Next(statuses.Length)],
            });
        }

        db.Enrollments.AddRange(enrollments);
        await db.SaveChangesAsync();
    }
}
