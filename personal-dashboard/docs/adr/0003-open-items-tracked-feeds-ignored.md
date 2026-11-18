# 0003. Open items are tracked in git, other feeds are ignored

Status: Accepted, 2026-11-17

## Context

Open items are a record of decisions, actions and questions that is worth keeping over time. Meeting and email feeds hold other people's names and message details, and they are regenerated constantly.

## Decision

We track `dashboard/open-items.json` in git and ignore everything else in `dashboard/`. Claude edits open items by hand during conversation.

## Consequences

- Easier: open items have a history and can be reviewed with `git log`.
- Harder: open-item edits show up as working-tree changes that need committing.

## Confirmation

`.gitignore` ignores `dashboard/*` and un-ignores `dashboard/open-items.json`.
