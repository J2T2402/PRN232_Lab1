using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Implements;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Implements;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");

        services.AddDbContext<LmsDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IDatabaseBootstrapService, DatabaseBootstrapService>();
        services.AddScoped<ISemesterService, SemesterService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();

        return services;
    }
}
