using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;


namespace BookyWooks.Identity;

public static class Configuration
{
    public static IEnumerable<ApiResource> ApiResources => new ApiResource[]
    {
        new ApiResource("resource_ocelot",new [] { JwtClaimTypes.Role }) { Scopes = { "ocelot_scope" } },
        new ApiResource("resource_order",new [] { JwtClaimTypes.Role }) { Scopes = { "order_scope" } },
        new ApiResource("resource_bookcatalogue",new [] { JwtClaimTypes.Role }) { Scopes = { "bookcatalogue_scope" } },
        new ApiResource(IdentityServerConstants.LocalApi.ScopeName)
    };

    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.Email(),
            new IdentityResources.OpenId(), // sub
            new IdentityResources.Profile(),
            new IdentityResource() { Name = "roles", DisplayName = "Roles", Description = "User roles", UserClaims = new[] { "role" } }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("ocelot_scope", "Full permission for ocelot gateway"),
            new ApiScope("scope1"),
            new ApiScope("scope2"),
            new ApiScope("order_scope", "Full permission for order"),
            new ApiScope("bookcatalogue_scope", "Full permission for book catalogue"),
            new ApiScope(IdentityServerConstants.LocalApi.ScopeName),
            new ApiScope("basic_scope", "Basic API Scope")
        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new Client()
            {
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientId = "orderclient",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedScopes = {"order_scope", "ocelot_scope" }
            },
             new Client()
            {
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientId = "bookcatalogueclient",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedScopes = { "bookcatalogue_scope", "ocelot_scope", IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.LocalApi.ScopeName }
            },
            new Client
            {
                ClientName = "Asp.Net Core MVC",
                ClientId = "WebMvcClient",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "ocelot_scope", "order_scope", "bookcatalogue_scope", IdentityServerConstants.LocalApi.ScopeName },
            },
            new Client
            {
                ClientName = "Asp.Net Core MVC",
                ClientId = "WebMvcClientForUserPass",
                AllowOfflineAccess = true, // refresh token
                ClientSecrets = { new IdentityServer4.Models.Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.LocalApi.ScopeName,
                    "roles",
                    "ocelot_scope",
                    "order_scope",
                    "bookcatalogue_scope",
                    IdentityServerConstants.StandardScopes.OfflineAccess, // refresh token
                },
                AccessTokenLifetime = 1 * 60 * 60,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                // AbsoluteRefreshTokenLifetime = (int)(DateTime.Now.AddDays(60) - DateTime.Now).TotalSeconds,
                RefreshTokenUsage = TokenUsage.ReUse
            },
            new Client
            {
                ClientName = "BasicClient",
                ClientId = "BasicClientId",
                ClientSecrets = { new Secret("basic_secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                RedirectUris = { "https://myapp.com/callback" },
                AllowedScopes = { "basic_scope", "openid", "profile" },
                RequirePkce = true, // Optional: Recommended to use PKCE
                AllowOfflineAccess = true // Optional: Allows the client to use refresh tokens
            }
        };
}
