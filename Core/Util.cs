using Microsoft.AspNetCore.Http.Features;
using NC.WebEngine.Core.Content;
using NC.WebEngine.Core.VueSync;
using System.Reflection.Metadata;

namespace NC.WebEngine.Core
{
    public static class Util
    {
        /// <summary>
        /// Gets Item from Dictionary if Key Exists, otherwise null
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue? GetIfExist<TKey, TValue>( this Dictionary<TKey, TValue> dict, TKey key  )
        {
            TValue value;
            if (dict.TryGetValue( key, out value ))
            {
                return value;
            }

            return default(TValue);
        }

        public static void MapPage<T>( this WebApplication app, string url, string title, string viewName)
            where T : IVueModel
        {
            app.MapGet(url, async (HttpContext ctx) =>
            {
                var syncIoFeature = ctx.Features.Get<IHttpBodyControlFeature>();
                if (syncIoFeature != null)
                {
                    syncIoFeature.AllowSynchronousIO = true;
                }

                var vueModel = (IVueModel?)Activator.CreateInstance(typeof(T));
                vueModel.OnCreated(ctx);

                var contentService = ctx.RequestServices.GetRequiredService<ContentService>();
                var document = await contentService.RenderView(viewName, new ContentRenderModel()
                {
                    ContentPage = new ContentPage()
                    {
                        Url = url,
                        View = viewName,
                        Title = title,
                    },
                    HttpContext = ctx,
                    Language = "",
                    VueModel = vueModel
                });

                ctx.Response.ContentType = "text/html";

                document.Save(ctx.Response.Body);
                

                await ctx.Response.CompleteAsync();
            });
        }
    }
}
