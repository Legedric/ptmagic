using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Main {
  public static class Constants {
    // Minimales Datum (für NULL)
    public static DateTime confMinDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
    public static DateTime confMaxDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue;

    public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public const string WhiteListMinimal = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    public const string WhiteListNames = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_";
    public const string WhiteListProperties = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-.,_ ";

    public const string PTPathTrading = "trading";

    public const string PTMagicPathData = "_data";
    public const string PTMagicPathPresets = "_presets";
    public const string PTMagicPathCoinMarketCap = "CoinMarketCap";
    public const string PTMagicPathExchange = "Exchange";
    public const string PTMagicPathLogs = "_logs";

    public const int PTMagicBotState_Idle = 0;
    public const int PTMagicBotState_Running = 1;

    public const double MaxTrendChange = 10000.0;
    public const double MinTrendChange = -100.0;

    public const double Max24hVolume = Double.MaxValue;

    public const int ValueModeDefault = 0;
    public const int ValueModeOffset = 1;
    public const int ValueModeOffsetPercent = 2;

    public const string MarketTrendRelationRelative = "Relative";
    public const string MarketTrendRelationAbsolute = "Absolute";
    public const string MarketTrendRelationRelativeTrigger = "RelativeTrigger";

    public static readonly string[] ChartLineColors = new string[] { "#e67e22", "#5d9cec", "#fb6d9d", "#ffffff", "#81c868", "#f05050", "#34d3eb", "#ffbd4a", "#dcdcdc", "#ef1442", "#d73d76", "#9b31c9", "#52e9f1", "#c9b56e", "#b49ec1", "#9885f3", "#85748a", "#85748a", "#85748a", "#b57a4b" };
  }
}
