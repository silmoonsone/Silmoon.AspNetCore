(function () {
    if (window.silmoonDropdownMenuLoaded) return;
    window.silmoonDropdownMenuLoaded = true;
    const selector = "[data-dropdown-menu]";
    const states = new WeakMap();
    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");

    customElements.define("silmoon-dropdown-menu", class extends HTMLElement {
        disconnectedCallback() {
            const instance = window.bootstrap?.Dropdown.getInstance(this.querySelector("[data-bs-toggle='dropdown']"));
            instance?.hide();
            instance?.dispose();
            clearSurface(this);
            reset(this);
            this.querySelector(".dropdown-menu-popup")?.classList.remove("show");
            const toggle = this.querySelector("[data-bs-toggle='dropdown']");
            toggle?.classList.remove("show");
            toggle?.setAttribute("aria-expanded", "false");
            states.delete(this);
        }
    });

    function getState(root) {
        if (!states.has(root)) states.set(root, { panel: root.querySelector("[data-dropdown-menu-panel='root']"), finish: null, frame: null, positionKey: null, surfaceAnimation: null });
        return states.get(root);
    }

    function viewportBounds() {
        const viewport = window.visualViewport;
        const left = viewport?.offsetLeft || 0;
        const top = viewport?.offsetTop || 0;
        const width = viewport?.width || document.documentElement.clientWidth;
        const height = viewport?.height || document.documentElement.clientHeight;
        return { left, top, width, height, right: left + width, bottom: top + height };
    }

    function updatePosition(root) {
        const menu = root.querySelector(".dropdown-menu-popup");
        const toggle = root.querySelector("[data-bs-toggle='dropdown']");
        if (!root.isConnected || !menu?.classList.contains("show") || !toggle) return;
        const viewport = viewportBounds();
        const bounds = toggle.getBoundingClientRect();
        const padding = 8;
        const gap = 8;
        const usePopover = typeof menu.showPopover === "function";
        // Popper is configured with display:static. Top-layer coordinates use the viewport,
        // not the transformed/backdrop-filtered ancestors retained in the Blazor DOM.
        menu.style.position = usePopover ? "fixed" : "absolute";
        menu.style.inset = "auto";
        menu.style.margin = "0";
        menu.style.transform = "none";
        menu.style.translate = "none";
        menu.style.maxWidth = Math.max(0, viewport.width - padding * 2) + "px";
        menu.style.maxHeight = Math.max(0, viewport.height - padding * 2) + "px";
        if (usePopover && !menu.matches(":popover-open")) menu.showPopover();
        const above = Math.max(0, bounds.top - viewport.top - padding - gap);
        const below = Math.max(0, viewport.bottom - bounds.bottom - padding - gap);
        const naturalHeight = menu.getBoundingClientRect().height;
        let upward = root.dataset.openUpward === "true";
        if (naturalHeight > (upward ? above : below) && (upward ? below > above : above > below)) upward = !upward;
        menu.style.maxHeight = (upward ? above : below) + "px";
        const size = menu.getBoundingClientRect();
        const rtl = getComputedStyle(root).direction === "rtl";
        const alignEnd = root.dataset.alignEnd === "true";
        const anchorLeft = rtl !== alignEnd ? bounds.right - size.width : bounds.left;
        const left = Math.max(viewport.left + padding, Math.min(anchorLeft, viewport.right - size.width - padding));
        const top = Math.max(viewport.top + padding, Math.min(upward ? bounds.top - gap - size.height : bounds.bottom + gap, viewport.bottom - size.height - padding));
        const origin = usePopover ? { left: 0, top: 0 } : root.getBoundingClientRect();
        menu.style.left = (left - origin.left) + "px";
        menu.style.top = (top - origin.top) + "px";
        root.dataset.placement = upward ? "above" : "below";
        revealFocusedItem(menu);
    }

    function revealFocusedItem(menu) {
        const item = document.activeElement;
        if (!item || !menu.contains(item)) return;
        const bounds = menu.getBoundingClientRect();
        const top = bounds.top + menu.clientTop;
        const bottom = top + menu.clientHeight;
        const target = item.getBoundingClientRect();
        if (target.top < top) menu.scrollTop -= top - target.top;
        else if (target.bottom > bottom) menu.scrollTop += target.bottom - bottom;
    }

    function stopTracking(root) {
        const state = states.get(root);
        if (state?.frame != null) cancelAnimationFrame(state.frame);
        if (state) { state.frame = null; state.positionKey = null; }
    }

    function cancelSurfaceAnimation(root) {
        const state = states.get(root);
        const animation = state?.surfaceAnimation;
        if (state) state.surfaceAnimation = null;
        animation?.cancel();
    }

    function clearSurface(root) {
        stopTracking(root);
        cancelSurfaceAnimation(root);
        const menu = root.querySelector(".dropdown-menu-popup");
        if (typeof menu?.hidePopover === "function" && menu.matches(":popover-open")) menu.hidePopover();
        menu?.classList.remove("is-closing");
        if (menu) menu.inert = false;
    }

    function animateSurface(root, opening) {
        const menu = root.querySelector(".dropdown-menu-popup");
        const state = getState(root);
        // Continue from the current opacity when a quick reopen interrupts a fade-out.
        const opacity = state.surfaceAnimation ? Number.parseFloat(getComputedStyle(menu).opacity) : opening ? 0 : 1;
        cancelSurfaceAnimation(root);
        menu.classList.toggle("is-closing", !opening);
        menu.inert = !opening;
        if (reducedMotion.matches || typeof menu.animate !== "function") {
            if (!opening) { clearSurface(root); reset(root); }
            return;
        }
        const animation = menu.animate([{ opacity }, { opacity: opening ? 1 : 0 }], {
            duration: opening ? 180 : 140, easing: opening ? "ease-out" : "ease-in", fill: "both"
        });
        state.surfaceAnimation = animation;
        const finish = () => {
            if (state.surfaceAnimation !== animation) return;
            state.surfaceAnimation = null;
            animation.cancel();
            if (!opening) { clearSurface(root); reset(root); }
        };
        animation.finished.then(finish, finish);
    }

    function hideSurface(root) {
        const menu = root.querySelector(".dropdown-menu-popup");
        const toggle = root.querySelector("[data-bs-toggle='dropdown']");
        stopTracking(root);
        getState(root).finish?.(false);
        if (!root.isConnected || toggle?.disabled || (typeof menu?.showPopover === "function" && !menu.matches(":popover-open"))) {
            clearSurface(root);
            reset(root);
            return;
        }
        // Bootstrap has already closed logically; only the non-interactive surface fades out.
        animateSurface(root, false);
    }

    function closeDisabled(root) {
        const toggle = root.querySelector("[data-bs-toggle='dropdown']");
        window.bootstrap?.Dropdown.getInstance(toggle)?.dispose();
        root.querySelector(".dropdown-menu-popup")?.classList.remove("show");
        toggle?.classList.remove("show");
        toggle?.setAttribute("aria-expanded", "false");
        clearSurface(root);
        reset(root);
    }

    function openSurface(root) {
        const menu = root.querySelector(".dropdown-menu-popup");
        const toggle = root.querySelector("[data-bs-toggle='dropdown']");
        if (!root.isConnected || !menu?.classList.contains("show")) return;
        if (toggle?.disabled) { closeDisabled(root); return; }
        if (menu.classList.contains("is-closing")) reset(root);
        updatePosition(root);
        const state = getState(root);
        if (state.frame != null) return;
        animateSurface(root, true);
        function track() {
            state.frame = null;
            if (!root.isConnected || !menu.classList.contains("show")) { clearSurface(root); return; }
            if (toggle.disabled) { closeDisabled(root); return; }
            const bounds = toggle.getBoundingClientRect();
            const viewport = viewportBounds();
            if (bounds.bottom <= viewport.top || bounds.top >= viewport.bottom || bounds.right <= viewport.left || bounds.left >= viewport.right) {
                window.bootstrap?.Dropdown.getInstance(toggle)?.hide();
                if (menu.classList.contains("show")) state.frame = requestAnimationFrame(track);
                return;
            }
            const key = [bounds.left, bounds.top, bounds.width, bounds.height, menu.offsetWidth, menu.offsetHeight, menu.scrollHeight, viewport.left, viewport.top, viewport.width, viewport.height, root.dataset.alignEnd, root.dataset.openUpward, root.dataset.menuWidth, getComputedStyle(root).direction].join(",");
            if (key !== state.positionKey) { updatePosition(root); state.positionKey = key; }
            state.frame = requestAnimationFrame(track);
        }
        state.frame = requestAnimationFrame(track);
    }

    function clearHighlight(root) {
        root.querySelectorAll(".is-navigating").forEach(item => item.classList.remove("is-navigating"));
        root.querySelectorAll(".is-returning").forEach(item => item.classList.remove("is-returning"));
    }

    function reset(root) {
        const state = getState(root);
        state.finish?.(false);
        clearHighlight(root);
        root.querySelectorAll("[data-dropdown-menu-panel]").forEach(panel => {
            panel.hidden = panel.dataset.dropdownMenuPanel !== "root";
            panel.inert = false;
        });
        state.panel = root.querySelector("[data-dropdown-menu-panel='root']");
        const menu = root.querySelector(".dropdown-menu-popup");
        if (menu) menu.scrollTop = 0;
    }

    function showPanel(root, name, returning = false) {
        const state = getState(root);
        const next = root.querySelector(`[data-dropdown-menu-panel='${name}']`);
        if (!next || next === state.panel) return;
        state.finish?.(false);
        clearHighlight(root);
        const previous = state.panel;
        const opener = returning ? next.querySelector(`[data-dropdown-menu-open='${previous.dataset.dropdownMenuPanel}']`) : previous.querySelector(`[data-dropdown-menu-open='${name}']`);
        const viewport = root.querySelector(".dropdown-menu-viewport");
        const oldHeight = previous.getBoundingClientRect().height;
        const animate = !reducedMotion.matches && typeof next.animate === "function";
        if (opener) opener.classList.add("is-navigating");
        next.hidden = false;
        const menu = root.querySelector(".dropdown-menu-popup");
        if (menu) menu.scrollTop = 0;
        state.panel = next;
        previous.inert = true;
        const newHeight = next.getBoundingClientRect().height;
        (returning ? opener : next.querySelector(".dropdown-item:not(:disabled)"))?.focus({ preventScroll: true });
        const animations = [];
        const finish = completed => {
            if (state.finish !== finish) return;
            state.finish = null;
            previous.hidden = true;
            previous.inert = false;
            animations.forEach(animation => animation.cancel());
            opener?.classList.remove("is-navigating");
            if (completed && returning && animate && opener) opener.classList.add("is-returning");
            updatePosition(root);
        };
        state.finish = finish;
        if (!animate) {
            finish(true);
            return;
        }
        const direction = returning ? -1 : 1;
        const options = { duration: 240, easing: "cubic-bezier(0.22, 0.61, 0.36, 1)", fill: "both" };
        animations.push(previous.animate([{ transform: "translateX(0)" }, { transform: `translateX(${-direction * 100}%)` }], options));
        animations.push(next.animate([{ transform: `translateX(${direction * 100}%)` }, { transform: "translateX(0)" }], options));
        animations.push(viewport.animate([{ height: `${oldHeight}px` }, { height: `${newHeight}px` }], options));
        updatePosition(root);
        Promise.all(animations.map(animation => animation.finished)).then(() => finish(true), () => finish(false));
    }

    function goBack(root) {
        const parent = getState(root).panel?.querySelector("[data-dropdown-menu-back]")?.dataset.dropdownMenuBack;
        if (parent) showPanel(root, parent, true);
    }

    function close(root) {
        const toggle = root.querySelector("[data-bs-toggle='dropdown']");
        window.bootstrap?.Dropdown.getInstance(toggle)?.hide();
        toggle.focus();
    }

    document.addEventListener("click", event => {
        const root = event.target.closest?.(selector);
        if (!root) return;
        const option = event.target.closest(".dropdown-item");
        if (!option || option.disabled || !getState(root).panel?.contains(option)) return;
        if (option.hasAttribute("data-dropdown-menu-open")) showPanel(root, option.dataset.dropdownMenuOpen);
        else if (option.hasAttribute("data-dropdown-menu-back")) goBack(root);
        else if (option.hasAttribute("data-dropdown-menu-item")) close(root);
    });

    document.addEventListener("shown.bs.dropdown", event => {
        const root = event.target.closest?.(selector);
        if (root) openSurface(root);
    });

    document.addEventListener("hidden.bs.dropdown", event => {
        const root = event.target.closest?.(selector);
        if (root) hideSurface(root);
    });

    document.addEventListener("toggle", event => {
        const menu = event.target;
        if (!menu.matches?.(".dropdown-menu-popup") || !menu.classList.contains("show") || menu.matches(":popover-open")) return;
        const root = menu.closest(selector);
        if (root?.isConnected) {
            window.bootstrap?.Dropdown.getInstance(root.querySelector("[data-bs-toggle='dropdown']"))?.hide();
            if (menu.classList.contains("show")) openSurface(root);
        }
    }, true);

    document.addEventListener("scroll", event => {
        if (event.target.closest?.(".dropdown-menu-popup")) return;
        document.querySelectorAll(selector).forEach(root => window.bootstrap?.Dropdown.getInstance(root.querySelector("[data-bs-toggle='dropdown']"))?.hide());
    }, true);

    const repositionOpen = () => document.querySelectorAll(selector).forEach(root => updatePosition(root));
    window.addEventListener("resize", repositionOpen);
    window.visualViewport?.addEventListener("resize", repositionOpen);
    window.visualViewport?.addEventListener("scroll", repositionOpen);

    document.addEventListener("animationend", event => {
        if (event.pseudoElement === "::before" && event.target.closest?.(selector)) event.target.classList.remove("is-returning");
    });

    // Capture before Bootstrap: animated outgoing panels must not enter keyboard navigation.
    window.addEventListener("keydown", event => {
        const root = event.target.closest?.(selector);
        if (!root?.querySelector(".dropdown-menu.show")) return;
        const panel = getState(root).panel;
        const opener = event.target.closest("[data-dropdown-menu-open]");
        if (event.key === "Escape") close(root);
        else if (event.key === "ArrowLeft" && panel?.dataset.dropdownMenuPanel !== "root") goBack(root);
        else if (event.key === "ArrowRight" && opener && !opener.disabled && panel.contains(opener)) showPanel(root, opener.dataset.dropdownMenuOpen);
        else if (["ArrowUp", "ArrowDown", "Home", "End"].includes(event.key)) {
            const items = Array.from(panel.querySelectorAll(".dropdown-item:not(:disabled):not(.disabled)"));
            const index = items.indexOf(document.activeElement);
            const nextIndex = event.key === "Home" ? 0 : event.key === "End" ? items.length - 1 : index < 0 ? (event.key === "ArrowUp" ? items.length - 1 : 0) : Math.max(0, Math.min(items.length - 1, index + (event.key === "ArrowUp" ? -1 : 1)));
            items[nextIndex]?.focus({ preventScroll: true });
            revealFocusedItem(root.querySelector(".dropdown-menu-popup"));
        }
        else return;
        event.preventDefault();
        event.stopPropagation();
    }, true);

    // Bootstrap may have opened a menu before this lazy module finished loading.
    document.querySelectorAll(selector).forEach(openSurface);
})();
