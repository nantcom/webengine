using NC.WebEngine.Core.Content;
using NC.WebEngine.Core.VueSync;

namespace NC.WebEngine.Core.EditableCollection
{
    public class EditableCollectionVueModel : IVueModel
    {
        public class SetPageDateParameter
        {
            public int pageId { get; set; }

            public string date { get; set; }

            public string outputFormat { get; set; } = "dd MMMM yyyy";
        }

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

        [VueCallableMethod]
        public string SetPageDate(SetPageDateParameter p)
        {
            var dt = DateTimeOffset.Parse(p.date);

            contentService.ChangePageDate( p.pageId, dt );

            return dt.ToString(p.outputFormat);
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
