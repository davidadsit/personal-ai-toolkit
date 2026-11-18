#!/bin/sh
set -e
root="$(cd "$(dirname "$0")/.." && pwd)"
label="com.$(id -un).dashboard"
plist="$HOME/Library/LaunchAgents/$label.plist"

intervals=""
for weekday in 1 2 3 4 5; do
  for hour in 8 9 10 11 12 13 14 15 16 17; do
    intervals="$intervals<dict><key>Weekday</key><integer>$weekday</integer><key>Hour</key><integer>$hour</integer><key>Minute</key><integer>0</integer></dict>"
  done
done

cat >"$plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>Label</key><string>$label</string>
  <key>ProgramArguments</key><array><string>$root/scripts/scheduled-refresh.sh</string></array>
  <key>StartCalendarInterval</key><array>$intervals</array>
  <key>StandardErrorPath</key><string>$root/dashboard/launchd.log</string>
</dict>
</plist>
PLIST

launchctl bootout "gui/$(id -u)/$label" 2>/dev/null || true
launchctl bootstrap "gui/$(id -u)" "$plist"
echo "Installed $label at $plist"
