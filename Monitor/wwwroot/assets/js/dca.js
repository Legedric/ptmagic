$('.dca-input').on('input', function (e) {
  var dcaMode = $('.btn-dca-mode.btn-dca-mode-active').data('dca-mode');

  var totalCap = parseFloat($('#dca-capital').val());
  var pairs = parseFloat($('#dca-pairs').val());
  var dcaLevels = parseFloat($('#dca-levels').val());
  var maxCost = parseFloat($('#dca-max-cost').val());
  var maxCostType = $('#dca-max-cost-type').val();
  if (maxCostType == 2) {
    maxCost = totalCap * maxCost / 100;
  }

  var startPrice = maxCost;
  for (var c = 0; c <= dcaLevels; c++) {
    var initialBuyValue = parseFloat($('.dca-trigger-0').data('triggervalue'));
    var totalDrop = initialBuyValue;
    var lastAvgPrice = startPrice;
    var lastBuyPrice = startPrice
    var totalCoins = 1;
    var costPerPairAdvanced = maxCost;
    var triggerCount = 0;

    var lastPercentageValue = 0;
    for (var t = 1; t <= c; t++) {
      var triggerValue = parseFloat($('.dca-trigger-' + t).data('triggervalue'));
      var percentageValue = parseFloat($('.dca-percentage-' + t).data('percentagevalue'));
      if (isNaN(percentageValue)) {
        percentageValue = lastPercentageValue;
      }

      if (isNaN(triggerValue)) {
        triggerValue = "";
        break;
      } else {
        triggerCount++;
        lastBuyPrice = lastAvgPrice + (lastAvgPrice * triggerValue / 100);
        var buyCoins = (totalCoins * percentageValue / 100);
        var buyCost = buyCoins * lastBuyPrice;

        costPerPairAdvanced = costPerPairAdvanced + buyCost;
        totalCoins = totalCoins + buyCoins;

        lastAvgPrice = costPerPairAdvanced / totalCoins;

        totalDrop = initialBuyValue - Math.round10((startPrice - lastBuyPrice) / startPrice * 100, -2);

        //console.log("DCA=" + c + " --- t=" + t + ", triggerValue=" + triggerValue + ", lastBuyPrice=" + lastBuyPrice + ", totalCostAdvanced=" + totalCostAdvanced + ", totalCoins=" + totalCoins);
      }

      lastPercentageValue = percentageValue;
    }

    $('.dca-drop-1-' + c).html(totalDrop.toFixed(2) + '%');

    for (var r = 0; r <= $('#dca-maxpairs').val(); r++) {
      getDCACost(maxCost, c, r, '.dca-' + r + '-' + c, dcaMode, triggerCount, costPerPairAdvanced);
    }
  }

  var costPerPair = Math.round10(getDCACostPerPairSimple(maxCost, dcaLevels), -8);
  var totalCost = getDCACost(maxCost, dcaLevels, pairs, '', dcaMode, triggerCount, costPerPairAdvanced);

  if (dcaMode == "advanced") {
    costPerPair = costPerPairAdvanced;
  }

  var result = Math.round10(totalCap - totalCost, -8);
  var resultPercent = Math.round10(result / totalCap * 100, -2);

  if (totalCap > 0 && (isFloat(result) || Number.isInteger(result))) {
    $('#dca-cost-pair').html(costPerPair.toFixed(8));
    $('#dca-capital-needed').html(totalCost.toFixed(8));
    $('#dca-result').html(result.toFixed(8));
    $('#dca-result-percent').html(resultPercent + '%');

    $('#dca-result,#dca-result-percent').removeClass('text-success');
    $('#dca-result,#dca-result-percent').removeClass('text-danger');
    if (result > 0) {
      $('#dca-result,#dca-result-percent').addClass('text-success');
    } else {
      $('#dca-result,#dca-result-percent').addClass('text-danger');
    }

    $('#dca-noresult').css('display', 'none');
    $('#dca-result-table').css('display', 'table');
  }
});

function getDCACostPerPairSimple(initialCost, dcaLevel) {
  var result = initialCost;

  for (var t = 1; t <= dcaLevel; t++) {
    var percentageValue = parseFloat($('.dca-percentage-' + t).data('percentagevalue'));
    if (isNaN(percentageValue)) {
      percentageValue = 100;
    }

    var buyCost = (result * percentageValue / 100);
    result = result + buyCost;

    //console.log("DCA=" + c + " --- t=" + t + ", triggerValue=" + triggerValue + ", lastBuyPrice=" + lastBuyPrice + ", totalCostAdvanced=" + totalCostAdvanced + ", totalCoins=" + totalCoins);
  }

  return result;
}

function getDCACost(maxCost, dcaLevel, pairs, container, dcaMode, triggerCount, costPerPairAdvanced) {
  var costPerPairSimple = Math.round10(getDCACostPerPairSimple(maxCost, dcaLevel), -8);
  var totalCostSimple = Math.round10(costPerPairSimple * pairs, -8);
  var totalCostAdvanced = Math.round10(costPerPairAdvanced * pairs, -8);
  var totalCapAvailable = parseFloat($('#dca-capital').val());
  var result = totalCostSimple;

  if (dcaMode == "advanced") {
    result = totalCostAdvanced;
  }

  if (isFloat(result) || Number.isInteger(result)) {
    if (container != '') {
      $(container + '-simp').html(totalCostSimple.toFixed(8));
      $(container + '-adv').html(costPerPairAdvanced.toFixed(8));

      if (dcaMode == "advanced") {
        if (dcaLevel > triggerCount) {
          result = "";
        }
      }

      if (result == "") {
        $(container).html(result);
      } else {
        $(container).html(result.toFixed(8));
      }

      $(container).removeClass('text-success');
      $(container).removeClass('text-danger');
      if (result < totalCapAvailable) {
        $(container).addClass('text-success');
      } else {
        $(container).addClass('text-danger');
      }
    }
  }

  return result;
}