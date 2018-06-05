using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;
using Newtonsoft.Json;

namespace Core.ProfitTrailer {
  public static class StrategyHelper {
    public static string GetStrategyShortcut(string strategyName, bool onlyValidStrategies) {
      string result = strategyName;

      switch (strategyName.ToLower()) {
        case "lowbb":
          result = "LBB";
          break;
        case "highbb":
          result = "HBB";
          break;
        case "gain":
          result = "G";
          break;
        case "loss":
          result = "L";
          break;
        case "smagain":
          result = "SG";
          break;
        case "emagain":
          result = "EG";
          break;
        case "smaspread":
          result = "SS";
          break;
        case "emaspread":
          result = "ES";
          break;
        case "smacross":
          result = "SC";
          break;
        case "emacross":
          result = "EC";
          break;
        case "rsi":
          result = "RSI";
          break;
        case "stoch":
          result = "STOCH";
          break;
        case "stochrsi":
          result = "SRSI";
          break;
        case "macd":
          result = "MACD";
          break;
        case "obv":
          result = "OBV";
          break;
        case "bbwidth":
          result = "BBW";
          break;
        case "anderson":
          result = "AND";
          break;
        case "som enabled":
          result = "SOM";
          break;
        case "max buy times":
          result = "MAX";
          break;
        case "max pairs":
          result = "PAIRS";
          break;
        case "max spread":
          result = "SPREAD";
          break;
        case "price increase":
          result = "PIN";
          break;
        case "min buy volume":
          result = "VOL";
          break;
        case "min buy balance":
          result = "MIN";
          break;
        case "coin age":
          result = "AGE";
          break;
        case "too new":
          result = "NEW";
          break;
        case "blacklisted":
          result = "BLACK";
          break;
        case "insufficient balance":
          result = "BAL";
          break;
        case "max cost reached":
          result = "COST";
          break;
        case "rebuy timeout":
          result = "TIMEOUT";
          break;
        default:
          break;
      }

      if (onlyValidStrategies) {
        if (strategyName.IndexOf("SOM") > -1 || strategyName.IndexOf("MAX") > -1 || strategyName.IndexOf("MIN") > -1 || strategyName.IndexOf("PRICE") > -1 || strategyName.IndexOf("BLACK") > -1 || strategyName.IndexOf("INSUFFICIENT") > -1 || strategyName.IndexOf("COST") > -1 || strategyName.IndexOf("TIMEOUT") > -1) {
          result = "";
        }
      }

      return result;
    }

    public static bool IsValidStrategy(string strategyName) {
      return StrategyHelper.IsValidStrategy(strategyName, false);
    }

    public static bool IsValidStrategy(string strategyName, bool checkForAnyInvalid) {
      bool result = false;

      if (!checkForAnyInvalid) {
        switch (strategyName.ToLower()) {
          case "lowbb":
          case "highbb":
          case "gain":
          case "loss":
          case "smagain":
          case "emagain":
          case "smaspread":
          case "emaspread":
          case "smacross":
          case "emacross":
          case "rsi":
          case "stoch":
          case "stochrsi":
          case "macd":
          case "obv":
          case "bbwidth":
          case "anderson":
            result = true;
            break;
          default:
            break;
        }
      } else {
        if (strategyName.IndexOf("max", StringComparison.InvariantCultureIgnoreCase) == -1
          && strategyName.IndexOf("min", StringComparison.InvariantCultureIgnoreCase) == -1
          && strategyName.IndexOf("som", StringComparison.InvariantCultureIgnoreCase) == -1
          && strategyName.IndexOf("price", StringComparison.InvariantCultureIgnoreCase) == -1
          && strategyName.IndexOf("black", StringComparison.InvariantCultureIgnoreCase) == -1
          && strategyName.IndexOf("new", StringComparison.InvariantCultureIgnoreCase) == -1
          && strategyName.IndexOf("insufficient", StringComparison.InvariantCultureIgnoreCase) == -1
          && strategyName.IndexOf("timeout", StringComparison.InvariantCultureIgnoreCase) == -1
          && strategyName.IndexOf("spread", StringComparison.InvariantCultureIgnoreCase) == -1
          && strategyName.IndexOf("pairs", StringComparison.InvariantCultureIgnoreCase) == -1) {
          result = true;
        }
      }

      return result;
    }

    public static int GetStrategyValueDecimals(string strategyName) {
      int result = 0;

      switch (strategyName.ToLower()) {
        case "lowbb":
        case "highbb":
          result = 8;
          break;
        case "gain":
        case "loss":
        case "smagain":
        case "emagain":
        case "smaspread":
        case "emaspread":
        case "anderson":
        case "smacross":
        case "emacross":
          result = 2;
          break;
        case "rsi":
        case "stochrsi":
        case "stoch":
        case "macd":
        case "obv":
        case "bbwidth":
          result = 0;
          break;
        default:
          break;
      }

      return result;
    }

    public static string GetStrategyText(Summary summary, List<Strategy> strategies, string strategyText, bool isTrue, bool isTrailingBuyActive) {
      if (strategies.Count > 0) {
        foreach (Strategy strategy in strategies) {
          string textClass = (strategy.IsTrue) ? "label-success" : "label-danger";
          if (!StrategyHelper.IsValidStrategy(strategy.Name)) {
            strategyText += "<span class=\"label label-warning\" data-toggle=\"tooltip\" data-placement=\"top\" title=\"" + strategy.Name + "\">" + StrategyHelper.GetStrategyShortcut(strategy.Name, false) + "</span> ";
          } else {
            strategyText += "<span class=\"label " + textClass + "\" data-toggle=\"tooltip\" data-placement=\"top\" title=\"" + strategy.Name + "\">" + StrategyHelper.GetStrategyShortcut(strategy.Name, false) + "</span> ";
          }
        }

        if (isTrailingBuyActive) {
          strategyText += " <i class=\"fa fa-flag text-success\" data-toggle=\"tooltip\" data-placement=\"top\" title=\"Trailing active!\"></i>";
        }
      } else {
        if (isTrue) {
          strategyText = "<span class=\"label label-success\" data-toggle=\"tooltip\" data-placement=\"top\" title=\"" + strategyText + "\">" + StrategyHelper.GetStrategyShortcut(strategyText, true) + "</span>";

          if (isTrailingBuyActive) {
            strategyText += " <i class=\"fa fa-flag text-success\" data-toggle=\"tooltip\" data-placement=\"top\" title=\"Trailing active!\"></i>";
          }
        } else {
          if (StrategyHelper.IsValidStrategy(strategyText)) {
            strategyText = "<span class=\"label label-danger\" data-toggle=\"tooltip\" data-placement=\"top\" title=\"" + strategyText + "\">" + StrategyHelper.GetStrategyShortcut(strategyText, true) + "</span>";
          } else if (strategyText.Equals("")) {
            strategyText = summary.DCABuyStrategy;
            strategyText = "<span class=\"label label-danger\" data-toggle=\"tooltip\" data-placement=\"top\" title=\"" + strategyText + "\">" + StrategyHelper.GetStrategyShortcut(strategyText, true) + "</span>";
          } else {
            strategyText = "<span class=\"label label-warning\" data-toggle=\"tooltip\" data-placement=\"top\" title=\"" + strategyText + "\">" + StrategyHelper.GetStrategyShortcut(strategyText, false) + "</span> ";
          }
        }
      }

      return strategyText;
    }

    public static string GetCurrentValueText(List<Strategy> strategies, string strategyText, double bbValue, double simpleValue, bool includeShortcut) {
      string result = "";

      if (strategies.Count > 0) {
        foreach (Strategy strategy in strategies) {
          if (StrategyHelper.IsValidStrategy(strategy.Name)) {
            if (!result.Equals("")) result += "<br />";

            string decimalFormat = "";
            int decimals = StrategyHelper.GetStrategyValueDecimals(strategy.Name);
            for (int d = 1; d <= decimals; d++) {
              decimalFormat += "0";
            }

            if (includeShortcut) {
              result += "<span class=\"text-muted\">" + StrategyHelper.GetStrategyShortcut(strategy.Name, true) + "</span> ";
            }

            if (StrategyHelper.GetStrategyShortcut(strategy.Name, true).IndexOf("and", StringComparison.InvariantCultureIgnoreCase) > -1) {
              result += simpleValue.ToString("#,#0.00", new System.Globalization.CultureInfo("en-US"));
            } else {
              if (decimals == 0) {
                if (!SystemHelper.IsInteger(strategy.CurrentValue)) {
                  result += strategy.CurrentValue.ToString("#,#", new System.Globalization.CultureInfo("en-US"));
                } else {
                  result += strategy.CurrentValue.ToString("#,#0", new System.Globalization.CultureInfo("en-US"));
                }
              } else {
                result += strategy.CurrentValue.ToString("#,#0." + decimalFormat, new System.Globalization.CultureInfo("en-US"));
              }
            }
          }
        }
      } else {
        if (StrategyHelper.GetStrategyShortcut(strategyText, true).IndexOf("bb", StringComparison.InvariantCultureIgnoreCase) > -1) {
          result = bbValue.ToString("#,#0.00000000", new System.Globalization.CultureInfo("en-US"));
        } else {
          result = simpleValue.ToString("#,#0.00", new System.Globalization.CultureInfo("en-US")) + "%";
        }
      }

      return result;
    }

    public static string GetTriggerValueText(Summary summary, List<Strategy> strategies, string strategyText, double bbValue, double simpleValue, int buyLevel, bool includeShortcut) {
      string result = "";

      if (strategies.Count > 0) {
        foreach (Strategy strategy in strategies) {
          if (StrategyHelper.IsValidStrategy(strategy.Name)) {
            if (!result.Equals("")) result += "<br />";

            string decimalFormat = "";
            int decimals = StrategyHelper.GetStrategyValueDecimals(strategy.Name);
            for (int d = 1; d <= decimals; d++) {
              decimalFormat += "0";
            }

            if (includeShortcut) {
              result += "<span class=\"text-muted\">" + StrategyHelper.GetStrategyShortcut(strategy.Name, true) + "</span> ";
            }

            if (StrategyHelper.GetStrategyShortcut(strategy.Name, true).IndexOf("and", StringComparison.InvariantCultureIgnoreCase) > -1) {
              result += strategy.TriggerValue.ToString("#,#0.00", new System.Globalization.CultureInfo("en-US"));
            } else {
              if (decimals == 0) {
                if (!SystemHelper.IsInteger(strategy.EntryValue)) {
                  result += strategy.EntryValue.ToString(new System.Globalization.CultureInfo("en-US"));
                } else {
                  result += strategy.EntryValue.ToString("#,#0", new System.Globalization.CultureInfo("en-US"));
                }
              } else {
                result += strategy.EntryValue.ToString("#,#0." + decimalFormat, new System.Globalization.CultureInfo("en-US"));
              }
            }
          }
        }
      } else {
        if (StrategyHelper.GetStrategyShortcut(strategyText, true).IndexOf("bb", StringComparison.InvariantCultureIgnoreCase) > -1) {
          result = bbValue.ToString("#,#0.00000000", new System.Globalization.CultureInfo("en-US"));
        } else {
          if (simpleValue == Constants.MinTrendChange) {
            if (summary.DCATriggers.ContainsKey(buyLevel + 1)) {
              simpleValue = summary.DCATriggers[buyLevel + 1];
            }
          }
          result = simpleValue.ToString("#,#0.00", new System.Globalization.CultureInfo("en-US")) + "%";
        }
      }

      return result;
    }
  }
}