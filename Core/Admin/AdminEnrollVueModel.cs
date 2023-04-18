using NC.WebEngine.Core.Membership;
using NC.WebEngine.Core.VueSync;

namespace NC.WebEngine.Core.Admin
{
    public class AdminEnrollVueModel : IVueModel
    {
        public string EnrollKey { get; set; }

        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public bool IsAlreadyAdmin { get; set; }

        public bool IsLoggedIn { get; set; }

        private MembershipService _membershipService;

        [VueSyncMethod(
            RequiredProperties = new[] {"EnrollKey"},
            MutatedProperties = new[] {"IsSuccess", "Message"}
            )]
        public void Enroll()
        {
            if ( this.EnrollKey != AdminModule.SelfEnrollKey )
            {
                this.IsSuccess = false;
                this.Message = "Invalid Enroll Key";
                return;
            }

            try
            {
                _membershipService.EnrollUserAsAdmin().Wait();
                this.IsSuccess = true;
                this.Message = null;

                AdminModule.RegenerateSelfEnrollKey();
            }
            catch (Exception ex)
            {
                this.IsSuccess = false;
                this.Message = ex.Message;
            }
        }

        public void OnCreated(HttpContext ctx)
        {
            var membership = ctx.RequestServices.GetRequiredService<MembershipService>();
            this.IsLoggedIn = membership.IsAnonymous == false;
            this.IsAlreadyAdmin = membership.IsAdmin;

            if (this.IsAlreadyAdmin)
            {
                AdminModule.RegenerateSelfEnrollKey();
                this.EnrollKey = AdminModule.SelfEnrollKey;
            }
        }

        public void OnPostback(HttpContext ctx)
        {
            _membershipService = ctx.RequestServices.GetRequiredService<MembershipService>();
        }
    }
}
