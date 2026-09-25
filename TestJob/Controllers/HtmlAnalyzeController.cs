using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace TestJob.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public partial class HtmlAnalyzeController(DbService dbService, IValidator<HtmlAnalyzeData> htmlAnalyzeDataValidator) : ControllerBase
    {
        private readonly DbService _dbService = dbService;
        private readonly IValidator<HtmlAnalyzeData> _htmlAnalyzeDataValidator = htmlAnalyzeDataValidator;

        [HttpPost(Name = "AnalyzeHtml")]
        public async Task<HtmlAnalyze> Analyze(HtmlAnalyzeData data)
        {
            var validationResult = await _htmlAnalyzeDataValidator.ValidateAsync(data);

            if (!validationResult.IsValid)
            {
                return new("InputDataValidationError", validationResult.Errors.FirstOrDefault()?.ErrorMessage ?? "Unknown validation error");
            }
            if (!Decoder.TryDecodeBytes(data.UrlBase64, out var urlBytes))
            {
                return new("InvalidUrlValue", "Unable to decode url from base64 string");
            }
            if (!Decoder.TryDecodeBytes(data.EncryptedTextBase64, out var encryptedTextBytes))
            {
                return new("InvalidEncryptedTextValue", "Unable to decode encrypted text from base64 string");
            }
            if (!Decoder.TryDecodeBytes(data.KeyBase64, out var encryptionKey))
            {
                return new("InvalidUrlValue", "Unable to decode encryption key from base64 string");
            }
            if (!Decoder.TryDecodeBytes(data.PageBase64, out var pageBytes))
            {
                return new("InvalidUrlValue", "Unable to decode page from base64 string");
            }
            if (!Decoder.TryDecodeText(encryptedTextBytes, encryptionKey, out var decryptedText))
            {
                return new("EncryptionError", "Failed to decrypt text");
            }

            IEnumerable<HtmlPageParser.SelectedElement> elements;
            List<string> emails;

            try
            {
                string page = Encoding.UTF8.GetString(pageBytes);
                HtmlPageParser document = await HtmlPageParser.Create(page);
                elements = document.SelectElements(data.Selector, data.Attribute);
                emails = document.FindEmails();

                await _dbService.CreateElements(elements);
            }
            catch (Exception exception)
            {
                return new(exception.GetType().Name, exception.Message);
            }

            return new()
            {
                Url = Encoding.UTF8.GetString(urlBytes),
                DecryptedPlainText = decryptedText,
                ElementsAttribute = [.. elements.Select(e => e.AttributeValue)],
                Emails = emails
            };
        }
    }
}
