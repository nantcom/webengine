using NC.WebEngine.Core.VueSync;

namespace NC.WebEngine.Core.Content
{
    public class ContentRenderModel
    {
        public ContentPage ContentPage { get; set; }

        public List<ContentPart> ContentParts { get; set; } = new();

        public IVueModel? VueModel { get; set; }

        public HttpContext HttpContext { get; set; }
    }
}
