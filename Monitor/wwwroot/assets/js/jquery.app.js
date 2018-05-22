/**
 * Theme: Adminox Admin Template
 * Author: Coderthemes
 * Module/App: Main Js
 */


(function ($) {

    'use strict';

    function initNavbar() {

        $('.navbar-toggle').on('click', function (event) {
            $(this).toggleClass('open');
            $('#navigation').slideToggle(400);
        });

        $('.navigation-menu>li').slice(-2).addClass('last-elements');

        $('.navigation-menu li.has-submenu a[href="#"]').on('click', function (e) {
            if ($(window).width() < 992) {
                e.preventDefault();
                $(this).parent('li').toggleClass('open').find('.submenu:first').toggleClass('open');
            }
        });
    }
    function initScrollbar() {
        $('.slimscroll-noti').slimScroll({
            height: '230px',
            position: 'right',
            size: "5px",
            color: '#98a6ad',
            wheelStep: 10
        });
        $('.slimscroller').slimScroll({
            height: 'auto',
            position: 'right',
            size: "5px",
            color: '#98a6ad'
        });
        $('#todo-list').slimScroll({
            height: '280px',
            color: '#98a6ad',
            size: "5px"
        });
        $('.conversation-list').slimScroll({
            height: '340px',
            color: '#98a6ad',
            size: "5px"
        });
        $('.inbox-widget').slimScroll({
            height: '380px',
            color: '#98a6ad',
            size: "5px"
        });
        $('.recent-activities').slimScroll({
            height: '355px',
            color: '#98a6ad',
            size: "5px"
        });
    }
    // === following js will activate the menu in left side bar based on url ====
    function initMenuItem() {
        $(".navigation-menu a").each(function () {
            if (this.href == window.location.href) {
                $(this).parent().addClass("active"); // add active to li of the current link
                $(this).parent().parent().parent().addClass("active"); // add active class to an anchor
                $(this).parent().parent().parent().parent().parent().addClass("active"); // add active class to an anchor
            }
        });
    }
    function init() {
        initNavbar();
        initScrollbar();
        initMenuItem();
    }

    init();

})(jQuery);



