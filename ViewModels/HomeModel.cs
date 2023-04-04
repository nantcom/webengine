using NC.WebEngine.Core.VueSync;

namespace NC.WebEngine.ViewModels
{
    public class HomeModel : IVueModel
    {
        public int Counter { get; set; } = 1;

        [VueSyncMethod(
            RequiredProperties = new[] { "Counter" },
            MutatedProperties = new[] {"Counter" })]
        public void Increment()
        {
            this.Counter++;
        }

        public void OnCreated(HttpContext ctx)
        {
            this.Counter = 0;
        }

        public void OnPostback(HttpContext ctx)
        {
        }
    }
}
