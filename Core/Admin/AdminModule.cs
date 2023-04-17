namespace NC.WebEngine.Core.Admin
{
    public class AdminModule : IModule
    {
        public static string SelfEnrollKey { get; set; }

        public void Register(WebApplication app)
        {
            AdminModule.SelfEnrollKey = Guid.NewGuid().ToString();
            File.WriteAllText("selfenrollkey.txt", AdminModule.SelfEnrollKey);

            app.MapPage<AdminEnrollVueModel>("/__admin/selfenroll", "Self Enroll", "SystemViews/Admin/SelfEnroll");
        }
    }
}
