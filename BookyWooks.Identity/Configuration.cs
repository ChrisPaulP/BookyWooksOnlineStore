using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using Secret = IdentityServer4.Models.Secret;

namespace BookyWooks.Identity;

public static class Configuration
{
    public static IEnumerable<ApiResource> ApiResources => new ApiResource[]
    {
        new ApiResource("resource_ocelot",new [] { JwtClaimTypes.Role }) { Scopes = { "ocelotfull_scope" } },
        
        new ApiResource("resource_order",new [] { JwtClaimTypes.Role }) { Scopes = { "orderfull_scope" } },
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
            new ApiScope("ocelotfull_scope", "Full permission for ocelot gateway"),
            new ApiScope("scope1"),
            new ApiScope("scope2"),
            new ApiScope("orderfull_scope", "Full permission for order"),
            new ApiScope("bookcataloguefull_scope", "Full permission for book catalogue"),
            new ApiScope(IdentityServerConstants.LocalApi.ScopeName),
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new Client
            {
                ClientName = "Asp.Net Core MVC",
                ClientId = "WebMvcClient",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "ocelotfull_scope", "accountfull_scope", "orderfull_scope", "bookcataloguefull_scope", IdentityServerConstants.LocalApi.ScopeName },
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
                    "ocelotfull_scope",
                    "orderfull_scope",
                    "bookcataloguefull_scope",
                    IdentityServerConstants.StandardScopes.OfflineAccess, // refresh token
                },
                AccessTokenLifetime = 1 * 60 * 60,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                // AbsoluteRefreshTokenLifetime = (int)(DateTime.Now.AddDays(60) - DateTime.Now).TotalSeconds,
                RefreshTokenUsage = TokenUsage.ReUse
            },
        };
}
