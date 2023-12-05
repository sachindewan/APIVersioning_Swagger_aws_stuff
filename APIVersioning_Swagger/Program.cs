using Amazon.CloudFront;

using Amazon.CloudWatchLogs;
using Amazon.S3;
using Amazon.SecretsManager;
using APIVersioning_Swagger.HealthCheckThings;
using APIVersioning_Swagger.Middleware;
using APIVersioning_Swagger.Services;
using AWS.Logger.SeriLog;
using FileManager;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using QueryEngine.Application;
using Serilog;
using Serilog.Formatting.Compact;
using HealthChecks.UI.Client;
using System.Net;
using Amazon.SimpleSystemsManagement;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Amazon.CloudWatch;

var builder = WebApplication.CreateBuilder(args);
var groupNameVersion = "'v'VVV";

//// Add services to the container.
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = groupNameVersion;
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

// Register the SSM client for dependency injection
//builder.Services.AddAWSService<IAmazonSimpleSystemsManagement>(); //below line depends on it
//builder.Services.AddHealthChecks()
//                        .AddCheck<S3HealthCheck>("s3_health_check")
//                        .AddCheck<ParameterStoreHealthCheck>("parameter_store_health_check");
//builder.Services.AddHealthChecks()
//        .AddCheck<ServiceHealthCheck>("self");
builder.Services.AddHealthChecks();
builder.Services.AddHealthChecksUI(options =>
                {
                    var provider = builder.Services.BuildServiceProvider();

                    var env = provider.GetRequiredService<IWebHostEnvironment>();
                    //connect to HealthChecks UI docker image with https Dns.GetHostName()
                    options.AddHealthCheckEndpoint("Healthcheck API",
                        env.EnvironmentName=="dev" ? "/awshealth" : $"http://{Dns.GetHostName()}/awshealth");
                })
                .AddInMemoryStorage();
//builder.Services.AddAuthentication("Bearer").AddCookie("Bearer", options =>
//{
//    options.ExpireTimeSpan = TimeSpan.FromSeconds(30);

//});
//    .AddJwtBearer("Bearer", options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidIssuer = "https://localhost:7168",
//        //ValidateAudience = true,
//        //ValidAudience = "myapp",
//         ValidateIssuerSigningKey = true,
//        //ValidateLifetime = true,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecretKey")),
//        ValidateAudience = false
//        //SaveSigninToken = true,
//    };
//});


// Add your OData controller here.
//builder.Services.AddOData();
builder.Services.AddControllers()
    .AddOData(opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(null));
builder.Services.AddAuthentication(options => options.DefaultScheme = "XApiKey").AddScheme<APIVersioning_Swagger.Authentication.KeyAuthenticationOptions, APIVersioning_Swagger.Authentication.ApiTokenAuthenticationHandler>("XApiKey", options =>
{
    options.ApiKeyHeaderName = "XApiKey";
    options.RequireHttps = true;
});


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0",
        Description = "Wheather for cast api version 1",
        Title = "Wheather.api",
        TermsOfService = new Uri("https://example.com/Terms"),
        License = new OpenApiLicense
        {
            Name = "Wheather.api license details",
            Url = new Uri("https://example.com/v1.0/License")
        },
        Contact = new OpenApiContact
        {
            Name = "Wheather.api contact details",
            Url = new Uri("https://example.com/v1.0/Contract")
        }
    });
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2.0",
        Description = "Wheather for cast api version 2",
        Title = "Wheather.apis",
        TermsOfService = new Uri("https://example.com/Terms"),
        License = new OpenApiLicense
        {
            Name = "Wheather.api license details",
            Url = new Uri("https://example.com/v2.0/License")
        },
        Contact = new OpenApiContact
        {
            Name = "Wheather.api contact details",
            Url = new Uri("https://example.com/v2.0/Contract")
        }
    });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "ApiKey must appear in header",
        Type = SecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = ParameterLocation.Header,
        //Scheme = "ApiKeyScheme"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
        Type = SecuritySchemeType.ApiKey,
        Name = "Authorization",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    var key = new OpenApiSecurityScheme()
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "ApiKey"
        },
        In = ParameterLocation.Header
    };
    var key2 = new OpenApiSecurityScheme()
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        },
        In = ParameterLocation.Header
    };
    var requirement = new OpenApiSecurityRequirement
                    {
                             { key, new List<string>() },
                             { key2, new List<string>() }
                    };
    options.AddSecurityRequirement(requirement);
});
builder.Services.AddMemoryCache();
builder.Services.AddTransient<IQueryService<QueryService>, QueryService>();
builder.Services.AddTransient<IQueryService<QueryServiceB>, QueryServiceB>();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IFileManagerService,FileManagerService>();
//builder.Services.AddTransient<AmazonS3Client>((sp) => new AmazonS3Client("AKIA3UUSCCNLSGPKMI7Z", "oLeFHjcI50rrR9Cijh/A3rBTbqXim5hacw+AxsyH", Amazon.RegionEndpoint.APSouth1));
//builder.Services.AddTransient<AmazonCloudFrontClient>(sp => new AmazonCloudFrontClient("AKIA3UUSCCNLSGPKMI7Z", "oLeFHjcI50rrR9Cijh/A3rBTbqXim5hacw+AxsyH", Amazon.RegionEndpoint.APSouth1));
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddAWSService<IAmazonCloudFront>();
builder.Services.AddHttpClient<ITokenService,TokenService>();
builder.Services.AddHttpClient();
builder.Services.AddAWSService<IAmazonCloudWatch>();
//builder.Services.AddResponseCaching();

//    .CreateLogger();
//builder.Host.UseSerilog((ctx, lc) =>
//{
//    lc
//        .ReadFrom.Configuration(ctx.Configuration).Enrich.WithCorrelationId()
//        .WriteTo.AWSSeriLog(
//            configuration: ctx.Configuration,
//            textFormatter: new RenderedCompactJsonFormatter()).WriteTo.Console().Enrich.FromLogContext();
//    //new ConfigManager(ctx, lc).ConfigureLogging();
//});
var env = builder.Environment.EnvironmentName;
var appname = builder.Environment.ApplicationName;
//builder.Configuration.AddSecretsManager(configurator: options =>
//{
//    options.SecretFilter = entry=> entry.Name.StartsWith($"{env}_{appName}");
//    options.KeyGenerator = (_, s) => s
//    .Replace($"{env}_{appName}_", string.Empty)
//    .Replace("__", ":");
//    options.PollingInterval = TimeSpan.FromSeconds(10);
//});
builder.Services.AddHttpContextAccessor();


builder.Host.ConfigureAppConfiguration((hostingContext,builder) =>
{
    //builder.AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true);
    //builder.AddJsonFile(path: $"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: false, reloadOnChange: true);
    builder.AddEnvironmentVariables();
    var appType = Environment.GetEnvironmentVariable("ApplicationType"); //hostingContext.Configuration["ApplicationType"];
    var env = hostingContext.Configuration["ASPNETCORE_ENVIRONMENT"];

    //builder.AddSystemsManager(configureSource =>
    //{
    //    Log.Information("Environment is {env}", hostingContext.HostingEnvironment.EnvironmentName);

    //    // Parameter Store prefix to pull configuration data from.
    //    configureSource.Path = $"/ecs/wm-qem-external/{hostingContext.HostingEnvironment.EnvironmentName+ "/apiName"}";

    //    // Reload configuration data every 5 minutes.
    //    configureSource.ReloadAfter = TimeSpan.FromMinutes(5);

    //    // Use custom logic to set AWS credentials and Region. Otherwise, the AWS SDK for .NET's default logic
    //    // will be used find credentials.
    //    //configureSource.AwsOptions = awsOptions;

    //    // Configure if the configuration data is optional.
    //    configureSource.Optional = true;

    //    configureSource.OnLoadException += exceptionContext =>
    //    {
    //        Log.Error("exception occured while fetching the param from store {exception}",exceptionContext);
    //        // Add custom error handling. For example, look at the exceptionContext.Exception and decide
    //        // whether to ignore the error or tell the provider to attempt to reload.

    //    };

    //    // Implement custom parameter process, which transforms Parameter Store names into
    //    // names for the .NET Core configuration system.
    //});

});

builder.Services.AddTransient<CloudWatchExecutionTimeMiddleware>();
builder.Services.AddTransient<CorrelationIdMiddleware>();
builder.Services.AddTransient<RequestLogger>();
builder.Services.AddTransient<ICorrelationIdProvider , HttpContextWrapper>();
builder.Services.AddTransient<ExceptionMiddleWare>();
//builder.Configuration.AddSystemsManager($"/{builder.Environment.EnvironmentName}");
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "WheatherAPIV1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "WheatherAPIV2");
});

// Configure the HTTP request pipeline.
//app.UseResponseCaching();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CloudWatchExecutionTimeMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLogger>();
app.UseMiddleware<ExceptionMiddleWare>();
//app.UseSerilogRequestLogging();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health1", new HealthCheckOptions()
    {
        Predicate = _ => _.Name == "self"
    });
    //endpoints.MapHealthChecks("/awshealth", new HealthCheckOptions()
    //{
    //    Predicate = _ => true,
    //    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    //});
    //endpoints.MapHealthChecksUI(options => options.UIPath = "/dashboard");
    endpoints.MapControllers();
});
app.Run();

static async Task<bool> CheckApiHealth(string apiUrl)
{
    using (HttpClient client = new HttpClient())
    {
        HttpResponseMessage response = await client.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            // Optionally, you can log more details about the failure
            Console.WriteLine($"API returned status code: {response.StatusCode}");
            return false;
        }
    }
}
