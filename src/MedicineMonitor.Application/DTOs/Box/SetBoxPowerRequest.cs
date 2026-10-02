using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicineMonitor.Application.DTOs.Box;

public sealed class SetBoxPowerRequest
{
    public bool IsOn { get; init; }
}