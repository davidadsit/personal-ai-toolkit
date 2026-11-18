#!/bin/sh
code="$(cd "$(dirname "$0")/../.." && pwd)"
cd "${CLAUDE_PROJECT_DIR:-$PWD}" || exit 0
"$code/scripts/dashboard" >/dev/null 2>&1 </dev/null &
exit 0
