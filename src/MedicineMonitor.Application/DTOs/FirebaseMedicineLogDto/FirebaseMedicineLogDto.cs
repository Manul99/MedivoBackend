using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MedicineMonitor.Application.DTOs.FirebaseMedicineLogDto
{
    public sealed class FirebaseMedicineLogDto
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("mac_address")]
        public string MacAddress { get; set; } = string.Empty;

        [JsonPropertyName("slot_number")]
        public int SlotNumber { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;
    }
}
