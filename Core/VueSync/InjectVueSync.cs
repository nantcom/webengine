using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using System.Text.Json;
using System.Reflection;
using NC.WebEngine.Core.VueSync;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NC.WebEngine.Core.Content.PostProcessors
{
    public class InjectVueSync : IPostProcessor
    {
        Dictionary<Type, string> _syncInfoCache = new();

        private string CreateSyncInfo( IVueModel model )
        {
            string syncInfo;
            if (_syncInfoCache.TryGetValue( model.GetType(), out syncInfo ) == false)
            {
                var typeName = model.GetType().FullName;
                var syncMethods = from method in model.GetType().GetMethods()
                                  let syncMethodInfo = method.GetCustomAttribute<VueSyncMethod>()
                                  where syncMethodInfo != null && method.GetParameters().Length == 0
                                  select (method.Name, new
                                  {
                                      required = syncMethodInfo.RequiredProperties.ToDictionary(p => p, p => true),
                                      mutated = syncMethodInfo.MutatedProperties.ToDictionary(p => p, p => true)
                                  });

                var callableMethods = from method in model.GetType().GetMethods()
                                      let callableMethodInfo = method.GetCustomAttribute<VueCallableMethod>()
                                      where callableMethodInfo != null && method.GetParameters().Length == 1
                                      select method.Name;

                syncInfo = JsonSerializer.Serialize(new
                {
                    typeName = typeName,
                    syncMethods = syncMethods.ToDictionary(m => m.Name, m => m.Item2),
                    computed = callableMethods.ToList()
                });

                _syncInfoCache[model.GetType()] = syncInfo;
            }

            return syncInfo;
        }

        public void Process(ContentRenderModel renderModel, HtmlDocument document)
        {
            var body = document.DocumentNode.QuerySelector("body");
            if ( renderModel.VueModel != null )
            {
                body.AppendChild(HtmlNode.CreateNode("<script src=\"/js/vue/vue.global.min.js\"></script>"));
                body.SetAttributeValue("id", "ncwapp");

                body.AppendChild(HtmlNode.CreateNode("<script src=\"/js/ncweb/vuesync.js\"></script>"));

                body.AppendChild(HtmlNode.CreateNode(
                @$"<script>
                    var syncInfo = {this.CreateSyncInfo(renderModel.VueModel)};
                    syncInfo.model = {JsonSerializer.Serialize((object)renderModel.VueModel)};
                    Vue.createApp(
                        window.ncvuesync.generateVueSync(syncInfo)
                    ).mount('#ncwapp');
                </script>"));
            }

        }
    }
}
