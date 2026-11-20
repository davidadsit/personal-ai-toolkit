#!/bin/sh
code="$(cd "$(dirname "$0")/.." && pwd)"
workspace="${1:-$PWD}"
export PATH="/usr/local/share/dotnet:$HOME/.local/bin:/opt/homebrew/bin:/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin"
mkdir -p "$workspace/dashboard"
cd "$workspace" || exit 1

allowed_tools="Read,Grep,Glob,Edit(./dashboard/**),Bash(scripts/dashboard),Bash(scripts/dashboard archive-done *),Bash(scripts/dashboard weather)"
allowed_tools="$allowed_tools,mcp__claude_ai_Google_Calendar__list_events"
allowed_tools="$allowed_tools,mcp__claude_ai_Gmail__search_threads,mcp__claude_ai_Gmail__get_thread,mcp__claude_ai_Gmail__unlabel_thread"

{
  echo "== $(date '+%Y-%m-%d %H:%M')"
  "$code/scripts/dashboard" weather
  claude -p "/dashboard" \
    --settings '{"disableAllHooks": true}' \
    --permission-mode dontAsk \
    --no-session-persistence \
    --allowedTools "$allowed_tools" </dev/null
  "$code/scripts/dashboard"
} >>"$workspace/dashboard/refresh.log" 2>&1
