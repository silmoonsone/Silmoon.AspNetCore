import { createPickerPopover } from "../js/picker-popover.js?v=1";

const handlers = new WeakMap();

export function initialize(picker) {
    if (!(picker instanceof Element) || handlers.has(picker)) return;
    const popover = createPickerPopover(picker, {
        toggleSelector: ".tree-picker-toggle",
        panelSelector: ".tree-picker-panel",
        closeSelector: "[data-tree-picker-close]",
        maxHeight: 520,
        onDispose() { handlers.delete(picker); }
    });
    handlers.set(picker, popover);
}

export function dispose(picker) {
    if (!(picker instanceof Element)) return;
    handlers.get(picker)?.dispose();
}
