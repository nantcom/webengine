using NC.WebEngine.Core.Content;
using NC.WebEngine.Core.Membership;
using NC.WebEngine.Core.VueSync;
using System.Text;

namespace NC.WebEngine.Core.Editor
{
    public class BlockEditorVueSyncMixins : IVueSyncMixins
    {
        public string[] JsFiles => this.EditorJsScripts().ToArray();

        public BlockEditorVueSyncMixins() {
        }

        public IEnumerable<string> EditorJsScripts()
        {
            yield return "/js/jquery.debounce.js";
            yield return "/js/ncweb/blockeditor.js";
            yield return "/js/editorjs/editor.js";

            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "js", "editorjs");
            var files = Directory.GetFiles(path, "*bundle.js", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                yield return file
                    .Replace(webRoot, "")
                    .Replace("\\", "/");
            }
        }

        public string CallMixins(ContentRenderModel renderModel, string vueSyncVariableName)
        {
            var savedData = renderModel.ContentParts.Values.FirstOrDefault(cp => cp.IsBlockContent);
            if (savedData == null)
            {
                return @$"
                window.ncblockeditor.data = {{}};
                window.ncblockeditor?.mixin?.({vueSyncVariableName}, {renderModel.ContentPage.Id});";

            }

            var escaped = savedData.Content
                            .Replace("<script", "&#60;script")
                            .Replace("script>", "script&gt;"); ;

            return @$"
                window.ncblockeditor.data = {escaped};
                window.ncblockeditor?.mixin?.({vueSyncVariableName}, {renderModel.ContentPage.Id});";

        }

        public bool WillInclude(ContentRenderModel renderModel)
        {
            return renderModel.HttpContext.RequestServices.GetRequiredService<MembershipService>().IsEditor;
        }
    }
}
