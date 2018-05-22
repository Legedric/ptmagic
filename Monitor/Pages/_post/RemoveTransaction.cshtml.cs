using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Main.DataObjects;
using Core.Main.DataObjects.PTMagicData;
using Core.MarketAnalyzer;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Monitor.Pages {
  public class RemoveTransactionForm {
    public string Transaction_GUID = "";
  }

  public class RemoveTransactionModel : _Internal.BasePageModelSecure {

    public void OnGet() {
      // Initialize Config
      base.Init();

    }

    [HttpPost]
    public ActionResult OnPost() {
      base.Init();

      JsonResult result = new JsonResult("Error removing transaction.");

      MemoryStream stream = new MemoryStream();
      Request.Body.CopyTo(stream);
      stream.Position = 0;
      using (StreamReader reader = new StreamReader(stream)) {
        string requestBody = reader.ReadToEnd();
        if (requestBody.Length > 0) {
          RemoveTransactionForm rtf = JsonConvert.DeserializeObject<RemoveTransactionForm>(requestBody);
          if (rtf != null) {
            TransactionData transactionData = new TransactionData(PTMagicBasePath);
            Transaction removeTransaction = transactionData.Transactions.Find(t => t.GUID.Equals(rtf.Transaction_GUID));
            if (removeTransaction != null) {
              transactionData.Transactions.Remove(removeTransaction);
              transactionData.SaveTransactions(PTMagicBasePath);

              result = new JsonResult("Success");
            } else {
              result = new JsonResult("Transaction not found!");
            }
          }
        }
      }

      return result;
    }
  }
}
