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
  public class SalesListModel : _Internal.BasePageModelSecure {
    public ProfitTrailerData PTData = null;
    private string salesDateString = "";
    private string salesMonthString = "";

    public string SalesTimeframe = "";
    public DateTime SalesDate = Constants.confMinDate;
    public List<SellLogData> SellLog = new List<SellLogData>();

    public void OnGet() {
      // Initialize Config
      base.Init();
      
      BindData();
    }

    private void BindData() {
      salesDateString = GetStringParameter("d", "");
      salesMonthString = GetStringParameter("m", "");

      PTData = new ProfitTrailerData(PTMagicBasePath, PTMagicConfiguration);

      if (!salesDateString.Equals("")) {
        SalesDate = SystemHelper.TextToDateTime(salesDateString, Constants.confMinDate);
        if (SalesDate != Constants.confMinDate) {
          SalesTimeframe = SalesDate.ToShortDateString();
          SellLog = PTData.SellLog.FindAll(sl => sl.SoldDate.Date == SalesDate.Date).OrderByDescending(sl => sl.SoldDate).ToList();
        }
      } else if (!salesMonthString.Equals("")) {
        SalesDate = SystemHelper.TextToDateTime(salesMonthString + "-01", Constants.confMinDate);
        if (SalesDate != Constants.confMinDate) {
          SalesTimeframe = SalesDate.ToString("MMMM yyyy", new System.Globalization.CultureInfo("en-US"));
          SellLog = PTData.SellLog.FindAll(sl => sl.SoldDate.Date.Month == SalesDate.Month && sl.SoldDate.Date.Year == SalesDate.Year).OrderByDescending(sl => sl.SoldDate).ToList();
        }
      }
    }
  }
}
