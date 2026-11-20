# 0005. List panels are declared in the configuration

Status: Accepted, 2026-11-20

## Context

People want panels beyond meetings, emails and open items, such as parcels on the way or candidates in a hiring pipeline. Those panels differ per person and per employer. The toolkit is public and must not hold anyone's personal or employer-specific rules, so each such panel can't be a feature in the code.

The panels people ask for share one shape: a list of rows, each with a title, a subtitle, a status and a date, sorted by date.

## Decision

We render extra list panels that the workspace declares in `dashboard.json`. Each declaration names a feed file, a heading and natural-language instructions. The `/dashboard` skill follows the instructions to write the feed, and the generator only reads it, as in ADR 0002.

## Consequences

- Easier: a new panel needs only configuration, and the personal rules for filling it stay in the private workspace.
- Harder: every list panel has the same row shape. Instructions in natural language are less predictable than code, so a panel's quality depends on how they are written. A panel that needs a different shape needs a new decision.

## Confirmation

The toolkit has no code for any specific list panel. Acceptance checks 20 to 23 in the spec cover rendering, empty states, ordering and feed-name validation.
