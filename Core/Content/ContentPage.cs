using NC.SQLite;

namespace NC.WebEngine.Core.Content
{
    [Table("Content")]
    public class ContentPage
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// URL of the page
        /// </summary>
        [Indexed(null, 0, true)]
        public string Url { get; set; }

        /// <summary>
        /// Template file for rendering
        /// </summary>
        public string View { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Keywords of this Content Page
        /// </summary>
        public List<string> Keywords { get; set; } = new();

        /// <summary>
        /// When this page was created
        /// </summary>
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    }
}
