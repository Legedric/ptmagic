using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;
using System.Globalization;

namespace Monitor.Pages {
  public class PresetFilesModel : _Internal.BasePageModelSecure {
    public List<GlobalSetting> GlobalSettingsWithPresets = new List<GlobalSetting>();

    public void OnGet() {
      base.Init();

      BindData();
    }

    public void BindData() {
      string notification = GetStringParameter("n", "");
      if (notification.Equals("PresetFileSaved")) {
        NotifyHeadline = "Preset File Saved!";
        NotifyMessage = "The preset file was saved and will be applied during the next interval.";
        NotifyType = "success";
      }

      foreach (GlobalSetting globalSetting in PTMagicConfiguration.AnalyzerSettings.GlobalSettings) {
        if (globalSetting.PairsProperties.ContainsKey("File") || globalSetting.DCAProperties.ContainsKey("File") || globalSetting.IndicatorsProperties.ContainsKey("File")) {
          GlobalSettingsWithPresets.Add(globalSetting);
        }
      }
    }
  }
}
