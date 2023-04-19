using NC.WebEngine.Core.Content;
using NC.WebEngine.Core.VueSync;
using Slugify;
using System.Text.Json.Nodes;
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

        [VueCallableMethod]
        public object Slugify( int pageId )
        {
            var helper = new SlugHelper();
            var page = _contentService.GetContentPaage(pageId);

            if ( page == null )
            {
                return new
                {
                    IsSucccess = false,
                    Message = "Not Valid Page"
                };
            }

            return helper.GenerateSlug(page.Title);
        }

        [VueCallableMethod]
        public object ChangeSlug(JsonObject parameter)
        {
            var pageId = (int)parameter["id"];
            var newSlug = (string)parameter["slug"];

            var page = _contentService.GetContentPaage(pageId);

            if (page == null)
            {
                return new
                {
                    IsSucccess = false,
                    Message = "Not Valid Page"
                };
            }

            var newUrl = page.Url.Substring(0, page.Url.LastIndexOf("/") + 1) + newSlug;

            try
            {
                _contentService.ChangePageUrl(pageId, newUrl);
            }
            catch (Exception ex)
            {
                return new
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

            return new
            {
                IsSuccess = true,
                Message = $"Url was Changed to {newUrl}"
            };
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
