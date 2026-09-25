using FluentValidation;

namespace TestJob
{
    public class HtmlAnalyzeDataValidator : AbstractValidator<HtmlAnalyzeData>
    {
        public HtmlAnalyzeDataValidator()
        {
            RuleFor(x => x.Selector).NotEmpty();
            RuleFor(x => x.Attribute).NotEmpty();
            RuleFor(x => x.UrlBase64).NotEmpty();
            RuleFor(x => x.EncryptedTextBase64).NotEmpty();
            RuleFor(x => x.KeyBase64).NotEmpty();
            RuleFor(x => x.PageBase64).NotEmpty();
            RuleFor(x => x.PageBase64).NotEmpty();
        }
    }
}
