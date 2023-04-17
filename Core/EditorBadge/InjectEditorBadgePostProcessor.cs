using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using NC.WebEngine.Core.Content;
using NC.WebEngine.Core.Membership;
using Razor.Templating.Core;

namespace NC.WebEngine.Core.Editor
{
    /// <summary>
    /// This injector add "Admin" mode badge to the page
    /// </summary>
    public class InjectEditorBadgePostProcessor : IPostProcessor
    {
        public void Process(ContentRenderModel renderModel, HtmlDocument document)
        {
            if (renderModel.HttpContext.RequestServices.GetRequiredService<MembershipService>().IsEditor)
            {
                var html = RazorTemplateEngine.RenderAsync("SystemViews/editorribbon", renderModel).Result;
                document.DocumentNode.InnerHtml += html;
            }
        }
    }
}
