using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Core.Main;
using Core.Helper;

namespace Monitor.Pages {
  public class LoginModel : _Internal.BasePageModel {
    public string CurrentPassword = "";

    public void OnGet() {
      base.PreInit();

      CurrentPassword = PTMagicConfiguration.SecureSettings.MonitorPassword;
      if (CurrentPassword.Equals("")) {
        Response.Redirect(PTMagicConfiguration.GeneralSettings.Monitor.RootUrl + "SetupPassword");
      }
    }

    public void OnPost(string password, string cbRememberMe) {
      base.PreInit();

      string encryptedPassword = EncryptionHelper.Encrypt(password);

      if (encryptedPassword.Equals(PTMagicConfiguration.SecureSettings.MonitorPassword)) {
        HttpContext.Session.SetString("LoggedIn" + PTMagicConfiguration.GeneralSettings.Monitor.Port.ToString(), DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"));

        if (cbRememberMe != null) {
          if (cbRememberMe.Equals("on", StringComparison.InvariantCultureIgnoreCase)) {
            CookieOptions cookieOption = new CookieOptions();
            cookieOption.Expires = DateTime.Now.AddYears(1);

            string cookieValue = EncryptionHelper.Encrypt(encryptedPassword);

            Response.Cookies.Append("PTMRememberMeKey", cookieValue, cookieOption);
          }
        }

        Response.Redirect(PTMagicConfiguration.GeneralSettings.Monitor.RootUrl);
      }
    }
  }
}
