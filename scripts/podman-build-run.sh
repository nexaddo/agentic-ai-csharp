#!/usr/bin/env bash
set -euo pipefail
IMG="localhost/ai-support-agent:local"
NAME="ai-support-agent"
sudo mkdir -p /var/lib/ai-support-agent/data && sudo chmod -R 777 /var/lib/ai-support-agent
podman build -t "${IMG}" -f Containerfile .
-podman rm -f "${NAME}" || true
podman run -d --name "${NAME}" --env-file podman.env -p 5000:5000 -v /var/lib/ai-support-agent/data:/data:Z "${IMG}"
echo "Up at http://localhost:5000/healthz"
