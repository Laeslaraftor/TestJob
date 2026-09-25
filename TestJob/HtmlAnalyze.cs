using System.Text.Json.Serialization;

namespace TestJob
{
    public class HtmlAnalyze()
    {
        public HtmlAnalyze(string errorCode, string errorMessage) : this()
        {
            IsError = true;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        [JsonPropertyName("is_error")]
        public bool IsError { get; set; }
        [JsonPropertyName("error_code")]
        public string? ErrorCode { get; set; }
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }
        [JsonPropertyName("elements_count")]
        public int ElementsCount => ElementsAttribute?.Count ?? 0;
        [JsonPropertyName("emails_count")]
        public int EmailsCount => Emails?.Count ?? 0;
        [JsonPropertyName("url")]
        public string? Url { get; set; }
        [JsonPropertyName("decrypted_plain_text")]
        public string? DecryptedPlainText { get; set; }
        [JsonPropertyName("elements_attr_list")]
        public List<string?>? ElementsAttribute { get; set; }
        [JsonPropertyName("emails_list")]
        public List<string>? Emails { get; set; }
    }
}
