using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;

namespace Monitor.Pages {
  public class StatusSummaryModel : _Internal.BasePageModelSecure {
    public List<string> MarketsWithSingleSettings = new List<string>();
    public string SettingsDistribution24hChartDataJSON = "";
    public string SettingsDistribution3dChartDataJSON = "";
    private Dictionary<string, string> settingsChartColors = new Dictionary<string, string>();

    public void OnGet() {
      base.Init();
      
      BindData();
    }

    private void BindData() {
      BuildMarketsWithSingleSettings();
      BuildChartColors();
      Build24hChartData();
      Build3dChartData();
    }

    private void BuildMarketsWithSingleSettings() {
      // Get markets with active single settings
      foreach (string key in Summary.MarketSummary.Keys) {
        if (Summary.MarketSummary[key].ActiveSingleSettings != null) {
          if (Summary.MarketSummary[key].ActiveSingleSettings.Count > 0) {
            MarketsWithSingleSettings.Add(key);
          }
        }
      }
      MarketsWithSingleSettings.Sort();
    }

    private void BuildChartColors() {
      int settingIndex = 0;
      foreach (GlobalSetting globalSetting in PTMagicConfiguration.AnalyzerSettings.GlobalSettings) {
        string chartColor = "";
        if (settingIndex < Constants.ChartLineColors.Length) {
          chartColor = Constants.ChartLineColors[settingIndex];
        } else {
          chartColor = Constants.ChartLineColors[settingIndex - 20];
        }

        settingsChartColors.Add(globalSetting.SettingName, chartColor);

        settingIndex++;
      }
    }

    private void Build24hChartData() {
      if (Summary.GlobalSettingSummary.Count > 0) {
        DateTime dateTime24hAgo = DateTime.Now.AddHours(-24);
        List<GlobalSettingSummary> gsSummaries24h = Summary.GlobalSettingSummary.FindAll(gss => gss.SwitchDateTime >= dateTime24hAgo);
        IEnumerable<GlobalSettingSummary> gsNames24h = gsSummaries24h.GroupBy(gss => gss.SettingName).Select(group => group.First());

        if (Summary.GlobalSettingSummary.FindAll(gss => gss.SwitchDateTime <= dateTime24hAgo).Count > 0) {
          GlobalSettingSummary gsBefore24h = Summary.GlobalSettingSummary.FindAll(gss => gss.SwitchDateTime <= dateTime24hAgo).OrderByDescending(gss => gss.SwitchDateTime).First();
          if (gsBefore24h != null) {
            DateTime gsSwitchedOffDateTime = gsBefore24h.SwitchDateTime.AddSeconds(gsBefore24h.ActiveSeconds);
            if (gsSwitchedOffDateTime > dateTime24hAgo) {
              gsBefore24h.ActiveSeconds = (int)Math.Floor(gsSwitchedOffDateTime.Subtract(dateTime24hAgo).TotalSeconds);

              gsSummaries24h.Add(gsBefore24h);

              if (gsNames24h.Select(gss => gss.SettingName.Equals(gsBefore24h.SettingName)) == null) {
                gsNames24h.Append(gsBefore24h);
              }
            }
          }
        }

        if (gsNames24h.Count() > 0) {
          SettingsDistribution24hChartDataJSON = "[";
          int gssIndex = 0;
          double totalCoveredSeconds = gsSummaries24h.Sum(gs => gs.ActiveSeconds);
          foreach (GlobalSettingSummary gss in gsNames24h) {
            string lineColor = "";
            if (settingsChartColors.ContainsKey(gss.SettingName)) {
              lineColor = settingsChartColors[gss.SettingName];
            } else {
              if (gssIndex < Constants.ChartLineColors.Length) {
                lineColor = Constants.ChartLineColors[gssIndex];
              } else {
                lineColor = Constants.ChartLineColors[gssIndex - 20];
              }
            }

            if (!SettingsDistribution24hChartDataJSON.Equals("[")) SettingsDistribution24hChartDataJSON += ",";

            double gsActiveSeconds = gsSummaries24h.FindAll(gs => gs.SettingName.Equals(gss.SettingName)).Sum(gs => gs.ActiveSeconds);
            double chartValue = gsActiveSeconds / totalCoveredSeconds * 100;

            SettingsDistribution24hChartDataJSON += "{";
            SettingsDistribution24hChartDataJSON += "label: '" + SystemHelper.SplitCamelCase(gss.SettingName) + "',";
            SettingsDistribution24hChartDataJSON += "color: '" + lineColor + "',";
            SettingsDistribution24hChartDataJSON += "value: " + chartValue.ToString("0.00", new System.Globalization.CultureInfo("en-US")) + "";
            SettingsDistribution24hChartDataJSON += "}";

            gssIndex++;
          }
          SettingsDistribution24hChartDataJSON += "]";
        }
      }
    }

    private void Build3dChartData() {
      if (Summary.GlobalSettingSummary.Count > 0) {
        DateTime dateTime3dAgo = DateTime.Now.AddHours(-72);
        List<GlobalSettingSummary> gsSummaries3d = Summary.GlobalSettingSummary.FindAll(gss => gss.SwitchDateTime >= dateTime3dAgo);
        IEnumerable<GlobalSettingSummary> gsNames3d = gsSummaries3d.GroupBy(gss => gss.SettingName).Select(group => group.First());

        if (Summary.GlobalSettingSummary.FindAll(gss => gss.SwitchDateTime <= dateTime3dAgo).Count > 0) {
          GlobalSettingSummary gsBefore3d = Summary.GlobalSettingSummary.FindAll(gss => gss.SwitchDateTime <= dateTime3dAgo).OrderByDescending(gss => gss.SwitchDateTime).First();
          if (gsBefore3d != null) {
            DateTime gsSwitchedOffDateTime = gsBefore3d.SwitchDateTime.AddSeconds(gsBefore3d.ActiveSeconds);
            if (gsSwitchedOffDateTime > dateTime3dAgo) {
              gsBefore3d.ActiveSeconds = (int)Math.Floor(gsSwitchedOffDateTime.Subtract(dateTime3dAgo).TotalSeconds);

              gsSummaries3d.Add(gsBefore3d);

              if (gsNames3d.Select(gss => gss.SettingName.Equals(gsBefore3d.SettingName)) == null) {
                gsNames3d.Append(gsBefore3d);
              }
            }
          }
        }

        if (gsNames3d.Count() > 0) {
          SettingsDistribution3dChartDataJSON = "[";
          int gssIndex = 0;
          double totalCoveredSeconds = gsSummaries3d.Sum(gs => gs.ActiveSeconds);
          foreach (GlobalSettingSummary gss in gsNames3d) {
            string lineColor = "";
            if (settingsChartColors.ContainsKey(gss.SettingName)) {
              lineColor = settingsChartColors[gss.SettingName];
            } else {
              if (gssIndex < Constants.ChartLineColors.Length) {
                lineColor = Constants.ChartLineColors[gssIndex];
              } else {
                lineColor = Constants.ChartLineColors[gssIndex - 20];
              }
            }

            if (!SettingsDistribution3dChartDataJSON.Equals("[")) SettingsDistribution3dChartDataJSON += ",";

            double gsActiveSeconds = gsSummaries3d.FindAll(gs => gs.SettingName.Equals(gss.SettingName)).Sum(gs => gs.ActiveSeconds);
            double chartValue = gsActiveSeconds / totalCoveredSeconds * 100;

            SettingsDistribution3dChartDataJSON += "{";
            SettingsDistribution3dChartDataJSON += "label: '" + SystemHelper.SplitCamelCase(gss.SettingName) + "',";
            SettingsDistribution3dChartDataJSON += "color: '" + lineColor + "',";
            SettingsDistribution3dChartDataJSON += "value: " + chartValue.ToString("0.00", new System.Globalization.CultureInfo("en-US")) + "";
            SettingsDistribution3dChartDataJSON += "}";

            gssIndex++;
          }
          SettingsDistribution3dChartDataJSON += "]";
        }
      }
    }
  }
}
