using NC.WebEngine.Core.Content;
using NC.WebEngine.Core.VueSync;

namespace NC.WebEngine.Core.EditableCollection
{
    public class EditableCollectionVueModel : IVueModel
    {
        private ContentService contentService;

        [VueCallableMethod]
        public ContentPage CreatePage( string baseUrl )
        {
            return contentService.CreatePage( baseUrl );
        }

        [VueCallableMethod]
        public void DeletePage(int pageId)
        {
            contentService.DeletePage( pageId );
        }

        public void OnPostback(HttpContext ctx)
        {
            contentService = ctx.RequestServices.GetRequiredService<ContentService>();
        }

        public void OnCreated(HttpContext ctx)
        {
        }

    }
}
