#!/bin/sh
root="$(cd "$(dirname "$0")/../.." && pwd)"
queue="$root/dashboard/archive-queue.json"

if [ "$(jq -r '.stop_hook_active // false')" = "true" ] || [ ! -f "$queue" ]; then
  exit 0
fi

count="$(jq '.emails | length' "$queue" 2>/dev/null || echo 0)"
if [ "$count" -gt 0 ]; then
  jq -n --argjson count "$count" '{
    decision: "block",
    reason: "The dashboard archive queue holds \($count) thread(s). For each thread in dashboard/archive-queue.json, remove the INBOX label in Gmail, then run scripts/dashboard archive-done <thread>."
  }'
fi
