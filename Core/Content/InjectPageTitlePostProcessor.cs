using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace NC.WebEngine.Core.Content
{
    public class InjectPageTitlePostProcessor : IPostProcessor
    {
        public void Process(ContentRenderModel renderModel, HtmlDocument document)
        {
            var title = document.DocumentNode.QuerySelector("head > title");
            if (title != null)
            {
                title.InnerHtml = $"{renderModel.SiteTitle} - {renderModel.ContentPage.Title}";
            }
        }
    }
}
