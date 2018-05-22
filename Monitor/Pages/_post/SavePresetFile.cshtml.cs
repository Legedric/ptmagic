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
  public class SavePresetFileForm {
    public string FileName = "";
    public string SettingName = "";
    public string FileContent = "";
  }

  public class SavePresetFileModel : _Internal.BasePageModelSecure {
    public void OnGet() {
      // Initialize Config
      base.Init();

    }

    [HttpPost]
    public ActionResult OnPost() {
      base.Init();

      JsonResult result = new JsonResult("Error saving preset file.");

      MemoryStream stream = new MemoryStream();
      Request.Body.CopyTo(stream);
      stream.Position = 0;
      using (StreamReader reader = new StreamReader(stream)) {
        string requestBody = reader.ReadToEnd();
        if (requestBody.Length > 0) {
          SavePresetFileForm spff = JsonConvert.DeserializeObject<SavePresetFileForm>(requestBody);
          if (spff != null) {
            string settingPropertiesPath = PTMagicBasePath + Constants.PTMagicPathPresets + Path.DirectorySeparatorChar + spff.SettingName + Path.DirectorySeparatorChar + spff.FileName;
            if (System.IO.File.Exists(settingPropertiesPath)) {
              try {
                System.IO.File.WriteAllText(settingPropertiesPath, spff.FileContent);
              } catch { }
            }
          }
        }
      }

      return result;
    }
  }
}
