using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Main.DataObjects.PTMagicData;
using Core.MarketAnalyzer;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Monitor.Pages {
  public class RestoreBackupForm {
    public string File = "";
  }

  public class RestoreBackupModel : _Internal.BasePageModelSecure {

    public void OnGet() {
      // Initialize Config
      base.Init();

    }

    [HttpPost]
    public ActionResult OnPost() {
      base.Init();

      JsonResult result = new JsonResult("Error restoring backup.");

      MemoryStream stream = new MemoryStream();
      Request.Body.CopyTo(stream);
      stream.Position = 0;
      using (StreamReader reader = new StreamReader(stream)) {
        string requestBody = reader.ReadToEnd();
        if (requestBody.Length > 0) {
          RestoreBackupForm rbf = JsonConvert.DeserializeObject<RestoreBackupForm>(requestBody);
          if (rbf != null) {
            if (System.IO.File.Exists(PTMagicBasePath + rbf.File)) {
              if (System.IO.File.Exists(PTMagicBasePath + rbf.File + ".backup")) {
                try {
                  System.IO.File.Copy(PTMagicBasePath + rbf.File + ".backup", PTMagicBasePath + rbf.File, true);

                  result = new JsonResult("Success");
                } catch { }
              } else {
                result = new JsonResult("Error restoring backup - File '" + rbf.File + ".backup' not found in " + PTMagicBasePath + ".");
              }
            } else {
              result = new JsonResult("Error restoring backup - File '" + rbf.File + "' not found in " + PTMagicBasePath + ".");
            }
          }
        }
      }

      return result;
    }
  }
}
