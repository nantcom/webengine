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
            var matchingElement = document.DocumentNode.QuerySelectorAll($"[ncweb-editableimage]");

            foreach ( var element in matchingElement )
            {
                var file = element.GetAttributeValue("ncweb-editableimage", "should_not_exists.xxx");
                if (file.StartsWith("/"))
                {
                    file = file.Substring(1);
                }

                var targetFile = Path.Combine(Directory.GetCurrentDirectory(),
                                    "wwwroot",
                                    file);

                if ( File.Exists(targetFile) == false)
                {
                    continue;
                }

                if (element.Name == "img")
                {
                    element.SetAttributeValue("src", "/" + file);
                }
                else
                {
                    var img = element.QuerySelector("img[ncweb-editableimage-placeholder]");
                    img?.SetAttributeValue("src", "/" + file);
                }
            }

        }
    }
}
