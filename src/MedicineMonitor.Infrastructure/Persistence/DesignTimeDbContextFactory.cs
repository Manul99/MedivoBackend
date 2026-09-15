using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MedicineMonitor.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MedicineMonitorDbContext>
{
    public MedicineMonitorDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("MEDICINE_MONITOR_POSTGRES")
            ?? "Host=localhost;Port=5432;Database=medicinemonitor;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<MedicineMonitorDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new MedicineMonitorDbContext(options);
    }
}
