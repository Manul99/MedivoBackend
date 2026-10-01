using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicineMonitor.Application.DTOs.Auth
{
    public sealed record FirebaseProfile(
    string UserId,
    string Name,
    int Age,
    string Blood,
    string Phone);
}
