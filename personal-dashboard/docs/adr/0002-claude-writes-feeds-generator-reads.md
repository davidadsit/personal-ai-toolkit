# 0002. Claude writes the feeds, the generator only reads them

Status: Accepted, 2026-11-17

## Context

Meetings and emails come from Google Calendar and Gmail. Claude Code already has signed-in connectors for both. Giving the program its own API keys would add secrets and an OAuth flow to a personal tool.

## Decision

We let the `/dashboard` skill fetch Calendar and Gmail data and write JSON feeds in `dashboard/`. The generator and server only read those feeds. The server queues archive requests in a file, and Claude performs them.

## Consequences

- Easier: no credentials in the program, and the generator is testable with fixture files.
- Harder: data is only as fresh as the last skill run, and archiving waits for Claude.

## Confirmation

The program has no Google client code or credentials. Generator tests run entirely from fixture feeds.
