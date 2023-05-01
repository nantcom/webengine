using Microsoft.AspNetCore.Http.Features;
using NC.WebEngine.Core;
using System.Reflection;
using System.Text.Json;

namespace NC.WebEngine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
            builder.Services.AddRazorTemplating();
            builder.Services.ConfigureHttpJsonOptions(opt =>
            {
                opt.SerializerOptions.PropertyNamingPolicy = null;
            });

            builder.Services.Configure<FormOptions>(x =>
            {
                x.MultipartBodyLengthLimit = 209715200;
            });

            builder.Services.AddMemoryCache();

            Program.RegisterBuilder(builder);

            var app = builder.Build();

            Program.RegisterModules(app);

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run();
        }

        private static void RegisterModules(WebApplication app)
        {
            var modules = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Where(t => typeof(IModule).IsAssignableFrom(t) && t != typeof(IModule) );

            foreach (var module in modules)
            {
                if (typeof(IService).IsAssignableFrom(module))
                {
                    // try to use same instance if the type is both IService and IModule
                    var moduleInstance = (IModule)app.Services.GetService(module);
                    moduleInstance!.Register(app);

                    continue;
                }

                var instance = (IModule?)Activator.CreateInstance(module);
                instance!.Register(app);
            }
        }

        private static void RegisterBuilder(WebApplicationBuilder builder)
        {
            var services = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Where(t => typeof(IService).IsAssignableFrom(t) && t != typeof(IService));

            foreach (var service in services)
            {
                var instance = (IService?)Activator.CreateInstance(service);
                instance!.RegisterBuilder(builder);
            }
        }
    }
}