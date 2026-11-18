# Adding work to the personal dashboard

This guide is for Claude sessions working in other folders. The dashboard shows every project, task and open item in one page at http://127.0.0.1:8771/. It is generated from plain files in this repo, so you add work by editing those files.

## What to record

- **A project** is an outcome with several steps, such as "Build a garden shed" or "Renew passport".
- **A task** is one step of a project.
- **An open item** is something waiting on the user that isn't a project step: an action, a decision, a question or a risk.

Record work the user should see: projects you start for them, steps you finish or discover, and questions or decisions you are waiting on.

## Ownership

- Each project file has one owner, named on an `Owner:` line in its notes. Edit only the projects your session owns, and the row for that project in `projects/README.md`.
- If a project already exists for your work, update it rather than adding a second one.
- Don't touch other files in this repo.
- Don't commit. The `personal-dashboard` session commits dashboard changes.
- If `ListAgents` shows a live session named `personal-dashboard`, you may instead send it the change with SendMessage, and it will make the edit.

## Add a project

1. Read `projects/README.md`. Use the next free number after the highest existing `P` number.
2. Add a row to the table:

   ```markdown
   | [P8. Build a garden shed](garden-shed.md) | Short | In progress | | 2026-12-01 | | |
   ```

   The columns are title and file, horizon, status, partners, next date, outcome and notes.

   - **Status:** start it with `In progress`, `Planned` or `Done`. Only `In progress` projects show a Doing card. `Done` moves the project to "Completed projects".
   - **Next date:** the first `YYYY-MM-DD` in the cell sets the sort order. Write `Unknown` when there is no date.
3. Create `projects/<file>.md`:

   ```markdown
   # Build a garden shed

   ## Notes

   - Owner: garden-shed session, ~/code/garden-shed
   - Source: links or context the user needs.

   ## Plan
   - [x] P8.1. Measure the site (done 2026-11-17)
   - [ ] P8.2. **Now:** Draw the plans
   - [ ] P8.3. Order the lumber
   ```

## Plan lines

Only checkbox lines under `## Plan` are read. Each line follows this pattern:

```
- [ ] P8.3. Step text
- [x] P8.1. Step text (done YYYY-MM-DD)
```

- `[x]` marks a finished step. Add `(done YYYY-MM-DD)` with the real date. A finished step shows in Done for 2 business days, then moves to Archive. A finished step without a date goes straight to Archive.
- Start the text with `**Now:** ` on the steps being worked on. In an `In progress` project they show in Doing. Without any `Now` step, the first open step is Doing.
- The first open step that isn't Doing shows in To do. The rest wait in Backlog.
- Keep steps in the order they should happen. Number them `P<project>.<step>`, and never reuse a number.
- Step text may use `` `code` `` and `**bold**`. Nothing else is rendered.

## Open items

Open items live in `dashboard/open-items.json`. Create the file if it's missing:

```json
{ "items": [
    { "code": "Q3", "kind": "question", "text": "Which supplier for the **lumber**?", "raised": "2026-11-17", "project": "P8", "folder": "garden-shed" } ] }
```

- `kind` is `action`, `decision`, `question` or `risk`. Its code starts with A, D, Q or R, followed by the next free number for that letter.
- Say what is needed and from whom.
- Set `project` to the project's number, for example `"P8"`, when the item belongs to one. The dashboard shows the project's title next to the raised date. Leave `project` out otherwise, and don't repeat the project in the text.
- Set `folder` to the name of the folder your session works in, for example `"garden-shed"` for `~/code/garden-shed`. The dashboard shows it after the project, so the user knows where to go to answer.
- Remove the item when it is resolved.
- This file is tracked in git, so never put secrets in it.

## Check your change

Run `scripts/dashboard` in the dashboard folder to regenerate the page, then confirm that your cards appear in `dashboard/index.html`. Tell the user what you added or changed.
