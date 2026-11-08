// Blazor imports library initializers at startup. Static MVC pages can import the same module.
const componentDirectory = new URL("../Components/", import.meta.url);

if (!customElements.get("silmoon-blazor-module")) {
    customElements.define("silmoon-blazor-module", class extends HTMLElement {
        static observedAttributes = ["src"];

        connectedCallback() { this.load(); }
        attributeChangedCallback(name, previous, current) {
            if (previous !== current && this.isConnected) this.load();
        }

        load() {
            const source = this.getAttribute("src");
            if (!source) return;
            const url = new URL(source, document.baseURI);
            if (url.origin === componentDirectory.origin && url.pathname.startsWith(componentDirectory.pathname) && url.pathname.endsWith(".razor.js"))
                import(url.href).catch(error => console.error("Component module load failed:", url.href, error));
        }
    });
}
