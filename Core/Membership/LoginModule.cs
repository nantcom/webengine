using System.Security.Claims;
using System.Text.Json;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Principal;
using System.Data;

namespace NC.WebEngine.Core.Membership;


public class LoginModule : IModule
{
    public class AzureADSettings
    {
        public string Instance { get; set; }

        public string Domain { get; set; }

        public string ClientId { get; set; }

        public string SignUpSignInPolicyId { get; set; }
    }

    public class AzureADOpenIdSettings
    {
        public string issuer { get; set; }
        public string authorization_endpoint { get; set; }
        public string token_endpoint { get; set; }
        public string end_session_endpoint { get; set; }
        public string jwks_uri { get; set; }

        public SecurityKey SigningKey { get; set; }
        public TokenValidationParameters ValidationParameters { get; set; }
    }

    public class AuzreADKeys
    {
        public List<AzureADKey> keys { get; set; }
    }

    public class AzureADKey
    {
        public string kid { get; set; }
        public int nbf { get; set; }
        public string use { get; set; }
        public string kty { get; set; }
        public string e { get; set; }
        public string n { get; set; }
    }

    public AzureADSettings AzureADB2C { get; set; } = new();
    public AzureADOpenIdSettings OpenIdSettings = new();
    public RBACSettings RBAC { get; set; } = new();

    public void Register(WebApplication app)
    {
        if (app.Configuration.GetSection(ConfigKeys.AzureADB2C).Exists() == false)
        {
            app.Services.GetService<ILogger>().LogWarning("There is no configuration for AzureAD B2C, Login Module does not initialized");
            return;
        }

        app.Configuration.Bind(ConfigKeys.AzureADB2C, this.AzureADB2C);
        app.Configuration.Bind(ConfigKeys.RBAC, this.RBAC);

        this.OpenIdSettings = ReadConfiguration(this.AzureADB2C).Result;

        app.Use((htx, next) =>
        {
            if (htx.Request.Headers.Accept.Any(s => s.Contains("text/html") || s.Contains("application/json")) == false)
            {
                return next(htx);
            }

            var token = htx.Request.Cookies[CookieKeys.LOGIN_TOKEN];
            if (token != null)
            {
                var cache = htx.RequestServices.GetRequiredService<IMemoryCache>();
                htx.User = this.GetUserFromToken(token, cache);
            }

            return next(htx);
        });

        app.MapGet("/__membership/signout", (HttpContext htx) =>
        {
            htx.Response.Cookies.Append(CookieKeys.LOGIN_TOKEN, "");

            return Results.Redirect("/");
        });

        app.MapGet("/__membership/receivetoken", async (HttpContext htx) =>
        {   
            var token = htx.Request.Query["id_token"];
            if (token.Count() > 0)
            {
                htx.Response.Cookies.Append(CookieKeys.LOGIN_TOKEN, token, new CookieOptions() { Expires = DateTimeOffset.Now.AddDays(180) });
            }

            if (token.Count() == 0)
            {
                var message = htx.Request.Query["error_description"];
                htx.Response.Cookies.Append(CookieKeys.GENERIC_SYSTEM_MESSAGE, message, new CookieOptions() { Expires = DateTimeOffset.Now.AddDays(-1) });
            }

            htx.Response.Headers["Location"] = htx.Request.Cookies[CookieKeys.LOGIN_BEFORE_LOGIN_URL] ?? "/";
            htx.Response.Cookies.Delete(CookieKeys.LOGIN_BEFORE_LOGIN_URL);

            return Results.Redirect(htx.Response.Headers["Location"]);
        });

        app.MapGet("/__membership/signin", (HttpContext htx) =>
        {
            var lastUrl = htx.Request.Headers["Referer"].FirstOrDefault();
            if (lastUrl != null)
            {
                htx.Response.Cookies.Append(CookieKeys.LOGIN_BEFORE_LOGIN_URL, lastUrl);
            }

            var nextPage = htx.Request.Query[QueryStringKeys.LOGIN_NEXT_PAGE];
            if (nextPage.Count != 0)
            {
                htx.Response.Cookies.Append(CookieKeys.LOGIN_BEFORE_LOGIN_URL, nextPage);
            }

            nextPage = htx.Request.Query[QueryStringKeys.LOGIN_NEXT_PAGE_ALT];
            if (nextPage.Count != 0)
            {
                htx.Response.Cookies.Append(CookieKeys.LOGIN_BEFORE_LOGIN_URL, nextPage);
            }

            var https = htx.Request.Host.Host != "localhost" && htx.Request.Host.Host.StartsWith("192") == false;

            var callback = $"http{(https ? "s" : "")}://{htx.Request.Host}/__membership/receivetoken";
            var url = $"{this.AzureADB2C.Instance}/{this.AzureADB2C.Domain}/oauth2/v2.0/authorize?p={this.AzureADB2C.SignUpSignInPolicyId}&client_id={this.AzureADB2C.ClientId}&nonce=defaultNonce&redirect_uri={Uri.EscapeDataString(callback)}&scope=openid&response_type=id_token&prompt=login&response_mode=query";

            return Results.Redirect(url);
        });

    }
    
    private ClaimsPrincipal? GetUserFromToken( string token, IMemoryCache cache)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        var cached = cache.Get(token) as ClaimsPrincipal;
        if (cached != null)
        {
            return cached;
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        if (jwtToken.ValidTo <= DateTime.UtcNow)
        {
            cache.Remove(token);
            return null;
        }

        ClaimsPrincipal? user = null;

        Action<string> extractRoles = (claimType) =>
        {
            var rolesClaim = user.Claims.Where(item => item.Type == claimType).FirstOrDefault();

            if (rolesClaim != null)
            {
                var roles = JsonSerializer.Deserialize<string[]>(rolesClaim.Value);
                foreach (var role in roles)
                {
                    if (this.RBAC.IsUseSiteSpecificRole)
                    {
                        var rolePart = role.Split("_", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (rolePart.Length == 2 && rolePart[1] == this.RBAC.SiteId)
                        {
                            user.AddIdentity(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, rolePart[0]) }));
                            user.AddIdentity(new ClaimsIdentity(new[] { new Claim("roles", rolePart[0]) }));
                        }
                    }
                    else
                    {
                        user.AddIdentity(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role) }));
                        user.AddIdentity(new ClaimsIdentity(new[] { new Claim("roles", role) }));
                    }
                }
            }
        };

        try
        {
            SecurityToken validatedToken;
            user = handler.ValidateToken(token, this.OpenIdSettings.ValidationParameters, out validatedToken);

            // Add Roles from Roles Claim from Azure AD B2C
            extractRoles("roles");
            extractRoles("extension_Roles");

            // We made the cache expire slightly before token, so we can get a new one
            cache.Set(token, user, new DateTimeOffset(jwtToken.ValidTo.AddSeconds(-20)));
        }
        catch (Exception e)
        {
            cache.Remove(token);
        }


        return user;
    }

    private async Task<AzureADOpenIdSettings> ReadConfiguration(AzureADSettings settings)
    {
        using var client = new HttpClient();
        var url = $"{settings.Instance}/{settings.Domain}/v2.0/.well-known/openid-configuration?p={settings.SignUpSignInPolicyId}";
        var settingsJson = await client.GetStringAsync(url);

        var settingsObj = JsonSerializer.Deserialize<AzureADOpenIdSettings>(settingsJson);
        var keyUrl = settingsObj!.jwks_uri;

        var jwksJson = await client.GetStringAsync(keyUrl);

        var jwks = new JsonWebKeySet(jwksJson);
        var jwk = jwks.Keys.First();

        settingsObj.SigningKey = jwk;

        settingsObj.ValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidIssuer = settingsObj.issuer,
            IssuerSigningKey = settingsObj.SigningKey,
            NameClaimType = "emails",
        };

        return settingsObj;
    }

}
