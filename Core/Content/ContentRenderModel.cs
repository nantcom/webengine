using NC.WebEngine.Core.VueSync;

namespace NC.WebEngine.Core.Content
{
    public class ContentRenderModel
    {
        public string SiteTitle { get; set; }

        public string Language { get; set; } = "th";

        public ContentPage ContentPage { get; set; }

        public List<ContentPart> ContentParts { get; set; } = new();

        public IVueModel? VueModel { get; set; }

        public HttpContext HttpContext { get; set; }

        public static readonly ContentRenderModel NotFound = new();
    }
}
