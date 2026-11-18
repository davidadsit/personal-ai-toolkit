# 0001. C# on .NET without a web framework

Status: Accepted, 2026-11-17

## Context

The dashboard is one generator and one tiny local server with three routes. It runs on one machine for one person. A web framework would add dependencies and startup cost for routing we don't need. The .NET 10 SDK is installed.

## Decision

We write the generator and server as one C# console program on .NET 10. The server uses `HttpListener`. The page uses no JavaScript libraries.

## Consequences

- Easier: one build, one binary, no package churn, fast tests with xUnit.
- Harder: routing, request parsing and HTML building are written by hand.

## Confirmation

The project file references no web framework package. Acceptance check 19 confirms the page loads nothing from the network.
