using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects;
using Core.Main.DataObjects.PTMagicData;
using System.Globalization;

namespace Monitor.Pages {
  public class TransactionsModel : _Internal.BasePageModelSecure {
    public TransactionData TransactionData = null;
    public string ValidationMessage = "";

    public void OnGet() {
      base.Init();

      BindData();
    }

    private void BindData() {
      TransactionData = new TransactionData(PTMagicBasePath);
    }

    public void OnPost() {
      base.Init();

      BindData();

      SaveTransaction();
    }

    private void SaveTransaction() {
      double transactionAmount = 0;
      DateTimeOffset transactionDateTime = Constants.confMinDate;

      try {
        transactionAmount = SystemHelper.TextToDouble(HttpContext.Request.Form["Transaction_Amount"], transactionAmount, "en-US");
        //transactionDateTime = DateTimeOffset.Parse(HttpContext.Request.Form["Transaction_Date"] + " " + HttpContext.Request.Form["Transaction_Time"], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        DateTime tmp = DateTime.Parse(HttpContext.Request.Form["Transaction_Date"] + " " + HttpContext.Request.Form["Transaction_Time"], CultureInfo.InvariantCulture, DateTimeStyles.None);

        // Convert local offset time to UTC
        TimeSpan offsetTimeSpan = TimeSpan.Parse(PTMagicConfiguration.GeneralSettings.Application.TimezoneOffset.Replace("+", ""));
        transactionDateTime = new DateTimeOffset(tmp, offsetTimeSpan);
      } catch { }

      if (transactionAmount == 0) {
        ValidationMessage = "Please enter a valid amount in the format 123.45!";
      } else {
        if (transactionDateTime == Constants.confMinDate) {
          ValidationMessage = "Please select a valid date and time!";
        } else {
          TransactionData.Transactions.Add(new Transaction() { GUID = Guid.NewGuid().ToString(), Amount = transactionAmount, UTCDateTime = transactionDateTime.UtcDateTime });
          TransactionData.SaveTransactions(PTMagicBasePath);

          NotifyHeadline = "Transaction saved!";
          NotifyMessage = "Transaction saved successfully to _data/Transactions.json.";
          NotifyType = "success";
        }
      }
    }
  }
}
