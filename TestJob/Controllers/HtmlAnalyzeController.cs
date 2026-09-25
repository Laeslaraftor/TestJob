using AngleSharp.Dom;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TestJob.Db;

namespace TestJob.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public partial class HtmlAnalyzeController(DbService dbService, IValidator<HtmlAnalyzeData> htmlAnalyzeDataValidator) : ControllerBase
    {
        private const string EmailPattern = @"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}";
        [GeneratedRegex(EmailPattern)]
        private static partial Regex MatchEmailsRegex { get; }

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
            if (!TryDecodeBytes(data.UrlBase64, out var urlBytes))
            {
                return new("InvalidUrlValue", "Unable to decode url from base64 string");
            }
            if (!TryDecodeBytes(data.EncryptedTextBase64, out var encryptedTextBytes))
            {
                return new("InvalidEncryptedTextValue", "Unable to decode encrypted text from base64 string");
            }
            if (!TryDecodeBytes(data.KeyBase64, out var encryptionKey))
            {
                return new("InvalidUrlValue", "Unable to decode encryption key from base64 string");
            }
            if (!TryDecodeBytes(data.PageBase64, out var pageBytes))
            {
                return new("InvalidUrlValue", "Unable to decode page from base64 string");
            }
            if (!TryDecodeText(encryptedTextBytes, encryptionKey, out var decryptedText))
            {
                return new("EncryptionError", "Failed to decrypt text");
            }

            IEnumerable<SelectedElement> elements;
            List<string> emails;

            try
            {
                string page = Encoding.UTF8.GetString(pageBytes);
                HtmlPageParser document = await HtmlPageParser.Create(page);
                elements = document.SelectElements(data.Selector!).Select(e => new SelectedElement(e, e.GetAttribute(data.Attribute!)));
                emails = FindEmails(page);

                await _dbService.CreateElements(elements);
            }
            catch (Exception exception)
            {
                return new(exception.GetType().Name, exception.Message + Environment.NewLine + exception.StackTrace);
            }

            HtmlAnalyze analyzeResult = new()
            {
                Url = Encoding.UTF8.GetString(urlBytes),
                DecryptedPlainText = decryptedText,
                ElementsAttribute = [.. elements.Select(e => e.AttributeValue)],
                Emails = emails
            };

            return analyzeResult;
        }

        private static bool TryDecodeBytes(string? base64, [NotNullWhen(true)] out byte[]? result)
        {
            result = null;

            if (string.IsNullOrEmpty(base64))
            {
                return false;
            }

            byte[]? byteArrayBuffer = null;
            Span<byte> buffer = 1024 >= base64.Length ? stackalloc byte[base64.Length] : default;

            if (base64.Length > 1024)
            {
                byteArrayBuffer = ArrayPool<byte>.Shared.Rent(base64.Length);
                buffer = byteArrayBuffer;
            }

            if (Convert.TryFromBase64String(base64, buffer, out int bytesWritten))
            {
                result = buffer[..bytesWritten].ToArray();
            }
            if (byteArrayBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(byteArrayBuffer);
            }

            return result != null;
        }
        private static bool TryDecodeText(byte[] encodedBytes, byte[] key, [NotNullWhen(true)] out string? result)
        {
            result = null;
            using Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = key;
            aes.Mode = CipherMode.ECB;

            byte[]? byteArrayBuffer = null;
            Span<byte> buffer = 1024 >= encodedBytes.Length ? stackalloc byte[encodedBytes.Length] : default;

            if (encodedBytes.Length > 1024)
            {
                byteArrayBuffer = ArrayPool<byte>.Shared.Rent(encodedBytes.Length);
                buffer = byteArrayBuffer;
            }

            if (aes.TryDecryptEcb(encodedBytes, buffer, PaddingMode.None, out int bytesWritten))
            {
                result = Encoding.UTF8.GetString(buffer[0..bytesWritten]);
            }
            if (byteArrayBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(byteArrayBuffer);
            }

            return result != null;
        }
        private static List<string> FindEmails(string html)
        {
            var matches = MatchEmailsRegex.Matches(html);
            return [.. matches.Select(m => m.Value)];
        }

        public record struct SelectedElement(IElement Element, string? AttributeValue);
    }
}
