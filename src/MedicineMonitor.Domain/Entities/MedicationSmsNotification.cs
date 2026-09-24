using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicineMonitor.Domain.Entities;

public sealed class MedicationSmsNotification
{
    private MedicationSmsNotification()
    {
    }

    public MedicationSmsNotification(
        string medicationId,
        Guid userId,
        DateOnly scheduledDate,
        TimeOnly scheduledTime,
        string phoneNumber)
    {
        Id = Guid.NewGuid();

        MedicationId = medicationId;

        UserId = userId;

        ScheduledDate = scheduledDate;

        ScheduledTime = scheduledTime;

        PhoneNumber = phoneNumber;

        Status = "Pending";

        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string MedicationId { get; private set; } =
        string.Empty;

    public Guid UserId { get; private set; }

    public DateOnly ScheduledDate { get; private set; }

    public TimeOnly ScheduledTime { get; private set; }

    public string PhoneNumber { get; private set; } =
        string.Empty;

    public string Status { get; private set; } =
        string.Empty;

    public string? TextLkMessageId { get; private set; }

    public DateTime? SentAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public void MarkSent(string? messageId)
    {
        Status = "Sent";

        TextLkMessageId = messageId;

        SentAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = "Failed";
    }
}
