(function () {
    if (window.SiteDialogs) return;
    let active = false;
    let nextDialogId = 0;
    let activeTemplate = null;
    let cancelActive = null;

    // 页面内模态框只限制背景交互，不暂停 JS、SignalR 回调或定时器。
    function confirm(options = {}, template = null) {
        return showDialog(options, false, template).then(result => result === "confirm");
    }

    function choose(options = {}, template = null) {
        return showDialog(options, true, template);
    }

    function enableDragging(dialog) {
        const header = dialog.querySelector("header");
        let drag = null;

        function moveTo(left, top) {
            const bounds = dialog.getBoundingClientRect();
            const margin = 8;
            dialog.style.left = Math.max(margin, Math.min(left, window.innerWidth - bounds.width - margin)) + "px";
            dialog.style.top = Math.max(margin, Math.min(top, window.innerHeight - bounds.height - margin)) + "px";
            dialog.dataset.dragged = "true";
        }
        function start(event) {
            if (event.button !== 0 || event.isPrimary === false || event.target.closest?.("button")) return;
            const bounds = dialog.getBoundingClientRect();
            drag = { pointerId: event.pointerId, x: event.clientX, y: event.clientY, left: bounds.left, top: bounds.top };
            header.setPointerCapture(event.pointerId);
            event.preventDefault();
        }
        function move(event) {
            if (!drag || event.pointerId !== drag.pointerId) return;
            moveTo(drag.left + event.clientX - drag.x, drag.top + event.clientY - drag.y);
        }
        function stop(event) {
            if (!drag || (event?.pointerId !== undefined && event.pointerId !== drag.pointerId)) return;
            const pointerId = drag.pointerId;
            drag = null;
            if (header.hasPointerCapture(pointerId)) header.releasePointerCapture(pointerId);
        }
        function resize() {
            stop();
            if (dialog.dataset.dragged !== "true") return;
            const bounds = dialog.getBoundingClientRect();
            moveTo(bounds.left, bounds.top);
        }
        header.addEventListener("pointerdown", start);
        header.addEventListener("pointermove", move);
        header.addEventListener("pointerup", stop);
        header.addEventListener("pointercancel", stop);
        header.addEventListener("lostpointercapture", stop);
        window.addEventListener("resize", resize);
        window.addEventListener("blur", stop);
        return function () {
            stop();
            header.removeEventListener("pointerdown", start);
            header.removeEventListener("pointermove", move);
            header.removeEventListener("pointerup", stop);
            header.removeEventListener("pointercancel", stop);
            header.removeEventListener("lostpointercapture", stop);
            window.removeEventListener("resize", resize);
            window.removeEventListener("blur", stop);
        };
    }

    function showDialog(options, allowAlternative, suppliedTemplate) {
        if (active || options.signal?.aborted) return Promise.resolve(null);
        const template = suppliedTemplate ?? document.getElementById("site-confirm-dialog-template");
        const dialog = (template?.content.querySelector("dialog") ?? template?.querySelector?.("dialog"))?.cloneNode(true);
        if (!dialog || typeof dialog.showModal !== "function") return Promise.reject(new Error("确认控件未加载或当前浏览器不支持页面对话框。"));

        const title = dialog.querySelector("[data-confirm-title]");
        const message = dialog.querySelector("[data-confirm-message]");
        const dialogId = "site-confirm-" + ++nextDialogId;
        title.id = dialogId + "-title";
        message.id = dialogId + "-message";
        dialog.setAttribute("aria-labelledby", title.id);
        dialog.setAttribute("aria-describedby", message.id);
        title.textContent = options.title ?? "确认操作";
        message.textContent = options.message ?? "是否继续？";
        dialog.querySelector("[data-confirm-accept]").textContent = options.confirmText ?? "确认";
        dialog.querySelector("[data-confirm-cancel-label]").textContent = options.cancelText ?? "取消";
        const alternative = dialog.querySelector("[data-confirm-alternative]");
        alternative.textContent = allowAlternative ? String(options.alternativeText ?? "") : "";
        alternative.hidden = !alternative.textContent.trim();
        dialog.dataset.danger = String(options.danger === true);
        active = true;
        activeTemplate = template;

        return new Promise(function (resolve, reject) {
            let completed = false;
            let disposeDragging = null;
            const observer = new MutationObserver(function () { if (!dialog.isConnected) finish(null); });

            function cleanup() {
                completed = true;
                disposeDragging?.();
                observer.disconnect();
                options.signal?.removeEventListener("abort", cancel);
                window.removeEventListener("pagehide", cancel);
                dialog.removeEventListener("click", handleClick);
                dialog.removeEventListener("cancel", handleCancel);
                dialog.removeEventListener("close", cancel);
                if (dialog.open) dialog.close();
                dialog.remove();
                active = false;
                activeTemplate = null;
                cancelActive = null;
            }

            function finish(result) {
                if (completed) return;
                cleanup();
                resolve(result);
            }

            function cancel() { finish(null); }
            cancelActive = cancel;
            function handleCancel(event) { event.preventDefault(); cancel(); }
            function handleClick(event) {
                const button = event.target.closest?.("button");
                if (!button || !dialog.contains(button)) return;
                if (button.disabled || button.hidden || !button.getClientRects().length || window.getComputedStyle(button).visibility !== "visible") return;
                if (Object.hasOwn(button.dataset, "confirmAccept")) finish("confirm");
                else if (allowAlternative && button === alternative) finish("alternative");
                else if (Object.hasOwn(button.dataset, "confirmCancel")) cancel();
            }

            dialog.addEventListener("click", handleClick);
            dialog.addEventListener("cancel", handleCancel);
            dialog.addEventListener("close", cancel);
            options.signal?.addEventListener("abort", cancel, { once: true });
            window.addEventListener("pagehide", cancel, { once: true });
            try {
                document.body.appendChild(dialog);
                observer.observe(document.body, { childList: true });
                dialog.showModal();
                if (options.draggable !== false) disposeDragging = enableDragging(dialog);
                dialog.querySelector("[data-confirm-cancel-label]").focus();
            }
            catch (error) {
                cleanup();
                reject(error);
            }
        });
    }

    function dismiss(templateId) {
        if (templateId && templateId === activeTemplate?.id) cancelActive?.();
    }

    window.SiteDialogs = { confirm: confirm, choose: choose, dismiss: dismiss };
})();
