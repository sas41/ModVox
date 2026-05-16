#!/usr/bin/env bash
set -euo pipefail

COMMAND="${1:-up}"

case "$COMMAND" in
  up)
    docker compose up --build --force-recreate web postgres valkey
    ;;
  down)
    docker compose down --remove-orphans
    ;;
  clean)
    docker compose down --remove-orphans --volumes
    ;;
  restart)
    docker compose down
    docker compose up --build --force-recreate web postgres valkey
    ;;
  logs)
    docker compose logs -f web postgres valkey
    ;;
  *)
    cat <<'EOF'
Usage: ./dev.sh [up|down|clean|restart|logs]

Commands:
  up       Start full stack with web hot reload
  down     Stop and remove stack containers
  clean    Stop stack and remove volumes
  restart  Restart stack with fresh build
  logs     Follow logs for web/postgres/valkey
EOF
    exit 1
    ;;
esac
