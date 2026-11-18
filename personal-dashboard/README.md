# Personal dashboard

A local page with a kanban board built from `projects/`, today's meetings, emails that need attention, and open items. [spec.md](spec.md) describes the behaviour, and [docs/adr](docs/adr/README.md) records the decisions.

## Use

- `scripts/dashboard` regenerates `dashboard/index.html`.
- Set `"theme"` in `dashboard.json` to `default`, `autumn` or `christmas` to change the look.
- `scripts/dashboard serve` also serves it on `http://127.0.0.1:8771/` so emails can be archived from the page.
- `/dashboard` in Claude Code refreshes meetings and emails.
- `scripts/install-scheduler.sh` installs a launchd agent that runs `/dashboard` hourly from 8:00 to 17:00 on weekdays. Output goes to `dashboard/refresh.log`.

Project hooks in `.claude/settings.json` regenerate the page after every turn, ask Claude to archive queued emails, and start and stop the server with the session.

## Keep your data in its own repo

The folder you run the dashboard from is its workspace. Out of the box that's this folder, with example data. To keep your projects and open items private, make a separate workspace repo that holds only your data and includes this toolkit as a git submodule:

```
my-dashboard/
  dashboard.json
  projects/
  dashboard/open-items.json
  scripts/dashboard            # shim: exec "$(dirname "$0")/../toolkit/personal-dashboard/scripts/dashboard" "$@"
  .claude/settings.json        # hooks: "$CLAUDE_PROJECT_DIR"/toolkit/personal-dashboard/scripts/hooks/<hook>.sh
  .claude/skills/dashboard     # link or copy of toolkit/personal-dashboard/.claude/skills/dashboard
  toolkit/                     # git submodule of personal-ai-toolkit
```

Install the scheduler with `toolkit/personal-dashboard/scripts/install-scheduler.sh` run from that workspace.

## Other Claude sessions

Sessions working in other folders add their projects, tasks and open items by following [docs/agents.md](docs/agents.md). `prompts/system_prompt.md` in the toolkit points every session there.

## Develop

`dotnet test` runs the unit, integration and acceptance tests. Requires the .NET 10 SDK, `jq` and `curl`.
