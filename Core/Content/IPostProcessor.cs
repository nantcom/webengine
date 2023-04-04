using HtmlAgilityPack;

namespace NC.WebEngine.Core.Content
{
    internal interface IPostProcessor
    {
        void Process(ContentRenderModel renderModel, HtmlDocument document);
    }
}
