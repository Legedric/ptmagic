using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;
using Core.MarketAnalyzer;
using Core.ProfitTrailer;
using Microsoft.Extensions.Primitives;

namespace Monitor._Internal {

  public class BasePageModelSecure : BasePageModel {
    public void Init() {
      base.PreInit();

      if (String.IsNullOrEmpty(HttpContext.Session.GetString("LoggedIn" + PTMagicConfiguration.GeneralSettings.Monitor.Port.ToString())) && PTMagicConfiguration.GeneralSettings.Monitor.IsPasswordProtected) {
        bool redirectToLogin = true;
        if (Request.Cookies.ContainsKey("PTMRememberMeKey")) {
          string rememberMeKey = Request.Cookies["PTMRememberMeKey"];
          if (!rememberMeKey.Equals("")) {
            string encryptedPassword = EncryptionHelper.Decrypt(Request.Cookies["PTMRememberMeKey"]);
            if (encryptedPassword.Equals(PTMagicConfiguration.SecureSettings.MonitorPassword)) {
              HttpContext.Session.SetString("LoggedIn" + PTMagicConfiguration.GeneralSettings.Monitor.Port.ToString(), DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"));
              redirectToLogin = false;
            }
          }
        }

        if (redirectToLogin) {
          HttpContext.Response.Redirect(PTMagicConfiguration.GeneralSettings.Monitor.RootUrl + "Login");
        }
      }
    }
  }
}
