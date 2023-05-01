using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace NC.WebEngine.Core.Content
{
    public class InjectKeywordsPostProcessor : IPostProcessor
    {
        public void Process(ContentRenderModel renderModel, HtmlDocument document)
        {
            if (renderModel.ContentPage.Keywords.Count == 0)
            {
                return;
            }

            var metaKeyword = document.DocumentNode.QuerySelector("head > meta[name='keywords']");
            if (metaKeyword == null)
            {
                metaKeyword = document.CreateElement("<meta name=\"keywords\" content=\"\">");
                document.DocumentNode.QuerySelector("head").AppendChild(metaKeyword);
            }

            metaKeyword.SetAttributeValue("content", string.Join(',', renderModel.ContentPage.Keywords));
        }
    }
}
