using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using System.Text.Json;
using System.Reflection;
using NC.WebEngine.Core.VueSync;
using Microsoft.AspNetCore.Mvc.Rendering;
using NC.WebEngine.Core.Editor;

namespace NC.WebEngine.Core.Content.PostProcessors
{
    public class InjectVueSync : IPostProcessor
    {
        Dictionary<Type, string> _syncInfoCache = new();

        private List<IVueSyncMixins> _mixIns;

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

        public InjectVueSync()
        {
            _mixIns = Assembly.GetExecutingAssembly().GetTypes()
                                .Where(t => typeof(IVueSyncMixins).IsAssignableFrom(t) && t.IsClass)
                                .Select(t => (IVueSyncMixins)Activator.CreateInstance(t)!)
                                .ToList();
        }

        public void Process(ContentRenderModel renderModel, HtmlDocument document)
        {
            if (renderModel.MembershipService.IsEditor)
            {
                if (renderModel.VueModel == null)
                {
                    renderModel.VueModel = EmptyVueModel.Instance;
                }
            }

            if ( renderModel.VueModel != null )
            {
                var body = document.DocumentNode.QuerySelector("body");
                body.AppendChild(HtmlNode.CreateNode("<script src=\"/js/vue/vue.global.min.js\"></script>"));
                body.SetAttributeValue("id", "ncwapp");

                body.AppendChild(HtmlNode.CreateNode("<script src=\"/js/ncweb/vuesync.js\"></script>"));

                var includedMixIns = _mixIns.Where(mi => mi.WillInclude(renderModel)).ToList();

                foreach (var item in includedMixIns.SelectMany( m => m.JsFiles ))
                {
                    body.AppendChild(HtmlNode.CreateNode($"<script src=\"{item}\"></script>"));
                }

                var httpContext = renderModel.HttpContext;

                body.AppendChild(HtmlNode.CreateNode(
                @$"<script>
                    var syncInfo = {this.CreateSyncInfo(renderModel.VueModel)};
                    syncInfo.model = {JsonSerializer.Serialize((object)renderModel.VueModel)};
                    vueModel = window.ncvuesync.generateVueSync(syncInfo);
                    
                    {string.Join( ";\r\n", includedMixIns.Select( mi => mi.CallMixins(renderModel, "vueModel")) )}

                    Vue.createApp(vueModel).mount('#ncwapp');
                </script>"));
            }

        }
    }
}
