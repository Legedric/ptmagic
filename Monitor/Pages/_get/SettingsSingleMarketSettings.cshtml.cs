using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;
using Core.MarketAnalyzer;

namespace Monitor.Pages {
  public class SettingsSingleMarketSettingsModel : _Internal.BasePageModelSecure {
    public SingleMarketSetting SingleMarketSetting = null;
    public string SettingName = "";

    public void OnGet() {
      // Initialize Config
      base.Init();
      
      BindData();
    }

    private void BindData() {
      SettingName = this.GetStringParameter("gs", "");
      if (!SettingName.Equals("")) {
        SingleMarketSetting = PTMagicConfiguration.AnalyzerSettings.SingleMarketSettings.Find(sms => SystemHelper.StripBadCode(sms.SettingName, Constants.WhiteListNames).Equals(SettingName));
      } else {
        SingleMarketSetting = new SingleMarketSetting();
        SingleMarketSetting.SettingName = "New Setting";
      }
    }

    public string GetMarketTrendSelection(Trigger t) {
      string result = "";

      foreach (MarketTrend mt in PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.MarketTrends.FindAll(m => m.Platform.Equals("Exchange", StringComparison.InvariantCultureIgnoreCase))) {
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

    public string GetOffTriggerMarketTrendSelection(OffTrigger ot) {
      string result = "";

      foreach (MarketTrend mt in PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.MarketTrends.FindAll(m => m.Platform.Equals("Exchange", StringComparison.InvariantCultureIgnoreCase))) {
        string selected = "";
        if (ot != null) {
          if (ot.MarketTrendName.Equals(mt.Name, StringComparison.InvariantCultureIgnoreCase)) {
            selected = " selected=\"selected\"";
          }
        }

        result += "<option" + selected + " value=\"" + SystemHelper.StripBadCode(mt.Name, Constants.WhiteListNames) + "\">" + mt.Name + "</option>";
      }

      return result;
    }

    public string GetMarketTrendRelationSelection(Trigger t) {
      string result = "";

      string selected = "";
      if (t != null) {
        if (t.MarketTrendRelation.Equals("Relative", StringComparison.InvariantCultureIgnoreCase)) {
          selected = " selected=\"selected\"";
        }
      }
      result += "<option" + selected + ">Relative</option>";

      if (t != null) {
        if (t.MarketTrendRelation.Equals("Absolute", StringComparison.InvariantCultureIgnoreCase)) {
          selected = " selected=\"selected\"";
        } else {
          selected = "";
        }
      }
      result += "<option" + selected + ">Absolute</option>";

      return result;
    }

    public string GetOffTriggerMarketTrendRelationSelection(OffTrigger ot) {
      string result = "";

      string selected = "";
      if (ot != null) {
        if (ot.MarketTrendRelation.Equals("Relative", StringComparison.InvariantCultureIgnoreCase)) {
          selected = " selected=\"selected\"";
        }
      }
      result += "<option" + selected + ">Relative</option>";

      if (ot != null) {
        if (ot.MarketTrendRelation.Equals("RelativeTrigger", StringComparison.InvariantCultureIgnoreCase)) {
          selected = " selected=\"selected\"";
        }
      }
      result += "<option" + selected + " value=\"RelativeTrigger\">Relative to trigger price</option>";

      if (ot != null) {
        if (ot.MarketTrendRelation.Equals("Absolute", StringComparison.InvariantCultureIgnoreCase)) {
          selected = " selected=\"selected\"";
        } else {
          selected = "";
        }
      }
      result += "<option" + selected + ">Absolute</option>";


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
