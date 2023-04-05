using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using System.Text.Json;

namespace NC.WebEngine.Core.VueSync
{
    public class VueSyncModule : IModule
    {
        public class SyncMethod
        {
            public string Key { get; set; }

            public Type VueSyncModelType { get; set; }

            public Action<object> Delegate { get; set; }

            public string[] MutatedProperties { get; set; }
        }

        public class CallableMethod
        {
            public string Key { get; set; }

            public Type VueSyncModelType { get; set; }

            public Type MethodParameter {  get; set; }

            public Func<object, object, object> Delegate { get; set; }
        }

        private Dictionary<string, SyncMethod> _SyncMethods = new();
        private Dictionary<string, CallableMethod> _CallableMethods = new();

        public void Register(WebApplication app)
        {
            app.MapPost("/__vuesync/{typeName}/sync/{method}", this.HandleSync);
            app.MapPost("/__vuesync/{typeName}/call/{method}", this.HandleCall);

            var types = Assembly.GetExecutingAssembly().GetTypes()
                            .Where(t => typeof(IVueModel).IsAssignableFrom(t) && t.IsClass);

            foreach (var type in types)
            {
                this.CreateSyncHandler(type);
                this.CreateCallableHandler(type);
            }
        }

        private void CreateSyncHandler( Type type )
        {
            var syncMethods = from m in type.GetMethods()
                              let syncMethodInfo = m.GetCustomAttribute<VueSyncMethod>()
                              where syncMethodInfo != null && m.GetParameters().Length == 0
                              select new SyncMethod()
                              {
                                  Key = $"{type.FullName}-{m.Name}",
                                  VueSyncModelType = type,
                                  Delegate = (object instance) => m.Invoke(instance, null),
                                  MutatedProperties = syncMethodInfo.MutatedProperties
                              };

            foreach ( var method in syncMethods )
            {
                _SyncMethods[method.Key] = method;
            }
        }

        private void CreateCallableHandler(Type type)
        {
            var callableMethods = from m in type.GetMethods()
                              let syncMethodInfo = m.GetCustomAttribute<VueCallableMethod>()
                              where syncMethodInfo != null && m.GetParameters().Length == 1
                              select new CallableMethod()
                              {
                                  Key = $"{type.FullName}-{m.Name}",
                                  VueSyncModelType = type,
                                  Delegate = (object instance, object parameter) => m.Invoke(instance, new [] { parameter }),
                                  MethodParameter = m.GetParameters()[0].ParameterType,
                              };

            foreach (var method in callableMethods)
            {
                _CallableMethods[method.Key] = method;
            }
        }

        private async Task<IResult> HandleSync(HttpContext ctx, string typeName, string method)
        {
            var key = $"{typeName}-{method}";
            SyncMethod handler;

            if (_SyncMethods.TryGetValue(key, out handler) == false)
            {
                return Results.NotFound();
            }

            var instance = await JsonSerializer.DeserializeAsync(ctx.Request.Body, handler.VueSyncModelType);
            IVueModel? model = instance as IVueModel;

            model!.OnPostback(ctx);
            handler.Delegate(instance!);

            return Results.Ok(instance);
        }

        private async Task<IResult> HandleCall(HttpContext ctx, string typeName, string method)
        {
            var key = $"{typeName}-{method}";
            CallableMethod handler;

            if (_CallableMethods.TryGetValue(key, out handler) == false)
            {
                return Results.NotFound();
            }

            var instance = Activator.CreateInstance(handler.VueSyncModelType);
            var parameter = await JsonSerializer.DeserializeAsync(ctx.Request.Body, handler.MethodParameter);

            var result = handler.Delegate(instance!, parameter!);

            return Results.Ok(result);
        }
    }
}
