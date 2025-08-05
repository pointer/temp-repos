using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CustomPrinter.NativeMessaging
{
    /// <summary>
    /// Represents a message from the browser extension
    /// </summary>
    public class BrowserMessage
    {
        /// <summary>
        /// The action to perform (e.g., "showPrintDialog")
        /// </summary>
        [JsonPropertyName("action")]
        public string Action { get; set; }

        /// <summary>
        /// The content to be printed
        /// </summary>
        [JsonPropertyName("content")]
        public PrintContent Content { get; set; }

        /// <summary>
        /// Optional: Timestamp when the message was sent
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Creates a JSON string from this object
        /// </summary>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Parses a JSON string into a BrowserMessage
        /// </summary>
        public static BrowserMessage FromJson(string json)
        {
            return JsonSerializer.Deserialize<BrowserMessage>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }

    /// <summary>
    /// Contains the actual print content and metadata
    /// </summary>
    public class PrintContent
    {
        /// <summary>
        /// URL of the page being printed
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// Title of the page/document
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// HTML content to be printed
        /// </summary>
        [JsonPropertyName("html")]
        public string Html { get; set; }

        /// <summary>
        /// Optional: CSS styles to apply
        /// </summary>
        [JsonPropertyName("styles")]
        public string Styles { get; set; }

        /// <summary>
        /// Optional: Print job identifier
        /// </summary>
        [JsonPropertyName("jobId")]
        public string JobId { get; set; }

        /// <summary>
        /// Optional: Print settings (margins, orientation, etc.)
        /// </summary>
        [JsonPropertyName("settings")]
        public PrintSettings Settings { get; set; }
    }

    /// <summary>
    /// Optional print settings
    /// </summary>
    public class PrintSettings
    {
        [JsonPropertyName("margins")]
        public Margins Margins { get; set; } = new Margins();

        [JsonPropertyName("orientation")]
        public string Orientation { get; set; } = "portrait";

        [JsonPropertyName("pageSize")]
        public string PageSize { get; set; } = "A4";
    }

    public class Margins
    {
        [JsonPropertyName("top")]
        public int Top { get; set; } = 20;

        [JsonPropertyName("right")]
        public int Right { get; set; } = 20;

        [JsonPropertyName("bottom")]
        public int Bottom { get; set; } = 20;

        [JsonPropertyName("left")]
        public int Left { get; set; } = 20;
    }
}
