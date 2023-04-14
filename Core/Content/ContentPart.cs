using NC.SQLite;

namespace NC.WebEngine.Core.Content
{
    [Table("ContentPart")]
    public class ContentPart
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Language of this content part
        /// </summary>
        public string Language { get; set; } = "th";

        /// <summary>
        /// Name of this content part
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Content Page that host this content part
        /// </summary>
        [Indexed(null, 0, false)]
        public int ContentPageId { get; set; }

        /// <summary>
        /// Content of this Content Part
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Whether the content is block format (from edit.js)
        /// </summary>
        public bool IsBlockContent { get; set; }

    }
}
