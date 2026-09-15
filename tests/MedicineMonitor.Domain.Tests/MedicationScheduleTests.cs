using MedicineMonitor.Domain.Entities;
using MedicineMonitor.Domain.Enums;
using Xunit;

namespace MedicineMonitor.Domain.Tests;

public sealed class MedicationScheduleTests
{
    [Fact]
    public void Schedule_rejects_invalid_hour()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MedicationSchedule(MedicationDay.Monday, 24, 0));
    }

    [Fact]
    public void Schedule_accepts_valid_time()
    {
        var schedule = new MedicationSchedule(MedicationDay.Monday, 8, 30);
        Assert.Equal(8, schedule.Hour);
        Assert.Equal(30, schedule.Minute);
    }
}
