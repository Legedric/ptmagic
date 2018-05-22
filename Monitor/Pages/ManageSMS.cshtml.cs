using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Main.DataObjects.PTMagicData;
using Newtonsoft.Json;

namespace Monitor.Pages {
  public class ManageSMSModel : _Internal.BasePageModelSecure {
    public List<SingleMarketSettingSummary> SingleMarketSettingSummaries = new List<SingleMarketSettingSummary>();

    public void OnGet() {
      base.Init();

      BindData();
    }

    private void BindData() {
      if (System.IO.File.Exists(PTMagicBasePath + Constants.PTMagicPathData + Path.DirectorySeparatorChar + "SingleMarketSettingSummary.json")) {
        try {
          SingleMarketSettingSummaries = JsonConvert.DeserializeObject<List<SingleMarketSettingSummary>>(System.IO.File.ReadAllText(PTMagicBasePath + Constants.PTMagicPathData + Path.DirectorySeparatorChar + "SingleMarketSettingSummary.json"));
        } catch { }
      }

      string notification = GetStringParameter("n", "");
      if (notification.Equals("SettingReset")) {
        NotifyHeadline = "Setting Reset!";
        NotifyMessage = "The setting will get reset on the next interval!";
        NotifyType = "success";
      }
    }

    public double GetTrendChange(string marketTrend, MarketPairSummary mps, TriggerSnapshot ts, string marketTrendRelation) {
      double result = 0;

      if (mps.MarketTrendChanges.ContainsKey(marketTrend)) {
        result = mps.MarketTrendChanges[marketTrend];
        double averageMarketTrendChange = Summary.MarketTrendChanges[marketTrend].OrderByDescending(mtc => mtc.TrendDateTime).First().TrendChange;
        if (marketTrendRelation.Equals(Constants.MarketTrendRelationAbsolute, StringComparison.InvariantCulture)) {
          result = result - averageMarketTrendChange;
        } else if (marketTrendRelation.Equals(Constants.MarketTrendRelationRelativeTrigger, StringComparison.InvariantCulture)) {
          double currentPrice = mps.LatestPrice;
          double triggerPrice = ts.LastPrice;
          double triggerTrend = (currentPrice - triggerPrice) / triggerPrice * 100;
          result = triggerTrend;
        }
      }

      return result;
    }
  }
}
