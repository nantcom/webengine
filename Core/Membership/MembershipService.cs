using RestSharp;
using System.Text.Json;
using System.Text.Json.Nodes;

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

        public string FullName => _context.User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

        public string Emails => _context.User.Claims.FirstOrDefault(c => c.Type == "emails")?.Value;

        /// <summary>
        /// Unique Identifier of this user
        /// </summary>
        public string UserId => _context.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

        public MembershipService(IHttpContextAccessor ctx) {
            _context = ctx.HttpContext;
        }

        public MembershipService() { }

        public void RegisterBuilder(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<MembershipService>();
        }

        public async Task EnrollUserAsAdmin()
        {
            if (this.IsAnonymous)
            {
                throw new InvalidOperationException("Require non anonymous user");
            }

            var config = _context.RequestServices.GetRequiredService<IConfiguration>();
            var rbacSettings = new RBACSettings();
            config.GetRequiredSection("RBAC").Bind(rbacSettings);

            var url = $"https://login.microsoftonline.com/{rbacSettings.TenantId}/oauth2/v2.0/token";

            var client = new RestClient();
            var req = new RestRequest( url, Method.Post );
            req.AddParameter("scope", "https://graph.microsoft.com/.default");
            req.AddParameter("grant_type", "client_credentials");
            req.AddParameter("client_id", rbacSettings.ClientId);
            req.AddParameter("client_secret", rbacSettings.ClientSecret);

            var response = await client.ExecuteAsync(req);

            if (response.IsSuccessStatusCode == false)
            {
                throw new UnauthorizedAccessException("Could not get Token");
            }

            var result = JsonNode.Parse(response.Content);
            var token = result["access_token"].ToString();
            client.AddDefaultHeader("Authorization", "Bearer " + token);

            var getExistingRequest = new RestRequest($"https://graph.microsoft.com/v1.0/users/{this.UserId}?$select=id,{rbacSettings.RoleAttribute}");
            var getExistingResponse = await client.ExecuteAsync(getExistingRequest);

            if (getExistingResponse.IsSuccessStatusCode == false)
            {
                throw new UnauthorizedAccessException("Could not get current Roles");
            }

            var getExistingResult = JsonNode.Parse(getExistingResponse.Content);
            var existingRoles = JsonSerializer.Deserialize<List<string>>(getExistingResult[rbacSettings.RoleAttribute].ToString());

            if (rbacSettings.IsUseSiteSpecificRole)
            {
                existingRoles.Add($"Admin_{rbacSettings.SiteId}");
            }
            else
            {
                existingRoles.Add("Admin");
            }

            var finalRoles = JsonSerializer.Serialize(existingRoles.Distinct());

            var requestBody = new JsonObject
            {
                [rbacSettings.RoleAttribute] = finalRoles
            };

            var addRoleRequest = new RestRequest( $"https://graph.microsoft.com/v1.0/users/{this.UserId}", Method.Patch);
            addRoleRequest.AddJsonBody(requestBody.ToJsonString(), false);
            var addRoleResponse = await client.ExecuteAsync(addRoleRequest);

            if (addRoleResponse.IsSuccessStatusCode == false)
            {
                throw new UnauthorizedAccessException("Unable to Add Role");
            }

        }
    }
}
