using NC.WebEngine.Core.Content;
using NC.WebEngine.Core.VueSync;
using System.Text.Json.Serialization;

namespace NC.WebEngine.Core.Editor
{
    public class EditorVueModel : IVueModel
    {
        private ContentService _contentService;

        [VueCallableMethod]
        public ContentPart SavePart(ContentPart p)
        {
            return _contentService.SaveContentPart(p);
        }

        public void OnCreated(HttpContext ctx)
        {
        }

        public void OnPostback(HttpContext ctx)
        {
            _contentService = ctx.RequestServices.GetRequiredService<ContentService>();
        }
    }
}
