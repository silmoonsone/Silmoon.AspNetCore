// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// jsclickable_start
function __jsclickable() {
    console.log("__jsclickable");
    $(".jsclickable").on("mousedown touchstart", (e) => {
        e.currentTarget.classList.add("jsclickable-active")
        e.currentTarget.classList.remove("jsclickable-hover")
    })
    $(".jsclickable").on("mouseup mouseover", (e) => {
        e.currentTarget.classList.add("jsclickable-hover")
        e.currentTarget.classList.remove("jsclickable-active")
    })
    $(".jsclickable").on("mouseout", (e) => {
        e.currentTarget.classList.remove("jsclickable-hover")
        e.currentTarget.classList.remove("jsclickable-hover")
    })
    $(".jsclickable").on("touchend", (e) => {
        for (var i = 1; i < 6; i++) {
            setTimeout(() => {
                e.currentTarget.classList.remove("jsclickable-hover")
                e.currentTarget.classList.remove("jsclickable-hover")
            }, 100 * i);
        }
    })

    $(".jsclickable-outline").on("mousedown touchstart", (e) => {
        e.currentTarget.classList.add("jsclickable-outline-active")
        e.currentTarget.classList.remove("jsclickable-outline-hover")
    })
    $(".jsclickable-outline").on("mouseup mouseover", (e) => {
        e.currentTarget.classList.add("jsclickable-outline-hover")
        e.currentTarget.classList.remove("jsclickable-outline-active")
    })
    $(".jsclickable-outline").on("mouseout", (e) => {
        e.currentTarget.classList.remove("jsclickable-outline-hover")
        e.currentTarget.classList.remove("jsclickable-outline-active")
    })
    $(".jsclickable-outline").on("touchend", (e) => {
        for (var i = 1; i < 6; i++) {
            setTimeout(() => {
                e.currentTarget.classList.remove("jsclickable-outline-hover")
                e.currentTarget.classList.remove("jsclickable-outline-active")
            }, 100 * i);
        }
    })
}
window.onload = function () {
    __jsclickable();
    new MutationObserver((mutations, observer) => __jsclickable()).observe(document.body, { childList: true, subtree: true });
}

// jsclickable_end
(function () {
    // Custom elements also connect after static SSR enhanced navigation.
    // Components own their module URLs; import() caches modules across instances.
    if (!customElements.get("silmoon-module")) {
        customElements.define("silmoon-module", class extends HTMLElement {
            static observedAttributes = ["src"];

            connectedCallback() { this.load(); }
            attributeChangedCallback(name, previous, current) {
                if (previous !== current && this.isConnected) this.load();
            }

            load() {
                const src = this.getAttribute("src");
                if (src) import(src).catch(error => console.error("Component module load failed: " + src, error));
            }
        });
    }
})();
