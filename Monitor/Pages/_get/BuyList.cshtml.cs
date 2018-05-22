using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Main.DataObjects;
using Core.Main.DataObjects.PTMagicData;
using Core.MarketAnalyzer;

namespace Monitor.Pages {
  public class BuyListModel : _Internal.BasePageModelSecureAJAX {
    public ProfitTrailerData PTData = null;
    public string SortFieldId = "";
    public string SortDirection = "";

    public void OnGet() {
      base.Init();
      
      BindData();
    }

    private void BindData() {
      SortFieldId = GetStringParameter("s", "ProfitPercent");
      SortDirection = GetStringParameter("d", "DESC");

      PTData = new ProfitTrailerData(PTMagicBasePath, PTMagicConfiguration);
    }
  }
}
