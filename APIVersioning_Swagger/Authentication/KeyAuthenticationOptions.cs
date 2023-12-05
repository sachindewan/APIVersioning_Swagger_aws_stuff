using Microsoft.AspNetCore.Authentication;

namespace APIVersioning_Swagger.Authentication
{
    public class KeyAuthenticationOptions: AuthenticationSchemeOptions
    {
        public string ApiKeyHeaderName { get; set; } = string.Empty;
        public bool RequireHttps { get; set; }
    }
}
