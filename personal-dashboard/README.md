# Personal dashboard

A local page with a kanban board built from `projects/`, today's meetings, emails that need attention, and open items. [spec.md](spec.md) describes the behaviour, and [docs/adr](docs/adr/README.md) records the decisions.

## Use

- `scripts/dashboard` regenerates `dashboard/index.html`.
- Set `"theme"` in `dashboard.json` to `default`, `autumn` or `christmas` to change the look.
- `scripts/dashboard serve` also serves it on `http://127.0.0.1:8771/` so emails can be archived from the page.
- `/dashboard` in Claude Code refreshes meetings and emails.
- `scripts/install-scheduler.sh` installs a launchd agent that runs `/dashboard` hourly from 8:00 to 17:00 on weekdays. Output goes to `dashboard/refresh.log`.

Project hooks in `.claude/settings.json` regenerate the page after every turn, ask Claude to archive queued emails, and start and stop the server with the session.

## Other Claude sessions

Sessions working in other folders add their projects, tasks and open items by following [docs/agents.md](docs/agents.md). `prompts/system_prompt.md` in the toolkit points every session there.

## Develop

`dotnet test` runs the unit, integration and acceptance tests. Requires the .NET 10 SDK, `jq` and `curl`.
