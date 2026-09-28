# Agent Instructions — Dataverse Plugins & Custom Actions

These instructions apply to any C# generated, edited, or reviewed for Dataverse
plugins and custom actions in this repo.

## Source of truth: the shared library

Never write configuration lookups, service calls, or constant literals inline.
This solution has a shared `ABC.Assembly.Common` project for exactly that
purpose. Every plugin and custom action MUST route through it:

- **Environment variables** → `ConfigurationManager.GetEnvironmentVariableValue`
  and `ConfigurationManager.GetEnvironmentVariableSecrectValue` — never a
  hardcoded GUID, URL, or API key
- **All CRUD / Execute calls against `IOrganizationService`** →
  `SimplifiedServiceProxy` (`Create`, `Update`, `Delete`, `Retrieve`,
  `RetrieveMultiple`, `Execute`, `Associate`, `Disassociate`) — never call the
  service directly from plugin code
- **CRM message names, input parameter keys, and display text** →
  `Constants` (`MessageName`, `InputParameter`, `DisplayText`) — never a
  string literal like `"create"` or `"Target"` scattered through a switch
  statement
- **Table and column references** → early-bound classes generated for this
  solution (Plugin Registration Tool, `CrmSvcUtil`, or the XrmToolBox Early
  Bound Generator), never a late-bound `Entity`/`SetAttributeValue` call with
  a hand-typed logical name — a hallucinated or misspelled table/field name
  must fail to compile, not fail at runtime
- **Every new plugin** → start from `XTemplate_PluginName.cs`. Copy it, rename
  the class and namespace, and implement only the private `Handler*` methods.
  Never change the shape of `Execute()` itself — service resolution, message
  routing, and exception handling stay exactly as the template has them

## Grounded in Microsoft's own plug-in best-practice guidance

This file follows the guidance published at
[Best practices for plug-in and workflow development](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/best-practices/business-logic/),
and the canonical patterns in
[PowerApps-Samples/dataverse/orgsvc/CSharp](https://github.com/microsoft/PowerApps-Samples/tree/master/dataverse/orgsvc/CSharp).
In particular:

- **Develop `IPlugin` implementations as stateless** — never cache `service`,
  `context`, or any other per-invocation object as an instance field; Dataverse
  reuses plugin instances across concurrent threads, so a cached field is a
  thread-safety bug waiting to happen. Resolve everything inside `Execute()`
- **Use `ITracingService` in plug-ins** — every wrapper method in
  `SimplifiedServiceProxy` traces the operation and its elapsed time, so slow
  or failing calls are visible without hand-adding logging to every plugin
- **Use `InvalidPluginExecutionException`** — the template's catch block
  always rethrows through this exception type after tracing the fault, never
  a raw `Exception` or a swallowed catch
- **Don't use batch request types (`ExecuteMultipleRequest` /
  `ExecuteTransactionRequest`) in plug-ins** — these aren't supported inside
  the plugin execution context
- **Don't use parallel execution inside plug-ins** — no `Task.Run`,
  `Parallel.For`, or fire-and-forget threads; multi-threading isn't supported
  inside a plugin
- **Manage plug-ins in a single solution** and **optimize assembly
  development** — keep related plugins and the shared library together rather
  than scattering logic across ad-hoc assemblies

## Why this exists (the failure modes each wrapper prevents)

- **Hardcoded configuration.** A GUID or API key baked into code means every
  change is a rebuild and a redeploy through every environment. Environment
  variables mean an admin changes the value per environment, through the UI,
  with no deployment. Secrets go through the secret-variable type, so they're
  encrypted at rest and never sit in source control in plain text.
- **Manual `RetrieveMultiple` pagination.** A raw call silently truncates at
  the page size. `SimplifiedServiceProxy.RetrieveMultiple` pages through
  `MoreRecords` / `PagingCookie` automatically, so callers always get the
  full result set.
- **No tracing or timing.** Every wrapper method logs the operation and its
  elapsed time via `ITracingService` and `Stopwatch`, so slow or failing SDK
  calls are visible without adding logging to every plugin by hand.
- **Magic strings.** Message names and parameter keys are grouped by purpose
  in nested `struct`s, not repeated as literals across every plugin's
  `Execute` method — a typo in a literal fails silently; a typo in a constant
  name fails to compile.

## Coding standards

- Never call `IOrganizationService.Create/Update/Delete/Retrieve/
  RetrieveMultiple/Execute/Associate/Disassociate` directly from plugin or
  custom action code — always go through `SimplifiedServiceProxy`
- Never hardcode a GUID, connection string, URL, or secret — always resolve
  it through `ConfigurationManager` at runtime
- Never use a raw string literal for a CRM message name or input parameter
  key — always reference the matching `Constants` struct
- Never reference a table or column by a hand-typed logical name string —
  always use the generated early-bound class for that table
- Wrap the plugin's `Execute` entry point in a try/catch that calls
  `SimplifiedServiceProxy.HandleException` before rethrowing, so faults are
  traced with full detail (timestamp, error code, plugin trace text) rather
  than swallowed or logged inconsistently
- XML-doc every public method: purpose, parameters, and return value

## When generating new code

1. Start from `XTemplate_PluginName.cs` rather than writing `Execute()` from
   scratch
2. Identify which shared-library methods the requirement needs
   (configuration lookup, CRUD, or a specific SDK message)
3. Ask for the entity logical name and the specific message/fields involved
   if not already given, and confirm an early-bound class exists for that
   table — if it doesn't, flag that it needs generating before writing logic
   against it
4. Generate the plugin or custom action calling into `ABC.Assembly.Common`
   for every configuration value and every service call, and into the
   matching early-bound class for every table/column reference
5. Flag anywhere the request would require bypassing the shared library
   (e.g. a raw `service.RetrieveMultiple` call, a literal GUID) or the
   early-bound classes (e.g. `new Entity("tablename")`) instead of silently
   complying
