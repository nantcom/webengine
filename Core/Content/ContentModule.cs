using NC.WebEngine.Core.Data;
using Razor.Templating.Core;
using System;
using System.IO.Pipelines;

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

        public void Register(WebApplication app)
        {
            app.Configuration.Bind("ContentModule", this);

            app.MapGet("/", (DatabaseService db, HttpContext ctx ) => this.RenderView( db, ctx, "/"));

            app.MapGet("/{*url}", this.RenderView);
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

            var viewModel = new ContentRenderModel()
            {
                ContentPage = pageToRender,
                ContentParts = parts.ToList()
            };

            var html = await RazorTemplateEngine.RenderAsync(pageToRender.View, viewModel);



            ctx.Response.ContentType = "text/html";
            await ctx.Response.WriteAsync(html);
            await ctx.Response.CompleteAsync();
        }
    }
}
