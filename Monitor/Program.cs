using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Core.Main;

namespace Monitor {
  public class Program {
    public static void Main(string[] args) {
      Console.WriteLine("##########################################################");
      Console.WriteLine("#********************************************************#");
      Console.WriteLine("INFO: Starting PT Magic Monitor...");
      Console.WriteLine("INFO: Beginning startup checks...");

      string monitorBasePath = Directory.GetCurrentDirectory();
      if (!System.IO.File.Exists(monitorBasePath + Path.DirectorySeparatorChar + "appsettings.json")) {
        monitorBasePath += Path.DirectorySeparatorChar + "Monitor";
      }

      // Startup checks
      string appsettingsJson = monitorBasePath + Path.DirectorySeparatorChar + "appsettings.json";
      if (!File.Exists(appsettingsJson)) {
        Console.WriteLine("ERROR: appsettings.json not found: '" + appsettingsJson + "'. Please check if the file exists. If not, review the PT Magic setup steps listed on the wiki!");
        if (Console.KeyAvailable) Console.ReadKey();
      } else {
        Console.WriteLine("INFO: appsettings.json found in " + monitorBasePath);

        IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(monitorBasePath)
                .AddJsonFile("appsettings.json", false)
                .Build();

        string ptMagicBasePath = config.GetValue<string>("PTMagicBasePath");

        if (!ptMagicBasePath.EndsWith(Path.DirectorySeparatorChar)) {
          ptMagicBasePath += Path.DirectorySeparatorChar;
        }

        // More startup checks
        // Check if PT Magic directoy is correctly configured
        if (!Directory.Exists(ptMagicBasePath)) {
          Console.WriteLine("ERROR: PT Magic directory not found: '" + ptMagicBasePath + "'. Please check your setting for 'PTMagicBasePath' in 'Monitor/appsettings.json'");
          if (Console.KeyAvailable) Console.ReadKey();
        } else {
          Console.WriteLine("INFO: PT Magic directory found at " + ptMagicBasePath);

          // Check if PT Magic settings file exists
          string settingsGeneralJson = ptMagicBasePath + "settings.general.json";
          if (!File.Exists(settingsGeneralJson)) {
            Console.WriteLine("ERROR: PT Magic settings not found: '" + settingsGeneralJson + "'. Please check if you setup PT Magic correctly!");
            if (Console.KeyAvailable) Console.ReadKey();
          } else {
            Console.WriteLine("INFO: settings.general.json found at " + settingsGeneralJson);

            // Check if PT Magic settings file exists
            string lastRuntimeSummaryJson = ptMagicBasePath + Constants.PTMagicPathData + Path.DirectorySeparatorChar + "LastRuntimeSummary.json";
            if (!File.Exists(lastRuntimeSummaryJson)) {
              Console.WriteLine("ERROR: PT Magic runtime summary not found: '" + lastRuntimeSummaryJson + "'. Please wait for PT Magic to complete its first run!");
              if (Console.KeyAvailable) Console.ReadKey();
            } else {
              Console.WriteLine("INFO: LastRuntimeSummary.json found at " + lastRuntimeSummaryJson);

              PTMagicConfiguration ptMagicConfiguration = null;
              try {
                ptMagicConfiguration = new PTMagicConfiguration(ptMagicBasePath);
              } catch (Exception ex) {
                throw ex;
              }

              string wwwrootPath = monitorBasePath + Path.DirectorySeparatorChar + "wwwroot";
              if (!Directory.Exists(wwwrootPath)) {
                Console.WriteLine("ERROR: wwwroot directory not found: '" + wwwrootPath + "'. Did you copy all files as instructed on the wiki?");
                if (Console.KeyAvailable) Console.ReadKey();
              } else {
                Console.WriteLine("INFO: wwwroot directory found at " + wwwrootPath);

                string assetsPath = wwwrootPath + Path.DirectorySeparatorChar + "assets";
                if (!Directory.Exists(assetsPath)) {
                  Console.WriteLine("ERROR: assets directory not found: '" + assetsPath + "'. Did you copy all files as instructed on the wiki?");
                  if (Console.KeyAvailable) Console.ReadKey();
                } else {
                  Console.WriteLine("INFO: assets directory found at " + assetsPath);
                  Console.WriteLine("INFO: ALL CHECKS COMPLETED - ATTEMPTING TO START WEBSERVER...");
                  Console.WriteLine("#********************************************************#");
                  Console.WriteLine("");
                  Console.WriteLine("DO NOT CLOSE THIS WINDOW! THIS IS THE WEBSERVER FOR YOUR MONITOR!");
                  Console.WriteLine("");
                  Console.WriteLine("##########################################################");
                  Console.WriteLine("");

                  BuildWebHost(args, monitorBasePath, monitorBasePath + Path.DirectorySeparatorChar + "wwwroot", ptMagicConfiguration.GeneralSettings.Monitor.Port).Run();
                }
              }

            }
          }
        }
      }
    }

    public static IWebHost BuildWebHost(string[] args, string contentRoot, string webroot, int port) =>
       new WebHostBuilder()
        .UseUrls("http://0.0.0.0:" + port.ToString())
        .UseStartup<Startup>()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseWebRoot(webroot)
        .Build();
  }
}
