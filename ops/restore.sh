#!/usr/bin/env bash
set -euo pipefail

backup=${1:?usage: restore.sh <backup.db> [target.db]}
target=${2:-commerce.db}

test -f "$backup"
sqlite3 "$backup" "PRAGMA integrity_check;" | grep -qx ok
test "$(sqlite3 "$backup" "SELECT COUNT(*) FROM sqlite_master WHERE type='table';")" -gt 0
temporary="$target.restore"
trap 'rm -f "$temporary"' EXIT
cp "$backup" "$temporary"
test -z "$(sqlite3 "$temporary" "PRAGMA foreign_key_check;")"
mv "$temporary" "$target"
trap - EXIT
sqlite3 "$target" "PRAGMA integrity_check;" | grep -qx ok
echo "$target"
