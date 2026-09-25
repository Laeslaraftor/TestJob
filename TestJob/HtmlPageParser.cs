using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Text.RegularExpressions;

namespace TestJob
{
    public partial class HtmlPageParser(IDocument document, string html)
    {
        [GeneratedRegex(@"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}")]
        private static partial Regex MatchEmailsRegex { get; }
        private readonly static HtmlParser _parser = new();

        private readonly IDocument _document = document;
        private readonly string _html = html;

        public IEnumerable<SelectedElement> SelectElements(string? selector, string? attributeName)
        {
            if (string.IsNullOrEmpty(selector))
            {
                return [];
            }
            if (string.IsNullOrEmpty(attributeName))
            {
                return _document.QuerySelectorAll(selector).Select(e => new SelectedElement(e, null));
            }

            return _document.QuerySelectorAll(selector)
                            .Select(e => new SelectedElement(e, string.IsNullOrEmpty(attributeName) ? 
                                                                null : 
                                                                e.GetAttribute(attributeName)));
        }
        public List<string> FindEmails()
        {
            var matches = MatchEmailsRegex.Matches(_html);
            return [.. matches.Select(m => m.Value)];
        }

        public static async Task<HtmlPageParser> Create(string html)
        {
            var document = await _parser.ParseDocumentAsync(html);
            return new(document, html);
        }

        public record struct SelectedElement(IElement Element, string? AttributeValue);
    }
}
