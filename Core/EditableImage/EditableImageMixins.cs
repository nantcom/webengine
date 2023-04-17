using NC.WebEngine.Core.Content;
using NC.WebEngine.Core.Membership;
using NC.WebEngine.Core.VueSync;

namespace NC.WebEngine.Core.EditableImage
{
    public class EditableImageMixins : IVueSyncMixins
    {
        public string[] JsFiles => new[] { "/js/ncweb/editableimage.js" };

        public string CallMixins(ContentRenderModel renderModel, string vueSyncVariableName)
        {
            return $"window.nceditableimage?.mixin?.({vueSyncVariableName}, {renderModel.ContentPage.Id});";
        }

        public bool WillInclude(ContentRenderModel renderModel)
        {
            return renderModel.HttpContext.RequestServices.GetRequiredService<MembershipService>().IsEditor;
        }
    }
}
