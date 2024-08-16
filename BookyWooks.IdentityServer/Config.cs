using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;

namespace BookyWooks.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<ApiResource> ApiResources => new ApiResource[]
    {
        new ApiResource("resource_ocelot",new [] { JwtClaimTypes.Role }) { Scopes = { "ocelotfull_scope" } },
        new ApiResource("resource_order",new [] { JwtClaimTypes.Role }) { Scopes = { "orderfull_scope" } },
        new ApiResource("resource_bookcatalogue",new [] { JwtClaimTypes.Role }) { Scopes = { "bookcataloguefull_scope" } },
        new ApiResource(IdentityServerConstants.LocalApi.ScopeName)
    };
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource() { Name = "roles", DisplayName = "Roles", Description = "User roles", UserClaims = new[] { "role" } }
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("scope1"),
                new ApiScope("scope2"),
                new ApiScope("microser_api_weather"),
                new ApiScope("bookcataloguefull_scope", "Full permission for products"),
                new ApiScope("ocelotfull_scope", "Full permission for Ocelot"),
                new ApiScope("orderfull_scope", "Full permission for Orders")
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // m2m client credentials flow client
                new Client
                {
                    ClientId = "m2m.client",
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                    AllowedScopes = { "scope1", "microser_api_weather", "bookcataloguefull_scope", "ocelotfull_scope", "orderfull_scope" }
                },

                // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "interactive",
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { "https://localhost:44300/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "scope2" }
                },
            };
    }
}
