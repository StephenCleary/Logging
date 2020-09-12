using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Nito.Logging.ExceptionContext;
using Nito.Logging.ExceptionContext.Internals;

namespace WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .CaptureLoggingContextForExceptions()
                .ConfigureServices((hostContext, services) =>
                {
                    var existing = services.FirstOrDefault(x => x.ImplementationType == typeof(ConsoleLoggerProvider));
                    services.Remove(existing);
                    services.AddSingleton<ConsoleLoggerProvider>();
                    services.AddSingleton<ILoggerProvider>(provider => new ScopeApplyingLoggingProviderWrapper(provider.GetService<ConsoleLoggerProvider>()));
                    services.AddHostedService<Worker>();
                });
    }
}
