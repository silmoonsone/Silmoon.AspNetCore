// Keep the panel in its Blazor-owned DOM tree; the native top layer escapes
// clipping and stacking contexts without moving nodes or losing scoped styles.
export function createPickerPopover(root, options) {
    const toggle = root.querySelector(options.toggleSelector);
    let panel = null;
    let closingPanel = null;
    let frame = 0;
    let disposed = false;
    const gap = 6;
    const margin = 8;

    function hide(element) {
        if (element?.matches(":popover-open")) element.hidePopover();
    }

    function close(restoreFocus = false) {
        if (!panel || closingPanel === panel) return;
        closingPanel = panel;
        hide(panel);
        root.querySelector(options.closeSelector)?.click();
        if (restoreFocus && !toggle.disabled && toggle.isConnected) toggle.focus();
    }

    function anchorIsVisible(anchor, leftEdge, topEdge, rightEdge, bottomEdge) {
        let left = Math.max(anchor.left, leftEdge);
        let top = Math.max(anchor.top, topEdge);
        let right = Math.min(anchor.right, rightEdge);
        let bottom = Math.min(anchor.bottom, bottomEdge);
        for (let ancestor = toggle.parentElement; ancestor; ancestor = ancestor.parentElement) {
            const style = window.getComputedStyle(ancestor);
            const clipsX = /^(hidden|clip|auto|scroll|overlay)$/.test(style.overflowX);
            const clipsY = /^(hidden|clip|auto|scroll|overlay)$/.test(style.overflowY);
            if (!clipsX && !clipsY) continue;
            const bounds = ancestor.getBoundingClientRect();
            if (clipsX) {
                left = Math.max(left, bounds.left + ancestor.clientLeft);
                right = Math.min(right, bounds.left + ancestor.clientLeft + ancestor.clientWidth);
            }
            if (clipsY) {
                top = Math.max(top, bounds.top + ancestor.clientTop);
                bottom = Math.min(bottom, bounds.top + ancestor.clientTop + ancestor.clientHeight);
            }
        }
        return right > left && bottom > top;
    }

    function position() {
        if (!panel?.isConnected || closingPanel === panel) return;
        const viewport = window.visualViewport;
        const leftEdge = (viewport?.offsetLeft ?? 0) + margin;
        const topEdge = (viewport?.offsetTop ?? 0) + margin;
        const rightEdge = (viewport?.offsetLeft ?? 0) + (viewport?.width ?? document.documentElement.clientWidth) - margin;
        const bottomEdge = (viewport?.offsetTop ?? 0) + (viewport?.height ?? window.innerHeight) - margin;
        const anchor = toggle.getBoundingClientRect();
        if (toggle.disabled || !toggle.getClientRects().length || !anchorIsVisible(anchor, leftEdge, topEdge, rightEdge, bottomEdge)) {
            close();
            return;
        }

        const width = Math.max(0, Math.min(anchor.width, rightEdge - leftEdge));
        panel.style.width = `${width}px`;
        const availableBelow = Math.max(0, bottomEdge - anchor.bottom - gap);
        const availableAbove = Math.max(0, anchor.top - topEdge - gap);
        const preferredHeight = Math.min(options.maxHeight, panel.scrollHeight + panel.offsetHeight - panel.clientHeight);
        const above = availableBelow < preferredHeight && availableAbove > availableBelow;
        const availableHeight = above ? availableAbove : availableBelow;
        panel.style.maxHeight = `${Math.max(0, Math.min(options.maxHeight, availableHeight))}px`;
        const height = panel.getBoundingClientRect().height;
        const desiredTop = above ? anchor.top - gap - height : anchor.bottom + gap;
        panel.style.left = `${Math.max(leftEdge, Math.min(anchor.left, rightEdge - width))}px`;
        panel.style.top = `${Math.max(topEdge, Math.min(desiredTop, bottomEdge - height))}px`;
    }

    function update() {
        if (disposed) return;
        if (!root.isConnected) {
            dispose();
            return;
        }
        const nextPanel = root.querySelector(options.panelSelector);
        if (panel !== nextPanel) {
            if (panel) {
                resizeObserver.unobserve(panel);
                panel.removeEventListener("toggle", nativeToggle);
            }
            hide(panel);
            panel = nextPanel;
            closingPanel = null;
            if (panel) {
                resizeObserver.observe(panel);
                panel.addEventListener("toggle", nativeToggle);
                panel.showPopover();
                position();
                if (closingPanel !== panel) options.onOpen?.();
            }
        }
        else position();
    }

    function schedule() {
        if (disposed || frame) return;
        frame = requestAnimationFrame(() => {
            frame = 0;
            update();
        });
    }

    function pointerDown(event) {
        if (panel && !root.contains(event.target)) close();
    }

    function nativeToggle(event) {
        if (event.target === panel && event.newState === "closed") close();
    }

    function keyDown(event) {
        if (event.key !== "Escape" || !panel || closingPanel === panel || !root.contains(event.target)) return;
        event.preventDefault();
        event.stopPropagation();
        close(true);
    }

    function dispose() {
        if (disposed) return;
        disposed = true;
        if (frame) cancelAnimationFrame(frame);
        frame = 0;
        panel?.removeEventListener("toggle", nativeToggle);
        hide(panel);
        panel = null;
        resizeObserver.disconnect();
        mutationObserver.disconnect();
        document.removeEventListener("pointerdown", pointerDown, true);
        root.removeEventListener("keydown", keyDown, true);
        window.removeEventListener("scroll", schedule, true);
        window.removeEventListener("resize", schedule);
        window.visualViewport?.removeEventListener("resize", schedule);
        window.visualViewport?.removeEventListener("scroll", schedule);
        options.onDispose?.();
    }

    const resizeObserver = new ResizeObserver(schedule);
    const mutationObserver = new MutationObserver(schedule);
    resizeObserver.observe(toggle);
    mutationObserver.observe(document.body, { childList: true, subtree: true });
    document.addEventListener("pointerdown", pointerDown, true);
    root.addEventListener("keydown", keyDown, true);
    window.addEventListener("scroll", schedule, true);
    window.addEventListener("resize", schedule);
    window.visualViewport?.addEventListener("resize", schedule);
    window.visualViewport?.addEventListener("scroll", schedule);
    update();
    return { update, dispose };
}
