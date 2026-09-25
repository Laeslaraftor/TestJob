using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace TestJob
{
    public class HtmlPageParser(IDocument document)
    {
        private readonly static HtmlParser _parser = new();

        private readonly IDocument _document = document;

        public IHtmlCollection<IElement> SelectElements(string selector) => _document.QuerySelectorAll(selector);

        public static async Task<HtmlPageParser> Create(string html)
        {
            var document = await _parser.ParseDocumentAsync(html);
            return new(document);
        }
    }
}
