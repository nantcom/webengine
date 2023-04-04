using NC.WebEngine.Core;
using System.Reflection;

namespace NC.WebEngine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
            builder.Services.AddRazorTemplating();
            
            Program.RegisterServices(builder.Services);

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
                var instance = (IModule?)Activator.CreateInstance(module);
                instance!.Register(app);
            }
        }

        private static void RegisterServices(IServiceCollection services)
        {
            var modules = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Where(t => typeof(IService).IsAssignableFrom(t) && t != typeof(IService));

            foreach (var module in modules)
            {
                var instance = (IService?)Activator.CreateInstance(module);
                instance!.Register(services);
            }
        }
    }
}