
/**
 * Theme: Ubold Admin Template
 * Author: Coderthemes
 * Demo: Editable (Inline editing)
 *
 */

$(function(){

    //modify buttons style
    $.fn.editableform.buttons =
        '<button type="submit" class="btn btn-primary editable-submit btn-sm waves-effect waves-light"><i class="md md-check"></i></button>' +
        '<button type="button" class="btn btn-danger editable-cancel btn-sm waves-effect"><i class="md md-close"></i></button>';

    //Inline editables
    $('#inline-username').editable({
        type: 'text',
        pk: 1,
        name: 'username',
        title: 'Enter username',
        mode: 'inline'
    });

    $('#inline-firstname').editable({
        validate: function(value) {
            if($.trim(value) == '') return 'This field is required';
        },
        mode: 'inline'
    });

    $('#inline-sex').editable({
        prepend: "not selected",
        mode: 'inline',
        source: [
            {value: 1, text: 'Male'},
            {value: 2, text: 'Female'}
        ],
        display: function(value, sourceData) {
            var colors = {"": "gray", 1: "green", 2: "blue"},
                elem = $.grep(sourceData, function(o){return o.value == value;});

            if(elem.length) {
                $(this).text(elem[0].text).css("color", colors[value]);
            } else {
                $(this).empty();
            }
        }
    });

    $('#inline-group').editable({
        showbuttons: false,
        mode: 'inline'
    });

    $('#inline-status').editable({
        mode: 'inline'
    });

    $('#inline-dob').editable({
        mode: 'inline'
    });

    $('#inline-event').editable({
        placement: 'right',
        mode: 'inline',
        combodate: {
            firstItem: 'name'
        }
    });

    $('#inline-comments').editable({
        showbuttons: 'bottom',
        mode: 'inline'
    });

    $('#inline-fruits').editable({
        pk: 1,
        limit: 3,
        mode: 'inline',
        source: [
            {value: 1, text: 'Banana'},
            {value: 2, text: 'Peach'},
            {value: 3, text: 'Apple'},
            {value: 4, text: 'Watermelon'},
            {value: 5, text: 'Orange'}
        ]
    });

    $('#inline-tags').editable({
        inputclass: 'input-large',
        mode: 'inline',
        select2: {
            tags: ['html', 'javascript', 'css', 'ajax'],
            tokenSeparators: [",", " "]
        }
    });

});