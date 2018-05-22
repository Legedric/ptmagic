using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;
using Core.MarketAnalyzer;

namespace Monitor.Pages {
  public class SettingsMarketTrendsModel : _Internal.BasePageModelSecure {
    public MarketTrend MarketTrend = null;
    public string MarketTrendName = "";

    public void OnGet() {
      // Initialize Config
      base.Init();
      
      BindData();
    }

    private void BindData() {
      MarketTrendName = this.GetStringParameter("mt", "");
      if (!MarketTrendName.Equals("")) {
        MarketTrend = PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.MarketTrends.Find(m => SystemHelper.StripBadCode(m.Name, Constants.WhiteListNames).Equals(MarketTrendName));
      } else {
        MarketTrend = new MarketTrend();
        MarketTrend.Name = "New Market Trend";
      }
    }
  }
}
