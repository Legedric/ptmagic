
/**
 * Theme: Ubold Admin Template
 * Author: Coderthemes
 * Dashboard 2
 */

!function($) {
    "use strict";

    var Dashboard2 = function() {
        this.$realData = []
    };

    //creates area chart with dotted
    Dashboard2.prototype.createAreaChartDotted = function(element, pointSize, lineWidth, data, xkey, ykeys, labels, Pfillcolor, Pstockcolor, lineColors) {
        Morris.Area({
            element: element,
            pointSize: 0,
            lineWidth: 0,
            data: data,
            xkey: xkey,
            ykeys: ykeys,
            labels: labels,
            hideHover: 'auto',
            pointFillColors: Pfillcolor,
            pointStrokeColors: Pstockcolor,
            resize: true,
            gridLineColor: '#2f3e47',
            gridTextColor: '#98a6ad',
            lineColors: lineColors
        });

    },
        Dashboard2.prototype.init = function() {

            //creating area chart
            var $areaDotData = [
                { y: '2009', a: 10, b: 20, c:30 },
                { y: '2010', a: 20,  b: 45, c:20 },
                { y: '2011', a: 75,  b: 60, c:80 },
                { y: '2012', a: 175,  b: 165, c:130 },
                { y: '2013', a: 50,  b: 40, c:30 },
                { y: '2014', a: 75,  b: 65, c:30 },
                { y: '2015', a: 90, b: 60, c:30 }
            ];
            this.createAreaChartDotted('morris-area-with-dotted', 0, 0, $areaDotData, 'y', ['a', 'b', 'c'], ['Desktops ', 'Tablets ', 'Mobiles '],['#ffffff'],['#999999'], ['#bbbbbb', '#5d9cec','#2dc4b9']);

        },
        //init
        $.Dashboard2 = new Dashboard2, $.Dashboard2.Constructor = Dashboard2
}(window.jQuery),

//initializing
    function($) {
        "use strict";
        $.Dashboard2.init();
    }(window.jQuery);