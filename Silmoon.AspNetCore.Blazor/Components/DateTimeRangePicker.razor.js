(function () {
    function localValue(date, includeTime = true) {
        const pad = (value, width = 2) => String(value).padStart(width, "0");
        const day = pad(date.getFullYear(), 4) + "-" + pad(date.getMonth() + 1) + "-" + pad(date.getDate());
        return includeTime ? day + "T" + pad(date.getHours()) + ":" + pad(date.getMinutes()) + ":" + pad(date.getSeconds()) + "." + pad(date.getMilliseconds(), 3) : day;
    }

    function parseLocal(value, includeTime = true, endOfDay = false) {
        if (!includeTime) {
            if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) return null;
            const date = new Date(value + "T00:00:00.000");
            if (!Number.isFinite(date.getTime()) || date.getFullYear() < 1 || localValue(date, false) !== value) return null;
            if (endOfDay) {
                date.setDate(date.getDate() + 1);
                date.setHours(0, 0, 0, 0);
                date.setTime(date.getTime() - 1);
            }
            return date.getUTCFullYear() >= 1 && date.getUTCFullYear() <= 9999 ? date : null;
        }
        if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}(?::\d{2}(?:\.\d{1,3})?)?$/.test(value)) return null;
        const date = new Date(value);
        if (!Number.isFinite(date.getTime()) || date.getFullYear() < 1 || date.getFullYear() > 9999 || date.getUTCFullYear() < 1 || date.getUTCFullYear() > 9999) return null;
        const normalized = value.length === 16 ? value + ":00.000" : value.length === 19 ? value + ".000" : value.padEnd(23, "0");
        return localValue(date) === normalized ? date : null;
    }

    function presetRange(preset, now) {
        let start = new Date(now.getFullYear(), now.getMonth(), now.getDate());
        let nextDay = new Date(now.getFullYear(), now.getMonth(), now.getDate() + 1);
        switch (preset) {
            case "today": break;
            case "yesterday": nextDay = new Date(start); start.setDate(start.getDate() - 1); break;
            case "threeDays": start.setDate(start.getDate() - 2); break;
            case "thisWeek": start.setDate(start.getDate() - (start.getDay() + 6) % 7); break;
            case "lastWeek":
                start.setDate(start.getDate() - (start.getDay() + 6) % 7);
                nextDay = new Date(start);
                start.setDate(start.getDate() - 7);
                break;
            case "thisMonth": start.setDate(1); break;
            case "thirtyDays": start = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 29); break;
            case "lastMonth":
                start.setDate(1);
                nextDay = new Date(start);
                start.setMonth(start.getMonth() - 1);
                break;
            case "threeMonths":
            case "sixMonths":
            case "oneYear": {
                const day = start.getDate();
                const months = preset === "threeMonths" ? 3 : preset === "sixMonths" ? 6 : 12;
                start.setDate(1);
                start.setMonth(start.getMonth() - months);
                const lastDay = new Date(start.getFullYear(), start.getMonth() + 1, 0).getDate();
                start.setDate(Math.min(day, lastDay));
                break;
            }
            default: return null;
        }
        return { startTime: start.toISOString(), endTime: new Date(nextDay.getTime() - 1).toISOString() };
    }

    function mount(root, options = {}) {
        let messageSource;
        let messages = {};
        function text(key) {
            const current = root?.dataset?.dateRangeMessages;
            if (current !== messageSource) {
                messageSource = current;
                messages = current ? JSON.parse(current) : {};
            }
            return typeof messages[key] === "string" && messages[key] ? messages[key] : key;
        }
        const trigger = root?.querySelector("[data-date-range-open]");
        const dialog = root?.querySelector("[data-date-range-dialog]");
        const form = root?.querySelector("[data-date-range-form]");
        const startInput = root?.querySelector("[data-date-range-start]");
        const endInput = root?.querySelector("[data-date-range-end]");
        const error = root?.querySelector("[data-date-range-error]");
        const presets = root ? [...root.querySelectorAll("[data-date-range-preset]")] : [];
        if (!trigger || !dialog || !form || !startInput || !endInput || !error || typeof dialog.showModal !== "function") throw new Error(text("时间范围控件未加载或当前浏览器不支持页面对话框。"));

        const lifetime = new AbortController();
        const includeTime = startInput.type !== "date";
        let committed = null;
        let draft = null;
        let selectedPreset = "all";
        let disposed = false;
        let pendingApply = 0;
        let disabledControls = [];

        function restoreControls() {
            for (const [control, disabled] of disabledControls) control.disabled = disabled;
            disabledControls = [];
            form.setAttribute("aria-busy", "false");
        }

        function showError(message) {
            error.textContent = message;
            error.hidden = !message;
        }

        function showSelection() {
            for (const button of presets) button.setAttribute("aria-pressed", String(button.dataset.dateRangePreset === selectedPreset));
        }

        function showDraft() {
            startInput.value = draft ? localValue(new Date(draft.startTime), includeTime) : "";
            endInput.value = draft ? localValue(new Date(draft.endTime), includeTime) : "";
            showSelection();
            showError("");
        }

        function close() {
            pendingApply++;
            restoreControls();
            if (dialog.open) dialog.close();
            trigger.setAttribute("aria-expanded", "false");
        }

        function open() {
            if (disposed || dialog.open) return;
            draft = committed ? { ...committed } : null;
            selectedPreset = draft ? "" : "all";
            if (draft) {
                for (const button of presets) {
                    const candidate = presetRange(button.dataset.dateRangePreset, new Date());
                    if (candidate?.startTime === draft.startTime && candidate.endTime === draft.endTime) selectedPreset = button.dataset.dateRangePreset;
                }
            }
            showDraft();
            dialog.showModal();
            trigger.setAttribute("aria-expanded", "true");
            (presets.find(button => button.dataset.dateRangePreset === selectedPreset) || startInput).focus();
        }

        function handleInput() {
            selectedPreset = "";
            showSelection();
            showError("");
        }

        function handleClick(event) {
            const button = event.target.closest?.("button");
            if (!button || !dialog.contains(button) || button.disabled) return;
            if (Object.hasOwn(button.dataset, "dateRangeCancel")) close();
            else if (Object.hasOwn(button.dataset, "dateRangeApply")) submit(event);
            else if (Object.hasOwn(button.dataset, "dateRangePreset")) {
                selectedPreset = button.dataset.dateRangePreset;
                draft = presetRange(selectedPreset, new Date());
                if (draft) draft.label = button.textContent.trim();
                showDraft();
            }
        }

        async function submit(event) {
            event.preventDefault();
            if (disposed || !dialog.open || disabledControls.length) return;
            let range = null;
            if (selectedPreset !== "all") {
                const start = parseLocal(startInput.value, includeTime);
                const end = parseLocal(endInput.value, includeTime, true);
                if (!start || !end) {
                    showError(text(includeTime ? "请选择有效的开始时间和结束时间。" : "请选择有效的开始日期和结束日期。"));
                    (!start ? startInput : endInput).focus();
                    return;
                }
                if (start > end) {
                    showError(text(includeTime ? "结束时间不能早于开始时间。" : "结束日期不能早于开始日期。"));
                    endInput.focus();
                    return;
                }
                range = { startTime: start.toISOString(), endTime: end.toISOString(), label: presets.find(button => button.dataset.dateRangePreset === selectedPreset)?.textContent.trim() || text("自定义") };
            }
            if (!options.commitOnApply) {
                close();
                options.onApply?.(range);
                return;
            }
            const applyVersion = ++pendingApply;
            disabledControls = [...root.querySelectorAll("button, input")].map(control => [control, control.disabled]);
            for (const [control] of disabledControls) control.disabled = true;
            form.setAttribute("aria-busy", "true");
            try {
                const accepted = await options.onApply?.(range);
                if (disposed || applyVersion !== pendingApply) return;
                if (accepted === false) showError(text("日期范围未应用，请重试。"));
                else {
                    setValue(range);
                    close();
                }
            } catch {
                if (!disposed && applyVersion === pendingApply) showError(text("日期范围应用失败，请重试。"));
            } finally {
                if (applyVersion === pendingApply) restoreControls();
            }
        }

        function setValue(range) {
            if (disposed) return;
            committed = range ? { ...range, startTime: new Date(range.startTime).toISOString(), endTime: new Date(range.endTime).toISOString() } : null;
            const label = committed ? (committed.label || text("自定义")) + " · " + localValue(new Date(committed.startTime), includeTime).replace("T", " ") + " — " + localValue(new Date(committed.endTime), includeTime).replace("T", " ") : text("全部时段");
            trigger.title = text(includeTime ? "选择时间范围：{0}" : "选择日期范围：{0}").replace("{0}", label);
            trigger.setAttribute("aria-label", trigger.title);
            trigger.dataset.filtered = String(!!committed);
        }

        trigger.addEventListener("click", open, { signal: lifetime.signal });
        dialog.addEventListener("click", handleClick, { signal: lifetime.signal });
        dialog.addEventListener("cancel", event => { event.preventDefault(); if (!disabledControls.length) close(); }, { signal: lifetime.signal });
        dialog.addEventListener("close", () => trigger.setAttribute("aria-expanded", String(dialog.open)), { signal: lifetime.signal });
        form.addEventListener("submit", submit, { signal: lifetime.signal });
        form.addEventListener("keydown", event => {
            if (event.key === "Enter" && (event.target === startInput || event.target === endInput)) {
                event.stopPropagation();
                submit(event);
            }
        }, { signal: lifetime.signal });
        startInput.addEventListener("input", handleInput, { signal: lifetime.signal });
        endInput.addEventListener("input", handleInput, { signal: lifetime.signal });
        window.addEventListener("pagehide", close, { signal: lifetime.signal });

        return {
            setValue: setValue,
            dispose: function () {
                if (disposed) return;
                disposed = true;
                close();
                lifetime.abort();
            }
        };
    }

    window.DateTimeRangePicker = {
        mount: mount,
        mountBlazor: function (root, callback) {
            return mount(root, { commitOnApply: true, onApply: range => callback.invokeMethodAsync("ApplyRangeAsync", range) });
        }
    };
})();
