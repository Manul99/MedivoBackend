using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicineMonitor.Infrastructure.Storage;

public sealed class SupabaseStorageOptions
{
    public string Url { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string BucketName { get; set; } =
        "medical-documents";
}
