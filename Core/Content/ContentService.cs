using NC.WebEngine.Core.Data;
using NC.WebEngine.Core.VueSync;

namespace NC.WebEngine.Core.Content
{
    public class ContentService : IService
    {
        /// <summary>
        /// Site Name
        /// </summary>
        public string SiteTitle { get; set; } = "Default Site";

        public Dictionary<string, string> PageModels { get; set; } = new();

        public void RegisterBuilder(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<ContentService>();
        }

        private DatabaseService? _db;

        public ContentService(DatabaseService db, IConfiguration config)
        {
            _db = db;
            config.Bind("ContentModule", this);
        }

        public ContentService()
        {
        }

        /// <summary>
        /// Gets the render
        /// </summary>
        /// <param name="contentModule"></param>
        /// <param name="db"></param>
        /// <param name="ctx"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public ContentRenderModel GetContentRenderModel(HttpContext ctx, string url)
        {   
            var pageToRender = _db.Connection.LinqTo<ContentPage>()
                            .Where(x => x.Url == url)
                            .Result()
                            .FirstOrDefault();

            if (pageToRender == null)
            {
                return ContentRenderModel.NotFound;
            }

            var parts = _db.Connection.LinqTo<ContentPart>()
                            .Where(cp => cp.ContentPageId == pageToRender.Id)
                            .Result();

            pageToRender.ContentPartNames = parts.Select(p => p.Name).ToList();

            var vueModelTypeName = this.PageModels.ContainsKey(url) ? this.PageModels[url] : string.Empty;
            IVueModel? vueModelInstance = null;
            var vueModelType = Type.GetType(vueModelTypeName);
            if (vueModelType != null)
            {
                vueModelInstance = (IVueModel?)Activator.CreateInstance(vueModelType);
                vueModelInstance!.OnCreated(ctx);
            }

            return new ContentRenderModel()
            {
                SiteTitle = this.SiteTitle,
                ContentPage = pageToRender,
                ContentParts = parts.ToList(),
                VueModel = vueModelInstance,
            };
        }

        /// <summary>
        /// Saves the content part
        /// </summary>
        public ContentPart SaveContentPart( ContentPart p )
        {
            if (p.ContentPageId == 0)
            {
                throw new InvalidOperationException("Require ContentPageId");
            }

            //TODO: Render HTML if block content
            //TODO: Sanitize the HTML
            _db!.Connection.Upsert( p );

            return p;
        }
    }
}
