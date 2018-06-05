using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Main.DataObjects.PTMagicData {
  #region Settings Objects
  public class GeneralSettingsWrapper {
    public GeneralSettings GeneralSettings { get; set; }
  }

  public class AnalyzerSettingsWrapper {
    public AnalyzerSettings AnalyzerSettings { get; set; }
  }

  public class SecureSettingsWrapper {
    public SecureSettings SecureSettings { get; set; }
  }

  #region GeneralSettings
  public class GeneralSettings {
    public Application Application { get; set; }
    public Monitor Monitor { get; set; }
    public Backup Backup { get; set; }
    public Telegram Telegram { get; set; }
  }

  public class Application {
    public bool IsEnabled { get; set; } = true;
    public bool TestMode { get; set; } = true;
    public bool EnableBetaFeatures { get; set; } = false;
    public int ProfitTrailerMajorVersion { get; set; } = 1;
    public string ProfitTrailerPath { get; set; }
    public string ProfitTrailerLicense { get; set; } = "";
    public string ProfitTrailerMonitorURL { get; set; } = "http://localhost:8081/";
    public string ProfitTrailerDefaultSettingName { get; set; } = "default";
    public bool AlwaysLoadDefaultBeforeSwitch { get; set; } = true;
    public int FloodProtectionMinutes { get; set; } = 15;
    public string Exchange { get; set; }
    public double StartBalance { get; set; } = 0;
    public string InstanceName { get; set; } = "PT Magic";
    public string TimezoneOffset { get; set; } = "+0:00";
    public string MainFiatCurrency { get; set; } = "USD";
  }

  public class Monitor {
    private string _rootUrl = "/";

    public bool IsPasswordProtected { get; set; } = true;
    public bool OpenBrowserOnStart { get; set; } = false;
    public int Port { get; set; } = 5000;
    public int GraphIntervalMinutes { get; set; } = 60;
    public int GraphMaxTimeframeHours { get; set; } = 24;
    public int RefreshSeconds { get; set; } = 30;
    public int BagAnalyzerRefreshSeconds { get; set; } = 5;
    public int BuyAnalyzerRefreshSeconds { get; set; } = 5;
    public int MaxTopMarkets { get; set; } = 20;
    public int MaxDailySummaries { get; set; } = 10;
    public int MaxMonthlySummaries { get; set; } = 10;
    public int MaxDashboardBuyEntries { get; set; } = 10;
    public int MaxDashboardBagEntries { get; set; } = 10;
    public int MaxDCAPairs { get; set; } = 24;
    public int MaxSettingsLogEntries { get; set; } = 20;
    public string LinkPlatform { get; set; } = "TradingView";
    public string DefaultDCAMode { get; set; } = "Simple";

    public string RootUrl {
      get {
        if (!_rootUrl.EndsWith("/")) _rootUrl += "/";
        return _rootUrl;
      }
      set {
        _rootUrl = value;
      }
    }
  }

  public class Backup {
    public bool IsEnabled { get; set; } = true;
    public int MaxHours { get; set; } = 48;
  }

  public class Telegram {
    public bool IsEnabled { get; set; } = false;
    public string BotToken { get; set; }
    public Int64 ChatId { get; set; }
    public bool SilentMode { get; set; } = false;
  }
  #endregion

  #region AnalyzerSettings
  public class AnalyzerSettings {
    public MarketAnalyzer MarketAnalyzer { get; set; }
    public List<GlobalSetting> GlobalSettings { get; set; }
    public List<SingleMarketSetting> SingleMarketSettings { get; set; }
  }

  public class MarketAnalyzer {
    public int StoreDataMaxHours { get; set; }
    public int IntervalMinutes { get; set; } = 5;
    public bool ExcludeMainCurrency { get; set; } = true;
    public List<MarketTrend> MarketTrends { get; set; }
  }

  public class MarketTrend {
    public string Name { get; set; }
    public string Platform { get; set; } = "Exchange";

    [DefaultValue("Market")]
    public string TrendCurrency { get; set; } = "Market";

    [DefaultValue(0)]
    public int MaxMarkets { get; set; } = 0;
    public int TrendMinutes { get; set; } = 0;

    [DefaultValue(true)]
    public bool DisplayGraph { get; set; } = true;

    [DefaultValue(true)]
    public bool DisplayOnMarketAnalyzerList { get; set; } = true;

    [DefaultValue("")]
    public string IgnoredMarkets { get; set; } = "";

    [DefaultValue("")]
    public string AllowedMarkets { get; set; } = "";

    [DefaultValue(true)]
    public bool ExcludeMainCurrency { get; set; } = true;
  }

  public class GlobalSetting {
    public string SettingName { get; set; }
    public string TriggerConnection { get; set; } = "AND";
    public List<Trigger> Triggers { get; set; } = new List<Trigger>();
    public Dictionary<string, object> PairsProperties { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object> DCAProperties { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object> IndicatorsProperties { get; set; } = new Dictionary<string, object>();
  }

  public class SingleMarketSetting {
    public string SettingName { get; set; }
    public string TriggerConnection { get; set; } = "AND";

    [DefaultValue("AND")]
    public string OffTriggerConnection { get; set; } = "AND";

    [DefaultValue(true)]
    public bool RefreshOffTriggers { get; set; } = true;

    [DefaultValue("")]
    public string IgnoredMarkets { get; set; } = "";

    [DefaultValue("")]
    public string AllowedMarkets { get; set; } = "";

    [DefaultValue("")]
    public string IgnoredGlobalSettings { get; set; } = "";

    [DefaultValue("")]
    public string AllowedGlobalSettings { get; set; } = "";

    [DefaultValue(false)]
    public bool StopProcessWhenTriggered { get; set; } = false;
    public List<Trigger> Triggers { get; set; } = new List<Trigger>();
    public List<OffTrigger> OffTriggers { get; set; } = new List<OffTrigger>();
    public Dictionary<string, object> PairsProperties { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object> DCAProperties { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object> IndicatorsProperties { get; set; } = new Dictionary<string, object>();
  }

  public class Trigger {
    [DefaultValue("")]
    public string MarketTrendName { get; set; } = "";

    [DefaultValue("Relative")]
    public string MarketTrendRelation { get; set; } = "Relative";

    [DefaultValue(Constants.MaxTrendChange)]
    public double MaxChange { get; set; } = Constants.MaxTrendChange;

    [DefaultValue(Constants.MinTrendChange)]
    public double MinChange { get; set; } = Constants.MinTrendChange;

    [DefaultValue(Constants.Max24hVolume)]
    public double Max24hVolume { get; set; } = Constants.Max24hVolume;

    [DefaultValue(0.0)]
    public double Min24hVolume { get; set; } = 0.0;

    [DefaultValue(0)]
    public int AgeDaysLowerThan { get; set; } = 0;
  }

  public class OffTrigger {
    [DefaultValue("")]
    public string MarketTrendName { get; set; } = "";

    [DefaultValue("Relative")]
    public string MarketTrendRelation { get; set; } = "Relative";

    [DefaultValue(Constants.MaxTrendChange)]
    public double MaxChange { get; set; } = Constants.MaxTrendChange;

    [DefaultValue(Constants.MinTrendChange)]
    public double MinChange { get; set; } = Constants.MinTrendChange;

    [DefaultValue(Constants.Max24hVolume)]
    public double Max24hVolume { get; set; } = Constants.Max24hVolume;

    [DefaultValue(0.0)]
    public double Min24hVolume { get; set; } = 0.0;

    [DefaultValue(0)]
    public int HoursSinceTriggered { get; set; } = 0;
  }
  #endregion

  #region SecureSettings
  public class SecureSettings {
    public string MonitorPassword { get; set; } = "";
  }
  #endregion

  #endregion

  #region Market Analyzer Objects
  public class Market {
    public int Position;
    public string Name = "";
    public string Symbol = "";
    public double Volume24h = 0.0;
    public double Price = 0.0;
    public double TrendChange24h = 0.0;
    public double MainCurrencyPriceUSD = 0.0;
  }

  public class MarketTick {
    public double Volume24h = 0.0;
    public double Price = 0.0;
    public DateTime Time = Constants.confMinDate;
  }

  public class MarketTrendChange {
    public string MarketTrendName = "";
    public string Market = "";
    public double LastPrice = 0.0;
    public double Volume24h = 0.0;
    public double TrendMinutes = 0.0;
    public double TrendChange = 0.0;
    public DateTime TrendDateTime = Constants.confMinDate;
  }

  public class MarketInfo {
    public string Name = "";
    public DateTime FirstSeen = Constants.confMinDate;
    public DateTime LastSeen = Constants.confMaxDate;
  }
  #endregion

  #region Summary Objects
  public class Summary {
    public string Version { get; set; } = "";
    public DateTime LastRuntime { get; set; } = Constants.confMinDate;
    public int LastRuntimeSeconds { get; set; } = 0;
    public DateTime LastGlobalSettingSwitch { get; set; } = Constants.confMinDate;
    public GlobalSetting CurrentGlobalSetting { get; set; } = null;
    public GlobalSetting FloodProtectedSetting { get; set; } = null;
    public bool IsSOMActive { get; set; } = false;
    public Dictionary<string, MarketPairSummary> MarketSummary { get; set; } = new Dictionary<string, MarketPairSummary>();
    public Dictionary<string, List<MarketTrendChange>> MarketTrendChanges { get; set; } = new Dictionary<string, List<MarketTrendChange>>();
    public List<GlobalSettingSummary> GlobalSettingSummary { get; set; } = new List<DataObjects.PTMagicData.GlobalSettingSummary>();
    public double BuyValue { get; set; } = 0;
    public double TrailingBuy { get; set; } = 0;
    public double SellValue { get; set; } = 0;
    public double TrailingProfit { get; set; } = 0;
    public double MaxTradingPairs { get; set; } = 0;
    public double MaxCost { get; set; } = 0;
    public double MaxCostPercentage { get; set; } = 0;
    public double MinBuyVolume { get; set; } = 0;
    public double DCALevels { get; set; } = 0;
    public double DCATrigger { get; set; } = 0;
    public Dictionary<int, double> DCATriggers { get; set; } = new Dictionary<int, double>();
    public double DCAPercentage { get; set; } = 0;
    public Dictionary<int, double> DCAPercentages { get; set; } = new Dictionary<int, double>();
    public string DCABuyStrategy { get; set; } = "";
    public string BuyStrategy { get; set; } = "";
    public string SellStrategy { get; set; } = "";
    public string MainMarket { get; set; } = "";
    public double MainMarketPrice { get; set; } = 0;
    public string MainFiatCurrency { get; set; } = "USD";
    public double MainFiatCurrencyExchangeRate { get; set; } = 1;
    public int ProfitTrailerMajorVersion { get; set; } = 1;
    public List<StrategySummary> BuyStrategies { get; set; } = new List<StrategySummary>();
    public List<StrategySummary> SellStrategies { get; set; } = new List<StrategySummary>();
    public List<StrategySummary> DCABuyStrategies { get; set; } = new List<StrategySummary>();
    public List<StrategySummary> DCASellStrategies { get; set; } = new List<StrategySummary>();
  }

  public class StrategySummary {
    public string Name { get; set; } = "";
    public double Value { get; set; } = 0;
  }

  public class GlobalSettingSummary {
    public string SettingName { get; set; }
    public DateTime SwitchDateTime { get; set; }
    public int ActiveSeconds { get; set; } = 0;
    public Dictionary<string, MarketTrendChange> MarketTrendChanges { get; set; } = new Dictionary<string, MarketTrendChange>();
  }

  public class MarketPairSummary {
    public bool IsTradingEnabled { get; set; } = false;
    public bool IsSOMActive { get; set; } = false;
    public bool IsDCAEnabled { get; set; } = false;
    public List<SingleMarketSetting> ActiveSingleSettings { get; set; } = null;
    public double CurrentBuyValue { get; set; } = 0;
    public double CurrentTrailingBuy { get; set; } = 0;
    public double CurrentSellValue { get; set; } = 0;
    public double CurrentTrailingProfit { get; set; } = 0;
    public double LatestPrice { get; set; } = 0;
    public double Latest24hVolume { get; set; } = 0;
    public Dictionary<string, double> MarketTrendChanges { get; set; } = new Dictionary<string, double>();
    public List<StrategySummary> BuyStrategies { get; set; } = new List<StrategySummary>();
    public List<StrategySummary> SellStrategies { get; set; } = new List<StrategySummary>();
    public List<StrategySummary> DCABuyStrategies { get; set; } = new List<StrategySummary>();
    public List<StrategySummary> DCASellStrategies { get; set; } = new List<StrategySummary>();
  }
  #endregion

  #region Transaction Objects
  public class Transaction {
    public string GUID { get; set; } = "";
    public DateTime UTCDateTime { get; set; } = Constants.confMinDate;
    public double Amount { get; set; } = 0.0;

    public DateTime GetLocalDateTime(string offset) {
      DateTimeOffset result = this.UTCDateTime;

      // Convert UTC sales time to local offset time
      TimeSpan offsetTimeSpan = TimeSpan.Parse(offset.Replace("+", ""));
      result = result.ToOffset(offsetTimeSpan);

      return result.DateTime;
    }
  }
  #endregion

  #region SingleMarketSettingSummary Objects
  public class SingleMarketSettingSummary {
    public string Market { get; set; } = "";
    public DateTime ActivationDateTimeUTC { get; set; } = Constants.confMinDate;
    public SingleMarketSetting SingleMarketSetting { get; set; } = null;
    public TriggerSnapshot TriggerSnapshot { get; set; } = null;
  }

  public class TriggerSnapshot {
    public Dictionary<int, double> RelevantTriggers { get; set; } = new Dictionary<int, double>();
    public List<string> MatchedTriggersContent { get; set; } = new List<string>();
    public double LastPrice { get; set; } = 0;
    public double Last24hVolume { get; set; } = 0;
  }
  #endregion

  #region Profit Trailer JSON Objects

  public class PTData {
    public List<sellLogData> SellLogData { get; set; } = new List<sellLogData>();
    public List<dcaLogData> DCALogData { get; set; } = new List<dcaLogData>();
    public List<dcaLogData> GainLogData { get; set; } = new List<dcaLogData>();
    public List<buyLogData> bbBuyLogData { get; set; } = new List<buyLogData>();
  }

  public class sellLogData {
    public double soldAmount { get; set; }
    public SoldDate soldDate { get; set; }
    public int boughtTimes { get; set; }
    public string market { get; set; }
    public double profit { get; set; }
    public AverageCalculator averageCalculator { get; set; }
    public double currentPrice { get; set; }
  }

  public class SellLogData {
    public double SoldAmount { get; set; }
    public DateTime SoldDate { get; set; }
    public int BoughtTimes { get; set; }
    public string Market { get; set; }
    public double ProfitPercent { get; set; }
    public double Profit { get; set; }
    public double AverageBuyPrice { get; set; }
    public double TotalCost { get; set; }
    public double SoldPrice { get; set; }
    public double SoldValue { get; set; }
  }

  public class SoldDate {
    public Date date { get; set; }
    public Time time { get; set; }
  }

  public class FirstBoughtDate {
    public Date date { get; set; }
    public Time time { get; set; }
  }

  public class Date {
    public int year { get; set; }
    public int month { get; set; }
    public int day { get; set; }
  }

  public class Time {
    public int hour { get; set; }
    public int minute { get; set; }
    public int second { get; set; }
    public int nano { get; set; }
  }

  public class AverageCalculator {
    public double totalCost { get; set; }
    public double totalAmount { get; set; }
    public double totalAmountWithSold { get; set; }
    public double avgPrice { get; set; }
    public double avgCost { get; set; }
    public FirstBoughtDate firstBoughtDate { get; set; }
    public double totalWeightedPrice { get; set; }
    public double orderNumber { get; set; }
    public double fee { get; set; }
  }

  public class PTStrategy {
    public string type { get; set; }
    public string name { get; set; }
    public double entryValue { get; set; }
    public double entryValueLimit { get; set; }
    public double triggerValue { get; set; }
    public double currentValue { get; set; }
    public double currentValuePercentage { get; set; }
    public int decimals { get; set; }
    public string positive { get; set; }
  }

  public class dcaLogData {
    public int boughtTimes { get; set; } = 0;
    public string market { get; set; }
    public string positive { get; set; }
    public double buyProfit { get; set; }
    public double BBLow { get; set; }
    public double BBTrigger { get; set; }
    public double highbb { get; set; }
    public double profit { get; set; }
    public AverageCalculator averageCalculator { get; set; }
    public double currentPrice { get; set; }
    public string sellStrategy { get; set; }
    public string buyStrategy { get; set; }
    public double triggerValue { get; set; }
    public double percChange { get; set; }
    public List<PTStrategy> buyStrategies { get; set; }
    public List<PTStrategy> sellStrategies { get; set; }
  }

  public class Strategy {
    public string Type { get; set; }
    public string Name { get; set; }
    public double EntryValue { get; set; }
    public double EntryValueLimit { get; set; }
    public double TriggerValue { get; set; }
    public double CurrentValue { get; set; }
    public double CurrentValuePercentage { get; set; }
    public int Decimals { get; set; }
    public bool IsTrailing { get; set; }
    public bool IsTrue { get; set; }
  }

  public class DCALogData {
    public int BoughtTimes { get; set; }
    public double CurrentLowBBValue { get; set; }
    public double CurrentHighBBValue { get; set; }
    public double BBTrigger { get; set; }
    public double BuyTriggerPercent { get; set; }
    public bool IsTrailing { get; set; }
    public bool IsTrue { get; set; }
    public string Market { get; set; }
    public double ProfitPercent { get; set; }
    public double AverageBuyPrice { get; set; }
    public double TotalCost { get; set; }
    public double Amount { get; set; }
    public double CurrentPrice { get; set; }
    public double SellTrigger { get; set; }
    public double PercChange { get; set; }
    public DateTime FirstBoughtDate { get; set; }
    public string SellStrategy { get; set; }
    public string BuyStrategy { get; set; }
    public List<Strategy> BuyStrategies { get; set; } = new List<Strategy>();
    public List<Strategy> SellStrategies { get; set; } = new List<Strategy>();
  }

  public class buyLogData {
    public string market { get; set; }
    public string positive { get; set; }
    public double BBLow { get; set; }
    public double BBHigh { get; set; }
    public double BBTrigger { get; set; }
    public double profit { get; set; }
    public double currentPrice { get; set; }
    public double currentValue { get; set; }
    public string buyStrategy { get; set; }
    public double triggerValue { get; set; }
    public double percChange { get; set; }
    public List<PTStrategy> buyStrategies { get; set; }
  }

  public class BuyLogData {
    public double CurrentLowBBValue { get; set; }
    public double CurrentHighBBValue { get; set; }
    public double BBTrigger { get; set; }
    public bool IsTrailing { get; set; }
    public bool IsTrue { get; set; }
    public string Market { get; set; }
    public double ProfitPercent { get; set; }
    public double CurrentPrice { get; set; }
    public double CurrentValue { get; set; }
    public double TriggerValue { get; set; }
    public double PercChange { get; set; }
    public string BuyStrategy { get; set; }
    public List<Strategy> BuyStrategies { get; set; } = new List<Strategy>();
  }

  #endregion
}
