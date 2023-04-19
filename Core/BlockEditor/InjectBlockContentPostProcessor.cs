using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using NC.WebEngine.Core.BlockEditor;
using NC.WebEngine.Core.Content;

namespace NC.WebEngine.Core.Editor
{
    /// <summary>
    /// This post processor set the content for element with ncweb-contentpart
    /// without ncweb-contentpage specified (which means it is the main content)
    /// </summary>
    public class InjectBlockContentPostProcessor : IPostProcessor
    {
        public void Process(ContentRenderModel renderModel, HtmlDocument document)
        {
            if (renderModel.MembershipService.IsEditor)
            {
                return;
            }

            var blockContentPart = renderModel.ContentParts.Values.FirstOrDefault(cp => cp.IsBlockContent);
            var blockContent = document.DocumentNode.QuerySelectorAll("[ncweb-blockcontent]").FirstOrDefault();

            if (blockContentPart != null && blockContent != null)
            {
                var renderer = renderModel.HttpContext.RequestServices.GetRequiredService<BlockContentRenderService>();
                var html = renderer.RenderContent(blockContentPart);

                blockContent.InnerHtml = string.Join("", html);
                blockContent.SetAttributeValue("v-pre", "");
            }
        }
    }
}
