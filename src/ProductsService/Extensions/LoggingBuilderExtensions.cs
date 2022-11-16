using Microsoft.Extensions.Logging.Console;

namespace Microsoft.Extensions.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder ConfigureLogging(this ILoggingBuilder loggingBuilder) =>
        loggingBuilder
            .ClearProviders()
            .AddConsole();
}