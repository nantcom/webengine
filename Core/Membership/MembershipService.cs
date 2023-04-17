namespace NC.WebEngine.Core.Membership
{
    public class MembershipService : IService
    {
        private HttpContext _context;

        /// <summary>
        /// Whether current user has Admin role
        /// </summary>
        public bool IsAdmin => _context.User.IsInRole("Admin");
        
        /// <summary>
        /// Whether current user has Editor role, user with Admin Role will also have Editor Role
        /// </summary>
        public bool IsEditor => _context.User.IsInRole("Editor") || this.IsAdmin;

        public bool IsAnonymous => _context.User.Claims.Any() == false;

        public MembershipService(IHttpContextAccessor ctx) {
            _context = ctx.HttpContext;
        }

        public MembershipService() { }

        public void RegisterBuilder(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<MembershipService>();
        }
    }
}
