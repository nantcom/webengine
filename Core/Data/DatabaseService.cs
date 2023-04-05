using NC.SQLite;

namespace NC.WebEngine.Core.Data
{
    public class DatabaseService : IService
    {
        private SQLiteConnection _connection;

        public SQLiteConnection Connection => _connection;

        public void RegisterBuilder(WebApplicationBuilder builder)
        {
            _connection = new SQLiteConnection("SiteData.sqlite");

            builder.Services.AddSingleton<DatabaseService>(this);
        }

    }
}
