var statusMarketTrends = [];
var statusGlobalSettings = [];
var statusSingleMarketSettings = [];

var checkedMTButtons = false;
var checkedGSButtons = false;
var checkedSMSButtons = false;

const GSStandardTriggerTemplate = ({ settingType, settingName, trendName, marketTrends, minChange, maxChange }) => `
  <div class="form-group row">
    <div class="col-md-4">
      <select name="MarketAnalyzer_${settingType}_${settingName}|Trigger_${trendName}|MarketTrendName" class="form-control">
        ${marketTrends}
      </select>
      <span class="help-block"><small>Market Trend name</small></span>
    </div>
    <div class="col-md-3">
      <input type="text" class="form-control" placeholder="MinChange in %" name="MarketAnalyzer_${settingType}_${settingName}|Trigger_${trendName}|MinChange" value="${minChange}">
      <span class="help-block"><small>Min. trend change % - <a href="https://github.com/Legedric/ptmagic/wiki/MinChange-&-MaxChange" target="_blank">Read the Wiki!</a></small></span>
    </div>
    <div class="col-md-3">
      <input type="text" class="form-control" placeholder="MaxChange in %" name="MarketAnalyzer_${settingType}_${settingName}|Trigger_${trendName}|MaxChange" value="${maxChange}">
      <span class="help-block"><small>Max. trend change % - <a href="https://github.com/Legedric/ptmagic/wiki/MinChange-&-MaxChange" target="_blank">Read the Wiki!</a></small></span>
    </div>
    <div class="col-md-2"><button class="btn btn-danger btn-custom btn-block text-uppercase waves-effect waves-light btn-remove-parentrow-${settingType}-${settingName}">Remove</button></div>
  </div>
`;

const SMSStandardTriggerTemplate = ({ settingType, settingName, trendName, triggerPrefix, marketTrends, marketTrendRelations, minChange, maxChange }) => `
  <div class="form-group row">
    <div class="col-md-4">
      <select name="MarketAnalyzer_${settingType}_${settingName}|${triggerPrefix}Trigger_${trendName}|MarketTrendName" class="form-control">
        ${marketTrends}
      </select>
      <span class="help-block"><small>Market Trend name</small></span>
    </div>
    <div class="col-md-2">
      <select name="MarketAnalyzer_${settingType}_${settingName}|${triggerPrefix}Trigger_${trendName}|MarketTrendRelation" class="form-control">
        ${marketTrendRelations}
      </select>
      <span class="help-block"><small>Market Trend relation</small></span>
    </div>
    <div class="col-md-2">
      <input type="text" class="form-control" placeholder="MinChange in %" name="MarketAnalyzer_${settingType}_${settingName}|${triggerPrefix}Trigger_${trendName}|MinChange" value="${minChange}">
      <span class="help-block"><small>Min. trend change % - <a href="https://github.com/Legedric/ptmagic/wiki/MinChange-&-MaxChange" target="_blank">Read the Wiki!</a></small></span>
    </div>
    <div class="col-md-2">
      <input type="text" class="form-control" placeholder="MaxChange in %" name="MarketAnalyzer_${settingType}_${settingName}|${triggerPrefix}Trigger_${trendName}|MaxChange" value="${maxChange}">
      <span class="help-block"><small>Max. trend change % - <a href="https://github.com/Legedric/ptmagic/wiki/MinChange-&-MaxChange" target="_blank">Read the Wiki!</a></small></span>
    </div>
    <div class="col-md-2"><button class="btn btn-danger btn-custom btn-block text-uppercase waves-effect waves-light btn-remove-parentrow-${settingType}-${settingName}">Remove</button></div>
  </div>
`;

const CoinAgeTriggerTemplate = ({ settingType, settingName, triggerIndex, coinAgeDays }) => `
  <div class="form-group row">
    <label class="col-md-4 col-form-label">Age Days Lower Than <i class="fa fa-info-circle text-muted" data-toggle="tooltip" data-placement="top" title="The max. age of a coin in days to match this trigger."></i></label>
    <div class="col-md-6">
      <input type="text" class="form-control" placeholder="Days" name="MarketAnalyzer_${settingType}_${settingName}|Trigger_AgeDaysLowerThan${triggerIndex}" value="${coinAgeDays}">
      <span class="help-block"><small>Days since a coin was added to an exchange.</small></span>
    </div>
    <div class="col-md-2"><button class="btn btn-danger btn-custom btn-block text-uppercase waves-effect waves-light btn-remove-parentrow-${settingType}-${settingName}">Remove</button></div>
  </div>
`;

const SMS24hVolumeTriggerTemplate = ({ settingType, settingName, triggerIndex, triggerPrefix, mainMarket, min24hVolume, max24hVolume }) => `
  <div class="form-group row">
    <label class="col-md-4 col-form-label">24h Volume in ${mainMarket} <i class="fa fa-info-circle text-muted" data-toggle="tooltip" data-placement="top" title="The 24h volume of a single coin in ${mainMarket}."></i></label>
    <div class="col-md-3">
      <input type="text" class="form-control" placeholder="Min 24h Volume" name="MarketAnalyzer_${settingType}_${settingName}|${triggerPrefix}Trigger_24hVolume${triggerIndex}|Min24hVolume" value="${min24hVolume}">
      <span class="help-block"><small>Min. 24h Volume</small></span>
    </div>
    <div class="col-md-3">
      <input type="text" class="form-control" placeholder="Max 24h Volume" name="MarketAnalyzer_${settingType}_${settingName}|${triggerPrefix}Trigger_24hVolume${triggerIndex}|Max24hVolume" value="${max24hVolume}">
      <span class="help-block"><small>Max. 24h Volume</small></span>
    </div>
    <div class="col-md-2"><button class="btn btn-danger btn-custom btn-block text-uppercase waves-effect waves-light btn-remove-parentrow-${settingType}-${settingName}">Remove</button></div>
  </div>
`;

const SMSHoursActiveTriggerTemplate = ({ settingType, settingName, triggerIndex, hoursSinceTriggered }) => `
  <div class="form-group row">
    <label class="col-md-4 col-form-label">Hours Since Triggered <i class="fa fa-info-circle text-muted" data-toggle="tooltip" data-placement="top" title="The number of hours after this setting gets off triggered."></i></label>
    <div class="col-md-6">
      <input type="text" class="form-control" placeholder="Hours" name="MarketAnalyzer_${settingType}_${settingName}|OffTrigger_HoursSinceTriggered${triggerIndex}" value="${hoursSinceTriggered}">
      <span class="help-block"><small>Number of hours the setting will be max. active.</small></span>
    </div>
    <div class="col-md-2"><button class="btn btn-danger btn-custom btn-block text-uppercase waves-effect waves-light btn-remove-parentrow-${settingType}-${settingName}">Remove</button></div>
  </div>
`;

const PropertyTemplate = ({ settingType, settingName, propertyType, propertyKey, propertyKeySimple, value, valueModes}) => `
  <div class="form-group row">
    <div class="col-md-4">
      <input type="text" class="form-control" placeholder="Profit Trailer setting" name="MarketAnalyzer_${settingType}_${settingName}|${propertyType}Property_${propertyKeySimple}" value="${propertyKeySimple}">
      <span class="help-block"><small>Any <a href="https://wiki.profittrailer.com/doku.php?id=${propertyType}.properties" target="_blank">variable from PT's settings</a> may be used!</small></span>
    </div>
    <div class="col-md-3">
      <input type="text" class="form-control" placeholder="Value" name="MarketAnalyzer_${settingType}_${settingName}|${propertyType}Property_${propertyKeySimple}|Value" value="${value}">
      <span class="help-block"><small>The value for this setting.</small></span>
    </div>
    <div class="col-md-3">
      <select name="MarketAnalyzer_${settingType}_${settingName}|${propertyType}Property_${propertyKeySimple}|ValueMode" class="form-control">
        ${valueModes}
      </select>
      <span class="help-block"><small>Value mode - <a href="https://github.com/Legedric/ptmagic/wiki/Writing-Properties" target="_blank">Read the Wiki!</a></small></span>
    </div>
    <div class="col-md-2"><button class="btn btn-danger btn-custom btn-block text-uppercase waves-effect waves-light btn-remove-parentrow-${settingType}-${settingName}">Remove</button></div>
  </div>
`;

function checkMTMoveButtons() {
  if (!checkedMTButtons) {
    $('.btn-move-MT').each(function (index) {
      var dataTarget = $(this).data('datatarget');
      var dataDirection = $(this).data('datadirection');

      var currentElement = $('#' + dataTarget).closest('.settings-markettrend');

      if (dataDirection === 'up') {
        if ($(currentElement).is('.settings-markettrend:first-child')) {
          $(this).addClass('hidden');
        } else {
          $(this).removeClass('hidden');
        }
      } else if (dataDirection === 'down') {
        if ($(currentElement).is('.settings-markettrend:last-child')) {
          $(this).addClass('hidden');
        } else {
          $(this).removeClass('hidden');
        }
      }
    });
  }
  checkedMTButtons = true;
}

function checkGSMoveButtons() {
  if (!checkedGSButtons) {
    $('.btn-move-GS').each(function (index) {
      var dataTarget = $(this).data('datatarget');
      var dataDirection = $(this).data('datadirection');

      var currentElement = $('#' + dataTarget).closest('.settings-globalsetting');

      if (dataDirection === 'up') {
        if ($(currentElement).is('.settings-globalsetting:first-child')) {
          $(this).addClass('hidden');
        } else {
          $(this).removeClass('hidden');
        }
      } else if (dataDirection === 'down') {
        if ($(currentElement).is('.settings-globalsetting:last-child')) {
          $(this).addClass('hidden');
        } else {
          $(this).removeClass('hidden');

          // Get next element
          var nextElement = $(currentElement).next('.settings-globalsetting').find('div');
          var nextElementId = $(nextElement).attr('id').toLowerCase().replace('globalsetting_', '');
          if (nextElementId.startsWith('default')) {
            // Hide "Move Down" button when the next setting is the default setting
            $(this).addClass('hidden');
          }
        }
      }
    });
  }
  checkedGSButtons = true;
}

function checkSMSMoveButtons() {
  if (!checkedSMSButtons) {
    $('.btn-move-SMS').each(function (index) {
      var dataTarget = $(this).data('datatarget');
      var dataDirection = $(this).data('datadirection');

      var currentElement = $('#' + dataTarget).closest('.settings-singlemarketsetting');

      if (dataDirection === 'up') {
        if ($(currentElement).is('.settings-singlemarketsetting:first-child')) {
          $(this).addClass('hidden');
        } else {
          $(this).removeClass('hidden');
        }
      } else if (dataDirection === 'down') {
        if ($(currentElement).is('.settings-singlemarketsetting:last-child')) {
          $(this).addClass('hidden');
        } else {
          $(this).removeClass('hidden');
        }
      }
    });
  }
  checkedSMSButtons = true;
}

function checkBuildStatus() {
  var buildCompleted = true;

  for (key in statusMarketTrends) {
    if (!statusMarketTrends[key]) {
      buildCompleted = false;
    }
  }

  if (buildCompleted) {
    for (key in statusGlobalSettings) {
      if (!statusGlobalSettings[key]) {
        buildCompleted = false;
      }
    }
  }

  if (buildCompleted) {
    for (key in statusSingleMarketSettings) {
      if (!statusSingleMarketSettings[key]) {
        buildCompleted = false;
      }
    }
  }

  if (buildCompleted) {
    $('#div-loading-settings').addClass('hidden');
    $('#div-save-settings').removeClass('hidden');

    checkMTMoveButtons();
    checkGSMoveButtons();
    checkSMSMoveButtons();
  }
}

$.fn.buildMarketTrendSettings = function () {
  return this.each(function (index) {
    var element = $(this);
    var marketTrendName = $(this).data('trendname');
    var rootUrl = $(this).data('rooturl');

    if (marketTrendName !== '') {
      statusMarketTrends[marketTrendName] = false;
    }

    $(this).load(rootUrl + '_get/SettingsMarketTrends?mt=' + encodeURIComponent(marketTrendName), '', function (responseText, textStatus, XMLHttpRequest) {
      if (textStatus === 'error') {
        $.Notification.notify('error', 'top left', 'Build MarketTrendSettings failed!', 'PTMagic Monitor failed to build MarketTrendSettings.')
      } else {
        statusMarketTrends[marketTrendName] = true;
        checkBuildStatus();

        $(this).removeClass('new');
        $('[data-toggle="tooltip"]').tooltip();
        var elems = Array.prototype.slice.call(document.querySelectorAll('[data-plugin="switchery"][data-switchery="false"]'));
        elems.forEach(function (html) {
          var switchery = new Switchery(html, { size : 'small' });
        });
      }
    });
  });
};

$.fn.buildGlobalSettings = function () {
  return this.each(function (index) {
    var element = $(this);
    var settingName = $(this).data('settingname');
    var rootUrl = $(this).data('rooturl');

    if (settingName !== '') {
      statusGlobalSettings[settingName] = false;
    }

    $(this).load(rootUrl + '_get/SettingsGlobalSettings?gs=' + encodeURIComponent(settingName), '', function (responseText, textStatus, XMLHttpRequest) {
      if (textStatus === 'error') {
        $.Notification.notify('error', 'top left', 'Build GlobalSettings failed!', 'PTMagic Monitor failed to build GlobalSettings.')
      } else {
        statusGlobalSettings[settingName] = true;
        checkBuildStatus();

        $(this).removeClass('new');
        $('[data-toggle="tooltip"]').tooltip();
        var elems = Array.prototype.slice.call(document.querySelectorAll('[data-plugin="switchery"][data-switchery="false"]'));
        elems.forEach(function (html) {
          var switchery = new Switchery(html, { size: 'small' });
        });
      }
    });
  });
};

$.fn.buildSingleMarketSettings = function () {
  return this.each(function (index) {
    var element = $(this);
    var settingName = $(this).data('settingname');
    var rootUrl = $(this).data('rooturl');

    if (settingName !== '') {
      statusSingleMarketSettings[settingName] = false;
    }

    $(this).load(rootUrl + '_get/SettingsSingleMarketSettings?gs=' + encodeURIComponent(settingName), '', function (responseText, textStatus, XMLHttpRequest) {
      if (textStatus === 'error') {
        $.Notification.notify('error', 'top left', 'Build SingleMarketSettings failed!', 'PTMagic Monitor failed to build SingleMarketSettings.')
      } else {
        statusSingleMarketSettings[settingName] = true;
        checkBuildStatus();

        $(this).removeClass('new');
        $('[data-toggle="tooltip"]').tooltip();
        var elems = Array.prototype.slice.call(document.querySelectorAll('[data-plugin="switchery"][data-switchery="false"]'));
        elems.forEach(function (html) {
          var switchery = new Switchery(html, { size: 'small' });
        });
      }
    });
  });
};