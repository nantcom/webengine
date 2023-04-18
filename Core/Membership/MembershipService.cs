using RestSharp;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NC.WebEngine.Core.Membership
{
    public class MembershipService : IService
    {
        private static RBACSettings _rbacSettings = new();
        private static JwtSecurityToken _token;
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
            builder.Configuration.GetRequiredSection("RBAC").Bind(MembershipService._rbacSettings);

            builder.Services.AddScoped<MembershipService>();
        }

        private void AddRole(List<string> roleList, string role)
        {
            if (_rbacSettings.IsUseSiteSpecificRole)
            {
                roleList.Add($"{role}_{_rbacSettings.SiteId}");
            }
            else
            {
                roleList.Add(role);
            }
        }

        private void RemoveRole(List<string> roleList, string role)
        {
            if (_rbacSettings.IsUseSiteSpecificRole)
            {
                roleList.Remove($"{role}_{_rbacSettings.SiteId}");
            }
            else
            {
                roleList.Remove(role);
            }
        }

        public async Task EnrollUserAsAdmin()
        {
            if (this.IsAnonymous)
            {
                throw new InvalidOperationException("Require non anonymous user");
            }

            await ModifyUserRole(this.UserId, (list) =>
            {
                this.AddRole(list, "Admin");
            });
        }

        private async Task EnsuresTokenIsValid()
        {
            if (_token != null && _token.ValidTo < DateTime.UtcNow.AddSeconds(-30))
            {
                return;
            }

            var url = $"https://login.microsoftonline.com/{_rbacSettings.TenantId}/oauth2/v2.0/token";

            var client = new RestClient();
            var req = new RestRequest(url, Method.Post);
            req.AddParameter("scope", "https://graph.microsoft.com/.default");
            req.AddParameter("grant_type", "client_credentials");
            req.AddParameter("client_id", _rbacSettings.ClientId);
            req.AddParameter("client_secret", _rbacSettings.ClientSecret);

            var response = await client.ExecuteAsync(req);

            if (response.IsSuccessStatusCode == false)
            {
                throw new UnauthorizedAccessException("Could not get Token");
            }

            var result = JsonNode.Parse(response.Content);
            var token = result["access_token"].ToString();

            var handler = new JwtSecurityTokenHandler();
            _token = handler.ReadJwtToken(token);

            
        }

        /// <summary>
        /// Modify Role of the user, use modifier delegate to perform the modification
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="modifier"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private async Task ModifyUserRole( string userId, Action<List<string>> modifier)
        {
            await this.EnsuresTokenIsValid();

            var client = new RestClient();
            client.AddDefaultHeader("Authorization", "Bearer " + _token.RawData);

            var getExistingRequest = new RestRequest($"https://graph.microsoft.com/v1.0/users/{userId}?$select=id,{_rbacSettings.RoleAttribute}");
            var getExistingResponse = await client.ExecuteAsync(getExistingRequest);

            if (getExistingResponse.IsSuccessStatusCode == false)
            {
                throw new UnauthorizedAccessException("Could not get current Roles");
            }

            var getExistingResult = JsonNode.Parse(getExistingResponse.Content);
            var existingRoles = JsonSerializer.Deserialize<List<string>>(getExistingResult[_rbacSettings.RoleAttribute].ToString());

            modifier(existingRoles);

            var finalRoles = JsonSerializer.Serialize(existingRoles.Distinct());

            var requestBody = new JsonObject
            {
                [_rbacSettings.RoleAttribute] = finalRoles
            };

            var addRoleRequest = new RestRequest( $"https://graph.microsoft.com/v1.0/users/{userId}", Method.Patch);
            addRoleRequest.AddJsonBody(requestBody.ToJsonString(), false);
            var addRoleResponse = await client.ExecuteAsync(addRoleRequest);

            if (addRoleResponse.IsSuccessStatusCode == false)
            {
                throw new UnauthorizedAccessException("Unable to Modify Role Assignment");
            }

        }
    }
}
