(function () {

  /**
   * Decimal adjustment of a number.
   *
   * @param   {String}    type    The type of adjustment.
   * @param   {Number}    value   The number.
   * @param   {Integer}   exp     The exponent (the 10 logarithm of the adjustment base).
   * @returns {Number}            The adjusted value.
   */
  function decimalAdjust(type, value, exp) {
    // If the exp is undefined or zero...
    if (typeof exp === 'undefined' || +exp === 0) {
      return Math[type](value);
    }
    value = +value;
    exp = +exp;
    // If the value is not a number or the exp is not an integer...
    if (isNaN(value) || !(typeof exp === 'number' && exp % 1 === 0)) {
      return NaN;
    }
    // Shift
    value = value.toString().split('e');
    value = Math[type](+(value[0] + 'e' + (value[1] ? (+value[1] - exp) : -exp)));
    // Shift back
    value = value.toString().split('e');
    return +(value[0] + 'e' + (value[1] ? (+value[1] + exp) : exp));
  }

  // Decimal round
  if (!Math.round10) {
    Math.round10 = function (value, exp) {
      return decimalAdjust('round', value, exp);
    };
  }
  // Decimal floor
  if (!Math.floor10) {
    Math.floor10 = function (value, exp) {
      return decimalAdjust('floor', value, exp);
    };
  }
  // Decimal ceil
  if (!Math.ceil10) {
    Math.ceil10 = function (value, exp) {
      return decimalAdjust('ceil', value, exp);
    };
  }
})();

function isFloat(n) {
  return Number(n) === n && n % 1 !== 0;
}

$.fn.autocolor = function (isSawTableCell = false) {
  return this.each(function (index) {
    var element = $(this);

    if (isSawTableCell) {
      element = $(".tablesaw-cell-content", this);
    }

    var text = $(element).text().replace(/[^\d.,-]/g, '');
    var floatVal = parseFloat(text);

    element.removeClass('text-danger');
    element.removeClass('text-success');
    if (floatVal > 0) {
      element.addClass('text-success');
    } else {
      element.addClass('text-danger');
    }
  });
  
};