using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;

namespace Monitor.Pages {
  public class MarketAnalyzerModel : _Internal.BasePageModelSecure {
    public List<MarketTrend> MarketTrends { get; set; } = new List<MarketTrend>();
    public string TrendChartDataJSON = "";

    public void OnGet() {
      base.Init();
      
      BindData();
    }

    private void BindData() {
      // Get market trends
      MarketTrends = PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.MarketTrends.OrderBy(mt => mt.TrendMinutes).ThenByDescending(mt => mt.Platform).ToList();

      BuildMarketTrendChartData();
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
  }
}
