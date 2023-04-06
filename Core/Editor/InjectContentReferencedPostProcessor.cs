using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using NC.WebEngine.Core.Content;

namespace NC.WebEngine.Core.Editor
{
    /// <summary>
    /// This post processor set the content for element with ncweb-contentpart
    /// WITH ncweb-contentpage specified (which means it is the content part of other pages that was reference in current page)
    /// </summary>
    public class InjectContentReferencedPostProcessor : IPostProcessor
    {
        public void Process(ContentRenderModel renderModel, HtmlDocument document)
        {
            var pageIds = document.DocumentNode.QuerySelectorAll($"*[ncweb-contentpageid]")
                                    .Select( el => el.GetAttributeValue("ncweb-contentpageid", 0))
                                    .Distinct();

            foreach ( var id in pageIds )
            {
                var partGroup = from part in renderModel.ContentService.GetContentParts( id )
                                   orderby part.Created
                                   group part by part.Name into g
                                   select g;

                foreach (var group in partGroup)
                {
                    var newest = group.Last();
                    var matchingElement = document.DocumentNode.QuerySelectorAll($"*[ncweb-contentpart='{group.Key}'][ncweb-contentpageid='{id}']");

                    foreach (var element in matchingElement)
                    {
                        element.InnerHtml = newest.Content;
                    }
                }
            }

        }
    }
}
