using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using NC.WebEngine.Core.Content;

namespace NC.WebEngine.Core.Editor
{
    /// <summary>
    /// This post processor set the content for element with ncweb-contentpart
    /// without ncweb-contentpage specified (which means it is the main content)
    /// </summary>
    public class InjectImagePostProcessor : IPostProcessor
    {
        public void Process(ContentRenderModel renderModel, HtmlDocument document)
        {
            var matchingElement = document.DocumentNode.QuerySelectorAll($"img[ncweb-editableimage]");

            foreach ( var element in matchingElement )
            {
                var file = element.GetAttributeValue("ncweb-editableimage", "should_not_exists.xxx");
                var targetFile = Path.Combine(Directory.GetCurrentDirectory(),
                                    "wwwroot",
                                    file);

                if ( File.Exists(targetFile) == false)
                {
                    continue;
                }

                element.SetAttributeValue("src", file);
            }

        }
    }
}
