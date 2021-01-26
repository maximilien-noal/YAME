using Serilog;

namespace Omegadrive.Logging
{
    public static class StaticLogger
    {
        private static readonly ILogger _looger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("log.txt")
            .CreateLogger();

        public static ILogger GetLogger() => _looger;
    }
}