#!/bin/sh
root="$(cd "$(dirname "$0")/../.." && pwd)"
"$root/scripts/dashboard" >/dev/null 2>&1 </dev/null &
exit 0
