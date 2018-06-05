using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Linq;
using System.Reflection;
using Core.Main;
using Core.Helper;
using Core.Main.DataObjects.PTMagicData;
using Core.MarketAnalyzer;
using Core.ProfitTrailer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Core.Main {
  public class PTMagic {

    public PTMagic(LogHelper log) {
      this.Log = log;
    }

    #region Properties
    private LogHelper _log;
    private PTMagicConfiguration _systemConfiguration;
    private System.Timers.Timer _timer;
    private Summary _lastRuntimeSummary = null;
    private int _state = 0;
    private int _runCount = 0;
    private int _totalElapsedSeconds = 0;
    private int _profitTrailerMajorVersion = 0;
    private int _configCheckRetryCount = 0;
    private bool _configCheckResult = true;
    private bool _globalSettingWritten = false;
    private bool _singleMarketSettingWritten = false;
    private bool _enforceSettingsReapply = false;
    private DateTime _lastRuntime = Constants.confMinDate;
    private DateTime _lastSettingsChange = Constants.confMinDate;
    private DateTime _lastSettingFileCheck = Constants.confMinDate;
    private DateTime _lastVersionCheck = Constants.confMinDate;
    private DateTime _lastFiatCurrencyCheck = Constants.confMinDate;
    private string _lastSetting = "";
    private string _activeSetting = "";
    private string _defaultSettingName = "";
    private string _pairsFileName = "PAIRS.PROPERTIES";
    private string _dcaFileName = "DCA.PROPERTIES";
    private string _indicatorsFileName = "INDICATORS.PROPERTIES";
    private Version _currentVersion = null;
    private string _latestVersion = "";
    private string _lastMainFiatCurrency = "USD";
    private double _lastMainFiatCurrencyExchangeRate = 1;
    private List<SingleMarketSettingSummary> _singleMarketSettingSummaries = new List<SingleMarketSettingSummary>();
    private List<string> _pairsLines = null;
    private List<string> _dcaLines = null;
    private List<string> _indicatorsLines = null;
    private List<string> _exchangeMarketList = null;
    private List<string> _marketList = new List<string>();
    private Dictionary<string, MarketInfo> _marketInfos = new Dictionary<string, MarketInfo>();
    private Dictionary<string, double> _averageMarketTrendChanges = new Dictionary<string, double>();
    private Dictionary<string, List<MarketTrendChange>> _singleMarketTrendChanges = new Dictionary<string, List<MarketTrendChange>>();
    private Dictionary<string, List<MarketTrendChange>> _globalMarketTrendChanges = new Dictionary<string, List<MarketTrendChange>>();
    private Dictionary<string, int> _singleMarketSettingsCount = new Dictionary<string, int>();
    Dictionary<string, List<SingleMarketSetting>> _triggeredSingleMarketSettings = new Dictionary<string, List<SingleMarketSetting>>();

    public LogHelper Log {
      get {
        return _log;
      }
      set {
        _log = value;
      }
    }

    public PTMagicConfiguration PTMagicConfiguration {
      get {
        return _systemConfiguration;
      }
      set {
        _systemConfiguration = value;
      }
    }

    public System.Timers.Timer Timer {
      get {
        return _timer;
      }
      set {
        _timer = value;
      }
    }

    public Summary LastRuntimeSummary {
      get {
        return _lastRuntimeSummary;
      }
      set {
        _lastRuntimeSummary = value;
      }
    }

    public int State {
      get {
        return _state;
      }
      set {
        _state = value;
      }
    }

    public int RunCount {
      get {
        return _runCount;
      }
      set {
        _runCount = value;
      }
    }

    public int TotalElapsedSeconds {
      get {
        return _totalElapsedSeconds;
      }
      set {
        _totalElapsedSeconds = value;
      }
    }

    public int ProfitTrailerMajorVersion {
      get {
        return _profitTrailerMajorVersion;
      }
      set {
        _profitTrailerMajorVersion = value;
      }
    }

    public bool GlobalSettingWritten {
      get {
        return _globalSettingWritten;
      }
      set {
        _globalSettingWritten = value;
      }
    }

    public bool SingleMarketSettingWritten {
      get {
        return _singleMarketSettingWritten;
      }
      set {
        _singleMarketSettingWritten = value;
      }
    }

    public bool EnforceSettingsReapply {
      get {
        return _enforceSettingsReapply;
      }
      set {
        _enforceSettingsReapply = value;
      }
    }

    public DateTime LastSettingsChange {
      get {
        return _lastSettingsChange;
      }
      set {
        _lastSettingsChange = value;
      }
    }

    public DateTime LastVersionCheck {
      get {
        return _lastVersionCheck;
      }
      set {
        _lastVersionCheck = value;
      }
    }

    public DateTime LastFiatCurrencyCheck {
      get {
        return _lastFiatCurrencyCheck;
      }
      set {
        _lastFiatCurrencyCheck = value;
      }
    }

    public DateTime LastSettingFileCheck {
      get {
        return _lastSettingFileCheck;
      }
      set {
        _lastSettingFileCheck = value;
      }
    }

    public DateTime LastRuntime {
      get {
        return _lastRuntime;
      }
      set {
        _lastRuntime = value;
      }
    }

    public string DefaultSettingName {
      get {
        return _defaultSettingName;
      }
      set {
        _defaultSettingName = value;
      }
    }

    public string LastSetting {
      get {
        return _lastSetting;
      }
      set {
        _lastSetting = value;
      }
    }

    public string ActiveSetting {
      get {
        return _activeSetting;
      }
      set {
        _activeSetting = value;
      }
    }

    public string PairsFileName {
      get {
        return _pairsFileName;
      }
      set {
        _pairsFileName = value;
      }
    }

    public string DCAFileName {
      get {
        return _dcaFileName;
      }
      set {
        _dcaFileName = value;
      }
    }

    public string IndicatorsFileName {
      get {
        return _indicatorsFileName;
      }
      set {
        _indicatorsFileName = value;
      }
    }

    public Version CurrentVersion {
      get {
        return _currentVersion;
      }
      set {
        _currentVersion = value;
      }
    }

    public string LatestVersion {
      get {
        return _latestVersion;
      }
      set {
        _latestVersion = value;
      }
    }

    public string LastMainFiatCurrency {
      get {
        return _lastMainFiatCurrency;
      }
      set {
        _lastMainFiatCurrency = value;
      }
    }

    public double LastMainFiatCurrencyExchangeRate {
      get {
        return _lastMainFiatCurrencyExchangeRate;
      }
      set {
        _lastMainFiatCurrencyExchangeRate = value;
      }
    }

    public List<SingleMarketSettingSummary> SingleMarketSettingSummaries {
      get {
        return _singleMarketSettingSummaries;
      }
      set {
        _singleMarketSettingSummaries = value;
      }
    }

    public List<string> PairsLines {
      get {
        return _pairsLines;
      }
      set {
        _pairsLines = value;
      }
    }

    public List<string> DCALines {
      get {
        return _dcaLines;
      }
      set {
        _dcaLines = value;
      }
    }

    public List<string> IndicatorsLines {
      get {
        return _indicatorsLines;
      }
      set {
        _indicatorsLines = value;
      }
    }

    public List<string> ExchangeMarketList {
      get {
        return _exchangeMarketList;
      }
      set {
        _exchangeMarketList = value;
      }
    }

    public List<string> MarketList {
      get {
        return _marketList;
      }
      set {
        _marketList = value;
      }
    }

    public Dictionary<string, MarketInfo> MarketInfos {
      get {
        return _marketInfos;
      }
      set {
        _marketInfos = value;
      }
    }

    public Dictionary<string, List<MarketTrendChange>> SingleMarketTrendChanges {
      get {
        return _singleMarketTrendChanges;
      }
      set {
        _singleMarketTrendChanges = value;
      }
    }

    public Dictionary<string, List<MarketTrendChange>> GlobalMarketTrendChanges {
      get {
        return _globalMarketTrendChanges;
      }
      set {
        _globalMarketTrendChanges = value;
      }
    }

    public Dictionary<string, double> AverageMarketTrendChanges {
      get {
        return _averageMarketTrendChanges;
      }
      set {
        _averageMarketTrendChanges = value;
      }
    }

    public Dictionary<string, int> SingleMarketSettingsCount {
      get {
        return _singleMarketSettingsCount;
      }
      set {
        _singleMarketSettingsCount = value;
      }
    }

    public Dictionary<string, List<SingleMarketSetting>> TriggeredSingleMarketSettings {
      get {
        return _triggeredSingleMarketSettings;
      }
      set {
        _triggeredSingleMarketSettings = value;
      }
    }
    #endregion

    #region PTMagic Startup  Methods
    public bool StartProcess() {
      bool result = true;

      this.Log.DoLogInfo("");
      this.Log.DoLogInfo("  ██████╗ ████████╗    ███╗   ███╗ █████╗  ██████╗ ██╗ ██████╗");
      this.Log.DoLogInfo("  ██╔══██╗╚══██╔══╝    ████╗ ████║██╔══██╗██╔════╝ ██║██╔════╝");
      this.Log.DoLogInfo("  ██████╔╝   ██║       ██╔████╔██║███████║██║  ███╗██║██║     ");
      this.Log.DoLogInfo("  ██╔═══╝    ██║       ██║╚██╔╝██║██╔══██║██║   ██║██║██║     ");
      this.Log.DoLogInfo("  ██║        ██║       ██║ ╚═╝ ██║██║  ██║╚██████╔╝██║╚██████╗");
      this.Log.DoLogInfo("  ╚═╝        ╚═╝       ╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚═╝ ╚═════╝");
      this.Log.DoLogInfo("                         Version " + this.CurrentVersion.Major + "." + this.CurrentVersion.Minor + "." + this.CurrentVersion.Build);
      this.Log.DoLogInfo("");
      this.Log.DoLogInfo("Starting PTMagic in " + Directory.GetCurrentDirectory());
      this.Log.DoLogInfo("with .NET Core: " + Path.GetDirectoryName(typeof(object).Assembly.Location));

      if (!this.RunStartupChecks()) {
        return false;
      }
      if (!this.InitializeConfiguration()) {
        return false;
      }

      _configCheckResult = this.RunConfigurationChecks();
      if (!_configCheckResult) {
        this.Log.DoLogInfo("Starting configuration check retry in 5 seconds...");
        System.Timers.Timer configCheckTimer = new System.Timers.Timer(5000);
        configCheckTimer.Enabled = true;
        configCheckTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.ConfigCheckTimer_Elapsed);

        while (!_configCheckResult && _configCheckRetryCount < 10) {
          Thread.Sleep(100);
        }
        configCheckTimer.Stop();
      }

      if (!_configCheckResult) {
        return false;
      }

      this.LastSettingFileCheck = DateTime.Now;

      SettingsFiles.CheckPresets(this.PTMagicConfiguration, this.Log, true);

      this.StartPTMagicIntervalTimer();

      return result;
    }

    public bool RunStartupChecks() {
      bool result = true;

      // Startup checks
      if (!File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "settings.general.json")) {
        this.Log.DoLogError("File 'settings.general.json' not found! Please review the setup steps on the wiki and double check every step that involves copying files!");
        result = false;
      }

      if (!File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "settings.analyzer.json")) {
        this.Log.DoLogError("File 'settings.analyzer.json' not found! Please review the setup steps on the wiki and double check every step that involves copying files!");
        result = false;
      }

      return result;
    }

    public bool InitializeConfiguration() {
      bool result = true;

      try {
        this.PTMagicConfiguration = new PTMagicConfiguration();

        this.Log.DoLogInfo("Configuration loaded. Found " + this.PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.MarketTrends.Count.ToString() + " Market Trends, " + this.PTMagicConfiguration.AnalyzerSettings.GlobalSettings.Count.ToString() + " Global Settings and " + this.PTMagicConfiguration.AnalyzerSettings.SingleMarketSettings.Count.ToString() + " Single Market Settings.");

      } catch (Exception ex) {
        result = false;
        this.Log.DoLogCritical("Error loading configuration!", ex);
        throw (ex);
      }

      return result;
    }

    public bool RunConfigurationChecks() {
      bool result = true;

      // Check for valid default setting
      GlobalSetting defaultSetting = this.PTMagicConfiguration.AnalyzerSettings.GlobalSettings.Find(s => s.SettingName.Equals("default", StringComparison.InvariantCultureIgnoreCase));
      if (defaultSetting == null) {
        defaultSetting = this.PTMagicConfiguration.AnalyzerSettings.GlobalSettings.Find(s => s.SettingName.IndexOf("default", StringComparison.InvariantCultureIgnoreCase) > -1);
        if (defaultSetting != null) {
          this.Log.DoLogDebug("No setting named 'default' found, taking '" + defaultSetting.SettingName + "' as default.");
          this.DefaultSettingName = defaultSetting.SettingName;
        } else {
          this.Log.DoLogError("No 'default' setting found! Terminating process...");
          result = false;
        }
      } else {
        this.DefaultSettingName = defaultSetting.SettingName;
      }

      // Check if exchange is valid
      if (!this.PTMagicConfiguration.GeneralSettings.Application.Exchange.Equals("Binance", StringComparison.InvariantCultureIgnoreCase)
        && !this.PTMagicConfiguration.GeneralSettings.Application.Exchange.Equals("Bittrex", StringComparison.InvariantCultureIgnoreCase)
        && !this.PTMagicConfiguration.GeneralSettings.Application.Exchange.Equals("Poloniex", StringComparison.InvariantCultureIgnoreCase)) {
        this.Log.DoLogError("Exchange '" + this.PTMagicConfiguration.GeneralSettings.Application.Exchange + "' specified in settings.general.json is invalid! Terminating process...");
        result = false;
      }

      // Check if the program is enabled
      if (this.PTMagicConfiguration.GeneralSettings.Application.IsEnabled) {
        if (this.PTMagicConfiguration.GeneralSettings.Application.TestMode) this.Log.DoLogInfo("TESTMODE ENABLED - No files will be changed!");

        // Check for PT Directory
        DirectoryInfo ptRoot = new DirectoryInfo(this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath);
        if (ptRoot.Exists) {
          this.Log.DoLogInfo("Profit Trailer directory found");

          // Run checks dependant on PT version
          this.ProfitTrailerMajorVersion = this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerMajorVersion;
          if (this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerMajorVersion < 2) {
            result = RunProfitTrailerTradingFilesChecks();
          } else {
            result = RunProfitTrailerSettingsAPIChecks();
          }
        } else {
          this.Log.DoLogError("Profit Trailer directory not found (" + this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + ")");
          result = false;
        }
      } else {
        this.Log.DoLogWarn("PTMagic disabled, shutting down...");
        result = false;
      }

      return result;
    }

    private bool RunProfitTrailerTradingFilesChecks() {
      bool result = true;

      this.Log.DoLogInfo("========== STARTING CHECKS FOR Profit Trailer 1.x ==========");

      // Check for settings directory "trading"
      string ptTradingPath = this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar;
      DirectoryInfo ptTrading = new DirectoryInfo(ptTradingPath);
      if (ptTrading.Exists) {
        this.Log.DoLogInfo("Profit Trailer 1.x check: Trading directory found");

        #region File Checks
        this.Log.DoLogInfo("Profit Trailer 1.x check: Checking for Pairs Properties file");
        if (File.Exists(this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar + this.PairsFileName)) {
          this.Log.DoLogInfo("Profit Trailer 1.x check: PAIRS.PROPERTIES found!");
        } else {
          this.PairsFileName = "PAIRS.properties";
          if (File.Exists(this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar + this.PairsFileName)) {
            this.Log.DoLogInfo("Profit Trailer 1.x check: PAIRS.properties found!");
          } else {
            this.Log.DoLogError("Profit Trailer 1.x check: No 'PAIRS.properties' found in " + this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar);
            result = false;
          }
        }

        this.Log.DoLogInfo("Profit Trailer 1.x check: Checking for DCA Properties file");
        if (File.Exists(this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar + this.DCAFileName)) {
          this.Log.DoLogInfo("Profit Trailer 1.x check: DCA.PROPERTIES found!");
        } else {
          this.DCAFileName = "DCA.properties";
          if (File.Exists(this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar + this.DCAFileName)) {
            this.Log.DoLogInfo("Profit Trailer 1.x check: DCA.properties found!");
          } else {
            this.Log.DoLogError("Profit Trailer 1.x check: No 'DCA.properties' found in " + this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar);
            result = false;
          }
        }

        this.Log.DoLogInfo("Profit Trailer 1.x check: Checking for Indicators Properties file");
        if (File.Exists(this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar + this.IndicatorsFileName)) {
          this.Log.DoLogInfo("Profit Trailer 1.x check: INDICATORS.PROPERTIES found!");
        } else {
          this.IndicatorsFileName = "INDICATORS.properties";
          if (File.Exists(this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar + this.IndicatorsFileName)) {
            this.Log.DoLogInfo("Profit Trailer 1.x check: INDICATORS.properties found!");
          } else {
            this.Log.DoLogError("Profit Trailer 1.x check: No 'INDICATORS.properties' found in " + this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar);
            result = false;
          }
        }
        #endregion
      } else {
        this.Log.DoLogError("Profit Trailer 1.x check: Trading settings directory not found (" + ptTradingPath + ")");
        result = false;
      }
      
      if (result) {
        this.Log.DoLogInfo("========== CHECKS FOR Profit Trailer 1.x COMPLETED! ==========");
      } else {
         this.Log.DoLogInfo("========== CHECKS FOR Profit Trailer 1.x FAILED! ==========");
      }

      return result;
    }

    private bool RunProfitTrailerSettingsAPIChecks() {
      bool result = true;

      this.Log.DoLogInfo("========== STARTING CHECKS FOR Profit Trailer 2.x ==========");

      // Check for PT license key
      if (!this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerLicense.Equals("")) {
        this.Log.DoLogInfo("Profit Trailer 2.x check: Profit Trailer license found");
      } else {
        this.Log.DoLogError("Profit Trailer 2.x check: No Profit Trailer license key specified! The license key is necessary to adjust your Profit Trailer settings since 2.0");
        result = false;
      }

      // Check for PT default setting key
      if (!this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerDefaultSettingName.Equals("")) {
        this.Log.DoLogInfo("Profit Trailer 2.x check: Profit Trailer default setting name specified");
      } else {
        this.Log.DoLogError("Profit Trailer 2.x check: No Profit Trailer default setting name specified! The default setting name is necessary to adjust your Profit Trailer settings since 2.0");
        result = false;
      }

      // Check for PT monitor
      if (!this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerMonitorURL.Equals("")) {
        this.Log.DoLogInfo("Profit Trailer 2.x check: Profit Trailer monitor URL found");
      } else {
        this.Log.DoLogError("Profit Trailer 2.x check: No Profit Trailer monitor URL specified! The monitor URL is necessary to adjust your Profit Trailer settings since 2.0");
        result = false;
      }

      // Check if PT monitor is reachable
      if (SystemHelper.UrlIsReachable(this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerMonitorURL)) {
        this.Log.DoLogInfo("Profit Trailer 2.x check: Profit Trailer monitor connection test succeeded");
      } else {
        this.Log.DoLogError("Profit Trailer 2.x check: Your Profit Trailer monitor (" + this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerMonitorURL + ") is not available! Make sure your Profit Trailer bot is up and running and your monitor is accessible.");
        result = false;
      }

      if (result) {
        this.Log.DoLogInfo("========== CHECKS FOR Profit Trailer 2.x COMPLETED! ==========");
      } else {
         this.Log.DoLogInfo("========== CHECKS FOR Profit Trailer 2.x FAILED! ==========");
      }

      return result;
    }

    public void StartPTMagicIntervalTimer() {
      this.Timer = new System.Timers.Timer(this.PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.IntervalMinutes * 60 * 1000);
      this.Timer.Enabled = true;
      this.Timer.Elapsed += new System.Timers.ElapsedEventHandler(this.PTMagicIntervalTimer_Elapsed);

      this.Log.DoLogInfo("Checking market trends every " + this.PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.IntervalMinutes.ToString() + " minutes...");

      // Fire the first start immediately
      this.PTMagicIntervalTimer_Elapsed(null, null);
    }
    #endregion

    #region PTMagic Interval Methods
    public void ConfigCheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
      // Reinit config in case the user changed something
      this.InitializeConfiguration();
      _configCheckResult = this.RunConfigurationChecks();
      _configCheckRetryCount++;
      if (!_configCheckResult) {
        this.Log.DoLogError("Configuration check retry " + _configCheckRetryCount + "/10 failed, starting next retry in 5 seconds...");
      }
    }

    public void PTMagicIntervalTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
      // Check if the bot is idle
      if (this.State == Constants.PTMagicBotState_Idle) {
        this.RunCount++;

        bool headerLinesAdded = false;
        this.EnforceSettingsReapply = this.HaveSettingsChanged();

        if (PTMagicConfiguration.GeneralSettings.Application.IsEnabled) {

          // Validate settings
          this.ValidateSettings();

          // Start the process
          this.Log.DoLogInfo("");
          this.Log.DoLogInfo("##########################################################");
          this.Log.DoLogInfo("#********************************************************#");
          this.Log.DoLogInfo("Starting market trend check with Version " + this.CurrentVersion.Major + "." + this.CurrentVersion.Minor + "." + this.CurrentVersion.Build);

          // Change state to "Running"
          this.State = Constants.PTMagicBotState_Running;

          this.LastRuntime = DateTime.Now;

          this.LastRuntimeSummary = new Summary();
          this.LastRuntimeSummary.LastRuntime = this.LastRuntime;
          this.LastRuntimeSummary.Version = this.CurrentVersion.Major.ToString() + "." + this.CurrentVersion.Minor.ToString() + "." + this.CurrentVersion.Build.ToString();

          // Check for latest GitHub version
          this.CheckLatestGitHubVersion(this.LastRuntimeSummary.Version);

          // Get latest main fiat currency exchange rate
          this.GetMainFiatCurrencyDetails();

          // Get current PT properties
          string pairsPropertiesPath = this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar + this.PairsFileName;
          string dcaPropertiesPath = this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar + this.DCAFileName;
          string indicatorsPropertiesPath = this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar + this.IndicatorsFileName;

          this.LoadCurrentProfitTrailerProperties(pairsPropertiesPath, dcaPropertiesPath, indicatorsPropertiesPath);

          // Loading SMS Summaries
          this.LoadSMSSummaries();

          // Get saved market info
          this.MarketInfos = BaseAnalyzer.GetMarketInfosFromFile(this.PTMagicConfiguration, this.Log);

          // Build exchange market data
          this.BuildMarketData();

          // Get markets from PT properties
          this.BuildMarketList();
          this.ValidateMarketList();

          // Build global market trends configured in settings
          this.BuildGlobalMarketTrends();

          // Check for global settings triggers
          GlobalSetting triggeredSetting = this.PTMagicConfiguration.AnalyzerSettings.GlobalSettings.Find(s => s.SettingName.Equals(this.DefaultSettingName, StringComparison.InvariantCultureIgnoreCase));
          List<string> matchedTriggers = new List<string>();
          this.CheckGlobalSettingsTriggers(ref triggeredSetting, ref matchedTriggers);

          // Activate global setting
          this.ActivateSetting(ref headerLinesAdded, ref triggeredSetting, ref matchedTriggers);

          // Check for single market trend triggers
          this.ApplySingleMarketSettings();

          // Save new properties to Profit Trailer
          this.SaveProfitTrailerProperties(headerLinesAdded, pairsPropertiesPath, dcaPropertiesPath, indicatorsPropertiesPath);

          // Save Single Market Settings Summary
          this.SaveSingleMarketSettingsSummary();

          // Save Runtime Summary
          this.SaveRuntimeSummary(headerLinesAdded);

          // Cleanup to free memory in between intervals
          this.Cleanup();

          // Change state to Finished / Stopped
          this.State = Constants.PTMagicBotState_Idle;
        } else {
          this.State = Constants.PTMagicBotState_Idle;
          Log.DoLogWarn("PTMagic disabled, shutting down until next raid...");
        }
      } else {
        if (this.RunCount > 1) {
          Log.DoLogWarn("PTMagic already raiding since " + this.LastRuntime.ToString() + " - Process frozen? Checking things...");

          if (File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + "LastRuntimeSummary.json")) {
            FileInfo fiLastSummary = new FileInfo(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + "LastRuntimeSummary.json");
            if (fiLastSummary.LastWriteTime < DateTime.Now.AddMinutes(-(this.PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.IntervalMinutes * 2))) {
              Log.DoLogWarn("PTMagic seems to have frozen after raid " + this.RunCount.ToString() + ", but don't worry I will sacrifice some Magicbots to get this running again...");
              this.State = Constants.PTMagicBotState_Idle;
              Log.DoLogInfo("PTMagic status resetted, waiting for the next raid to be good to go again.");
            }
          } else {
            Log.DoLogWarn("No LastRuntimeSummary.json found after raid " + this.RunCount.ToString() + ", trying to reset PT Magic status...");
            this.State = Constants.PTMagicBotState_Idle;
            Log.DoLogInfo("PTMagic status resetted, waiting for the next raid to be good to go again.");
          }
        }
      }
    }

    private bool HaveSettingsChanged() {
      bool result = false;

      FileInfo generalSettingsFile = new FileInfo(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "settings.general.json");
      FileInfo analyzerSettingsFile = new FileInfo(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "settings.analyzer.json");
      if (generalSettingsFile.LastWriteTime > this.LastSettingFileCheck || analyzerSettingsFile.LastWriteTime > this.LastSettingFileCheck) {
        Log.DoLogInfo("Detected configuration changes. Reloading settings...");

        try {
          PTMagicConfiguration = new PTMagicConfiguration();

          GlobalSetting defaultSetting = this.PTMagicConfiguration.AnalyzerSettings.GlobalSettings.Find(s => s.SettingName.Equals("default", StringComparison.InvariantCultureIgnoreCase));
          if (defaultSetting == null) {
            defaultSetting = this.PTMagicConfiguration.AnalyzerSettings.GlobalSettings.Find(s => s.SettingName.IndexOf("default", StringComparison.InvariantCultureIgnoreCase) > -1);
            if (defaultSetting != null) {
              Log.DoLogDebug("No setting named 'default' found, taking '" + defaultSetting.SettingName + "' as default.");
              this.DefaultSettingName = defaultSetting.SettingName;
            } else {
              Log.DoLogError("No 'default' setting found! Terminating process...");
              this.Timer.Stop();
              Exception ex = new Exception("No 'default' setting found!Terminating process...");
              throw ex;
            }
          } else {
            this.DefaultSettingName = defaultSetting.SettingName;
          }

          Log.DoLogInfo("New configuration reloaded.");
          this.LastSettingFileCheck = DateTime.Now;
          result = true;

          if (this.Timer.Interval != this.PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.IntervalMinutes * 60 * 1000) {
            Log.DoLogInfo("Setting for 'IntervalMinutes' changed in MarketAnalyzer, setting new timer...");
            this.Timer.Stop();
            this.Timer.Interval = this.PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.IntervalMinutes * 60 * 1000;
            this.Timer.Start();
            Log.DoLogInfo("New timer set to " + this.PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.IntervalMinutes.ToString() + " minutes.");
          }

          SettingsFiles.CheckPresets(this.PTMagicConfiguration, this.Log, true);
        } catch (Exception ex) {
          Log.DoLogCritical("Error loading new configuration!", ex);
        }
      } else {
        result = SettingsFiles.CheckPresets(this.PTMagicConfiguration, this.Log, false);
      }

      return result;
    }

    private void ValidateSettings() {
      // Check for a valid exchange
      if (this.PTMagicConfiguration.GeneralSettings.Application.Exchange == null) {
        Log.DoLogError("Your setting for Application.Exchange in settings.general.json is invalid (null)! Terminating process.");
        this.Timer.Stop();
        Exception ex = new Exception("Your setting for Application.Exchange in settings.general.json is invalid (null)! Terminating process.");
        throw ex;
      } else {
        if (this.PTMagicConfiguration.GeneralSettings.Application.Exchange.Equals("")) {
          Log.DoLogError("Your setting for Application.Exchange in settings.general.json is invalid (empty)! Terminating process.");
          this.Timer.Stop();
          Exception ex = new Exception("Your setting for Application.Exchange in settings.general.json is invalid (empty)! Terminating process.");
          throw ex;
        } else {
          if (!this.PTMagicConfiguration.GeneralSettings.Application.Exchange.Equals("Binance", StringComparison.InvariantCultureIgnoreCase) && !this.PTMagicConfiguration.GeneralSettings.Application.Exchange.Equals("Bittrex", StringComparison.InvariantCultureIgnoreCase) && !this.PTMagicConfiguration.GeneralSettings.Application.Exchange.Equals("Poloniex", StringComparison.InvariantCultureIgnoreCase)) {
            Log.DoLogError("Your setting for Application.Exchange in settings.general.json is invalid (" + this.PTMagicConfiguration.GeneralSettings.Application.Exchange + ")! Terminating process.");
            this.Timer.Stop();
            Exception ex = new Exception("Your setting for Application.Exchange in settings.general.json is invalid (" + this.PTMagicConfiguration.GeneralSettings.Application.Exchange + ")! Terminating process.");
            throw ex;
          }
        }
      }
    }

    private void CheckLatestGitHubVersion(string currentVersion) {
      // Get latest version number
      if (this.LastVersionCheck < DateTime.Now.AddMinutes(-30)) {
        this.LatestVersion = BaseAnalyzer.GetLatestGitHubRelease(this.Log, currentVersion);
        this.LastVersionCheck = DateTime.Now;
        if (!SystemHelper.IsRecentVersion(currentVersion, this.LatestVersion)) {
          this.Log.DoLogWarn("Your bot is out of date! The most recent version of PTMagic is " + this.LatestVersion);
        }
      }
    }

    private void GetMainFiatCurrencyDetails() {
      this.LastRuntimeSummary.MainFiatCurrency = this.LastMainFiatCurrency;
      this.LastRuntimeSummary.MainFiatCurrencyExchangeRate = this.LastMainFiatCurrencyExchangeRate;

      if (this.LastFiatCurrencyCheck < DateTime.Now.AddHours(-12) && !this.PTMagicConfiguration.GeneralSettings.Application.MainFiatCurrency.Equals("USD", StringComparison.InvariantCultureIgnoreCase)) {
        try {
          this.LastRuntimeSummary.MainFiatCurrency = this.PTMagicConfiguration.GeneralSettings.Application.MainFiatCurrency;
          this.LastRuntimeSummary.MainFiatCurrencyExchangeRate = BaseAnalyzer.GetMainFiatCurrencyRate(this.PTMagicConfiguration.GeneralSettings.Application.MainFiatCurrency, this.Log);
          this.LastMainFiatCurrency = this.LastRuntimeSummary.MainFiatCurrency;
          this.LastMainFiatCurrencyExchangeRate = this.LastRuntimeSummary.MainFiatCurrencyExchangeRate;

          this.LastFiatCurrencyCheck = DateTime.Now;
        } catch (Exception ex) {

          // Fallback to USD in case something went wrong
          this.Log.DoLogError("Fixer.io exchange rate check error: " + ex.Message);
          this.LastRuntimeSummary.MainFiatCurrency = "USD";
          this.LastRuntimeSummary.MainFiatCurrencyExchangeRate = 1;
          this.LastMainFiatCurrency = "USD";
          this.LastMainFiatCurrencyExchangeRate = 1;
        }
      }
    }

    private void LoadCurrentProfitTrailerProperties(string pairsPropertiesPath, string dcaPropertiesPath, string indicatorsPropertiesPath) {
      if (this.ProfitTrailerMajorVersion == 1) {
        // Load current PT properties from files (Valid for PT 1.x)
        this.Log.DoLogInfo("Loading current Profit Trailer properties from files...");

        this.PairsLines = File.ReadLines(pairsPropertiesPath).ToList();
        this.DCALines = File.ReadLines(dcaPropertiesPath).ToList();
        this.IndicatorsLines = File.ReadLines(indicatorsPropertiesPath).ToList();
      } else {
        // Load current PT properties from API (Valid for PT 2.x and above)
        this.Log.DoLogInfo("Loading current Profit Trailer properties from API...");

        this.PairsLines = SettingsAPI.GetPropertyLinesFromAPI("PAIRS", this.PTMagicConfiguration, this.Log);
        this.DCALines = SettingsAPI.GetPropertyLinesFromAPI("DCA", this.PTMagicConfiguration, this.Log);
        this.IndicatorsLines = SettingsAPI.GetPropertyLinesFromAPI("INDICATORS", this.PTMagicConfiguration, this.Log);
      }

      if (this.PairsLines != null && this.DCALines != null && this.IndicatorsLines != null) {
        this.Log.DoLogInfo("Properties loaded - P (" + this.PairsLines.Count.ToString() + " lines) - D (" + this.DCALines.Count.ToString() + " lines) - I (" + this.IndicatorsLines.Count.ToString() + " lines).");
      } else {
        this.Log.DoLogError("Unable to load all Profit Trailer properties! Waiting for the next interval to retry...");
        Exception ex = new Exception("Unable to load all Profit Trailer properties! Waiting for the next interval to retry...");
        this.State = 0;
        throw ex;
      }

      // Get market from PT properties
      this.LastRuntimeSummary.MainMarket = SettingsHandler.GetMainMarket(this.PTMagicConfiguration, this.PairsLines, this.Log);

      this.LastRuntimeSummary.ProfitTrailerMajorVersion = this.ProfitTrailerMajorVersion;
    }

    private void LoadSMSSummaries() {
      this.Log.DoLogInfo("Loading Single Market Setting Summaries...");
      this.SingleMarketSettingSummaries = new List<SingleMarketSettingSummary>();
      if (File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + "SingleMarketSettingSummary.json")) {
        try {
          Dictionary<string, bool> smsVerificationResult = new Dictionary<string, bool>();
          // Cleanup SMS Summaries in case a SMS got removed
          foreach (SingleMarketSettingSummary smsSummary in JsonConvert.DeserializeObject<List<SingleMarketSettingSummary>>(System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + "SingleMarketSettingSummary.json"))) {
            string smsName = smsSummary.SingleMarketSetting.SettingName;
            bool smsIsValid = false;
            if (smsVerificationResult.ContainsKey(smsName)) {
              smsIsValid = smsVerificationResult[smsName];
            } else {
              SingleMarketSetting sms = this.PTMagicConfiguration.AnalyzerSettings.SingleMarketSettings.Find(s => s.SettingName.Equals(smsName));
              if (sms != null) {
                smsIsValid = true;
                smsVerificationResult.Add(smsName, true);
              } else {
                smsVerificationResult.Add(smsName, false);
              }
            }

            if (smsIsValid) {
              this.SingleMarketSettingSummaries.Add(smsSummary);
            }
          }

          this.Log.DoLogInfo("Single Market Setting Summaries loaded.");
        } catch { }
      }
    }

    private void BuildMarketData() {
      // Get most recent market data from CMC
      string cmcMarketDataResult = CoinMarketCap.GetMarketData(this.PTMagicConfiguration, this.Log);

      if (this.PTMagicConfiguration.GeneralSettings.Application.Exchange.Equals("Bittrex", StringComparison.InvariantCultureIgnoreCase)) {

        // Get most recent market data from Bittrex
        this.ExchangeMarketList = Bittrex.GetMarketData(this.LastRuntimeSummary.MainMarket, this.MarketInfos, this.PTMagicConfiguration, this.Log);
      } else if (this.PTMagicConfiguration.GeneralSettings.Application.Exchange.Equals("Binance", StringComparison.InvariantCultureIgnoreCase)) {

        // Get most recent market data from Binance
        this.ExchangeMarketList = Binance.GetMarketData(this.LastRuntimeSummary.MainMarket, this.MarketInfos, this.PTMagicConfiguration, this.Log);
      } else if (this.PTMagicConfiguration.GeneralSettings.Application.Exchange.Equals("Poloniex", StringComparison.InvariantCultureIgnoreCase)) {

        // Get most recent market data from Poloniex
        this.ExchangeMarketList = Poloniex.GetMarketData(this.LastRuntimeSummary.MainMarket, this.MarketInfos, this.PTMagicConfiguration, this.Log);
      }

      // Check if problems occured during the Exchange contact
      if (this.ExchangeMarketList == null) {
        Exception ex = new Exception("Unable to contact " + this.PTMagicConfiguration.GeneralSettings.Application.Exchange + " for fresh market data. Trying again in " + this.PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.IntervalMinutes + " minute(s).");
        Log.DoLogError(ex.Message);
        this.State = Constants.PTMagicBotState_Idle;
        throw ex;
      }
    }

    private void BuildMarketList() {
      string marketPairs = SettingsHandler.GetMarketPairs(this.PTMagicConfiguration, this.PairsLines, this.Log);
      if (marketPairs.ToLower().Equals("all") || marketPairs.ToLower().Equals("false") || marketPairs.ToLower().Equals("true") || marketPairs.Equals("")) {
        this.MarketList = this.ExchangeMarketList;
      } else {
        if (this.ProfitTrailerMajorVersion == 1) {
          this.MarketList = SystemHelper.ConvertTokenStringToList(marketPairs, ",");
        } else {
          // Since PT 2.0 the main market is no longer included in the market list so we need to rebuild the list
          List<string> originalMarketList = SystemHelper.ConvertTokenStringToList(marketPairs, ",");
          foreach (string market in originalMarketList) {
            this.MarketList.Add(SystemHelper.GetFullMarketName(this.LastRuntimeSummary.MainMarket, market, this.PTMagicConfiguration.GeneralSettings.Application.Exchange));
          }
        }
      }
    }

    private void ValidateMarketList() {
      // Check if markets are valid for the selected main market
      List<string> validMarkets = this.MarketList.FindAll(m => m.IndexOf(this.LastRuntimeSummary.MainMarket, StringComparison.InvariantCultureIgnoreCase) > -1);
      if (validMarkets.Count == 0) {
        Exception ex = new Exception("No valid pairs found for main market '" + this.LastRuntimeSummary.MainMarket + "' in configured pars list (" + SystemHelper.ConvertListToTokenString(this.MarketList, ",", true) + ")! Terminating process...");
        Log.DoLogError(ex.Message);
        this.State = Constants.PTMagicBotState_Idle;
        this.Timer.Stop();
        throw ex;
      }
    }

    private void BuildGlobalMarketTrends() {
      this.Log.DoLogInfo("Build global market trends...");
      this.SingleMarketTrendChanges = BaseAnalyzer.BuildMarketTrends("Exchange", this.LastRuntimeSummary.MainMarket, this.MarketList, "Volume", false, new Dictionary<string, List<MarketTrendChange>>(), this.PTMagicConfiguration, this.Log);
      this.GlobalMarketTrendChanges = new Dictionary<string, List<MarketTrendChange>>();

      // CoinMarketCap
      this.GlobalMarketTrendChanges = BaseAnalyzer.BuildMarketTrends("CoinMarketCap", this.LastRuntimeSummary.MainMarket, new List<string>(), "", true, this.GlobalMarketTrendChanges, this.PTMagicConfiguration, this.Log);

      // Bittrex
      foreach (MarketTrend marketTrend in this.PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.MarketTrends.FindAll(mt => mt.Platform.Equals("Exchange", StringComparison.InvariantCultureIgnoreCase))) {
        if (this.SingleMarketTrendChanges.ContainsKey(marketTrend.Name)) {
          int maxMarkets = this.SingleMarketTrendChanges[marketTrend.Name].Count;
          if (marketTrend.MaxMarkets > 0 && marketTrend.MaxMarkets <= this.SingleMarketTrendChanges[marketTrend.Name].Count) {
            maxMarkets = marketTrend.MaxMarkets;
          }

          this.GlobalMarketTrendChanges.Add(marketTrend.Name, this.SingleMarketTrendChanges[marketTrend.Name].Take(maxMarkets).ToList());
        }
      }

      this.AverageMarketTrendChanges = BaseAnalyzer.BuildGlobalMarketTrends(this.GlobalMarketTrendChanges, this.PTMagicConfiguration, this.Log);

      this.Log.DoLogInfo("Global market trends built.");
    }

    private void CheckGlobalSettingsTriggers(ref GlobalSetting triggeredSetting, ref List<string> matchedTriggers) {
      this.Log.DoLogInfo("Checking global settings triggers...");
      foreach (GlobalSetting globalSetting in this.PTMagicConfiguration.AnalyzerSettings.GlobalSettings) {
        // Reset triggers for each setting
        matchedTriggers = new List<string>();

        if (globalSetting.Triggers.Count > 0) {
          this.Log.DoLogInfo("Checking triggers for '" + globalSetting.SettingName + "'...");
          List<bool> triggerResults = new List<bool>();
          foreach (Trigger trigger in globalSetting.Triggers) {
            MarketTrend marketTrend = this.PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.MarketTrends.Find(mt => mt.Name == trigger.MarketTrendName);
            if (marketTrend != null) {

              // Get market trend change for trigger
              if (this.AverageMarketTrendChanges.ContainsKey(marketTrend.Name)) {
                double averageMarketTrendChange = this.AverageMarketTrendChanges[marketTrend.Name];
                if (averageMarketTrendChange >= trigger.MinChange && averageMarketTrendChange < trigger.MaxChange) {

                  // Trigger met!
                  this.Log.DoLogInfo("Trigger '" + trigger.MarketTrendName + "' triggered! TrendChange = " + averageMarketTrendChange.ToString("#,#0.00", new System.Globalization.CultureInfo("en-US")) + "%");

                  string triggerContent = trigger.MarketTrendName + " - ";
                  if (trigger.MinChange != Constants.MinTrendChange) {
                    triggerContent += " - Min: " + trigger.MinChange.ToString("#,#0.00", new System.Globalization.CultureInfo("en-US")) + "%";
                  }

                  if (trigger.MaxChange != Constants.MaxTrendChange) {
                    triggerContent += " - Max: " + trigger.MaxChange.ToString("#,#0.00", new System.Globalization.CultureInfo("en-US")) + "%";
                  }

                  matchedTriggers.Add(triggerContent);

                  triggerResults.Add(true);
                } else {
                  this.Log.DoLogDebug("Trigger '" + trigger.MarketTrendName + "' not triggered. TrendChange = " + averageMarketTrendChange.ToString("#,#0.00", new System.Globalization.CultureInfo("en-US")) + "%");
                  triggerResults.Add(false);
                }
              } else {
                this.Log.DoLogError("Trigger '" + trigger.MarketTrendName + "' not found in this.AverageMarketTrendChanges[] (" + SystemHelper.ConvertListToTokenString(this.AverageMarketTrendChanges.Keys.ToList(), ",", true) + "). Unable to load recent trends?");
                triggerResults.Add(false);
              }
            } else {
              this.Log.DoLogWarn("Market Trend '" + trigger.MarketTrendName + "' not found! Trigger ignored!");
              triggerResults.Add(false);
            }
          }

          // Check if all triggers have to get triggered or just one
          bool settingTriggered = false;
          switch (globalSetting.TriggerConnection.ToLower()) {
            case "and":
              settingTriggered = triggerResults.FindAll(tr => tr == false).Count == 0;
              break;
            case "or":
              settingTriggered = triggerResults.FindAll(tr => tr == true).Count > 0;
              break;
          }

          // Setting got triggered -> Activate it!
          if (settingTriggered) {
            triggeredSetting = globalSetting;
            break;
          }
        }
      }
    }

    private void ActivateSetting(ref bool headerLinesAdded, ref GlobalSetting triggeredSetting, ref List<string> matchedTriggers) {
      string activeSettingName = SettingsHandler.GetActiveSetting(this, ref headerLinesAdded);
      if (activeSettingName.Equals("") && this.PTMagicConfiguration.GeneralSettings.Application.TestMode) {
        activeSettingName = this.ActiveSetting;
      }

      if (this.EnforceSettingsReapply) {
        this.Log.DoLogInfo("Reapply '" + activeSettingName + "' as the settings.analyzer.json or a preset file got changed.");
      }

      GlobalSetting activeSetting = this.PTMagicConfiguration.AnalyzerSettings.GlobalSettings.Find(s => s.SettingName.Equals(activeSettingName, StringComparison.InvariantCultureIgnoreCase));
      if (this.EnforceSettingsReapply || !activeSettingName.Equals(triggeredSetting.SettingName, StringComparison.InvariantCultureIgnoreCase)) {
        this.Log.DoLogInfo("Setting '" + activeSettingName + "' currently active. Checking for flood protection...");

        // If the setting we are about to activate is the default one, do not list matched triggers
        if (triggeredSetting.SettingName.Equals(this.DefaultSettingName, StringComparison.InvariantCultureIgnoreCase)) {
          matchedTriggers = new List<string>();
        }

        // Check if flood protection is active
        if (this.EnforceSettingsReapply || !this.LastSetting.Equals(triggeredSetting.SettingName, StringComparison.InvariantCultureIgnoreCase) || this.LastSettingsChange <= DateTime.UtcNow.AddMinutes(-PTMagicConfiguration.GeneralSettings.Application.FloodProtectionMinutes)) {

          // Setting not set => Change setting
          this.Log.DoLogInfo("Switching global settings to '" + triggeredSetting.SettingName + "'...");
          SettingsHandler.CompileProperties(this, triggeredSetting);
          this.GlobalSettingWritten = true;
          this.Log.DoLogInfo("Setting '" + triggeredSetting.SettingName + "' now active!");

          this.LastRuntimeSummary.LastGlobalSettingSwitch = this.LastRuntimeSummary.LastRuntime;
          this.LastRuntimeSummary.CurrentGlobalSetting = triggeredSetting;

          // Build Telegram message
          string telegramMessage = this.PTMagicConfiguration.GeneralSettings.Application.InstanceName + ": Setting switched to '*" + SystemHelper.SplitCamelCase(triggeredSetting.SettingName) + "*'.";

          if (matchedTriggers.Count > 0) {
            telegramMessage += "\n\n*Matching Triggers:*";
            foreach (string triggerResult in matchedTriggers) {
              telegramMessage += "\n" + triggerResult;
            }
          }

          if (this.AverageMarketTrendChanges.Keys.Count > 0) {
            telegramMessage += "\n\n*Market Trends:*";
            foreach (string key in this.AverageMarketTrendChanges.Keys) {
              telegramMessage += "\n" + key + ": " + this.AverageMarketTrendChanges[key].ToString("#,#0.00", new System.Globalization.CultureInfo("en-US")) + "%";
            }
          }

          // Send Telegram message
          if (this.PTMagicConfiguration.GeneralSettings.Telegram.IsEnabled) TelegramHelper.SendMessage(this.PTMagicConfiguration.GeneralSettings.Telegram.BotToken, this.PTMagicConfiguration.GeneralSettings.Telegram.ChatId, telegramMessage, this.PTMagicConfiguration.GeneralSettings.Telegram.SilentMode, this.Log);

          this.LastSetting = activeSettingName;
          this.LastSettingsChange = DateTime.UtcNow;

        } else {
          this.Log.DoLogInfo("Flood protection active until " + this.LastSettingsChange.AddMinutes(PTMagicConfiguration.GeneralSettings.Application.FloodProtectionMinutes).ToString() + " (UTC). Not switching settings to '" + triggeredSetting.SettingName + "'!");

          this.LastRuntimeSummary.FloodProtectedSetting = triggeredSetting;
          this.LastRuntimeSummary.CurrentGlobalSetting = activeSetting;
        }
      } else {
        matchedTriggers = new List<string>();

        // Setting already set => Do nothing
        this.Log.DoLogInfo("Setting '" + triggeredSetting.SettingName + "' already active. No action taken.");

        this.LastRuntimeSummary.CurrentGlobalSetting = triggeredSetting;
      }

      this.ActiveSetting = this.LastRuntimeSummary.CurrentGlobalSetting.SettingName;
    }

    private void ApplySingleMarketSettings() {
      if (this.PTMagicConfiguration.AnalyzerSettings.SingleMarketSettings.Count > 0) {
        this.Log.DoLogInfo("Checking single market settings triggers for " + this.MarketList.Count.ToString() + " markets...");

        int marketPairProcess = 1;
        Dictionary<string, List<string>> matchedMarketTriggers = new Dictionary<string, List<string>>();
        foreach (string marketPair in this.MarketList) {
          this.Log.DoLogDebug("'" + marketPair + "' - Checking triggers (" + marketPairProcess.ToString() + "/" + this.MarketList.Count.ToString() + ")...");

          bool stopTriggers = false;
          foreach (SingleMarketSetting marketSetting in this.PTMagicConfiguration.AnalyzerSettings.SingleMarketSettings) {
            List<string> matchedSingleMarketTriggers = new List<string>();

            // Check ignore markets
            List<string> ignoredMarkets = SystemHelper.ConvertTokenStringToList(marketSetting.IgnoredMarkets, ",");
            if (ignoredMarkets.Contains(marketPair)) {
              this.Log.DoLogDebug("'" + marketPair + "' - Is ignored in '" + marketSetting.SettingName + "'.");
              continue;
            }

            // Check allowed markets
            List<string> allowedMarkets = SystemHelper.ConvertTokenStringToList(marketSetting.AllowedMarkets, ",");
            if (allowedMarkets.Count > 0 && !allowedMarkets.Contains(marketPair)) {
              this.Log.DoLogDebug("'" + marketPair + "' - Is not allowed in '" + marketSetting.SettingName + "'.");
              continue;
            }

            // Check ignore global settings
            List<string> ignoredGlobalSettings = SystemHelper.ConvertTokenStringToList(marketSetting.IgnoredGlobalSettings, ",");
            if (ignoredGlobalSettings.Contains(this.ActiveSetting)) {
              this.Log.DoLogDebug("'" + marketPair + "' - '" + this.ActiveSetting + "' - Is ignored in '" + marketSetting.SettingName + "'.");
              continue;
            }

            // Check allowed global settings
            List<string> allowedGlobalSettings = SystemHelper.ConvertTokenStringToList(marketSetting.AllowedGlobalSettings, ",");
            if (allowedGlobalSettings.Count > 0 && !allowedGlobalSettings.Contains(this.ActiveSetting)) {
              this.Log.DoLogDebug("'" + marketPair + "' - '" + this.ActiveSetting + "' - Is not allowed in '" + marketSetting.SettingName + "'.");
              continue;
            }

            #region Checking Off Triggers
            SingleMarketSettingSummary smss = this.SingleMarketSettingSummaries.Find(s => s.Market.Equals(marketPair, StringComparison.InvariantCultureIgnoreCase) && s.SingleMarketSetting.SettingName.Equals(marketSetting.SettingName, StringComparison.InvariantCultureIgnoreCase));
            if (smss != null) {
              if (marketSetting.OffTriggers != null) {
                if (marketSetting.OffTriggers.Count > 0) {

                  this.Log.DoLogDebug("'" + marketPair + "' - Checking off triggers '" + marketSetting.SettingName + "'...");

                  List<bool> offTriggerResults = new List<bool>();
                  foreach (OffTrigger offTrigger in marketSetting.OffTriggers) {
                    if (offTrigger.HoursSinceTriggered > 0) {
                      #region Check for Activation time period trigger
                      int smsActiveHours = (int)Math.Floor(DateTime.UtcNow.Subtract(smss.ActivationDateTimeUTC).TotalHours);
                      if (smsActiveHours >= offTrigger.HoursSinceTriggered) {

                        // Trigger met!
                        this.Log.DoLogDebug("'" + marketPair + "' - SMS already active for  " + smsActiveHours.ToString() + " hours. Trigger matched!");

                        offTriggerResults.Add(true);
                      } else {

                        // Trigger not met!
                        this.Log.DoLogDebug("'" + marketPair + "' - SMS only active for  " + smsActiveHours.ToString() + " hours. Trigger not matched!");

                        offTriggerResults.Add(false);
                      }
                      #endregion
                    } else if (offTrigger.Min24hVolume > 0 || offTrigger.Max24hVolume < Constants.Max24hVolume) {
                      #region Check for 24h volume trigger
                      List<MarketTrendChange> marketTrendChanges = this.SingleMarketTrendChanges[this.SingleMarketTrendChanges.Keys.Last()];
                      if (marketTrendChanges.Count > 0) {
                        MarketTrendChange mtc = marketTrendChanges.Find(m => m.Market.Equals(marketPair, StringComparison.InvariantCultureIgnoreCase));
                        if (mtc != null) {
                          if (mtc.Volume24h >= offTrigger.Min24hVolume && mtc.Volume24h <= offTrigger.Max24hVolume) {

                            // Trigger met!
                            this.Log.DoLogDebug("'" + marketPair + "' - 24h volume off trigger matched! 24h volume = " + mtc.Volume24h.ToString(new System.Globalization.CultureInfo("en-US")) + " " + this.LastRuntimeSummary.MainMarket);

                            offTriggerResults.Add(true);
                          } else {

                            // Trigger not met!
                            this.Log.DoLogDebug("'" + marketPair + "' - 24h volume off trigger not matched! 24h volume = " + mtc.Volume24h.ToString(new System.Globalization.CultureInfo("en-US")) + " " + this.LastRuntimeSummary.MainMarket);

                            offTriggerResults.Add(false);
                          }
                        }
                      }

                      #endregion
                    } else {
                      #region Check for market trend triggers
                      if (this.SingleMarketTrendChanges.ContainsKey(offTrigger.MarketTrendName)) {

                        List<MarketTrendChange> marketTrendChanges = this.SingleMarketTrendChanges[offTrigger.MarketTrendName];
                        if (marketTrendChanges.Count > 0) {
                          double averageMarketTrendChange = marketTrendChanges.Average(m => m.TrendChange);

                          MarketTrendChange mtc = marketTrendChanges.Find(m => m.Market.Equals(marketPair, StringComparison.InvariantCultureIgnoreCase));
                          if (mtc != null) {
                            // Get trend change according to configured relation
                            double trendChange = mtc.TrendChange;

                            if (offTrigger.MarketTrendRelation.Equals(Constants.MarketTrendRelationRelative)) {

                              // Build pair trend change relative to the global market trend
                              trendChange = trendChange - averageMarketTrendChange;
                            } else if (offTrigger.MarketTrendRelation.Equals(Constants.MarketTrendRelationRelativeTrigger)) {

                              // Build pair trend change relative to the trigger price
                              double currentPrice = mtc.LastPrice;
                              double triggerPrice = smss.TriggerSnapshot.LastPrice;
                              double triggerTrend = (currentPrice - triggerPrice) / triggerPrice * 100;
                              trendChange = triggerTrend;
                            }

                            // Get market trend change for trigger
                            if (trendChange >= offTrigger.MinChange && trendChange < offTrigger.MaxChange) {

                              // Trigger met!
                              this.Log.DoLogDebug("'" + marketPair + "' - Off Trigger '" + offTrigger.MarketTrendName + "' triggered! TrendChange (" + offTrigger.MarketTrendRelation + ") = " + trendChange.ToString("#,#0.00", new System.Globalization.CultureInfo("en-US")) + "%");

                              offTriggerResults.Add(true);
                            } else {
                              this.Log.DoLogDebug("'" + marketPair + "' - Off Trigger '" + offTrigger.MarketTrendName + "' not triggered. TrendChange (" + offTrigger.MarketTrendRelation + ") = " + trendChange.ToString("#,#0.00", new System.Globalization.CultureInfo("en-US")) + "%");
                              offTriggerResults.Add(false);
                            }
                          } else {
                            offTriggerResults.Add(false);
                          }
                        } else {
                          offTriggerResults.Add(false);
                        }
                      }
                      #endregion
                    }
                  }

                  // Check if all off triggers have to get triggered or just one
                  bool settingOffTriggered = false;
                  switch (marketSetting.OffTriggerConnection.ToLower()) {
                    case "and":
                      settingOffTriggered = offTriggerResults.FindAll(tr => tr == false).Count == 0;
                      break;
                    case "or":
                      settingOffTriggered = offTriggerResults.FindAll(tr => tr == true).Count > 0;
                      break;
                  }

                  // Setting got off triggered, remove it from the summary
                  if (settingOffTriggered) {
                    this.Log.DoLogDebug("'" + marketPair + "' - '" + marketSetting.SettingName + "' off triggered!");
                    this.SingleMarketSettingSummaries.Remove(smss);
                    smss = null;
                  } else {
                    this.Log.DoLogDebug("'" + marketPair + "' - '" + marketSetting.SettingName + "' not off triggered!");
                  }
                } else {
                  this.Log.DoLogDebug("'" + marketPair + "' - '" + marketSetting.SettingName + "' has no off triggers -> triggering off!");
                  this.SingleMarketSettingSummaries.Remove(smss);
                  smss = null;
                }
              }
            }
            #endregion

            if (marketSetting.Triggers.Count > 0 && !stopTriggers) {
              #region Checking Triggers
              this.Log.DoLogDebug("'" + marketPair + "' - Checking triggers for '" + marketSetting.SettingName + "'...");

              List<bool> triggerResults = new List<bool>();
              Dictionary<int, double> relevantTriggers = new Dictionary<int, double>();
              int triggerIndex = 0;
              foreach (Trigger trigger in marketSetting.Triggers) {

                if (trigger.Min24hVolume > 0 || trigger.Max24hVolume < Constants.Max24hVolume) {
                  #region Check for 24h volume trigger
                  List<MarketTrendChange> marketTrendChanges = this.SingleMarketTrendChanges[this.SingleMarketTrendChanges.Keys.Last()];
                  if (marketTrendChanges.Count > 0) {
                    MarketTrendChange mtc = marketTrendChanges.Find(m => m.Market.Equals(marketPair, StringComparison.InvariantCultureIgnoreCase));
                    if (mtc != null) {

                      if (mtc.Volume24h >= trigger.Min24hVolume && mtc.Volume24h <= trigger.Max24hVolume) {
                        // Trigger met!
                        this.Log.DoLogDebug("'" + marketPair + "' - 24h volume trigger matched! 24h volume = " + mtc.Volume24h.ToString(new System.Globalization.CultureInfo("en-US")) + " " + this.LastRuntimeSummary.MainMarket);

                        relevantTriggers.Add(triggerIndex, mtc.Volume24h);

                        string triggerContent = "24h Volume";
                        if (trigger.Min24hVolume > 0) {
                          triggerContent += " - Min: " + trigger.Min24hVolume.ToString(new System.Globalization.CultureInfo("en-US")) + " " + this.LastRuntimeSummary.MainMarket;
                        }

                        if (trigger.Max24hVolume < Constants.Max24hVolume) {
                          triggerContent += " - Max: " + trigger.Max24hVolume.ToString(new System.Globalization.CultureInfo("en-US")) + " " + this.LastRuntimeSummary.MainMarket;
                        }

                        matchedSingleMarketTriggers.Add(marketSetting.SettingName + ": " + triggerContent + " - 24h volume = " + mtc.Volume24h.ToString(new System.Globalization.CultureInfo("en-US")) + " " + this.LastRuntimeSummary.MainMarket);

                        triggerResults.Add(true);
                      } else {
                        this.Log.DoLogDebug("'" + marketPair + "' - 24h volume trigger not matched. 24h volume = " + mtc.Volume24h.ToString(new System.Globalization.CultureInfo("en-US")) + " " + this.LastRuntimeSummary.MainMarket);
                        triggerResults.Add(false);
                      }
                    }
                  }

                  #endregion
                } else if (trigger.AgeDaysLowerThan > 0) {
                  #region Check for age trigger
                  MarketInfo marketInfo = null;
                  if (this.MarketInfos.ContainsKey(marketPair)) {
                    marketInfo = this.MarketInfos[marketPair];
                  }

                  if (marketInfo != null) {
                    int marketAge = (int)Math.Floor(DateTime.Now.ToUniversalTime().Subtract(marketInfo.FirstSeen).TotalDays);
                    if (marketAge < trigger.AgeDaysLowerThan) {
                      matchedSingleMarketTriggers.Add(marketSetting.SettingName + ": '" + marketPair + "' is only " + marketAge.ToString() + " days old on this exchange. Trigger matched!");
                      this.Log.DoLogDebug("'" + marketPair + "' - Is only " + marketAge.ToString() + " days old on this exchange. Trigger matched!");

                      relevantTriggers.Add(triggerIndex, marketAge);
                      triggerResults.Add(true);
                    } else {
                      this.Log.DoLogDebug("'" + marketPair + "' - Age Trigger not triggered. Is already " + marketAge.ToString() + " days old on this exchange.");
                      triggerResults.Add(false);
                    }
                  } else {
                    matchedSingleMarketTriggers.Add("Age for '" + marketPair + "' not found, trigger matched just to be safe!");
                    this.Log.DoLogDebug("'" + marketPair + "' - Age not found, trigger matched just to be safe!");
                    triggerResults.Add(true);
                  }

                  #endregion
                } else {
                  #region Check for market trend triggers
                  if (this.SingleMarketTrendChanges.ContainsKey(trigger.MarketTrendName)) {

                    List<MarketTrendChange> marketTrendChanges = this.SingleMarketTrendChanges[trigger.MarketTrendName];
                    if (marketTrendChanges.Count > 0) {
                      double averageMarketTrendChange = marketTrendChanges.Average(m => m.TrendChange);

                      MarketTrendChange mtc = marketTrendChanges.Find(m => m.Market.Equals(marketPair, StringComparison.InvariantCultureIgnoreCase));
                      if (mtc != null) {
                        // Get trend change according to configured relation
                        double trendChange = mtc.TrendChange;

                        if (trigger.MarketTrendRelation.Equals(Constants.MarketTrendRelationRelative)) {
                          // Build pair trend change relative to the global market trend
                          trendChange = trendChange - averageMarketTrendChange;
                        }

                        // Get market trend change for trigger
                        if (trendChange >= trigger.MinChange && trendChange < trigger.MaxChange) {

                          // Trigger met!
                          this.Log.DoLogDebug("'" + marketPair + "' - Trigger '" + trigger.MarketTrendName + "' triggered! TrendChange (" + trigger.MarketTrendRelation + ") = " + trendChange.ToString("#,#0.00", new System.Globalization.CultureInfo("en-US")) + "%");

                          relevantTriggers.Add(triggerIndex, trendChange);

                          string triggerContent = trigger.MarketTrendName + " - ";
                          if (trigger.MinChange != Constants.MinTrendChange) {
                            triggerContent += " - Min: " + trigger.MinChange.ToString("#,#0.00", new System.Globalization.CultureInfo("en-US")) + "%";
                          }

                          if (trigger.MaxChange != Constants.MaxTrendChange) {
                            triggerContent += " - Max: " + trigger.MaxChange.ToString("#,#0.00", new System.Globalization.CultureInfo("en-US")) + "%";
                          }

                          matchedSingleMarketTriggers.Add(marketSetting.SettingName + ": " + triggerContent + " - TrendChange (" + trigger.MarketTrendRelation + ") = " + trendChange.ToString("#,#0.00", new System.Globalization.CultureInfo("en-US")) + "%");

                          triggerResults.Add(true);
                        } else {
                          this.Log.DoLogDebug("'" + marketPair + "' - Trigger '" + trigger.MarketTrendName + "' not triggered. TrendChange (" + trigger.MarketTrendRelation + ") = " + trendChange.ToString("#,#0.00", new System.Globalization.CultureInfo("en-US")) + "%");
                          triggerResults.Add(false);
                        }
                      } else {
                        this.Log.DoLogDebug("'" + marketPair + "' - No market trend change found for '" + trigger.MarketTrendName + "'! Coin just got released? Trigger ignored!");
                        triggerResults.Add(false);
                      }
                    } else {
                      this.Log.DoLogWarn("'" + marketPair + "' - No market trend changes found for '" + trigger.MarketTrendName + "'! Trigger ignored!");
                      triggerResults.Add(false);
                    }
                  } else {
                    MarketTrend marketTrend = this.PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.MarketTrends.Find(mt => mt.Name.Equals(trigger.MarketTrendName, StringComparison.InvariantCultureIgnoreCase));
                    if (marketTrend != null) {
                      if (!marketTrend.Platform.Equals("Exchange", StringComparison.InvariantCultureIgnoreCase)) {
                        this.Log.DoLogWarn("Market Trend '" + trigger.MarketTrendName + "' is invalid for single market settings! Only trends using the platform 'Exchange' are valid for single market settings.");
                        triggerResults.Add(false);
                      }
                    } else {
                      this.Log.DoLogWarn("Market Trend '" + trigger.MarketTrendName + "' not found! Trigger ignored!");
                      triggerResults.Add(false);
                    }
                  }
                  #endregion
                }
                triggerIndex++;
              }

              // Check if all triggers have to get triggered or just one
              bool settingTriggered = false;
              switch (marketSetting.TriggerConnection.ToLower()) {
                case "and":
                  settingTriggered = triggerResults.FindAll(tr => tr == false).Count == 0;
                  break;
                case "or":
                  settingTriggered = triggerResults.FindAll(tr => tr == true).Count > 0;
                  break;
              }
              #endregion

              bool isFreshTrigger = true;

              // Setting not triggered -> Check if it is already active as a long term SMS using Off Triggers
              if (!settingTriggered) {
                this.Log.DoLogDebug("'" + marketPair + "' - SMS '" + marketSetting.SettingName + "' not triggered, checking for long term activation.");
                if (smss != null) {
                  if (marketSetting.OffTriggers != null) {
                    if (marketSetting.OffTriggers.Count > 0) {
                      this.Log.DoLogDebug("'" + marketPair + "' - SMS '" + marketSetting.SettingName + "' has off triggers, starting special trigger...");
                      // Setting already active and using off triggers -> set as triggered
                      settingTriggered = true;
                      isFreshTrigger = false;

                      matchedSingleMarketTriggers = new List<string>();
                      foreach (string matchedTriggerContent in smss.TriggerSnapshot.MatchedTriggersContent) {
                        if (matchedTriggerContent.StartsWith(marketSetting.SettingName + ":")) {
                          matchedSingleMarketTriggers.Add(matchedTriggerContent);
                        }
                      }

                      int removalLength = matchedSingleMarketTriggers.Count - marketSetting.Triggers.Count;
                      if (removalLength > 0) {
                        matchedSingleMarketTriggers.RemoveRange(0, removalLength);
                      }

                      this.Log.DoLogDebug("'" + marketPair + "' - Activating SMS '" + marketSetting.SettingName + "' as off triggers are not met.");
                    }
                  }
                }
              }

              // Setting got triggered -> Activate it!
              if (settingTriggered) {
                this.Log.DoLogDebug("'" + marketPair + "' - '" + marketSetting.SettingName + "' triggered!");

                // Save matched triggers to get displayed in the comment lines
                if (!matchedMarketTriggers.ContainsKey(marketPair)) {
                  matchedMarketTriggers.Add(marketPair, matchedSingleMarketTriggers);
                } else {
                  matchedMarketTriggers[marketPair].AddRange(matchedSingleMarketTriggers);
                }

                if (!this.TriggeredSingleMarketSettings.ContainsKey(marketPair)) {
                  List<SingleMarketSetting> smsList = new List<SingleMarketSetting>();
                  smsList.Add(marketSetting);
                  this.TriggeredSingleMarketSettings.Add(marketPair, smsList);
                } else {
                  this.TriggeredSingleMarketSettings[marketPair].Add(marketSetting);
                }

                // Counting triggered setting
                if (!this.SingleMarketSettingsCount.ContainsKey(marketSetting.SettingName)) {
                  this.SingleMarketSettingsCount.Add(marketSetting.SettingName, 1);
                } else {
                  this.SingleMarketSettingsCount[marketSetting.SettingName]++;
                }

                if (isFreshTrigger) {
                  this.Log.DoLogDebug("'" + marketPair + "' - SMS '" + marketSetting.SettingName + "' saving summary data...");

                  // Check if this setting is already active for this market
                  if (smss == null || marketSetting.RefreshOffTriggers) {
                    if (smss == null) {
                      smss = new SingleMarketSettingSummary();
                    } else {
                      this.SingleMarketSettingSummaries.Remove(smss);
                    }

                    smss.ActivationDateTimeUTC = DateTime.UtcNow;
                    smss.Market = marketPair;
                    smss.SingleMarketSetting = marketSetting;
                    smss.TriggerSnapshot = new TriggerSnapshot();
                    smss.TriggerSnapshot.Last24hVolume = 0;
                    smss.TriggerSnapshot.LastPrice = 0;
                    smss.TriggerSnapshot.RelevantTriggers = relevantTriggers;
                    smss.TriggerSnapshot.MatchedTriggersContent = matchedSingleMarketTriggers;

                    List<MarketTrendChange> marketTrendChanges = this.SingleMarketTrendChanges[this.SingleMarketTrendChanges.Keys.Last()];
                    if (marketTrendChanges.Count > 0) {
                      MarketTrendChange mtc = marketTrendChanges.Find(m => m.Market.Equals(marketPair, StringComparison.InvariantCultureIgnoreCase));
                      if (mtc != null) {
                        smss.TriggerSnapshot.Last24hVolume = mtc.Volume24h;
                        smss.TriggerSnapshot.LastPrice = mtc.LastPrice;
                      }
                    }

                    this.SingleMarketSettingSummaries.Add(smss);

                    this.Log.DoLogDebug("'" + marketPair + "' - SMS '" + marketSetting.SettingName + "' summary data saved.");
                  } else {
                    this.Log.DoLogDebug("'" + marketPair + "' - SMS '" + marketSetting.SettingName + "' already active for this market and no refresh allowed.");
                  }
                }

                // Stop processing other settings if configured
                if (marketSetting.StopProcessWhenTriggered) {
                  stopTriggers = true;
                }
              } else {
                this.Log.DoLogDebug("'" + marketPair + "' - '" + marketSetting.SettingName + "' not triggered!");
              }
            }
          }

          if ((marketPairProcess % 10) == 0) {
            this.Log.DoLogInfo("What are you looking at? " + marketPairProcess + "/" + this.MarketList.Count + " markets done...");
          }

          marketPairProcess++;
        }

        if (this.TriggeredSingleMarketSettings.Count > 0) {

          // Write single market settings
          this.Log.DoLogInfo("Building single market settings for '" + this.TriggeredSingleMarketSettings.Count.ToString() + "' markets...");

          SettingsHandler.CompileSingleMarketProperties(this, matchedMarketTriggers);
          this.SingleMarketSettingWritten = true;

          this.Log.DoLogInfo("Building single market settings completed.");
        } else {
          this.Log.DoLogInfo("No settings triggered for single markets.");

          // Remove single market settings if no triggers are met - if necessary
          this.SingleMarketSettingWritten = SettingsHandler.RemoveSingleMarketSettings(this);
        }

      } else {
        this.Log.DoLogInfo("No single market settings found.");
      }
    }

    private void SaveProfitTrailerProperties(bool headerLinesAdded, string pairsPropertiesPath, string dcaPropertiesPath, string indicatorsPropertiesPath) {
      if (headerLinesAdded || this.GlobalSettingWritten || this.SingleMarketSettingWritten) {
        if (this.ProfitTrailerMajorVersion == 1) {
          // Save current PT properties to files (Valid for PT 1.x)
          this.Log.DoLogInfo("Saving properties files...");

          // Write Pairs.properties
          if (this.PTMagicConfiguration.GeneralSettings.Backup.IsEnabled) FileHelper.CreateBackup(pairsPropertiesPath, this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar + "_backups" + Path.DirectorySeparatorChar);
          this.Log.DoLogInfo("Writing Pairs.properties...");
          if (!this.PTMagicConfiguration.GeneralSettings.Application.TestMode) File.WriteAllLines(pairsPropertiesPath, this.PairsLines);

          // Write DCA.properties
          if (this.PTMagicConfiguration.GeneralSettings.Backup.IsEnabled) FileHelper.CreateBackup(dcaPropertiesPath, this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar + "_backups" + Path.DirectorySeparatorChar);
          this.Log.DoLogInfo("Writing DCA.properties...");
          if (!this.PTMagicConfiguration.GeneralSettings.Application.TestMode) File.WriteAllLines(dcaPropertiesPath, this.DCALines);

          // Write Indicators.properties
          if (this.PTMagicConfiguration.GeneralSettings.Backup.IsEnabled) FileHelper.CreateBackup(indicatorsPropertiesPath, this.PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerPath + Constants.PTPathTrading + Path.DirectorySeparatorChar + "_backups" + Path.DirectorySeparatorChar);
          this.Log.DoLogInfo("Writing Indicators.properties...");
          if (!this.PTMagicConfiguration.GeneralSettings.Application.TestMode) File.WriteAllLines(indicatorsPropertiesPath, this.IndicatorsLines);

          this.Log.DoLogInfo("All properties files saved!");
        } else {
          // Save current PT properties to API (Valid for PT 2.x and above)
          this.Log.DoLogInfo("Saving properties using API...");

          // Send Pairs.properties
          if (!this.PTMagicConfiguration.GeneralSettings.Application.TestMode) SettingsAPI.SendPropertyLinesToAPI("PAIRS", this.PairsLines, this.PTMagicConfiguration, this.Log);

          // Send DCA.properties
          if (!this.PTMagicConfiguration.GeneralSettings.Application.TestMode) SettingsAPI.SendPropertyLinesToAPI("DCA", this.DCALines, this.PTMagicConfiguration, this.Log);

          // Send Indicators.properties
          if (!this.PTMagicConfiguration.GeneralSettings.Application.TestMode) SettingsAPI.SendPropertyLinesToAPI("INDICATORS", this.IndicatorsLines, this.PTMagicConfiguration, this.Log);

          this.Log.DoLogInfo("All properties saved!");
        }

      } else {
        this.Log.DoLogInfo("Nothing changed, no files touched!");
      }
    }

    private void SaveSingleMarketSettingsSummary() {
      JsonSerializerSettings smsSummaryJsonSettings = new JsonSerializerSettings();
      smsSummaryJsonSettings.NullValueHandling = NullValueHandling.Ignore;
      smsSummaryJsonSettings.DefaultValueHandling = DefaultValueHandling.Ignore;

      FileHelper.WriteTextToFile(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar, "SingleMarketSettingSummary.json", JsonConvert.SerializeObject(this.SingleMarketSettingSummaries, Formatting.None, smsSummaryJsonSettings));

      this.Log.DoLogInfo("Single Market Settings Summary saved.");
    }

    private void SaveRuntimeSummary(bool headerLinesAdded) {
      DateTime endTime = DateTime.Now;
      int elapsedSeconds = (int)Math.Round(endTime.Subtract(this.LastRuntime).TotalSeconds, 0);

      this.LastRuntimeSummary.LastRuntimeSeconds = elapsedSeconds;

      this.Log.DoLogInfo("Building LastRuntimeSummary.json for your monitor...");

      // Load existing runtime summary and read ongoing data
      if (File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + "LastRuntimeSummary.json")) {
        try {
          Summary summary = JsonConvert.DeserializeObject<Summary>(System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar + "LastRuntimeSummary.json"));
          if (summary != null) {

            // Last setting switch in case the app got restarted and has no history
            if (this.LastRuntimeSummary.LastGlobalSettingSwitch == Constants.confMinDate) {
              this.LastRuntimeSummary.LastGlobalSettingSwitch = summary.LastGlobalSettingSwitch;
            }

            // Market trend changes history for graph data
            foreach (string key in summary.MarketTrendChanges.Keys) {
              this.LastRuntimeSummary.MarketTrendChanges.Add(key, summary.MarketTrendChanges[key].FindAll(mtc => mtc.TrendDateTime >= DateTime.Now.AddHours(-PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.StoreDataMaxHours)));
            }

            // Global setting summary to be kept
            this.LastRuntimeSummary.GlobalSettingSummary.AddRange(summary.GlobalSettingSummary.FindAll(gss => gss.SwitchDateTime >= DateTime.Now.AddHours(-96)));

            this.Log.DoLogInfo("Summary: Loaded old LastRuntimeSummary.json to keep data.");
          }
        } catch (Exception ex) {
          this.Log.DoLogCritical("Summary: Error loading old summary (" + ex.Message + "). Creating new one.", ex);
        }
      }

      this.Log.DoLogInfo("Summary: Building global settings summary...");

      // Change setting summary
      GlobalSettingSummary lastSettingSummary = null;
      if (this.LastRuntimeSummary.LastGlobalSettingSwitch == this.LastRuntimeSummary.LastRuntime || this.LastRuntimeSummary.GlobalSettingSummary.Count == 0) {

        // Setting got switched this run, add a new setting summary
        GlobalSettingSummary gss = new GlobalSettingSummary();
        gss.SettingName = this.LastRuntimeSummary.CurrentGlobalSetting.SettingName;
        gss.SwitchDateTime = this.LastRuntimeSummary.LastRuntime.ToUniversalTime();

        if (this.LastRuntimeSummary.GlobalSettingSummary.Count > 0) {
          lastSettingSummary = this.LastRuntimeSummary.GlobalSettingSummary.OrderByDescending(lss => lss.SwitchDateTime).First();
          lastSettingSummary.ActiveSeconds = (int)Math.Ceiling(DateTime.Now.ToUniversalTime().Subtract(lastSettingSummary.SwitchDateTime).TotalSeconds);
        }

        this.LastRuntimeSummary.GlobalSettingSummary.Add(gss);

        lastSettingSummary = this.LastRuntimeSummary.GlobalSettingSummary.OrderByDescending(lss => lss.SwitchDateTime).First();
      } else {

        // Setting did not get switched, update data
        if (this.LastRuntimeSummary.GlobalSettingSummary.Count > 0) {
          lastSettingSummary = this.LastRuntimeSummary.GlobalSettingSummary.OrderByDescending(lss => lss.SwitchDateTime).First();
          lastSettingSummary.ActiveSeconds = (int)Math.Ceiling(DateTime.Now.ToUniversalTime().Subtract(lastSettingSummary.SwitchDateTime).TotalSeconds);
        }
      }

      this.Log.DoLogInfo("Summary: Built global settings summary.");

      this.Log.DoLogInfo("Summary: Save market trend changes for summary.");
      // Save market trend changes for the summary
      foreach (string key in this.AverageMarketTrendChanges.Keys) {
        List<MarketTrendChange> mtChanges = new List<MarketTrendChange>();
        if (this.LastRuntimeSummary.MarketTrendChanges.ContainsKey(key)) {
          mtChanges = this.LastRuntimeSummary.MarketTrendChanges[key];
        }

        MarketTrendChange newChange = new MarketTrendChange();
        newChange.MarketTrendName = key;
        newChange.TrendChange = this.AverageMarketTrendChanges[key];
        newChange.TrendDateTime = this.LastRuntimeSummary.LastRuntime;

        mtChanges.Add(newChange);
        if (lastSettingSummary != null) {
          if (!lastSettingSummary.MarketTrendChanges.ContainsKey(key)) {
            GlobalSetting gs = this.PTMagicConfiguration.AnalyzerSettings.GlobalSettings.Find(g => g.SettingName.Equals(lastSettingSummary.SettingName));
            if (gs != null) {
              if (gs.SettingName.Equals("Default", StringComparison.InvariantCultureIgnoreCase) || gs.Triggers.Find(t => t.MarketTrendName.Equals(key)) != null) {
                lastSettingSummary.MarketTrendChanges.Add(key, newChange);
              }
            }
          }
        }

        this.LastRuntimeSummary.MarketTrendChanges[key] = mtChanges;
      }
      this.Log.DoLogInfo("Summary: Market trends saved.");

      this.Log.DoLogInfo("Summary: Getting current global properties...");

      // Get current global settings from PAIRS.PROPERTIES
      Dictionary<string, string> pairsProperties = SettingsHandler.GetPropertiesAsDictionary(this.PairsLines);
      Dictionary<string, string> dcaProperties = SettingsHandler.GetPropertiesAsDictionary(this.DCALines);

      string defaultBuyValueString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, "ALL_buy_value", "DEFAULT_A_buy_value");
      double defaultBuyValue = SystemHelper.TextToDouble(defaultBuyValueString, 0, "en-US");

      string defaultTrailingBuyString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, "ALL_trailing_buy", "DEFAULT_trailing_buy");
      double defaultTrailingBuy = SystemHelper.TextToDouble(defaultTrailingBuyString, 0, "en-US");

      string defaultSellValueString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, "ALL_sell_value", "DEFAULT_A_sell_value");
      double defaultSellValue = SystemHelper.TextToDouble(defaultSellValueString, 0, "en-US");

      string defaultTrailingProfitString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, "ALL_trailing_profit", "DEFAULT_trailing_profit");
      double defaultTrailingProfit = SystemHelper.TextToDouble(defaultTrailingProfitString, 0, "en-US");

      string defaultMaxTradingPairsString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, "ALL_max_trading_pairs", "max_trading_pairs");
      double defaultMaxTradingPairs = SystemHelper.TextToDouble(defaultMaxTradingPairsString, 0, "en-US");

      string defaultMaxCostString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, "ALL_max_cost", "DEFAULT_initial_cost");
      double defaultMaxCost = SystemHelper.TextToDouble(defaultMaxCostString, 0, "en-US");

      string defaultMaxCostPercentageString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, "ALL_max_cost_percentage", "DEFAULT_initial_cost_percentage");
      double defaultMaxCostPercentage = SystemHelper.TextToDouble(defaultMaxCostPercentageString, 0, "en-US");

      string defaultMinBuyVolumeString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, "ALL_min_buy_volume", "DEFAULT_min_buy_volume");
      double defaultMinBuyVolume = SystemHelper.TextToDouble(defaultMinBuyVolumeString, 0, "en-US");

      string defaultDCALevelString = SettingsHandler.GetCurrentPropertyValue(dcaProperties, "max_buy_times", "DEFAULT_DCA_max_buy_times");
      double defaultDCALevel = SystemHelper.TextToDouble(defaultDCALevelString, 0, "en-US");

      string defaultBuyStrategy = SettingsHandler.GetCurrentPropertyValue(pairsProperties, "ALL_buy_strategy", "DEFAULT_A_buy_strategy");

      string defaultSellStrategy = SettingsHandler.GetCurrentPropertyValue(pairsProperties, "ALL_sell_strategy", "DEFAULT_A_sell_strategy");

      string defaultDCAEnabledString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, "ALL_DCA_enabled", "DEFAULT_DCA_enabled");
      bool defaultDCAEnabled = (defaultDCAEnabledString.Equals("false", StringComparison.InvariantCultureIgnoreCase)) ? false : true;

      string defaultSOMActiveString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, "ALL_sell_only_mode", "DEFAULT_sell_only_mode_enabled");
      bool defaultSOMActive = (defaultSOMActiveString.Equals("false", StringComparison.InvariantCultureIgnoreCase)) ? false : true;
      this.LastRuntimeSummary.IsSOMActive = defaultSOMActive;

      string dcaBuyStrategyString = SettingsHandler.GetCurrentPropertyValue(dcaProperties, "buy_strategy", "DEFAULT_DCA_A_buy_strategy");
      this.LastRuntimeSummary.DCABuyStrategy = dcaBuyStrategyString;

      this.LastRuntimeSummary.BuyValue = defaultBuyValue;
      this.LastRuntimeSummary.TrailingBuy = defaultTrailingBuy;
      this.LastRuntimeSummary.SellValue = defaultSellValue;
      this.LastRuntimeSummary.TrailingProfit = defaultTrailingProfit;
      this.LastRuntimeSummary.MaxTradingPairs = defaultMaxTradingPairs;
      this.LastRuntimeSummary.MaxCost = defaultMaxCost;
      this.LastRuntimeSummary.MaxCostPercentage = defaultMaxCostPercentage;
      this.LastRuntimeSummary.MinBuyVolume = defaultMinBuyVolume;
      this.LastRuntimeSummary.BuyStrategy = defaultBuyStrategy;
      this.LastRuntimeSummary.SellStrategy = defaultSellStrategy;
      this.LastRuntimeSummary.DCALevels = defaultDCALevel;

      double maxDCALevel = this.LastRuntimeSummary.DCALevels;
      if (maxDCALevel == 0) maxDCALevel = 1000;

      string dcaDefaultTriggerString = SettingsHandler.GetCurrentPropertyValue(dcaProperties, "buy_trigger", "DEFAULT_DCA_buy_trigger");
      double dcaDefaultTrigger = SystemHelper.TextToDouble(dcaDefaultTriggerString, 0, "en-US");

      this.LastRuntimeSummary.DCATrigger = dcaDefaultTrigger;

      // Get configured DCA triggers
      for (int dca = 1; dca <= maxDCALevel; dca++) {
        string dcaTriggerString = SettingsHandler.GetCurrentPropertyValue(dcaProperties, "buy_trigger_" + dca.ToString(), "DEFAULT_DCA_buy_trigger_" + dca.ToString());
        if (!dcaTriggerString.Equals("")) {
          double dcaTrigger = SystemHelper.TextToDouble(dcaTriggerString, 0, "en-US");

          this.LastRuntimeSummary.DCATriggers.Add(dca, dcaTrigger);
        } else {
          if (this.LastRuntimeSummary.DCALevels == 0) this.LastRuntimeSummary.DCALevels = dca - 1;
          break;
        }
      }

      // Get configured DCA percentages
      if (this.ProfitTrailerMajorVersion >= 2) {
        string dcaDefaultPercentageString = SettingsHandler.GetCurrentPropertyValue(dcaProperties, "DEFAULT_DCA_buy_percentage", "");
        double dcaDefaultPercentage = SystemHelper.TextToDouble(dcaDefaultPercentageString, 0, "en-US");

        this.LastRuntimeSummary.DCAPercentage = dcaDefaultPercentage;

        for (int dca = 1; dca <= maxDCALevel; dca++) {
          string dcaPercentageString = SettingsHandler.GetCurrentPropertyValue(dcaProperties, "DEFAULT_DCA_buy_percentage_" + dca.ToString(), "");
          if (!dcaPercentageString.Equals("")) {
            double dcaPercentage = SystemHelper.TextToDouble(dcaPercentageString, 0, "en-US");

            this.LastRuntimeSummary.DCAPercentages.Add(dca, dcaPercentage);
          } else {
            if (this.LastRuntimeSummary.DCALevels == 0) this.LastRuntimeSummary.DCALevels = dca - 1;
            break;
          }
        }
      } else {
        this.LastRuntimeSummary.DCAPercentage = 100;
      }

      // Get configured Buy Strategies
      for (char c = 'A'; c <= 'Z'; c++) {
        string buyStrategyName = SettingsHandler.GetCurrentPropertyValue(pairsProperties, "DEFAULT_" + c + "_buy_strategy", "");
        if (!buyStrategyName.Equals("")) {
          StrategySummary buyStrategy = new StrategySummary();
          buyStrategy.Name = buyStrategyName;
          buyStrategy.Value = SystemHelper.TextToDouble(SettingsHandler.GetCurrentPropertyValue(pairsProperties, "DEFAULT_" + c + "_buy_value", ""), 0, "en-US");

          this.LastRuntimeSummary.BuyStrategies.Add(buyStrategy);
        } else {
          break;
        }
      }

      // Get configured Sell Strategies
      for (char c = 'A'; c <= 'Z'; c++) {
        string sellStrategyName = SettingsHandler.GetCurrentPropertyValue(pairsProperties, "DEFAULT_" + c + "_sell_strategy", "");
        if (!sellStrategyName.Equals("")) {
          StrategySummary sellStrategy = new StrategySummary();
          sellStrategy.Name = sellStrategyName;
          sellStrategy.Value = SystemHelper.TextToDouble(SettingsHandler.GetCurrentPropertyValue(pairsProperties, "DEFAULT_" + c + "_sell_value", ""), 0, "en-US");

          this.LastRuntimeSummary.SellStrategies.Add(sellStrategy);
        } else {
          break;
        }
      }

      // Get configured DCA Buy Strategies
      for (char c = 'A'; c <= 'Z'; c++) {
        string buyStrategyName = SettingsHandler.GetCurrentPropertyValue(dcaProperties, "DEFAULT_DCA_" + c + "_buy_strategy", "");
        if (!buyStrategyName.Equals("")) {
          StrategySummary buyStrategy = new StrategySummary();
          buyStrategy.Name = buyStrategyName;
          buyStrategy.Value = SystemHelper.TextToDouble(SettingsHandler.GetCurrentPropertyValue(dcaProperties, "DEFAULT_DCA_" + c + "_buy_value", ""), 0, "en-US");

          this.LastRuntimeSummary.DCABuyStrategies.Add(buyStrategy);
        } else {
          break;
        }
      }

      // Get configured DCA Sell Strategies
      for (char c = 'A'; c <= 'Z'; c++) {
        string sellStrategyName = SettingsHandler.GetCurrentPropertyValue(dcaProperties, "DEFAULT_DCA_" + c + "_sell_strategy", "");
        if (!sellStrategyName.Equals("")) {
          StrategySummary sellStrategy = new StrategySummary();
          sellStrategy.Name = sellStrategyName;
          sellStrategy.Value = SystemHelper.TextToDouble(SettingsHandler.GetCurrentPropertyValue(dcaProperties, "DEFAULT_DCA_" + c + "_sell_value", ""), 0, "en-US");

          this.LastRuntimeSummary.DCASellStrategies.Add(sellStrategy);
        } else {
          break;
        }
      }

      // Get current main currency price
      Dictionary<string, Market> recentMarkets = BaseAnalyzer.GetMarketDataFromFile(this.PTMagicConfiguration, this.Log, "Exchange", DateTime.UtcNow, "Recent");
      if (recentMarkets.Keys.Count > 0) {
        this.LastRuntimeSummary.MainMarketPrice = recentMarkets.First().Value.MainCurrencyPriceUSD;

        if (!this.LastRuntimeSummary.MainFiatCurrency.Equals("USD", StringComparison.InvariantCultureIgnoreCase)) {
          this.LastRuntimeSummary.MainMarketPrice = this.LastRuntimeSummary.MainMarketPrice * this.LastRuntimeSummary.MainFiatCurrencyExchangeRate;
        }
      }

      this.Log.DoLogInfo("Summary: Current global properties saved.");

      this.Log.DoLogInfo("Summary: Getting current single market properties...");
      // Get current single market settings from PAIRS.PROPERTIES for each configured market
      foreach (string marketPair in this.MarketList) {
        MarketPairSummary mpSummary = new MarketPairSummary();
        mpSummary.CurrentBuyValue = defaultBuyValue;
        mpSummary.CurrentTrailingBuy = defaultTrailingBuy;
        mpSummary.CurrentSellValue = defaultSellValue;
        mpSummary.CurrentTrailingProfit = defaultTrailingProfit;
        mpSummary.IsDCAEnabled = defaultDCAEnabled;
        mpSummary.IsSOMActive = defaultSOMActive;
        mpSummary.ActiveSingleSettings = new List<SingleMarketSetting>();

        if (this.MarketList.Contains(marketPair)) {

          // Pair is allowed for trading, check for individual values
          mpSummary.IsTradingEnabled = true;

          if (this.TriggeredSingleMarketSettings.Count > 0) {
            if (this.TriggeredSingleMarketSettings.ContainsKey(marketPair)) {
              mpSummary.ActiveSingleSettings = this.TriggeredSingleMarketSettings[marketPair];
            }
          }

          string marketPairSimple = marketPair.Replace(this.LastRuntimeSummary.MainMarket, "").Replace("_", "").Replace("-", "");

          // Get configured Buy Strategies
          for (char c = 'A'; c <= 'Z'; c++) {
            string buyStrategyName = SettingsHandler.GetCurrentPropertyValue(pairsProperties, marketPairSimple + "_" + c + "_buy_strategy", "");
            if (!buyStrategyName.Equals("")) {
              StrategySummary buyStrategy = new StrategySummary();
              buyStrategy.Name = buyStrategyName;
              buyStrategy.Value = SystemHelper.TextToDouble(SettingsHandler.GetCurrentPropertyValue(pairsProperties, marketPair + "_" + c + "_buy_value", ""), 0, "en-US");

              mpSummary.BuyStrategies.Add(buyStrategy);
            } else {
              break;
            }
          }
          if (mpSummary.BuyStrategies.Count == 0) mpSummary.BuyStrategies = this.LastRuntimeSummary.BuyStrategies;

          // Get configured Sell Strategies
          for (char c = 'A'; c <= 'Z'; c++) {
            string sellStrategyName = SettingsHandler.GetCurrentPropertyValue(pairsProperties, marketPairSimple + "_" + c + "_sell_strategy", "");
            if (!sellStrategyName.Equals("")) {
              StrategySummary sellStrategy = new StrategySummary();
              sellStrategy.Name = sellStrategyName;
              sellStrategy.Value = SystemHelper.TextToDouble(SettingsHandler.GetCurrentPropertyValue(pairsProperties, marketPairSimple + "_" + c + "_sell_value", ""), 0, "en-US");

              mpSummary.SellStrategies.Add(sellStrategy);
            } else {
              break;
            }
          }
          if (mpSummary.SellStrategies.Count == 0) mpSummary.SellStrategies = this.LastRuntimeSummary.SellStrategies;

          string pairBuyValueString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, marketPairSimple + "_buy_value", marketPairSimple + "_A_buy_value");
          double pairBuyValue = SystemHelper.TextToDouble(pairBuyValueString, 100, "en-US");
          if (pairBuyValue < 100) mpSummary.CurrentBuyValue = pairBuyValue;

          string pairTrailingBuyString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, marketPair + "_trailing_buy", marketPairSimple + "_trailing_buy");
          double pairTrailingBuy = SystemHelper.TextToDouble(pairTrailingBuyString, -1, "en-US");
          if (pairTrailingBuy > -1) mpSummary.CurrentTrailingBuy = pairTrailingBuy;

          string pairSellValueString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, marketPair + "_sell_value", marketPairSimple + "_A_sell_value");
          double pairSellValue = SystemHelper.TextToDouble(pairSellValueString, 0, "en-US");
          if (pairSellValue > -1) mpSummary.CurrentSellValue = pairSellValue;

          string pairTrailingProfitString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, marketPair + "_trailing_profit", marketPairSimple + "_trailing_profit");
          double pairTrailingProfit = SystemHelper.TextToDouble(pairTrailingProfitString, 0, "en-US");
          if (pairTrailingProfit > -1) mpSummary.CurrentTrailingProfit = pairTrailingProfit;

          string pairTradingEnabledString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, marketPair + "_trading_enabled", marketPairSimple + "_trading_enabled");
          mpSummary.IsTradingEnabled = (pairTradingEnabledString.Equals("false", StringComparison.InvariantCultureIgnoreCase)) ? false : true;

          string pairDCAEnabledString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, marketPair + "_DCA_enabled", marketPairSimple + "_DCA_enabled");
          mpSummary.IsDCAEnabled = (pairDCAEnabledString.Equals("false", StringComparison.InvariantCultureIgnoreCase)) ? false : defaultDCAEnabled;

          string pairSOMActiveString = SettingsHandler.GetCurrentPropertyValue(pairsProperties, marketPair + "_sell_only_mode", marketPairSimple + "_sell_only_mode_enabled");
          mpSummary.IsSOMActive = (pairSOMActiveString.Equals("true", StringComparison.InvariantCultureIgnoreCase)) ? true : false;
        }

        // Get market trend values for each market pair
        foreach (string marketTrendName in this.SingleMarketTrendChanges.Keys) {
          if (this.SingleMarketTrendChanges.ContainsKey(marketTrendName)) {
            MarketTrendChange mtc = this.SingleMarketTrendChanges[marketTrendName].Find(m => m.Market == marketPair);
            if (mtc != null) {
              double marketTrendChange = mtc.TrendChange;
              mpSummary.MarketTrendChanges.Add(marketTrendName, marketTrendChange);

              mpSummary.LatestPrice = mtc.LastPrice;
              mpSummary.Latest24hVolume = mtc.Volume24h;
            }
          }
        }

        if (!this.LastRuntimeSummary.MarketSummary.ContainsKey(marketPair)) {
          this.LastRuntimeSummary.MarketSummary.Add(marketPair, mpSummary);
        }
      }

      this.Log.DoLogInfo("Summary: Current single market properties saved.");

      string serialziedJson = JsonConvert.SerializeObject(this.LastRuntimeSummary);

      // Save the summary JSON file
      try {
        FileHelper.WriteTextToFile(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar, "LastRuntimeSummary.json", serialziedJson);

        this.Log.DoLogInfo("Summary: LastRuntimeSummary.json saved.");
      } catch (Exception ex) {
        this.Log.DoLogCritical("Exception while writing LastRuntimeSummary.json", ex);

        try {
          this.Log.DoLogInfo("Summary: Retrying one more time to save LastRuntimeSummary.json.");
          FileHelper.WriteTextToFile(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathData + Path.DirectorySeparatorChar, "LastRuntimeSummary.json", serialziedJson);

          this.Log.DoLogInfo("Summary: LastRuntimeSummary.json saved.");
        } catch (Exception ex2) {
          this.Log.DoLogCritical("Nope, another Exception while writing LastRuntimeSummary.json", ex2);
        }
      }

      string logsPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Constants.PTMagicPathLogs + Path.DirectorySeparatorChar;
      if (Directory.Exists(logsPath)) {
        FileHelper.CleanupFiles(logsPath, 24 * 3);
        this.Log.DoLogInfo("Cleaned up logfiles.");
      }

      this.TotalElapsedSeconds += elapsedSeconds;
      this.Log.DoLogInfo("##########################################################");
      this.Log.DoLogInfo("#******************* RAID SUMMARY ********************#");
      this.Log.DoLogInfo("+ PT Magic Version: " + this.LastRuntimeSummary.Version);
      if (!SystemHelper.IsRecentVersion(this.LastRuntimeSummary.Version, this.LatestVersion)) {
        this.Log.DoLogWarn("+ Your version is out of date! The most recent version is " + this.LatestVersion);
      }
      this.Log.DoLogInfo("+ Proft Trailer Major Version: " + PTMagicConfiguration.GeneralSettings.Application.ProfitTrailerMajorVersion.ToString());
      this.Log.DoLogInfo("+ Instance name: " + PTMagicConfiguration.GeneralSettings.Application.InstanceName);
      this.Log.DoLogInfo("+ Time spent: " + SystemHelper.GetProperDurationTime(elapsedSeconds));
      this.Log.DoLogInfo("+ Active setting: " + this.LastRuntimeSummary.CurrentGlobalSetting.SettingName);
      this.Log.DoLogInfo("+ Global setting changed: " + ((this.LastRuntimeSummary.LastGlobalSettingSwitch == this.LastRuntimeSummary.LastRuntime) ? "Yes" : "No") + " " + ((this.LastRuntimeSummary.FloodProtectedSetting != null) ? "(Flood protection!)" : ""));
      this.Log.DoLogInfo("+ Files changed: " + (((headerLinesAdded || this.GlobalSettingWritten || this.SingleMarketSettingWritten) && !this.PTMagicConfiguration.GeneralSettings.Application.TestMode) ? "Yes" : "No"));
      this.Log.DoLogInfo("+ Markets with active single market settings: " + this.TriggeredSingleMarketSettings.Count.ToString());
      foreach (string activeSMS in this.SingleMarketSettingsCount.Keys) {
        this.Log.DoLogInfo("+   " + activeSMS + ": " + this.SingleMarketSettingsCount[activeSMS].ToString());
      }
      this.Log.DoLogInfo("+ " + this.TotalElapsedSeconds.ToString() + " Magicbots killed in " + this.RunCount.ToString() + " raids on Cryptodragon's Lair " + this.PTMagicConfiguration.AnalyzerSettings.MarketAnalyzer.IntervalMinutes.ToString() + ".");
      this.Log.DoLogInfo("");
      this.Log.DoLogInfo("DO NOT CLOSE THIS WINDOW! THIS IS THE BOT THAT ANALYZES TRENDS AND CHANGES SETTINGS!");
      this.Log.DoLogInfo("");
      this.Log.DoLogInfo("#********************************************************#");
      this.Log.DoLogInfo("##########################################################");
      this.Log.DoLogInfo("");
    }

    private void Cleanup() {
      this.GlobalSettingWritten = false;
      this.SingleMarketSettingWritten = false;
      this.EnforceSettingsReapply = false;

      this.PairsLines = null;
      this.DCALines = null;
      this.IndicatorsLines = null;

      this.SingleMarketTrendChanges = new Dictionary<string, List<MarketTrendChange>>();
      this.GlobalMarketTrendChanges = new Dictionary<string, List<MarketTrendChange>>();
      this.AverageMarketTrendChanges = new Dictionary<string, double>();
      this.SingleMarketSettingsCount = new Dictionary<string, int>();
      this.TriggeredSingleMarketSettings = new Dictionary<string, List<SingleMarketSetting>>();
      this.ExchangeMarketList = null;
      this.MarketList = new List<string>();
      this.LastRuntimeSummary = null;
    }
    #endregion
  }
}
