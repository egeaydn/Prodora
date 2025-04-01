using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Prodora.WebUI.Models;

namespace Prodora.WebUI.TagHelpers
{
	[HtmlTargetElement("div", Attributes = "page-model")]
	public class PageLinkTagHelpers : TagHelper
	{
		public PageInfo PageModel { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "div";

			StringBuilder stringBuilder = new StringBuilder();

			stringBuilder.Append("<ul class='pagination'>");

			for (int i = 1; i <= PageModel.TotalPages(); i++)
			{
				stringBuilder.AppendFormat("<li class='page-item {0}'>", i == PageModel.CurrentPage ? "active" : "");

				if (string.IsNullOrEmpty(PageModel.CurrentCategory))
				{
					stringBuilder.AppendFormat("<a class='page-link' href='/products?page={0}'>{0}</a>", i);
				}
				else
				{
					stringBuilder.AppendFormat("<a class='page-link' href='/products/{0}?page={1}'>{1}</a>", PageModel.CurrentCategory, i);
				}

				stringBuilder.AppendFormat("</li>");
			}
			stringBuilder.AppendFormat("</ul>");

			output.Content.SetHtmlContent(stringBuilder.ToString());

			base.Process(context, output);

		}
	}
}
