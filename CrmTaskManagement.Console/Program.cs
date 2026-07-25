using CrmTaskManagement.Console.Demo;
using CrmTaskManagement.Data;
using CrmTaskManagement.Data.Repositories;
using CrmTaskManagement.Data.Seed;
using CrmTaskManagement.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

using var db = new AppDbContext(optionsBuilder.Options);

var canConnect = db.Database.CanConnect();
Console.WriteLine($"Can connect to database: {canConnect}");

var seeded = await DbInitializer.SeedAsync(db);
Console.WriteLine(seeded ? "Database seeded successfully." : "Database already seeded, skipped.");

var employeeCount = await db.Employees.CountAsync();
var workTaskCount = await db.WorkTasks.CountAsync();
Console.WriteLine($"Employees: {employeeCount}, WorkTasks: {workTaskCount}");

var workTaskService = new WorkTaskService(new WorkTaskRepository(db), new EmployeeRepository(db));
var demoRunner = new WorkTaskDemoRunner(workTaskService, new EmployeeRepository(db));
await demoRunner.RunAsync();
