namespace NC.WebEngine.Core.Admin
{
    public class AdminModule : IModule
    {
        public static string SelfEnrollKey { get; set; }

        public static void RegenerateSelfEnrollKey()
        {
            AdminModule.SelfEnrollKey = Guid.NewGuid().ToString();
            File.WriteAllText("selfenrollkey.txt", AdminModule.SelfEnrollKey);
        }

        public void Register(WebApplication app)
        {
            if (app.Configuration.GetSection(ConfigKeys.RBAC).Exists() == false)
            {
                app.Services.GetService<ILogger>().LogWarning("There is no configuration for Role Based Access Control, Managing user role is not available.");
            }
            else
            {
                AdminModule.RegenerateSelfEnrollKey();
                app.MapPage<AdminEnrollVueModel>("/__admin/selfenroll", "Self Enroll", "SystemViews/Admin/SelfEnroll");
            }

        }
    }
}
