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
  public static class SettingsFiles {
    public static string GetActiveSetting(PTMagicConfiguration systemConfiguration, string pairsFileName, string dcaFileName, string indicatorsFileName, LogHelper log) {
      string pairsPropertiesPath = systemConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar + pairsFileName;

      string result = SettingsFiles.GetActiveSettingFromFile(pairsPropertiesPath, systemConfiguration, log);

      if (result.Equals("")) {
        SettingsFiles.WriteHeaderLines(pairsPropertiesPath, "Default", systemConfiguration);

        string dcaPropertiesPath = systemConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar + dcaFileName;
        SettingsFiles.WriteHeaderLines(dcaPropertiesPath, "Default", systemConfiguration);

        string inditactorsPropertiesPath = systemConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar + indicatorsFileName;
        SettingsFiles.WriteHeaderLines(inditactorsPropertiesPath, "Default", systemConfiguration);
      }


      return result;
    }

    public static void WriteHeaderLines(string filePath, string settingName, PTMagicConfiguration systemConfiguration) {
      // Writing Header lines
      List<string> lines = File.ReadAllLines(filePath).ToList();
      lines.Insert(0, "");
      lines.Insert(0, "# ####################################");
      lines.Insert(0, "# PTMagic_LastChanged = " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
      lines.Insert(0, "# PTMagic_ActiveSetting = " + SystemHelper.StripBadCode(settingName, Constants.WhiteListProperties));
      lines.Insert(0, "# ####### PTMagic Current Setting ########");
      lines.Insert(0, "# ####################################");

      if (!systemConfiguration.GeneralSettings.Application.TestMode) File.WriteAllLines(filePath, lines);
    }

    public static string GetActiveSettingFromFile(string filePath, PTMagicConfiguration systemConfiguration, LogHelper log) {
      string result = "";

      if (File.Exists(filePath)) {
        StreamReader sr = new StreamReader(filePath);
        try {
          string line = sr.ReadLine();
          while (line != null) {
            if (line.IndexOf("PTMagic_ActiveSetting", StringComparison.InvariantCultureIgnoreCase) > -1) {
              result = line.Replace("PTMagic_ActiveSetting", "", StringComparison.InvariantCultureIgnoreCase);
              result = result.Replace("#", "");
              result = result.Replace("=", "").Trim();
              result = SystemHelper.StripBadCode(result, Constants.WhiteListProperties);
              break;
            }
            line = sr.ReadLine();
          }
        } catch { } finally {
          sr.Close();
        }
      }

      return result;
    }


    public static List<string> GetPresetFileLinesAsList(string settingName, string fileName, PTMagicConfiguration systemConfiguration) {
      return SettingsFiles.GetPresetFileLinesAsList(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar, settingName, fileName, systemConfiguration);
    }

    public static List<string> GetPresetFileLinesAsList(string baseFolderPath, string settingName, string fileName, PTMagicConfiguration systemConfiguration) {
      fileName = fileName.Replace(".PROPERTIES", ".properties");
      string settingPropertiesPath = baseFolderPath + Constants.PTMagicPathPresets + Path.DirectorySeparatorChar + settingName + Path.DirectorySeparatorChar + fileName;

      List<string> result = new List<string>();
      if (File.Exists(settingPropertiesPath)) {
        result = File.ReadAllLines(settingPropertiesPath).ToList();
      }

      return result;
    }

    public static bool CheckPresets(PTMagicConfiguration systemConfiguration, LogHelper log, bool forceCheck) {
      if (!forceCheck) {

        // If the check is not enforced, check for file changes
        string[] presetFilePaths = Directory.GetFiles(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathPresets, "*.*", SearchOption.AllDirectories);
        foreach (string presetFilePath in presetFilePaths) {
          if (presetFilePath.IndexOf(".properties", StringComparison.InvariantCultureIgnoreCase) > -1) {
            FileInfo presetFile = new FileInfo(presetFilePath);
            if (presetFile.LastWriteTime > DateTime.Now.AddMinutes(-systemConfiguration.AnalyzerSettings.MarketAnalyzer.IntervalMinutes).AddSeconds(2)) {

              // File has changed recently, force preparation check
              log.DoLogInfo("Preset files changed, enforcing preparation check...");
              forceCheck = true;
              break;
            }
          }
        }
      }


      if (forceCheck) {
        log.DoLogInfo("Checking automated settings for presets...");
        foreach (GlobalSetting setting in systemConfiguration.AnalyzerSettings.GlobalSettings) {
          if (setting.PairsProperties != null) {
            if (setting.PairsProperties.ContainsKey("File")) {
              setting.PairsProperties["File"] = SystemHelper.PropertyToString(setting.PairsProperties["File"]).Replace(".PROPERTIES", ".properties");

              // Check preset PAIRS.PROPERTIES for header lines
              string settingPairsPropertiesPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathPresets + Path.DirectorySeparatorChar + setting.SettingName + Path.DirectorySeparatorChar + setting.PairsProperties["File"];
              string headerPairsSetting = SettingsFiles.GetActiveSettingFromFile(settingPairsPropertiesPath, systemConfiguration, log);
              if (headerPairsSetting.Equals("")) {
                if (File.Exists(settingPairsPropertiesPath)) {
                  SettingsFiles.WriteHeaderLines(settingPairsPropertiesPath, setting.SettingName, systemConfiguration);
                } else {
                  Exception ex = new Exception("Not able to find preset file " + SystemHelper.PropertyToString(setting.PairsProperties["File"]) + " for '" + setting.SettingName + "'");
                  log.DoLogCritical("Not able to find preset file " + SystemHelper.PropertyToString(setting.PairsProperties["File"]) + " for '" + setting.SettingName + "'", ex);
                  throw ex;
                }

              }

              log.DoLogInfo("Prepared " + SystemHelper.PropertyToString(setting.PairsProperties["File"]) + " for '" + setting.SettingName + "'");
            }
          }

          if (setting.DCAProperties != null) {
            if (setting.DCAProperties.ContainsKey("File")) {
              setting.DCAProperties["File"] = SystemHelper.PropertyToString(setting.DCAProperties["File"]).Replace(".PROPERTIES", ".properties");

              // Check preset DCA.PROPERTIES for header lines
              string settingDCAPropertiesPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathPresets + Path.DirectorySeparatorChar + setting.SettingName + Path.DirectorySeparatorChar + setting.DCAProperties["File"];
              string headerDCASetting = SettingsFiles.GetActiveSettingFromFile(settingDCAPropertiesPath, systemConfiguration, log);
              if (headerDCASetting.Equals("")) {
                if (File.Exists(settingDCAPropertiesPath)) {
                  SettingsFiles.WriteHeaderLines(settingDCAPropertiesPath, setting.SettingName, systemConfiguration);
                } else {
                  Exception ex = new Exception("Not able to find preset file " + SystemHelper.PropertyToString(setting.DCAProperties["File"]) + " for '" + setting.SettingName + "'");
                  log.DoLogCritical("Not able to find preset file " + SystemHelper.PropertyToString(setting.DCAProperties["File"]) + " for '" + setting.SettingName + "'", ex);
                  throw ex;
                }
              }

              log.DoLogInfo("Prepared " + SystemHelper.PropertyToString(setting.DCAProperties["File"]) + " for '" + setting.SettingName + "'");
            }
          }

          if (setting.IndicatorsProperties != null) {
            if (setting.IndicatorsProperties.ContainsKey("File")) {
              setting.IndicatorsProperties["File"] = SystemHelper.PropertyToString(setting.IndicatorsProperties["File"]).Replace(".PROPERTIES", ".properties");

              // Check preset INDICATORS.PROPERTIES for header lines
              string settingIndicatorsPropertiesPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathPresets + Path.DirectorySeparatorChar + setting.SettingName + Path.DirectorySeparatorChar + setting.IndicatorsProperties["File"];
              string headerIndicatorsSetting = SettingsFiles.GetActiveSettingFromFile(settingIndicatorsPropertiesPath, systemConfiguration, log);
              if (headerIndicatorsSetting.Equals("")) {
                if (File.Exists(settingIndicatorsPropertiesPath)) {
                  SettingsFiles.WriteHeaderLines(settingIndicatorsPropertiesPath, setting.SettingName, systemConfiguration);
                } else {
                  Exception ex = new Exception("Not able to find preset file " + SystemHelper.PropertyToString(setting.IndicatorsProperties["File"]) + " for '" + setting.SettingName + "'");
                  log.DoLogCritical("Not able to find preset file " + SystemHelper.PropertyToString(setting.IndicatorsProperties["File"]) + " for '" + setting.SettingName + "'", ex);
                  throw ex;
                }
              }

              log.DoLogInfo("Prepared " + SystemHelper.PropertyToString(setting.IndicatorsProperties["File"]) + " for '" + setting.SettingName + "'");
            }
          }
        }
      }

      return forceCheck;
    }
  }
}