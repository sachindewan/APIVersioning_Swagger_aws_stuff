using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using FileManager;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using QueryEngine.Application;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using APIVersioning_Swagger.Services;
using Amazon.S3.Model;
using Amazon.S3;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using QueryEngine.Application.Exceptions;

namespace APIVersioning_Swagger.Controllers
{
    public class WeatherForecastController : BaseController
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IQueryService<QueryService> queryService;
        private readonly IQueryService<QueryServiceB> queryService1;
        private readonly IConfiguration configuration;
        private readonly IMemoryCache memoryCache;
        private readonly ITokenService tokenService;
        private readonly IAmazonS3 _amazonS3;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,IQueryService<QueryServiceB> queryService1, IQueryService<QueryService> queryService,
            IFileManagerService fileManagerService,IConfiguration configuration, IMemoryCache memoryCache,ITokenService tokenService, IAmazonS3 amazonS3)
        {
            _logger = logger;
            this.queryService = queryService;
            this.queryService1 = queryService1;
            FileManagerService = fileManagerService;
            this.configuration = configuration;
            this.memoryCache = memoryCache;
            this.tokenService = tokenService;
            _amazonS3 = amazonS3;
        }

        public IFileManagerService FileManagerService { get; }

        [HttpPost]
        public async Task<IEnumerable<WeatherForecast>> Get([FromBody] Customer customer)
        {
            //var token = await tokenService.GeTokenAsync();
            _logger.LogInformation("calling query service to check headers using {HttpContext}", nameof(HttpContext));
            var data = queryService.ProcessRequest();
            var data12 = queryService1.ProcessRequest();
            //var exist = FileManagerService.FolderExistAsync().Result;
            var data1= Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
            _logger.LogInformation("calling {FileManagerService} to upload files on s3 bucket", nameof(FileManagerService));
            //var m = await FileManagerService.UploadAsync(data1);
            _logger.LogInformation("generating cloud front url {name}", nameof(FileManagerService));
            
            var url = await FileManagerService.GenerateUrl();
            _logger.LogInformation("executed generated url {hostedservice}", this.configuration["HostedService"]);
            _logger.LogInformation("servername  {servername}", this.configuration["ServerName"]);
            return data1;
        }

        [HttpGet("GetSecrets")]
        public async Task<IActionResult> GetSecrets(string secretId)
        {        
            var parameter = new { Name = "sachin" };
            Data(parameter);
            //var token = HttpContext.GetTokenAsync("access_token");
            // var value = configuration.GetValue<string>("ApiKey");
            //var value1 = configuration.GetValue<string>("dev_APIVersioning_Swagger_ExtenalApiService__apikey");
            //var data  = configuration.GetSection("ExtenalApiService").GetValue<string>("ApiKey");
            try {
                var secretsManagerClient = new AmazonSecretsManagerClient();
                var request = new GetSecretValueRequest
                {
                    SecretId = secretId
                };

                var response1 = await memoryCache.GetOrCreateAsync(request.SecretId, cacheEntry =>
                {
                    cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(10);
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20);
                    return secretsManagerClient.GetSecretValueAsync(request);
            

                });
                return Ok(response1);
            }
            catch(AmazonSecretsManagerException ex)
            {
                if (memoryCache is MemoryCache cache)
                {
                    cache.Clear();
                }
                return BadRequest(ex.Message);
            }

            //var response = await secretsManagerClient.GetSecretValueAsync(request);
        }

        private void Data(dynamic data)
        {
            var v = data;
        }
        [HttpGet("readEnv")]
        public async Task<IActionResult> ReadEnvironment()
        {
            return Ok(configuration["ApplicationType"]);
        }
        [HttpGet("Token")]
        [AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> GetToken()
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("YourSuperSecretKeyYourSuperSecretKeyYourSuperSecretKeyYourSuperSecretKeyYourSuperSecretKeyYourSuperSecretKey");

        //    var tokenDescriptor = new JwtSecurityToken
        //    {
        //        Subject = new ClaimsIdentity(new[]
        //        {
        //    new Claim(ClaimTypes.Name, "username"),
        //}),
        //        Expires = DateTime.UtcNow.AddHours(1),
        //        Issuer = "https://localhost:7168",
        //        Audience="myapp",
        //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        //    };

            var jwtToken = new JwtSecurityToken(issuer: "https://localhost:7168",
                 audience: "myapp",
                 notBefore: DateTime.UtcNow,
                 expires: DateTime.UtcNow.AddMinutes(30),
                 signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
             );
           // var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(tokenHandler.WriteToken(jwtToken));
        }

        [HttpGet("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            // user validation logic
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "admin@mywebsite.com"),
                new Claim("Department","Hr"),
                new Claim("Admin", "true"),
                new Claim("Manager", "true"),
                new Claim("EmploymentDate", "20/12/2021")
            };
            var identity = new ClaimsIdentity(claims, "Bearer");
            var principle = new ClaimsPrincipal(identity);

            var authenticatioProperties = new AuthenticationProperties()
            {

            };

            await HttpContext.SignInAsync("Bearer", principle);
            return Ok();

        }

        [HttpGet("{bucketname}/{fileName}")]
        public async Task<IActionResult> ReadStreamAsync(string bucketname,string fileName)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = bucketname,
                    Key = fileName
                };

                using (var response = await _amazonS3.GetObjectAsync(request))
                using (var responseStream = response.ResponseStream)
                using (var reader = new StreamReader(responseStream))
                {
                    var jsonContent = await reader.ReadToEndAsync();
                    var data= JsonConvert.DeserializeObject<IDictionary<string, dynamic>>(jsonContent);
                    return Ok(jsonContent);
                }
            }
            catch (AmazonS3Exception ex)
            {
                //_logger.LogError("Error encountered while accessing s3 bucket {BucketName} Error: {message}", BucketName, ex.Message);
                throw new Exception();
            }
        }

        [HttpGet("HealthCheck")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok();
        }
    }
   
    public class Customer
    {
        public int Id { get; set; }
        public string  Name { get; set; }
    }
}