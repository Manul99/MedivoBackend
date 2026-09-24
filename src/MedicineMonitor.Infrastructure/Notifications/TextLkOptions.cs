using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicineMonitor.Infrastructure.Notifications
{
    public sealed class TextLkOptions
    {
        public string ApiToken { get; set; } = string.Empty;

        public string SenderId { get; set; } = "TextLKDemo";

        public string BaseUrl { get; set; } =
            "https://app.text.lk/api/v3";
    }
}
