using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;
using Core.MarketAnalyzer;
using Core.ProfitTrailer;

namespace Monitor.Pages {
  public class PresetFileModel : _Internal.BasePageModelSecureAJAX {
    public string FileName = "";
    public string SettingName = "";
    public string FileContent = "";

    public void OnGet() {
      base.Init();
      
      BindData();
    }

    private void BindData() {
      FileName = GetStringParameter("f", "");
      SettingName = GetStringParameter("gs", "");

      if (!FileName.Equals("") && !SettingName.Equals("")) {
        List<string> presetFileLines = SettingsFiles.GetPresetFileLinesAsList(PTMagicBasePath, SettingName, FileName, PTMagicConfiguration);
        FileContent = SystemHelper.ConvertListToTokenString(presetFileLines, Environment.NewLine, false);
      }
    }
  }
}
