using AWS.Logger.SeriLog;
using Serilog;
using Serilog.Core;
using Serilog.Formatting.Compact;
 
namespace APIVersioning_Swagger
{
    internal class ConfigManager
    {
        private readonly HostBuilderContext _hostBuilderContext;
        private readonly LoggerConfiguration _loggerConfiguration;

        internal ConfigManager(HostBuilderContext hostBuilderContext,LoggerConfiguration loggerConfiguration)
        {
            _hostBuilderContext = hostBuilderContext;
            _loggerConfiguration = loggerConfiguration;
        }

        internal void ConfigureLogging()
        {
           _loggerConfiguration//.MinimumLevel.Information().MinimumLevel
               // .Override("Microsoft", new LoggingLevelSwitch() { MinimumLevel=Serilog.Events.LogEventLevel.Warning})
               //.MinimumLevel.Override("System", new LoggingLevelSwitch() { MinimumLevel = Serilog.Events.LogEventLevel.Warning })
                //.Enrich.WithCorrelationId()
              .ReadFrom.Configuration(_hostBuilderContext.Configuration).Enrich.WithCorrelationId()
           .WriteTo.AWSSeriLog(
           configuration: _hostBuilderContext.Configuration,
           textFormatter: new RenderedCompactJsonFormatter()).WriteTo.Console().Enrich.FromLogContext();
        }

    }
}
