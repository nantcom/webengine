namespace NC.WebEngine.Core.Content
{
    public class ContentRenderModel
    {
        public ContentPage ContentPage { get; set; }

        public List<ContentPart> ContentParts { get; set; } = new();
    }
}
