#!/usr/bin/env bash
set -euo pipefail

COMMAND="${1:-up}"
COMPOSE_FILE="docker-compose.dev.yml"

case "$COMMAND" in
  up)
    docker compose -f "$COMPOSE_FILE" up --build --force-recreate web postgres valkey
    ;;
  down)
    docker compose -f "$COMPOSE_FILE" down --remove-orphans
    ;;
  clean)
    docker compose -f "$COMPOSE_FILE" down --remove-orphans --volumes
    ;;
  restart)
    docker compose -f "$COMPOSE_FILE" down
    docker compose -f "$COMPOSE_FILE" up --build --force-recreate web postgres valkey
    ;;
  logs)
    docker compose -f "$COMPOSE_FILE" logs -f web postgres valkey
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
