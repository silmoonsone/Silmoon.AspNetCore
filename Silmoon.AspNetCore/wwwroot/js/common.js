function getQueryStringByName(name, url) {
    if (typeof (url) == "undefined") var url = location.search;
    var result = url.match(new RegExp("[\?\&]" + name + "=([^\&]+)", "i"));
    if (result == null || result.length < 1) {
        return "";
    }
    return result[1];
}
function setDisableCountdown(ele, text, seconds, CountDownCompletedCallback) {
    var sText = ele.innerText;
    ele.innerText = text.replace("{0}", seconds);

    if (ele.tagName.toUpperCase() == "A") {
        ele.style.pointerEvents = "none";
    } else {
        ele.disabled = true;
    }
    setTimeout(_disableCountdown, 1000);
    function _disableCountdown() {
        if (seconds > 0) {
            seconds--;
            ele.disabled = true;
            ele.innerText = text.replace("{0}", seconds);
            setTimeout(_disableCountdown, 1000);
        } else {
            if (ele.tagName.toUpperCase() == "A") {
                ele.style.pointerEvents = "";
            } else {
                ele.disabled = false;
            }
            ele.innerText = sText;
            if (typeof (CountDownCompletedCallback) == "function") {
                CountDownCompletedCallback(ele);
            }
        }
    }
}
function setDisableCountdown2(id, text, seconds, CountDownCompletedCallback) {
    var ele = document.getElementById(id);
    var sText = ele.innerText;
    ele.innerText = text.replace("{0}", seconds);

    if (ele.tagName.toUpperCase() == "A") {
        ele.style.pointerEvents = "none";
    } else {
        ele.disabled = true;
    }
    setTimeout(_disableCountdown, 1000);
    function _disableCountdown() {
        if (seconds > 0) {
            seconds--;
            ele.disabled = true;
            ele.innerText = text.replace("{0}", seconds);
            setTimeout(_disableCountdown, 1000);
        } else {
            if (ele.tagName.toUpperCase() == "A") {
                ele.style.pointerEvents = "";
            } else {
                ele.disabled = false;
            }
            ele.innerText = sText;
            if (typeof (CountDownCompletedCallback) == "function") {
                CountDownCompletedCallback(ele);
            }
        }
    }
}
function switchDisplay(element1, element2) {
    if ($("#" + element1).css("display") != "none") {
        $("#" + element1).fadeToggle("fast", function () {
            $("#" + element2).fadeToggle();
        });
    } else {
        $("#" + element2).fadeToggle("fast", function () {
            $("#" + element1).fadeToggle();
        });
    }
}

function ajaxFileUploadPost(elementId, url, successCallback, errorCallback, processCallback) {
    var fileObj = $(elementId)[0];
    if (typeof (fileObj.files[0]) == "undefined") return;
    var f = new FormData();
    f.append(fileObj.name, fileObj.files[0]);
    ajaxPostFormData(f, url, successCallback, errorCallback, processCallback);
    fileObj.value = null;
}
function ajaxPostFormData(data, url, successCallback, errorCallback, processCallback) {
    $.ajax({
        url: url,
        type: "POST",
        data: data,
        processData: false,
        contentType: false,
        success: function (e) {
            if (typeof (successCallback) == "function") successCallback(e);

        },
        error: function (e) {
            if (typeof (errorCallback) == "function") errorCallback(e);
        },
        xhr: function () {
            myXhr = $.ajaxSettings.xhr();
            if (myXhr.upload) {
                myXhr.upload.addEventListener('progress', function (e) {
                    var percent = Math.floor(100 * e.loaded / e.total);
                    if (typeof (processCallback) == "function") processCallback(percent, e);
                }, false);
            }
            return myXhr;
        },
    });
}
