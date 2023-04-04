using NC.SQLite;

namespace NC.WebEngine.Core.Data
{
    public class DatabaseService : IService
    {
        private SQLiteConnection _connection;

        public SQLiteConnection Connection => _connection;

        public void Register(IServiceCollection services)
        {
            _connection = new SQLiteConnection("SiteData.sqlite");

            services.AddSingleton<DatabaseService>(this);
        }

    }
}
