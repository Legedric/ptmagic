using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Text;
using System.Globalization;
using Core.Main;

namespace Core.Helper {

  public class SystemHelper {
    private static bool AllwaysGoodCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors) {
      return true;
    }

    /// <summary>
    /// Checks, if a string is numeric.
    /// </summary>
    /// <param name="s">The string to check.</param>
    /// <returns>True, if the string is numeric.</returns>
    public static bool IsNumeric(string s) {
      try {
        Int32.Parse(s);
      } catch {
        return false;
      }
      return true;
    }

    public static bool IsInteger(double d) {
      return d % 1 == 0;
    }

    public static bool IsBoolean(string s) {
      try {
        Boolean.Parse(s);
      } catch {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Checks, if a string is a double value.
    /// </summary>
    /// <param name="s">The string to check.</param>
    /// <returns>True, if the string is a double value.</returns>
    public static bool IsDouble(string s) {
      try {
        Double.Parse(s);
      } catch {
        return false;
      }
      return true;
    }

    public static bool IsDouble(string s, string culture) {
      try {
        Double.Parse(s, new CultureInfo(culture));
      } catch {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Checks, if a string is a DateTime value.
    /// </summary>
    /// <param name="s">The string to check.</param>
    /// <returns>True, if the string is a DateTime value.</returns>
    public static bool IsDateTime(string s) {
      try {
        DateTime.Parse(s);
      } catch {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Konvertiert einen Text zu einem Integer-Wert mit Fehlerbehandlung. 
    /// </summary>
    /// <param name="text">Zu konvertierender Text.</param>
    /// <param name="defaultValue">Der Vorgabewert für den Fall, dass keine gültige Zahl eingegeben wurde.</param>
    /// <returns>Den Text als Integer. Wenn die Konvertierung fehlschlägt, dann wird der Defaultwert zurückgegeben.</returns>
    public static int TextToInteger(string text, int defaultValue) {
      int result = defaultValue;
      try {
        string localText = text.Replace(".", "");
        result = Convert.ToInt32(localText.Trim());
      } catch { }

      return result;
    }

    /// <summary>
    /// Konvertiert einen Text zu einem Integer64-Wert mit Fehlerbehandlung. 
    /// </summary>
    /// <param name="text">Zu konvertierender Text.</param>
    /// <param name="defaultValue">Der Vorgabewert für den Fall, dass keine gültige Zahl eingegeben wurde.</param>
    /// <returns>Den Text als Integer64. Wenn die Konvertierung fehlschlägt, dann wird der Defaultwert zurückgegeben.</returns>
    public static Int64 TextToInteger64(string text, Int64 defaultValue) {
      Int64 result = defaultValue;
      try {
        string localText = text.Replace(".", "");
        result = Convert.ToInt64(localText.Trim());
      } catch { }

      return result;
    }

    public static double TextToDouble(string text, double defaultValue, string culture) {
      double result = defaultValue;
      try {
        if (!string.IsNullOrEmpty(text)) {
          double.TryParse(text, NumberStyles.Any, new System.Globalization.CultureInfo(culture), out result);
        }
      } catch { }

      return result;
    }

    /// <summary>
    /// Konvertiert einen Text zu einem DateTime-Wert mit Fehlerbehandlung. 
    /// </summary>
    /// <param name="text">Zu konvertierender Text.</param>
    /// <param name="defaultValue">Der Vorgabewert für den Fall, dass keine gültige DateTime eingegeben wurde.</param>
    /// <returns>Den Text als DateTime. Wenn die Konvertierung fehlschlägt, dann wird der Defaultwert zurückgegeben.</returns>
    public static DateTime TextToDateTime(string text, DateTime defaultValue) {
      DateTime result = defaultValue;
      try {
        result = Convert.ToDateTime(text.Trim());
      } catch { }

      return result;
    }

    public static DateTime TextToDateTime(string text, DateTime defaultValue, string culture) {
      DateTime result = defaultValue;
      try {
        result = Convert.ToDateTime(text.Trim(), new System.Globalization.CultureInfo(culture));
      } catch { }

      return result;
    }

    /// <summary>
    /// Konvertiert einen Text zu einem Boolean-Wert mit Fehlerbehandlung. 
    /// </summary>
    /// <param name="text">Zu konvertierender Text.</param>
    /// <param name="defaultValue">Der Vorgabewert für den Fall, dass keine gültige Boolean eingegeben wurde.</param>
    /// <returns>Den Text als Boolean. Wenn die Konvertierung fehlschlägt, dann wird der Defaultwert zurückgegeben.</returns>
    public static bool TextToBoolean(string text, bool defaultValue) {
      bool result = defaultValue;
      try {
        result = Convert.ToBoolean(text.Trim());
      } catch {
        try {
          int intValue = Convert.ToInt32(text.Trim());
          result = intValue == 0 ? false : true;
        } catch { }
      }

      return result;
    }

    public static string SplitCamelCase(string s) {
      string result = "";

      string whiteList = "ABCDEFGHIJKLMNOPQRSTUVWXYZÄÜÖßabcdefghijklmnopqrstuvwxyzäüö0123456789_- ";

      if (!string.IsNullOrEmpty(s)) {
        for (int i = 0; i < s.Length; i++) {
          if (char.IsUpper(s[i]) || char.IsNumber(s[i])) {
            if (i > 0 && whiteList.Contains(s[i - 1].ToString())) {
              if (char.IsUpper(s[i])) {
                if (!char.IsUpper(s[i - 1]) && !char.IsNumber(s[i - 1])) result += " ";
              } else if (char.IsNumber(s[i])) {
                if (!char.IsNumber(s[i - 1])) result += " ";
              }
            }
          }
          result += s[i].ToString();
        }
      }

      return result;
    }

    /// <summary>
    /// Clears a string using a whitelist.
    /// </summary>
    /// <param name="text">Text to clear.</param>
    /// <param name="allowedCharacters">Allowed characters.</param>
    /// <returns>The cleared text.</returns>
    public static string StripBadCode(string text, string allowedCharacters) {
      StringBuilder sb = new StringBuilder();
      if (text != null) {
        for (int i = 0; i < text.Length; i++) {
          if (allowedCharacters.Contains(text[i].ToString())) sb.Append(text[i]);
        }
      }

      return sb.ToString();
    }

    public static bool CheckForBadCode(string text, string allowedCharacters) {
      bool result = false;
      for (int i = 0; i < text.Length; i++) {
        if (!allowedCharacters.Contains(text[i].ToString())) {
          result = true;
          break;
        }
      }

      return result;
    }

    /// <summary>
    /// Schneidet einen Text nach x Zeichen ab
    /// </summary>
    /// <param name="text">Der Text, der gekürzt werden soll.</param>
    /// <param name="maxLength">Die maximale Länge, auf die der Text gekürzt werden soll.</param>
    /// <returns>Der gekürzte Text.</returns>
    public static string CutText(string text, int maxLength, bool addDots) {
      string result = text;

      if (result.Length > maxLength) {
        result = result.Substring(0, maxLength);

        if (addDots) result += "...";
      }

      return result;
    }

    /// <summary>
    /// Ermittelt den Teilstring eines Zeitstring, der die Stunden darstellt.
    /// </summary>
    public static string GetHourFromString(string timeString) {
      string result = "";

      if (timeString.Contains(":")) {
        string[] arrTime = timeString.Split(":".ToCharArray());
        result = arrTime[0];
      }

      return result;
    }

    /// <summary>
    /// Ermittelt den Teilstring eines Zeitstring, der die Minuten darstellt.
    /// </summary>
    public static string GetMinutesFromString(string timeString) {
      string result = "";

      if (timeString.Contains(":")) {
        string[] arrTime = timeString.Split(":".ToCharArray());
        result = arrTime[1];
      }

      return result;
    }

    public static List<string> ConvertTokenStringToList(string tokenizedString, string separator) {
      List<string> result = new List<string>();

      if (!String.IsNullOrEmpty(tokenizedString) && !String.IsNullOrEmpty(separator)) {
        string[] arrTokens = tokenizedString.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < arrTokens.Length; i++) {
          result.Add(arrTokens[i].Trim());
        }
      }

      return result;
    }

    public static List<int> ConvertTokenStringToListInt(string tokenizedString, string separator) {
      List<int> result = new List<int>();

      if (!String.IsNullOrEmpty(tokenizedString) && !String.IsNullOrEmpty(separator)) {
        string[] arrTokens = tokenizedString.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < arrTokens.Length; i++) {
          result.Add(Convert.ToInt32(arrTokens[i]));
        }
      }

      return result;
    }

    public static string ConvertListToTokenString(List<string> tokenList, string separator, bool cropDoubleSeparators) {
      string result = "";

      if (tokenList.Count > 0) {
        for (int i = 0; i < tokenList.Count; i++) {
          result += tokenList[i].Trim() + separator;
        }

        if (cropDoubleSeparators)result = result.Replace(separator + separator, "");
      }

      return result;
    }

    public static string ConvertListToTokenString(List<int> tokenList, string separator) {
      string result = "";

      if (tokenList.Count > 0) {
        for (int i = 0; i < tokenList.Count; i++) {
          result += tokenList[i].ToString() + separator;
        }

        result += separator;
        result = result.Replace(separator + separator, "");
      }

      return result;
    }

    public static List<object> ConvertToObjectList<T>(List<T> inputList) {
      List<object> result = new List<object>();

      foreach (T item in inputList) {
        result.Add(item);
      }

      return result;
    }

    public static Hashtable ConvertTokenStringToHashtable(string tokenizedString, string pairSeparator, string fieldSeperator) {
      Hashtable result = new Hashtable();

      if (!String.IsNullOrEmpty(tokenizedString) && !String.IsNullOrEmpty(pairSeparator) && !String.IsNullOrEmpty(fieldSeperator)) {
        string[] arrTokens = tokenizedString.Split(pairSeparator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < arrTokens.Length; i++) {
          string[] arrKeyValuePair = arrTokens[i].Split(fieldSeperator.ToCharArray());

          result.Add(arrKeyValuePair[0], arrKeyValuePair[1]);
        }
      }

      return result;
    }

    public static string ConvertHashtableToTokenString(Hashtable tokenHashtable, string pairSeparator, string fieldSeperator) {
      string result = "";

      if (tokenHashtable.Keys.Count > 0) {
        foreach (string key in tokenHashtable.Keys) {
          result += key + fieldSeperator + tokenHashtable[key] + pairSeparator;
        }

        result += pairSeparator;
        result = result.Replace(pairSeparator + pairSeparator, "");
      }

      return result;
    }

    public static string GetProperDurationTime(int durationSeconds, bool includeDays = true) {
      string result = "";

      int days = (int)Math.Floor((double)durationSeconds / (60.0 * 60.0 * 24.0));
      if (!includeDays) days = 0;

      int hours = (int)Math.Floor((double)durationSeconds / (60.0 * 60.0)) - days * 24;
      int minutes = (int)Math.Floor((double)durationSeconds / 60.0) - (hours * 60) - (days * 24 * 60);
      int seconds = durationSeconds - (minutes * 60) - (hours * 60 * 60) - (days * 24 * 60 * 60);

      if (days > 0) {
        result += days.ToString() + "d";
      }

      if (hours > 0) {
        if (days > 0) result += " ";
        result += hours.ToString() + "h";
      }

      if (minutes > 0) {
        if (hours > 0 || days > 0) result += " ";
        result += minutes.ToString() + "m";
      }

      if (seconds > 0) {
        if (minutes > 0 || hours > 0 || days > 0) result += " ";
        result += seconds.ToString() + "s";
      }

      return result;
    }

    public static void AddValueToStringBuilder(StringBuilder sb, string value, int length, bool fillField, string delimiter) {
      if (!string.IsNullOrEmpty(value)) {
        if (value.Length > length)
          sb.Append(value.Substring(0, length)); // Beschneiden
        else {
          if (fillField)
            sb.Append(value.PadRight(length));
          else
            sb.Append(value);
        }
      } else {
        if (fillField)
          sb.Append(string.Empty.PadRight(length));
      }
      sb.Append(delimiter);
    }

    public static bool UrlIsReachable(string url) {
      ServicePointManager.Expect100Continue = true;
      ServicePointManager.DefaultConnectionLimit = 9999;
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
      ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(AllwaysGoodCertificate);

      HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
      request.Timeout = 10000;
      request.Method = "GET";

      try {
        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
          return response.StatusCode == HttpStatusCode.OK;
        }
      } catch (WebException) {
        return false;
      }
    }

    public static string GetMarketLink(string platform, string exchange, string market, string mainMarket) {
      string result = "#";
      if (platform.Equals("TradingView")) {
        result = "https://www.tradingview.com/chart/?symbol=" + exchange.ToUpper() + ":";

        string pairName = SystemHelper.StripBadCode(market, Constants.WhiteListMinimal);

        if (pairName.StartsWith(mainMarket)) {
          pairName = pairName.Replace(mainMarket, "") + mainMarket;
        }

        result += pairName;
      } else {
        switch (exchange) {
          case "Bittrex":
            result = "https://bittrex.com/Market/Index?MarketName=" + market;
            break;
          case "Binance":
            result = "https://www.binance.com/trade.html?symbol=" + market;
            break;
          case "Poloniex":
            result = "https://poloniex.com/exchange#" + market.ToLower();
            break;
        }
      }

      return result;
    }

    public static string GetFullMarketName(string mainMarket, string market, string exchange) {
      string result = market;

      switch (exchange) {
        case "Bittrex":
          result = mainMarket + "-" + market;
          break;
        case "Binance":
          result = market + mainMarket;
          break;
        case "Poloniex":
          result = mainMarket + "_" + market;
          break;
      }


      return result;
    }

    public static string GetTradingViewSymbol(string exchange, string market, string mainMarket) {
      string result = exchange.ToUpper() + ":";

      string pairName = SystemHelper.StripBadCode(market, Constants.WhiteListMinimal);

      if (pairName.StartsWith(mainMarket)) {
        pairName = pairName.Replace(mainMarket, "") + mainMarket;
      }

      result += pairName;


      return result;
    }

    public static string GetCurrencySymbol(string code) {
      string result = code;
      try {
        System.Globalization.RegionInfo regionInfo = (from culture in System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures)
                                                      where culture.Name.Length > 0 && !culture.IsNeutralCulture
                                                      let region = new System.Globalization.RegionInfo(culture.LCID)
                                                      where String.Equals(region.ISOCurrencySymbol, code, StringComparison.InvariantCultureIgnoreCase)
                                                      select region).First();
        result = regionInfo.CurrencySymbol;
      } catch {

      }

      return result;
    }

    public static string PropertyToString(object property) {
      string result = property.ToString();

      if (!property.ToString().Equals("true", StringComparison.InvariantCultureIgnoreCase) && !property.ToString().Equals("false", StringComparison.InvariantCultureIgnoreCase)) {
        try {
          double resultDouble = Convert.ToDouble(property);
          result = resultDouble.ToString(new System.Globalization.CultureInfo("en-US"));
        } catch {
        }
      } else {
        result = property.ToString().ToLower();
      }

      return result;
    }

    public static bool IsRecentVersion(string currentVersion, string latestVersion) {
      bool result = true;

      List<int> currentVersionInfo = SystemHelper.ConvertTokenStringToListInt(currentVersion, ".");
      List<int> latestVersionInfo = SystemHelper.ConvertTokenStringToListInt(latestVersion, ".");

      if (currentVersionInfo[0] < latestVersionInfo[0]) {
        result = false;
      }

      if (currentVersionInfo[0] == latestVersionInfo[0] && currentVersionInfo[1] < latestVersionInfo[1]) {
        result = false;
      }

      if (currentVersionInfo[0] == latestVersionInfo[0] && currentVersionInfo[1] == latestVersionInfo[1] && currentVersionInfo[2] < latestVersionInfo[2]) {
        result = false;
      }

      return result;
    }
  }
}
