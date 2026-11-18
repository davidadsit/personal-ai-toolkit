# Personal AI toolkit

How I work with Claude Code day to day: the system prompts I give every session, the shell aliases that load them, and a personal dashboard that every session reports into.

## What's here

| Folder | What it is |
|---|---|
| [prompts/](prompts) | `system_prompt.md` sets how Claude communicates with me. `system_prompt_for_coding.md` adds coding rules: test-driven development, simple design, tidy-first commits and ADRs. |
| [shell/](shell) | Zsh aliases: `c` starts a session with the communication prompt, and `cc` adds the coding prompt. |
| [personal-dashboard/](personal-dashboard) | A local page with a kanban board built from plain-text project files, today's meetings, the emails that need me, and open items. Claude Code fills it from Google Calendar and Gmail, and sessions in other folders add their projects and questions to it. |

## Set up

1. Clone into `~/code/personal-ai-toolkit`, or anywhere else and set `PERSONAL_AI_TOOLKIT` to that path.
2. Add `source ~/code/personal-ai-toolkit/shell/aliases.zsh` to `~/.zshrc`. To add instructions of your own, such as where your private dashboard workspace lives, put them in a file and set `PERSONAL_AI_PROMPT` to its path before that line. The aliases append it after the toolkit prompts.
3. Edit the prompts to fit how you work. They're written for me, so change anything that doesn't fit you.
4. For the dashboard, follow [personal-dashboard/README.md](personal-dashboard/README.md). It needs the .NET 10 SDK, `jq`, `curl`, and Claude Code with the Google Calendar and Gmail connectors.

## License

MIT. See [LICENSE](LICENSE).
