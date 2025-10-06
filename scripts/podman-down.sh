#!/usr/bin/env bash
set -euo pipefail
POD="ai-support-agent"
podman pod stop "${POD}" 2>/dev/null || true
podman pod rm "${POD}" 2>/dev/null || true
