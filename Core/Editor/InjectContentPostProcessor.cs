using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using NC.WebEngine.Core.Content;

namespace NC.WebEngine.Core.Editor
{
    /// <summary>
    /// This post processor set the content for element with ncweb-contentpart
    /// without ncweb-contentpage specified (which means it is the main content)
    /// </summary>
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
                var matchingElement = document.DocumentNode.QuerySelectorAll($"*[ncweb-contentpart='{group.Key}']")
                                        .Where( el => el.Attributes.Contains("ncweb-contentpageid") == false);

                foreach (var element in matchingElement)
                {
                    element.InnerHtml = newest.Content;
                }
            }
        }
    }
}
