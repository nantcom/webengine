using NC.WebEngine.Core.Content;
using NC.WebEngine.Core.VueSync;

namespace NC.WebEngine.Core.EditableImage
{
    public class EditableCollectionMixins : IVueSyncMixins
    {
        public string[] JsFiles => new[] { "/js/ncweb/editablecollection.js" };

        public string CallMixins(ContentRenderModel renderModel, string vueSyncVariableName)
        {
            return $"window.nceditablecollection?.mixin?.({vueSyncVariableName}, {renderModel.ContentPage.Id});";
        }

        public bool WillInclude(ContentRenderModel renderModel)
        {
            return true;
        }
    }
}
