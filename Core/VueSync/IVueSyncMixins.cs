using NC.WebEngine.Core.Content;

namespace NC.WebEngine.Core.VueSync
{
    internal interface IVueSyncMixins
    {
        bool WillInclude(ContentRenderModel renderModel);

        string[] JsFiles { get; }

        string CallMixins(ContentRenderModel renderModel, string vueSyncVariableName);
    }
}
