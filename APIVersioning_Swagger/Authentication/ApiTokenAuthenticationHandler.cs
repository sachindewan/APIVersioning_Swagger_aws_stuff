using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace APIVersioning_Swagger.Authentication
{
    public class ApiTokenAuthenticationHandler : AuthenticationHandler<KeyAuthenticationOptions>
    {
        private new readonly IOptionsMonitor<KeyAuthenticationOptions> Options;

        public ApiTokenAuthenticationHandler(IOptionsMonitor<KeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            Options = options;
        }

        public string ApiKeySchemeName { get; set; } = "XApiKey";

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {


            if (!Request.Headers.ContainsKey(HeaderNames.Authorization))
            {
                //Authorization header not in request
                return AuthenticateResult.NoResult();
            }

            if (!AuthenticationHeaderValue.TryParse(Request.Headers[HeaderNames.Authorization], out AuthenticationHeaderValue? headerValue))
            {
                //Invalid Authorization header
                return AuthenticateResult.NoResult();
            }

            if (!ApiKeySchemeName.Equals(headerValue.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                //Not ApiKey authentication header
                return AuthenticateResult.NoResult();
            }
            if (headerValue.Parameter is null)
            {
                //Missing key
                return AuthenticateResult.Fail("Missing apiKey");
            }

            //bool isValid =  IsValidApiToken(headerValue.Parameter);

            //if (!isValid)
            //{
            //    return AuthenticateResult.Fail("Invalid apiKey");
            //}
            var claims = new[] { new Claim(ClaimTypes.Name, "Service") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
            //if (!Request.Headers.TryGetValue(Options.CurrentValue.ApiKeyHeaderName, out var apiToken))
            //{
            //    return AuthenticateResult.NoResult();
            //}

            //// TODO: Implement your custom API token validation logic
            //// Example: Verify the API token against your database or any other storage

            //if (!IsValidApiToken(apiToken))
            //{
            //    return AuthenticateResult.Fail("Invalid API token");
            //}

            //// TODO: Create the identity and principal based on your user information
            //// Example: Retrieve user information from the database or any other source

            //var claims = new[] { new Claim(ClaimTypes.Name, "Dummy") };
            //var identity = new ClaimsIdentity(claims, Scheme.Name);
            //var principal = new ClaimsPrincipal(identity);
            //var ticket = new AuthenticationTicket(principal, Scheme.Name);

            //return AuthenticateResult.Success(ticket);
        }

        private bool IsValidApiToken(StringValues apiToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
