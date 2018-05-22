using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;
using Microsoft.Extensions.Primitives;

namespace Monitor.Pages {
  public class SettingsAnalyzerModel : _Internal.BasePageModelSecure {
    public string ValidationMessage = "";

    public void OnGet() {
      base.Init();

      string notification = GetStringParameter("n", "");
      if (notification.Equals("BackupRestored")) {
        NotifyHeadline = "Backup restored!";
        NotifyMessage = "Your backup of settings.analyzer.json was successfully restored.";
        NotifyType = "success";
      }
    }

    public void OnPost() {
      base.Init();

      PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.StoreDataMaxHours = SystemHelper.TextToInteger(HttpContext.Request.Form["MarketAnalyzer_StoreDataMaxHours"], PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.StoreDataMaxHours);
      PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.IntervalMinutes = SystemHelper.TextToInteger(HttpContext.Request.Form["MarketAnalyzer_IntervalMinutes"], PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.IntervalMinutes);
      PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.ExcludeMainCurrency = HttpContext.Request.Form["MarketAnalyzer_ExcludeMainCurrency"].Equals("on");

      List<string> formKeys = HttpContext.Request.Form.Keys.ToList();

      SaveMarketTrends(formKeys);
      SaveGlobalSettings(formKeys);
      SaveSingleMarketSettings(formKeys);

      PTMagicConfiguration.WriteAnalyzerSettings(PTMagicBasePath);

      NotifyHeadline = "Settings saved!";
      NotifyMessage = "Settings saved successfully to settings.analyzer.json.";
      NotifyType = "success";
    }

    private void SaveMarketTrends(List<string> formKeys) {
      List<MarketTrend> newMarketTrends = new List<MarketTrend>();
      List<string> marketTrendFormKeys = formKeys.FindAll(k => k.StartsWith("MarketAnalyzer_MarketTrend_") && k.EndsWith("|Name"));
      foreach (string marketTrendFormKey in marketTrendFormKeys) {
        MarketTrend mt = null;

        string originalNameSimplified = marketTrendFormKey.Replace("MarketAnalyzer_MarketTrend_", "").Replace("|Name", "");
        string mtFormKey = "MarketAnalyzer_MarketTrend_" + originalNameSimplified + "|";

        if (originalNameSimplified.Equals("")) {
          mt = new MarketTrend();
        } else {
          mt = PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.MarketTrends.Find(m => SystemHelper.StripBadCode(m.Name, Constants.WhiteListNames).Equals(originalNameSimplified));
        }

        mt.Name = HttpContext.Request.Form[marketTrendFormKey];
        mt.Platform = HttpContext.Request.Form[mtFormKey + "Platform"];
        mt.MaxMarkets = SystemHelper.TextToInteger(HttpContext.Request.Form[mtFormKey + "MaxMarkets"], mt.MaxMarkets);
        mt.TrendMinutes = SystemHelper.TextToInteger(HttpContext.Request.Form[mtFormKey + "TrendMinutes"], mt.TrendMinutes);
        mt.TrendCurrency = HttpContext.Request.Form[mtFormKey + "TrendCurrency"];
        mt.IgnoredMarkets = HttpContext.Request.Form[mtFormKey + "IgnoredMarkets"];
        mt.AllowedMarkets = HttpContext.Request.Form[mtFormKey + "AllowedMarkets"];
        mt.DisplayGraph = HttpContext.Request.Form[mtFormKey + "DisplayGraph"].Equals("on");
        mt.ExcludeMainCurrency = HttpContext.Request.Form[mtFormKey + "ExcludeMainCurrency"].Equals("on");

        newMarketTrends.Add(mt);
      }
      PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.MarketTrends = newMarketTrends;
    }

    private void SaveGlobalSettings(List<string> formKeys) {
      List<GlobalSetting> newGlobalMarketSettings = new List<GlobalSetting>();
      List<string> globalSettingFormKeys = formKeys.FindAll(k => k.StartsWith("MarketAnalyzer_GlobalSetting_") && k.EndsWith("|SettingName"));
      foreach (string globalSettingFormKey in globalSettingFormKeys) {
        GlobalSetting gs = null;

        string originalNameSimplified = globalSettingFormKey.Replace("MarketAnalyzer_GlobalSetting_", "").Replace("|SettingName", "");
        string gsFormKey = "MarketAnalyzer_GlobalSetting_" + originalNameSimplified + "|";

        if (originalNameSimplified.Equals("")) {
          gs = new GlobalSetting();
        } else {
          gs = PTMagicConfiguration.AnalyzerSettings.GlobalSettings.Find(s => SystemHelper.StripBadCode(s.SettingName, Constants.WhiteListNames).Equals(originalNameSimplified));
        }

        gs.SettingName = SystemHelper.StripBadCode(HttpContext.Request.Form[gsFormKey + "SettingName"], Constants.WhiteListNames);
        gs.TriggerConnection = HttpContext.Request.Form[gsFormKey + "TriggerConnection"];

        // Triggers
        if (!gs.SettingName.Equals("Default", StringComparison.InvariantCultureIgnoreCase)) {
          List<Trigger> newTriggers = new List<Trigger>();
          List<string> globalSettingTriggerFormKeys = formKeys.FindAll(k => k.StartsWith(gsFormKey + "Trigger_") && k.EndsWith("|MarketTrendName"));
          foreach (string globalSettingTriggerFormKey in globalSettingTriggerFormKeys) {
            Trigger trigger = null;

            string originalTriggerNameSimplified = globalSettingTriggerFormKey.Replace(gsFormKey + "Trigger_", "").Replace("|MarketTrendName", "");
            string tFormKey = gsFormKey + "Trigger_" + originalTriggerNameSimplified + "|";

            for (int f = 0; f < HttpContext.Request.Form[tFormKey + "MarketTrendName"].Count; f++) {

              if (originalTriggerNameSimplified.Equals("")) {
                trigger = new Trigger();
              } else {
                trigger = gs.Triggers.Find(t => SystemHelper.StripBadCode(t.MarketTrendName, Constants.WhiteListNames).Equals(originalTriggerNameSimplified));
              }

              trigger.MarketTrendName = HttpContext.Request.Form[tFormKey + "MarketTrendName"][f];
              trigger.MinChange = SystemHelper.TextToDouble(HttpContext.Request.Form[tFormKey + "MinChange"][f], Constants.MinTrendChange, "en-US");
              trigger.MaxChange = SystemHelper.TextToDouble(HttpContext.Request.Form[tFormKey + "MaxChange"][f], Constants.MaxTrendChange, "en-US");

              newTriggers.Add(trigger);
            }
          }
          gs.Triggers = newTriggers;
        }

        // Pairs Properties
        Dictionary<string, object> newPairsProperties = GetProfitTrailerProperties(formKeys, gsFormKey, "Pairs");
        gs.PairsProperties = newPairsProperties;

        // DCA Properties
        Dictionary<string, object> newDCAProperties = GetProfitTrailerProperties(formKeys, gsFormKey, "DCA");
        gs.DCAProperties = newDCAProperties;

        // Indicators Properties
        Dictionary<string, object> newIndicatorsProperties = GetProfitTrailerProperties(formKeys, gsFormKey, "Indicators");
        gs.IndicatorsProperties = newIndicatorsProperties;

        newGlobalMarketSettings.Add(gs);
      }
      PTMagicConfiguration.AnalyzerSettings.GlobalSettings = newGlobalMarketSettings;
    }

    private void SaveSingleMarketSettings(List<string> formKeys) {
      List<SingleMarketSetting> newSingleMarketMarketSettings = new List<SingleMarketSetting>();
      List<string> singleMarketSettingFormKeys = formKeys.FindAll(k => k.StartsWith("MarketAnalyzer_SingleMarketSetting_") && k.EndsWith("|SettingName"));
      foreach (string singleMarketSettingFormKey in singleMarketSettingFormKeys) {
        SingleMarketSetting sms = null;

        string originalNameSimplified = singleMarketSettingFormKey.Replace("MarketAnalyzer_SingleMarketSetting_", "").Replace("|SettingName", "");
        string smsFormKey = "MarketAnalyzer_SingleMarketSetting_" + originalNameSimplified + "|";

        if (originalNameSimplified.Equals("")) {
          sms = new SingleMarketSetting();
        } else {
          sms = PTMagicConfiguration.AnalyzerSettings.SingleMarketSettings.Find(s => SystemHelper.StripBadCode(s.SettingName, Constants.WhiteListNames).Equals(originalNameSimplified));
        }

        sms.SettingName = SystemHelper.StripBadCode(HttpContext.Request.Form[smsFormKey + "SettingName"], Constants.WhiteListNames);
        sms.TriggerConnection = HttpContext.Request.Form[smsFormKey + "TriggerConnection"];
        sms.OffTriggerConnection = HttpContext.Request.Form[smsFormKey + "OffTriggerConnection"];
        sms.IgnoredMarkets = HttpContext.Request.Form[smsFormKey + "IgnoredMarkets"];
        sms.AllowedMarkets = HttpContext.Request.Form[smsFormKey + "AllowedMarkets"];
        sms.StopProcessWhenTriggered = HttpContext.Request.Form[smsFormKey + "StopProcessWhenTriggered"].Equals("on");

        #region Triggers
        List<Trigger> newTriggers = new List<Trigger>();
        List<string> singleMarketSettingTriggerFormKeys = formKeys.FindAll(k => k.StartsWith(smsFormKey + "Trigger_") && k.EndsWith("|MarketTrendName"));
        foreach (string singleMarketSettingTriggerFormKey in singleMarketSettingTriggerFormKeys) {
          Trigger trigger = null;

          string originalTriggerNameSimplified = singleMarketSettingTriggerFormKey.Replace(smsFormKey + "Trigger_", "").Replace("|MarketTrendName", "");
          string tFormKey = smsFormKey + "Trigger_" + originalTriggerNameSimplified + "|";

          for (int f = 0; f < HttpContext.Request.Form[tFormKey + "MarketTrendName"].Count; f++) {
            if (originalTriggerNameSimplified.Equals("")) {
              trigger = new Trigger();
            } else {
              trigger = sms.Triggers.Find(t => SystemHelper.StripBadCode(t.MarketTrendName, Constants.WhiteListNames).Equals(originalTriggerNameSimplified));
            }

            trigger.MarketTrendName = HttpContext.Request.Form[tFormKey + "MarketTrendName"][f];
            trigger.MarketTrendRelation = HttpContext.Request.Form[tFormKey + "MarketTrendRelation"][f];
            trigger.MinChange = SystemHelper.TextToDouble(HttpContext.Request.Form[tFormKey + "MinChange"][f], Constants.MinTrendChange, "en-US");
            trigger.MaxChange = SystemHelper.TextToDouble(HttpContext.Request.Form[tFormKey + "MaxChange"][f], Constants.MaxTrendChange, "en-US");

            newTriggers.Add(trigger);
          }
        }

        List<string> singleMarketSettingCoinAgeTriggerFormKeys = formKeys.FindAll(k => k.StartsWith(smsFormKey + "Trigger_AgeDaysLowerThan"));
        foreach (string singleMarketSettingCoinAgeTriggerFormKey in singleMarketSettingCoinAgeTriggerFormKeys) {
          Trigger trigger = null;

          string originalTriggerIndex = singleMarketSettingCoinAgeTriggerFormKey.Replace(smsFormKey + "Trigger_AgeDaysLowerThan", "");
          string tFormKey = smsFormKey + "Trigger_AgeDaysLowerThan" + originalTriggerIndex;

          for (int f = 0; f < HttpContext.Request.Form[tFormKey].Count; f++) {
            trigger = new Trigger();

            trigger.AgeDaysLowerThan = SystemHelper.TextToInteger(HttpContext.Request.Form[tFormKey][f], 0);

            newTriggers.Add(trigger);
          }
        }

        List<string> singleMarketSetting24hVolumeTriggerFormKeys = formKeys.FindAll(k => k.StartsWith(smsFormKey + "Trigger_24hVolume") && k.EndsWith("|Min24hVolume"));
        foreach (string singleMarketSetting24hVolumeTriggerFormKey in singleMarketSetting24hVolumeTriggerFormKeys) {
          Trigger trigger = null;

          string originalTriggerIndex = singleMarketSetting24hVolumeTriggerFormKey.Replace(smsFormKey + "Trigger_24hVolume", "").Replace("|Min24hVolume", "");
          string tFormKey = smsFormKey + "Trigger_24hVolume" + originalTriggerIndex + "|";

          for (int f = 0; f < HttpContext.Request.Form[tFormKey + "Min24hVolume"].Count; f++) {
            trigger = new Trigger();

            trigger.Min24hVolume = SystemHelper.TextToDouble(HttpContext.Request.Form[tFormKey + "Min24hVolume"][f], 0, "en-US");
            trigger.Max24hVolume = SystemHelper.TextToDouble(HttpContext.Request.Form[tFormKey + "Max24hVolume"][f], Constants.Max24hVolume, "en-US");

            newTriggers.Add(trigger);
          }
        }
        sms.Triggers = newTriggers;

        #endregion

        #region Off Triggers
        List<OffTrigger> newOffTriggers = new List<OffTrigger>();
        List<string> singleMarketSettingOffTriggerFormKeys = formKeys.FindAll(k => k.StartsWith(smsFormKey + "OffTrigger_") && k.EndsWith("|MarketTrendName"));
        foreach (string singleMarketSettingOffTriggerFormKey in singleMarketSettingOffTriggerFormKeys) {
          OffTrigger offTrigger = null;

          string originalOffTriggerNameSimplified = singleMarketSettingOffTriggerFormKey.Replace(smsFormKey + "OffTrigger_", "").Replace("|MarketTrendName", "");
          string tFormKey = smsFormKey + "OffTrigger_" + originalOffTriggerNameSimplified + "|";

          for (int f = 0; f < HttpContext.Request.Form[tFormKey + "MarketTrendName"].Count; f++) {
            if (originalOffTriggerNameSimplified.Equals("")) {
              offTrigger = new OffTrigger();
            } else {
              offTrigger = sms.OffTriggers.Find(t => SystemHelper.StripBadCode(t.MarketTrendName, Constants.WhiteListNames).Equals(originalOffTriggerNameSimplified));
            }

            offTrigger.MarketTrendName = HttpContext.Request.Form[tFormKey + "MarketTrendName"][f];
            offTrigger.MarketTrendRelation = HttpContext.Request.Form[tFormKey + "MarketTrendRelation"][f];
            offTrigger.MinChange = SystemHelper.TextToDouble(HttpContext.Request.Form[tFormKey + "MinChange"][f], Constants.MinTrendChange, "en-US");
            offTrigger.MaxChange = SystemHelper.TextToDouble(HttpContext.Request.Form[tFormKey + "MaxChange"][f], Constants.MaxTrendChange, "en-US");

            newOffTriggers.Add(offTrigger);
          }
        }

        List<string> singleMarketSettingHoursActiveOffTriggerFormKeys = formKeys.FindAll(k => k.StartsWith(smsFormKey + "OffTrigger_HoursSinceTriggered"));
        foreach (string singleMarketSettingHoursActiveOffTriggerFormKey in singleMarketSettingHoursActiveOffTriggerFormKeys) {
          OffTrigger offTrigger = null;

          string originalOffTriggerIndex = singleMarketSettingHoursActiveOffTriggerFormKey.Replace(smsFormKey + "OffTrigger_HoursSinceTriggered", "");
          string tFormKey = smsFormKey + "OffTrigger_HoursSinceTriggered" + originalOffTriggerIndex;

          for (int f = 0; f < HttpContext.Request.Form[tFormKey].Count; f++) {
            offTrigger = new OffTrigger();

            offTrigger.HoursSinceTriggered = SystemHelper.TextToInteger(HttpContext.Request.Form[tFormKey][f], 0);

            newOffTriggers.Add(offTrigger);
          }
        }

        List<string> singleMarketSetting24hVolumeOffTriggerFormKeys = formKeys.FindAll(k => k.StartsWith(smsFormKey + "OffTrigger_24hVolume") && k.EndsWith("|Min24hVolume"));
        foreach (string singleMarketSetting24hVolumeOffTriggerFormKey in singleMarketSetting24hVolumeOffTriggerFormKeys) {
          OffTrigger offTrigger = null;

          string originalOffTriggerIndex = singleMarketSetting24hVolumeOffTriggerFormKey.Replace(smsFormKey + "OffTrigger_24hVolume", "").Replace("|Min24hVolume", "");
          string tFormKey = smsFormKey + "OffTrigger_24hVolume" + originalOffTriggerIndex + "|";

          for (int f = 0; f < HttpContext.Request.Form[tFormKey + "Min24hVolume"].Count; f++) {
            offTrigger = new OffTrigger();

            offTrigger.Min24hVolume = SystemHelper.TextToDouble(HttpContext.Request.Form[tFormKey + "Min24hVolume"][f], 0, "en-US");
            offTrigger.Max24hVolume = SystemHelper.TextToDouble(HttpContext.Request.Form[tFormKey + "Max24hVolume"][f], Constants.Max24hVolume, "en-US");

            newOffTriggers.Add(offTrigger);
          }
        }
        sms.OffTriggers = newOffTriggers;

        #endregion

        // Pairs Properties
        Dictionary<string, object> newPairsProperties = GetProfitTrailerProperties(formKeys, smsFormKey, "Pairs");
        sms.PairsProperties = newPairsProperties;

        // DCA Properties
        Dictionary<string, object> newDCAProperties = GetProfitTrailerProperties(formKeys, smsFormKey, "DCA");
        sms.DCAProperties = newDCAProperties;

        // Indicators Properties
        Dictionary<string, object> newIndicatorsProperties = GetProfitTrailerProperties(formKeys, smsFormKey, "Indicators");
        sms.IndicatorsProperties = newIndicatorsProperties;

        newSingleMarketMarketSettings.Add(sms);
      }
      PTMagicConfiguration.AnalyzerSettings.SingleMarketSettings = newSingleMarketMarketSettings;
    }

    private Dictionary<string, object> GetProfitTrailerProperties(List<string> formKeys, string sFormKey, string propertyType) {
      Dictionary<string, object> result = new Dictionary<string, object>();

      List<string> globalSettingPairsPropertiesFormKeys = formKeys.FindAll(k => k.StartsWith(sFormKey + propertyType + "Property_") && k.IndexOf("|Value") == -1);
      foreach (string globalSettingPairsFormKey in globalSettingPairsPropertiesFormKeys) {
        string originalKeySimplified = globalSettingPairsFormKey.Replace(sFormKey + propertyType + "Property_", "");
        string propertyFormKey = sFormKey + propertyType + "Property_" + originalKeySimplified;

        for (int f = 0; f < HttpContext.Request.Form[propertyFormKey].Count; f++) {
          string propertyKey = HttpContext.Request.Form[propertyFormKey][f] + HttpContext.Request.Form[propertyFormKey + "|ValueMode"][f];
          string propertyValueString = HttpContext.Request.Form[propertyFormKey + "|Value"][f];

          object propertyValue = new object();
          if (propertyValueString.Equals("true", StringComparison.InvariantCultureIgnoreCase) | propertyValueString.Equals("false", StringComparison.InvariantCultureIgnoreCase)) {
            propertyValue = Convert.ToBoolean(propertyValueString);
          } else {
            if (SystemHelper.IsDouble(propertyValueString, "en-US")) {
              propertyValue = SystemHelper.TextToDouble(propertyValueString, 0, "en-US");

              if (((double)propertyValue % 1) == 0) {
                propertyValue = Convert.ToInt32(propertyValue);
              }
            } else {
              propertyValue = propertyValueString;
            }
          }

          result.Add(propertyKey, propertyValue);
        }
      }

      return result;
    }
  }
}
