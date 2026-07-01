using System.Text.Json;

namespace TheCharityBLL.Jobs.Context
{
    public class JobContext
    {
        public string JobId { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public int RetryCount { get; set; }
        public DateTime ScheduledAt { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
        public T? GetMetadata<T>(string key)
        {
            if (!Metadata.TryGetValue(key, out var value))
                return default;

            // Handle JsonElement (from JSON deserialization)
            if (value is JsonElement jsonElement)
            {
                return jsonElement.Deserialize<T>();
            }

            // Handle direct values
            if (value is T typedValue)
                return typedValue;

            // Try to convert
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }
        public void SetMetadata(string key, object value)
        {
            Metadata[key] = value;
        }
    }
}
