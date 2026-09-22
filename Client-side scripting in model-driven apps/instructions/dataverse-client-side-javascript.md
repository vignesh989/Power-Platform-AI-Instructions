# Agent Instructions — Dataverse Client-Side JavaScript

These instructions apply to any JavaScript generated, edited, or reviewed for
model-driven app client scripting (form scripts and ribbon/command bar
scripts) in this repo.

## Source of truth: templates

Every new form script or ribbon script MUST be generated from the matching
template below — do not invent a different module shape, IIFE pattern, or
naming convention.

- **Form scripts** → `templates/XTemplate_TableName_FormRules.js`
- **Ribbon scripts** → `templates/XTemplate_TableName_RibbonRules.js`

When asked to create a script for a specific table (e.g. `abc_idea`), copy
the relevant template, replace:
- `TableName` in the filename with the table's schema name (e.g.
  `ABC_Idea_FormRules.js`)
- `Contoso.YourModuleName` / `Contoso.RibbonButtonModule` with a module name specific
  to the table/feature (e.g. `Contoso.Idea`, `Contoso.IdeaRibbon`)

and keep the IIFE structure, `"use strict"` directive, and public/private
method split exactly as in the template.

## Module pattern (non-negotiable)

- Single global namespace root: `var Contoso = Contoso || {};`
- One IIFE per file, assigned to a property of `Contoso`
- Only event handlers and ribbon command entry points are exposed in the
  `return { ... }` object, using PascalCase keys (e.g. `OnFormLoad`,
  `OnChange`) that map to the exact names used in the form's Event Handlers
  or the ribbon command's Function name
- All helper/private logic stays as unexposed functions inside the IIFE —
  never attach helpers directly to `Contoso` or to `window`
- Always retrieve context via `executionContext.getFormContext()` inside the
  handler — never rely on a global `Xrm.Page` reference (deprecated)

## Coding standards

- `"use strict";` at the top of every file
- `const`/`let` only — no `var` for anything except the namespace root
  declaration (`var Contoso = Contoso || {};`), which must stay `var` for safe
  re-declaration across multiple loaded scripts
- Optional chaining (`?.`) for attribute/control lookups that may be null,
  matching the template's `formContext.getAttribute(attributeName)?.getValue()`
  style
- Every `Xrm.WebApi` call follows the template's callback pattern: a
  `success`/`error` pair, with an optional caller-supplied callback checked
  via `typeof x === "function"` before invoking
- JSDoc-style comments on any public handler describing purpose, parameters,
  and which form/ribbon event it's bound to
- No hard-coded GUIDs, environment URLs, or table/column logical names
  scattered inline — hoist repeated ones to named constants at the top of the
  module

## Microsoft Client API — required practices

Generated code must follow the official guidance at
https://learn.microsoft.com/en-us/power-apps/developer/model-driven-apps/clientapi/reference
and the accompanying best-practices guidance
(https://learn.microsoft.com/en-us/power-apps/developer/model-driven-apps/best-practices/business-logic):

- **Never reference `window.top`** — it breaks in embedded/Teams/mobile
  contexts and causes script errors
- **Use Web API v9.x / OData v4.0 only** (`Xrm.WebApi.*`) — never call the
  deprecated OData v2.0 endpoint directly
- **All HTTP/HTTPS and Web API calls must be asynchronous** — use the
  promise-based `Xrm.WebApi` methods as shown in the templates; never block
  the UI thread with synchronous XHR
- **Use `formContext` / `executionContext`, never `Xrm.Page`** — `Xrm.Page`
  is deprecated and must not appear in new code
- **Client scripting is not a security boundary** — any validation or
  business rule enforced here must also be enforced server-side (plugin/
  Power Automate); don't treat a form script as sufficient on its own
- **Prefer Business Rules over script for simple field logic** (show/hide,
  required, simple conditional defaults) — only reach for JavaScript when
  the logic needs something Business Rules can't express (Web API calls,
  ribbon buttons, complex branching, calling other libraries)
- Remove/comment out example handlers that aren't wired to a real event —
  don't ship template placeholders (`onExampleButtonClick`, "Add more
  handlers as needed") into production files

## Ribbon-specific rules

- Every command handler receives `primaryControl` (or `selectedControl` for
  grid commands) as its first parameter — don't assume access to a global
  form reference
- Confirm before any destructive or state-changing action using
  `Xrm.Navigation.openConfirmDialog`, matching `showConfirmDialog` in the
  ribbon template
- Refresh the record/grid (`primaryControl.data.refresh()`) after a
  successful Web API update so the ribbon and form reflect the new state
- Surface errors to the user via `Xrm.Navigation.openAlertDialog` with the
  raw `error.message` — don't swallow errors silently

## File/solution naming

- JS web resource files: `abc_/js/<table-or-feature>_formrules.js` and
  `abc_/js/<table-or-feature>_ribbonrules.js` (lowercase, publisher-prefixed
  path, matching the `abc` Dataverse publisher prefix (replace with your own) used across this
  solution)
- Keep one form-rules file and one ribbon-rules file per table unless a
  table's logic is large enough to warrant splitting by feature — ask before
  splitting a file

## When generating new code

1. Identify which template applies (form vs. ribbon)
2. Ask for the table logical name and the specific fields/events/buttons
   involved if not already given
3. Generate the file following every rule above
4. Flag anywhere the request would require an unsupported pattern (e.g.
   `Xrm.Page`, synchronous calls, OData v2.0) instead of silently complying
