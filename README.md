# Silmoon.AspNetCore

AspNetCore related open source projects forked from Silmoon (44876f64eeb23c16bb86aa133f7ddc5ffdb95edb).

# Requirement:

* .net8.0 and .net9.0 SDK
* NuGet package Silmoon
* NuGet package Silmoon.Extension

---
Many JS calls return objects that are generally the following.

```json
{ Success: "#Boolean"; Message: "#String"; Data: "#Object" }
```
---

## User session or user authentication

POST **/_session/signIn** to sign in with Username and Password QueryString or FormData.POST **/_session/signOut** to sign out.

or

import **/_content/Silmoon.AspNetCore.Blazor/js/jsSilmoonAuthInterop.js**

call **doSignIn(username, password)** and **doSignOut()**.