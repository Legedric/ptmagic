using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;
using Core.MarketAnalyzer;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Monitor.Pages {
  public class ResetSingleMarketSettingForm {
    public string Market = "";
    public string Setting = "";
  }

  public class ResetSingleMarketSettingModel : _Internal.BasePageModelSecure {

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
          ResetSingleMarketSettingForm rsf = JsonConvert.DeserializeObject<ResetSingleMarketSettingForm>(requestBody);
          if (rsf != null) {
            if (System.IO.File.Exists(PTMagicBasePath + Constants.PTMagicPathData + Path.DirectorySeparatorChar + "SingleMarketSettingSummary.json")) {
              try {
                List<SingleMarketSettingSummary> smsSummaries = JsonConvert.DeserializeObject<List<SingleMarketSettingSummary>>(System.IO.File.ReadAllText(PTMagicBasePath + Constants.PTMagicPathData + Path.DirectorySeparatorChar + "SingleMarketSettingSummary.json"));
                SingleMarketSettingSummary smsSummary = smsSummaries.Find(s => s.Market.Equals(rsf.Market) && s.SingleMarketSetting.SettingName.Equals(rsf.Setting));
                if (smsSummary != null) {
                  smsSummaries.Remove(smsSummary);

                  // Save Single Market Settings Summary
                  JsonSerializerSettings smsSummaryJsonSettings = new JsonSerializerSettings();
                  smsSummaryJsonSettings.NullValueHandling = NullValueHandling.Ignore;
                  smsSummaryJsonSettings.DefaultValueHandling = DefaultValueHandling.Ignore;

                  FileHelper.WriteTextToFile(PTMagicBasePath + Constants.PTMagicPathData + Path.DirectorySeparatorChar, "SingleMarketSettingSummary.json", JsonConvert.SerializeObject(smsSummaries, Formatting.None, smsSummaryJsonSettings));

                }

              } catch { }
            }
          }
        }
      }

      return result;
    }
  }
}
