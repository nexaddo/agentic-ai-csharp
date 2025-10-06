#!/usr/bin/env bash
set -euo pipefail
IMG="localhost/ai-support-agent:local"
if ! podman image exists "${IMG}"; then
  podman build -t "${IMG}" -f Containerfile .
fi
sudo mkdir -p /var/lib/ai-support-agent/data && sudo chmod -R 777 /var/lib/ai-support-agent
podman play kube podman/kube-pod.yaml --env-file podman.env
