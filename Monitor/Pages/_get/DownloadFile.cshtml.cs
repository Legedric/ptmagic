using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;
using Core.MarketAnalyzer;

namespace Monitor.Pages {
  public class DownloadFileModel : _Internal.BasePageModelSecure {

    public void OnGet() {
      // Initialize Config
      base.Init();
      
      InitializeDownload();
    }

    private void InitializeDownload() {
      string fileName = GetStringParameter("f", "");
      if (System.IO.File.Exists(PTMagicBasePath + fileName)) {
        if (!System.IO.Directory.Exists(PTMagicMonitorBasePath + "wwwroot" + System.IO.Path.DirectorySeparatorChar + "assets" + System.IO.Path.DirectorySeparatorChar + "tmp" + System.IO.Path.DirectorySeparatorChar)) {
          System.IO.Directory.CreateDirectory(PTMagicMonitorBasePath + "wwwroot" + System.IO.Path.DirectorySeparatorChar + "assets" + System.IO.Path.DirectorySeparatorChar + "tmp" + System.IO.Path.DirectorySeparatorChar);
        }

        string sourcefilePath = PTMagicBasePath + fileName;
        string destinationFilePath = PTMagicMonitorBasePath + "wwwroot" + System.IO.Path.DirectorySeparatorChar + "assets" + System.IO.Path.DirectorySeparatorChar + "tmp" + System.IO.Path.DirectorySeparatorChar + fileName + ".zip";

        ZIPHelper.CreateZipFile(new ArrayList() { sourcefilePath }, destinationFilePath);

        Response.Redirect(PTMagicConfiguration.GeneralSettings.Monitor.RootUrl + "assets/tmp/" + fileName + ".zip");
      }
    }
  }
}
