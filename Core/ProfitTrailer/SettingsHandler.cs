using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;
using Newtonsoft.Json;

namespace Core.ProfitTrailer {
  public static class SettingsHandler {
    public static string GetMainMarket(PTMagicConfiguration systemConfiguration, List<string> pairsLines, LogHelper log) {
      string result = "";

      foreach (string line in pairsLines) {
        if (line.Replace(" ", "").StartsWith("MARKET", StringComparison.InvariantCultureIgnoreCase)) {
          result = line.Replace("MARKET", "", StringComparison.InvariantCultureIgnoreCase);
          result = result.Replace("#", "");
          result = result.Replace("=", "").Trim();
          break;
        }
      }

      return result;
    }

    public static string GetMarketPairs(PTMagicConfiguration systemConfiguration, List<string> pairsLines, LogHelper log) {
      string result = "";

      foreach (string line in pairsLines) {
        if (line.Replace(" ", "").StartsWith("ALL_enabled_pairs", StringComparison.InvariantCultureIgnoreCase) || line.Replace(" ", "").StartsWith("enabled_pairs", StringComparison.InvariantCultureIgnoreCase)) {
          result = line.Replace("ALL_enabled_pairs", "", StringComparison.InvariantCultureIgnoreCase);
          result = result.Replace("enabled_pairs", "", StringComparison.InvariantCultureIgnoreCase);
          result = result.Replace("#", "");
          result = result.Replace("=", "").Trim();
          break;
        }
      }

      return result;
    }

    public static string GetActiveSetting(PTMagicConfiguration systemConfiguration, string defaultSettingName, ref bool headerLinesAdded, List<string> pairsLines, List<string> dcaLines, List<string> indicatorsLines, LogHelper log) {
      string result = "";

      foreach (string line in pairsLines) {
        if (line.IndexOf("PTMagic_ActiveSetting", StringComparison.InvariantCultureIgnoreCase) > -1) {
          result = line.Replace("PTMagic_ActiveSetting", "", StringComparison.InvariantCultureIgnoreCase);
          result = result.Replace("#", "");
          result = result.Replace("=", "").Trim();
          result = SystemHelper.StripBadCode(result, Constants.WhiteListProperties);
          break;
        }
      }

      if (result.Equals("")) {
        SettingsHandler.WriteHeaderLines(ref pairsLines, defaultSettingName, systemConfiguration);
        SettingsHandler.WriteHeaderLines(ref dcaLines, defaultSettingName, systemConfiguration);
        SettingsHandler.WriteHeaderLines(ref indicatorsLines, defaultSettingName, systemConfiguration);
        headerLinesAdded = true;
      }

      return result;
    }

    public static void WriteHeaderLines(ref List<string> lines, string settingName, PTMagicConfiguration systemConfiguration) {
      // Writing Header lines
      lines.Insert(0, "");
      lines.Insert(0, "# ####################################");
      lines.Insert(0, "# PTMagic_LastChanged = " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
      lines.Insert(0, "# PTMagic_ActiveSetting = " + SystemHelper.StripBadCode(settingName, Constants.WhiteListProperties));
      lines.Insert(0, "# ####### PTMagic Current Setting ########");
      lines.Insert(0, "# ####################################");
    }

    public static Dictionary<string, string> GetPropertiesAsDictionary(List<string> propertyLines) {
      Dictionary<string, string> result = new Dictionary<string, string>();

      foreach (string line in propertyLines) {
        if (!line.StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
          string[] lineContentArray = line.Split("=");
          if (lineContentArray.Length == 2) {
            if (!result.ContainsKey(lineContentArray[0].Trim())) {
              result.Add(lineContentArray[0].Trim(), lineContentArray[1].Trim());
            } else {
              result[lineContentArray[0].Trim()] = lineContentArray[1].Trim();
            }
          }
        }
      }

      return result;
    }

    public static string GetCurrentPropertyValue(Dictionary<string, string> properties, string propertyKey, string fallbackPropertyKey) {
      string result = "";

      if (properties.ContainsKey(propertyKey)) {
        result = properties[propertyKey];
      } else if (!fallbackPropertyKey.Equals("") && properties.ContainsKey(fallbackPropertyKey)) {
        result = properties[fallbackPropertyKey];
      }

      return result;
    }

    public static void CompileProperties(string defaultSettingName, GlobalSetting setting, PTMagicConfiguration systemConfiguration, List<string> pairsLines, List<string> dcaLines, List<string> indicatorsLines, LogHelper log) {
      SettingsHandler.BuildPropertyLines(ref pairsLines, defaultSettingName, setting, setting.PairsProperties, "pairs", systemConfiguration, log);
      SettingsHandler.BuildPropertyLines(ref dcaLines, defaultSettingName, setting, setting.DCAProperties, "dca", systemConfiguration, log);
      SettingsHandler.BuildPropertyLines(ref indicatorsLines, defaultSettingName, setting, setting.IndicatorsProperties, "indicators", systemConfiguration, log);
    }

    public static void BuildPropertyLines(ref List<string> lines, string defaultSettingName, GlobalSetting setting, Dictionary<string, object> properties, string propertiesType, PTMagicConfiguration systemConfiguration, LogHelper log) {
      List<string> result = new List<string>();

      if (properties != null) {

        // Building Properties
        if (!setting.SettingName.Equals(defaultSettingName, StringComparison.InvariantCultureIgnoreCase) && systemConfiguration.GeneralSettings.Application.AlwaysLoadDefaultBeforeSwitch && !properties.ContainsKey("File")) {

          // Load default settings as basis for the switch
          GlobalSetting defaultSetting = systemConfiguration.AnalyzerSettings.GlobalSettings.Find(a => a.SettingName.Equals(defaultSettingName, StringComparison.InvariantCultureIgnoreCase));
          if (defaultSetting != null) {

            Dictionary<string, object> defaultProperties = new Dictionary<string, object>();
            switch (propertiesType) {
              case "pairs":
                defaultProperties = defaultSetting.PairsProperties;
                break;
              case "dca":
                defaultProperties = defaultSetting.DCAProperties;
                break;
              case "inidcators":
                defaultProperties = defaultSetting.IndicatorsProperties;
                break;
            }

            if (defaultProperties.ContainsKey("File")) {
              lines = SettingsFiles.GetPresetFileLinesAsList(defaultSetting.SettingName, defaultProperties["File"].ToString(), systemConfiguration);
            }
          }
        } else {

          // Check if settings are configured in a seperate file
          if (properties.ContainsKey("File")) {
            lines = SettingsFiles.GetPresetFileLinesAsList(setting.SettingName, properties["File"].ToString(), systemConfiguration);
          }
        }

        foreach (string line in lines) {
          if (line.IndexOf("PTMagic_ActiveSetting", StringComparison.InvariantCultureIgnoreCase) > -1) {

            // Setting current active setting
            result.Add("# PTMagic_ActiveSetting = " + setting.SettingName);

          } else if (line.IndexOf("PTMagic_LastChanged", StringComparison.InvariantCultureIgnoreCase) > -1) {

            // Setting last change datetime
            result.Add("# PTMagic_LastChanged = " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());

          } else if (line.IndexOf("PTMagic_SingleMarketSettings", StringComparison.InvariantCultureIgnoreCase) > -1) {

            // Single Market Settings will get overwritten every single run => crop the lines
            break;
          } else {

            // Writing property items
            int oldResultCount = result.Count;
            if (properties != null) {
              foreach (string settingProperty in properties.Keys) {
                result = SettingsHandler.BuildPropertyLine(result, setting.SettingName, line, properties, settingProperty);
              }
            }
            if (oldResultCount == result.Count) result.Add(line);
          }
        }
      }

      lines = result;
    }

    public static List<string> BuildPropertyLine(List<string> result, string settingName, string line, Dictionary<string, object> properties, string settingProperty) {
      int valueMode = Constants.ValueModeDefault;
      string propertyKey = settingProperty;

      // Check for offset values
      if (propertyKey.IndexOf("_OFFSETPERCENT") > -1) {
        valueMode = Constants.ValueModeOffsetPercent;
        propertyKey = propertyKey.Replace("_OFFSETPERCENT", "");
      } else if (propertyKey.IndexOf("_OFFSET") > -1) {
        valueMode = Constants.ValueModeOffset;
        propertyKey = propertyKey.Replace("_OFFSET", "");
      }

      if (line.StartsWith(propertyKey + " ", StringComparison.InvariantCultureIgnoreCase) || line.StartsWith(propertyKey + "=", StringComparison.InvariantCultureIgnoreCase)) {
        string newValueString = SystemHelper.PropertyToString(properties[settingProperty]);
        if (newValueString.ToLower().Equals("true") || newValueString.ToLower().Equals("false")) {
          newValueString = newValueString.ToLower();
        }

        string oldValueString = line.Replace(propertyKey, "").Replace("=", "").Trim();

        switch (valueMode) {
          case Constants.ValueModeOffset:
            // Offset value by a fixed amount
            double offsetValue = SystemHelper.TextToDouble(newValueString, 0, "en-US");
            if (offsetValue != 0) {
              double oldValue = SystemHelper.TextToDouble(oldValueString, 0, "en-US");
              newValueString = Math.Round((oldValue + offsetValue), 8).ToString(new System.Globalization.CultureInfo("en-US"));
            }
            break;
          case Constants.ValueModeOffsetPercent:
            // Offset value by percentage
            double offsetValuePercent = SystemHelper.TextToDouble(newValueString, 0, "en-US");
            if (offsetValuePercent != 0) {
              double oldValue = SystemHelper.TextToDouble(oldValueString, 0, "en-US");
              if (oldValue < 0) offsetValuePercent = offsetValuePercent * -1;
              double oldValueOffset = (oldValue * (offsetValuePercent / 100));
              newValueString = Math.Round((oldValue + oldValueOffset), 8).ToString(new System.Globalization.CultureInfo("en-US"));
            }
            break;
          default:
            break;
        }

        line = propertyKey + " = " + newValueString;

        string previousLine = result.Last();
        if (previousLine.IndexOf("PTMagic Changed Line", StringComparison.InvariantCultureIgnoreCase) > -1) {
          previousLine = "# PTMagic changed line for setting '" + settingName + "' on " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();

          result.RemoveAt(result.Count - 1);
          result.Add(previousLine);
        } else {
          string editLine = "# PTMagic changed line for setting '" + settingName + "' on " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
          result.Add(editLine);
        }
        result.Add(line);
      }

      return result;
    }

    public static void CompileSingleMarketProperties(PTMagic ptmagicInstance, Dictionary<string, List<string>> matchedTriggers) {
      try {
        List<string> globalPairsLines = new List<string>();
        List<string> globalDCALines = new List<string>();
        List<string> globalIndicatorsLines = new List<string>();

        List<string> newPairsLines = new List<string>();
        List<string> newDCALines = new List<string>();
        List<string> newIndicatorsLines = new List<string>();

        foreach (string pairsLine in ptmagicInstance.PairsLines) {
          if (pairsLine.IndexOf("PTMagic_SingleMarketSettings", StringComparison.InvariantCultureIgnoreCase) > -1) {

            // Single Market Settings will get overwritten every single run => crop the lines
            break;
          } else {
            string globalPairsLine = pairsLine;

            globalPairsLines.Add(globalPairsLine);
          }
        }

        newPairsLines.Add("# PTMagic_SingleMarketSettings - Written on " + DateTime.Now.ToString());
        newPairsLines.Add("# ########################################################################");
        newPairsLines.Add("");

        foreach (string dcaLine in ptmagicInstance.DCALines) {
          if (dcaLine.IndexOf("PTMagic_SingleMarketSettings", StringComparison.InvariantCultureIgnoreCase) > -1) {

            // Single Market Settings will get overwritten every single run => crop the lines
            break;
          } else {
            string globalDCALine = dcaLine;

            globalDCALines.Add(globalDCALine);
          }
        }

        newDCALines.Add("# PTMagic_SingleMarketSettings - Written on " + DateTime.Now.ToString());
        newDCALines.Add("# ########################################################################");
        newDCALines.Add("");

        foreach (string indicatorsLine in ptmagicInstance.IndicatorsLines) {
          if (indicatorsLine.IndexOf("PTMagic_SingleMarketSettings", StringComparison.InvariantCultureIgnoreCase) > -1) {

            // Single Market Settings will get overwritten every single run => crop the lines
            break;
          } else {
            string globalIndicatorsLine = indicatorsLine;

            globalIndicatorsLines.Add(globalIndicatorsLine);
          }
        }

        Dictionary<string, string> globalPairsProperties = SettingsHandler.GetPropertiesAsDictionary(globalPairsLines);
        Dictionary<string, string> globalDCAProperties = SettingsHandler.GetPropertiesAsDictionary(globalDCALines);
        Dictionary<string, string> globalIndicatorsProperties = SettingsHandler.GetPropertiesAsDictionary(globalIndicatorsLines);

        newIndicatorsLines.Add("# PTMagic_SingleMarketSettings - Written on " + DateTime.Now.ToString());
        newIndicatorsLines.Add("# ########################################################################");
        newIndicatorsLines.Add("");

        foreach (string marketPair in ptmagicInstance.TriggeredSingleMarketSettings.Keys.OrderBy(k => k)) {
          Dictionary<string, object> pairsPropertiesToApply = new Dictionary<string, object>();
          Dictionary<string, object> dcaPropertiesToApply = new Dictionary<string, object>();
          Dictionary<string, object> indicatorsPropertiesToApply = new Dictionary<string, object>();

          // Build Properties as a whole list so that a single coin also has only one block with single market settings applied to it
          foreach (SingleMarketSetting setting in ptmagicInstance.TriggeredSingleMarketSettings[marketPair]) {
            ptmagicInstance.Log.DoLogInfo("Building single market settings '" + setting.SettingName + "' for '" + marketPair + "'...");

            foreach (string settingPairsProperty in setting.PairsProperties.Keys) {
              if (!pairsPropertiesToApply.ContainsKey(settingPairsProperty)) {
                pairsPropertiesToApply.Add(settingPairsProperty, setting.PairsProperties[settingPairsProperty]);
              } else {
                pairsPropertiesToApply[settingPairsProperty] = setting.PairsProperties[settingPairsProperty];
              }
            }

            foreach (string settingDCAProperty in setting.DCAProperties.Keys) {
              if (!dcaPropertiesToApply.ContainsKey(settingDCAProperty)) {
                dcaPropertiesToApply.Add(settingDCAProperty, setting.DCAProperties[settingDCAProperty]);
              } else {
                dcaPropertiesToApply[settingDCAProperty] = setting.DCAProperties[settingDCAProperty];
              }
            }

            foreach (string settingIndicatorsProperty in setting.IndicatorsProperties.Keys) {
              if (!indicatorsPropertiesToApply.ContainsKey(settingIndicatorsProperty)) {
                indicatorsPropertiesToApply.Add(settingIndicatorsProperty, setting.IndicatorsProperties[settingIndicatorsProperty]);
              } else {
                indicatorsPropertiesToApply[settingIndicatorsProperty] = setting.IndicatorsProperties[settingIndicatorsProperty];
              }
            }

            ptmagicInstance.Log.DoLogInfo("Built single market settings '" + setting.SettingName + "' for '" + marketPair + "'.");
          }

          newPairsLines = SettingsHandler.BuildPropertyLinesForSingleMarketSetting(ptmagicInstance.ProfitTrailerMajorVersion, ptmagicInstance.LastRuntimeSummary.MainMarket, marketPair, ptmagicInstance.TriggeredSingleMarketSettings[marketPair], pairsPropertiesToApply, matchedTriggers, globalPairsProperties, newPairsLines, ptmagicInstance.PTMagicConfiguration, ptmagicInstance.Log);
          newDCALines = SettingsHandler.BuildPropertyLinesForSingleMarketSetting(ptmagicInstance.ProfitTrailerMajorVersion, ptmagicInstance.LastRuntimeSummary.MainMarket, marketPair, ptmagicInstance.TriggeredSingleMarketSettings[marketPair], dcaPropertiesToApply, matchedTriggers, globalDCAProperties, newDCALines, ptmagicInstance.PTMagicConfiguration, ptmagicInstance.Log);
          newIndicatorsLines = SettingsHandler.BuildPropertyLinesForSingleMarketSetting(ptmagicInstance.ProfitTrailerMajorVersion, ptmagicInstance.LastRuntimeSummary.MainMarket, marketPair, ptmagicInstance.TriggeredSingleMarketSettings[marketPair], indicatorsPropertiesToApply, matchedTriggers, globalIndicatorsProperties, newIndicatorsLines, ptmagicInstance.PTMagicConfiguration, ptmagicInstance.Log);
        }

        // Combine global settings lines with single market settings lines
        globalPairsLines.AddRange(newPairsLines);
        globalDCALines.AddRange(newDCALines);
        globalIndicatorsLines.AddRange(newIndicatorsLines);

        ptmagicInstance.PairsLines = globalPairsLines;
        ptmagicInstance.DCALines = globalDCALines;
        ptmagicInstance.IndicatorsLines = globalIndicatorsLines;
      } catch (Exception ex) {
        ptmagicInstance.Log.DoLogCritical("Critical error while writing settings!", ex);
        throw (ex);
      }
    }

    public static List<string> BuildPropertyLinesForSingleMarketSetting(int ptMajorVersion, string mainMarket, string marketPair, List<SingleMarketSetting> appliedSettings, Dictionary<string, object> properties, Dictionary<string, List<string>> matchedTriggers, Dictionary<string, string> fullProperties, List<string> newPropertyLines, PTMagicConfiguration systemConfiguration, LogHelper log) {
      if (properties.Keys.Count > 0) {
        string appliedSettingsStringList = "";
        foreach (SingleMarketSetting sms in appliedSettings) {
          if (!appliedSettingsStringList.Equals("")) appliedSettingsStringList += ", ";
          appliedSettingsStringList += sms.SettingName;
        }

        newPropertyLines.Add("# " + marketPair + " - Current active settings: " + appliedSettingsStringList);
        newPropertyLines.Add("# Matching triggers:");
        foreach (string matchingTrigger in matchedTriggers[marketPair]) {
          newPropertyLines.Add("# " + matchingTrigger);
        }

        foreach (string settingProperty in properties.Keys) {
          int valueMode = Constants.ValueModeDefault;
          string propertyKey = settingProperty;

          // Check for offset values
          if (propertyKey.IndexOf("_OFFSETPERCENT") > -1) {
            valueMode = Constants.ValueModeOffsetPercent;
            propertyKey = propertyKey.Replace("_OFFSETPERCENT", "");
          } else if (propertyKey.IndexOf("_OFFSET") > -1) {
            valueMode = Constants.ValueModeOffset;
            propertyKey = propertyKey.Replace("_OFFSET", "");
          }

          string newValueString = SystemHelper.PropertyToString(properties[settingProperty]);
          if (newValueString.ToLower().Equals("true") || newValueString.ToLower().Equals("false")) {
            newValueString = newValueString.ToLower();
          }

          string propertyMarketName = marketPair;
          if (ptMajorVersion > 1) {
            // Adjust market pair name for PT 2.0 and above
            propertyMarketName = propertyMarketName.Replace(mainMarket, "").Replace("_", "").Replace("-", "");
          }

          string propertyKeyString = "";
          if (propertyKey.StartsWith("ALL", StringComparison.InvariantCultureIgnoreCase)) {
            propertyKeyString = propertyKey.Replace("ALL", propertyMarketName, StringComparison.InvariantCultureIgnoreCase);
          } else if (propertyKey.StartsWith("DEFAULT", StringComparison.InvariantCultureIgnoreCase)) {
            propertyKeyString = propertyKey.Replace("DEFAULT", propertyMarketName, StringComparison.InvariantCultureIgnoreCase);
          } else {
            if (propertyKey.StartsWith("_", StringComparison.InvariantCultureIgnoreCase)) {
              propertyKeyString = propertyMarketName + propertyKey;
            } else {
              propertyKeyString = propertyMarketName + "_" + propertyKey;
            }
          }

          switch (valueMode) {
            case Constants.ValueModeOffset:
              // Offset value by a fixed amount
              double offsetValue = SystemHelper.TextToDouble(newValueString, 0, "en-US");
              if (offsetValue != 0) {
                double oldValue = SystemHelper.TextToDouble(SettingsHandler.GetCurrentPropertyValue(fullProperties, propertyKey, propertyKey.Replace("ALL_", "DEFAULT_")), 0, "en-US");
                newValueString = Math.Round((oldValue + offsetValue), 8).ToString(new System.Globalization.CultureInfo("en-US"));
              }
              break;
            case Constants.ValueModeOffsetPercent:
              // Offset value by percentage
              double offsetValuePercent = SystemHelper.TextToDouble(newValueString, 0, "en-US");
              if (offsetValuePercent != 0) {
                double oldValue = SystemHelper.TextToDouble(SettingsHandler.GetCurrentPropertyValue(fullProperties, propertyKey, propertyKey.Replace("ALL_", "DEFAULT_")), 0, "en-US");
                if (oldValue < 0) offsetValuePercent = offsetValuePercent * -1;
                double oldValueOffset = (oldValue * (offsetValuePercent / 100));
                newValueString = Math.Round((oldValue + oldValueOffset), 8).ToString(new System.Globalization.CultureInfo("en-US"));
              }
              break;
            default:
              break;
          }

          newPropertyLines.Add(propertyKeyString + " = " + newValueString);
        }
        newPropertyLines.Add("");
      }

      return newPropertyLines;
    }

    public static bool RemoveSingleMarketSettings(PTMagic ptmagicInstance) {
      bool result = false;
      try {
        List<string> cleanedUpPairsLines = new List<string>();
        List<string> cleanedUpDCALines = new List<string>();
        List<string> cleanedUpIndicatorsLines = new List<string>();

        bool removedPairsSingleMarketSettings = false;
        foreach (string pairsLine in ptmagicInstance.PairsLines) {
          if (pairsLine.IndexOf("PTMagic_SingleMarketSettings", StringComparison.InvariantCultureIgnoreCase) > -1) {

            // Single Market Settings will get overwritten every single run => crop the lines
            removedPairsSingleMarketSettings = true;
            break;
          } else {
            string newPairsLine = pairsLine;

            cleanedUpPairsLines.Add(newPairsLine);
          }
        }

        bool removedDCASingleMarketSettings = false;
        foreach (string dcaLine in ptmagicInstance.DCALines) {
          if (dcaLine.IndexOf("PTMagic_SingleMarketSettings", StringComparison.InvariantCultureIgnoreCase) > -1) {

            // Single Market Settings will get overwritten every single run => crop the lines
            removedDCASingleMarketSettings = true;
            break;
          } else {
            string newDCALine = dcaLine;

            cleanedUpDCALines.Add(newDCALine);
          }
        }

        bool removedIndicatorsSingleMarketSettings = false;
        foreach (string indicatorsLine in ptmagicInstance.IndicatorsLines) {
          if (indicatorsLine.IndexOf("PTMagic_SingleMarketSettings", StringComparison.InvariantCultureIgnoreCase) > -1) {

            // Single Market Settings will get overwritten every single run => crop the lines
            removedIndicatorsSingleMarketSettings = true;
            break;
          } else {
            string newIndicatorsLine = indicatorsLine;

            cleanedUpIndicatorsLines.Add(newIndicatorsLine);
          }
        }

        ptmagicInstance.PairsLines = cleanedUpPairsLines;
        ptmagicInstance.DCALines = cleanedUpDCALines;
        ptmagicInstance.IndicatorsLines = cleanedUpIndicatorsLines;

        result = removedPairsSingleMarketSettings && removedDCASingleMarketSettings && removedIndicatorsSingleMarketSettings;
      } catch (Exception ex) {
        ptmagicInstance.Log.DoLogCritical("Critical error while writing settings!", ex);
      }

      return result;
    }

    
  }
}
