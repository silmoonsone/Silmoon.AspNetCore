const instances = new WeakMap();
const interactive = "button, a, input, select, textarea, label, summary, [contenteditable], [role=button], [role=link], [data-grid-ignore]";
const isMissing = value => value == null || value === "" || (typeof value === "number" && !Number.isFinite(value));

export function mount(root, options = {}, blazor = null) {
    instances.get(root)?.dispose();
    const body = root.querySelector("[data-grid-body]");
    const scroll = root.querySelector("[data-grid-scroll]");
    const head = body.closest("table").querySelector("thead");
    let filter, empty, headers, single;
    const collator = new Intl.Collator(options.locale, { numeric: true });
    const events = new AbortController();
    const listen = { signal: events.signal };
    let records = [], selectedKey = null, sortKey = null, direction = 0, disabled = false;
    let entries = new Map(), rows = new WeakMap(), lastOrder = null;
    let query = "";

    function normalize(value) {
        return String(value).trim().toLocaleLowerCase(options.locale);
    }

    function compare(a, b) {
        if (sortKey !== null) {
            const x = a.sortValue, y = b.sortValue;
            if (isMissing(x) !== isMissing(y)) return isMissing(x) ? 1 : -1;
            if (!isMissing(x)) {
                const order = typeof x === "string" && typeof y === "string" ? collator.compare(x, y) : x < y ? -1 : x > y ? 1 : 0;
                if (order) return direction * order;
            }
        }
        return options.compareDefault?.(a.item, b.item) || a.index - b.index;
    }

    // A Blazor sort request may still be in flight. Navigation and events use the order on screen.
    function visibleRows() {
        return Array.from(body.children, row => rows.get(row)).filter(entry => entry && !entry.row.hidden);
    }

    function setSelection(key) {
        if (selectedKey === key) return;
        const first = visibleRows()[0];
        const previous = entries.get(selectedKey);
        if (previous) {
            previous.row.setAttribute("aria-selected", "false");
            previous.row.tabIndex = !disabled && previous === first && key === null ? 0 : -1;
        }
        selectedKey = key;
        if (single && first) {
            first.row.tabIndex = !disabled && key === null ? 0 : -1;
            const current = entries.get(key);
            if (current) {
                current.row.setAttribute("aria-selected", "true");
                current.row.tabIndex = disabled ? -1 : 0;
            }
        }
        options.onSelectionChange?.(key);
        if (blazor && root.dataset.gridSelectionCallback === "true") invoke("NotifyGridSelection", key);
    }

    function refreshView() {
        const column = sortKey === null ? -1 : headers.findIndex(header => header.key === sortKey);
        for (const entry of records) {
            if (column >= 0 && options.getValue) entry.sortValue = options.getValue(entry.item, sortKey);
            else if (column >= 0) {
                const cell = entry.row.children[column];
                const raw = cell?.dataset.gridValue ?? cell?.textContent?.trim();
                entry.sortValue = cell?.dataset.gridType === "number" && !isMissing(raw) ? Number(raw) : raw;
            }
            entry.row.hidden = Boolean(query && !(options.matchesFilter ? options.matchesFilter(entry.item, query) :
                Array.from(entry.row.children).some(cell => normalize(cell.textContent).includes(query))));
        }
        const ordered = [...records].sort(compare);
        const focused = document.activeElement;
        if (blazor) {
            const needsOrder = ordered.some((entry, index) => body.children[index] !== entry.row);
            const keys = ordered.map(entry => entry.key);
            if (!needsOrder) lastOrder = null;
            else if (!lastOrder || keys.some((key, index) => key !== lastOrder[index]) || keys.length !== lastOrder.length) {
                lastOrder = keys;
                invoke("ApplyGridOrder", keys);
            }
        } else {
            for (let index = 0; index < ordered.length; index++) {
                const row = ordered[index].row, at = body.children[index];
                if (at !== row) body.insertBefore(row, at ?? null);
            }
            if (focused && focused !== document.activeElement && body.contains(focused)) focused.focus({ preventScroll: true });
        }
        const visible = visibleRows();
        if (single) {
            if (selectedKey !== null && (!entries.has(selectedKey) || entries.get(selectedKey).row.hidden)) setSelection(null);
            for (const entry of records) {
                const selected = entry.key === selectedKey;
                entry.row.setAttribute("aria-selected", String(selected));
                entry.row.tabIndex = !disabled && (selected || (selectedKey === null && entry === visible[0])) ? 0 : -1;
            }
        }
        if (empty) empty.hidden = !query || visible.length !== 0;
    }

    function indexRecords(source) {
        records = source;
        entries = new Map(source.map(entry => [entry.key, entry]));
        rows = new WeakMap(source.map(entry => [entry.row, entry]));
    }

    function setItems(items) {
        const focused = document.activeElement;
        const focusedRow = focused?.closest?.("tr");
        const oldEntry = rows.get(focusedRow);
        const oldCell = oldEntry && focused.closest("td, th");
        const cellIndex = oldCell ? Array.prototype.indexOf.call(focusedRow.children, oldCell) : -1;
        const controlIndex = oldCell ? Array.from(oldCell.querySelectorAll(interactive)).indexOf(focused) : -1;
        const source = items.map((item, index) => {
            const key = options.getKey ? options.getKey(item, index) : item.id ?? index;
            const row = options.createRow(item, index);
            row.dataset.gridKey = String(key);
            return { key, item, index, row };
        });
        indexRecords(source);
        body.replaceChildren();
        refreshView();
        if (oldEntry) {
            const next = entries.get(oldEntry.key);
            if (next && !next.row.hidden) {
                const cell = next.row.children[cellIndex];
                const control = controlIndex >= 0 ? cell?.querySelectorAll(interactive)[controlIndex] : null;
                if (control && !control.disabled) control.focus({ preventScroll: true });
                else if (single || focusedRow === focused) next.row.focus({ preventScroll: true });
            }
        }
    }

    function refreshRows() {
        filter = Array.from(root.children).find(child => child.hasAttribute("data-grid-filter"));
        empty = Array.from(scroll.children).find(child => child.hasAttribute("data-grid-empty-filter"));
        query = normalize(filter?.value ?? "");
        headers = Array.from(head.firstElementChild.children, th => {
            const button = th.querySelector("[data-grid-sort]");
            return { th, button, key: th.dataset.gridColumn ?? button?.dataset.gridSort ?? null };
        });
        if (sortKey !== null && !headers.some(header => header.button?.dataset.gridSort === sortKey)) {
            sortKey = null;
            direction = 0;
        }
        const wasSingle = single;
        single = root.dataset.singleSelect === "true";
        indexRecords(Array.from(body.children, (row, index) => !blazor && rows.has(row) ? rows.get(row) : ({
            key: row.dataset.gridKey ?? String(index), item: row, row, index: Number(row.dataset.gridIndex ?? index)
        })));
        if (wasSingle && !single) {
            setSelection(null);
            for (const { row } of records) {
                row.removeAttribute("aria-selected");
                row.removeAttribute("tabindex");
            }
        }
        updateHeaders();
        refreshView();
    }

    function setFilter(value) {
        const text = String(value ?? "");
        if (filter && filter.value !== text) filter.value = text;
        const next = normalize(text);
        if (query === next) return;
        query = next;
        refreshView();
        options.onFilterChange?.(query);
    }

    function updateHeaders() {
        for (const { th, button, key } of headers) {
            if (!button) continue;
            const dir = key !== sortKey ? "none" : direction > 0 ? "ascending" : "descending";
            th.setAttribute("aria-sort", dir);
            button.dataset.sortDirection = dir;
            if (options.sortHint) button.title = options.sortHint(dir, key);
        }
    }

    function invoke(method, ...args) {
        blazor.invokeMethodAsync(method, ...args).catch(error => {
            if (!events.signal.aborted && root.isConnected) console.error(error);
        });
    }

    function rowEvent(event, entry, doubleClick) {
        const rowIndex = visibleRows().indexOf(entry);
        if (rowIndex < 0) return;
        const cell = event.target.closest("td, th") ?? entry.row.children[0];
        const columnIndex = cell ? Array.prototype.indexOf.call(entry.row.children, cell) : -1;
        const column = headers[columnIndex]?.key ?? null;
        const detail = { key: entry.key, item: entry.item, column, rowIndex, columnIndex, row: entry.row, cell, originalEvent: event };
        if (!entry.row.dispatchEvent(new CustomEvent(doubleClick ? "data-grid-rowdblclick" : "data-grid-rowclick", { bubbles: true, cancelable: true, detail }))) return;
        if (single) setSelection(entry.key);
        if (blazor) invoke("NotifyGridRow", doubleClick, entry.key, column, rowIndex, columnIndex);
        else (doubleClick ? options.onRowDoubleClick : options.onRowClick)?.(detail);
    }

    function handle(event) {
        if (disabled || event.defaultPrevented || event.target.closest("[data-grid]") !== root) return;
        if (event.target === filter && event.type === "keydown" && event.key === "Enter") event.preventDefault();
        const button = event.target.closest("[data-grid-sort]");
        if (event.type === "click" && button) {
            const key = button.dataset.gridSort;
            direction = key !== sortKey ? 1 : direction === 1 ? -1 : 0;
            sortKey = direction ? key : null;
            updateHeaders();
            refreshView();
            return;
        }
        if (event.target.closest(interactive)) return;
        const entry = rows.get(event.target.closest("tr"));
        if (!entry) return;
        if (event.type !== "keydown") { rowEvent(event, entry, event.type === "dblclick"); return; }
        if (!single || event.target !== entry.row) return;
        const visible = visibleRows();
        const index = visible.indexOf(entry);
        if (index < 0) return;
        const next = { ArrowDown: Math.min(index + 1, visible.length - 1), ArrowUp: Math.max(index - 1, 0), Home: 0, End: visible.length - 1 }[event.key];
        if (next !== undefined) {
            event.preventDefault();
            setSelection(visible[next].key);
            visible[next].row.focus();
        } else if (event.key === " " || event.key === "Enter") {
            event.preventDefault();
            rowEvent(event, entry, false);
        }
    }

    for (const type of ["click", "dblclick", "keydown"]) root.addEventListener(type, handle, listen);
    root.addEventListener("input", event => { if (event.target === filter && !disabled) setFilter(filter.value); }, listen);
    const api = {
        setItems, refreshView, refreshRows, setFilter,
        get sortKey() { return sortKey; },
        get selectedKey() { return selectedKey; },
        get visibleItems() { return visibleRows().map(entry => entry.item); },
        select(key) { if (single) setSelection(key != null && entries.has(key) && !entries.get(key).row.hidden ? key : null); },
        setDisabled(value) {
            value = Boolean(value);
            if (value === disabled) return;
            disabled = value;
            root.setAttribute("aria-disabled", String(value));
            if (filter) filter.disabled = value;
            for (const { button } of headers) if (button) button.disabled = value;
            if (single) {
                const focusable = entries.get(selectedKey) ?? visibleRows()[0];
                if (focusable) focusable.row.tabIndex = disabled ? -1 : 0;
            }
        },
        dispose() {
            if (events.signal.aborted) return;
            events.abort();
            options.signal?.removeEventListener("abort", api.dispose);
            instances.delete(root);
        }
    };
    instances.set(root, api);
    refreshRows();
    if (options.signal?.aborted) api.dispose();
    else options.signal?.addEventListener("abort", api.dispose, { once: true });
    return api;
}

export function mountBlazor(root, receiver) {
    return root?.isConnected ? mount(root, {}, receiver) : null;
}
