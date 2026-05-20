using Microsoft.OpenApi.Models;
using PRN232.LMS.Services;
using PRN232.LMS.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PRN232 LMS API",
        Version = "v1",
        Description = "Learning Management System RESTful API following 3-layer architecture."
    });
});
builder.Services.AddBusinessServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var bootstrapService = scope.ServiceProvider.GetRequiredService<IDatabaseBootstrapService>();
    await bootstrapService.InitializeAsync();
}

app.Run();
