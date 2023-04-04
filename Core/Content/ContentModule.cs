using HtmlAgilityPack;
using NC.WebEngine.Core.Data;
using NC.WebEngine.Core.VueSync;
using Razor.Templating.Core;
using System;
using System.IO.Pipelines;
using System.Reflection;

namespace NC.WebEngine.Core.Content
{
    public class ContentModule : IModule
    {
        /// <summary>
        /// Site Name
        /// </summary>
        public string SiteTitle { get; set; } = "Default Site";

        /// <summary>
        /// List of standard pages, which they will be pre-created
        /// </summary>
        public List<ContentPage> StandardPages { get; set; } = new();

        public Dictionary<string, string> PageModels { get; set; } = new();

        private List<IPostProcessor> _postProcessors;

        public void Register(WebApplication app)
        {
            app.Configuration.Bind("ContentModule", this);

            app.MapGet("/", (DatabaseService db, HttpContext ctx ) => this.RenderView( db, ctx, "/"));

            app.MapGet("/{*url}", this.RenderView);

            _postProcessors = Assembly.GetExecutingAssembly().GetTypes()
                                .Where( t => typeof(IPostProcessor).IsAssignableFrom(t) && t.IsClass)
                                .Select( t => (IPostProcessor)Activator.CreateInstance(t)! )
                                .ToList();
        }

        private string ConvertPathToUrl( string path)
        {
            var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            return path.Replace(wwwroot, "").Replace("\\", "/");
        }

        private string ConvertUrlToPath(string url)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", url.Replace("/", "\\"));
        }

        private async Task<bool> StaticFiles( HttpContext ctx, string url )
        {
            var path = this.ConvertUrlToPath(url);
            if (File.Exists(path))
            {
                ctx.Response.ContentType = MimeTypes.GetMimeType(url);

                using var fs = File.OpenRead(path);
                await fs.CopyToAsync(ctx.Response.Body);
                await ctx.Response.CompleteAsync();

                return true;
            }

            return false;
        }

        private async Task RenderView( DatabaseService db, HttpContext ctx, string url )
        {
            var staticServed = await this.StaticFiles(ctx, url);
            if (staticServed)
            {
                return;
            }

            var pageToRender = db.Connection.LinqTo<ContentPage>()
                            .Where(x => x.Url == url)
                            .Result()
                            .FirstOrDefault();

            if ( pageToRender == null)
            {
                pageToRender = this.StandardPages.FirstOrDefault(p => p.Url == url);
                if (pageToRender != null)
                {
                    db.Connection.Insert(pageToRender);
                }
            }

            if (pageToRender == null)
            {
                ctx.Response.StatusCode = 404;
                return;
            }

            var parts = db.Connection.LinqTo<ContentPart>()
                            .Where(cp => cp.ContentPageId == pageToRender.Id)
                            .Result();

            pageToRender.ContentPartNames = parts.Select(p => p.Name).ToList();

            var vueModelTypeName = this.PageModels.ContainsKey(url) ? this.PageModels[url] : string.Empty;
            IVueModel? vueModelInstance = null;
            var vueModelType = Type.GetType(vueModelTypeName);
            if (vueModelType != null )
            {
                vueModelInstance = (IVueModel?)Activator.CreateInstance(vueModelType);
                vueModelInstance!.OnCreated(ctx);
            }

            var viewModel = new ContentRenderModel()
            {
                ContentPage = pageToRender,
                ContentParts = parts.ToList(),
                VueModel = vueModelInstance,
            };

            var html = await RazorTemplateEngine.RenderAsync(pageToRender.View, viewModel);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            foreach (var processor in _postProcessors)
            {
                processor.Process(viewModel, document);
            }

            ctx.Response.ContentType = "text/html";

            document.Save(ctx.Response.Body);

            await ctx.Response.CompleteAsync();
        }

    }
}
