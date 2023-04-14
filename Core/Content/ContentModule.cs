using HtmlAgilityPack;
using Microsoft.AspNetCore.Http.Features;
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

        private List<IPostProcessor> _postProcessors;

        public void Register(WebApplication app)
        {
            app.Configuration.Bind("ContentModule", this);

            app.MapGet("/", (ContentService contentService, HttpContext ctx) => this.RenderView(contentService, ctx, "/"));

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

        private async Task RenderView(ContentService contentService, HttpContext ctx, string url )
        {
            var staticServed = await this.StaticFiles(ctx, url);
            if (staticServed)
            {
                return;
            }

            var contentRendererModel = contentService.GetContentRenderModel(ctx, url);
            if (contentRendererModel == ContentRenderModel.NotFound )
            {
                ctx.Response.StatusCode = 404;
                return;
            }

            var syncIoFeature = ctx.Features.Get<IHttpBodyControlFeature>();
            if (syncIoFeature != null)
            {
                syncIoFeature.AllowSynchronousIO = true;
            }

            var html = await RazorTemplateEngine.RenderAsync(contentRendererModel.ContentPage.View, contentRendererModel);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            foreach (var processor in _postProcessors)
            {
                processor.Process(contentRendererModel, document);
            }

            ctx.Response.ContentType = "text/html";

            document.Save(ctx.Response.Body);

            await ctx.Response.CompleteAsync();
        }

    }
}
