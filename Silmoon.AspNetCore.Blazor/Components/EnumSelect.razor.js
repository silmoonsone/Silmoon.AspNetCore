import { createPickerPopover } from "../js/picker-popover.js?v=1";

const handlers = new WeakMap();

function getToggle(select) {
    return select.querySelector(".enum-select-toggle");
}

function getOptions(select) {
    return Array.from(select.querySelectorAll("[data-enum-select-option]"));
}

function focusOption(select, index) {
    const options = getOptions(select);
    if (!options.length) return;
    options[Math.max(0, Math.min(index, options.length - 1))].focus();
}

function focusOpenedOption(select, direction) {
    const options = getOptions(select);
    if (!options.length) return;
    const selectedIndex = options.findIndex(option => option.getAttribute("aria-selected") === "true");
    focusOption(select, direction > 0 ? Math.max(0, selectedIndex) : selectedIndex >= 0 ? selectedIndex : options.length - 1);
}

export function initialize(select) {
    if (!(select instanceof Element) || handlers.has(select)) return;

    let pendingDirection = 0;
    const keyDownHandler = function (event) {
        const toggle = getToggle(select);
        const option = event.target instanceof Element ? event.target.closest("[data-enum-select-option]") : null;
        if (event.target === toggle && (event.key === "ArrowDown" || event.key === "ArrowUp")) {
            if (toggle.disabled) return;
            event.preventDefault();
            const direction = event.key === "ArrowDown" ? 1 : -1;
            if (!select.querySelector(".enum-select-panel")) {
                pendingDirection = direction;
                toggle.click();
            }
            else focusOpenedOption(select, direction);
            return;
        }
        if (!option) return;
        const options = getOptions(select);
        const currentIndex = options.indexOf(option);
        if (event.key === "ArrowDown" || event.key === "ArrowUp") {
            event.preventDefault();
            focusOption(select, currentIndex + (event.key === "ArrowDown" ? 1 : -1));
        }
        else if (event.key === "Home" || event.key === "End") {
            event.preventDefault();
            focusOption(select, event.key === "Home" ? 0 : options.length - 1);
        }
    };

    select.addEventListener("keydown", keyDownHandler);
    const popover = createPickerPopover(select, {
        toggleSelector: ".enum-select-toggle",
        panelSelector: ".enum-select-panel",
        closeSelector: "[data-enum-select-close]",
        maxHeight: 320,
        onOpen() {
            if (pendingDirection) focusOpenedOption(select, pendingDirection);
            pendingDirection = 0;
        },
        onDispose() {
            select.removeEventListener("keydown", keyDownHandler);
            handlers.delete(select);
        }
    });
    handlers.set(select, popover);
}

export function dispose(select) {
    if (!(select instanceof Element)) return;
    const value = handlers.get(select);
    if (!value) return;
    value.dispose();
}
