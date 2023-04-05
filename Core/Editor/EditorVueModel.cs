using NC.WebEngine.Core.Content;
using NC.WebEngine.Core.VueSync;
using System.Text.Json.Serialization;

namespace NC.WebEngine.Core.Editor
{
    public class EditorVueModel : IVueModel
    {
        public class SavePartParams
        {
            [JsonPropertyName("content")]
            public string Content { get; set; }

            [JsonPropertyName("partName")]
            public string PartName { get; set; }

            [JsonPropertyName("pageUrl")]
            public string PageUrl { get; set; }
        }

        [VueCallableMethod]
        public void SavePart(SavePartParams p)
        {

        }

        public void OnCreated(HttpContext ctx)
        {
        }

        public void OnPostback(HttpContext ctx)
        {
        }
    }
}
