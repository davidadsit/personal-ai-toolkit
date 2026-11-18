# 0004. The page is static HTML with inline CSS and JavaScript

Status: Accepted, 2026-11-17

## Context

The page must work when opened from disk and when served locally. It must not depend on the server for anything except archiving.

## Decision

We generate one self-contained `index.html` with inline CSS and JavaScript. The page reloads itself to pick up regenerated content.

## Consequences

- Easier: no asset pipeline, and the page works offline from a file.
- Harder: the page can't update parts in place, so it restores scroll position across reloads.

## Confirmation

Acceptance check 19 confirms the page has no external references besides data links and the calendar URL.
