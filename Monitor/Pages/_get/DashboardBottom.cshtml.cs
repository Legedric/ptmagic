using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects;
using Core.Main.DataObjects.PTMagicData;
using Core.MarketAnalyzer;

namespace Monitor.Pages {
  public class DashboardBottomModel : _Internal.BasePageModelSecureAJAX {
    public ProfitTrailerData PTData = null;
    public List<MarketTrend> MarketTrends { get; set; } = new List<MarketTrend>();
    public string TrendChartDataJSON = "";
    public string ProfitChartDataJSON = "";
    public string LastGlobalSetting = "Default";
    public DateTimeOffset DateTimeNow = Constants.confMinDate;

    public void OnGet() {
      // Initialize Config
      base.Init();

      BindData();
    }

    private void BindData() {
      PTData = new ProfitTrailerData(PTMagicBasePath, PTMagicConfiguration);

      // Cleanup temp files
      FileHelper.CleanupFilesMinutes(PTMagicMonitorBasePath + "wwwroot" + System.IO.Path.DirectorySeparatorChar + "assets" + System.IO.Path.DirectorySeparatorChar + "tmp" + System.IO.Path.DirectorySeparatorChar, 5);

      // Convert local offset time to UTC
      TimeSpan offsetTimeSpan = TimeSpan.Parse(PTMagicConfiguration.GeneralSettings.Application.TimezoneOffset.Replace("+", ""));
      DateTimeNow = DateTimeOffset.UtcNow.ToOffset(offsetTimeSpan);

      // Get last and current active setting
      if (!String.IsNullOrEmpty(HttpContext.Session.GetString("LastGlobalSetting"))) {
        LastGlobalSetting = HttpContext.Session.GetString("LastGlobalSetting");
      }
      HttpContext.Session.SetString("LastGlobalSetting", Summary.CurrentGlobalSetting.SettingName);

      // Get market trends
      MarketTrends = PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.MarketTrends.OrderBy(mt => mt.TrendMinutes).ThenByDescending(mt => mt.Platform).ToList();

      BuildMarketTrendChartData();
      BuildProfitChartData();
    }

    private void BuildMarketTrendChartData() {
      if (MarketTrends.Count > 0) {
        TrendChartDataJSON = "[";
        int mtIndex = 0;
        foreach (MarketTrend mt in MarketTrends) {
          if (mt.DisplayGraph) {
            string lineColor = "";
            if (mtIndex < Constants.ChartLineColors.Length) {
              lineColor = Constants.ChartLineColors[mtIndex];
            } else {
              lineColor = Constants.ChartLineColors[mtIndex - 20];
            }

            if (Summary.MarketTrendChanges.ContainsKey(mt.Name)) {
              List<MarketTrendChange> marketTrendChangeSummaries = Summary.MarketTrendChanges[mt.Name];

              if (marketTrendChangeSummaries.Count > 0) {
                if (!TrendChartDataJSON.Equals("[")) TrendChartDataJSON += ",";

                TrendChartDataJSON += "{";
                TrendChartDataJSON += "key: '" + SystemHelper.SplitCamelCase(mt.Name) + "',";
                TrendChartDataJSON += "color: '" + lineColor + "',";
                TrendChartDataJSON += "values: [";

                // Get trend ticks for chart
                DateTime currentDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                DateTime startDateTime = currentDateTime.AddHours(-PTMagicConfiguration.GeneralSettings.Monitor.GraphMaxTimeframeHours);
                DateTime endDateTime = currentDateTime;
                int trendChartTicks = 0;
                for (DateTime tickTime = startDateTime; tickTime <= endDateTime; tickTime = tickTime.AddMinutes(PTMagicConfiguration.GeneralSettings.Monitor.GraphIntervalMinutes)) {
                  List<MarketTrendChange> tickRange = marketTrendChangeSummaries.FindAll(m => m.TrendDateTime >= tickTime).OrderBy(m => m.TrendDateTime).ToList();
                  if (tickRange.Count > 0) {
                    MarketTrendChange mtc = tickRange.First();
                    if (tickTime != startDateTime) TrendChartDataJSON += ",\n";
                    if (Double.IsInfinity(mtc.TrendChange)) mtc.TrendChange = 0;

                    TrendChartDataJSON += "{ x: new Date('" + tickTime.ToString("yyyy-MM-ddTHH:mm:ss").Replace(".", ":") + "'), y: " + mtc.TrendChange.ToString("0.00", new System.Globalization.CultureInfo("en-US")) + "}";
                    trendChartTicks++;
                  }
                }

                // Add most recent tick
                List<MarketTrendChange> latestTickRange = marketTrendChangeSummaries.OrderByDescending(m => m.TrendDateTime).ToList();
                if (latestTickRange.Count > 0) {
                  MarketTrendChange mtc = latestTickRange.First();
                  if (trendChartTicks > 0) TrendChartDataJSON += ",\n";
                  if (Double.IsInfinity(mtc.TrendChange)) mtc.TrendChange = 0;

                  TrendChartDataJSON += "{ x: new Date('" + mtc.TrendDateTime.ToString("yyyy-MM-ddTHH:mm:ss").Replace(".", ":") + "'), y: " + mtc.TrendChange.ToString("0.00", new System.Globalization.CultureInfo("en-US")) + "}";
                }

                TrendChartDataJSON += "]";
                TrendChartDataJSON += "}";

                mtIndex++;
              }
            }
          }
        }
        TrendChartDataJSON += "]";
      }
    }

    private void BuildProfitChartData() {
      int tradeDayIndex = 0;
      string profitPerDayJSON = "";
      if (PTData.SellLog.Count > 0) {
        DateTime minSellLogDate = PTData.SellLog.OrderBy(sl => sl.SoldDate).First().SoldDate.Date;
        DateTime graphStartDate = DateTime.Now.Date.AddDays(-30);
        if (minSellLogDate > graphStartDate) graphStartDate = minSellLogDate;
        for (DateTime salesDate = graphStartDate; salesDate <= DateTime.Now.Date; salesDate = salesDate.AddDays(1)) {
          if (tradeDayIndex > 0) {
            profitPerDayJSON += ",\n";
          }

          int trades = PTData.SellLog.FindAll(t => t.SoldDate.Date == salesDate).Count;
          double profit = PTData.SellLog.FindAll(t => t.SoldDate.Date == salesDate).Sum(t => t.Profit);
          double profitFiat = Math.Round(profit * Summary.MainMarketPrice, 2);

          profitPerDayJSON += "{x: new Date('" + salesDate.ToString("yyyy-MM-dd") + "'), y: " + profitFiat.ToString("0.00", new System.Globalization.CultureInfo("en-US")) + "}";

          tradeDayIndex++;
        }

        ProfitChartDataJSON = "[";
        ProfitChartDataJSON += "{";
        ProfitChartDataJSON += "key: 'Profit in " + Summary.MainFiatCurrency + "',";
        ProfitChartDataJSON += "color: '" + Constants.ChartLineColors[1] + "',";
        ProfitChartDataJSON += "values: [" + profitPerDayJSON + "]";
        ProfitChartDataJSON += "}";
        ProfitChartDataJSON += "]";
      }
    }
  }
}
