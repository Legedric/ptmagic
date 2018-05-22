using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Core.Main.DataObjects.PTMagicData;
using Core.Helper;

namespace Core.Main.DataObjects {

  public class TransactionData {
    private List<Transaction> _transactions = new List<Transaction>();

    public TransactionData(string basePath) {
      string transactionsFilePath = basePath + Constants.PTMagicPathData + Path.DirectorySeparatorChar + "Transactions.json";
      if (File.Exists(transactionsFilePath)) {
        this._transactions = JsonConvert.DeserializeObject<List<Transaction>>(File.ReadAllText(transactionsFilePath));
      }
    }

    public List<Transaction> Transactions {
      get {
        return _transactions;
      }
    }

    public void SaveTransactions(string basePath) {
      FileHelper.WriteTextToFile(basePath + Constants.PTMagicPathData + Path.DirectorySeparatorChar, "Transactions.json", JsonConvert.SerializeObject(this.Transactions));
    }
  }
}
