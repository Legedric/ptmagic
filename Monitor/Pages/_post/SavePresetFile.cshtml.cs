using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

      Response.StatusCode = (int)HttpStatusCode.InternalServerError;
      JsonResult result = new JsonResult("Error saving preset file.");

      MemoryStream stream = new MemoryStream();
      Request.Body.CopyTo(stream);
      stream.Position = 0;
      using (StreamReader reader = new StreamReader(stream)) {
        string requestBody = reader.ReadToEnd();
        if (requestBody.Length > 0) {
          SavePresetFileForm spff = JsonConvert.DeserializeObject<SavePresetFileForm>(requestBody);
          if (spff != null) {
            spff.FileName = spff.FileName.Replace(".PROPERTIES", ".properties");
            string settingPropertiesPath = PTMagicBasePath + Constants.PTMagicPathPresets + Path.DirectorySeparatorChar + spff.SettingName + Path.DirectorySeparatorChar + spff.FileName;
            if (System.IO.File.Exists(settingPropertiesPath)) {
              try {
                System.IO.File.WriteAllText(settingPropertiesPath, spff.FileContent);
                Response.StatusCode = (int)HttpStatusCode.OK;
                result = new JsonResult("Settings saved to file " + spff.FileName + " in " + PTMagicBasePath + Constants.PTMagicPathPresets + Path.DirectorySeparatorChar + spff.SettingName + Path.DirectorySeparatorChar);
                Log.DoLogDebug("Settings saved to file " + spff.FileName + " in " + PTMagicBasePath + Constants.PTMagicPathPresets + Path.DirectorySeparatorChar + spff.SettingName + Path.DirectorySeparatorChar);
              } catch (Exception ex) {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                result = new JsonResult("Error saving preset file:" + ex.Message);
                Log.DoLogError("Error saving preset file:" + ex.Message);
              }
            } else {
              Response.StatusCode = (int)HttpStatusCode.InternalServerError;
              result = new JsonResult("Error saving preset file: " + settingPropertiesPath + " not found!");
              Log.DoLogError("Error saving preset file: " + settingPropertiesPath + " not found!");
            }
          }
        }
      }

      return result;
    }
  }
}
