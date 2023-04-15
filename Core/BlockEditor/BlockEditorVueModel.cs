using NC.WebEngine.Core.BlockEditor;
using NC.WebEngine.Core.Content;
using NC.WebEngine.Core.VueSync;
using System.Text.Json.Serialization;

namespace NC.WebEngine.Core.BlockEditor
{
    public class BlockEditorVueModel : IVueModel
    {
        private BlockContentRenderService _renderService;

        [VueCallableMethod]
        public string Render(ContentPart p)
        {
            return string.Join("", _renderService.RenderContent(p));
        }

        public void OnCreated(HttpContext ctx)
        {
        }

        public void OnPostback(HttpContext ctx)
        {
            _renderService = ctx.RequestServices.GetRequiredService<BlockContentRenderService>();
        }
    }
}
