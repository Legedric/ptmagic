using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;

namespace Monitor.Pages {
  public class SettingsGeneralModel : _Internal.BasePageModelSecure {
    public string ValidationMessage = "";

    private string GetTimezoneOffsetString(TimeZoneInfo tzi) {
      string result = "";

      result += (tzi.BaseUtcOffset >= TimeSpan.Zero) ? "+" : "-";
      result += Math.Abs(tzi.BaseUtcOffset.Hours).ToString().Trim();
      result += ":";
      result += Math.Abs(tzi.BaseUtcOffset.Minutes).ToString("00").Trim();

      return result;
    }

    public string GetTimezoneSelection() {
      string result = "";

      List<string> tzOffsetList = new List<string>();
      foreach (TimeZoneInfo tzi in TimeZoneInfo.GetSystemTimeZones()) {
        string offsetString = this.GetTimezoneOffsetString(tzi);
        if (!tzOffsetList.Contains(offsetString)) {
          string selected = "";
          if (PTMagicConfiguration.GeneralSettings.Application.TimezoneOffset.Equals(offsetString, StringComparison.InvariantCultureIgnoreCase)) {
            selected = " selected=\"selected\"";
          }

          result += "<option" + selected + ">" + offsetString + "</option>\n";
          tzOffsetList.Add(offsetString);
        }
      }

      return result;
    }

    public void OnGet() {
      base.Init();

      string notification = GetStringParameter("n", "");
      if (notification.Equals("BackupRestored")) {
        NotifyHeadline = "Backup restored!";
        NotifyMessage = "Your backup of settings.general.json was successfully restored.";
        NotifyType = "success";
      }
    }

    public void OnPost() {
      base.Init();

      PTMagicConfiguration.GeneralSettings.Application.IsEnabled = HttpContext.Request.Form["Application_IsEnabled"].Equals("on");
      PTMagicConfiguration.GeneralSettings.Application.TestMode = HttpContext.Request.Form["Application_TestMode"].Equals("on");
      PTMagicConfiguration.GeneralSettings.Application.Exchange = HttpContext.Request.Form["Application_Exchange"];
      PTMagicConfiguration.GeneralSettings.Application.StartBalance = SystemHelper.TextToDouble(HttpContext.Request.Form["Application_StartBalance"], PTMagicConfiguration.GeneralSettings.Application.StartBalance, "en-US");
      PTMagicConfiguration.GeneralSettings.Application.TimezoneOffset = HttpContext.Request.Form["Application_TimezoneOffset"].ToString().Replace(" ", "");
      PTMagicConfiguration.GeneralSettings.Application.AlwaysLoadDefaultBeforeSwitch = HttpContext.Request.Form["Application_AlwaysLoadDefaultBeforeSwitch"].Equals("on");
      PTMagicConfiguration.GeneralSettings.Application.FloodProtectionMinutes = SystemHelper.TextToInteger(HttpContext.Request.Form["Application_FloodProtectionMinutes"], PTMagicConfiguration.GeneralSettings.Application.FloodProtectionMinutes);
      PTMagicConfiguration.GeneralSettings.Application.InstanceName = HttpContext.Request.Form["Application_InstanceName"];

      PTMagicConfiguration.GeneralSettings.Monitor.IsPasswordProtected = HttpContext.Request.Form["Monitor_IsPasswordProtected"].Equals("on");
      PTMagicConfiguration.GeneralSettings.Monitor.OpenBrowserOnStart = HttpContext.Request.Form["Monitor_OpenBrowserOnStart"].Equals("on");
      PTMagicConfiguration.GeneralSettings.Monitor.GraphIntervalMinutes = SystemHelper.TextToInteger(HttpContext.Request.Form["Monitor_GraphIntervalMinutes"], PTMagicConfiguration.GeneralSettings.Monitor.GraphIntervalMinutes);
      PTMagicConfiguration.GeneralSettings.Monitor.GraphMaxTimeframeHours = SystemHelper.TextToInteger(HttpContext.Request.Form["Monitor_GraphMaxTimeframeHours"], PTMagicConfiguration.GeneralSettings.Monitor.GraphMaxTimeframeHours);
      PTMagicConfiguration.GeneralSettings.Monitor.RefreshSeconds = SystemHelper.TextToInteger(HttpContext.Request.Form["Monitor_RefreshSeconds"], PTMagicConfiguration.GeneralSettings.Monitor.RefreshSeconds);
      PTMagicConfiguration.GeneralSettings.Monitor.BagAnalyzerRefreshSeconds = SystemHelper.TextToInteger(HttpContext.Request.Form["Monitor_BagAnalyzerRefreshSeconds"], PTMagicConfiguration.GeneralSettings.Monitor.BagAnalyzerRefreshSeconds);
      PTMagicConfiguration.GeneralSettings.Monitor.BuyAnalyzerRefreshSeconds = SystemHelper.TextToInteger(HttpContext.Request.Form["Monitor_BuyAnalyzerRefreshSeconds"], PTMagicConfiguration.GeneralSettings.Monitor.BuyAnalyzerRefreshSeconds);
      PTMagicConfiguration.GeneralSettings.Monitor.LinkPlatform = HttpContext.Request.Form["Monitor_LinkPlatform"];
      PTMagicConfiguration.GeneralSettings.Monitor.MaxTopMarkets = SystemHelper.TextToInteger(HttpContext.Request.Form["Monitor_MaxTopMarkets"], PTMagicConfiguration.GeneralSettings.Monitor.MaxTopMarkets);
      PTMagicConfiguration.GeneralSettings.Monitor.MaxDailySummaries = SystemHelper.TextToInteger(HttpContext.Request.Form["Monitor_MaxDailySummaries"], PTMagicConfiguration.GeneralSettings.Monitor.MaxDailySummaries);
      PTMagicConfiguration.GeneralSettings.Monitor.MaxMonthlySummaries = SystemHelper.TextToInteger(HttpContext.Request.Form["Monitor_MaxMonthlySummaries"], PTMagicConfiguration.GeneralSettings.Monitor.MaxMonthlySummaries);
      PTMagicConfiguration.GeneralSettings.Monitor.MaxDashboardBuyEntries = SystemHelper.TextToInteger(HttpContext.Request.Form["Monitor_MaxDashboardBuyEntries"], PTMagicConfiguration.GeneralSettings.Monitor.MaxDashboardBuyEntries);
      PTMagicConfiguration.GeneralSettings.Monitor.MaxDashboardBagEntries = SystemHelper.TextToInteger(HttpContext.Request.Form["Monitor_MaxDashboardBagEntries"], PTMagicConfiguration.GeneralSettings.Monitor.MaxDashboardBagEntries);
      PTMagicConfiguration.GeneralSettings.Monitor.MaxDCAPairs = SystemHelper.TextToInteger(HttpContext.Request.Form["Monitor_MaxDCAPairs"], PTMagicConfiguration.GeneralSettings.Monitor.MaxDCAPairs);
      PTMagicConfiguration.GeneralSettings.Monitor.DefaultDCAMode = HttpContext.Request.Form["Monitor_DefaultDCAMode"];

      PTMagicConfiguration.GeneralSettings.Backup.IsEnabled = HttpContext.Request.Form["Backup_IsEnabled"].Equals("on");
      PTMagicConfiguration.GeneralSettings.Backup.MaxHours = SystemHelper.TextToInteger(HttpContext.Request.Form["Backup_MaxHours"], PTMagicConfiguration.GeneralSettings.Backup.MaxHours);

      PTMagicConfiguration.GeneralSettings.Telegram.IsEnabled = HttpContext.Request.Form["Telegram_IsEnabled"].Equals("on");
      PTMagicConfiguration.GeneralSettings.Telegram.BotToken = HttpContext.Request.Form["Telegram_BotToken"].ToString().Trim();
      PTMagicConfiguration.GeneralSettings.Telegram.ChatId = SystemHelper.TextToInteger64(HttpContext.Request.Form["Telegram_ChatId"], PTMagicConfiguration.GeneralSettings.Telegram.ChatId);
      PTMagicConfiguration.GeneralSettings.Telegram.SilentMode = HttpContext.Request.Form["Telegram_SilentMode"].Equals("on");

      PTMagicConfiguration.WriteGeneralSettings(PTMagicBasePath);

      NotifyHeadline = "Settings saved!";
      NotifyMessage = "Settings saved successfully to settings.general.json.";
      NotifyType = "success";
    }
  }
}
