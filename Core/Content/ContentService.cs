using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NC.WebEngine.Core.Data;
using NC.WebEngine.Core.VueSync;
using System;
using System.Runtime.CompilerServices;

namespace NC.WebEngine.Core.Content
{
    public class ContentService : IService
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

        public Dictionary<string, string> PageLayouts { get; set; } = new();

        public void RegisterBuilder(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<ContentService>();
        }

        private DatabaseService? _db;

        public ContentService(DatabaseService db, IConfiguration config)
        {
            _db = db;
            config.Bind("ContentModule", this);

            this.CreateStandardPages();
        }

        public ContentService()
        {
        }

        private void CreateStandardPages()
        {
            foreach (var page in this.StandardPages)
            {
                var exists = _db!.Connection.LinqTo<ContentPage>()
                            .Where(x => x.Url == page.Url)
                            .Result()
                            .Any();

                if (!exists)
                {
                    _db.Connection.Upsert(page);
                }
            }
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
            if (url.StartsWith("/deleted"))
            {
                return ContentRenderModel.NotFound;
            }

            if (url.StartsWith("/") == false)
            {
                url = "/" + url;
            }

            var pageToRender = _db!.Connection.LinqTo<ContentPage>()
                            .Where(x => x.Url == url)
                            .Result()
                            .FirstOrDefault();

            if (pageToRender == null)
            {
                return ContentRenderModel.NotFound;
            }

            if (string.IsNullOrEmpty(pageToRender.View))
            {
                foreach (var viewSettings in this.PageLayouts)
                {
                    if ( pageToRender.Url.StartsWith( viewSettings.Key ))
                    {
                        pageToRender.View = viewSettings.Value;
                        break;
                    }
                }
            }

            var vueModelTypeName = this.PageModels.ContainsKey(url) ? this.PageModels[url] : string.Empty;
            IVueModel? vueModelInstance = null;
            var vueModelType = Type.GetType(vueModelTypeName);
            if (vueModelType != null)
            {
                vueModelInstance = (IVueModel?)Activator.CreateInstance(vueModelType);
                vueModelInstance!.OnCreated(ctx);
            }

            var parts = _db.Connection.LinqTo<ContentPart>()
                            .Where(cp => cp.ContentPageId == pageToRender.Id)
                            .Result();

            var latestParts = from part in parts
                            orderby part.Created descending
                            group part by part.Name into g
                            select g.First();


            return new ContentRenderModel()
            {
                SiteTitle = this.SiteTitle,
                ContentPage = pageToRender,
                ContentPartHistory = parts.ToList(),
                ContentParts = latestParts.ToDictionary( p => p.Name ),
                ContentService = this,
                VueModel = vueModelInstance,
            };
        }

        public IEnumerable<ContentPage> GetPagesUnder( string url)
        {
            return _db!.Connection.LinqTo<ContentPage>()
                            .Where(x => x.Url.StartsWith(url) && x.Url != url)
                            .Result();
        }

        public IEnumerable<ContentPart> GetContentParts(int id)
        {
            return _db!.Connection.LinqTo<ContentPart>()
                            .Where(x => x.ContentPageId == id)
                            .Result();
        }

        public ContentPage CreatePage( string baseUrl )
        {
            var contentPage = new ContentPage();
            contentPage.Url = $"{baseUrl}/{Guid.NewGuid()}";
            contentPage.Title = "(New Page)";

            _db!.Connection.Upsert(contentPage);

            return contentPage;
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

            _db!.Connection.Upsert( p );

            if (p.Name == "Title" || p.Name == "Description")
            {
                var contentPage = _db!.Connection.LinqTo<ContentPage>()
                                    .Where(page => page.Id == p.ContentPageId)
                                    .Result()
                                    .FirstOrDefault();

                var doc = new HtmlDocument();
                doc.LoadHtml(p.Content);

                if (p.Name == "Title")
                {
                    contentPage.Title = doc.DocumentNode.InnerText;
                }
                else
                {
                    contentPage.Description = doc.DocumentNode.InnerText;
                }
                

                _db!.Connection.Upsert(contentPage);
            }

            return p;
        }

        private ContentPage GetPageById(int id)
        {
            var page = _db!.Connection.LinqTo<ContentPage>()
                            .Where(page => page.Id == id)
                            .Result()
                            .FirstOrDefault();

            if (page == null)
            {
                throw new ArgumentException("Not a valid page id");
            }

            return page;
        }

        /// <summary>
        /// Delete page of given id - the page URL will change to /deleted/{date-time}
        /// </summary>
        /// <param name="id"></param>
        public void DeletePage( int id )
        {
            var page = this.GetPageById(id);

            if (page.Url.StartsWith("/deleted"))
            {
                return;
            }

            page.Url = $"/deleted/{DateTime.Now.ToString("yyyyMMdd-HHmm")}{page.Url}";

            _db!.Connection.Upsert(page);
        }

        /// <summary>
        /// Change the created date of the page
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public ContentPage ChangePageDate( int id, DateTimeOffset newDate )
        {
            var page = this.GetPageById(id);

            page.Created = newDate;

            _db!.Connection.Upsert(page);

            return page;
        }
    }
}
