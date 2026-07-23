#!/usr/bin/env bash
# Linux dedicated server launch script.
# Rename DeathmatchServer.x86_64 to match your build's executable if different.
DIR="$(cd "$(dirname "$0")" && pwd)"
"$DIR/DeathmatchServer.x86_64" \
  -batchmode -nographics \
  -port 7777 \
  -maxplayers 8 \
  -map Deathmatch \
  -logFile "$DIR/server.log"
