// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function alert(message) {
    return window.alert(message);
}

export function confirm(message) {
    return window.confirm(message);
}

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

export async function copyText(text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (e) {
        return false;
    }
}

export function copyElementText(elementId, clearSelected) {
    const element = document.getElementById(elementId);
    if (!element) return false;

    const selection = window.getSelection();
    const range = document.createRange();
    range.selectNodeContents(element);

    selection.removeAllRanges();
    selection.addRange(range);

    try {
        const success = document.execCommand('copy');
        return success;
    } catch (err) {
        return false;
    } finally {
        if (clearSelected)
            selection.removeAllRanges();
    }
}

/**
 * Blazor call web browser download file
 * @param {string} fileName
 * @param {string | Uint8Array} content
 * @param {string} contentType
 * @param {string} contentTypeDescription
 * @returns
 */
export async function blazorDownloadFile(fileName, content, contentType, contentTypeDescription) {
    if (!window.showSaveFilePicker) {
        console.error("File System Access API is not supported in this browser.");
        window.alert("File System Access API is not supported in this browser.");
        return { state: false, data: null, message: "File System Access API is not supported in this browser." };
    }

    if (contentTypeDescription === null || contentTypeDescription === undefined) {
        contentTypeDescription = contentType;
    }

    try {
        const fileHandle = await window.showSaveFilePicker({
            suggestedName: fileName,
            types: [{
                description: contentTypeDescription,
                accept: { [contentType]: ['.' + fileName.split('.').pop()] }
            }]
        });

        const writable = await fileHandle.createWritable();
        await writable.write(content);
        await writable.close();
        return { state: true, data: null, message: null };
        ;
    } catch (error) {
        console.error('下载文件出错:', error);
        return { state: false, data: null, message: error.message };
    }
}