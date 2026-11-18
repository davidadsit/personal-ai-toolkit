#!/bin/sh
pid_file="${CLAUDE_PROJECT_DIR:-$PWD}/dashboard/server.pid"

if [ -f "$pid_file" ]; then
  kill "$(cat "$pid_file")" 2>/dev/null || true
  rm -f "$pid_file"
fi
