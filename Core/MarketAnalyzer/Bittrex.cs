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
  public class Bittrex : BaseAnalyzer {
    public static double GetMainCurrencyPrice(string mainMarket, PTMagicConfiguration systemConfiguration, LogHelper log) {
      double result = 0;

      try {
        string baseUrl = "https://bittrex.com/api/v1.1/public/getmarketsummary?market=USDT-" + mainMarket;

        log.DoLogInfo("Bittrex - Getting main market price...");
        Dictionary<string, dynamic> jsonObject = GetJsonFromURL(baseUrl, log);
        if (jsonObject.Count > 0) {
          if (jsonObject["success"]) {
            log.DoLogInfo("Bittrex - Market data received for USDT-" + mainMarket);

            result = jsonObject["result"][0]["Last"];
            log.DoLogInfo("Bittrex - Current price for USDT-" + mainMarket + ": " + result.ToString("#,#0.00") + " USD");
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
      Newtonsoft.Json.Linq.JObject lastTicker = null;
      try {
        string baseUrl = "https://bittrex.com/api/v2.0/pub/markets/GetMarketSummaries";

        log.DoLogInfo("Bittrex - Getting market data...");
        Dictionary<string, dynamic> jsonObject = GetJsonFromURL(baseUrl, log);
        if (jsonObject.Count > 0) {
          if (jsonObject["success"]) {
            log.DoLogInfo("Bittrex - Market data received for " + jsonObject["result"].Count.ToString() + " currencies");

            double mainCurrencyPrice = 1;
            if (!mainMarket.Equals("USDT", StringComparison.InvariantCultureIgnoreCase)) {
              mainCurrencyPrice = Bittrex.GetMainCurrencyPrice(mainMarket, systemConfiguration, log);
            }

            if (mainCurrencyPrice > 0) {
              Dictionary<string, Market> markets = new Dictionary<string, Market>();
              foreach (Newtonsoft.Json.Linq.JObject currencyTicker in jsonObject["result"]) {
                string marketName = currencyTicker["Summary"]["MarketName"].ToString();
                if (marketName.StartsWith(mainMarket, StringComparison.InvariantCultureIgnoreCase)) {

                  // Set last values in case any error occurs
                  lastMarket = marketName;
                  lastTicker = currencyTicker;

                  Market market = new Market();
                  market.Position = markets.Count + 1;
                  market.Name = marketName;
                  market.Symbol = currencyTicker["Summary"]["MarketName"].ToString();
                  if (currencyTicker["Summary"]["Last"].Type == Newtonsoft.Json.Linq.JTokenType.Float) market.Price = (double)currencyTicker["Summary"]["Last"];
                  if (currencyTicker["Summary"]["BaseVolume"].Type == Newtonsoft.Json.Linq.JTokenType.Float) market.Volume24h = (double)currencyTicker["Summary"]["BaseVolume"];
                  market.MainCurrencyPriceUSD = mainCurrencyPrice;

                  markets.Add(market.Name, market);

                  result.Add(market.Name);

                  // Save market info
                  MarketInfo marketInfo = null;
                  if (marketInfos.ContainsKey(marketName)) {
                    marketInfo = marketInfos[marketName];
                  }

                  if (marketInfo == null) {
                    marketInfo = new MarketInfo();
                    marketInfo.Name = marketName;
                    marketInfos.Add(marketName, marketInfo);
                  }
                  if (currencyTicker["Summary"]["Created"].Type == Newtonsoft.Json.Linq.JTokenType.Date) marketInfo.FirstSeen = (DateTime)currencyTicker["Summary"]["Created"];
                  marketInfo.LastSeen = DateTime.Now.ToUniversalTime();
                }
              }

              BaseAnalyzer.SaveMarketInfosToFile(marketInfos, systemConfiguration, log);

              Bittrex.CheckForMarketDataRecreation(mainMarket, markets, systemConfiguration, log);

              DateTime fileDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0).ToUniversalTime();

              FileHelper.WriteTextToFile(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + Constants.PTMagicPathExchange + Path.DirectorySeparatorChar, "MarketData_" + fileDateTime.ToString("yyyy-MM-dd_HH.mm") + ".json", JsonConvert.SerializeObject(markets), fileDateTime, fileDateTime);

              log.DoLogInfo("Bittrex - Market data saved for " + markets.Count.ToString() + " markets with " + mainMarket + ".");

              FileHelper.CleanupFiles(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + Constants.PTMagicPathExchange + Path.DirectorySeparatorChar, systemConfiguration.AnalyzerSettings.MarketAnalyzer.StoreDataMaxHours);
              log.DoLogInfo("Bittrex - Market data cleaned.");
            } else {
              log.DoLogError("Bittrex - Failed to get main market price for " + mainMarket + ".");
              result = null;
            }
          }
        }
      } catch (Exception ex) {
        log.DoLogCritical("Exception while getting data for '" + lastMarket + "': " + ex.Message, ex);
        result = null;
      }

      return result;
    }

    public static List<MarketTick> GetMarketTicks(string marketName, PTMagicConfiguration systemConfiguration, LogHelper log) {
      List<MarketTick> result = new List<MarketTick>();

      try {
        string baseUrl = "https://bittrex.com/Api/v2.0/pub/market/GetTicks?tickInterval=oneMin&marketName=" + marketName;

        log.DoLogDebug("Bittrex - Getting ticks for '" + marketName + "'...");
        Dictionary<string, dynamic> jsonObject = GetJsonFromURL(baseUrl, log);
        if (jsonObject.Count > 0) {
          if (jsonObject["success"]) {
            if (jsonObject["result"] != null) {
              log.DoLogDebug("Bittrex - " + jsonObject["result"].Count.ToString() + " ticks received.");

              foreach (var marketTick in jsonObject["result"]) {

                MarketTick tick = new MarketTick();
                tick.Price = (double)marketTick["C"];
                tick.Time = SystemHelper.TextToDateTime(marketTick["T"].ToString(), Constants.confMinDate);

                result.Add(tick);
              }
            } else {
              log.DoLogDebug("Bittrex - No ticks received.");
            }
          }
        }
      } catch (Exception ex) {
        log.DoLogCritical(ex.Message, ex);
      }

      return result;
    }

    public static void CheckForMarketDataRecreation(string mainMarket, Dictionary<string, Market> markets, PTMagicConfiguration systemConfiguration, LogHelper log) {
      string bittrexDataDirectoryPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + Constants.PTMagicPathExchange + Path.DirectorySeparatorChar;

      if (!Directory.Exists(bittrexDataDirectoryPath)) {
        Directory.CreateDirectory(bittrexDataDirectoryPath);
      }

      DirectoryInfo dataDirectory = new DirectoryInfo(bittrexDataDirectoryPath);

      // Check for existing market files
      DateTime latestMarketDataFileDateTime = Constants.confMinDate;
      List<FileInfo> marketFiles = dataDirectory.EnumerateFiles("MarketData*").ToList();
      FileInfo latestMarketDataFile = null;
      if (marketFiles.Count > 0) {
        latestMarketDataFile = marketFiles.OrderByDescending(mdf => mdf.LastWriteTimeUtc).First();
        latestMarketDataFileDateTime = latestMarketDataFile.LastWriteTimeUtc;
      }

      if (latestMarketDataFileDateTime < DateTime.Now.ToUniversalTime().AddMinutes(-20)) {
        int lastMarketDataAgeInSeconds = (int)Math.Ceiling(DateTime.Now.ToUniversalTime().Subtract(latestMarketDataFileDateTime).TotalSeconds);

        // Go back in time and create market data
        DateTime startDateTime = DateTime.Now.ToUniversalTime();
        DateTime endDateTime = DateTime.Now.ToUniversalTime().AddHours(-systemConfiguration.AnalyzerSettings.MarketAnalyzer.StoreDataMaxHours);
        if (latestMarketDataFileDateTime != Constants.confMinDate && latestMarketDataFileDateTime > endDateTime) {
          // Existing market files too old => Recreate market data for configured timeframe
          log.DoLogInfo("Bittrex - Recreating market data for " + markets.Count + " markets over " + SystemHelper.GetProperDurationTime(lastMarketDataAgeInSeconds) + ". This may take a while...");
          endDateTime = latestMarketDataFileDateTime;
        } else {
          // No existing market files found => Recreate market data for configured timeframe
          log.DoLogInfo("Bittrex - Recreating market data for " + markets.Count + " markets over " + systemConfiguration.AnalyzerSettings.MarketAnalyzer.StoreDataMaxHours + " hours. This may take a while...");
        }

        // Get Ticks for main market
        List<MarketTick> mainMarketTicks = new List<MarketTick>();
        if (!mainMarket.Equals("USDT", StringComparison.InvariantCultureIgnoreCase)) {
          mainMarketTicks = Bittrex.GetMarketTicks("USDT-" + mainMarket, systemConfiguration, log);
        }

        // Get Ticks for all markets
        log.DoLogDebug("Bittrex - Getting ticks for '" + markets.Count + "' markets");
        Dictionary<string, List<MarketTick>> marketTicks = new Dictionary<string, List<MarketTick>>();
        foreach (string key in markets.Keys) {
          marketTicks.Add(key, Bittrex.GetMarketTicks(key, systemConfiguration, log));

          if ((marketTicks.Count % 10) == 0) {
            log.DoLogInfo("Bittrex - No worries, I am still alive... " + marketTicks.Count + "/" + markets.Count + " markets done...");
          }
        }

        log.DoLogInfo("Bittrex - Ticks completed.");

        log.DoLogInfo("Bittrex - Creating initial market data ticks. This may take another while...");

        int totalTicks = (int)Math.Ceiling(startDateTime.Subtract(endDateTime).TotalMinutes);
        int completedTicks = 0;
        if (marketTicks.Count > 0) {
          for (DateTime tickTime = startDateTime.ToUniversalTime(); tickTime >= endDateTime.ToUniversalTime(); tickTime = tickTime.AddMinutes(-1)) {
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

            log.DoLogDebug("Bittrex - Market data saved for tick " + fileDateTime.ToString() + " - MainCurrencyPrice=" + mainCurrencyPrice.ToString("#,#0.00") + " USD.");

            if ((completedTicks % 100) == 0) {
              log.DoLogInfo("Bittrex - Our magicbots are still at work, hang on... " + completedTicks + "/" + totalTicks + " ticks done...");
            }
          }
        }

        log.DoLogInfo("Bittrex - Initial market data created. Ready to go!");
      }

    }
  }
}
