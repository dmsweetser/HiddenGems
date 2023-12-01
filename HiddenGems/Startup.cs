using HiddenGems.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace HiddenGems
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddRazorRuntimeCompilation();

            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = long.MaxValue;
            });

            services.AddAuthorization();

            if (LicenseManager.IsDemoMode())
            {
                services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(Configuration.GetValue("DemoModeIdleTimeout", 720));
                    options.Cookie.IsEssential = true;
                    options.Cookie.HttpOnly = true;
                });
            }
            else
            {
                services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(Configuration.GetValue("IdleTimeout", 720));
                    options.Cookie.IsEssential = true;
                    options.Cookie.HttpOnly = true;
                });
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Console.WriteLine("Welcome to Hidden Gems!");
            Console.WriteLine("Your browser should open momentarily.");
            Console.WriteLine("Please do not close this window...");
            Console.WriteLine();
            app.UseSession();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Index");
                app.UseHsts();
            }

            if (LicenseManager.IsDemoMode())
            {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                DefaultContentType = "text/plain",
                FileProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "wwwroot"))
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });

            //Shared settings are used throughout the application as needed
            SharedSettings.PopulateSharedSettings(Configuration);

            var startupUrl = Configuration.GetValue("StartupUrl", "http://127.0.0.1:9050");
            var autoStartBrowser = Configuration.GetValue("AutoStartBrowser", false);

            if (!LicenseManager.IsDemoMode() && autoStartBrowser)
            {
                OpenUrl(startupUrl);
            }
        }

        //Derived from https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start("cmd", $"/C start {url}");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
