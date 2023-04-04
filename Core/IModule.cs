namespace NC.WebEngine.Core
{
    internal interface IModule
    {
        /// <summary>
        /// Registers the route in the web application
        /// </summary>
        /// <param name="app"></param>
        public void Register(WebApplication app);
    }
}
