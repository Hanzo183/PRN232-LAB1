using Microsoft.Extensions.DependencyInjection;
using PRN232.LMS.Repositories.Repositories.Implementations;
using PRN232.LMS.Repositories.Repositories.Interfaces;
using PRN232.LMS.Services.Implementations;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLmsServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ISemesterRepository, SemesterRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Services
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ISemesterService, SemesterService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStudentLookupClient, RepositoryStudentLookupClient>();

        return services;
    }
}
