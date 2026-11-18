#!/bin/sh
root="$(cd "$(dirname "$0")/../.." && pwd)"
pid_file="$root/dashboard/server.pid"

if [ -f "$pid_file" ]; then
  kill "$(cat "$pid_file")" 2>/dev/null || true
  rm -f "$pid_file"
fi
