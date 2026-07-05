#!/usr/bin/env bash
set -euo pipefail

database=${1:-commerce.db}
backup=${2:-backups/commerce-$(date -u +%Y%m%dT%H%M%SZ).db}

mkdir -p "$(dirname "$backup")"
sqlite3 "$database" ".timeout 5000" ".backup '$backup'"
sqlite3 "$backup" "PRAGMA integrity_check;" | grep -qx ok
test "$(sqlite3 "$backup" "SELECT COUNT(*) FROM sqlite_master WHERE type='table';")" -gt 0
echo "$backup"
