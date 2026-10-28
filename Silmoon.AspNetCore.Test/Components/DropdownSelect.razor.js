(function () {
    if (window.silmoonDropdownSelectLoaded) return;
    window.silmoonDropdownSelectLoaded = true;
    const selector = "details[data-dropdown-select]";
    const states = new WeakMap();
    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
    const canKeepClosedContent = window.CSS?.supports?.("selector(::details-content)") === true;

    function getState(root) {
        if (!states.has(root)) states.set(root, { phase: "closed", frame: null, positionKey: null, animation: null });
        return states.get(root);
    }

    function cancelAnimation(root) {
        const state = states.get(root);
        const animation = state?.animation;
        if (state) state.animation = null;
        animation?.cancel();
    }

    function clearSurface(root) {
        const state = states.get(root);
        if (state?.frame != null) cancelAnimationFrame(state.frame);
        if (state) { state.phase = "closed"; state.frame = null; state.positionKey = null; }
        cancelAnimation(root);
        const menu = root.querySelector(".dropdown-select-options");
        if (typeof menu?.hidePopover === "function" && menu.matches(":popover-open")) menu.hidePopover();
        root.classList.remove("is-closing");
        if (menu) menu.inert = false;
    }

    function animateSurface(root, opening) {
        const menu = root.querySelector(".dropdown-select-options");
        const state = getState(root);
        const opacity = state.animation ? Number.parseFloat(getComputedStyle(menu).opacity) : opening ? 0 : 1;
        cancelAnimation(root);
        root.classList.toggle("is-closing", !opening);
        menu.inert = !opening;
        if (reducedMotion.matches || typeof menu.animate !== "function" || (!opening && !canKeepClosedContent)) {
            if (!opening) clearSurface(root);
            return;
        }
        const animation = menu.animate([{ opacity }, { opacity: opening ? 1 : 0 }], {
            duration: opening ? 180 : 140, easing: opening ? "ease-out" : "ease-in", fill: "both"
        });
        state.animation = animation;
        const finish = () => {
            if (state.animation !== animation) return;
            state.animation = null;
            animation.cancel();
            if (!opening) clearSurface(root);
        };
        animation.finished.then(finish, finish);
    }

    function closeMenu(root, immediate = false) {
        const state = getState(root);
        if (state.phase === "open" && !immediate) root.classList.add("is-closing");
        root.open = false;
        if (immediate || !root.isConnected || root.dataset.disabled === "true" || state.phase === "closed") {
            clearSurface(root);
            return;
        }
        if (state.phase === "closing") return;
        state.phase = "closing";
        animateSurface(root, false);
        if (state.phase === "closing") trackPosition(root);
    }

    function closeOthers(current) {
        document.querySelectorAll(`${selector}[open]`).forEach(item => {
            if (item !== current) closeMenu(item);
        });
    }

    function viewportBounds() {
        const viewport = window.visualViewport;
        const left = viewport?.offsetLeft || 0;
        const top = viewport?.offsetTop || 0;
        const width = viewport?.width || document.documentElement.clientWidth;
        const height = viewport?.height || document.documentElement.clientHeight;
        return { left, top, width, height, right: left + width, bottom: top + height };
    }

    function trackPosition(root) {
        const state = getState(root);
        if (state.frame != null) return;
        const menu = root.querySelector(".dropdown-select-options");
        function update() {
            state.frame = null;
            if (!root.isConnected || root.dataset.disabled === "true") {
                closeMenu(root, true);
                return;
            }
            if (state.phase === "closed") return;
            if (state.phase === "open") {
                if (!root.open) { closeMenu(root); return; }
                const bounds = root.getBoundingClientRect();
                const viewport = viewportBounds();
                if (bounds.bottom <= viewport.top || bounds.top >= viewport.bottom || bounds.right <= viewport.left || bounds.left >= viewport.right) {
                    closeMenu(root);
                    return;
                }
                const position = [bounds.left, bounds.top, bounds.width, bounds.height, menu.offsetWidth, menu.offsetHeight, menu.scrollHeight, viewport.left, viewport.top, viewport.width, viewport.height, root.dataset.alignEnd, root.dataset.openUpward, getComputedStyle(root).direction].join(",");
                if (position !== state.positionKey) { placeMenu(root); state.positionKey = position; }
            }
            state.frame = requestAnimationFrame(update);
        }
        state.frame = requestAnimationFrame(update);
    }

    function openMenu(root) {
        if (!root.isConnected || !root.open) return;
        if (root.dataset.disabled === "true") { closeMenu(root, true); return; }
        closeOthers(root);
        const state = getState(root);
        const opening = state.phase !== "open";
        state.phase = "open";
        placeMenu(root);
        if (opening) animateSurface(root, true);
        trackPosition(root);
    }

    function revealFocusedOption(menu) {
        const option = document.activeElement;
        if (!option || !menu.contains(option)) return;
        const bounds = menu.getBoundingClientRect();
        const top = bounds.top + menu.clientTop;
        const bottom = top + menu.clientHeight;
        const target = option.getBoundingClientRect();
        if (target.top < top) menu.scrollTop -= top - target.top;
        else if (target.bottom > bottom) menu.scrollTop += target.bottom - bottom;
    }

    function placeMenu(root) {
        if (!root.open) return;
        const menu = root.querySelector(".dropdown-select-options");
        if (!menu) return;
        const usePopover = typeof menu.showPopover === "function";
        const viewport = viewportBounds();
        const bounds = root.getBoundingClientRect();
        const padding = 8;
        const gap = 8;
        menu.style.position = usePopover ? "fixed" : "absolute";
        menu.style.inset = "auto";
        menu.style.margin = "0";
        menu.style.translate = "none";
        const maxWidth = Math.max(0, viewport.width - padding * 2) + "px";
        menu.style.minWidth = `min(10rem, ${maxWidth})`;
        menu.style.maxWidth = maxWidth;
        menu.style.maxHeight = `min(18rem, ${Math.max(0, viewport.height - padding * 2)}px)`;
        if (usePopover && !menu.matches(":popover-open")) menu.showPopover();
        const above = Math.max(0, bounds.top - viewport.top - padding - gap);
        const below = Math.max(0, viewport.bottom - bounds.bottom - padding - gap);
        const height = menu.getBoundingClientRect().height;
        let upward = root.dataset.openUpward === "true";
        if (height > (upward ? above : below) && (upward ? below > above : above > below)) upward = !upward;
        root.dataset.placement = upward ? "above" : "below";
        menu.style.maxHeight = `min(18rem, ${upward ? above : below}px)`;
        const size = menu.getBoundingClientRect();
        const rtl = getComputedStyle(root).direction === "rtl";
        const alignEnd = root.dataset.alignEnd === "true";
        const anchorLeft = rtl !== alignEnd ? bounds.right - size.width : bounds.left;
        const left = Math.max(viewport.left + padding, Math.min(anchorLeft, viewport.right - size.width - padding));
        const top = Math.max(viewport.top + padding, Math.min(upward ? bounds.top - gap - size.height : bounds.bottom + gap, viewport.bottom - size.height - padding));
        const origin = usePopover ? { left: 0, top: 0 } : bounds;
        menu.style.left = (left - origin.left) + "px";
        menu.style.top = (top - origin.top) + "px";
        revealFocusedOption(menu);
    }

    document.addEventListener("toggle", event => {
        const root = event.target;
        if (root.matches?.(selector)) {
            if (root.open) openMenu(root);
            else closeMenu(root);
        } else if (root.matches?.(".dropdown-select-options") && !root.matches(":popover-open")) {
            const owner = root.closest(selector);
            if (owner) closeMenu(owner, true);
        }
    }, true);

    document.addEventListener("pointerdown", event => closeOthers(event.target.closest?.(selector)), true);
    document.addEventListener("focusin", event => closeOthers(event.target.closest?.(selector)));
    document.addEventListener("click", event => {
        const root = event.target.closest?.(selector);
        if (!root) return;
        if (root.dataset.disabled === "true") { closeMenu(root, true); event.preventDefault(); return; }
        const option = event.target.closest("[data-dropdown-option]");
        if (!option || option.disabled) return;
        closeMenu(root);
        if (option.tagName === "BUTTON") root.querySelector("summary").focus();
    });

    document.addEventListener("keydown", event => {
        const root = event.target.closest?.(selector);
        if (!root || root.dataset.disabled === "true") return;
        const trigger = root.querySelector("summary");
        if (event.key === "Escape" && root.open) {
            event.preventDefault();
            event.stopPropagation();
            closeMenu(root);
            trigger.focus();
            return;
        }
        if (!["ArrowDown", "ArrowUp", "Home", "End"].includes(event.key) || (!root.open && event.target !== trigger)) return;
        const options = [...root.querySelectorAll("[data-dropdown-option]:not(:disabled)")];
        if (!options.length) return;
        event.preventDefault();
        root.open = true;
        openMenu(root);
        const index = options.indexOf(document.activeElement);
        const next = event.key === "Home" ? 0 : event.key === "End" ? options.length - 1 : index < 0 ? (event.key === "ArrowUp" ? options.length - 1 : 0) : (index + (event.key === "ArrowUp" ? -1 : 1) + options.length) % options.length;
        options[next].focus({ preventScroll: true });
        revealFocusedOption(root.querySelector(".dropdown-select-options"));
    });

    const repositionOpen = () => document.querySelectorAll(`${selector}[open]`).forEach(placeMenu);
    window.addEventListener("resize", repositionOpen);
    window.visualViewport?.addEventListener("resize", repositionOpen);
    window.visualViewport?.addEventListener("scroll", repositionOpen);
    document.addEventListener("scroll", event => {
        if (!event.target.closest?.(".dropdown-select-options")) closeOthers(null);
    }, true);

    // A user may open native details before this lazy module finishes loading.
    document.querySelectorAll(`${selector}[open]`).forEach(openMenu);
})();
