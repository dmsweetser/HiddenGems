using HiddenGems.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;

namespace HiddenGems
{
    public class Program
    {
        public static IConfiguration Configuration { get; set; }
        public static IConfiguration InitialConfiguration { get; set; }
        public static void Main(string[] args)
        {
            var existingProcesses = Process.GetProcessesByName("HiddenGems");
            var currentProcess = Process.GetCurrentProcess();
            foreach (var process in existingProcesses)
            {
                if (process.Id != currentProcess.Id) process.Kill();
            }

            int workerThreads, ioCompletionThreads;
            ThreadPool.GetMinThreads(out workerThreads, out ioCompletionThreads);
            ThreadPool.SetMinThreads(10000, 10000);

            var configBuilder = new ConfigurationBuilder();
            configBuilder.Sources.Clear();
            configBuilder.SetBasePath(AppContext.BaseDirectory);
            configBuilder.AddJsonFile("appsettings.json", optional: false);
            Configuration = configBuilder.Build();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(config =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
            })
            .ConfigureLogging(builder =>
                {
                    if (!LicenseManager.IsDemoMode())
                    {
                        builder.AddFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                            + "/Five Loaves Two Fish Software/Hidden Gems/logs/hiddengems-{Date}.txt");
                    }
                })
            .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    if (!LicenseManager.IsDemoMode())
                    {
                        webBuilder.UseUrls(Configuration.GetValue("ListeningUrl", "http://127.0.0.1:9050"));
                    }
                });
    }
}
