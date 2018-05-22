using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Main.DataObjects;
using Core.Main.DataObjects.PTMagicData;
using Core.MarketAnalyzer;

namespace Monitor.Pages {
  public class DashboardTopModel : _Internal.BasePageModelSecureAJAX {
    public ProfitTrailerData PTData = null;
    public DateTimeOffset DateTimeNow = Constants.confMinDate;

    public void OnGet() {
      // Initialize Config
      base.Init();

      BindData();
    }

    private void BindData() {
      PTData = new ProfitTrailerData(PTMagicBasePath, PTMagicConfiguration);

      // Convert local offset time to UTC
      TimeSpan offsetTimeSpan = TimeSpan.Parse(PTMagicConfiguration.GeneralSettings.Application.TimezoneOffset.Replace("+", ""));
      DateTimeNow = DateTimeOffset.UtcNow.ToOffset(offsetTimeSpan);
    }
  }
}
