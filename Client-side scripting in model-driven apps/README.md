# Power Platform AI Instructions

Instruction files and templates I use to get reliable output from AI coding agents (GitHub Copilot, Claude Code and others) on Microsoft Power Platform and Dynamics 365 builds.

These accompany my LinkedIn series reviewing AI on a real Power Platform build. Each part of the series adds the artefacts that made the difference for that area.

## Why this exists

Given a generic requirement, AI will happily write Dataverse code using deprecated methods (`Xrm.Page`, OData v2.0, synchronous calls) picked up from years of old blog posts. The fix is not a better prompt. It is giving the agent the right context before you ask: a template to start from, an instructions file with your standards, and the real metadata of what you're building against.

## Contents

| Area | Instructions | Templates |
| --- | --- | --- |
| Client-side scripting (model-driven apps) | [`instructions/dataverse-client-side-javascript.md`](instructions/dataverse-client-side-javascript.md) | Form: [`templates/XTemplate_TableName_FormRules.js`](templates/XTemplate_TableName_FormRules.js)<br>Ribbon: [`templates/XTemplate_TableName_RibbonRules.js`](templates/XTemplate_TableName_RibbonRules.js) |

More areas will be added as the series continues.

## How to use it

1. Copy the instructions file and templates into your repo.
2. Replace the `abc` publisher prefix and the `ABC` namespace with your own.
3. Export the form XML for the form you're scripting (XrmToolBox's Form XML Editor works well) and save it alongside your JavaScript.
4. In your agent, attach the instructions file and the form XML, then give it the requirement.

## Disclaimer

Shared as-is from my own practice. Adapt to your organisation's standards and review everything an agent generates before it goes anywhere near production.

## Licence

MIT. Use it, change it, share it.
