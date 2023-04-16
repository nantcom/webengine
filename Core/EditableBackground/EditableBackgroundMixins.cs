using NC.WebEngine.Core.Content;
using NC.WebEngine.Core.VueSync;

namespace NC.WebEngine.Core.EditableBackground
{
    public class EditableBackgroundMixins : IVueSyncMixins
    {
        public string[] JsFiles => new[] { "/js/ncweb/editablebackground.js" };

        public string CallMixins(ContentRenderModel renderModel, string vueSyncVariableName)
        {
            return $"window.nceditablebackground?.mixin?.({vueSyncVariableName}, {renderModel.ContentPage.Id});";
        }

        public bool WillInclude(ContentRenderModel renderModel)
        {
            return true;
        }
    }
}
