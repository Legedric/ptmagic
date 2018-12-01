using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;
using Newtonsoft.Json;
using Core.ProfitTrailer;

namespace Core.MarketAnalyzer {
  public class Poloniex : BaseAnalyzer {
    public static double GetMainCurrencyPrice(string mainMarket, PTMagicConfiguration systemConfiguration, LogHelper log) {
      double result = 0;

      try {
        string baseUrl = "https://bittrex.com/api/v1.1/public/getmarketsummary?market=USDT-" + mainMarket;

        log.DoLogInfo("Poloniex - Getting main market price...");
        Dictionary<string, dynamic> jsonObject = GetJsonFromURL(baseUrl, log, "");
        if (jsonObject.Count > 0) {
          if (jsonObject["success"]) {
            log.DoLogInfo("Poloniex - Market data received for USDT_" + mainMarket);

            result = jsonObject["result"][0]["Last"];
            log.DoLogInfo("Poloniex - Current price for USDT_" + mainMarket + ": " + result.ToString("#,#0.00") + " USD");
          }
        }
      } catch (Exception ex) {
        log.DoLogCritical(ex.Message, ex);
      }

      return result;
    }

    public static List<string> GetMarketData(string mainMarket, Dictionary<string, MarketInfo> marketInfos, PTMagicConfiguration systemConfiguration, LogHelper log) {
      List<string> result = new List<string>();

      string lastMarket = "";
      KeyValuePair<string, dynamic> lastTicker = new KeyValuePair<string, dynamic>();
      try {
        string baseUrl = "https://poloniex.com/public?command=returnTicker";

        log.DoLogInfo("Poloniex - Getting market data...");
        Dictionary<string, dynamic> jsonObject = GetJsonFromURL(baseUrl, log, "");
        if (jsonObject.Count > 0) {
          log.DoLogInfo("Poloniex - Market data received for " + jsonObject.Count.ToString() + " currencies");

          double mainCurrencyPrice = 1;
          if (!mainMarket.Equals("USDT", StringComparison.InvariantCultureIgnoreCase)) {
            mainCurrencyPrice = Poloniex.GetMainCurrencyPrice(mainMarket, systemConfiguration, log);
          }

          if (mainCurrencyPrice > 0) {
            Dictionary<string, Market> markets = new Dictionary<string, Market>();
            foreach (KeyValuePair<string, dynamic> currencyTicker in jsonObject) {
              string marketName = currencyTicker.Key.ToString();
              if (marketName.StartsWith(mainMarket, StringComparison.InvariantCultureIgnoreCase)) {

                // Set last values in case any error occurs
                lastMarket = marketName;
                lastTicker = currencyTicker;

                Market market = new Market();
                market.Position = markets.Count + 1;
                market.Name = marketName;
                market.Symbol = currencyTicker.Key.ToString();
                market.Price = SystemHelper.TextToDouble(currencyTicker.Value["last"].ToString(), 0.0, "en-US");
                market.Volume24h = SystemHelper.TextToDouble(currencyTicker.Value["baseVolume"].ToString(), 0.0, "en-US");
                market.MainCurrencyPriceUSD = mainCurrencyPrice;

                markets.Add(market.Name, market);

                result.Add(market.Name);
              }
            }

            Poloniex.CheckFirstSeenDates(markets, ref marketInfos, systemConfiguration, log);

            BaseAnalyzer.SaveMarketInfosToFile(marketInfos, systemConfiguration, log);

            Poloniex.CheckForMarketDataRecreation(mainMarket, markets, systemConfiguration, log);

            DateTime fileDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0).ToUniversalTime();

            FileHelper.WriteTextToFile(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + Constants.PTMagicPathExchange + Path.DirectorySeparatorChar, "MarketData_" + fileDateTime.ToString("yyyy-MM-dd_HH.mm") + ".json", JsonConvert.SerializeObject(markets), fileDateTime, fileDateTime);


            log.DoLogInfo("Poloniex - Market data saved for " + markets.Count.ToString() + " markets with " + mainMarket + ".");

            FileHelper.CleanupFiles(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + Constants.PTMagicPathExchange + Path.DirectorySeparatorChar, systemConfiguration.AnalyzerSettings.MarketAnalyzer.StoreDataMaxHours);
            log.DoLogInfo("Poloniex - Market data cleaned.");
          } else {
            log.DoLogError("Poloniex - Failed to get main market price for " + mainMarket + ".");
            result = null;
          }
        }
      } catch (Exception ex) {
        log.DoLogCritical("Exception while getting data for '" + lastMarket + "': " + ex.Message, ex);
        result = null;
      }

      return result;
    }

    public static void CheckFirstSeenDates(Dictionary<string, Market> markets, ref Dictionary<string, MarketInfo> marketInfos, PTMagicConfiguration systemConfiguration, LogHelper log) {
      log.DoLogInfo("Poloniex - Checking first seen dates for " + markets.Count + " markets. This may take a while...");

      int marketsChecked = 0;
      foreach (string key in markets.Keys) {
        // Save market info
        MarketInfo marketInfo = null;
        if (marketInfos.ContainsKey(key)) {
          marketInfo = marketInfos[key];
        }
        if (marketInfo == null) {
          marketInfo = new MarketInfo();
          marketInfo.Name = key;
          marketInfos.Add(key, marketInfo);
          marketInfo.FirstSeen = Poloniex.GetFirstSeenDate(key, systemConfiguration, log);
        } else {
          if (marketInfo.FirstSeen == Constants.confMinDate) {
            marketInfo.FirstSeen = Poloniex.GetFirstSeenDate(key, systemConfiguration, log);
          }
        }
        marketInfo.LastSeen = DateTime.Now.ToUniversalTime();

        marketsChecked++;

        if ((marketsChecked % 20) == 0) {
          log.DoLogInfo("Poloniex - Yes, I am still checking first seen dates... " + marketsChecked + "/" + markets.Count + " markets done...");
        }
      }
    }

    public static DateTime GetFirstSeenDate(string marketName, PTMagicConfiguration systemConfiguration, LogHelper log) {
      DateTime result = Constants.confMinDate;

      Int64 startTime = (Int64)Math.Ceiling(DateTime.Now.ToUniversalTime().AddDays(-100).Subtract(Constants.Epoch).TotalSeconds);
      string baseUrl = "https://poloniex.com/public?command=returnChartData&period=14400&start=" + startTime.ToString() + "&end=9999999999&currencyPair=" + marketName;

      log.DoLogDebug("Poloniex - Getting first seen date for '" + marketName + "'...");

      List<dynamic> jsonObject = GetSimpleJsonListFromURL(baseUrl, log);
      if (jsonObject.Count > 0) {
        var marketTick = jsonObject[0];

        result = Constants.Epoch.AddSeconds((int)marketTick["date"]);
        log.DoLogDebug("Poloniex - First seen date for '" + marketName + "' set to " + result.ToString());
      }

      return result;
    }

    public static List<MarketTick> GetMarketTicks(string marketName, PTMagicConfiguration systemConfiguration, LogHelper log) {
      List<MarketTick> result = new List<MarketTick>();

      try {
        Int64 startTime = (Int64)Math.Ceiling(DateTime.Now.ToUniversalTime().AddHours(-systemConfiguration.AnalyzerSettings.MarketAnalyzer.StoreDataMaxHours).Subtract(Constants.Epoch).TotalSeconds);
        string baseUrl = "https://poloniex.com/public?command=returnChartData&period=300&start=" + startTime.ToString() + "&end=9999999999&currencyPair=" + marketName;

        log.DoLogDebug("Poloniex - Getting ticks for '" + marketName + "'...");
        List<dynamic> jsonObject = GetSimpleJsonListFromURL(baseUrl, log);
        if (jsonObject.Count > 0) {
          log.DoLogDebug("Poloniex - " + jsonObject.Count.ToString() + " ticks received.");

          foreach (var marketTick in jsonObject) {

            MarketTick tick = new MarketTick();
            tick.Price = (double)marketTick["close"];
            tick.Time = Constants.Epoch.AddSeconds((int)marketTick["date"]);

            result.Add(tick);
          }
        }
      } catch (Exception ex) {
        log.DoLogCritical(ex.Message, ex);
      }

      return result;
    }

    public static void CheckForMarketDataRecreation(string mainMarket, Dictionary<string, Market> markets, PTMagicConfiguration systemConfiguration, LogHelper log) {
      string poloniexDataDirectoryPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + Constants.PTMagicPathExchange + Path.DirectorySeparatorChar;

      if (!Directory.Exists(poloniexDataDirectoryPath)) {
        Directory.CreateDirectory(poloniexDataDirectoryPath);
      }

      DirectoryInfo dataDirectory = new DirectoryInfo(poloniexDataDirectoryPath);

      // Check for existing market files
      DateTime latestMarketDataFileDateTime = Constants.confMinDate;
      List<FileInfo> marketFiles = dataDirectory.EnumerateFiles("MarketData*").ToList();
      FileInfo latestMarketDataFile = null;
      if (marketFiles.Count > 0) {
        latestMarketDataFile = marketFiles.OrderByDescending(mdf => mdf.LastWriteTimeUtc).First();
        latestMarketDataFileDateTime = latestMarketDataFile.LastWriteTimeUtc;
      }

      if (latestMarketDataFileDateTime < DateTime.Now.ToUniversalTime().AddMinutes(-(systemConfiguration.AnalyzerSettings.MarketAnalyzer.IntervalMinutes * 3))) {
        int lastMarketDataAgeInSeconds = (int)Math.Ceiling(DateTime.Now.ToUniversalTime().Subtract(latestMarketDataFileDateTime).TotalSeconds);

        // Go back in time and create market data
        DateTime startDateTime = DateTime.Now.ToUniversalTime();
        DateTime endDateTime = DateTime.Now.ToUniversalTime().AddHours(-systemConfiguration.AnalyzerSettings.MarketAnalyzer.StoreDataMaxHours);
        if (latestMarketDataFileDateTime != Constants.confMinDate && latestMarketDataFileDateTime > endDateTime) {
          // Existing market files too old => Recreate market data for configured timeframe
          log.DoLogInfo("Poloniex - Recreating market data for " + markets.Count + " markets over " + SystemHelper.GetProperDurationTime(lastMarketDataAgeInSeconds) + ". This may take a while...");
          endDateTime = latestMarketDataFileDateTime;
        } else {
          // No existing market files found => Recreate market data for configured timeframe
          log.DoLogInfo("Poloniex - Recreating market data for " + markets.Count + " markets over " + systemConfiguration.AnalyzerSettings.MarketAnalyzer.StoreDataMaxHours + " hours. This may take a while...");
        }

        // Get Ticks for main market
        List<MarketTick> mainMarketTicks = new List<MarketTick>();
        if (!mainMarket.Equals("USDT", StringComparison.InvariantCultureIgnoreCase)) {
          mainMarketTicks = Poloniex.GetMarketTicks("USDT_" + mainMarket, systemConfiguration, log);
        }

        // Get Ticks for all markets
        log.DoLogDebug("Poloniex - Getting ticks for '" + markets.Count + "' markets");
        Dictionary<string, List<MarketTick>> marketTicks = new Dictionary<string, List<MarketTick>>();
        foreach (string key in markets.Keys) {
          marketTicks.Add(key, Poloniex.GetMarketTicks(key, systemConfiguration, log));

          if ((marketTicks.Count % 10) == 0) {
            log.DoLogInfo("Poloniex - No worries, I am still alive... " + marketTicks.Count + "/" + markets.Count + " markets done...");
          }
        }

        log.DoLogInfo("Poloniex - Ticks completed.");

        log.DoLogInfo("Poloniex - Creating initial market data ticks. This may take another while...");

        // Go back in time and create market data
        int totalTicks = (int)Math.Ceiling(startDateTime.Subtract(endDateTime).TotalMinutes);
        int completedTicks = 0;
        if (marketTicks.Count > 0) {
          for (DateTime tickTime = startDateTime; tickTime >= endDateTime; tickTime = tickTime.AddMinutes(-5)) {
            completedTicks++;

            double mainCurrencyPrice = 1;
            if (mainMarketTicks.Count > 0) {
              List<MarketTick> mainCurrencyTickRange = mainMarketTicks.FindAll(t => t.Time <= tickTime);
              if (mainCurrencyTickRange.Count > 0) {
                MarketTick mainCurrencyTick = mainCurrencyTickRange.OrderByDescending(t => t.Time).First();
                mainCurrencyPrice = mainCurrencyTick.Price;
              }
            }

            Dictionary<string, Market> tickMarkets = new Dictionary<string, Market>();
            foreach (string key in markets.Keys) {
              List<MarketTick> tickRange = marketTicks[key].FindAll(t => t.Time <= tickTime);

              if (tickRange.Count > 0) {
                MarketTick marketTick = tickRange.OrderByDescending(t => t.Time).First();

                Market market = new Market();
                market.Position = markets.Count + 1;
                market.Name = key;
                market.Symbol = key;
                market.Price = marketTick.Price;
                //market.Volume24h = marketTick.Volume24h;
                market.MainCurrencyPriceUSD = mainCurrencyPrice;

                tickMarkets.Add(market.Name, market);
              }
            }

            DateTime fileDateTime = new DateTime(tickTime.ToLocalTime().Year, tickTime.ToLocalTime().Month, tickTime.ToLocalTime().Day, tickTime.ToLocalTime().Hour, tickTime.ToLocalTime().Minute, 0).ToUniversalTime();

            FileHelper.WriteTextToFile(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + Constants.PTMagicPathExchange + Path.DirectorySeparatorChar, "MarketData_" + fileDateTime.ToString("yyyy-MM-dd_HH.mm") + ".json", JsonConvert.SerializeObject(tickMarkets), fileDateTime, fileDateTime);


            log.DoLogDebug("Poloniex - Market data saved for tick " + tickTime.ToLocalTime().ToString() + " - MainCurrencyPrice=" + mainCurrencyPrice.ToString("#,#0.00") + " USD.");

            if ((completedTicks % 100) == 0) {
              log.DoLogInfo("Poloniex - Our magicbots are still at work, hang on... " + completedTicks + "/" + totalTicks + " ticks done...");
            }
          }
        }

        log.DoLogInfo("Poloniex - Initial market data created. Ready to go!");
      }

    }
  }
}
