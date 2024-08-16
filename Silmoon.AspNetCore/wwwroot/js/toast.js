function toast(msg, delay = 1000) {
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