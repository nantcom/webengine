using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using NC.WebEngine.Core.Content;

namespace NC.WebEngine.Core.Favicon
{
    public class InjectFaviconPostProcessor : IPostProcessor
    {
        public void Process(ContentRenderModel renderModel, HtmlDocument document)
        {
            if (renderModel.HttpContext.RequestServices.GetService<FaviconModule>()?.IsFaviconAvailable == false)
            {
                return;
            }

            var head = document.DocumentNode.QuerySelector("head");

            head.AppendChild(HtmlNode.CreateNode("<link rel=\"shortcut icon\" href=\"/favicon.ico\" type=\"image/x-icon\">"));
            head.AppendChild(HtmlNode.CreateNode("<link rel=\"apple-touch-icon\" href=\"/favicon-180x180.png\">"));
            head.AppendChild(HtmlNode.CreateNode("<link rel=\"apple-touch-icon\" sizes=\"180x180\" href=\"/favicon-180x180.png\">"));
            head.AppendChild(HtmlNode.CreateNode("<link rel=\"icon\" type=\"image/png\" sizes=\"192x192\" href=\"/favicon-192x192.png\">"));
            head.AppendChild(HtmlNode.CreateNode("<link rel=\"icon\" type=\"image/png\" sizes=\"96x96\" href=\"/favicon-96x96.png\">"));
            head.AppendChild(HtmlNode.CreateNode("<link rel=\"icon\" type=\"image/png\" sizes=\"32x32\" href=\"/favicon-32x32.png\">"));
            head.AppendChild(HtmlNode.CreateNode("<link rel=\"icon\" type=\"image/png\" sizes=\"16x16\" href=\"/favicon-16x16.png\">"));
            head.AppendChild(HtmlNode.CreateNode("<meta name=\"msapplication-TileImage\" content=\"/favicon-144x144.png\">"));


        }
    }
}
