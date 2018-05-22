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
  public class TestTelegramForm {
    public string Telegram_BotToken = "";
    public Int64 Telegram_ChatId = 0;
    public string Telegram_SilentMode = "off";
  }

  public class TestTelegramModel : _Internal.BasePageModelSecure {

    public void OnGet() {
      // Initialize Config
      base.Init();

    }

    [HttpPost]
    public ActionResult OnPost() {
      base.Init();

      JsonResult result = new JsonResult("Error sending Telegram message.");

      MemoryStream stream = new MemoryStream();
      Request.Body.CopyTo(stream);
      stream.Position = 0;
      using (StreamReader reader = new StreamReader(stream)) {
        string requestBody = reader.ReadToEnd();
        if (requestBody.Length > 0) {
          TestTelegramForm tf = JsonConvert.DeserializeObject<TestTelegramForm>(requestBody);
          if (tf != null) {
            TelegramHelper.SendMessage(tf.Telegram_BotToken.Trim(), tf.Telegram_ChatId, "PT Magic Telegram test message.", tf.Telegram_SilentMode.Equals("on"), Log);

            result =new JsonResult("Success");
          }
        }
      }

      return result;
    }
  }
}
