// JavaScript Document
// MetroUI Confirm MessageBox.
// Author : SILMOON
// Website : silmoon.com
function MetroUIConfirm(title, msg, isConfirm, callback) {
    var a = $("<div id='metroUIConfirm' style='display:none;'>" +
        "<div id='metroUIConfirmOuter'>" +
        "<div id='metroUIConfirmInner'>" +
        "<div id='metroUIConfirmTitle'>" + title + "</div>" +
        "<div id='metroUIConfirmMsg'>" + msg + "</div>" +
        "<div id='metroUIConfirmController'>" +
        "<input id='metroUIConfirmOKButton' type='button' value='确定' class='metroUIConfirmButton'/>" +
        "<input id='metroUIConfirmCancelButton' type='button' value='取消' class='metroUIConfirmButton'/>" +
        "</div>" +
        "</div>" +
        "</div>").appendTo("body");
    $("#metroUIConfirm").fadeIn(200);

    if (!isConfirm) $("#metroUIConfirmCancelButton").hide();

    $("#metroUIConfirmOKButton").click(function (e) {
        a.fadeOut(function () {
            if (callback != null) callback(true);
            a.remove();
        });
    }).focus();
    $("#metroUIConfirmCancelButton").click(function (e) {
        a.fadeOut(function () {
            if (callback != null) callback(false);
            a.remove();
        });
    });
}