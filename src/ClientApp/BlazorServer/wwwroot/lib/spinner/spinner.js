/*
$(window).on('load', function () {
    // Animate loader off screen
    $("#spinner-container").show();
});

$(document).ready(function () {
    
});

$(document).bind('ajaxStart', function () {
    $("#spinner-container").show();
}).bind('ajaxStop', function () {
    $("#spinner-container").hide();
});
*/

// global AJAX loading varible
window.ajax_loading = false;
const spinner = document.getElementById('spinner-container');

function HideSpinner() {
    spinner.style.display = 'none';
}

function ShowSpinner() {
    spinner.style.display = '';
}

$(document)
    .ajaxStart(function () {
        window.ajax_loading = true;
        ShowSpinner();
    })
    .ajaxStop(function () {
        window.ajax_loading = false;
        HideSpinner();
    });

// Place in header (do not use async or defer)
document.addEventListener('readystatechange', event => {
    switch (document.readyState) {
        //case "loading":
        //    $("#spinner-container").show();
        //    break;
        //case "interactive":
        //    $("#spinner-container").show();
        //    break;
        case "complete":
            {
                // hide spinner only when ajax is not running
                // when ajax is running, spinner will hide auto after ajaxStop
                if (!window.ajax_loading) {
                    HideSpinner();
                }
                break;
            }
        default:
            ShowSpinner();
            break;
    }
});

