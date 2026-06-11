---
name: sillas3cantos-frontend
description: Use when working on the ORTOPEDIA PUEBLOS 3C Angular frontend, especially landing page, login, backoffice UI, styles, public catalog, auth flow, API proxy, or frontend documentation inside Frontend/.
---

# ORTOPEDIA PUEBLOS 3C Frontend

Use this skill for frontend work under `Frontend/`.

## First Step

Read `../../AGENTS.MD` before editing. It contains the current product intent, routes, commands, contact data, style rules, and validation checklist.

## Working Rules

- Keep the public site client-ready, not demo-like.
- Do not add visible header links for `Backoffice`, `Login`, or technical routes.
- Do not show `Invitado` or any session chip on the public landing.
- Do not add demo credential buttons or demo login copy.
- Keep `Portal empleados` as the discreet footer entry to `/login`.
- Keep public catalog calls using visible content only.
- Keep the main color direction blue/navy, not green/teal.
- Preserve mobile-first behavior.

## Common Files

Public shell:

```text
sillas-tres-cantos-web/src/app/app.html
sillas-tres-cantos-web/src/app/app.scss
sillas-tres-cantos-web/src/app/app.ts
```

Landing and catalog:

```text
sillas-tres-cantos-web/src/app/pages/public-catalog/
```

Employee login:

```text
sillas-tres-cantos-web/src/app/pages/login/
```

Internal portal:

```text
sillas-tres-cantos-web/src/app/pages/backoffice/
```

API/auth:

```text
sillas-tres-cantos-web/src/app/core/
```

Global theme:

```text
sillas-tres-cantos-web/src/styles.scss
```

## Validation

Run from repository root:

```bash
npm run build
```

For visual checking:

```bash
npm start
```

Then inspect:

```text
http://localhost:4200/
http://localhost:4200/login
http://localhost:4200/backoffice
```

If `4200` is busy, use the port Angular reports.
