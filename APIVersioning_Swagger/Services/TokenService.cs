using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;

namespace APIVersioning_Swagger.Services
{
    public class TokenService : ITokenService
    {
        private readonly HttpClient httpClient;

        public TokenService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public async Task<string> GeTokenAsync()
        {
            var issuer = "https://verisk.okta.com/oauth2/aus11pkpumefaOgUq0x8";

            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                issuer + "/.well-known/oauth-authorization-server",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());
            var discoveryDocument = await configurationManager.GetConfigurationAsync(new CancellationToken());
            var signingKeys = discoveryDocument.SigningKeys;
            var response = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = "https://verisk.okta.com/oauth2/aus11pkpumefaOgUq0x8/v1/token",
                ClientId = "0oa11pkmg81B2aucF0x8",
                ClientSecret = "mpQgq8X_9wA7aRyoEimWcdy0xEqsH39MqHN-oAyi8NwsSKQh5mj6AnrISUoQNUhq",
                Scope = "cs:read"
            });
            return response.AccessToken??string.Empty;
        }
    }
}
