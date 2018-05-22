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
using System.Net;

namespace Core.MarketAnalyzer {
  public class Binance : BaseAnalyzer {
    public static double GetMainCurrencyPrice(string mainMarket, PTMagicConfiguration systemConfiguration, LogHelper log) {
      double result = 0;

      try {
        string baseUrl = "https://api.binance.com/api/v1/ticker/24hr?symbol=" + mainMarket + "USDT";

        log.DoLogInfo("Binance - Getting main market price...");
        Newtonsoft.Json.Linq.JObject jsonObject = GetSimpleJsonObjectFromURL(baseUrl, log, false);
        if (jsonObject != null) {
          log.DoLogInfo("Binance - Market data received for " + mainMarket + "USDT");

          result = (double)jsonObject.GetValue("lastPrice");
          log.DoLogInfo("Binance - Current price for " + mainMarket + "USDT: " + result.ToString("#,#0.00") + " USD");
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
        string baseUrl = "https://api.binance.com/api/v1/ticker/24hr";

        log.DoLogInfo("Binance - Getting market data...");
        Newtonsoft.Json.Linq.JArray jsonArray = GetSimpleJsonArrayFromURL(baseUrl, log);
        if (jsonArray.Count > 0) {
          double mainCurrencyPrice = 1;
          if (!mainMarket.Equals("USDT", StringComparison.InvariantCultureIgnoreCase)) {
            mainCurrencyPrice = Binance.GetMainCurrencyPrice(mainMarket, systemConfiguration, log);
          }

          log.DoLogInfo("Binance - Market data received for " + jsonArray.Count.ToString() + " currencies");

          if (mainCurrencyPrice > 0) {
            Dictionary<string, Market> markets = new Dictionary<string, Market>();
            foreach (Newtonsoft.Json.Linq.JObject currencyTicker in jsonArray) {
              string marketName = currencyTicker["symbol"].ToString();
              if (marketName.EndsWith(mainMarket, StringComparison.InvariantCultureIgnoreCase)) {

                // Set last values in case any error occurs
                lastMarket = marketName;
                lastTicker = currencyTicker;

                Market market = new Market();
                market.Position = markets.Count + 1;
                market.Name = marketName;
                market.Symbol = currencyTicker["symbol"].ToString();
                market.Price = SystemHelper.TextToDouble(currencyTicker["lastPrice"].ToString(), 0, "en-US");
                market.Volume24h = SystemHelper.TextToDouble(currencyTicker["quoteVolume"].ToString(), 0, "en-US");
                market.MainCurrencyPriceUSD = mainCurrencyPrice;

                markets.Add(market.Name, market);

                result.Add(market.Name);
              }
            }

            Binance.CheckFirstSeenDates(markets, ref marketInfos, systemConfiguration, log);

            BaseAnalyzer.SaveMarketInfosToFile(marketInfos, systemConfiguration, log);

            Binance.CheckForMarketDataRecreation(mainMarket, markets, systemConfiguration, log);

            DateTime fileDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0).ToUniversalTime();

            FileHelper.WriteTextToFile(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + Constants.PTMagicPathExchange + Path.DirectorySeparatorChar, "MarketData_" + fileDateTime.ToString("yyyy-MM-dd_HH.mm") + ".json", JsonConvert.SerializeObject(markets), fileDateTime, fileDateTime);

            log.DoLogInfo("Binance - Market data saved for " + markets.Count.ToString() + " markets with " + mainMarket + ".");

            FileHelper.CleanupFiles(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + Constants.PTMagicPathExchange + Path.DirectorySeparatorChar, systemConfiguration.AnalyzerSettings.MarketAnalyzer.StoreDataMaxHours);
            log.DoLogInfo("Binance - Market data cleaned.");
          } else {
            log.DoLogError("Binance - Failed to get main market price for " + mainMarket + ".");
            result = null;
          }
        }
      } catch (WebException ex) {
        if (ex.Response != null) {
          using (HttpWebResponse errorResponse = (HttpWebResponse)ex.Response) {
            using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream())) {
              Dictionary<string, string> errorData = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
              if (errorData != null) {
                string errorMessage = "Unable to get data from Binance with URL '" + errorResponse.ResponseUri + "'!";
                if (errorData.ContainsKey("code")) {
                  errorMessage += " - Code: " + errorData["code"];
                }

                if (errorData.ContainsKey("msg")) {
                  errorMessage += " - Message: " + errorData["msg"];
                }

                log.DoLogError(errorMessage);
              }
            }
          }
        }
        result = null;
      } catch (Exception ex) {
        log.DoLogCritical("Exception while getting data for '" + lastMarket + "': " + ex.Message, ex);
        result = null;
      }

      return result;
    }

    public static void CheckFirstSeenDates(Dictionary<string, Market> markets, ref Dictionary<string, MarketInfo> marketInfos, PTMagicConfiguration systemConfiguration, LogHelper log) {
      log.DoLogInfo("Binance - Checking first seen dates for " + markets.Count + " markets. This may take a while...");

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
          marketInfo.FirstSeen = Binance.GetFirstSeenDate(key, systemConfiguration, log);
        } else {
          if (marketInfo.FirstSeen == Constants.confMinDate) {
            marketInfo.FirstSeen = Binance.GetFirstSeenDate(key, systemConfiguration, log);
          }
        }
        marketInfo.LastSeen = DateTime.Now.ToUniversalTime();

        marketsChecked++;

        if ((marketsChecked % 20) == 0) {
          log.DoLogInfo("Binance - Yes, I am still checking first seen dates... " + marketsChecked + "/" + markets.Count + " markets done...");
        }
      }
    }

    public static DateTime GetFirstSeenDate(string marketName, PTMagicConfiguration systemConfiguration, LogHelper log) {
      DateTime result = Constants.confMinDate;

      string baseUrl = "https://api.binance.com/api/v1/klines?interval=1d&symbol=" + marketName + "&limit=100";

      log.DoLogDebug("Binance - Getting first seen date for '" + marketName + "'...");

      Newtonsoft.Json.Linq.JArray jsonArray = GetSimpleJsonArrayFromURL(baseUrl, log);
      if (jsonArray.Count > 0) {
        result = Constants.Epoch.AddMilliseconds((Int64)jsonArray[0][0]);
        log.DoLogDebug("Binance - First seen date for '" + marketName + "' set to " + result.ToString());
      }

      return result;
    }

    public static List<MarketTick> GetMarketTicks(string marketName, int ticksNeeded, PTMagicConfiguration systemConfiguration, LogHelper log) {
      List<MarketTick> result = new List<MarketTick>();

      try {
        Int64 endTime = (Int64)Math.Ceiling(DateTime.Now.ToUniversalTime().Subtract(Constants.Epoch).TotalMilliseconds);
        int ticksLimit = 500;
        string baseUrl = "";
        int ticksFetched = 0;

        if (ticksNeeded < ticksLimit) {
          ticksLimit = ticksNeeded;
        }

        bool go = true;
        while (ticksFetched < ticksNeeded && go) {
          baseUrl = "https://api.binance.com/api/v1/klines?interval=1m&symbol=" + marketName + "&endTime=" + endTime.ToString() + "&limit=" + ticksLimit.ToString();

          log.DoLogDebug("Binance - Getting " + ticksLimit.ToString() + " ticks for '" + marketName + "'...");
          Newtonsoft.Json.Linq.JArray jsonArray = GetSimpleJsonArrayFromURL(baseUrl, log);
          if (jsonArray.Count > 0) {
            log.DoLogDebug("Binance - " + jsonArray.Count.ToString() + " ticks received.");

            foreach (Newtonsoft.Json.Linq.JArray marketTick in jsonArray) {

              MarketTick tick = new MarketTick();
              tick.Price = (double)marketTick[4];
              tick.Volume24h = (double)marketTick[7];
              tick.Time = Constants.Epoch.AddMilliseconds((Int64)marketTick[0]);

              result.Add(tick);
            }

            ticksFetched = ticksFetched + jsonArray.Count;
            endTime = endTime - ticksLimit * 60 * 1000;
            if (ticksNeeded - ticksFetched < ticksLimit) {
              ticksLimit = ticksNeeded - ticksFetched;
            }
          } else {
            log.DoLogDebug("Binance - No ticks received.");
            go = false;
          }
        }
      } catch (WebException ex) {
        if (ex.Response != null) {
          using (HttpWebResponse errorResponse = (HttpWebResponse)ex.Response) {
            using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream())) {
              Dictionary<string, string> errorData = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
              if (errorData != null) {
                string errorMessage = "Unable to get data from Binance with URL '" + errorResponse.ResponseUri + "'!";
                if (errorData.ContainsKey("code")) {
                  errorMessage += " - Code: " + errorData["code"];
                }

                if (errorData.ContainsKey("msg")) {
                  errorMessage += " - Message: " + errorData["msg"];
                }

                log.DoLogError(errorMessage);
              }
            }
          }
        }
        result = null;
      } catch (Exception ex) {
        log.DoLogCritical(ex.Message, ex);
      }

      return result;
    }

    public static void CheckForMarketDataRecreation(string mainMarket, Dictionary<string, Market> markets, PTMagicConfiguration systemConfiguration, LogHelper log) {
      string binanceDataDirectoryPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + Constants.PTMagicPathExchange + Path.DirectorySeparatorChar;

      if (!Directory.Exists(binanceDataDirectoryPath)) {
        Directory.CreateDirectory(binanceDataDirectoryPath);
      }

      DirectoryInfo dataDirectory = new DirectoryInfo(binanceDataDirectoryPath);

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
          log.DoLogInfo("Binance - Recreating market data for " + markets.Count + " markets over " + SystemHelper.GetProperDurationTime(lastMarketDataAgeInSeconds) + ". This may take a while...");
          endDateTime = latestMarketDataFileDateTime;
        } else {
          // No existing market files found => Recreate market data for configured timeframe
          log.DoLogInfo("Binance - Recreating market data for " + markets.Count + " markets over " + systemConfiguration.AnalyzerSettings.MarketAnalyzer.StoreDataMaxHours + " hours. This may take a while...");
        }

        int totalTicks = (int)Math.Ceiling(startDateTime.Subtract(endDateTime).TotalMinutes);

        // Get Ticks for main market
        List<MarketTick> mainMarketTicks = new List<MarketTick>();
        if (!mainMarket.Equals("USDT", StringComparison.InvariantCultureIgnoreCase)) {
          mainMarketTicks = Binance.GetMarketTicks(mainMarket + "USDT", totalTicks, systemConfiguration, log);
        }

        // Get Ticks for all markets
        log.DoLogDebug("Binance - Getting ticks for '" + markets.Count + "' markets");
        Dictionary<string, List<MarketTick>> marketTicks = new Dictionary<string, List<MarketTick>>();
        foreach (string key in markets.Keys) {
          marketTicks.Add(key, Binance.GetMarketTicks(key, totalTicks, systemConfiguration, log));

          if ((marketTicks.Count % 10) == 0) {
            log.DoLogInfo("Binance - No worries, I am still alive... " + marketTicks.Count + "/" + markets.Count + " markets done...");
          }
        }

        log.DoLogInfo("Binance - Ticks completed.");

        log.DoLogInfo("Binance - Creating initial market data ticks. This may take another while...");

        // Go back in time and create market data
        int completedTicks = 0;
        if (marketTicks.Count > 0) {
          for (DateTime tickTime = startDateTime; tickTime >= endDateTime; tickTime = tickTime.AddMinutes(-1)) {
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

            log.DoLogDebug("Binance - Market data saved for tick " + fileDateTime.ToString() + " - MainCurrencyPrice=" + mainCurrencyPrice.ToString("#,#0.00") + " USD.");

            if ((completedTicks % 100) == 0) {
              log.DoLogInfo("Binance - Our magicbots are still at work, hang on... " + completedTicks + "/" + totalTicks + " ticks done...");
            }
          }
        }

        log.DoLogInfo("Binance - Initial market data created. Ready to go!");
      }

    }
  }
}
