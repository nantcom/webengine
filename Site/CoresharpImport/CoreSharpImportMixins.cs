using NC.WebEngine.Core.Content;
using NC.WebEngine.Core.Membership;
using NC.WebEngine.Core.VueSync;

namespace NC.WebEngine.Site.CoresharpImport
{
    public class CoreSharpImportMixins : IVueSyncMixins
    {
        public string[] JsFiles => new[] { "/js/coresharpimport/coresharpimport.js" };

        public string CallMixins(ContentRenderModel renderModel, string vueSyncVariableName)
        {
            return $"window.coresharpimport?.mixin?.({vueSyncVariableName}, {renderModel.ContentPage.Id});";
        }

        public bool WillInclude(ContentRenderModel renderModel)
        {
            return renderModel.HttpContext.RequestServices.GetRequiredService<MembershipService>().IsEditor;
        }
    }
}
