using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.StudentService.Infrastructure;

public static class StudentDbSeeder
{
    public static async Task SeedAsync(LmsDbContext db)
    {
        if (await db.Students.AnyAsync())
        {
            return;
        }

        var rng = new Random(123);
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
    }
}
