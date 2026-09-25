using System.Text.Json.Serialization;

namespace TestJob
{
    public class HtmlAnalyzeData
    {
        [JsonPropertyName("selector")]
        public string? Selector { get; set; }
        [JsonPropertyName("attribute")]
        public string? Attribute { get; set; }
        [JsonPropertyName("url_b64")]
        public string? UrlBase64 { get; set; }
        [JsonPropertyName("encrypted_text_bytes_b64")]
        public string? EncryptedTextBase64 { get; set; }
        [JsonPropertyName("key_bytes_b64")]
        public string? KeyBase64 { get; set; }
        [JsonPropertyName("page_b64")]
        public string? PageBase64 { get; set; }
    }
}
