using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace NC.WebEngine.Core.Content
{
    public class InjectContentPostProcessor : IPostProcessor
    {
        public void Process(ContentRenderModel renderModel, HtmlDocument document)
        {
            var partGroup = from part in renderModel.ContentParts
                            orderby part.Created
                            group part by part.Name into g
                            select g;

            foreach (var group in partGroup)
            {
                var newest = group.Last();
                var matchingElement = document.DocumentNode.QuerySelectorAll($"*[ncweb-contentpart='{group.Key}']");

                foreach (var element in matchingElement)
                {
                    element.InnerHtml = newest.Content;
                }
            }
        }
    }
}
