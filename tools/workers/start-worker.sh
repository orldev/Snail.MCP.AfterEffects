#!/usr/bin/env bash
# Starts one After Effects worker on this macOS host and keeps it running: when it quits, idle or asked to, it comes back fresh.
# The worker names itself from SNAIL_BRIDGE_WORKER, dials every server announced under the state folder (the server in Docker among them)
# and the server adopts it. Run one copy per worker:  tools/workers/start-worker.sh w1 [idle-minutes]
# To keep it across logins, wrap it in a launchd agent (docs/content/docker.md).
set -euo pipefail

name="${1:-w1}"
idle="${2:-15}"
# The server's folder under its s3l name; the snail name for a server from before the rename. SNAIL_BRIDGE_DATA_ROOT wins over both.
if [ -d "$HOME/.s3l-mcp-aftereffects" ]; then default_state="$HOME/.s3l-mcp-aftereffects"; else default_state="$HOME/.snail-mcp-aftereffects"; fi
state="${SNAIL_BRIDGE_DATA_ROOT:-$default_state}"
binary="${AFTER_EFFECTS:-}"

if [ -z "$binary" ]; then
    bundle="$(ls -d "/Applications/Adobe After Effects "*/"Adobe After Effects "*.app 2>/dev/null | grep -v "Render Engine" | sort | tail -n 1 || true)"
    binary="$bundle/Contents/MacOS/After Effects"
fi

if [ ! -x "$binary" ]; then
    echo "No After Effects binary at '$binary'; set AFTER_EFFECTS to it." >&2
    exit 1
fi

echo "worker $name: $binary, state $state, idle $idle min"

while true; do
    SNAIL_BRIDGE_WORKER="$name" SNAIL_BRIDGE_IDLE_MINUTES="$idle" SNAIL_BRIDGE_DATA_ROOT="$state" "$binary" -m >/dev/null 2>&1 || true
    echo "worker $name exited; starting again in 5 s"
    sleep 5
done
