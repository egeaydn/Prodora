using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Prodora.WebUI.TagHelpers;

/// <summary>Replace MVC's empty inline-styled placeholder with native hidden markup under the site's CSP.</summary>
[HtmlTargetElement("div", Attributes = "asp-validation-summary")]
public sealed class CspValidationSummaryTagHelper : TagHelper
{
    public override int Order => 1000;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // The MVC helper has already encoded the real errors. Only its empty placeholder is changed.
        var content = output.PostContent.GetContent();
        output.PostContent.SetHtmlContent(content.Replace("<li style=\"display:none\"></li>", "<li hidden></li>", StringComparison.Ordinal));
    }
}
