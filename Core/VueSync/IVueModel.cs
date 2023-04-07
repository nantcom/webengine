namespace NC.WebEngine.Core.VueSync
{
    public interface IVueModel
    {
        /// <summary>
        /// Callbed by VueSync system when instance was restored on server side
        /// </summary>
        /// <param name="ctx"></param>
        void OnPostback(HttpContext ctx);

        /// <summary>
        /// Called by VueSync system when instance was created
        /// </summary>
        /// <param name="ctx"></param>
        void OnCreated(HttpContext ctx);
    }

    public class EmptyVueModel : IVueModel
    {
        public static readonly EmptyVueModel Instance = new EmptyVueModel();

        public void OnCreated(HttpContext ctx)
        {
        }

        public void OnPostback(HttpContext ctx)
        {
        }
    }
}
