#!/usr/bin/env bash
set -euo pipefail
./scripts/podman-build-run.sh
SYSTEMD_DIR="${HOME}/.config/systemd/user"
mkdir -p "${SYSTEMD_DIR}"
podman generate systemd --new --files --name ai-support-agent --container-prefix "" --restart-policy=always
mv ./*.service "${SYSTEMD_DIR}/" 2>/dev/null || true
systemctl --user daemon-reload
systemctl --user enable ai-support-agent.service
systemctl --user start ai-support-agent.service
systemctl --user status ai-support-agent.service
