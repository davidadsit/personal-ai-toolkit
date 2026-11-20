# 0006. The program fetches weather from a public service without credentials

Status: Accepted, 2026-11-20

## Context

People want current weather for a few places at the top of the dashboard. Weather changes through the day, so it has to be refreshed on a schedule.

ADR 0002 keeps Google data with Claude, because the connectors hold the credentials. Weather needs no credentials: Open-Meteo serves forecasts for any coordinates with no key or account. Having Claude look weather up by web search would be slow, unreliable and costly for something a single HTTP request answers.

ADR 0004 keeps the page free of network references, so the browser must not fetch weather itself.

## Decision

We add a `dashboard weather` command that fetches current conditions and today's high and low from Open-Meteo for the places in `dashboard.json`, and writes `dashboard/weather.json`. The scheduled refresh and the `/dashboard` skill run it. The generator only reads the feed, as with every other feed.

## Consequences

- Easier: weather stays fresh without using Claude, and the page stays static.
- Harder: the program now has one network dependency. When Open-Meteo is down, the command fails, keeps the previous feed and reports the error. The configured coordinates are sent to Open-Meteo.

## Confirmation

Only the weather command makes network requests, and only to `api.open-meteo.com`. Its tests use a stub HTTP handler. The generator and server tests run without a network. Acceptance check 19 still holds: the page has no network references.
