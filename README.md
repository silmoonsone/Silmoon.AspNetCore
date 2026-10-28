[![.NET](https://github.com/silmoonsone/Silmoon.AspNetCore/actions/workflows/dotnet.yml/badge.svg)](https://github.com/silmoonsone/Silmoon.AspNetCore/actions/workflows/dotnet.yml)

# Silmoon.AspNetCore

AspNetCore related open source projects forked from Silmoon (44876f64eeb23c16bb86aa133f7ddc5ffdb95edb).

# Requirement:

* .net10 SDK

---

Many JS calls return objects that are generally the following (data is optional).

```json
{ "success": true, "message": null, "data": { "user": { "id": 1, "name": "Silmoon Sone" } } }
```

or

```json
{ "success": false, "message": "error message", "data": null }
```

---

[User session or user authentication](README/UserAuthentication.md)

[Passkey authentication](README/Passkey.md)

`Silmoon.AspNetCore.Test` includes local `DropdownSelect` and `DropdownMenu` components; see the [template component guide](https://github.com/silmoonsone/Silmoon.Templates/blob/master/Silmoon.Templates/content/Silmoon.AspNetCore.FullFunctionTemplate/_markdown/blazor-components-guide.md). Each component declares its JavaScript module for loading when used; the host must keep `site.js`, the application scoped CSS bundle, and Bootstrap Icons. `DropdownMenu` also requires the Bootstrap 5 bundle (including Popper).

`DropdownMenu` opens below the trigger from its start edge by default. Use `AlignEnd` for end alignment and `OpenUpward` to prefer opening above; available viewport space can change placement. Browsers with native Popover keep the menu in the top layer without moving its Blazor DOM, avoiding card clipping; the ordinary-position fallback cannot guarantee this.

`MenuWidth` now defaults to `12.5rem` (200px with a 16px root font), replacing the earlier `15rem`. Pass `"200px"` for a fixed pixel width or `"auto"` to fit the current menu level; viewport limits still apply. Rem widths follow root-font changes, including mobile themes. Opening fades in over 180ms and closing fades out over 140ms; reduced-motion preferences skip these animations. The component module is v4; refresh the scoped CSS cache together with it. The shared `site.js` is unchanged and needs no additional version for this component update.

`DropdownSelect` now uses module v3 and adds `AlignEnd` (false by default). It shares the four placement combinations, RTL behavior, viewport avoidance, and 180ms entrance / 140ms exit fades with `DropdownMenu`; callbacks and native links do not wait for animation. Browsers without `::details-content` close immediately.

The template's former `AuthFormCard` is now the general-purpose `ContentCard`, supplied as a Razor/CSS pair with no JavaScript. Title, brand, description, and icon are optional, with no default user icon. It supports body content, `FooterContent`, `Class`, and additional HTML attributes. `Width` / `MaxWidth` default to `100%`, `Centered` to false, and `HeadingLevel` to 2 (clamped to 1–6). Preserve the old authentication appearance with `Centered="true" MaxWidth="480px" HeadingLevel="1" Icon="bi-person-circle"`. Form styles only apply through explicit classes such as `card-form`, `card-fields`, and `card-actions`. See the template guide linked above for complete usage and migration; refresh the scoped CSS cache when updating this card or Select.

The template's [component demo source](https://github.com/silmoonsone/Silmoon.Templates/blob/master/Silmoon.Templates/content/Silmoon.AspNetCore.FullFunctionTemplate/RazorPages/Backend/ComponentsDemo.razor) serves its `/backend/components-demo` route; this project does not add a separate copy of that page. The main area shows individual controls and a simple `ContentCard`; centered profile forms, multiple instances, and asynchronous combinations are grouped under the initially collapsed “更多交互示例” section. Nested cards use `HeadingLevel="3"`, and select/menu parameter controls occupy separate rows.

For in-page navigation in Blazor pages with `<base>`, use an application-base-relative page route plus fragment, such as `Href="backend/components-demo#input-heading"`, substituting the actual target route when reusing the example. A bare `#input-heading` may resolve against `<base>` to the application home page instead of the current demo.
