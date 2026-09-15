using MedicineMonitor.Domain.Enums;

namespace MedicineMonitor.Domain.Entities;

public sealed class MedicationSchedule
{
    private MedicationSchedule() { }

    public MedicationSchedule(MedicationDay day, int hour, int minute)
    {
        if (hour is < 0 or > 23) throw new ArgumentOutOfRangeException(nameof(hour));
        if (minute is < 0 or > 59) throw new ArgumentOutOfRangeException(nameof(minute));

        Day = day;
        Hour = hour;
        Minute = minute;
    }

    public MedicationDay Day { get; private set; }
    public int Hour { get; private set; }
    public int Minute { get; private set; }
}
