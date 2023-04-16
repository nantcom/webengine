using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using NC.WebEngine.Core.Content;

namespace NC.WebEngine.Core.EditableBackground
{
    /// <summary>
    /// This post processor set the content for element with ncweb-contentpart
    /// without ncweb-contentpage specified (which means it is the main content)
    /// </summary>
    public class InjectBackgroundImagePostProcessor : IPostProcessor
    {
        public void Process(ContentRenderModel renderModel, HtmlDocument document)
        {
            var matchingElement = document.DocumentNode.QuerySelectorAll($"*[ncweb-editablebackground]");
            foreach ( var element in matchingElement )
            {
                var file = element.GetAttributeValue("ncweb-editablebackground", "should_not_exists.xxx");
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

                string oldStyle = element.GetAttributeValue("style", "");
                                
                var styleDict = oldStyle.Split(";").Select( item =>
                {
                    var parts = item.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    return new
                    {
                        Key = parts[0].Trim(),
                        Value = parts[1].Trim()
                    };
                }).ToDictionary( item => item.Key, item => item.Value);

                styleDict["background-image"] = $"url('{file}')";

                element.SetAttributeValue("style", string.Join(';', styleDict.Select( pair => $"{pair.Key}:{pair.Value}")));
            }

        }
    }
}
