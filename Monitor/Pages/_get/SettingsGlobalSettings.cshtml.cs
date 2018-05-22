using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;
using Core.MarketAnalyzer;

namespace Monitor.Pages {
  public class SettingsGlobalSettingsModel : _Internal.BasePageModelSecure {
    public GlobalSetting GlobalSetting = null;
    public string SettingName = "";

    public void OnGet() {
      // Initialize Config
      base.Init();
      
      BindData();
    }

    private void BindData() {
      SettingName = this.GetStringParameter("gs", "");
      if (!SettingName.Equals("")) {
        GlobalSetting = PTMagicConfiguration.AnalyzerSettings.GlobalSettings.Find(gs => SystemHelper.StripBadCode(gs.SettingName, Constants.WhiteListNames).Equals(SettingName));
      } else {
        GlobalSetting = new GlobalSetting();
        GlobalSetting.SettingName = "New Setting";
      }
    }

    public string GetMarketTrendSelection(Trigger t) {
      string result = "";

      foreach (MarketTrend mt in PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.MarketTrends) {
        string selected = "";
        if (t != null) {
          if (t.MarketTrendName.Equals(mt.Name, StringComparison.InvariantCultureIgnoreCase)) {
            selected = " selected=\"selected\"";
          }
        }

        result += "<option" + selected + " value=\"" + SystemHelper.StripBadCode(mt.Name, Constants.WhiteListNames) + "\">" + mt.Name + "</option>";
      }

      return result;
    }

    public string GetValueModes(string propertyKey) {
      string result = "";

      string selected = "";
      if (propertyKey.IndexOf("_OFFSET", StringComparison.InvariantCultureIgnoreCase) == -1) {
        selected = " selected=\"selected\"";
      }
      result += "<option" + selected + " value=\"\">Flat value</option>";

      if (propertyKey.EndsWith("_OFFSET", StringComparison.InvariantCultureIgnoreCase)) {
        selected = " selected=\"selected\"";
      } else {
        selected = "";
      }
      result += "<option" + selected + " value=\"_OFFSET\">Offset by flat value</option>";

      if (propertyKey.EndsWith("_OFFSETPERCENT", StringComparison.InvariantCultureIgnoreCase)) {
        selected = " selected=\"selected\"";
      } else {
        selected = "";
      }
      result += "<option" + selected + " value=\"_OFFSETPERCENT\">Offset by percent</option>";

      return result;
    }
  }
}
