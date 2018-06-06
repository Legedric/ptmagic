using System;
using System.Threading;
using System.IO;
using System.Reflection;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;
using Microsoft.Extensions.DependencyInjection;

[assembly: AssemblyVersion("2.0.3")]
[assembly: AssemblyProduct("PT Magic")]

namespace PTMagic {
  class Program {
    static void Main(string[] args) {
      // Init PTMagic
      Core.Main.PTMagic ptMagic = new Core.Main.PTMagic(ServiceHelper.BuildLoggerService().GetRequiredService<LogHelper>());
      ptMagic.CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;

      // Start process
      ptMagic.StartProcess();

      // Keep the app running
      for (; ; ) {
        Thread.Sleep(100);
      }
    }
  }
}
