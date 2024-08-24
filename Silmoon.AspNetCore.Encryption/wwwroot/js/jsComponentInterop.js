// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function showPrompt(message) {
    return prompt(message, 'Type anything here');
}

export function metroUIConfirm(title, msg, isConfirmDialog, dotNetObjRef) {
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

    if (!isConfirmDialog) $("#metroUIConfirmCancelButton").hide();

    $("#metroUIConfirmOKButton").click(function (e) {
        a.fadeOut(function () {
            if (typeof dotNetObjRef != "undefined" || dotNetObjRef != null) dotNetObjRef.invokeMethodAsync('InvokeCallback', true);
            a.remove();
        });
    }).focus();
    $("#metroUIConfirmCancelButton").click(function (e) {
        a.fadeOut(function () {
            if (typeof dotNetObjRef != "undefined" || dotNetObjRef != null) dotNetObjRef.invokeMethodAsync('InvokeCallback', false);
            a.remove();
        });
    });
}

export function toast(msg, delay = 1000) {
    var _toast_id = "toast_" + Math.ceil(Math.random() * 9999);
    var a = $("<div id='" + _toast_id + "' style='display:none;'>" +
        "<div>" + msg + "</div>");
    a.appendTo("body");

    $("#" + _toast_id).css("position", "fixed");
    $("#" + _toast_id).css("background-color", "rgba(220,220,220,0.9)");
    $("#" + _toast_id).css("box-shadow", "5px 5px 10px rgba(0,0,0,0.2)");
    $("#" + _toast_id).css("top", "80%");
    $("#" + _toast_id).css("left", "50%");
    $("#" + _toast_id).css("transform", "translate(-50%,-50%)");
    $("#" + _toast_id).css("z-index", "100000");
    $("#" + _toast_id).css("border-radius", "3px 4px");
    $("#" + _toast_id).css("padding", "10px");
    $("#" + _toast_id).css("color", "black");

    $("#" + _toast_id).fadeIn(100);

    setTimeout(function () {
        $(a).fadeOut(function () { a.remove(); })
    }, delay);
}