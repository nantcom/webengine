using NC.WebEngine.Core.VueSync;
using System.Text.Json.Serialization;

namespace NC.WebEngine.Core.Content
{
    public class ContentRenderModel
    {
        public string SiteTitle { get; set; }

        public string Language { get; set; } = "th";

        public ContentPage ContentPage { get; set; }

        public List<ContentPart> ContentParts { get; set; } = new();

        public IVueModel? VueModel { get; set; }

        [JsonIgnore]
        public HttpContext HttpContext { get; set; }

        public ContentService ContentService { get; set; }

        public static readonly ContentRenderModel NotFound = new();
    }
}
