using CrmTaskManagement.Data;
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
