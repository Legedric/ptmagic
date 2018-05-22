using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Main.DataObjects;
using Core.Main.DataObjects.PTMagicData;
using Core.MarketAnalyzer;

namespace Monitor.Pages {
  public class TickerWidgetsModel : _Internal.BasePageModelSecureAJAX {
    public ProfitTrailerData PTData = null;
    public List<string> MarketsWithSingleSettings = new List<string>();

    public void OnGet() {
      // Initialize Config
      base.Init();
      
      BindData();
    }

    private void BindData() {
      PTData = new ProfitTrailerData(PTMagicBasePath, PTMagicConfiguration);

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
  }
}
