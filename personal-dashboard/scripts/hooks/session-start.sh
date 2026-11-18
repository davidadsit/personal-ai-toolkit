#!/bin/sh
root="$(cd "$(dirname "$0")/../.." && pwd)"
port="$(jq -r '.port // 8771' "$root/dashboard.json" 2>/dev/null || echo 8771)"

if ! curl -s -o /dev/null --max-time 1 "http://127.0.0.1:$port/"; then
  mkdir -p "$root/dashboard"
  nohup "$root/scripts/dashboard" serve >>"$root/dashboard/server.log" 2>&1 </dev/null &
fi

echo "Run /dashboard to refresh today's meetings and emails. The dashboard is at http://127.0.0.1:$port/"
