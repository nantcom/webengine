using NC.WebEngine.Core.Content;
using NC.WebEngine.Core.VueSync;

namespace NC.WebEngine.Core.Editor
{
    public class EditorVueSyncMixins : IVueSyncMixins
    {
        public string[] JsFiles => new[] { "/js/ncweb/editor.js" };

        public string CallMixins(ContentRenderModel renderModel, string vueSyncVariableName)
        {
            return $"window.nceditor?.editormixin?.({vueSyncVariableName}, {renderModel.ContentPage.Id});";
        }

        public bool WillInclude(ContentRenderModel renderModel)
        {
            //TODO: Check Login
            return true;
        }
    }
}
